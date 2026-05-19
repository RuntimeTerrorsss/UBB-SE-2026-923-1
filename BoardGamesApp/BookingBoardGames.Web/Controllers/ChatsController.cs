using System;
using System.Linq;
using System.Threading.Tasks;
using BookingBoardGames.Data.Enum;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Sharing.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookingBoardGames.Web.Controllers
{
    public class ChatsController : BaseController
    {
        private readonly IConversationService _conversationService;

        public ChatsController(IConversationService conversationService)
        {
            _conversationService = conversationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
            return View(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetChat(int conversationId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return redirect;

            int currentUserId = CurrentUserId ?? -1;
            _conversationService.Initialize(currentUserId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);

            if (conversation == null) return NotFound();

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserName = await _conversationService.GetOtherUserNameByConversationDTO(conversation);

            return PartialView("_ActiveChat", conversation);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            int senderId = CurrentUserId ?? -1;
            _conversationService.Initialize(senderId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            if (conversation == null) return NotFound();

            var receiver = conversation.Participants.FirstOrDefault(p => p.UserId != senderId);
            if (receiver == null) return BadRequest();

            var dto = new MessageDataTransferObject(
                Id: 0,
                ConversationId: conversationId,
                SenderId: senderId,
                ReceiverId: receiver.UserId,
                SentAt: DateTime.Now,
                Content: content,
                Type: MessageType.MessageText,
                ImageUrl: string.Empty,
                IsResolved: false,
                IsAccepted: false,
                IsAcceptedByBuyer: false,
                IsAcceptedBySeller: false,
                PaymentId: -1,
                RequestId: -1
            );

            await _conversationService.SendMessage(dto);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ResolveRentalRequest(int messageId, int conversationId, bool accepted)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return NotFound();
            }

            if (message.SenderId == userId)
            {
                return BadRequest("Only the game owner can accept or decline this request.");
            }

            var updated = message with
            {
                IsAccepted = accepted,
                IsResolved = !accepted,
            };

            await _conversationService.UpdateMessage(updated);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> CancelRentalRequest(int messageId, int conversationId)
        {
            var redirect = RequireLogin();
            if (redirect != null) return Unauthorized();

            int userId = CurrentUserId ?? -1;
            _conversationService.Initialize(userId);

            var conversations = await _conversationService.FetchConversations();
            var conversation = conversations.FirstOrDefault(c => c.Id == conversationId);
            var message = conversation?.MessageList.FirstOrDefault(m => m.Id == messageId);

            if (message == null || message.Type != MessageType.MessageRentalRequest)
            {
                return NotFound();
            }

            if (message.SenderId != userId)
            {
                return BadRequest("Only the person who sent the request can cancel it.");
            }

            var updated = message with
            {
                IsAccepted = false,
                IsResolved = true,
            };

            await _conversationService.UpdateMessage(updated);
            return Ok();
        }
    }
}
