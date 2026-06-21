using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Scalar Field
/// A field with no spin or direction components
/// Single value at each spacetime point - fundamental representation
/// </summary>
public class ScalarField
{
    private Dictionary<(int, int, int, int), double> fieldValues;
    private int spatialSize;
    private int temporalSize;
    private double[] amplitudes;
    private Random random;

    public ScalarField(int spatialSize = 8, int temporalSize = 10)
    {
        this.spatialSize = spatialSize;
        this.temporalSize = temporalSize;
        this.fieldValues = new Dictionary<(int, int, int, int), double>();
        this.amplitudes = new double[spatialSize * spatialSize * spatialSize * temporalSize];
        this.random = new Random();

        InitializeField();
    }

    private void InitializeField()
    {
        for (int t = 0; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                for (int y = 0; y < spatialSize; y++)
                {
                    for (int z = 0; z < spatialSize; z++)
                    {
                        double value = Math.Sin(x * Math.PI / spatialSize) *
                                      Math.Cos(y * Math.PI / spatialSize) *
                                      Math.Sin(z * Math.PI / spatialSize);
                        fieldValues[(t, x, y, z)] = value;
                    }
                }
            }
        }
    }

    public double GetFieldValue(int t, int x, int y, int z)
    {
        if (fieldValues.ContainsKey((t, x, y, z)))
            return fieldValues[(t, x, y, z)];
        return 0.0;
    }

    public void SetFieldValue(int t, int x, int y, int z, double value)
    {
        fieldValues[(t, x, y, z)] = value;
    }

    public double CalculateFieldEnergy()
    {
        double energy = 0.0;

        foreach (var kvp in fieldValues)
        {
            double phi = kvp.Value;
            energy += Math.Pow(phi, 2);
        }

        return energy;
    }

    public double CalculateFieldGradient(int t, int x, int y, int z)
    {
        double phi = GetFieldValue(t, x, y, z);
        double phi_xp = GetFieldValue(t, x + 1, y, z);
        double phi_xm = GetFieldValue(t, x - 1, y, z);
        double phi_yp = GetFieldValue(t, x, y + 1, z);
        double phi_ym = GetFieldValue(t, x, y - 1, z);
        double phi_zp = GetFieldValue(t, x, y, z + 1);
        double phi_zm = GetFieldValue(t, x, y, z - 1);

        double gradX = (phi_xp - phi_xm) / 2.0;
        double gradY = (phi_yp - phi_ym) / 2.0;
        double gradZ = (phi_zp - phi_zm) / 2.0;

        return Math.Sqrt(gradX * gradX + gradY * gradY + gradZ * gradZ);
    }

    public void EvolveField(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving scalar field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            var newValues = new Dictionary<(int, int, int, int), double>(fieldValues);

            foreach (var kvp in fieldValues)
            {
                var (t, x, y, z) = kvp.Key;
                double phi = kvp.Value;

                double laplacian = CalculateFieldGradient(t, x, y, z);
                double evolution = phi + couplingStrength * laplacian * 0.01;

                newValues[(t, x, y, z)] = evolution;
            }

            fieldValues = newValues;

            if (step % (steps / 3) == 0)
            {
                double energy = CalculateFieldEnergy();
                Console.WriteLine($"Step {step}: Field Energy = {energy:F6}");
            }
        }
    }

    public Dictionary<string, object> GetFieldMetrics()
    {
        double maxValue = fieldValues.Values.Max();
        double minValue = fieldValues.Values.Min();
        double avgValue = fieldValues.Values.Average();
        double energy = CalculateFieldEnergy();

        return new Dictionary<string, object>
        {
            { "GridPoints", fieldValues.Count },
            { "MaxFieldValue", maxValue },
            { "MinFieldValue", minValue },
            { "AverageFieldValue", avgValue },
            { "TotalEnergy", energy }
        };
    }

    public void PrintFieldSlice(int t, int z)
    {
        Console.WriteLine($"\nScalar Field Slice (t={t}, z={z}):");
        for (int y = 0; y < spatialSize; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < spatialSize; x++)
            {
                double value = GetFieldValue(t, x, y, z);
                char symbol = value > 0.5 ? '█' : value > 0 ? '▓' : ' ';
                Console.Write(symbol);
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Scalar Field - No Spin, No Direction ===\n");

        var field = new ScalarField(spatialSize: 8, temporalSize: 10);

        Console.WriteLine("--- Initial Field State ---");
        field.PrintFieldSlice(0, 4);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = field.GetFieldMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Scalar Field ---");
        field.EvolveField(steps: 20, couplingStrength: 0.5);

        field.PrintFieldSlice(0, 4);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = field.GetFieldMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nScalar Field represents fields with no spin or directional components.");
    }
}
