namespace AIAgent.Core.Models;

public class ChatMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? TokenCount { get; set; }
    public string? ModelUsed { get; set; }
}

public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = "新对话";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<ChatMessage> Messages { get; set; } = new();
}

public class ChatRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class ChatResponse
{
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? TokenUsed { get; set; }
    public string? ThinkingProcess { get; set; }
}

public class ChatStreamChunk
{
    public string Content { get; set; } = string.Empty;
    public bool IsThinking { get; set; }
    public bool IsComplete { get; set; }
    public int? TokenUsed { get; set; }
}
