using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

public enum Difficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3,
}

/// <summary>A kind of challenge the game can generate, such as "prime or not".</summary>
public interface ISkill
{
    /// <summary>Stable id referenced from stages.json.</summary>
    string Id { get; }

    string Name { get; }

    Challenge Generate(Random random, Difficulty difficulty);
}
