namespace AIAgent.Core.Models;

public class AccessLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IpAddress { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? TokenUsed { get; set; }
    public long? ResponseTimeMs { get; set; }
    public int StatusCode { get; set; }
    public string? UserAgent { get; set; }
}

public class IpStatistics
{
    public string IpAddress { get; set; } = string.Empty;
    public int AccessCount { get; set; }
    public long TotalTokens { get; set; }
    public DateTime FirstAccess { get; set; }
    public DateTime LastAccess { get; set; }
    public bool IsBlacklisted { get; set; }
    public string? Remark { get; set; }
}

public class HourlyStats
{
    public DateTime Hour { get; set; }
    public int AccessCount { get; set; }
    public int UniqueIps { get; set; }
    public long TotalTokens { get; set; }
}

public class DashboardStats
{
    public int TotalAccessCount { get; set; }
    public int ActiveSessions { get; set; }
    public int UniqueIps { get; set; }
    public long TotalTokensUsed { get; set; }
    public List<HourlyStats> HourlyStatistics { get; set; } = new();
    public List<IpStatistics> TopIps { get; set; } = new();
}

public class MonitoringConfig
{
    public bool EnableIpTracking { get; set; } = true;
    public bool EnableTokenTracking { get; set; } = true;
    public int RetentionDays { get; set; } = 30;
    public List<string> BlacklistedIps { get; set; } = new();
    public List<string> WhitelistedIps { get; set; } = new();
}
