using MathGame.Core.Challenges;

namespace MathGame.Core.Skills;

/// <summary>Is 91 prime? — a quick yes/no.</summary>
public sealed class PrimeOrNotSkill : ISkill
{
    public string Id => "prime-or-not";

    public string Name => "Prime or not";

    public Challenge Generate(Random random, Difficulty difficulty)
    {
        var (min, max) = difficulty switch
        {
            Difficulty.Easy => (2, 30),
            Difficulty.Medium => (10, 100),
            _ => (50, 200),
        };

        // Half primes, half not. Above Easy, non-primes are mostly odd so they can't be spotted at a glance.
        var wantPrime = random.Next(2) == 0;
        var preferOdd = difficulty != Difficulty.Easy && random.Next(10) < 7;
        var candidates = Enumerable.Range(min, max - min + 1)
            .Where(n => Primes.IsPrime(n) == wantPrime)
            .Where(n => wantPrime || !preferOdd || n % 2 == 1)
            .ToList();
        var number = random.Pick(candidates);

        var isPrime = Primes.IsPrime(number);
        var factor = Primes.SmallestFactor(number);
        var explanation = isPrime
            ? $"{number} is prime: only 1 and {number} divide it"
            : $"{number} = {factor} × {number / factor}, so it isn't prime";

        return new ChoiceChallenge(
            Id,
            $"Is {number} prime?",
            explanation,
            [new Choice("Prime", isPrime, "y"), new Choice("Not prime", !isPrime, "n")]);
    }
}
