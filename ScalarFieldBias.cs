using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Scalar Field Bias
/// Bias towards creation of scalar fields with no spin or direction components
/// Preferentially generates and evolves single-value scalar structures
/// </summary>
public class ScalarFieldBias
{
    private List<double> scalarFields;
    private int fieldCount;
    private Random random;

    public ScalarFieldBias(int fieldCount = 20)
    {
        this.fieldCount = fieldCount;
        this.scalarFields = new List<double>();
        this.random = new Random();

        InitializeScalarFields();
    }

    private void InitializeScalarFields()
    {
        for (int i = 0; i < fieldCount; i++)
        {
            scalarFields.Add(random.NextDouble());
        }
    }

    public void GenerateScalarFieldsPreferentially(int count)
    {
        Console.WriteLine($"\nGenerating {count} scalar fields preferentially...\n");

        for (int i = 0; i < count; i++)
        {
            scalarFields.Add(random.NextDouble());
        }

        Console.WriteLine($"Created {count} additional scalar fields. Total: {scalarFields.Count}");
    }

    public double MeasureScalarCharacter()
    {
        if (scalarFields.Count == 0)
            return 0.0;

        double average = scalarFields.Average();
        double variance = scalarFields.Sum(x => (x - average) * (x - average)) / scalarFields.Count;

        return 1.0 / (1.0 + variance);
    }

    public void EnforceScalarStructure()
    {
        Console.WriteLine("\nEnforcing scalar structure on all fields...\n");

        for (int i = 0; i < scalarFields.Count; i++)
        {
            if (scalarFields[i] < 0 || scalarFields[i] > 1)
            {
                scalarFields[i] = Math.Abs(scalarFields[i]) % 1.0;
            }
        }

        Console.WriteLine("Scalar structure enforcement complete.");
    }

    public void EvolveWithScalarBias(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving scalar fields with scalar bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            double[] newFields = new double[scalarFields.Count];

            for (int i = 0; i < scalarFields.Count; i++)
            {
                double avgNeighbors = 0.0;
                int neighborCount = 0;

                for (int j = 0; j < scalarFields.Count; j++)
                {
                    if (i != j)
                    {
                        avgNeighbors += scalarFields[j];
                        neighborCount++;
                    }
                }

                if (neighborCount > 0)
                    avgNeighbors /= neighborCount;

                newFields[i] = (1.0 - couplingStrength) * scalarFields[i] + couplingStrength * avgNeighbors;
                newFields[i] = Math.Max(0, Math.Min(1, newFields[i]));
            }

            for (int i = 0; i < scalarFields.Count; i++)
            {
                scalarFields[i] = newFields[i];
            }

            if (step % (steps / 3) == 0)
            {
                double scalarCharacter = MeasureScalarCharacter();
                double avgValue = scalarFields.Average();
                Console.WriteLine($"Step {step}: Scalar Character = {scalarCharacter:F6}, Average Value = {avgValue:F6}");
            }
        }
    }

    public double CalculateSimplicity()
    {
        if (scalarFields.Count == 0)
            return 0.0;

        double simplicityScore = 0.0;

        double min = scalarFields.Min();
        double max = scalarFields.Max();
        double range = max - min;

        if (range > 0)
        {
            for (int i = 0; i < scalarFields.Count; i++)
            {
                double normalized = (scalarFields[i] - min) / range;
                double deviation = Math.Abs(normalized - 0.5);
                simplicityScore += 1.0 - 2.0 * deviation;
            }

            simplicityScore /= scalarFields.Count;
        }

        return Math.Max(0, simplicityScore);
    }

    public double CalculatePotential()
    {
        double potential = 0.0;

        foreach (var phi in scalarFields)
        {
            potential += phi * phi + 0.1 * phi * phi * phi * phi;
        }

        return potential;
    }

    public Dictionary<string, object> GetScalarBiasMetrics()
    {
        double scalarCharacter = MeasureScalarCharacter();
        double simplicity = CalculateSimplicity();
        double potential = CalculatePotential();

        return new Dictionary<string, object>
        {
            { "ScalarFieldCount", scalarFields.Count },
            { "ScalarCharacter", scalarCharacter },
            { "Simplicity", simplicity },
            { "TotalPotential", potential },
            { "AverageValue", scalarFields.Average() },
            { "MaxValue", scalarFields.Max() },
            { "MinValue", scalarFields.Min() },
            { "ComponentsPerField", 1 }
        };
    }

    public void PrintScalarSummary()
    {
        Console.WriteLine("\nScalar Field Distribution:");
        double min = scalarFields.Min();
        double max = scalarFields.Max();
        double range = max - min;

        for (int i = 0; i < Math.Min(scalarFields.Count, 15); i++)
        {
            double normalized = range > 0 ? (scalarFields[i] - min) / range : 0.5;
            int barLength = (int)(normalized * 40);

            Console.Write($"  [{i:D2}]: ");
            for (int k = 0; k < barLength; k++)
                Console.Write("█");
            Console.WriteLine($" {scalarFields[i]:F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Scalar Field Bias - Preferential Scalar Creation ===\n");

        var bias = new ScalarFieldBias(fieldCount: 20);

        Console.WriteLine("--- Initial Scalar Fields ---");
        bias.PrintScalarSummary();

        Console.WriteLine("\n--- Initial Scalar Bias Metrics ---");
        var metricsInitial = bias.GetScalarBiasMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Generating Additional Scalar Fields ---");
        bias.GenerateScalarFieldsPreferentially(count: 15);

        Console.WriteLine("\n--- Enforcing Scalar Structure ---");
        bias.EnforceScalarStructure();

        Console.WriteLine("\n--- Evolving with Scalar Bias ---");
        bias.EvolveWithScalarBias(steps: 25, couplingStrength: 0.3);

        bias.PrintScalarSummary();

        Console.WriteLine("\n--- Final Scalar Bias Metrics ---");
        var metricsFinal = bias.GetScalarBiasMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nScalar Field Bias preferentially creates fields with no spin or directional components.");
    }
}
