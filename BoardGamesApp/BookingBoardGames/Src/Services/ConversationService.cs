using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using BookingBoardGames.Data;
using BookingBoardGames.Src.DTO;
using BookingBoardGames.Src.Enum;
using BookingBoardGames.Src.Repositories;

namespace BookingBoardGames.Src.Services
{
    public class ConversationService : IConversationService
    {
        private readonly IConversationRepository _repo;
        private readonly IConversationNotifier _notifier;
        private readonly IUserRepository _userRepo;
        private readonly int _userId;

        public event Action<MessageDataTransferObject, string>? ActionMessageProcessed;
        public event Action<ConversationDTO, string>? ActionConversationProcessed;
        public event Action<ReadReceiptDTO>? ActionReadReceiptProcessed;
        public event Action<MessageDataTransferObject, string>? ActionMessageUpdateProcessed;

        public ConversationService(IConversationRepository conversationRepo, int userIdInput)
            : this(conversationRepo, userIdInput, App.UserRepository, ResolveNotifier())
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo)
            : this(conversationRepo, userIdInput, userRepo, ResolveNotifier())
        {
        }

        public ConversationService(IConversationRepository conversationRepo, int userIdInput, IUserRepository userRepo, IConversationNotifier conversationNotifier)
        {
            _repo = conversationRepo;
            _userId = userIdInput;
            _userRepo = userRepo;
            _notifier = conversationNotifier;

            _notifier.Register(_userId, this);
        }

        private static IConversationNotifier ResolveNotifier()
        {
            return App.ConversationNotifier ?? new ConversationNotifier();
        }

        public List<ConversationDTO> FetchConversations()
        {
            return _repo.GetConversationsForUser(_userId).Select(MapToDto).ToList();
        }

        public string GetOtherUserNameByConversationDTO(ConversationDTO conversation)
        {
            int otherUserId = conversation.Participants.First(p => p.UserId != _userId).UserId;
            return GetUsername(otherUserId);
        }

        public string GetOtherUserNameByMessageDTO(MessageDataTransferObject message)
        {
            int id = message.SenderId == _userId ? message.ReceiverId : message.SenderId;
            return GetUsername(id);
        }

        public void SendMessage(MessageDataTransferObject dto)
        {
            var msg = MapDtoToEntity(dto);
            var persisted = _repo.HandleNewMessage(msg);
            if (persisted != null)
            {
                var participants = new List<int> { dto.SenderId, dto.ReceiverId };
                _notifier.NotifyMessage(participants, persisted);
            }
        }

        public int CreateConversation(int senderId, int receiverId)
        {
            return _repo.CreateConversation(senderId, receiverId);
        }

        public void UpdateMessage(MessageDataTransferObject dto)
        {
            var msg = MapDtoToEntity(dto);
            var updated = _repo.HandleMessageUpdate(msg);
            if (updated != null)
            {
                var participants = _repo.GetParticipantUserIds(updated.ConversationId).ToList();
                _notifier.NotifyMessageUpdate(participants, updated);
            }
        }

        public void SendReadReceipt(ConversationDTO conversation)
        {
            var otherId = conversation.Participants.First(p => p.UserId != _userId).UserId;
            var receipt = new ReadReceiptDTO(conversation.Id, _userId, otherId, DateTime.Now);
            _repo.HandleReadReceipt(receipt);
            var participants = new List<int> { _userId, otherId };
            _notifier.NotifyReadReceipt(participants, receipt);
        }

        public void OnCardPaymentSelected(int messageId) => FinalizeRentalRequest(messageId);

        public void OnCashPaymentSelected(int messageId, int paymentId)
        {
            FinalizeRentalRequest(messageId);
            var created = _repo.CreateCashAgreementMessage(messageId, paymentId);
            if (created != null)
            {
                var participants = _repo.GetParticipantUserIds(created.ConversationId).ToList();
                _notifier.NotifyMessage(participants, created);
            }
        }

        private void FinalizeRentalRequest(int messageId)
        {
            var updated = _repo.HandleRentalRequestFinalization(messageId);
            if (updated != null)
            {
                var participants = _repo.GetParticipantUserIds(updated.ConversationId).ToList();
                _notifier.NotifyMessageUpdate(participants, updated);
            }
        }

        public void OnMessageReceived(Message msg) => ActionMessageProcessed?.Invoke(MapToMessageDto(msg), GetUsername(msg.MessageSenderId == _userId ? msg.MessageReceiverId : msg.MessageSenderId));

        public void OnConversationReceived(Conversation c) => ActionConversationProcessed?.Invoke(MapToDto(c), GetUsername(c.Participants.First(p => p.UserId != _userId).UserId));

        public void OnReadReceiptReceived(ReadReceiptDTO r) => ActionReadReceiptProcessed?.Invoke(r);

        public void OnMessageUpdateReceived(Message msg) => ActionMessageUpdateProcessed?.Invoke(MapToMessageDto(msg), GetUsername(msg.MessageSenderId == _userId ? msg.MessageReceiverId : msg.MessageSenderId));

        private string GetUsername(int id)
        {
            var user = App.Client.GetFromJsonAsync<User>($"users/{id}").GetAwaiter().GetResult();
            return user?.Username ?? "Unknown";
        }

        private ConversationDTO MapToDto(Conversation c)
        {
            return new ConversationDTO(
                c.ConversationId,
                c.Participants.OrderBy(p => p.UserId).ToList(),
                c.Messages.OrderBy(m => m.MessageSentTime).Select(MapToMessageDto).ToList(),
                c.Participants.ToDictionary(p => p.UserId, p => p.LastMessageReadTime ?? DateTime.MinValue)
            );
        }

        private MessageDataTransferObject MapToMessageDto(Message m)
        {
            var type = m switch { TextMessage => MessageType.MessageText, ImageMessage => MessageType.MessageImage, RentalRequestMessage => MessageType.MessageRentalRequest, CashAgreementMessage => MessageType.MessageCashAgreement, SystemMessage => MessageType.MessageSystem, _ => throw new ArgumentException() };
            var content = m switch { TextMessage t => t.TextMessageContent, RentalRequestMessage r => r.RequestContent, SystemMessage s => s.MessageContent, _ => m.MessageContentAsString } ?? string.Empty;
            return new MessageDataTransferObject(m.MessageId, m.ConversationId, m.MessageSenderId, m.MessageReceiverId, m.MessageSentTime, content, type, m is ImageMessage i ? i.MessageImageUrl ?? "" : "", m is RentalRequestMessage rr ? rr.IsRequestResolved : m is CashAgreementMessage ca && ca.IsCashAgreementResolved, m is RentalRequestMessage ra && ra.IsRequestAccepted, m is CashAgreementMessage cab && cab.IsCashAgreementAcceptedByBuyer, m is CashAgreementMessage cas && cas.IsCashAgreementAcceptedBySeller, m is CashAgreementMessage cap ? cap.CashPaymentId : -1, m is RentalRequestMessage rar ? rar.RentalRequestId : -1);
        }

        private Message MapDtoToEntity(MessageDataTransferObject dto)
        {
            Message msg = dto.Type switch
            {
                MessageType.MessageText => new TextMessage { TextMessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageImage => new ImageMessage { MessageImageUrl = dto.ImageUrl, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageRentalRequest => new RentalRequestMessage { RentalRequestId = dto.RequestId, IsRequestResolved = dto.IsResolved, IsRequestAccepted = dto.IsAccepted, RequestContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageCashAgreement => new CashAgreementMessage { CashPaymentId = dto.PaymentId, IsCashAgreementResolved = dto.IsResolved, IsCashAgreementAcceptedByBuyer = dto.IsAcceptedByBuyer, IsCashAgreementAcceptedBySeller = dto.IsAcceptedBySeller, Conversation = null!, Sender = null!, Receiver = null! },
                MessageType.MessageSystem => new SystemMessage { MessageContent = dto.Content, Conversation = null!, Sender = null!, Receiver = null! },
                _ => throw new ArgumentOutOfRangeException()
            };
            msg.MessageId = dto.Id; msg.ConversationId = dto.ConversationId; msg.MessageSenderId = dto.SenderId; msg.MessageReceiverId = dto.ReceiverId; msg.MessageSentTime = dto.SentAt; msg.MessageContentAsString = dto.Content;
            return msg;
        }
    }
}
