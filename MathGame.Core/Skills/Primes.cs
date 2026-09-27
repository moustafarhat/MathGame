namespace MathGame.Core.Skills;

public static class Primes
{
    public static bool IsPrime(int n)
    {
        if (n < 2)
        {
            return false;
        }

        for (var d = 2; d * d <= n; d++)
        {
            if (n % d == 0)
            {
                return false;
            }
        }

        return true;
    }

    public static int SmallestFactor(int n)
    {
        for (var d = 2; d * d <= n; d++)
        {
            if (n % d == 0)
            {
                return d;
            }
        }

        return n;
    }
}
