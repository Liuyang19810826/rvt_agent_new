namespace AIAgent.Core.Skills;

public interface ISkill
{
    string Name { get; }
    string Description { get; }
    Task<SkillResult> ExecuteAsync(SkillContext context, CancellationToken cancellationToken = default);
}

public class SkillContext
{
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class SkillResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _skills = new();

    public void Register(ISkill skill)
    {
        _skills[skill.Name] = skill;
    }

    public ISkill? Get(string name)
    {
        return _skills.TryGetValue(name, out var skill) ? skill : null;
    }

    public IEnumerable<ISkill> GetAll()
    {
        return _skills.Values;
    }
}
