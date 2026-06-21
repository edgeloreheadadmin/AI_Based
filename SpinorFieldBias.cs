using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Spinor Field Bias
/// Bias towards creation of spinor fields with spin and directional components
/// Preferentially generates and evolves 4-component spinor structures
/// </summary>
public class SpinorFieldBias
{
    private struct ComplexSpinor
    {
        public double[] Real;
        public double[] Imaginary;

        public ComplexSpinor()
        {
            Real = new double[4];
            Imaginary = new double[4];
        }

        public double Norm()
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
            {
                sum += Real[i] * Real[i] + Imaginary[i] * Imaginary[i];
            }
            return Math.Sqrt(sum);
        }

        public double Chirality()
        {
            return (Real[0] + Real[1]) - (Real[2] + Real[3]);
        }

        public double Helicity()
        {
            double realPart = Real[0] * Real[3] - Real[1] * Real[2];
            double imaginaryPart = Imaginary[0] * Imaginary[3] - Imaginary[1] * Imaginary[2];
            return Math.Sqrt(realPart * realPart + imaginaryPart * imaginaryPart);
        }
    }

    private List<ComplexSpinor> spinorFields;
    private int fieldCount;
    private Random random;

    public SpinorFieldBias(int fieldCount = 15)
    {
        this.fieldCount = fieldCount;
        this.spinorFields = new List<ComplexSpinor>();
        this.random = new Random();

        InitializeSpinorFields();
    }

    private void InitializeSpinorFields()
    {
        for (int i = 0; i < fieldCount; i++)
        {
            ComplexSpinor spinor = new ComplexSpinor();

            double phase = random.NextDouble() * 2 * Math.PI;
            for (int j = 0; j < 4; j++)
            {
                double mag = 0.5;
                spinor.Real[j] = mag * Math.Cos(phase + j * Math.PI / 2);
                spinor.Imaginary[j] = mag * Math.Sin(phase + j * Math.PI / 2);
            }

            NormalizeSpinor(ref spinor);
            spinorFields.Add(spinor);
        }
    }

    private void NormalizeSpinor(ref ComplexSpinor spinor)
    {
        double norm = spinor.Norm();
        if (norm > 0)
        {
            for (int i = 0; i < 4; i++)
            {
                spinor.Real[i] /= norm;
                spinor.Imaginary[i] /= norm;
            }
        }
    }

    public void GenerateSpinorFieldsPreferentially(int count)
    {
        Console.WriteLine($"\nGenerating {count} spinor fields preferentially...\n");

        for (int i = 0; i < count; i++)
        {
            ComplexSpinor spinor = new ComplexSpinor();

            for (int j = 0; j < 4; j++)
            {
                spinor.Real[j] = (random.NextDouble() - 0.5) * 2.0;
                spinor.Imaginary[j] = (random.NextDouble() - 0.5) * 2.0;
            }

            NormalizeSpinor(ref spinor);
            spinorFields.Add(spinor);
        }

        Console.WriteLine($"Created {count} additional spinor fields. Total: {spinorFields.Count}");
    }

    public double MeasureSpinorCharacter()
    {
        if (spinorFields.Count == 0)
            return 0.0;

        double totalSpinorScore = 0.0;

        foreach (var spinor in spinorFields)
        {
            double norm = spinor.Norm();
            double chirality = Math.Abs(spinor.Chirality());
            double helicity = spinor.Helicity();

            double spinorScore = (norm + chirality + helicity) / 3.0;
            totalSpinorScore += spinorScore;
        }

        return totalSpinorScore / spinorFields.Count;
    }

    public void EnforceSpinorStructure()
    {
        Console.WriteLine("\nEnforcing spinor structure on all fields...\n");

        for (int idx = 0; idx < spinorFields.Count; idx++)
        {
            ComplexSpinor spinor = spinorFields[idx];
            NormalizeSpinor(ref spinor);
            spinorFields[idx] = spinor;
        }

        Console.WriteLine("Spinor structure enforcement complete.");
    }

    public void ApplyDiracDynamics(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nApplying Dirac dynamics to spinors for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int idx = 0; idx < spinorFields.Count; idx++)
            {
                ComplexSpinor spinor = spinorFields[idx];

                for (int i = 0; i < 4; i++)
                {
                    double phase = couplingStrength * step * Math.PI / steps;

                    double oldReal = spinor.Real[i];
                    double oldImag = spinor.Imaginary[i];

                    spinor.Real[i] = oldReal * Math.Cos(phase) - oldImag * Math.Sin(phase);
                    spinor.Imaginary[i] = oldReal * Math.Sin(phase) + oldImag * Math.Cos(phase);
                }

                NormalizeSpinor(ref spinor);
                spinorFields[idx] = spinor;
            }

            if (step % (steps / 3) == 0)
            {
                double spinorCharacter = MeasureSpinorCharacter();
                Console.WriteLine($"Step {step}: Spinor Character Score = {spinorCharacter:F6}");
            }
        }
    }

    public double CalculateChiralityDensity()
    {
        double totalChirality = 0.0;

        foreach (var spinor in spinorFields)
        {
            totalChirality += Math.Abs(spinor.Chirality());
        }

        return spinorFields.Count > 0 ? totalChirality / spinorFields.Count : 0.0;
    }

    public Dictionary<string, object> GetSpinorBiasMetrics()
    {
        double spinorCharacter = MeasureSpinorCharacter();
        double chiralityDensity = CalculateChiralityDensity();

        double avgHelicity = 0.0;
        foreach (var spinor in spinorFields)
        {
            avgHelicity += spinor.Helicity();
        }
        avgHelicity /= spinorFields.Count;

        return new Dictionary<string, object>
        {
            { "SpinorFieldCount", spinorFields.Count },
            { "SpinorCharacter", spinorCharacter },
            { "ChiralityDensity", chiralityDensity },
            { "AverageHelicity", avgHelicity },
            { "ComponentsPerSpinor", 4 }
        };
    }

    public void PrintSpinorSummary(int limit = 5)
    {
        Console.WriteLine("\nSpinor Field Summary:");
        for (int i = 0; i < Math.Min(limit, spinorFields.Count); i++)
        {
            var spinor = spinorFields[i];
            Console.WriteLine($"  Spinor {i}: Norm={spinor.Norm():F4}, Chirality={spinor.Chirality():F4}, Helicity={spinor.Helicity():F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Spinor Field Bias - Preferential Spinor Creation ===\n");

        var bias = new SpinorFieldBias(fieldCount: 15);

        Console.WriteLine("--- Initial Spinor Fields ---");
        bias.PrintSpinorSummary(5);

        Console.WriteLine("\n--- Initial Spinor Bias Metrics ---");
        var metricsInitial = bias.GetSpinorBiasMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Generating Additional Spinor Fields ---");
        bias.GenerateSpinorFieldsPreferentially(count: 10);

        Console.WriteLine("\n--- Enforcing Spinor Structure ---");
        bias.EnforceSpinorStructure();

        Console.WriteLine("\n--- Applying Dirac Dynamics ---");
        bias.ApplyDiracDynamics(steps: 20, couplingStrength: 0.5);

        bias.PrintSpinorSummary(8);

        Console.WriteLine("\n--- Final Spinor Bias Metrics ---");
        var metricsFinal = bias.GetSpinorBiasMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSpinor Field Bias preferentially creates fields with spin and directional components.");
    }
}
