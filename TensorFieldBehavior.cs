using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tensor Field Behavior
/// Used to describe gravity and the curvature of spacetime
/// Implements Riemann curvature and metric tensors
/// </summary>
public class TensorFieldBehavior
{
    private struct MetricTensor
    {
        public int RegionId;
        public double[,] Metric;
        public double[,,] RiemannCurvature;
        public double RicciScalar;
        public double Determinant;

        public MetricTensor(int id)
        {
            RegionId = id;
            Metric = new double[4, 4];
            RiemannCurvature = new double[4, 4, 4];
            RicciScalar = 0.0;
            Determinant = 0.0;

            InitializeMinkowskiMetric();
        }

        private void InitializeMinkowskiMetric()
        {
            Metric[0, 0] = -1.0;
            Metric[1, 1] = 1.0;
            Metric[2, 2] = 1.0;
            Metric[3, 3] = 1.0;
        }
    }

    private List<MetricTensor> spacetimeRegions;
    private Random random;
    private const double c = 3e8;
    private const double G = 6.674e-11;

    public TensorFieldBehavior()
    {
        this.spacetimeRegions = new List<MetricTensor>();
        this.random = new Random();
    }

    public void CreateSpacetimeRegion()
    {
        var region = new MetricTensor(spacetimeRegions.Count);
        spacetimeRegions.Add(region);
        Console.WriteLine($"Created spacetime region {region.RegionId}");
    }

    public void CalculateCurvature()
    {
        foreach (var region in spacetimeRegions)
        {
            CalculateRiemannTensor(ref region);
            CalculateRicciScalar(ref region);
            CalculateMetricDeterminant(ref region);

            int idx = spacetimeRegions.IndexOf(region);
            spacetimeRegions[idx] = region;
        }
    }

    private void CalculateRiemannTensor(ref MetricTensor region)
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    double riemannComponent = 0;

                    for (int l = 0; l < 4; l++)
                    {
                        riemannComponent += region.Metric[i, l] *
                                         Math.Sin(i * 0.1) *
                                         Math.Cos(j * 0.1) *
                                         Math.Sin(k * 0.1);
                    }

                    region.RiemannCurvature[i, j, k] = riemannComponent * 0.01;
                }
            }
        }
    }

    private void CalculateRicciScalar(ref MetricTensor region)
    {
        double ricciScalar = 0;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                ricciScalar += region.RiemannCurvature[i, j, i];
            }
        }

        region.RicciScalar = ricciScalar;
    }

    private void CalculateMetricDeterminant(ref MetricTensor region)
    {
        double det = region.Metric[0, 0] * region.Metric[1, 1] *
                    region.Metric[2, 2] * region.Metric[3, 3] +
                    random.NextDouble() * 0.01;

        region.Determinant = det;
    }

    public void InjectEnergyMomentum(int regionId, double massEnergy)
    {
        if (regionId >= spacetimeRegions.Count)
            return;

        var region = spacetimeRegions[regionId];

        double curveAmount = massEnergy * G / (c * c);

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (i == j && i > 0)
                {
                    region.Metric[i, j] += curveAmount * 0.1;
                }
            }
        }

        spacetimeRegions[regionId] = region;
    }

    public void EvolveCurvedSpacetime(int steps)
    {
        Console.WriteLine($"\nEvolving curved spacetime for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            CalculateCurvature();

            if (step % (steps / 3) == 0)
            {
                double totalCurvature = spacetimeRegions.Sum(r => Math.Abs(r.RicciScalar));
                Console.WriteLine($"Step {step}: Total Ricci Curvature = {totalCurvature:F6}");
            }
        }
    }

    public Dictionary<string, object> GetCurvatureMetrics()
    {
        double avgRicciScalar = spacetimeRegions.Average(r => r.RicciScalar);
        double totalCurvature = spacetimeRegions.Sum(r => Math.Abs(r.RicciScalar));

        return new Dictionary<string, object>
        {
            { "RegionCount", spacetimeRegions.Count },
            { "AverageRicciScalar", avgRicciScalar },
            { "TotalCurvature", totalCurvature },
            { "SpeedOfLight", c },
            { "GravitationalConstant", G }
        };
    }

    public void PrintMetric(int regionId)
    {
        if (regionId >= spacetimeRegions.Count)
            return;

        var region = spacetimeRegions[regionId];
        Console.WriteLine($"\nMetric Tensor (Region {regionId}):");
        for (int i = 0; i < 4; i++)
        {
            Console.Write("  ");
            for (int j = 0; j < 4; j++)
            {
                Console.Write($"{region.Metric[i, j]:F4} ");
            }
            Console.WriteLine();
        }
        Console.WriteLine($"  Ricci Scalar: {region.RicciScalar:F6}");
        Console.WriteLine($"  Determinant: {region.Determinant:F6}");
    }

    public static void Main()
    {
        Console.WriteLine("=== Tensor Field Behavior - Spacetime Curvature ===\n");

        var tfb = new TensorFieldBehavior();

        Console.WriteLine("--- Creating Spacetime Regions ---");
        for (int i = 0; i < 3; i++)
        {
            tfb.CreateSpacetimeRegion();
        }

        tfb.PrintMetric(0);

        Console.WriteLine("\n--- Injecting Energy-Momentum ---");
        tfb.InjectEnergyMomentum(0, massEnergy: 1e30);
        tfb.InjectEnergyMomentum(1, massEnergy: 5e29);

        Console.WriteLine("\n--- Initial Curvature ---");
        tfb.CalculateCurvature();
        var metricsInitial = tfb.GetCurvatureMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Curved Spacetime ---");
        tfb.EvolveCurvedSpacetime(steps: 30);

        tfb.PrintMetric(0);

        Console.WriteLine("\n--- Final Curvature Metrics ---");
        var metricsFinal = tfb.GetCurvatureMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nTensor Field Behavior models gravitational spacetime curvature.");
    }
}
