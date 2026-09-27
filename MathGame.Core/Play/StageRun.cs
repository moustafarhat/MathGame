using MathGame.Core.Challenges;
using MathGame.Core.Skills;
using MathGame.Core.Stages;

namespace MathGame.Core.Play;

public sealed record StageResult(int Correct, int Total, int Stars);

public static class StarRating
{
    /// <summary>1 star from 60% correct, 2 from 80%, 3 from 95%. Below 60% the stage isn't passed.</summary>
    public static int For(int correct, int total)
    {
        var accuracy = total == 0 ? 0 : (double)correct / total;
        return accuracy switch
        {
            >= 0.95 => 3,
            >= 0.8 => 2,
            >= 0.6 => 1,
            _ => 0,
        };
    }
}

/// <summary>
/// One play-through of a stage. Each question gets a single answer; after it the player
/// sees the explanation and calls <see cref="Advance"/>.
/// </summary>
public sealed class StageRun
{
    private readonly SkillRegistry _skills;
    private readonly Random _random;
    private readonly HashSet<string> _seenPrompts = [];

    public StageRun(Stage stage, SkillRegistry skills, Random? random = null)
    {
        Stage = stage;
        _skills = skills;
        _random = random ?? Random.Shared;
        Current = NextChallenge();
    }

    public Stage Stage { get; }

    public Challenge Current { get; private set; }

    /// <summary>1-based number of the current question.</summary>
    public int Number { get; private set; } = 1;

    public int Total => Stage.Questions;

    public int Correct { get; private set; }

    public int Mistakes { get; private set; }

    public int Streak { get; private set; }

    public int BestStreak { get; private set; }

    /// <summary>True once the current question has been answered, right or wrong.</summary>
    public bool IsAnswered { get; private set; }

    public bool IsFinished { get; private set; }

    public StageResult Result => IsFinished
        ? new StageResult(Correct, Total, StarRating.For(Correct, Total))
        : throw new InvalidOperationException("The stage isn't finished yet.");

    public bool Submit(Guess guess)
    {
        if (IsAnswered)
        {
            throw new InvalidOperationException("This question was already answered; call Advance first.");
        }

        IsAnswered = true;
        if (!Current.Accepts(guess))
        {
            Mistakes++;
            Streak = 0;
            return false;
        }

        Correct++;
        Streak++;
        BestStreak = Math.Max(BestStreak, Streak);
        return true;
    }

    /// <summary>Moves to the next question. Returns false, and finishes the stage, after the last one.</summary>
    public bool Advance()
    {
        if (!IsAnswered)
        {
            throw new InvalidOperationException("Answer the current question before moving on.");
        }

        if (Number == Total)
        {
            IsFinished = true;
            return false;
        }

        Number++;
        Current = NextChallenge();
        IsAnswered = false;
        return true;
    }

    private Challenge NextChallenge()
    {
        // Avoid repeating a question within the stage. Small pools (e.g. primes up to 30)
        // can run out, so give up after a few tries rather than loop forever.
        Challenge challenge;
        var attempts = 0;
        do
        {
            var skill = _skills.Get(Stage.Skills[_random.Next(Stage.Skills.Count)]);
            challenge = skill.Generate(_random, Stage.Difficulty);
        }
        while (!_seenPrompts.Add(challenge.Prompt) && ++attempts < 20);

        return challenge;
    }
}
