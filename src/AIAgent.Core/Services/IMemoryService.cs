using AIAgent.Core.Models;

namespace AIAgent.Core.Services;

public interface IMemoryService
{
    Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default);
    Task<List<MemoryEntry>> RetrieveAsync(MemoryQuery query, CancellationToken cancellationToken = default);
    Task<List<MemoryEntry>> GetHistoryAsync(string userId, string? sessionId = null, int maxItems = 50, CancellationToken cancellationToken = default);
    Task DeleteAsync(string memoryId, CancellationToken cancellationToken = default);
    Task ClearUserMemoryAsync(string userId, CancellationToken cancellationToken = default);
}
