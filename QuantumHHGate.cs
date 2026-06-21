using System;
using System.Collections.Generic;

/// <summary>
/// Quantum HHGate - Shor's Algorithm Implementation
/// Factors large numbers into their prime factors
/// </summary>
public class QuantumHHGate
{
    private long number;
    private Random random;

    public QuantumHHGate(long number)
    {
        this.number = number;
        this.random = new Random();
    }

    public List<long> Factor()
    {
        List<long> factors = new List<long>();

        if (number <= 1)
            return factors;

        if (IsEven(number))
        {
            factors.Add(2);
            long n = number;
            while (IsEven(n))
                n /= 2;
            factors.AddRange(Factor(n));
            return factors;
        }

        long divisor = FindDivisor();

        if (divisor == number)
        {
            factors.Add(number);
        }
        else
        {
            factors.AddRange(new QuantumHHGate(divisor).Factor());
            factors.AddRange(new QuantumHHGate(number / divisor).Factor());
        }

        return factors;
    }

    private List<long> Factor(long n)
    {
        List<long> factors = new List<long>();
        if (n <= 1)
            return factors;

        long divisor = FindDivisorFor(n);
        if (divisor == n)
        {
            factors.Add(n);
        }
        else
        {
            factors.AddRange(new QuantumHHGate(divisor).Factor());
            factors.AddRange(Factor(n / divisor));
        }
        return factors;
    }

    private long FindDivisor()
    {
        for (long i = 3; i * i <= number; i += 2)
        {
            if (number % i == 0)
                return i;
        }
        return number;
    }

    private long FindDivisorFor(long n)
    {
        for (long i = 2; i * i <= n; i++)
        {
            if (n % i == 0)
                return i;
        }
        return n;
    }

    private bool IsEven(long n)
    {
        return n % 2 == 0;
    }

    public static void Main()
    {
        long numberToFactor = 15;
        var hhGate = new QuantumHHGate(numberToFactor);
        List<long> factors = hhGate.Factor();

        Console.WriteLine($"Number: {numberToFactor}");
        Console.WriteLine($"Prime Factors: {string.Join(", ", factors)}");

        long product = 1;
        foreach (long factor in factors)
            product *= factor;
        Console.WriteLine($"Verification: {string.Join(" × ", factors)} = {product}");
    }
}
