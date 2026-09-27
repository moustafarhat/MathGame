using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary><c>−7 + 12 = ?</c> — arithmetic across zero.</summary>
public sealed class NegativeNumbersSkill : ISkill
{
    public string Id => "negative-numbers";

    public string Name => "Negative numbers";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        var (operations, range) = difficulty switch
        {
            Difficulty.Easy => (new[] { Operation.Add, Operation.Subtract }, 10),
            Difficulty.Medium => (new[] { Operation.Add, Operation.Subtract, Operation.Multiply }, 15),
            _ => (Enum.GetValues<Operation>(), 12),
        };

        int left, right, result;
        Operation operation;
        do
        {
            operation = random.Pick(operations);
            (left, right) = Operands(random, operation, range);
            result = operation.Apply(left, right)!.Value;
        }
        while (left >= 0 && right >= 0 && result >= 0);

        var expression = $"{Format.Number(left)} {operation.Symbol()} {Format.Operand(right)}";
        return new NumberChallenge(Id, $"{expression} = ?", $"{expression} = {Format.Number(result)}", result);
    }

    private static (int, int) Operands(Random random, Operation operation, int range)
    {
        int NonZero(int limit)
        {
            var n = random.Next(1, limit + 1);
            return random.Next(2) == 0 ? n : -n;
        }

        return operation switch
        {
            Operation.Multiply => (NonZero(10), NonZero(10)),
            Operation.Divide => DivisionPair(NonZero(10), NonZero(10)),
            _ => (random.Next(-range, range + 1), random.Next(-range, range + 1)),
        };

        // Build from the quotient so the division is exact.
        static (int, int) DivisionPair(int divisor, int quotient) => (divisor * quotient, divisor);
    }
}
