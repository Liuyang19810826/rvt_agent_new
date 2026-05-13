using AIAgent.Core.Models;
using AIAgent.Core.Services;
using AIAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIAgent.Infrastructure.Services;

public class MonitoringService : IMonitoringService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(ApplicationDbContext dbContext, ILogger<MonitoringService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task LogAccessAsync(AccessLog log)
    {
        try
        {
            _dbContext.AccessLogs.Add(log);
            
            // 更新或创建IP统计
            var ipStat = await _dbContext.IpStatistics.FindAsync(log.IpAddress);
            if (ipStat == null)
            {
                ipStat = new IpStatistics
                {
                    IpAddress = log.IpAddress,
                    AccessCount = 0,
                    TotalTokens = 0,
                    FirstAccess = log.Timestamp,
                    LastAccess = log.Timestamp
                };
                _dbContext.IpStatistics.Add(ipStat);
            }

            ipStat.AccessCount++;
            ipStat.LastAccess = log.Timestamp;
            if (log.TokenUsed.HasValue)
            {
                ipStat.TotalTokens += log.TokenUsed.Value;
            }

            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "记录访问日志失败");
        }
    }

    public async Task<DashboardStats> GetDashboardStatsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;

        var logs = await _dbContext.AccessLogs
            .Where(l => l.Timestamp >= start && l.Timestamp <= end)
            .ToListAsync();

        var hourlyStats = logs
            .GroupBy(l => new DateTime(l.Timestamp.Year, l.Timestamp.Month, l.Timestamp.Day, l.Timestamp.Hour, 0, 0))
            .Select(g => new HourlyStats
            {
                Hour = g.Key,
                AccessCount = g.Count(),
                UniqueIps = g.Select(l => l.IpAddress).Distinct().Count(),
                TotalTokens = g.Sum(l => (long)(l.TokenUsed ?? 0))
            })
            .OrderBy(h => h.Hour)
            .ToList();

        var topIps = await _dbContext.IpStatistics
            .OrderByDescending(i => i.AccessCount)
            .Take(10)
            .ToListAsync();

        return new DashboardStats
        {
            TotalAccessCount = logs.Count,
            ActiveSessions = logs.Select(l => l.UserId).Distinct().Count(),
            UniqueIps = logs.Select(l => l.IpAddress).Distinct().Count(),
            TotalTokensUsed = logs.Sum(l => (long)(l.TokenUsed ?? 0)),
            HourlyStatistics = hourlyStats,
            TopIps = topIps
        };
    }

    public async Task<List<IpStatistics>> GetIpStatisticsAsync()
    {
        return await _dbContext.IpStatistics
            .OrderByDescending(i => i.AccessCount)
            .ToListAsync();
    }

    public async Task<List<AccessLog>> GetAccessLogsAsync(string? ipAddress = null, DateTime? startDate = null, DateTime? endDate = null, int maxResults = 100)
    {
        var query = _dbContext.AccessLogs.AsQueryable();

        if (!string.IsNullOrEmpty(ipAddress))
        {
            query = query.Where(l => l.IpAddress == ipAddress);
        }

        if (startDate.HasValue)
        {
            query = query.Where(l => l.Timestamp >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(l => l.Timestamp <= endDate.Value);
        }

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Take(maxResults)
            .ToListAsync();
    }

    public async Task<List<HourlyStats>> GetHourlyStatsAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1);

        var logs = await _dbContext.AccessLogs
            .Where(l => l.Timestamp >= start && l.Timestamp < end)
            .ToListAsync();

        return logs
            .GroupBy(l => l.Timestamp.Hour)
            .Select(g => new HourlyStats
            {
                Hour = start.AddHours(g.Key),
                AccessCount = g.Count(),
                UniqueIps = g.Select(l => l.IpAddress).Distinct().Count(),
                TotalTokens = g.Sum(l => (long)(l.TokenUsed ?? 0))
            })
            .OrderBy(h => h.Hour)
            .ToList();
    }

    public async Task BlacklistIpAsync(string ipAddress, string? remark = null)
    {
        var ipStat = await _dbContext.IpStatistics.FindAsync(ipAddress);
        if (ipStat != null)
        {
            ipStat.IsBlacklisted = true;
            ipStat.Remark = remark;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task WhitelistIpAsync(string ipAddress)
    {
        var ipStat = await _dbContext.IpStatistics.FindAsync(ipAddress);
        if (ipStat != null)
        {
            ipStat.IsBlacklisted = false;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task RemoveFromBlacklistAsync(string ipAddress)
    {
        var ipStat = await _dbContext.IpStatistics.FindAsync(ipAddress);
        if (ipStat != null)
        {
            ipStat.IsBlacklisted = false;
            ipStat.Remark = null;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task<bool> IsIpBlacklistedAsync(string ipAddress)
    {
        var ipStat = await _dbContext.IpStatistics.FindAsync(ipAddress);
        return ipStat?.IsBlacklisted ?? false;
    }

    public async Task<long> GetTotalTokensByIpAsync(string ipAddress)
    {
        var ipStat = await _dbContext.IpStatistics.FindAsync(ipAddress);
        return ipStat?.TotalTokens ?? 0;
    }
}
