using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary><c>12 + ? = 20</c> — type the hidden number.</summary>
public sealed class MissingNumberSkill : ISkill
{
    public string Id => "missing-number";

    public string Name => "Missing number";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        // Easy sticks to + and −; the operands are always ≥ 1, so every blank has exactly one answer.
        var operation = difficulty == Difficulty.Easy
            ? random.Pick([Operation.Add, Operation.Subtract])
            : Arithmetic.RandomOperation(random);
        var (left, right) = Arithmetic.Operands(random, operation, Arithmetic.For(difficulty));
        var result = operation.Apply(left, right)!.Value;

        var parts = new[] { left.ToString(), right.ToString(), result.ToString() };
        var hidden = random.Next(parts.Length);
        var answer = hidden switch { 0 => left, 1 => right, _ => result };
        parts[hidden] = "?";

        return new NumberChallenge(
            Id,
            $"{parts[0]} {operation.Symbol()} {parts[1]} = {parts[2]}",
            $"{left} {operation.Symbol()} {right} = {result}",
            answer);
    }
}
