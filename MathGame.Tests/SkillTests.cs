using System.Text.RegularExpressions;
using MathGame.Core.Challenges;
using MathGame.Core.Skills;

namespace MathGame.Tests;

public class SkillTests
{
    private const int Samples = 2_000;

    public static TheoryData<string, Difficulty> AllSkillsAndDifficulties()
    {
        var data = new TheoryData<string, Difficulty>();
        foreach (var skill in SkillRegistry.Default.All)
        {
            foreach (var difficulty in Enum.GetValues<Difficulty>())
            {
                data.Add(skill.Id, difficulty);
            }
        }

        return data;
    }

    private static List<Challenge> Generate(string skillId, Difficulty difficulty, int seed = 42)
    {
        var skill = SkillRegistry.Default.Get(skillId);
        var random = new Random(seed);
        return Enumerable.Range(0, Samples).Select(_ => skill.Generate(random, difficulty)).ToList();
    }

    private static List<T> Generate<T>(string skillId, Difficulty difficulty) where T : Challenge =>
        Generate(skillId, difficulty).Cast<T>().ToList();

    [Theory]
    [MemberData(nameof(AllSkillsAndDifficulties))]
    public void Every_challenge_is_well_formed(string skillId, Difficulty difficulty)
    {
        Assert.All(Generate(skillId, difficulty), challenge =>
        {
            Assert.Equal(skillId, challenge.SkillId);
            Assert.False(string.IsNullOrWhiteSpace(challenge.Prompt));
            Assert.False(string.IsNullOrWhiteSpace(challenge.Explanation));
            Assert.True(challenge.Accepts(TestHelpers.RightGuess(challenge)));

            if (challenge is ChoiceChallenge choice)
            {
                Assert.InRange(choice.Choices.Count, 2, 9);
                Assert.Contains(choice.Choices, c => c.IsCorrect);
                Assert.Contains(choice.Choices, c => !c.IsCorrect);
                Assert.Equal(choice.Choices.Count, choice.Choices.Select(c => c.Label).Distinct().Count());
            }
        });
    }

    [Theory]
    [MemberData(nameof(AllSkillsAndDifficulties))]
    public void Same_seed_gives_same_challenges(string skillId, Difficulty difficulty) =>
        Assert.Equal(
            Generate(skillId, difficulty, seed: 7).Select(c => c.Prompt),
            Generate(skillId, difficulty, seed: 7).Select(c => c.Prompt));

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Hard)]
    public void Guess_operator_marks_exactly_the_operators_that_work(Difficulty difficulty)
    {
        Assert.All(Generate<ChoiceChallenge>("guess-operator", difficulty), challenge =>
        {
            foreach (var choice in challenge.Choices)
            {
                var equation = challenge.Prompt.Replace("?", choice.Label);
                // Division must be exact to count; the independent evaluator would accept 7 ÷ 2 = 3.5.
                var exact = choice.Label != "÷" || IsExactDivision(challenge.Prompt);
                Assert.Equal(choice.IsCorrect, TestHelpers.Holds(equation) && exact);
            }
        });
    }

    [Fact]
    public void Guess_operator_accepts_both_answers_when_ambiguous()
    {
        var ambiguous = Generate<ChoiceChallenge>("guess-operator", Difficulty.Easy)
            .Where(c => c.Choices.Count(x => x.IsCorrect) > 1)
            .ToList();

        Assert.NotEmpty(ambiguous);
        Assert.All(ambiguous, c => Assert.Contains("and", c.Explanation));
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void Missing_number_answer_completes_the_equation(Difficulty difficulty)
    {
        Assert.All(Generate<NumberChallenge>("missing-number", difficulty), challenge =>
        {
            Assert.Single(challenge.Prompt, '?');
            Assert.True(TestHelpers.Holds(challenge.Prompt.Replace("?", challenge.Answer.ToString())), challenge.Prompt);
            Assert.True(TestHelpers.Holds(challenge.Explanation));
        });
    }

    [Fact]
    public void Missing_number_easy_uses_only_add_and_subtract() =>
        Assert.All(
            Generate<NumberChallenge>("missing-number", Difficulty.Easy),
            c => Assert.DoesNotMatch("[×÷]", c.Prompt));

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Hard)]
    public void Order_of_operations_follows_precedence(Difficulty difficulty)
    {
        Assert.All(Generate<ChoiceChallenge>("order-of-operations", difficulty), challenge =>
        {
            var target = TestHelpers.Evaluate(challenge.Prompt.Split('=')[1]);
            foreach (var choice in challenge.Choices)
            {
                Assert.Equal(choice.IsCorrect, TestHelpers.Evaluate(choice.Label) == target);
            }
        });
    }

    [Fact]
    public void Order_of_operations_easy_always_mixes_multiply_with_add_or_subtract()
    {
        Assert.All(Generate<ChoiceChallenge>("order-of-operations", Difficulty.Easy), challenge =>
            Assert.Contains(challenge.Choices, c => c.IsCorrect && c.Label.Count(ch => ch == '×') == 1));
    }

    [Theory]
    [InlineData("guess-operator")]
    [InlineData("order-of-operations")]
    public void Choice_notes_show_what_each_choice_really_equals(string skillId)
    {
        Assert.All(Generate<ChoiceChallenge>(skillId, Difficulty.Hard), challenge =>
        {
            foreach (var choice in challenge.Choices.Where(c => !c.Note.Contains("whole number")))
            {
                // Drop the "(× first: ...)" hint, then the note must be a true equation.
                Assert.True(TestHelpers.Holds(choice.Note.Split("   (")[0]), choice.Note);
            }
        });
    }

    [Fact]
    public void Order_of_operations_explains_every_correct_choice()
    {
        Assert.All(Generate<ChoiceChallenge>("order-of-operations", Difficulty.Hard), challenge =>
        {
            foreach (var choice in challenge.Choices.Where(c => c.IsCorrect))
            {
                Assert.Contains(choice.Label, challenge.Explanation);
            }
        });
    }

    [Fact]
    public void Prime_factors_notes_explain_each_wrong_choice()
    {
        Assert.All(Generate<ChoiceChallenge>("prime-factors", Difficulty.Hard), challenge =>
            Assert.All(challenge.Choices.Where(c => !c.IsCorrect), c =>
                Assert.Matches(@"(, not \d+$)|(isn't prime)", c.Note)));
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void Prime_or_not_is_correct(Difficulty difficulty)
    {
        var challenges = Generate<ChoiceChallenge>("prime-or-not", difficulty);

        Assert.All(challenges, challenge =>
        {
            var number = int.Parse(challenge.Prompt.Split(' ')[1]);
            Assert.Equal(IsPrimeByTrialDivision(number), challenge.Choices.Single(c => c.Label == "Prime").IsCorrect);
        });

        // Roughly balanced, so guessing one answer doesn't win.
        var primeShare = challenges.Count(c => c.Choices[0].IsCorrect) / (double)challenges.Count;
        Assert.InRange(primeShare, 0.4, 0.6);
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void Prime_factors_has_one_true_factorisation(Difficulty difficulty)
    {
        Assert.All(Generate<ChoiceChallenge>("prime-factors", difficulty), challenge =>
        {
            var number = int.Parse(challenge.Prompt.Split(' ')[^1].TrimEnd('?'));
            Assert.Equal(4, challenge.Choices.Count);

            foreach (var choice in challenge.Choices)
            {
                var factors = choice.Label.Split(" × ").Select(int.Parse).ToList();
                var isFactorisation = factors.All(IsPrimeByTrialDivision)
                    && factors.Aggregate(1, (a, b) => a * b) == number;
                Assert.Equal(choice.IsCorrect, isFactorisation);
            }
        });
    }

    [Theory]
    [InlineData(Difficulty.Easy)]
    [InlineData(Difficulty.Medium)]
    [InlineData(Difficulty.Hard)]
    public void Negative_numbers_answer_is_right_and_involves_a_negative(Difficulty difficulty)
    {
        Assert.All(Generate<NumberChallenge>("negative-numbers", difficulty), challenge =>
        {
            Assert.True(TestHelpers.Holds(challenge.Prompt.Replace("?", Format.Operand(challenge.Answer))), challenge.Prompt);
            Assert.True(SignedNumber.IsMatch(challenge.Prompt) || challenge.Answer < 0, challenge.Prompt);
        });
    }

    // A minus used as a sign (at the start or right after a bracket), not as the subtraction operator.
    private static readonly Regex SignedNumber = new(@"(^|\()−\d");

    private static bool IsExactDivision(string prompt)
    {
        var parts = prompt.Split(' ');
        return int.Parse(parts[0]) % int.Parse(parts[2]) == 0;
    }

    private static bool IsPrimeByTrialDivision(int n) =>
        n >= 2 && Enumerable.Range(2, n - 2).All(d => n % d != 0);
}
