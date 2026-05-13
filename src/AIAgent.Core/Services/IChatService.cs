using AIAgent.Core.Models;
using System.Runtime.CompilerServices;

namespace AIAgent.Core.Services;

public interface IChatService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> SendMessageStreamAsync(ChatRequest request, CancellationToken cancellationToken = default);
    Task<ChatSession> CreateSessionAsync(string userId);
    Task<List<ChatSession>> GetUserSessionsAsync(string userId);
    Task<ChatSession?> GetSessionAsync(string sessionId);
    Task DeleteSessionAsync(string sessionId);
}
