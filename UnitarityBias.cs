using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unitarity Bias
/// Bias towards creating unitary fields that preserve probability and norm
/// Fields maintain hermiticity and conservation laws through evolution
/// </summary>
public class UnitarityBias
{
    private struct ComplexAmplitude
    {
        public double Real;
        public double Imaginary;

        public double Magnitude()
        {
            return Math.Sqrt(Real * Real + Imaginary * Imaginary);
        }

        public double Probability()
        {
            return Magnitude() * Magnitude();
        }

        public void Normalize()
        {
            double mag = Magnitude();
            if (mag > 0)
            {
                Real /= mag;
                Imaginary /= mag;
            }
        }
    }

    private ComplexAmplitude[][] fieldAmplitudes;
    private int fieldDimension;
    private int stateCount;
    private Random random;

    public UnitarityBias(int fieldDimension = 4, int stateCount = 20)
    {
        this.fieldDimension = fieldDimension;
        this.stateCount = stateCount;
        this.fieldAmplitudes = new ComplexAmplitude[stateCount][];
        this.random = new Random();

        InitializeField();
    }

    private void InitializeField()
    {
        for (int i = 0; i < stateCount; i++)
        {
            fieldAmplitudes[i] = new ComplexAmplitude[fieldDimension];
            for (int j = 0; j < fieldDimension; j++)
            {
                fieldAmplitudes[i][j] = new ComplexAmplitude
                {
                    Real = (random.NextDouble() - 0.5),
                    Imaginary = (random.NextDouble() - 0.5)
                };
            }

            NormalizeState(i);
        }
    }

    public void NormalizeState(int stateIndex)
    {
        double norm = 0.0;

        for (int j = 0; j < fieldDimension; j++)
        {
            norm += fieldAmplitudes[stateIndex][j].Probability();
        }

        norm = Math.Sqrt(norm);

        if (norm > 0)
        {
            for (int j = 0; j < fieldDimension; j++)
            {
                fieldAmplitudes[stateIndex][j].Real /= norm;
                fieldAmplitudes[stateIndex][j].Imaginary /= norm;
            }
        }
    }

    public double CalculateTotalProbability()
    {
        double totalProbability = 0.0;

        for (int i = 0; i < stateCount; i++)
        {
            for (int j = 0; j < fieldDimension; j++)
            {
                totalProbability += fieldAmplitudes[i][j].Probability();
            }
        }

        return totalProbability;
    }

    public double CalculateHermitianExpectation(int stateIndex)
    {
        double expectation = 0.0;

        for (int j = 0; j < fieldDimension; j++)
        {
            expectation += j * fieldAmplitudes[stateIndex][j].Probability();
        }

        return expectation;
    }

    public double MeasureUnitarity()
    {
        double totalProbability = CalculateTotalProbability();
        double unitarityScore = 1.0 / (1.0 + Math.Abs(totalProbability - stateCount));

        return unitarityScore;
    }

    public void ApplyUnitaryEvolution(int steps, double evolutionStrength)
    {
        Console.WriteLine($"\nApplying unitary evolution for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int i = 0; i < stateCount; i++)
            {
                double phase = evolutionStrength * step * Math.PI / steps;

                for (int j = 0; j < fieldDimension; j++)
                {
                    double oldReal = fieldAmplitudes[i][j].Real;
                    double oldImaginary = fieldAmplitudes[i][j].Imaginary;

                    double cosPhase = Math.Cos(phase);
                    double sinPhase = Math.Sin(phase);

                    fieldAmplitudes[i][j].Real = oldReal * cosPhase - oldImaginary * sinPhase;
                    fieldAmplitudes[i][j].Imaginary = oldReal * sinPhase + oldImaginary * cosPhase;
                }

                NormalizeState(i);
            }

            if (step % (steps / 3) == 0)
            {
                double unitarity = MeasureUnitarity();
                double totalProb = CalculateTotalProbability();
                Console.WriteLine($"Step {step}: Unitarity Score = {unitarity:F6}, Total Probability = {totalProb:F6}");
            }
        }
    }

    public void EnforceUnitarity()
    {
        Console.WriteLine("\nEnforcing unitarity on all states...\n");

        for (int i = 0; i < stateCount; i++)
        {
            NormalizeState(i);
        }

        Console.WriteLine("Unitarity enforcement complete.");
    }

    public double[][] CalculateUnitaryMatrix()
    {
        double[][] unitaryMatrix = new double[stateCount][];

        for (int i = 0; i < stateCount; i++)
        {
            unitaryMatrix[i] = new double[stateCount];

            for (int j = 0; j < stateCount; j++)
            {
                double innerProduct = 0.0;

                for (int k = 0; k < fieldDimension; k++)
                {
                    double realProd = fieldAmplitudes[i][k].Real * fieldAmplitudes[j][k].Real +
                                     fieldAmplitudes[i][k].Imaginary * fieldAmplitudes[j][k].Imaginary;
                    double imaginaryProd = fieldAmplitudes[i][k].Imaginary * fieldAmplitudes[j][k].Real -
                                          fieldAmplitudes[i][k].Real * fieldAmplitudes[j][k].Imaginary;

                    innerProduct += realProd * realProd + imaginaryProd * imaginaryProd;
                }

                unitaryMatrix[i][j] = Math.Sqrt(innerProduct);
            }
        }

        return unitaryMatrix;
    }

    public Dictionary<string, object> GetUnitarityMetrics()
    {
        double totalProbability = CalculateTotalProbability();
        double unitarityScore = MeasureUnitarity();
        double avgExpectation = 0.0;

        for (int i = 0; i < stateCount; i++)
        {
            avgExpectation += CalculateHermitianExpectation(i);
        }
        avgExpectation /= stateCount;

        return new Dictionary<string, object>
        {
            { "StateCount", stateCount },
            { "FieldDimension", fieldDimension },
            { "TotalProbability", totalProbability },
            { "UnitarityScore", unitarityScore },
            { "AverageHermitianExpectation", avgExpectation }
        };
    }

    public void PrintProbabilityDistribution()
    {
        Console.WriteLine("\nProbability Distribution across states:");
        for (int i = 0; i < Math.Min(stateCount, 10); i++)
        {
            double prob = 0.0;
            for (int j = 0; j < fieldDimension; j++)
            {
                prob += fieldAmplitudes[i][j].Probability();
            }

            Console.Write($"  State {i}: ");
            int barLength = (int)(prob * 30);
            for (int k = 0; k < barLength; k++)
                Console.Write("█");
            Console.WriteLine($" {prob:F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Unitarity Bias - Probability Conservation ===\n");

        var bias = new UnitarityBias(fieldDimension: 4, stateCount: 20);

        Console.WriteLine("--- Initial Field State ---");
        bias.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Initial Unitarity Metrics ---");
        var metricsInitial = bias.GetUnitarityMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Enforcing Unitarity ---");
        bias.EnforceUnitarity();

        Console.WriteLine("\n--- Applying Unitary Evolution ---");
        bias.ApplyUnitaryEvolution(steps: 20, evolutionStrength: 0.5);

        bias.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Final Unitarity Metrics ---");
        var metricsFinal = bias.GetUnitarityMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nUnitarity Bias ensures fields maintain probability conservation through evolution.");
    }
}
