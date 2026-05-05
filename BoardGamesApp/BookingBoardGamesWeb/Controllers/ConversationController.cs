using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookingBoardGamesWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConversationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Conversation>>> GetConversationsForUser(int userId)
        {
            var conversations = await _context.Conversations
                .AsNoTracking()
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();

            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetConversationById(int id)
        {
            var conversation = await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == id);

            if (conversation is null) return NotFound();
            return Ok(conversation);
        }

        [HttpGet("{id}/participants")]
        public async Task<ActionResult<IReadOnlyList<int>>> GetParticipantUserIds(int id)
        {
            var userIds = await _context.ConversationParticipants
                .Where(p => p.ConversationId == id)
                .Select(p => p.UserId)
                .ToListAsync();

            return Ok(userIds);
        }

        public record CreateConversationRequest(int SenderId, int ReceiverId);

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var conversation = new Conversation
            {
                Participants = new List<ConversationParticipant>
                {
                    new ConversationParticipant { UserId = request.SenderId },
                    new ConversationParticipant { UserId = request.ReceiverId },
                },
                Messages = new List<Message>()
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var created = await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c => c.ConversationId == conversation.ConversationId);

            return CreatedAtAction(nameof(GetConversationById), new { id = conversation.ConversationId }, created);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] MessageDto messageDto)
        {
            var message = MessageDtoToEntity(messageDto);
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var persisted = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);

            return Ok(EntityToMessageDto(persisted!));
        }

        [HttpPut("messages")]
        public async Task<ActionResult<MessageDto>> UpdateMessage([FromBody] MessageDto messageDto)
        {
            var tracked = await _context.Messages.FirstOrDefaultAsync(m => m.MessageId == messageDto.Id);
            if (tracked is null) return NotFound();

            tracked.MessageContentAsString = messageDto.Content;
            tracked.MessageSentTime = messageDto.SentAt;

            if (tracked is RentalRequestMessage rentalTracked && messageDto.Type == MessageType.MessageRentalRequest)
            {
                rentalTracked.IsRequestResolved = messageDto.IsResolved;
                rentalTracked.IsRequestAccepted = messageDto.IsAccepted;
                rentalTracked.RequestContent = messageDto.Content;
            }
            else if (tracked is CashAgreementMessage cashTracked && messageDto.Type == MessageType.MessageCashAgreement)
            {
                cashTracked.IsCashAgreementResolved = messageDto.IsResolved;
                cashTracked.IsCashAgreementAcceptedByBuyer = messageDto.IsAcceptedByBuyer;
                cashTracked.IsCashAgreementAcceptedBySeller = messageDto.IsAcceptedBySeller;
            }

            await _context.SaveChangesAsync();

            var persisted = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.MessageId == tracked.MessageId);

            return Ok(EntityToMessageDto(persisted!));
        }

        [HttpPost("readreceipt")]
        public async Task<ActionResult> SendReadReceipt([FromBody] ReadReceiptDto readReceipt)
        {
            var participant = await _context.ConversationParticipants
                .FirstOrDefaultAsync(p => p.ConversationId == readReceipt.ConversationId && p.UserId == readReceipt.ReaderId);

            if (participant is null) return NotFound();

            participant.LastMessageReadTime = readReceipt.ReceiptTimeStamp;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("rental/finalize/{messageId}")]
        public async Task<ActionResult<MessageDto>> FinalizeRentalRequest(int messageId)
        {
            var rentalMessage = await _context.Messages.OfType<RentalRequestMessage>().FirstOrDefaultAsync(m => m.MessageId == messageId);
            if (rentalMessage is null) return NotFound();

            rentalMessage.IsRequestResolved = true;
            rentalMessage.IsRequestAccepted = true;
            await _context.SaveChangesAsync();

            var updated = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.MessageId == messageId);

            return Ok(EntityToMessageDto(updated!));
        }

        [HttpPost("cash/{parentMessageId}/{paymentId}")]
        public async Task<ActionResult<MessageDto>> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var parent = await _context.Messages.OfType<RentalRequestMessage>().FirstOrDefaultAsync(m => m.MessageId == parentMessageId);
            if (parent is null) return NotFound();

            var cash = new CashAgreementMessage
            {
                ConversationId = parent.ConversationId,
                MessageSenderId = parent.MessageSenderId,
                MessageReceiverId = parent.MessageReceiverId,
                CashPaymentId = paymentId,
                MessageSentTime = DateTime.UtcNow,
                Conversation = null!,
                Sender = null!,
                Receiver = null!,
            };

            _context.Messages.Add(cash);
            await _context.SaveChangesAsync();

            var created = await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Conversation)
                .FirstOrDefaultAsync(m => m.MessageId == cash.MessageId);

            return Ok(EntityToMessageDto(created!));
        }

        private Message MessageDtoToEntity(MessageDto messageDto)
        {
            return messageDto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    TextMessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageImageUrl = messageDto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    RentalRequestId = messageDto.RequestId,
                    IsRequestResolved = messageDto.IsResolved,
                    IsRequestAccepted = messageDto.IsAccepted,
                    RequestContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    CashPaymentId = messageDto.PaymentId,
                    IsCashAgreementResolved = messageDto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = messageDto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = messageDto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    ConversationId = messageDto.ConversationId,
                    MessageSenderId = messageDto.SenderId,
                    MessageReceiverId = messageDto.ReceiverId,
                    MessageSentTime = messageDto.SentAt,
                    MessageContentAsString = messageDto.Content,
                    MessageContent = messageDto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(messageDto.Type), messageDto.Type, "Unsupported message type."),
            };
        }

        private MessageDto EntityToMessageDto(Message message)
        {
            int defaultMissingIdentifier = -1;

            MessageType messageType = message switch
            {
                TextMessage => MessageType.MessageText,
                ImageMessage => MessageType.MessageImage,
                RentalRequestMessage => MessageType.MessageRentalRequest,
                CashAgreementMessage => MessageType.MessageCashAgreement,
                SystemMessage => MessageType.MessageSystem,
                _ => throw new ArgumentOutOfRangeException(nameof(message), message.GetType().Name, "Unknown message subtype."),
            };

            string content = message switch
            {
                TextMessage textMessage => textMessage.TextMessageContent ?? textMessage.MessageContentAsString ?? string.Empty,
                RentalRequestMessage rentalForContent => rentalForContent.RequestContent ?? rentalForContent.MessageContentAsString ?? string.Empty,
                SystemMessage systemMessage => systemMessage.MessageContent ?? systemMessage.MessageContentAsString ?? string.Empty,
                _ => message.MessageContentAsString ?? string.Empty,
            };

            return new MessageDto(
                Id: message.MessageId,
                ConversationId: message.ConversationId,
                SenderId: message.MessageSenderId,
                ReceiverId: message.MessageReceiverId,
                SentAt: message.MessageSentTime,
                Content: content,
                Type: messageType,
                ImageUrl: message is ImageMessage imageMessage ? imageMessage.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rentalResolvedMessage ? rentalResolvedMessage.IsRequestResolved
                          : message is CashAgreementMessage cashResolvedMessage ? cashResolvedMessage.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage rentalAcceptedMessage ? rentalAcceptedMessage.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage cashBuyerMessage ? cashBuyerMessage.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cashSellerMessage ? cashSellerMessage.IsCashAgreementAcceptedBySeller : false,
                RequestId: message is RentalRequestMessage rentalRequestMessage ? rentalRequestMessage.RentalRequestId : defaultMissingIdentifier,
                PaymentId: message is CashAgreementMessage cashPaymentMessage ? cashPaymentMessage.CashPaymentId : defaultMissingIdentifier);
        }

        public record MessageDto(
            int Id,
            int ConversationId,
            int SenderId,
            int ReceiverId,
            DateTime SentAt,
            string Content,
            MessageType Type,
            string ImageUrl,
            bool IsResolved,
            bool IsAccepted,
            bool IsAcceptedByBuyer,
            bool IsAcceptedBySeller,
            int RequestId,
            int PaymentId);

        public enum MessageType
        {
            MessageSystem,
            MessageText,
            MessageImage,
            MessageRentalRequest,
            MessageCashAgreement,
        }

        public record ReadReceiptDto(int ConversationId, int ReaderId, int ReceiverId, DateTime ReceiptTimeStamp);
    }
}
