using System;
using System.Collections.Generic;

/// <summary>
/// QFBWPA - Quantum Fokker-Blankardt-Wiener Poisson Algorithm
/// Calculates probability of a given number of events occurring in a fixed interval
/// Uses quantum mechanics principles with Poisson distribution modeling
/// </summary>
public class QFBWPA
{
    private struct QuantumEvent
    {
        public int EventCount;
        public double TimeInterval;
        public double ArrivalRate;
        public double Probability;
        public double QuantumAmplitude;

        public QuantumEvent(int count, double interval, double rate)
        {
            EventCount = count;
            TimeInterval = interval;
            ArrivalRate = rate;
            Probability = 0;
            QuantumAmplitude = 0;
        }
    }

    private double arrivalRateParameter;
    private List<QuantumEvent> eventSpace;
    private double[] amplitudes;
    private Random random;

    public QFBWPA(double arrivalRate = 2.5)
    {
        this.arrivalRateParameter = arrivalRate;
        this.eventSpace = new List<QuantumEvent>();
        this.random = new Random();
    }

    public void InitializeEventSpace(int maxEvents, double timeInterval)
    {
        eventSpace.Clear();
        amplitudes = new double[maxEvents + 1];

        for (int k = 0; k <= maxEvents; k++)
        {
            var evt = new QuantumEvent(k, timeInterval, arrivalRateParameter);
            eventSpace.Add(evt);
        }
    }

    public double CalculatePoissonProbability(int eventCount, double lambda)
    {
        double numerator = Math.Pow(lambda, eventCount) * Math.Exp(-lambda);
        double denominator = Factorial(eventCount);
        return numerator / denominator;
    }

    private double Factorial(int n)
    {
        if (n <= 1)
            return 1;

        double result = 1;
        for (int i = 2; i <= n; i++)
            result *= i;

        return result;
    }

    public void ComputeQuantumAmplitudes(double timeInterval)
    {
        for (int k = 0; k < eventSpace.Count; k++)
        {
            double lambda = arrivalRateParameter * timeInterval;
            double probability = CalculatePoissonProbability(k, lambda);

            double amplitude = Math.Sqrt(probability);
            amplitudes[k] = amplitude;

            var evt = eventSpace[k];
            evt.Probability = probability;
            evt.QuantumAmplitude = amplitude;
            eventSpace[k] = evt;
        }

        NormalizeAmplitudes();
    }

    private void NormalizeAmplitudes()
    {
        double norm = 0;
        foreach (double amp in amplitudes)
            norm += amp * amp;

        norm = Math.Sqrt(norm);

        if (norm > 0)
        {
            for (int i = 0; i < amplitudes.Length; i++)
                amplitudes[i] /= norm;

            for (int i = 0; i < eventSpace.Count; i++)
            {
                var evt = eventSpace[i];
                evt.QuantumAmplitude = amplitudes[i];
                eventSpace[i] = evt;
            }
        }
    }

    public Dictionary<string, double> AnalyzeEventDistribution()
    {
        double expectedValue = 0;
        double variance = 0;
        double skewness = 0;

        foreach (var evt in eventSpace)
        {
            expectedValue += evt.EventCount * evt.Probability;
        }

        foreach (var evt in eventSpace)
        {
            variance += Math.Pow(evt.EventCount - expectedValue, 2) * evt.Probability;
        }

        foreach (var evt in eventSpace)
        {
            skewness += Math.Pow(evt.EventCount - expectedValue, 3) * evt.Probability;
        }

        double stdDev = Math.Sqrt(variance);
        skewness = (stdDev > 0) ? skewness / (stdDev * stdDev * stdDev) : 0;

        return new Dictionary<string, double>
        {
            { "ExpectedValue", expectedValue },
            { "Variance", variance },
            { "StdDev", stdDev },
            { "Skewness", skewness },
            { "ArrivalRate", arrivalRateParameter }
        };
    }

    public int MeasureEventOutcome()
    {
        double randomValue = random.NextDouble();
        double cumulativeProbability = 0;

        for (int i = 0; i < eventSpace.Count; i++)
        {
            cumulativeProbability += eventSpace[i].Probability;
            if (randomValue < cumulativeProbability)
                return eventSpace[i].EventCount;
        }

        return eventSpace.Count - 1;
    }

    public void PrintEventProbabilities()
    {
        Console.WriteLine("Event Probability Distribution:");
        Console.WriteLine("Events | Probability | Quantum Amplitude");
        Console.WriteLine("-------|-------------|------------------");

        foreach (var evt in eventSpace)
        {
            if (evt.Probability > 0.001)
            {
                Console.WriteLine($"  {evt.EventCount,2}   | {evt.Probability,10:F4} | {evt.QuantumAmplitude,15:F4}");
            }
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== QFBWPA - Quantum Fokker-Blankardt-Wiener Poisson Algorithm ===\n");

        Console.WriteLine("Scenario: Customers arriving at a service center");
        Console.WriteLine("Arrival Rate (λ): 2.5 customers per hour\n");

        var qfbwpa = new QFBWPA(arrivalRate: 2.5);

        Console.WriteLine("--- Analysis for 1-hour interval ---");
        qfbwpa.InitializeEventSpace(maxEvents: 10, timeInterval: 1.0);
        qfbwpa.ComputeQuantumAmplitudes(timeInterval: 1.0);
        qfbwpa.PrintEventProbabilities();

        var stats1 = qfbwpa.AnalyzeEventDistribution();
        Console.WriteLine("\nStatistics:");
        foreach (var kvp in stats1)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
        }

        Console.WriteLine("\n\n--- Measurement Simulation (1000 trials) ---");
        var measurements = new Dictionary<int, int>();

        for (int i = 0; i < 1000; i++)
        {
            int outcome = qfbwpa.MeasureEventOutcome();
            if (!measurements.ContainsKey(outcome))
                measurements[outcome] = 0;
            measurements[outcome]++;
        }

        Console.WriteLine("Measurement Results:");
        foreach (var kvp in measurements)
        {
            int count = kvp.Value;
            string bar = new string('█', count / 20);
            Console.WriteLine($"  {kvp.Key} events: {bar} ({count / 10.0:F1}%)");
        }

        Console.WriteLine("\n\n--- Analysis for 2-hour interval ---");
        var qfbwpa2 = new QFBWPA(arrivalRate: 2.5);
        qfbwpa2.InitializeEventSpace(maxEvents: 15, timeInterval: 2.0);
        qfbwpa2.ComputeQuantumAmplitudes(timeInterval: 2.0);

        var stats2 = qfbwpa2.AnalyzeEventDistribution();
        Console.WriteLine("Statistics:");
        foreach (var kvp in stats2)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
        }

        Console.WriteLine("\nQFBWPA successfully models quantum probabilistic events with Poisson distribution.");
    }
}
