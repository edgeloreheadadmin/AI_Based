using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Causality Bias
/// Bias towards creating fields where effects follow causes (temporal ordering preserved)
/// Fields respect the light cone structure: information propagates at most at speed of light
/// </summary>
public class CausalityBias
{
    private struct Event
    {
        public int TimeStep;
        public int Position;
        public double FieldValue;
        public double CausalPast;

        public Event(int time, int pos, double value)
        {
            TimeStep = time;
            Position = pos;
            FieldValue = value;
            CausalPast = 0.0;
        }
    }

    private List<Event> events;
    private double[][] fieldTimeSeries;
    private int spatialSize;
    private int temporalSize;
    private double speedOfCausalInfluence;
    private Random random;

    public CausalityBias(int spatialSize = 20, int temporalSize = 50)
    {
        this.spatialSize = spatialSize;
        this.temporalSize = temporalSize;
        this.events = new List<Event>();
        this.fieldTimeSeries = new double[temporalSize][];
        this.speedOfCausalInfluence = 1.0;
        this.random = new Random();

        InitializeFieldEvolution();
    }

    private void InitializeFieldEvolution()
    {
        for (int t = 0; t < temporalSize; t++)
        {
            fieldTimeSeries[t] = new double[spatialSize];
            for (int x = 0; x < spatialSize; x++)
            {
                fieldTimeSeries[t][x] = random.NextDouble();
            }
        }
    }

    public double GetFieldValue(int t, int x)
    {
        if (t < 0 || t >= temporalSize || x < 0 || x >= spatialSize)
            return 0.0;

        return fieldTimeSeries[t][x];
    }

    public void SetFieldValue(int t, int x, double value)
    {
        if (t >= 0 && t < temporalSize && x >= 0 && x < spatialSize)
        {
            fieldTimeSeries[t][x] = value;
        }
    }

    public bool IsInLightCone(int t1, int x1, int t2, int x2)
    {
        int deltaT = Math.Abs(t2 - t1);
        int deltaX = Math.Abs(x2 - x1);

        return deltaX <= speedOfCausalInfluence * deltaT;
    }

    public double CalculateCausalPast(int t, int x)
    {
        double pastInfluence = 0.0;
        int influenceCount = 0;

        for (int pastTime = Math.Max(0, t - 3); pastTime < t; pastTime++)
        {
            int timeDiff = t - pastTime;

            for (int pastX = Math.Max(0, x - 3); pastX <= Math.Min(spatialSize - 1, x + 3); pastX++)
            {
                if (IsInLightCone(pastTime, pastX, t, x))
                {
                    double distance = Math.Abs(x - pastX);
                    double causalWeight = 1.0 / (1.0 + distance + timeDiff);

                    pastInfluence += GetFieldValue(pastTime, pastX) * causalWeight;
                    influenceCount++;
                }
            }
        }

        return influenceCount > 0 ? pastInfluence / influenceCount : 0.0;
    }

    public double MeasureCausality()
    {
        double totalCausalityScore = 0.0;

        for (int t = 1; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                double currentValue = GetFieldValue(t, x);
                double causalPast = CalculateCausalPast(t, x);

                double causalityScore = 1.0 / (1.0 + Math.Abs(currentValue - causalPast));
                totalCausalityScore += causalityScore;
            }
        }

        return totalCausalityScore / (temporalSize * spatialSize);
    }

    public void EnforceCausality()
    {
        Console.WriteLine("\nEnforcing causality on field evolution...\n");

        for (int t = 1; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                double causalPast = CalculateCausalPast(t, x);
                double currentValue = GetFieldValue(t, x);

                double causalizedValue = 0.6 * currentValue + 0.4 * causalPast;
                SetFieldValue(t, x, causalizedValue);
            }
        }

        Console.WriteLine("Causality enforcement complete.");
    }

    public void EvolveWithCausalityBias(int steps, double biasMagnitude)
    {
        Console.WriteLine($"\nEvolving field with causality bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int t = 1; t < temporalSize - 1; t++)
            {
                for (int x = 0; x < spatialSize; x++)
                {
                    double causalPast = CalculateCausalPast(t, x);
                    double currentValue = GetFieldValue(t, x);
                    double futureValue = GetFieldValue(t + 1, x);

                    double causalDerivative = futureValue - currentValue;
                    double newValue = currentValue + biasMagnitude * (causalPast - currentValue) * 0.01 + causalDerivative * 0.01;

                    SetFieldValue(t, x, newValue);
                }
            }

            if (step % (steps / 3) == 0)
            {
                double causalityScore = MeasureCausality();
                Console.WriteLine($"Step {step}: Causality Score = {causalityScore:F6}");
            }
        }
    }

    public Dictionary<string, object> GetCausalityMetrics()
    {
        double causalityScore = MeasureCausality();

        double maxViolation = 0.0;
        int violationCount = 0;

        for (int t = 1; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                double causalPast = CalculateCausalPast(t, x);
                double currentValue = GetFieldValue(t, x);
                double violation = Math.Abs(currentValue - causalPast);

                if (violation > 0.5)
                {
                    violationCount++;
                }
                maxViolation = Math.Max(maxViolation, violation);
            }
        }

        return new Dictionary<string, object>
        {
            { "SpatialSize", spatialSize },
            { "TemporalSize", temporalSize },
            { "CausalityScore", causalityScore },
            { "MaxCausalityViolation", maxViolation },
            { "ViolationCount", violationCount },
            { "SpeedOfCausalInfluence", speedOfCausalInfluence }
        };
    }

    public void PrintFieldEvolution(int xPosition)
    {
        Console.WriteLine($"\nField Evolution at x={xPosition} (Time flowing downward):");
        for (int t = 0; t < Math.Min(temporalSize, 15); t++)
        {
            double value = GetFieldValue(t, xPosition);
            char symbol = value > 0.66 ? '█' : value > 0.33 ? '▓' : '░';
            Console.WriteLine($"  t={t:D2}: {symbol} {value:F3}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Causality Bias - Temporal Ordering Preservation ===\n");

        var bias = new CausalityBias(spatialSize: 20, temporalSize: 50);

        Console.WriteLine("--- Initial Field Evolution ---");
        bias.PrintFieldEvolution(10);

        Console.WriteLine("\n--- Initial Causality Metrics ---");
        var metricsInitial = bias.GetCausalityMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Enforcing Causality ---");
        bias.EnforceCausality();

        Console.WriteLine("\n--- Evolving with Causality Bias ---");
        bias.EvolveWithCausalityBias(steps: 15, biasMagnitude: 0.5);

        bias.PrintFieldEvolution(10);

        Console.WriteLine("\n--- Final Causality Metrics ---");
        var metricsFinal = bias.GetCausalityMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nCausality Bias ensures effects follow causes within light cone structure.");
    }
}
