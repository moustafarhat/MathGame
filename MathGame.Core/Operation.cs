namespace MathGame.Core;

public enum Operation
{
    Add,
    Subtract,
    Multiply,
    Divide,
}

public static class OperationExtensions
{
    public static char Symbol(this Operation operation) => operation switch
    {
        Operation.Add => '+',
        Operation.Subtract => '−',
        Operation.Multiply => '×',
        Operation.Divide => '÷',
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

    /// <summary>
    /// Keyboard shortcuts for the operation: the original P/M/X/D letters plus the symbol key.
    /// </summary>
    public static string Keys(this Operation operation) => operation switch
    {
        Operation.Add => "p+",
        Operation.Subtract => "m-",
        Operation.Multiply => "x*",
        Operation.Divide => "d/",
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

    /// <summary>
    /// Applies the operation. Division only yields a value when it is exact,
    /// so the game never shows truncated results like 39 ÷ 5 = 7.
    /// </summary>
    public static int? Apply(this Operation operation, int left, int right) => operation switch
    {
        Operation.Add => left + right,
        Operation.Subtract => left - right,
        Operation.Multiply => left * right,
        Operation.Divide => right != 0 && left % right == 0 ? left / right : null,
        _ => throw new ArgumentOutOfRangeException(nameof(operation)),
    };

    public static bool TryFromKey(char key, out Operation operation)
    {
        var lower = char.ToLowerInvariant(key);
        foreach (var candidate in Enum.GetValues<Operation>())
        {
            if (candidate.Keys().Contains(lower))
            {
                operation = candidate;
                return true;
            }
        }

        operation = default;
        return false;
    }
}
