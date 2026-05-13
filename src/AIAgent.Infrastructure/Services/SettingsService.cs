using AIAgent.Core.Models;
using AIAgent.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OAIChat = OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;

namespace AIAgent.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SettingsService> _logger;
    private readonly string _settingsFilePath;

    public SettingsService(IConfiguration configuration, ILogger<SettingsService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.user.json");
    }

    public async Task<AISettings> GetSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<AISettings>(json);
                if (settings != null)
                {
                    return settings;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "读取用户设置失败");
        }

        // 返回默认配置
        var defaultSettings = new AISettings
        {
            Providers = new List<AIProviderConfig>()
        };
        var providers = _configuration.GetSection("AI:Providers").Get<List<AIProviderConfig>>();
        if (providers != null)
        {
            defaultSettings.Providers = providers;
        }

        return defaultSettings;
    }

    public async Task SaveSettingsAsync(AISettings settings)
    {
        try
        {
            // 限制最多3个提供商
            if (settings.Providers.Count > AISettings.MaxProviders)
            {
                settings.Providers = settings.Providers.Take(AISettings.MaxProviders).ToList();
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存用户设置失败");
            throw;
        }
    }

    public async Task<AIProviderConfig?> GetActiveProviderAsync()
    {
        var settings = await GetSettingsAsync();
        var activeProvider = settings.Providers.FirstOrDefault(p => p.IsActive);
        
        if (activeProvider == null && settings.Providers.Any())
        {
            activeProvider = settings.Providers.First();
        }

        return activeProvider;
    }

    public async Task SetActiveProviderAsync(string providerName)
    {
        var settings = await GetSettingsAsync();
        
        foreach (var provider in settings.Providers)
        {
            provider.IsActive = provider.Name == providerName;
        }

        await SaveSettingsAsync(settings);
    }

    public async Task<bool> ValidateProviderAsync(AIProviderConfig provider)
    {
        try
        {
            var chatClient = new OAIChat.ChatClient(
                model: provider.Model,
                credential: new ApiKeyCredential(provider.ApiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(provider.Endpoint)
                }
            );
            
            // 尝试发送一个简单的测试消息
            var messages = new List<OAIChat.ChatMessage>
            {
                new OAIChat.UserChatMessage("Hi")
            };

            var result = await chatClient.CompleteChatAsync(messages);
            var completion = result.Value;
            return completion.Content.Count > 0 && !string.IsNullOrEmpty(completion.Content[0].Text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证AI提供商失败");
            return false;
        }
    }
}
