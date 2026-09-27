using System.Data;
using MathGame.Core.Challenges;

namespace MathGame.Tests;

internal static class TestHelpers
{
    /// <summary>Evaluates a displayed expression like <c>3 − (−4) × 2</c> with an independent evaluator.</summary>
    public static double Evaluate(string expression)
    {
        var ascii = expression.Replace('−', '-').Replace('×', '*').Replace('÷', '/');
        return Convert.ToDouble(new DataTable().Compute(ascii, null));
    }

    /// <summary>True when <c>left = right</c> holds for an equation such as <c>12 + 8 = 20</c>.</summary>
    public static bool Holds(string equation)
    {
        var sides = equation.Split('=');
        return Math.Abs(Evaluate(sides[0]) - Evaluate(sides[1])) < 1e-9;
    }

    public static Guess RightGuess(Challenge challenge) => challenge switch
    {
        ChoiceChallenge c => new ChoiceGuess(c.Choices.ToList().FindIndex(x => x.IsCorrect)),
        NumberChallenge n => new NumberGuess(n.Answer),
        _ => throw new NotSupportedException(),
    };

    public static Guess WrongGuess(Challenge challenge) => challenge switch
    {
        ChoiceChallenge c => new ChoiceGuess(c.Choices.ToList().FindIndex(x => !x.IsCorrect)),
        NumberChallenge n => new NumberGuess(n.Answer + 1),
        _ => throw new NotSupportedException(),
    };
}
