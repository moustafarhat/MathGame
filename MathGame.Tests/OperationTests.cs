using MathGame.Core;

namespace MathGame.Tests;

public class OperationTests
{
    [Theory]
    [InlineData(Operation.Add, 12, 5, 17)]
    [InlineData(Operation.Subtract, 12, 5, 7)]
    [InlineData(Operation.Multiply, 12, 5, 60)]
    [InlineData(Operation.Divide, 60, 5, 12)]
    public void Apply_computes_result(Operation operation, int left, int right, int expected) =>
        Assert.Equal(expected, operation.Apply(left, right));

    [Theory]
    [InlineData(39, 5)]  // the old game showed 7 here
    [InlineData(39, 49)] // ...and 0 here
    [InlineData(5, 0)]
    public void Divide_is_undefined_unless_exact(int left, int right) =>
        Assert.Null(Operation.Divide.Apply(left, right));

    [Theory]
    [InlineData('p', Operation.Add)]
    [InlineData('P', Operation.Add)]
    [InlineData('+', Operation.Add)]
    [InlineData('m', Operation.Subtract)]
    [InlineData('M', Operation.Subtract)]
    [InlineData('-', Operation.Subtract)]
    [InlineData('x', Operation.Multiply)]
    [InlineData('X', Operation.Multiply)]
    [InlineData('*', Operation.Multiply)]
    [InlineData('d', Operation.Divide)]
    [InlineData('D', Operation.Divide)]
    [InlineData('/', Operation.Divide)]
    public void TryFromKey_maps_letters_and_symbols(char key, Operation expected)
    {
        Assert.True(OperationExtensions.TryFromKey(key, out var operation));
        Assert.Equal(expected, operation);
    }

    [Theory]
    [InlineData('a')]
    [InlineData(' ')]
    [InlineData('1')]
    public void TryFromKey_rejects_other_keys(char key) =>
        Assert.False(OperationExtensions.TryFromKey(key, out _));
}
