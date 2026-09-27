using System.Globalization;

namespace MathGame.Core.Challenges;

/// <summary>Formats numbers the way the game displays them.</summary>
public static class Format
{
    /// <summary>Uses the Unicode minus sign so negatives line up with the other operators.</summary>
    public static string Number(int value) =>
        value < 0
            ? "−" + (-(long)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString(CultureInfo.InvariantCulture);

    /// <summary>A number placed after an operator: negatives get brackets, as in <c>3 − (−4)</c>.</summary>
    public static string Operand(int value) => value < 0 ? $"({Number(value)})" : Number(value);

    public static string Product(IEnumerable<int> factors) => string.Join(" × ", factors.Select(Number));
}
