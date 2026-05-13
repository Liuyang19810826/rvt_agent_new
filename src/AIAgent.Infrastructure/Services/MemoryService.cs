using AIAgent.Core.Models;
using AIAgent.Core.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIAgent.Infrastructure.Services;

public class MemoryService : IMemoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MemoryService> _logger;
    private readonly MemoryConfig _config;

    public MemoryService(HttpClient httpClient, ILogger<MemoryService> logger, MemoryConfig config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        _httpClient.BaseAddress = new Uri(_config.Mem0Url);
    }

    public async Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                messages = new[]
                {
                    new { role = "user", content = entry.Content }
                },
                user_id = entry.UserId,
                agent_id = entry.SessionId,
                metadata = new
                {
                    type = entry.Type,
                    session_id = entry.SessionId,
                    created_at = entry.CreatedAt
                }
            };

            var response = await _httpClient.PostAsJsonAsync("/v1/memories/", payload, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "存储记忆失败");
        }
    }

    public async Task<List<MemoryEntry>> RetrieveAsync(MemoryQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/v1/memories/search/?query={Uri.EscapeDataString(query.Query)}&user_id={query.UserId}&limit={query.MaxResults}";
            if (!string.IsNullOrEmpty(query.SessionId))
            {
                url += $"&agent_id={query.SessionId}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<List<MemorySearchResult>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return results?.Select(r => new MemoryEntry
            {
                Id = r.Id ?? Guid.NewGuid().ToString(),
                UserId = query.UserId,
                Content = r.Memory ?? string.Empty,
                Type = r.Metadata?.Type ?? "chat",
                CreatedAt = r.Metadata?.CreatedAt ?? DateTime.UtcNow,
                Metadata = r.Metadata?.AdditionalData ?? new Dictionary<string, object>()
            }).ToList() ?? new List<MemoryEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检索记忆失败");
            return new List<MemoryEntry>();
        }
    }

    public async Task<List<MemoryEntry>> GetHistoryAsync(string userId, string? sessionId = null, int maxItems = 50, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = $"/v1/memories/?user_id={userId}&limit={maxItems}";
            if (!string.IsNullOrEmpty(sessionId))
            {
                url += $"&agent_id={sessionId}";
            }

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<List<MemorySearchResult>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return results?.Select(r => new MemoryEntry
            {
                Id = r.Id ?? Guid.NewGuid().ToString(),
                UserId = userId,
                SessionId = r.Metadata?.SessionId ?? sessionId ?? string.Empty,
                Content = r.Memory ?? string.Empty,
                Type = r.Metadata?.Type ?? "chat",
                CreatedAt = r.Metadata?.CreatedAt ?? DateTime.UtcNow,
                Metadata = r.Metadata?.AdditionalData ?? new Dictionary<string, object>()
            }).ToList() ?? new List<MemoryEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取历史记录失败");
            return new List<MemoryEntry>();
        }
    }

    public async Task DeleteAsync(string memoryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/memories/{memoryId}/", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除记忆失败");
        }
    }

    public async Task ClearUserMemoryAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/v1/memories/?user_id={userId}", cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清除用户记忆失败");
        }
    }
}

public class MemorySearchResult
{
    public string? Id { get; set; }
    public string? Memory { get; set; }
    public MemoryMetadata? Metadata { get; set; }
}

public class MemoryMetadata
{
    public string? Type { get; set; }
    public string? SessionId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}
