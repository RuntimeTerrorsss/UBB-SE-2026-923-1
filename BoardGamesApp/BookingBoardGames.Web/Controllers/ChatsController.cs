using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BookingBoardGames.Sharing.Services;
using BookingBoardGames.Sharing.DTO;
using BookingBoardGames.Data.Enum;
using Microsoft.AspNetCore.Authorization;
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
                SentAt: DateTime.UtcNow,
                Content: content,
                Type: MessageType.MessageText,
                ImageUrl: "",
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
    }
}