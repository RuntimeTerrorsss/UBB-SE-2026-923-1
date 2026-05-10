using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingBoardGames.Data;
using BookingBoardGames.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConversationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConversationRepository conversationRepository;

        public ConversationController(AppDbContext context, IConversationRepository conversationRepository)
        {
            _context = context;
            this.conversationRepository = conversationRepository;
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<Conversation>>> GetConversationsForUser(int userId)
        {
            var conversations = await this.conversationRepository.GetConversationsForUser(userId);
            return Ok(conversations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Conversation>> GetConversationById(int id)
        {
            try
            {
                var conversation = await this.conversationRepository.GetConversationById(id);
                return Ok(conversation);
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/participants")]
        public async Task<ActionResult<IReadOnlyList<int>>> GetParticipantUserIds(int id)
        {
            var userIds = await this.conversationRepository.GetParticipantUserIds(id);
            return Ok(userIds);
        }

        public record CreateConversationRequest(int SenderId, int ReceiverId);

        [HttpPost]
        public async Task<ActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            if (request.SenderId <= 0 || request.ReceiverId <= 0 || request.SenderId == request.ReceiverId)
                return BadRequest("Invalid conversation participants.");

            var existingConversation = await _context.Conversations
                .Include(c => c.Participants)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.Participants.Any(p => p.UserId == request.SenderId) &&
                    c.Participants.Any(p => p.UserId == request.ReceiverId));

            if (existingConversation is not null)
            {
                return Ok(existingConversation);
            }

            int conversationId = await this.conversationRepository.CreateConversation(request.SenderId, request.ReceiverId);
            var created = await this.conversationRepository.GetConversationById(conversationId);
            return CreatedAtAction(nameof(GetConversationById), new { id = conversationId }, created);
        }

        [HttpPost("messages")]
        public async Task<ActionResult<MessageDto>> SendMessage([FromBody] MessageDto messageDto)
        {
            var message = MessageDtoToEntity(messageDto);
            // Reset ID so EF always inserts a new row (prevents accidental update / triple-insert)
            message.MessageId = 0;

            if (!conversation.Participants.Any(p => p.UserId == messageDto.SenderId))
            {
                return BadRequest("Sender is not part of this conversation.");
            }

            int receiverId = messageDto.ReceiverId;
            if (!conversation.Participants.Any(p => p.UserId == receiverId))
            {
                receiverId = conversation.Participants
                    .Where(p => p.UserId != messageDto.SenderId)
                    .Select(p => p.UserId)
                    .FirstOrDefault();
            }

            if (receiverId <= 0)
            {
                return BadRequest("Receiver is invalid for this conversation.");
            }

            var normalizedMessageDto = messageDto with { ReceiverId = receiverId };
            var message = MessageDtoToEntity(normalizedMessageDto);
            var persisted = await this.conversationRepository.HandleNewMessage(message);
            return Ok(EntityToMessageDto(persisted));
        }

        [HttpPut("messages")]
        public async Task<ActionResult<MessageDto>> UpdateMessage([FromBody] MessageDto messageDto)
        {
            var updated = await this.conversationRepository.HandleMessageUpdate(MessageDtoToEntity(messageDto));
            if (updated is null)
            {
                return NotFound();
            }

            return Ok(EntityToMessageDto(updated));
        }

        [HttpPost("readreceipt")]
        public async Task<ActionResult> SendReadReceipt([FromBody] ReadReceiptDto readReceipt)
        {
            var dto = new ReadReceiptDTO(
                readReceipt.ConversationId,
                readReceipt.ReaderId,
                readReceipt.ReceiverId,
                readReceipt.ReceiptTimeStamp);
            await _repo.HandleReadReceipt(dto);
            return NoContent();
        }

        [HttpPost("rental/finalize/{messageId}")]
        public async Task<ActionResult<MessageDto>> FinalizeRentalRequest(int messageId)
        {
            var updated = await _repo.HandleRentalRequestFinalization(messageId);
            if (updated is null) return NotFound();
            return Ok(EntityToMessageDto(updated));
        }

        [HttpPost("cash/{parentMessageId}/{paymentId}")]
        public async Task<ActionResult<MessageDto>> CreateCashAgreementMessage(int parentMessageId, int paymentId)
        {
            var created = await _repo.CreateCashAgreementMessage(parentMessageId, paymentId);
            if (created is null) return NotFound();
            return Ok(EntityToMessageDto(created));
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private Message MessageDtoToEntity(MessageDto dto)
        {
            return dto.Type switch
            {
                MessageType.MessageText => new TextMessage
                {
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    TextMessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageImage => new ImageMessage
                {
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageImageUrl = dto.ImageUrl,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageRentalRequest => new RentalRequestMessage
                {
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    RentalRequestId = dto.RequestId,
                    IsRequestResolved = dto.IsResolved,
                    IsRequestAccepted = dto.IsAccepted,
                    RequestContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageCashAgreement => new CashAgreementMessage
                {
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    CashPaymentId = dto.PaymentId,
                    IsCashAgreementResolved = dto.IsResolved,
                    IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer,
                    IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                MessageType.MessageSystem => new SystemMessage
                {
                    ConversationId = dto.ConversationId,
                    MessageSenderId = dto.SenderId,
                    MessageReceiverId = dto.ReceiverId,
                    MessageSentTime = dto.SentAt,
                    MessageContentAsString = dto.Content,
                    MessageContent = dto.Content,
                    Conversation = null!,
                    Sender = null!,
                    Receiver = null!,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(dto.Type), dto.Type, "Unsupported message type."),
            };
        }

        private MessageDto EntityToMessageDto(Message message)
        {
            const int defaultMissingIdentifier = -1;

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
                RentalRequestMessage rentalMsg => rentalMsg.RequestContent ?? rentalMsg.MessageContentAsString ?? string.Empty,
                SystemMessage systemMsg => systemMsg.MessageContent ?? systemMsg.MessageContentAsString ?? string.Empty,
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
                ImageUrl: message is ImageMessage img ? img.MessageImageUrl ?? string.Empty : string.Empty,
                IsResolved: message is RentalRequestMessage rrm ? rrm.IsRequestResolved
                          : message is CashAgreementMessage cam ? cam.IsCashAgreementResolved
                          : false,
                IsAccepted: message is RentalRequestMessage ram ? ram.IsRequestAccepted : false,
                IsAcceptedByBuyer: message is CashAgreementMessage camb ? camb.IsCashAgreementAcceptedByBuyer : false,
                IsAcceptedBySeller: message is CashAgreementMessage cams ? cams.IsCashAgreementAcceptedBySeller : false,
                RequestId: message is RentalRequestMessage rrm2 ? rrm2.RentalRequestId : defaultMissingIdentifier,
                PaymentId: message is CashAgreementMessage cam2 ? cam2.CashPaymentId : defaultMissingIdentifier);
        }

        // ── DTOs / enums ─────────────────────────────────────────────────────

        public record MessageDto(
            int Id,
            int ConversationId,
            int SenderId,
            int ReceiverId,
            DateTime SentAt,
            string? Content,
            MessageType Type,
            string? ImageUrl,
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