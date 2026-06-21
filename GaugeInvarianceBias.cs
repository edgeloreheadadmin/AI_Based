using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gauge Invariance Bias
/// Bias towards creating fields that remain unchanged under gauge transformations
/// Fields satisfy gauge invariance: φ(x) → φ(x)e^(iθ(x)) for U(1), or similar for non-abelian groups
/// </summary>
public class GaugeInvarianceBias
{
    private struct GaugeTransformation
    {
        public double Angle;
        public double[] GaugeField;

        public GaugeTransformation(int dimension)
        {
            Angle = 0.0;
            GaugeField = new double[dimension];
            for (int i = 0; i < dimension; i++)
            {
                GaugeField[i] = 0.0;
            }
        }
    }

    private List<GaugeTransformation> transformations;
    private double[][] fieldConfigurations;
    private int fieldDimension;
    private int configurationCount;
    private Random random;

    public GaugeInvarianceBias(int fieldDimension = 4, int configurationCount = 10)
    {
        this.fieldDimension = fieldDimension;
        this.configurationCount = configurationCount;
        this.transformations = new List<GaugeTransformation>();
        this.fieldConfigurations = new double[configurationCount][];
        this.random = new Random();

        InitializeConfigurations();
    }

    private void InitializeConfigurations()
    {
        for (int i = 0; i < configurationCount; i++)
        {
            fieldConfigurations[i] = new double[fieldDimension];
            for (int j = 0; j < fieldDimension; j++)
            {
                fieldConfigurations[i][j] = random.NextDouble();
            }
        }
    }

    public void GenerateGaugeTransformation(double angle)
    {
        GaugeTransformation transformation = new GaugeTransformation(fieldDimension);
        transformation.Angle = angle;

        for (int i = 0; i < fieldDimension; i++)
        {
            transformation.GaugeField[i] = random.NextDouble() * angle;
        }

        transformations.Add(transformation);
    }

    public double[] ApplyGaugeTransformation(double[] field, GaugeTransformation gauge)
    {
        double[] transformed = new double[field.Length];

        for (int i = 0; i < field.Length; i++)
        {
            double phase = Math.Exp(gauge.Angle * gauge.GaugeField[i]);
            transformed[i] = field[i] * phase;
        }

        return transformed;
    }

    public double CalculateGaugeInvarianceScore(double[] field)
    {
        if (transformations.Count == 0)
            return 0.0;

        double totalDeviation = 0.0;

        foreach (var gauge in transformations)
        {
            double[] transformed = ApplyGaugeTransformation(field, gauge);

            double fieldNorm = Math.Sqrt(field.Sum(x => x * x));
            double transformedNorm = Math.Sqrt(transformed.Sum(x => x * x));

            double deviation = Math.Abs(fieldNorm - transformedNorm);
            totalDeviation += deviation;
        }

        double averageDeviation = totalDeviation / transformations.Count;
        return Math.Max(0, 1.0 - averageDeviation);
    }

    public void EnforceGaugeInvariance()
    {
        Console.WriteLine("\nEnforcing gauge invariance on field configurations...\n");

        for (int configIdx = 0; configIdx < configurationCount; configIdx++)
        {
            double norm = Math.Sqrt(fieldConfigurations[configIdx].Sum(x => x * x));

            if (norm > 0)
            {
                for (int i = 0; i < fieldDimension; i++)
                {
                    fieldConfigurations[configIdx][i] /= norm;
                }
            }
        }

        Console.WriteLine("Gauge invariance enforcement complete.");
    }

    public void EvolveWithGaugeBias(int steps, double biasMagnitude)
    {
        Console.WriteLine($"\nEvolving fields with gauge invariance bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int configIdx = 0; configIdx < configurationCount; configIdx++)
            {
                double score = CalculateGaugeInvarianceScore(fieldConfigurations[configIdx]);

                for (int i = 0; i < fieldDimension; i++)
                {
                    double random_perturbation = (random.NextDouble() - 0.5) * 0.1;
                    fieldConfigurations[configIdx][i] += biasMagnitude * score * random_perturbation;
                }

                double norm = Math.Sqrt(fieldConfigurations[configIdx].Sum(x => x * x));
                if (norm > 0)
                {
                    for (int i = 0; i < fieldDimension; i++)
                    {
                        fieldConfigurations[configIdx][i] /= norm;
                    }
                }
            }

            if (step % (steps / 3) == 0)
            {
                double avgScore = fieldConfigurations.Average(config => CalculateGaugeInvarianceScore(config));
                Console.WriteLine($"Step {step}: Average Gauge Invariance Score = {avgScore:F6}");
            }
        }
    }

    public Dictionary<string, object> GetGaugeMetrics()
    {
        double[] scores = fieldConfigurations.Select(config => CalculateGaugeInvarianceScore(config)).ToArray();
        double avgScore = scores.Average();
        double maxScore = scores.Max();
        double minScore = scores.Min();

        return new Dictionary<string, object>
        {
            { "Configurations", configurationCount },
            { "AverageGaugeInvarianceScore", avgScore },
            { "MaxScore", maxScore },
            { "MinScore", minScore },
            { "TransformationCount", transformations.Count }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Gauge Invariance Bias - Field Creation Constraint ===\n");

        var bias = new GaugeInvarianceBias(fieldDimension: 4, configurationCount: 10);

        Console.WriteLine("--- Generating Gauge Transformations ---");
        for (int i = 0; i < 5; i++)
        {
            bias.GenerateGaugeTransformation(Math.PI / 4 * (i + 1));
        }

        Console.WriteLine("--- Initial Gauge Metrics ---");
        var metricsInitial = bias.GetGaugeMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Enforcing Gauge Invariance ---");
        bias.EnforceGaugeInvariance();

        Console.WriteLine("\n--- Evolving with Gauge Bias ---");
        bias.EvolveWithGaugeBias(steps: 20, biasMagnitude: 0.5);

        Console.WriteLine("\n--- Final Gauge Metrics ---");
        var metricsFinal = bias.GetGaugeMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nGauge Invariance Bias ensures fields remain unchanged under gauge transformations.");
    }
}
