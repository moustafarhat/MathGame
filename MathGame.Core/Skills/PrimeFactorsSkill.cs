using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary>Which is the prime factorisation of 60? — spot 2 × 2 × 3 × 5 among near misses.</summary>
public sealed class PrimeFactorsSkill : ISkill
{
    private const int DistractorCount = 3;

    public string Id => "prime-factors";

    public string Name => "Prime factors";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        var (pool, minCount, maxCount, maxValue) = difficulty switch
        {
            Difficulty.Easy => (new[] { 2, 3, 5 }, 2, 3, 100),
            Difficulty.Medium => (new[] { 2, 3, 5, 7 }, 3, 4, 500),
            _ => (new[] { 2, 3, 5, 7, 11, 13 }, 3, 5, 2000),
        };

        List<int> factors;
        do
        {
            factors = Enumerable.Range(0, random.Next(minCount, maxCount + 1))
                .Select(_ => random.Pick(pool))
                .Order()
                .ToList();
        }
        while (Product(factors) > maxValue);

        var number = Product(factors);
        var correct = Format.Product(factors);

        var distractors = new HashSet<string>();
        for (var attempt = 0; distractors.Count < DistractorCount && attempt < 100; attempt++)
        {
            var label = Format.Product(NearMiss(random, factors, pool).Order());
            if (label != correct)
            {
                distractors.Add(label);
            }
        }

        var choices = distractors
            .Select(label => new Choice(label, false))
            .Append(new Choice(correct, true))
            .OrderBy(_ => random.Next())
            .ToList();

        return new ChoiceChallenge(
            Id,
            $"Which is the prime factorisation of {number}?",
            $"{number} = {correct}, and every factor is prime",
            choices);
    }

    /// <summary>
    /// A plausible wrong answer: two factors merged (right value, but not all prime),
    /// one prime swapped for another (wrong value), or one factor added or dropped (wrong value).
    /// </summary>
    private static List<int> NearMiss(Random random, List<int> factors, int[] pool)
    {
        var copy = factors.ToList();
        switch (random.Next(3))
        {
            case 0:
                var i = random.Next(copy.Count - 1);
                copy[i] *= copy[i + 1];
                copy.RemoveAt(i + 1);
                break;
            case 1:
                var j = random.Next(copy.Count);
                copy[j] = random.Pick(pool.Where(p => p != copy[j]).ToList());
                break;
            default:
                if (copy.Count > 2 && random.Next(2) == 0)
                {
                    copy.RemoveAt(random.Next(copy.Count));
                }
                else
                {
                    copy.Add(random.Pick(pool));
                }

                break;
        }

        return copy;
    }

    private static int Product(IEnumerable<int> factors) => factors.Aggregate(1, (a, b) => a * b);
}
