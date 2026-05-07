using System;
using System.Collections.Generic;
using System.Linq;
using BookingBoardGames.Data;
using BookingBoardGames.Data.DTO;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Data.Interfaces;

namespace BookingBoardGames.Data.Services
{
    public class ConversationService : IConversationService
    {
        private IConversationRepository ConversationRepository { get; set; }
        
        private IUserRepository userRepository;
        private IConversationNotifier notifier;

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

        private void NotifySubscribersAboutMessage(Message message)
        {
            IReadOnlyList<int> participants = this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessage(participants, message);
        }

        private void NotifySubscribersAboutMessageUpdate(Message message)
        {
            IReadOnlyList<int> participants = this.ConversationRepository.GetParticipantUserIds(message.ConversationId);
            this.notifier.NotifyMessageUpdate(participants, message);
        }

        private void NotifySubscribersAboutReadReceipt(ReadReceiptDTO readReceipt)
        {
            IReadOnlyList<int> participants = this.ConversationRepository.GetParticipantUserIds(readReceipt.ConversationId);
            this.notifier.NotifyReadReceipt(participants, readReceipt);
        }

        private void NotifySubscribersAboutNewConversation(Conversation conversation)
        {
            this.notifier.NotifyNewConversation(conversation);
        }

        public List<ConversationDTO> FetchConversations()
        {
            List<ConversationDTO> conversationList = new List<ConversationDTO>();

            foreach (var conversation in this.ConversationRepository.GetConversationsForUser(this.UserId))
            {
                conversationList.Add(this.ConversationToConversationDTO(conversation));
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

        public void SendMessage(MessageDataTransferObject message)
        {
            Message persisted = this.ConversationRepository.HandleNewMessage(this.MessageDTOToMessage(message));
            this.NotifySubscribersAboutMessage(persisted);
        }

        public async Task<int> CreateConversation(int senderId, int receiverId)
        {
            int conversationId = this.ConversationRepository.CreateConversation(senderId, receiverId);
            Conversation createdConversation = this.ConversationRepository.GetConversationById(conversationId);
            this.NotifySubscribersAboutNewConversation(createdConversation);
            return conversationId;
        }

        public void UpdateMessage(MessageDataTransferObject message)
        {
            Message? persisted = this.ConversationRepository.HandleMessageUpdate(this.MessageDTOToMessage(message));
            if (persisted != null)
            {
                this.NotifySubscribersAboutMessageUpdate(persisted);
            }
        }

        public async Task SendReadReceipt(ConversationDTO conversation)
        {
            var readReceipt = new ReadReceiptDTO(
                conversation.Id,
                this.UserId,
                conversation.Participants.First(participantItem => participantItem.UserId != this.UserId).UserId,
                DateTime.Now);
            this.ConversationRepository.HandleReadReceipt(readReceipt);
            this.NotifySubscribersAboutReadReceipt(readReceipt);
        }

        public void OnCardPaymentSelected(int messageId)
        {
            this.FinalizeRentalRequest(messageId);
        }

        public async Task OnCashPaymentSelected(int messageId, int paymentId)
        {
            this.FinalizeRentalRequest(messageId);
            this.SendCashAgreementMessage(messageId, paymentId);
        }

        private async Task FinalizeRentalRequest(int messageId)
        {
            Message? updated = this.ConversationRepository.HandleRentalRequestFinalization(messageId);
            if (updated != null)
            {
                this.NotifySubscribersAboutMessageUpdate(updated);
            }
        }

        private void SendCashAgreementMessage(int messageIdOfParentRentalRequestMessage, int paymentId)
        {
            Message? created = this.ConversationRepository.CreateCashAgreementMessage(messageIdOfParentRentalRequestMessage, paymentId);
            if (created != null)
            {
                this.NotifySubscribersAboutMessage(created);
            }
        }

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
