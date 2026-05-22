using AIAgent.Core.Models;
using AIAgent.Core.Services;
using AIAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OAIChat = OpenAI.Chat;
using System.ClientModel;
using System.Runtime.CompilerServices;

namespace AIAgent.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IMemoryService _memoryService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<ChatService> _logger;
    private readonly IConfiguration _configuration;

    public ChatService(
        ApplicationDbContext dbContext,
        IMemoryService memoryService,
        ISettingsService settingsService,
        ILogger<ChatService> logger,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _memoryService = memoryService;
        _settingsService = settingsService;
        _logger = logger;
        _configuration = configuration;
    }

    private string GetSystemPrompt()
    {
        return _configuration["AI:SystemPrompt"] ?? "你是一个 helpful AI 助手。请用中文回答用户的问题。";
    }

    public async Task<AIAgent.Core.Models.ChatResponse> SendMessageAsync(AIAgent.Core.Models.ChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _settingsService.GetActiveProviderAsync();
            if (provider == null)
            {
                return new AIAgent.Core.Models.ChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "未配置有效的AI提供商"
                };
            }

            // 获取历史记录（最多50条）
            var history = await _memoryService.GetHistoryAsync(request.UserId, request.SessionId, 50, cancellationToken);

            // 创建 OpenAI 客户端
            var chatClient = new OAIChat.ChatClient(
                model: provider.Model,
                credential: new ApiKeyCredential(provider.ApiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(provider.Endpoint)
                }
            );

            // 构建消息列表
            var messages = new List<OAIChat.ChatMessage>();

            // 添加系统提示
            messages.Add(new OAIChat.SystemChatMessage(GetSystemPrompt()));

            // 添加历史记录
            foreach (var h in history)
            {
                if (h.Type == "user")
                    messages.Add(new OAIChat.UserChatMessage(h.Content));
                else
                    messages.Add(new OAIChat.AssistantChatMessage(h.Content));
            }

            // 添加当前消息
            messages.Add(new OAIChat.UserChatMessage(request.Message));

            // 调用 AI
            var result = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);
            var completion = result.Value;

            var aiMessage = completion.Content[0]?.Text ?? string.Empty;

            // 保存用户消息到数据库
            var userMessage = new Core.Models.ChatMessage
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                Role = "user",
                Content = request.Message,
                Timestamp = DateTime.UtcNow
            };
            _dbContext.ChatMessages.Add(userMessage);

            // 保存 AI 响应到数据库
            var assistantMessage = new Core.Models.ChatMessage
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                Role = "assistant",
                Content = aiMessage,
                Timestamp = DateTime.UtcNow,
                ModelUsed = provider.Model
            };
            _dbContext.ChatMessages.Add(assistantMessage);

            // 保存到记忆服务
            await _memoryService.StoreAsync(new MemoryEntry
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                Content = request.Message,
                Type = "user",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _memoryService.StoreAsync(new MemoryEntry
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                Content = aiMessage,
                Type = "assistant",
                CreatedAt = DateTime.UtcNow
            }, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // 更新会话时间
            var session = await _dbContext.ChatSessions.FindAsync(request.SessionId);
            if (session != null)
            {
                session.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return new AIAgent.Core.Models.ChatResponse
            {
                IsSuccess = true,
                Message = aiMessage,
                TokenUsed = (int?)completion.Usage?.TotalTokenCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息时发生错误");
            return new AIAgent.Core.Models.ChatResponse
            {
                IsSuccess = false,
                ErrorMessage = $"发送消息失败: {ex.Message}"
            };
        }
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(AIAgent.Core.Models.ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var provider = await _settingsService.GetActiveProviderAsync();
        if (provider == null)
        {
            yield return "未配置有效的AI提供商";
            yield break;
        }

        // 获取历史记录（最多50条）
        var history = await _memoryService.GetHistoryAsync(request.UserId, request.SessionId, 50, cancellationToken);

        // 创建 OpenAI 客户端
        var chatClient = new OAIChat.ChatClient(
            model: provider.Model,
            credential: new ApiKeyCredential(provider.ApiKey),
            options: new OpenAIClientOptions
            {
                Endpoint = new Uri(provider.Endpoint)
            }
        );

        // 构建消息列表
        var messages = new List<OAIChat.ChatMessage>();

        // 添加系统提示
        messages.Add(new OAIChat.SystemChatMessage(GetSystemPrompt()));

        // 添加历史记录
        foreach (var h in history)
        {
            if (h.Type == "user")
                messages.Add(new OAIChat.UserChatMessage(h.Content));
            else
                messages.Add(new OAIChat.AssistantChatMessage(h.Content));
        }

        // 添加当前消息
        messages.Add(new OAIChat.UserChatMessage(request.Message));

        // 保存用户消息到数据库
        var userMessage = new Core.Models.ChatMessage
        {
            SessionId = request.SessionId,
            UserId = request.UserId,
            Role = "user",
            Content = request.Message,
            Timestamp = DateTime.UtcNow
        };
        _dbContext.ChatMessages.Add(userMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // 保存到记忆服务
        await _memoryService.StoreAsync(new MemoryEntry
        {
            UserId = request.UserId,
            SessionId = request.SessionId,
            Content = request.Message,
            Type = "user",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        // 调用 AI 流式接口
        var fullResponse = new System.Text.StringBuilder();
        var streamingResult = chatClient.CompleteChatStreamingAsync(messages, cancellationToken: cancellationToken);
        await foreach (var update in streamingResult)
        {
            foreach (var contentPart in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(contentPart.Text))
                {
                    fullResponse.Append(contentPart.Text);
                    yield return contentPart.Text;
                }
            }
        }

        // 保存 AI 响应到数据库
        var assistantMessage = new Core.Models.ChatMessage
        {
            SessionId = request.SessionId,
            UserId = request.UserId,
            Role = "assistant",
            Content = fullResponse.ToString(),
            Timestamp = DateTime.UtcNow,
            ModelUsed = provider.Model
        };
        _dbContext.ChatMessages.Add(assistantMessage);

        // 保存到记忆服务
        await _memoryService.StoreAsync(new MemoryEntry
        {
            UserId = request.UserId,
            SessionId = request.SessionId,
            Content = fullResponse.ToString(),
            Type = "assistant",
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);

        // 更新会话时间
        var session = await _dbContext.ChatSessions.FindAsync(request.SessionId);
        if (session != null)
        {
            session.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<ChatSession> CreateSessionAsync(string userId)
    {
        var session = new ChatSession
        {
            UserId = userId,
            Title = "新对话",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _dbContext.ChatSessions.Add(session);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId)
    {
        return await _dbContext.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionAsync(string sessionId)
    {
        return await _dbContext.ChatSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
    }

    public async Task DeleteSessionAsync(string sessionId)
    {
        var session = await _dbContext.ChatSessions.FindAsync(sessionId);
        if (session != null)
        {
            _dbContext.ChatSessions.Remove(session);
            await _dbContext.SaveChangesAsync();
        }
    }
}
