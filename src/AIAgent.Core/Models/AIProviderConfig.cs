namespace AIAgent.Core.Models;

public class AIProviderConfig
{
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 2048;
    public bool IsActive { get; set; }
}

public class AISettings
{
    public const int MaxProviders = 3;
    public List<AIProviderConfig> Providers { get; set; } = new();
}
