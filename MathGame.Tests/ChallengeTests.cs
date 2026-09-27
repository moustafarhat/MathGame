using MathGame.Core.Challenges;

namespace MathGame.Tests;

public class ChallengeTests
{
    private static readonly ChoiceChallenge TwoPlusTwo = new(
        "test",
        "2 ? 2 = 4",
        "",
        [new Choice("+", true, "p+"), new Choice("−", false, "m-"), new Choice("×", true, "x*"), new Choice("÷", false, "d/")]);

    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(-1, false)]
    [InlineData(4, false)]
    public void Choice_challenge_accepts_every_correct_choice(int index, bool expected) =>
        Assert.Equal(expected, TwoPlusTwo.Accepts(new ChoiceGuess(index)));

    [Fact]
    public void Choice_challenge_rejects_number_guesses() =>
        Assert.False(TwoPlusTwo.Accepts(new NumberGuess(0)));

    [Theory]
    [InlineData('1', 0)]
    [InlineData('4', 3)]
    [InlineData('P', 0)]
    [InlineData('*', 2)]
    [InlineData('d', 3)]
    public void ChoiceForKey_maps_numbers_and_shortcuts(char key, int expected) =>
        Assert.Equal(expected, TwoPlusTwo.ChoiceForKey(key));

    [Theory]
    [InlineData('5')]
    [InlineData('0')]
    [InlineData('q')]
    public void ChoiceForKey_ignores_other_keys(char key) =>
        Assert.Null(TwoPlusTwo.ChoiceForKey(key));

    [Fact]
    public void Number_challenge_checks_the_value()
    {
        var challenge = new NumberChallenge("test", "−7 + 12 = ?", "", 5);

        Assert.True(challenge.Accepts(new NumberGuess(5)));
        Assert.False(challenge.Accepts(new NumberGuess(-5)));
        Assert.False(challenge.Accepts(new ChoiceGuess(5)));
    }

    [Theory]
    [InlineData("12", 12)]
    [InlineData("  12 ", 12)]
    [InlineData("-5", -5)]
    [InlineData("−5", -5)]
    [InlineData("+3", 3)]
    public void TryParseAnswer_accepts_both_minus_signs(string text, int expected)
    {
        Assert.True(NumberChallenge.TryParseAnswer(text, out var value));
        Assert.Equal(expected, value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData(null)]
    public void TryParseAnswer_rejects_non_integers(string? text) =>
        Assert.False(NumberChallenge.TryParseAnswer(text, out _));

    [Theory]
    [InlineData(5, "5", "5")]
    [InlineData(-4, "−4", "(−4)")]
    [InlineData(0, "0", "0")]
    public void Format_uses_unicode_minus_and_brackets(int value, string number, string operand)
    {
        Assert.Equal(number, Format.Number(value));
        Assert.Equal(operand, Format.Operand(value));
    }
}
