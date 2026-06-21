using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Vector Field Interactions
/// Fields interact through vector couplings
/// Used to describe fundamental forces and gauge bosons
/// </summary>
public class VectorFieldInteractions
{
    private struct VectorField
    {
        public int FieldId;
        public double[] ComponentX, ComponentY, ComponentZ;
        public double GaugeCoupling;
        public string GaugeType;
        public double MagnitudeSquared;

        public VectorField(int id, string gauge = "U(1)")
        {
            FieldId = id;
            GaugeType = gauge;
            ComponentX = new double[3];
            ComponentY = new double[3];
            ComponentZ = new double[3];
            GaugeCoupling = 0.0;
            MagnitudeSquared = 0.0;

            for (int i = 0; i < 3; i++)
            {
                ComponentX[i] = Math.Sin(i * Math.PI / 3);
                ComponentY[i] = Math.Cos(i * Math.PI / 3);
                ComponentZ[i] = 0.5 * Math.Sin(2 * i * Math.PI / 3);
            }
        }
    }

    private List<VectorField> fields;
    private Random random;

    public VectorFieldInteractions()
    {
        this.fields = new List<VectorField>();
        this.random = new Random();
    }

    public void CreateVectorField(string gaugeType)
    {
        var field = new VectorField(fields.Count, gaugeType);
        field.GaugeCoupling = random.NextDouble() * 0.5;

        double mag = 0;
        for (int i = 0; i < 3; i++)
        {
            mag += field.ComponentX[i] * field.ComponentX[i] +
                   field.ComponentY[i] * field.ComponentY[i] +
                   field.ComponentZ[i] * field.ComponentZ[i];
        }
        field.MagnitudeSquared = mag;

        fields.Add(field);
        Console.WriteLine($"Created vector field {field.FieldId} ({gaugeType})");
    }

    public void CalculateFieldStrength()
    {
        foreach (var field in fields)
        {
            double magnitude = 0;

            for (int i = 0; i < 3; i++)
            {
                magnitude += field.ComponentX[i] * field.ComponentX[i] +
                           field.ComponentY[i] * field.ComponentY[i] +
                           field.ComponentZ[i] * field.ComponentZ[i];
            }

            field.MagnitudeSquared = magnitude;
            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplyGaugeTransformation()
    {
        foreach (var field in fields)
        {
            double angle = field.GaugeCoupling * Math.PI;
            double cosAngle = Math.Cos(angle);
            double sinAngle = Math.Sin(angle);

            for (int i = 0; i < 3; i++)
            {
                double tempX = field.ComponentX[i];
                field.ComponentX[i] = tempX * cosAngle - field.ComponentY[i] * sinAngle;
                field.ComponentY[i] = tempX * sinAngle + field.ComponentY[i] * cosAngle;
            }

            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplyVectorCouplings()
    {
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = i + 1; j < fields.Count; j++)
            {
                double coupling = fields[i].GaugeCoupling * fields[j].GaugeCoupling;

                for (int k = 0; k < 3; k++)
                {
                    double dotProduct = fields[i].ComponentX[k] * fields[j].ComponentX[k] +
                                       fields[i].ComponentY[k] * fields[j].ComponentY[k] +
                                       fields[i].ComponentZ[k] * fields[j].ComponentZ[k];

                    fields[i].ComponentX[k] += coupling * fields[j].ComponentX[k] * 0.05;
                    fields[i].ComponentY[k] += coupling * fields[j].ComponentY[k] * 0.05;
                    fields[i].ComponentZ[k] += coupling * fields[j].ComponentZ[k] * 0.05;

                    fields[j].ComponentX[k] -= coupling * fields[i].ComponentX[k] * 0.05;
                    fields[j].ComponentY[k] -= coupling * fields[i].ComponentY[k] * 0.05;
                    fields[j].ComponentZ[k] -= coupling * fields[i].ComponentZ[k] * 0.05;
                }
            }
        }
    }

    public void EvolveGaugeFields(int steps)
    {
        Console.WriteLine($"\nEvolving gauge fields for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            ApplyGaugeTransformation();
            ApplyVectorCouplings();
            CalculateFieldStrength();

            if (step % (steps / 3) == 0)
            {
                double totalMagnitude = fields.Sum(f => f.MagnitudeSquared);
                Console.WriteLine($"Step {step}: Total Field Magnitude = {totalMagnitude:F6}");
            }
        }
    }

    public Dictionary<string, object> GetVectorMetrics()
    {
        double totalMagnitude = fields.Sum(f => f.MagnitudeSquared);
        double avgCoupling = fields.Average(f => f.GaugeCoupling);

        var gaugeCount = fields.GroupBy(f => f.GaugeType)
            .ToDictionary(g => g.Key, g => g.Count());

        return new Dictionary<string, object>
        {
            { "FieldCount", fields.Count },
            { "TotalFieldMagnitude", totalMagnitude },
            { "AverageGaugeCoupling", avgCoupling },
            { "GaugeTypes", string.Join(", ", gaugeCount.Keys) }
        };
    }

    public void PrintVectorComponents(int fieldId)
    {
        if (fieldId >= fields.Count)
            return;

        var field = fields[fieldId];
        Console.WriteLine($"\nVector Field {fieldId} Components ({field.GaugeType}):");
        Console.WriteLine($"  X: [{string.Join(", ", field.ComponentX.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Y: [{string.Join(", ", field.ComponentY.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Z: [{string.Join(", ", field.ComponentZ.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Magnitude²: {field.MagnitudeSquared:F6}");
    }

    public static void Main()
    {
        Console.WriteLine("=== Vector Field Interactions - Gauge Boson Coupling ===\n");

        var vfi = new VectorFieldInteractions();

        Console.WriteLine("--- Creating Vector Fields ---");
        vfi.CreateVectorField("U(1)");
        vfi.CreateVectorField("SU(2)");
        vfi.CreateVectorField("SU(3)");

        vfi.PrintVectorComponents(0);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = vfi.GetVectorMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Gauge Fields ---");
        vfi.EvolveGaugeFields(steps: 30);

        vfi.PrintVectorComponents(0);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = vfi.GetVectorMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nVector Field Interactions model fundamental forces.");
    }
}
