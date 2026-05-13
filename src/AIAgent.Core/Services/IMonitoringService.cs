using AIAgent.Core.Models;

namespace AIAgent.Core.Services;

public interface IMonitoringService
{
    Task LogAccessAsync(AccessLog log);
    Task<DashboardStats> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<IpStatistics>> GetIpStatisticsAsync();
    Task<List<AccessLog>> GetAccessLogsAsync(string? ipAddress = null, DateTime? startDate = null, DateTime? endDate = null, int maxResults = 100);
    Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date);
    Task BlacklistIpAsync(string ipAddress, string? remark = null);
    Task WhitelistIpAsync(string ipAddress);
    Task RemoveFromBlacklistAsync(string ipAddress);
    Task<bool> IsIpBlacklistedAsync(string ipAddress);
    Task<long> GetTotalTokensByIpAsync(string ipAddress);
}
