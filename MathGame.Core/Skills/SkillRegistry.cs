namespace MathGame.Core.Skills;

/// <summary>Looks up skills by the id used in stages.json.</summary>
public sealed class SkillRegistry
{
    private readonly Dictionary<string, ISkill> _skills;

    public SkillRegistry(IEnumerable<ISkill> skills)
    {
        _skills = skills.ToDictionary(s => s.Id, StringComparer.Ordinal);
    }

    public static SkillRegistry Default { get; } = new(
    [
        new GuessOperatorSkill(),
        new MissingNumberSkill(),
        new OrderOfOperationsSkill(),
        new PrimeOrNotSkill(),
        new PrimeFactorsSkill(),
        new NegativeNumbersSkill(),
    ]);

    public IEnumerable<ISkill> All => _skills.Values;

    public bool Contains(string id) => _skills.ContainsKey(id);

    public ISkill Get(string id) =>
        _skills.TryGetValue(id, out var skill)
            ? skill
            : throw new KeyNotFoundException($"Unknown skill '{id}'.");
}
