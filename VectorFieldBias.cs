using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Vector Field Bias
/// Bias towards creation of vector fields with directional components but no spin
/// Preferentially generates and evolves 3-component vector structures
/// </summary>
public class VectorFieldBias
{
    private struct ThreeDVector
    {
        public double X;
        public double Y;
        public double Z;

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public void Normalize()
        {
            double mag = Magnitude();
            if (mag > 0)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
            }
        }

        public double DotProduct(ThreeDVector other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public ThreeDVector CrossProduct(ThreeDVector other)
        {
            return new ThreeDVector
            {
                X = Y * other.Z - Z * other.Y,
                Y = Z * other.X - X * other.Z,
                Z = X * other.Y - Y * other.X
            };
        }
    }

    private List<ThreeDVector> vectorFields;
    private int fieldCount;
    private Random random;

    public VectorFieldBias(int fieldCount = 15)
    {
        this.fieldCount = fieldCount;
        this.vectorFields = new List<ThreeDVector>();
        this.random = new Random();

        InitializeVectorFields();
    }

    private void InitializeVectorFields()
    {
        for (int i = 0; i < fieldCount; i++)
        {
            ThreeDVector vector = new ThreeDVector
            {
                X = random.NextDouble() - 0.5,
                Y = random.NextDouble() - 0.5,
                Z = random.NextDouble() - 0.5
            };
            vector.Normalize();
            vectorFields.Add(vector);
        }
    }

    public void GenerateVectorFieldsPreferentially(int count)
    {
        Console.WriteLine($"\nGenerating {count} vector fields preferentially...\n");

        for (int i = 0; i < count; i++)
        {
            ThreeDVector vector = new ThreeDVector
            {
                X = (random.NextDouble() - 0.5) * 2.0,
                Y = (random.NextDouble() - 0.5) * 2.0,
                Z = (random.NextDouble() - 0.5) * 2.0
            };
            vector.Normalize();
            vectorFields.Add(vector);
        }

        Console.WriteLine($"Created {count} additional vector fields. Total: {vectorFields.Count}");
    }

    public double MeasureVectorCharacter()
    {
        if (vectorFields.Count == 0)
            return 0.0;

        double totalVectorScore = 0.0;

        foreach (var vector in vectorFields)
        {
            double magnitude = vector.Magnitude();
            totalVectorScore += magnitude;
        }

        return totalVectorScore / vectorFields.Count;
    }

    public void EnforceVectorStructure()
    {
        Console.WriteLine("\nEnforcing vector structure on all fields...\n");

        for (int idx = 0; idx < vectorFields.Count; idx++)
        {
            ThreeDVector vector = vectorFields[idx];
            vector.Normalize();
            vectorFields[idx] = vector;
        }

        Console.WriteLine("Vector structure enforcement complete.");
    }

    public void EvolveWithVectorBias(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving vector fields with vector bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int idx = 0; idx < vectorFields.Count; idx++)
            {
                ThreeDVector vector = vectorFields[idx];

                double angleChange = couplingStrength * step * Math.PI / steps;
                double cosAngle = Math.Cos(angleChange);
                double sinAngle = Math.Sin(angleChange);

                ThreeDVector rotated = new ThreeDVector
                {
                    X = vector.X * cosAngle - vector.Y * sinAngle,
                    Y = vector.X * sinAngle + vector.Y * cosAngle,
                    Z = vector.Z
                };
                rotated.Normalize();

                vectorFields[idx] = rotated;
            }

            if (step % (steps / 3) == 0)
            {
                double vectorCharacter = MeasureVectorCharacter();
                Console.WriteLine($"Step {step}: Vector Character Score = {vectorCharacter:F6}");
            }
        }
    }

    public double CalculateDivergence()
    {
        double divergenceSum = 0.0;

        for (int i = 0; i < vectorFields.Count - 1; i++)
        {
            ThreeDVector diff = new ThreeDVector
            {
                X = vectorFields[i + 1].X - vectorFields[i].X,
                Y = vectorFields[i + 1].Y - vectorFields[i].Y,
                Z = vectorFields[i + 1].Z - vectorFields[i].Z
            };

            divergenceSum += diff.Magnitude();
        }

        return vectorFields.Count > 1 ? divergenceSum / (vectorFields.Count - 1) : 0.0;
    }

    public double CalculateCurl()
    {
        double curlSum = 0.0;

        for (int i = 0; i < vectorFields.Count - 1; i++)
        {
            ThreeDVector curl = vectorFields[i].CrossProduct(vectorFields[i + 1]);
            curlSum += curl.Magnitude();
        }

        return vectorFields.Count > 1 ? curlSum / (vectorFields.Count - 1) : 0.0;
    }

    public Dictionary<string, object> GetVectorBiasMetrics()
    {
        double vectorCharacter = MeasureVectorCharacter();
        double divergence = CalculateDivergence();
        double curl = CalculateCurl();

        double avgMagnitude = 0.0;
        foreach (var vector in vectorFields)
        {
            avgMagnitude += vector.Magnitude();
        }
        avgMagnitude /= vectorFields.Count;

        return new Dictionary<string, object>
        {
            { "VectorFieldCount", vectorFields.Count },
            { "VectorCharacter", vectorCharacter },
            { "Divergence", divergence },
            { "Curl", curl },
            { "AverageMagnitude", avgMagnitude },
            { "ComponentsPerVector", 3 }
        };
    }

    public void PrintVectorSummary(int limit = 5)
    {
        Console.WriteLine("\nVector Field Summary:");
        for (int i = 0; i < Math.Min(limit, vectorFields.Count); i++)
        {
            var vector = vectorFields[i];
            Console.WriteLine($"  Vector {i}: ({vector.X:F3}, {vector.Y:F3}, {vector.Z:F3}) Magnitude={vector.Magnitude():F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Vector Field Bias - Preferential Vector Creation ===\n");

        var bias = new VectorFieldBias(fieldCount: 15);

        Console.WriteLine("--- Initial Vector Fields ---");
        bias.PrintVectorSummary(5);

        Console.WriteLine("\n--- Initial Vector Bias Metrics ---");
        var metricsInitial = bias.GetVectorBiasMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Generating Additional Vector Fields ---");
        bias.GenerateVectorFieldsPreferentially(count: 10);

        Console.WriteLine("\n--- Enforcing Vector Structure ---");
        bias.EnforceVectorStructure();

        Console.WriteLine("\n--- Evolving with Vector Bias ---");
        bias.EvolveWithVectorBias(steps: 20, couplingStrength: 0.4);

        bias.PrintVectorSummary(8);

        Console.WriteLine("\n--- Final Vector Bias Metrics ---");
        var metricsFinal = bias.GetVectorBiasMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nVector Field Bias preferentially creates fields with directional components.");
    }
}
