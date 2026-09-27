using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary><c>3 ? 4 ? 2 = 11</c> — pick the pair of operators, remembering × comes before + and −.</summary>
public sealed class OrderOfOperationsSkill : ISkill
{
    private const int ChoiceCount = 4;

    private static readonly Operation[] Operators = [Operation.Add, Operation.Subtract, Operation.Multiply];

    public string Id => "order-of-operations";

    public string Name => "Order of operations";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        var max = difficulty switch { Difficulty.Easy => 6, Difficulty.Medium => 10, _ => 12 };
        // No 1s: "× 1" makes several choices equal and the puzzle trivial.
        int Number() => random.Next(2, max + 1);

        var (a, b, c) = (Number(), Number(), Number());

        // Easy always mixes × with + or −, so the order actually matters.
        Operation op1, op2;
        do
        {
            (op1, op2) = (random.Pick(Operators), random.Pick(Operators));
        }
        while (difficulty == Difficulty.Easy && Arithmetic.IsMultiplicative(op1) == Arithmetic.IsMultiplicative(op2));

        var result = Arithmetic.Evaluate(a, op1, b, op2, c)!.Value;

        var pairs = (from x in Operators from y in Operators select (X: x, Y: y)).ToList();
        List<(Operation X, Operation Y)> shown = pairs
            .Where(p => p != (op1, op2))
            .OrderBy(_ => random.Next())
            .Take(ChoiceCount - 1)
            .Append((op1, op2))
            .OrderBy(_ => random.Next())
            .ToList();

        string Expression((Operation X, Operation Y) p) => $"{a} {p.X.Symbol()} {b} {p.Y.Symbol()} {c}";

        // "2 + 2 × 5 = 12   (× first: 2 × 5 = 10)" — spell out the step people get wrong.
        string Worked((Operation X, Operation Y) p)
        {
            var text = $"{Expression(p)} = {Format.Number(Arithmetic.Evaluate(a, p.X, b, p.Y, c)!.Value)}";
            return Arithmetic.IsMultiplicative(p.Y) && !Arithmetic.IsMultiplicative(p.X)
                ? $"{text}   (× first: {b} × {c} = {b * c})"
                : text;
        }

        var choices = shown
            .Select(p => new Choice(Expression(p), Arithmetic.Evaluate(a, p.X, b, p.Y, c) == result, Note: Worked(p)))
            .ToList();

        var explanation = string.Join(
            "  and  ",
            shown.Where(p => Arithmetic.Evaluate(a, p.X, b, p.Y, c) == result).Select(Worked));

        return new ChoiceChallenge(Id, $"{a} ? {b} ? {c} = {Format.Number(result)}", explanation, choices);
    }
}
