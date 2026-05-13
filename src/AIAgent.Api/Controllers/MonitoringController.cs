using AIAgent.Core.Models;
using AIAgent.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIAgent.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;

    public MonitoringController(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStats>> GetDashboardStats(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate)
    {
        var stats = await _monitoringService.GetDashboardStatsAsync(startDate, endDate);
        return Ok(stats);
    }

    [HttpGet("ips")]
    public async Task<ActionResult<List<IpStatistics>>> GetIpStatistics()
    {
        var stats = await _monitoringService.GetIpStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("logs")]
    public async Task<ActionResult<List<AccessLog>>> GetAccessLogs(
        [FromQuery] string? ipAddress,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int maxResults = 100)
    {
        var logs = await _monitoringService.GetAccessLogsAsync(ipAddress, startDate, endDate, maxResults);
        return Ok(logs);
    }

    [HttpGet("hourly/{date}")]
    public async Task<ActionResult<List<HourlyStats>>> GetHourlyStats(DateTime date)
    {
        var stats = await _monitoringService.GetHourlyStatsAsync(date);
        return Ok(stats);
    }

    [HttpPost("blacklist/{ipAddress}")]
    public async Task<IActionResult> BlacklistIp(string ipAddress, [FromQuery] string? remark)
    {
        await _monitoringService.BlacklistIpAsync(ipAddress, remark);
        return Ok();
    }

    [HttpPost("whitelist/{ipAddress}")]
    public async Task<IActionResult> WhitelistIp(string ipAddress)
    {
        await _monitoringService.WhitelistIpAsync(ipAddress);
        return Ok();
    }

    [HttpDelete("blacklist/{ipAddress}")]
    public async Task<IActionResult> RemoveFromBlacklist(string ipAddress)
    {
        await _monitoringService.RemoveFromBlacklistAsync(ipAddress);
        return Ok();
    }

    [HttpGet("tokens/{ipAddress}")]
    public async Task<ActionResult<long>> GetTotalTokensByIp(string ipAddress)
    {
        var tokens = await _monitoringService.GetTotalTokensByIpAsync(ipAddress);
        return Ok(tokens);
    }
}
