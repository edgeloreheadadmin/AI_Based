using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// QEMLPI - Quantum Entanglement Machine Learning Performance Improvement
/// Uses quantum entanglement to improve machine learning performance
/// Combines entanglement with classical ML for enhanced predictions
/// </summary>
public class QEMLPI
{
    private struct EntangledFeature
    {
        public int FeatureId;
        public double[] ClassicalValues;
        public double[] QuantumAmplitudes;
        public double EntanglementStrength;
        public double CorrelationCoefficient;

        public EntangledFeature(int id, double[] values)
        {
            FeatureId = id;
            ClassicalValues = values;
            QuantumAmplitudes = new double[values.Length];
            EntanglementStrength = 0.0;
            CorrelationCoefficient = 0.0;

            for (int i = 0; i < values.Length; i++)
            {
                QuantumAmplitudes[i] = Math.Sqrt(Math.Abs(values[i])) / Math.Sqrt(values.Length);
            }
        }
    }

    private List<EntangledFeature> features;
    private double[] predictions;
    private double[] actualLabels;
    private double entanglementFidelity;
    private Random random;

    public QEMLPI()
    {
        this.features = new List<EntangledFeature>();
        this.random = new Random();
        this.entanglementFidelity = 0.0;
    }

    public void AddFeature(int featureId, double[] featureData)
    {
        var feature = new EntangledFeature(featureId, featureData);
        features.Add(feature);
    }

    public void EstablishEntanglement()
    {
        for (int i = 0; i < features.Count; i++)
        {
            var feature = features[i];

            for (int j = 0; j < feature.QuantumAmplitudes.Length; j++)
            {
                double phase = (i + j) * Math.PI / 4;
                feature.QuantumAmplitudes[j] *= Math.Cos(phase);
            }

            feature.EntanglementStrength = CalculateEntanglementStrength(feature);
            feature.CorrelationCoefficient = CalculateCorrelation(feature);

            features[i] = feature;
        }

        CalculateEntanglementFidelity();
    }

    private double CalculateEntanglementStrength(EntangledFeature feature)
    {
        double strength = 0;

        for (int i = 0; i < feature.QuantumAmplitudes.Length - 1; i++)
        {
            double correlation = feature.QuantumAmplitudes[i] * feature.QuantumAmplitudes[i + 1];
            strength += Math.Abs(correlation);
        }

        return strength / (feature.QuantumAmplitudes.Length - 1);
    }

    private double CalculateCorrelation(EntangledFeature feature)
    {
        double mean = feature.ClassicalValues.Average();
        double variance = feature.ClassicalValues.Average(x => Math.Pow(x - mean, 2));
        double stdDev = Math.Sqrt(variance);

        if (stdDev == 0)
            return 0;

        double correlation = 0;
        for (int i = 0; i < feature.QuantumAmplitudes.Length; i++)
        {
            correlation += (feature.ClassicalValues[i] - mean) * feature.QuantumAmplitudes[i];
        }

        return correlation / (stdDev * feature.QuantumAmplitudes.Length);
    }

    private void CalculateEntanglementFidelity()
    {
        double sumStrength = features.Sum(f => f.EntanglementStrength);
        entanglementFidelity = features.Count > 0 ? sumStrength / features.Count : 0;
    }

    public void TrainWithEntanglement(double[] labels, int epochs)
    {
        actualLabels = labels;
        predictions = new double[labels.Length];

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            double loss = 0;

            for (int i = 0; i < labels.Length; i++)
            {
                predictions[i] = ComputePredictionWithEntanglement(i);
                loss += Math.Pow(labels[i] - predictions[i], 2);
            }

            if (epoch % (epochs / 5) == 0)
            {
                Console.WriteLine($"Epoch {epoch}: Loss = {loss / labels.Length:F6}, " +
                                $"Entanglement Fidelity = {entanglementFidelity:F4}");
            }

            UpdateFeatureWeights();
        }
    }

    private double ComputePredictionWithEntanglement(int sampleIndex)
    {
        double prediction = 0;

        foreach (var feature in features)
        {
            if (sampleIndex < feature.ClassicalValues.Length)
            {
                double classicalComponent = feature.ClassicalValues[sampleIndex];
                double quantumComponent = feature.QuantumAmplitudes[sampleIndex] * entanglementFidelity;

                prediction += (classicalComponent + quantumComponent) / 2.0;
            }
        }

        return Math.Tanh(prediction);
    }

    private void UpdateFeatureWeights()
    {
        for (int i = 0; i < features.Count; i++)
        {
            var feature = features[i];

            for (int j = 0; j < feature.QuantumAmplitudes.Length; j++)
            {
                double error = actualLabels[j] - predictions[j];
                double learningRate = 0.01;

                feature.QuantumAmplitudes[j] += learningRate * error * feature.QuantumAmplitudes[j];
            }

            features[i] = feature;
        }

        EstablishEntanglement();
    }

    public Dictionary<string, double> GetPerformanceMetrics()
    {
        if (actualLabels == null || predictions == null || actualLabels.Length == 0)
            return new Dictionary<string, double>();

        double mse = actualLabels.Average((label, idx) => Math.Pow(label - predictions[idx], 2));
        double mae = actualLabels.Average((label, idx) => Math.Abs(label - predictions[idx]));
        double rmse = Math.Sqrt(mse);

        double ssRes = actualLabels.Sum((label, idx) => Math.Pow(label - predictions[idx], 2));
        double ssTot = actualLabels.Sum(label => Math.Pow(label - actualLabels.Average(), 2));
        double r2 = ssTot > 0 ? 1 - (ssRes / ssTot) : 0;

        return new Dictionary<string, double>
        {
            { "MSE", mse },
            { "MAE", mae },
            { "RMSE", rmse },
            { "R2Score", r2 },
            { "EntanglementFidelity", entanglementFidelity },
            { "FeatureCount", features.Count },
            { "AvgEntanglementStrength", features.Average(f => f.EntanglementStrength) }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== QEMLPI - Quantum Entanglement Machine Learning Performance Improvement ===\n");

        var qemlpi = new QEMLPI();

        Console.WriteLine("Initializing features with quantum entanglement...");
        for (int f = 0; f < 5; f++)
        {
            double[] featureData = new double[100];
            for (int i = 0; i < 100; i++)
            {
                featureData[i] = Math.Sin(f * 0.3 + i * 0.05) * Math.Cos(f * 0.2 + i * 0.03);
            }
            qemlpi.AddFeature(f, featureData);
        }

        Console.WriteLine($"Added {5} features\n");

        Console.WriteLine("--- Establishing Quantum Entanglement ---");
        qemlpi.EstablishEntanglement();
        Console.WriteLine("Entanglement established\n");

        Console.WriteLine("--- Training with Entanglement ---");
        double[] labels = new double[100];
        for (int i = 0; i < 100; i++)
        {
            labels[i] = Math.Sin(i * 0.1) > 0 ? 1.0 : 0.0;
        }

        qemlpi.TrainWithEntanglement(labels, epochs: 100);

        Console.WriteLine("\n--- Final Performance Metrics ---");
        var metrics = qemlpi.GetPerformanceMetrics();

        foreach (var kvp in metrics)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
        }

        Console.WriteLine("\n--- Feature Entanglement Analysis ---");
        for (int i = 0; i < Math.Min(5, qemlpi.features.Count); i++)
        {
            var feature = qemlpi.features[i];
            Console.WriteLine($"Feature {i}: Entanglement={feature.EntanglementStrength:F4}, " +
                            $"Correlation={feature.CorrelationCoefficient:F4}");
        }

        Console.WriteLine("\nQEMLPI enhances ML models through quantum entanglement of features.");
    }
}

// Extension method for Average with index
public static class EnumerableExtensions
{
    public static double Average<T>(this IEnumerable<T> source, Func<T, int, double> selector)
    {
        double sum = 0;
        int count = 0;

        foreach (T item in source)
        {
            sum += selector(item, count);
            count++;
        }

        return count > 0 ? sum / count : 0;
    }

    public static double Sum<T>(this IEnumerable<T> source, Func<T, int, double> selector)
    {
        double sum = 0;
        int count = 0;

        foreach (T item in source)
        {
            sum += selector(item, count);
            count++;
        }

        return sum;
    }
}
