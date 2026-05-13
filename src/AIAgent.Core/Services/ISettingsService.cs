using AIAgent.Core.Models;

namespace AIAgent.Core.Services;

public interface ISettingsService
{
    Task<AISettings> GetSettingsAsync();
    Task SaveSettingsAsync(AISettings settings);
    Task<AIProviderConfig?> GetActiveProviderAsync();
    Task SetActiveProviderAsync(string providerName);
    Task<bool> ValidateProviderAsync(AIProviderConfig provider);
}
