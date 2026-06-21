using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Vector Field Behavior
/// Used to describe fundamental forces and gauge bosons
/// Models electromagnetic, weak, and strong interactions
/// </summary>
public class VectorFieldBehavior
{
    private struct GaugeBoson
    {
        public int BosonId;
        public string ForceType;
        public double[] PolarizationVector;
        public double Momentum;
        public double Energy;
        public double Propagator;

        public GaugeBoson(int id, string force)
        {
            BosonId = id;
            ForceType = force;
            PolarizationVector = new double[3];
            Momentum = 0.0;
            Energy = 0.0;
            Propagator = 0.0;

            for (int i = 0; i < 3; i++)
            {
                PolarizationVector[i] = 1.0 / Math.Sqrt(3);
            }
        }
    }

    private List<GaugeBoson> bosons;
    private Random random;
    private const double c = 3e8;
    private const double HBAR = 1.054e-34;
    private const double alpha = 1.0 / 137.036;

    public VectorFieldBehavior()
    {
        this.bosons = new List<GaugeBoson>();
        this.random = new Random();
    }

    public void CreateGaugeBoson(string forceType)
    {
        var boson = new GaugeBoson(bosons.Count, forceType);
        boson.Momentum = random.NextDouble() * 1e-20;

        double mass = GetBosonMass(forceType);
        boson.Energy = Math.Sqrt(Math.Pow(boson.Momentum * c, 2) +
                                Math.Pow(mass * c * c, 2));

        bosons.Add(boson);
        Console.WriteLine($"Created {forceType} boson {boson.BosonId}");
    }

    private double GetBosonMass(string forceType)
    {
        return forceType switch
        {
            "photon" => 0.0,
            "W-boson" => 80.4e9 * 1.602e-19 / (c * c),
            "Z-boson" => 91.2e9 * 1.602e-19 / (c * c),
            "gluon" => 0.0,
            _ => 1e-30
        };
    }

    public void CalculateFeynmanPropagator()
    {
        foreach (var boson in bosons)
        {
            double fourMomentumSquared = Math.Pow(boson.Energy, 2) -
                                        Math.Pow(boson.Momentum * c, 2);

            boson.Propagator = 1.0 / (fourMomentumSquared + 1e-20);

            int idx = bosons.IndexOf(boson);
            bosons[idx] = boson;
        }
    }

    public void SimulateCoupling()
    {
        for (int i = 0; i < bosons.Count; i++)
        {
            for (int j = i + 1; j < bosons.Count; j++)
            {
                if (bosons[i].ForceType == bosons[j].ForceType)
                {
                    double couplingStrength = GetCouplingConstant(bosons[i].ForceType);

                    for (int k = 0; k < 3; k++)
                    {
                        bosons[i].PolarizationVector[k] +=
                            couplingStrength * bosons[j].PolarizationVector[k] * 0.01;

                        bosons[j].PolarizationVector[k] +=
                            couplingStrength * bosons[i].PolarizationVector[k] * 0.01;
                    }

                    bosons[i].Energy += couplingStrength * bosons[j].Energy * 0.001;
                    bosons[j].Energy += couplingStrength * bosons[i].Energy * 0.001;
                }
            }
        }

        NormalizePolarization();
    }

    private double GetCouplingConstant(string forceType)
    {
        return forceType switch
        {
            "photon" => alpha,
            "W-boson" => 0.65,
            "Z-boson" => 0.65,
            "gluon" => 0.118,
            _ => 0.1
        };
    }

    private void NormalizePolarization()
    {
        foreach (var boson in bosons)
        {
            double norm = Math.Sqrt(boson.PolarizationVector.Sum(x => x * x));

            if (norm > 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    boson.PolarizationVector[i] /= norm;
                }
            }

            int idx = bosons.IndexOf(boson);
            bosons[idx] = boson;
        }
    }

    public void EvolveFieldConfiguration(int steps)
    {
        Console.WriteLine($"\nEvolving gauge field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            CalculateFeynmanPropagator();
            SimulateCoupling();

            if (step % (steps / 3) == 0)
            {
                double totalEnergy = bosons.Sum(b => b.Energy);
                Console.WriteLine($"Step {step}: Total Boson Energy = {totalEnergy:E4}");
            }
        }
    }

    public Dictionary<string, object> GetGaugeMetrics()
    {
        var forceTypeCounts = bosons.GroupBy(b => b.ForceType)
            .ToDictionary(g => g.Key, g => g.Count());

        double totalEnergy = bosons.Sum(b => b.Energy);
        double avgMomentum = bosons.Average(b => b.Momentum);

        return new Dictionary<string, object>
        {
            { "BosonCount", bosons.Count },
            { "TotalEnergy", totalEnergy },
            { "AverageMomentum", avgMomentum },
            { "ForceTypes", string.Join(", ", forceTypeCounts.Keys) },
            { "FineStructureConstant", alpha }
        };
    }

    public void PrintBosonProperties(int limit = 5)
    {
        Console.WriteLine("Gauge Boson Properties:");
        for (int i = 0; i < Math.Min(limit, bosons.Count); i++)
        {
            var b = bosons[i];
            Console.WriteLine($"  [{i}] {b.ForceType}: E={b.Energy:E4}, " +
                            $"p={b.Momentum:E4}, Propagator={b.Propagator:E6}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Vector Field Behavior - Fundamental Forces ===\n");

        var vfb = new VectorFieldBehavior();

        Console.WriteLine("--- Creating Gauge Bosons ---");
        vfb.CreateGaugeBoson("photon");
        vfb.CreateGaugeBoson("photon");
        vfb.CreateGaugeBoson("W-boson");
        vfb.CreateGaugeBoson("Z-boson");
        vfb.CreateGaugeBoson("gluon");

        vfb.PrintBosonProperties(5);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = vfb.GetGaugeMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Gauge Field ---");
        vfb.EvolveFieldConfiguration(steps: 30);

        vfb.PrintBosonProperties(5);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = vfb.GetGaugeMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nVector Field Behavior models gauge boson interactions.");
    }
}
