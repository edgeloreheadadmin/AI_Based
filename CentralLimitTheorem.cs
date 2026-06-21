using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central Limit Theorem with Quantum Field Ray Manipulation
/// Uses quantum field ray manipulation for particle ray distribution
/// Demonstrates that sum of random variables approaches normal distribution
/// </summary>
public class CentralLimitTheorem
{
    private struct FieldRayParticle
    {
        public double Position;
        public double Momentum;
        public double Amplitude;

        public FieldRayParticle(double pos, double mom, double amp)
        {
            Position = pos;
            Momentum = mom;
            Amplitude = amp;
        }
    }

    private List<FieldRayParticle> particles;
    private Random random;
    private int sampleSize;

    public CentralLimitTheorem(int sampleSize = 30)
    {
        this.sampleSize = sampleSize;
        this.particles = new List<FieldRayParticle>();
        this.random = new Random();
    }

    public void GenerateParticleDistribution(int particleCount, int trialCount)
    {
        particles.Clear();

        for (int trial = 0; trial < trialCount; trial++)
        {
            double sumDistribution = 0;

            for (int i = 0; i < particleCount; i++)
            {
                double randomValue = random.NextDouble();
                sumDistribution += randomValue;
            }

            double meanValue = sumDistribution / particleCount;
            double momentum = (meanValue - 0.5) * 2.0;
            double amplitude = CalculateAmplitude(meanValue);

            particles.Add(new FieldRayParticle(
                position: meanValue,
                momentum: momentum,
                amplitude: amplitude
            ));
        }
    }

    private double CalculateAmplitude(double position)
    {
        double mean = 0.5;
        double sigma = Math.Sqrt(1.0 / 12.0 / sampleSize);
        double exponent = -Math.Pow(position - mean, 2) / (2 * sigma * sigma);
        return Math.Exp(exponent) / (sigma * Math.Sqrt(2 * Math.PI));
    }

    public Dictionary<string, double> AnalyzeDistribution()
    {
        if (particles.Count == 0)
            return new Dictionary<string, double>();

        double mean = particles.Average(p => p.Position);
        double variance = particles.Average(p => Math.Pow(p.Position - mean, 2));
        double stdDev = Math.Sqrt(variance);
        double skewness = CalculateSkewness();
        double kurtosis = CalculateKurtosis();

        return new Dictionary<string, double>
        {
            { "Mean", mean },
            { "StdDev", stdDev },
            { "Variance", variance },
            { "Skewness", skewness },
            { "Kurtosis", kurtosis },
            { "Min", particles.Min(p => p.Position) },
            { "Max", particles.Max(p => p.Position) }
        };
    }

    private double CalculateSkewness()
    {
        if (particles.Count == 0)
            return 0;

        double mean = particles.Average(p => p.Position);
        double m3 = particles.Average(p => Math.Pow(p.Position - mean, 3));
        double m2 = particles.Average(p => Math.Pow(p.Position - mean, 2));
        double sigma = Math.Sqrt(m2);

        return (sigma > 0) ? m3 / Math.Pow(sigma, 3) : 0;
    }

    private double CalculateKurtosis()
    {
        if (particles.Count == 0)
            return 0;

        double mean = particles.Average(p => p.Position);
        double m4 = particles.Average(p => Math.Pow(p.Position - mean, 4));
        double m2 = particles.Average(p => Math.Pow(p.Position - mean, 2));

        return (m2 > 0) ? (m4 / (m2 * m2)) - 3 : 0;
    }

    public void PrintHistogram(int bins = 10)
    {
        if (particles.Count == 0)
            return;

        double minPos = particles.Min(p => p.Position);
        double maxPos = particles.Max(p => p.Position);
        double binWidth = (maxPos - minPos) / bins;

        int[] histogram = new int[bins];

        foreach (var particle in particles)
        {
            int binIndex = (int)((particle.Position - minPos) / binWidth);
            if (binIndex >= bins)
                binIndex = bins - 1;
            histogram[binIndex]++;
        }

        Console.WriteLine("Particle Distribution Histogram:");
        for (int i = 0; i < bins; i++)
        {
            double binStart = minPos + i * binWidth;
            string bar = new string('█', histogram[i] / 2);
            Console.WriteLine($"  [{binStart:F3}]: {bar} ({histogram[i]})");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Central Limit Theorem - Quantum Field Ray Manipulation ===\n");

        Console.WriteLine("Test 1: Small Sample Size (n=5)");
        var clt5 = new CentralLimitTheorem(sampleSize: 5);
        clt5.GenerateParticleDistribution(particleCount: 5, trialCount: 1000);
        var stats5 = clt5.AnalyzeDistribution();
        PrintStats(stats5);
        clt5.PrintHistogram(10);

        Console.WriteLine("\n\nTest 2: Medium Sample Size (n=30)");
        var clt30 = new CentralLimitTheorem(sampleSize: 30);
        clt30.GenerateParticleDistribution(particleCount: 30, trialCount: 1000);
        var stats30 = clt30.AnalyzeDistribution();
        PrintStats(stats30);
        clt30.PrintHistogram(10);

        Console.WriteLine("\n\nTest 3: Large Sample Size (n=100)");
        var clt100 = new CentralLimitTheorem(sampleSize: 100);
        clt100.GenerateParticleDistribution(particleCount: 100, trialCount: 1000);
        var stats100 = clt100.AnalyzeDistribution();
        PrintStats(stats100);
        clt100.PrintHistogram(10);

        Console.WriteLine("\n\nObservation: As sample size increases, distribution approaches normal (Gaussian)");
        Console.WriteLine($"Skewness trend: {stats5["Skewness"]:F4} → {stats30["Skewness"]:F4} → {stats100["Skewness"]:F4}");
        Console.WriteLine("(Values closer to 0 indicate more normal distribution)");
    }

    private static void PrintStats(Dictionary<string, double> stats)
    {
        Console.WriteLine("Statistics:");
        foreach (var kvp in stats)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
        }
    }
}
