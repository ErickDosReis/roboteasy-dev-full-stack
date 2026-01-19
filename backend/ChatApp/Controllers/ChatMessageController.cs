using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using ChatApp.DTOs;
using ChatApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatMessageController(IChatMessageService service) : ControllerBase
{
    private readonly IChatMessageService _service = service;

    [HttpGet("{targetUserId}")]
    public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetHistory([FromRoute] string targetUserId, CancellationToken ct)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentUserId == null) return Unauthorized();

        var messages = await _service.GetConversationHistoryAsync(currentUserId, targetUserId, ct);

        var response = messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            SenderId = m.FromUserId,
            SenderName = m.FromUserName,
            Text = m.Content,
            Timestamp = m.SentAtUtc,
            IsMine = m.FromUserId == currentUserId
        });

        return Ok(response);
    }
}