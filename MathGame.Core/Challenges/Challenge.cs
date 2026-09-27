using System.Globalization;

namespace MathGame.Core.Challenges;

/// <summary>What the player answered.</summary>
public abstract record Guess;

public sealed record ChoiceGuess(int Index) : Guess;

public sealed record NumberGuess(int Value) : Guess;

/// <summary>One question inside a stage.</summary>
/// <param name="SkillId">The skill that generated it.</param>
/// <param name="Prompt">What the player sees, e.g. <c>12 ? 5 = 60</c>.</param>
/// <param name="Explanation">Shown after answering, e.g. <c>12 × 5 = 60</c>.</param>
public abstract record Challenge(string SkillId, string Prompt, string Explanation)
{
    public abstract bool Accepts(Guess guess);
}

/// <param name="Keys">Keyboard shortcuts besides the choice's position number, e.g. "p+" for addition.</param>
public sealed record Choice(string Label, bool IsCorrect, string Keys = "");

/// <summary>Pick one of several answers. More than one may be correct.</summary>
public sealed record ChoiceChallenge(string SkillId, string Prompt, string Explanation, IReadOnlyList<Choice> Choices)
    : Challenge(SkillId, Prompt, Explanation)
{
    public override bool Accepts(Guess guess) =>
        guess is ChoiceGuess { Index: var index }
        && index >= 0 && index < Choices.Count
        && Choices[index].IsCorrect;

    /// <summary>Maps a key press to a choice: 1–9 by position, or the choice's own shortcut keys.</summary>
    public int? ChoiceForKey(char key)
    {
        if (key is >= '1' and <= '9' && key - '1' < Choices.Count)
        {
            return key - '1';
        }

        var lower = char.ToLowerInvariant(key);
        for (var i = 0; i < Choices.Count; i++)
        {
            if (Choices[i].Keys.Contains(lower))
            {
                return i;
            }
        }

        return null;
    }
}

/// <summary>Type in a whole number.</summary>
public sealed record NumberChallenge(string SkillId, string Prompt, string Explanation, int Answer)
    : Challenge(SkillId, Prompt, Explanation)
{
    public override bool Accepts(Guess guess) => guess is NumberGuess { Value: var value } && value == Answer;

    /// <summary>Parses typed input, accepting both the ASCII hyphen and the Unicode minus sign.</summary>
    public static bool TryParseAnswer(string? text, out int value) =>
        int.TryParse(
            text?.Trim().Replace('−', '-'),
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out value);
}
