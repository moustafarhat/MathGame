using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary><c>12 ? 5 = 60</c> — pick the hidden operator. The original game.</summary>
public sealed class GuessOperatorSkill : ISkill
{
    public string Id => "guess-operator";

    public string Name => "Find the operator";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        var operation = Arithmetic.RandomOperation(random);
        var (left, right) = Arithmetic.Operands(random, operation, Arithmetic.For(difficulty));
        var result = operation.Apply(left, right)!.Value;

        // More than one operator can be right (2 ? 2 = 4 is both + and ×), so mark every one that works.
        var choices = Enum.GetValues<Operation>()
            .Select(op => new Choice(op.Symbol().ToString(), op.Apply(left, right) == result, op.Keys()))
            .ToList();

        var explanation = string.Join(
            "  and  ",
            Enum.GetValues<Operation>()
                .Where(op => op.Apply(left, right) == result)
                .Select(op => $"{left} {op.Symbol()} {right} = {result}"));

        return new ChoiceChallenge(Id, $"{left} ? {right} = {result}", explanation, choices);
    }
}
