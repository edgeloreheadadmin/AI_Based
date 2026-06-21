using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Frequency Field Transform
/// Mathematical operation transforming quantum fields between frequency domains
/// Enables spectral analysis, filtering, and mode decomposition of quantum fields
/// </summary>
public class QuantumFrequencyFieldTransform
{
    private struct ComplexNumber
    {
        public double Real;
        public double Imaginary;

        public double Magnitude()
        {
            return Math.Sqrt(Real * Real + Imaginary * Imaginary);
        }

        public double Phase()
        {
            return Math.Atan2(Imaginary, Real);
        }

        public ComplexNumber Multiply(ComplexNumber other)
        {
            return new ComplexNumber
            {
                Real = Real * other.Real - Imaginary * other.Imaginary,
                Imaginary = Real * other.Imaginary + Imaginary * other.Real
            };
        }
    }

    private ComplexNumber[][] spatialDomain;
    private ComplexNumber[][] frequencyDomain;
    private int fieldCount;
    private int domainSize;
    private Random random;

    public QuantumFrequencyFieldTransform(int fieldCount = 4, int domainSize = 16)
    {
        this.fieldCount = fieldCount;
        this.domainSize = domainSize;
        this.spatialDomain = new ComplexNumber[fieldCount][];
        this.frequencyDomain = new ComplexNumber[fieldCount][];
        this.random = new Random();

        InitializeFields();
    }

    private void InitializeFields()
    {
        for (int i = 0; i < fieldCount; i++)
        {
            spatialDomain[i] = new ComplexNumber[domainSize];
            frequencyDomain[i] = new ComplexNumber[domainSize];

            for (int j = 0; j < domainSize; j++)
            {
                double phase = random.NextDouble() * 2 * Math.PI;
                double magnitude = Math.Sin(j * Math.PI / domainSize);

                spatialDomain[i][j] = new ComplexNumber
                {
                    Real = magnitude * Math.Cos(phase),
                    Imaginary = magnitude * Math.Sin(phase)
                };
            }
        }
    }

    public void PerformDiscreteQuantumFourierTransform(int fieldIndex)
    {
        if (fieldIndex < 0 || fieldIndex >= fieldCount)
            return;

        Console.WriteLine($"\nPerforming Discrete Quantum Fourier Transform on field {fieldIndex}...\n");

        for (int k = 0; k < domainSize; k++)
        {
            ComplexNumber sum = new ComplexNumber { Real = 0, Imaginary = 0 };

            for (int n = 0; n < domainSize; n++)
            {
                double angle = -2 * Math.PI * k * n / domainSize;
                ComplexNumber basis = new ComplexNumber
                {
                    Real = Math.Cos(angle),
                    Imaginary = Math.Sin(angle)
                };

                ComplexNumber product = spatialDomain[fieldIndex][n].Multiply(basis);
                sum.Real += product.Real;
                sum.Imaginary += product.Imaginary;
            }

            sum.Real /= Math.Sqrt(domainSize);
            sum.Imaginary /= Math.Sqrt(domainSize);

            frequencyDomain[fieldIndex][k] = sum;
        }

        Console.WriteLine("Discrete Quantum Fourier Transform complete.");
    }

    public void PerformInverseQuantumFourierTransform(int fieldIndex)
    {
        if (fieldIndex < 0 || fieldIndex >= fieldCount)
            return;

        Console.WriteLine($"\nPerforming Inverse Quantum Fourier Transform on field {fieldIndex}...\n");

        ComplexNumber[][] reconstructed = new ComplexNumber[fieldCount][];
        reconstructed[fieldIndex] = new ComplexNumber[domainSize];

        for (int n = 0; n < domainSize; n++)
        {
            ComplexNumber sum = new ComplexNumber { Real = 0, Imaginary = 0 };

            for (int k = 0; k < domainSize; k++)
            {
                double angle = 2 * Math.PI * k * n / domainSize;
                ComplexNumber basis = new ComplexNumber
                {
                    Real = Math.Cos(angle),
                    Imaginary = Math.Sin(angle)
                };

                ComplexNumber product = frequencyDomain[fieldIndex][k].Multiply(basis);
                sum.Real += product.Real;
                sum.Imaginary += product.Imaginary;
            }

            sum.Real /= Math.Sqrt(domainSize);
            sum.Imaginary /= Math.Sqrt(domainSize);

            reconstructed[fieldIndex][n] = sum;
        }

        spatialDomain[fieldIndex] = reconstructed[fieldIndex];
        Console.WriteLine("Inverse Transform complete.");
    }

    public void ApplyFrequencyFilter(int fieldIndex, double cutoffFrequency)
    {
        Console.WriteLine($"\nApplying frequency filter (cutoff={cutoffFrequency:F3}) to field {fieldIndex}...\n");

        for (int k = 0; k < domainSize; k++)
        {
            double normalizedFrequency = (double)k / domainSize;

            if (normalizedFrequency > cutoffFrequency)
            {
                frequencyDomain[fieldIndex][k].Real = 0;
                frequencyDomain[fieldIndex][k].Imaginary = 0;
            }
            else
            {
                double filterFactor = 1.0 - (normalizedFrequency / cutoffFrequency) * 0.2;
                frequencyDomain[fieldIndex][k].Real *= filterFactor;
                frequencyDomain[fieldIndex][k].Imaginary *= filterFactor;
            }
        }

        Console.WriteLine("Frequency filter applied.");
    }

    public double[] GetPowerSpectrum(int fieldIndex)
    {
        double[] spectrum = new double[domainSize];

        for (int k = 0; k < domainSize; k++)
        {
            spectrum[k] = frequencyDomain[fieldIndex][k].Magnitude() * frequencyDomain[fieldIndex][k].Magnitude();
        }

        return spectrum;
    }

    public void PerformSpectralAnalysis()
    {
        Console.WriteLine("\nPerforming Spectral Analysis on all fields...\n");

        for (int i = 0; i < fieldCount; i++)
        {
            PerformDiscreteQuantumFourierTransform(i);

            double[] spectrum = GetPowerSpectrum(i);
            double dominantFrequency = Array.IndexOf(spectrum, spectrum.Max());
            double totalPower = spectrum.Sum();

            Console.WriteLine($"Field {i}:");
            Console.WriteLine($"  Dominant Frequency: {dominantFrequency}");
            Console.WriteLine($"  Total Power: {totalPower:F6}");
            Console.WriteLine($"  Spectral Centroid: {CalculateSpectralCentroid(spectrum):F6}");
        }
    }

    private double CalculateSpectralCentroid(double[] spectrum)
    {
        double weightedSum = 0.0;
        double totalPower = spectrum.Sum();

        for (int k = 0; k < spectrum.Length; k++)
        {
            weightedSum += k * spectrum[k];
        }

        return totalPower > 0 ? weightedSum / totalPower : 0.0;
    }

    public Dictionary<string, object> GetTransformMetrics()
    {
        double totalSpatialEnergy = 0.0;
        double totalFrequencyEnergy = 0.0;

        for (int i = 0; i < fieldCount; i++)
        {
            for (int j = 0; j < domainSize; j++)
            {
                totalSpatialEnergy += spatialDomain[i][j].Magnitude() * spatialDomain[i][j].Magnitude();
                totalFrequencyEnergy += frequencyDomain[i][j].Magnitude() * frequencyDomain[i][j].Magnitude();
            }
        }

        return new Dictionary<string, object>
        {
            { "FieldCount", fieldCount },
            { "DomainSize", domainSize },
            { "TotalSpatialEnergy", totalSpatialEnergy },
            { "TotalFrequencyEnergy", totalFrequencyEnergy },
            { "EnergyConservation", Math.Abs(totalSpatialEnergy - totalFrequencyEnergy) / totalSpatialEnergy }
        };
    }

    public void PrintSpectrum(int fieldIndex)
    {
        Console.WriteLine($"\nFrequency Spectrum - Field {fieldIndex}:");
        double[] spectrum = GetPowerSpectrum(fieldIndex);

        double maxPower = spectrum.Max();
        for (int k = 0; k < Math.Min(domainSize, 16); k++)
        {
            int barLength = (int)(spectrum[k] / maxPower * 40);
            Console.Write($"  f{k:D2}: ");
            for (int i = 0; i < barLength; i++)
                Console.Write("█");
            Console.WriteLine($" {spectrum[k]:F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Frequency Field Transform - Spectral Analysis ===\n");

        var transform = new QuantumFrequencyFieldTransform(fieldCount: 4, domainSize: 16);

        Console.WriteLine("--- Initial Transform Metrics ---");
        var metricsInitial = transform.GetTransformMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Performing Spectral Analysis ---");
        transform.PerformSpectralAnalysis();

        transform.PrintSpectrum(0);

        Console.WriteLine("\n--- Applying Frequency Filters ---");
        transform.ApplyFrequencyFilter(0, cutoffFrequency: 0.5);
        transform.ApplyFrequencyFilter(1, cutoffFrequency: 0.6);
        transform.ApplyFrequencyFilter(2, cutoffFrequency: 0.7);

        Console.WriteLine("\n--- Inverse Transform ---");
        transform.PerformInverseQuantumFourierTransform(0);

        transform.PrintSpectrum(0);

        Console.WriteLine("\n--- Final Transform Metrics ---");
        var metricsFinal = transform.GetTransformMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Frequency Field Transform enables spectral analysis and filtering of fields.");
    }
}
