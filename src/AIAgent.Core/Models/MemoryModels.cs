namespace AIAgent.Core.Models;

public class MemoryEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "chat"; // chat, fact, preference
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public float[]? Embedding { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MemoryQuery
{
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 50;
    public string? SessionId { get; set; }
}

public class MemoryConfig
{
    public string ChromaUrl { get; set; } = "http://localhost:8000";
    public string Mem0Url { get; set; } = "http://localhost:8001";
    public int MaxHistoryItems { get; set; } = 50;
    public string CollectionName { get; set; } = "aiagent_memory";
}
