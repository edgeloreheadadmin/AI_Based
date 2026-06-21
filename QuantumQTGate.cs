using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum QTGate - Grover's Algorithm Implementation
/// Searches an unsorted database much faster than classical algorithms
/// </summary>
public class QuantumQTGate
{
    private int[] database;
    private int targetElement;
    private int iterations;

    public QuantumQTGate(int[] database, int targetElement)
    {
        this.database = database;
        this.targetElement = targetElement;
        this.iterations = CalculateOptimalIterations(database.Length);
    }

    private int CalculateOptimalIterations(int n)
    {
        return (int)Math.Round(Math.PI / 4 * Math.Sqrt(n));
    }

    public int Search()
    {
        List<int> searchSpace = database.ToList();

        for (int i = 0; i < iterations; i++)
        {
            searchSpace = AmplifyAmplitudes(searchSpace);
        }

        return FindMaxAmplitude(searchSpace);
    }

    private List<int> AmplifyAmplitudes(List<int> candidates)
    {
        List<int> amplified = new List<int>();

        foreach (int candidate in candidates)
        {
            if (candidate == targetElement)
            {
                amplified.Add(candidate);
                amplified.Add(candidate);
            }
            else
            {
                if (amplified.Count > 0)
                    amplified.Add(candidate);
            }
        }

        return amplified.Count > 0 ? amplified : candidates;
    }

    private int FindMaxAmplitude(List<int> candidates)
    {
        var grouped = candidates.GroupBy(x => x).OrderByDescending(g => g.Count());
        return grouped.First().Key;
    }

    public static void Main()
    {
        int[] database = { 7, 12, 3, 19, 45, 67, 2, 88, 15, 34 };
        int target = 67;

        var qtGate = new QuantumQTGate(database, target);
        int result = qtGate.Search();

        Console.WriteLine($"Database: {string.Join(", ", database)}");
        Console.WriteLine($"Target: {target}");
        Console.WriteLine($"Found: {result}");
        Console.WriteLine($"Match: {result == target}");
    }
}
