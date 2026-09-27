namespace MathGame.Core.Skills;

/// <summary>Shared number ranges and operand generation for the arithmetic skills.</summary>
internal static class Arithmetic
{
    /// <param name="MaxOperand">Largest number in addition and subtraction.</param>
    /// <param name="MaxFactor">Largest factor in multiplication and division.</param>
    public sealed record Limits(int MaxOperand, int MaxFactor);

    public static Limits For(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => new(20, 5),
        Difficulty.Medium => new(50, 10),
        _ => new(100, 12),
    };

    public static Operation RandomOperation(Random random) => (Operation)random.Next(4);

    /// <summary>
    /// Picks operands for <paramref name="operation"/>: subtraction never goes negative,
    /// and division is built backwards from the quotient so it always comes out whole.
    /// </summary>
    public static (int Left, int Right) Operands(Random random, Operation operation, Limits limits)
    {
        int Operand() => random.Next(1, limits.MaxOperand + 1);
        int Factor() => random.Next(1, limits.MaxFactor + 1);

        switch (operation)
        {
            case Operation.Add:
                return (Operand(), Operand());
            case Operation.Subtract:
                var (a, b) = (Operand(), Operand());
                return a >= b ? (a, b) : (b, a);
            case Operation.Multiply:
                return (Factor(), Factor());
            case Operation.Divide:
                var divisor = Factor();
                return (divisor * Factor(), divisor);
            default:
                throw new ArgumentOutOfRangeException(nameof(operation));
        }
    }

    /// <summary>
    /// Evaluates <c>a op1 b op2 c</c> with the usual precedence: × and ÷ before + and −.
    /// Returns null if a division isn't exact.
    /// </summary>
    public static int? Evaluate(int a, Operation op1, int b, Operation op2, int c)
    {
        if (IsMultiplicative(op2) && !IsMultiplicative(op1))
        {
            return op2.Apply(b, c) is { } right ? op1.Apply(a, right) : null;
        }

        return op1.Apply(a, b) is { } left ? op2.Apply(left, c) : null;
    }

    public static bool IsMultiplicative(Operation operation) =>
        operation is Operation.Multiply or Operation.Divide;

    public static T Pick<T>(this Random random, IReadOnlyList<T> items) => items[random.Next(items.Count)];
}
