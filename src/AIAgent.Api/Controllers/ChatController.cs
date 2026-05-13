using AIAgent.Core.Models;
using AIAgent.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IMonitoringService _monitoringService;

    public ChatController(IChatService chatService, IMonitoringService monitoringService)
    {
        _chatService = chatService;
        _monitoringService = monitoringService;
    }

    [HttpPost("send")]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var startTime = DateTime.UtcNow;

        var response = await _chatService.SendMessageAsync(request);

        // Log access
        await _monitoringService.LogAccessAsync(new AccessLog
        {
            IpAddress = ipAddress,
            UserId = request.UserId,
            Endpoint = "/api/chat/send",
            Method = "POST",
            Timestamp = startTime,
            TokenUsed = response.TokenUsed,
            ResponseTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            StatusCode = response.IsSuccess ? 200 : 400,
            UserAgent = Request.Headers.UserAgent.ToString()
        });

        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSession>> CreateSession([FromQuery] string userId)
    {
        var session = await _chatService.CreateSessionAsync(userId);
        return Ok(session);
    }

    [HttpGet("sessions/{userId}")]
    public async Task<ActionResult<List<ChatSession>>> GetUserSessions(string userId)
    {
        var sessions = await _chatService.GetUserSessionsAsync(userId);
        return Ok(sessions);
    }

    [HttpGet("sessions/detail/{sessionId}")]
    public async Task<ActionResult<ChatSession>> GetSession(string sessionId)
    {
        var session = await _chatService.GetSessionAsync(sessionId);
        if (session == null)
        {
            return NotFound();
        }
        return Ok(session);
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> DeleteSession(string sessionId)
    {
        await _chatService.DeleteSessionAsync(sessionId);
        return NoContent();
    }
}
