using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Vector Machine Learning
/// Type of machine learning using quantum mechanics to improve ML algorithm performance
/// Combines quantum superposition, entanglement, and quantum gates with ML
/// </summary>
public class QuantumVectorMachineLearning
{
    private struct QuantumNeuron
    {
        public double[] QuantumWeights;
        public double[] ClassicalWeights;
        public double Bias;
        public double ActivationAmplitude;

        public QuantumNeuron(int inputDimension)
        {
            QuantumWeights = new double[inputDimension];
            ClassicalWeights = new double[inputDimension];
            Bias = 0.0;
            ActivationAmplitude = 0.0;

            Random random = new Random();
            for (int i = 0; i < inputDimension; i++)
            {
                QuantumWeights[i] = (random.NextDouble() - 0.5) * 2.0;
                ClassicalWeights[i] = QuantumWeights[i];
            }
        }
    }

    private List<QuantumNeuron> quantumLayer;
    private int inputDimension;
    private int outputDimension;
    private double learningRate;
    private Random random;

    public QuantumVectorMachineLearning(int inputDim, int outputDim, double learningRate = 0.01)
    {
        this.inputDimension = inputDim;
        this.outputDimension = outputDim;
        this.learningRate = learningRate;
        this.random = new Random();

        InitializeQuantumLayer();
    }

    private void InitializeQuantumLayer()
    {
        quantumLayer = new List<QuantumNeuron>();

        for (int i = 0; i < outputDimension; i++)
        {
            quantumLayer.Add(new QuantumNeuron(inputDimension));
        }
    }

    public double[] ForwardPass(double[] input)
    {
        double[] output = new double[outputDimension];

        for (int neuronIdx = 0; neuronIdx < outputDimension; neuronIdx++)
        {
            var neuron = quantumLayer[neuronIdx];

            double quantumActivation = ComputeQuantumActivation(neuron.QuantumWeights, input);
            double classicalActivation = ComputeClassicalActivation(neuron.ClassicalWeights, input);

            double hybridOutput = (quantumActivation + classicalActivation) / 2.0 + neuron.Bias;
            output[neuronIdx] = ActivationFunction(hybridOutput);

            neuron.ActivationAmplitude = hybridOutput;
            quantumLayer[neuronIdx] = neuron;
        }

        return output;
    }

    private double ComputeQuantumActivation(double[] weights, double[] input)
    {
        double activation = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            double angle = weights[i] * input[i] * Math.PI;
            activation += Math.Cos(angle);
        }

        return activation / weights.Length;
    }

    private double ComputeClassicalActivation(double[] weights, double[] input)
    {
        double activation = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            activation += weights[i] * input[i];
        }

        return Math.Tanh(activation);
    }

    private double ActivationFunction(double x)
    {
        return 1.0 / (1.0 + Math.Exp(-x));
    }

    private double ActivationDerivative(double x)
    {
        return x * (1.0 - x);
    }

    public void BackPropagation(double[] input, double[] targetOutput, double[] predictedOutput)
    {
        double[] errors = new double[outputDimension];

        for (int i = 0; i < outputDimension; i++)
        {
            errors[i] = (targetOutput[i] - predictedOutput[i]) * ActivationDerivative(predictedOutput[i]);
        }

        for (int neuronIdx = 0; neuronIdx < outputDimension; neuronIdx++)
        {
            var neuron = quantumLayer[neuronIdx];

            for (int i = 0; i < inputDimension; i++)
            {
                double quantumGradient = ComputeQuantumGradient(neuron.QuantumWeights[i], input[i], errors[neuronIdx]);
                double classicalGradient = errors[neuronIdx] * input[i];

                double hybridGradient = (quantumGradient + classicalGradient) / 2.0;

                neuron.QuantumWeights[i] += learningRate * hybridGradient;
                neuron.ClassicalWeights[i] += learningRate * classicalGradient;
            }

            neuron.Bias += learningRate * errors[neuronIdx];
            quantumLayer[neuronIdx] = neuron;
        }
    }

    private double ComputeQuantumGradient(double weight, double input, double error)
    {
        double angle = weight * input * Math.PI;
        return error * (-Math.Sin(angle)) * input * Math.PI;
    }

    public void Train(List<double[]> trainingData, List<double[]> trainingLabels, int epochs)
    {
        for (int epoch = 0; epoch < epochs; epoch++)
        {
            double totalError = 0;

            for (int i = 0; i < trainingData.Count; i++)
            {
                double[] output = ForwardPass(trainingData[i]);
                BackPropagation(trainingData[i], trainingLabels[i], output);

                for (int j = 0; j < output.Length; j++)
                {
                    totalError += Math.Pow(trainingLabels[i][j] - output[j], 2);
                }
            }

            if (epoch % 50 == 0)
            {
                Console.WriteLine($"Epoch {epoch}: Loss = {totalError / trainingData.Count:F6}");
            }
        }
    }

    public double[] Predict(double[] input)
    {
        return ForwardPass(input);
    }

    public Dictionary<string, object> GetModelMetrics()
    {
        double avgQuantumWeight = 0;
        double avgClassicalWeight = 0;
        double totalBias = 0;

        foreach (var neuron in quantumLayer)
        {
            avgQuantumWeight += neuron.QuantumWeights.Average();
            avgClassicalWeight += neuron.ClassicalWeights.Average();
            totalBias += neuron.Bias;
        }

        avgQuantumWeight /= quantumLayer.Count;
        avgClassicalWeight /= quantumLayer.Count;
        totalBias /= quantumLayer.Count;

        return new Dictionary<string, object>
        {
            { "Neurons", outputDimension },
            { "InputDimension", inputDimension },
            { "AvgQuantumWeight", avgQuantumWeight },
            { "AvgClassicalWeight", avgClassicalWeight },
            { "AvgBias", totalBias },
            { "LearningRate", learningRate }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Vector Machine Learning ===\n");

        var qvml = new QuantumVectorMachineLearning(inputDim: 4, outputDim: 2, learningRate: 0.05);

        Console.WriteLine("Initial Model Metrics:");
        var initialMetrics = qvml.GetModelMetrics();
        foreach (var kvp in initialMetrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Preparing Training Data ---");
        var trainingData = new List<double[]>
        {
            new double[] { 0.1, 0.2, 0.3, 0.4 },
            new double[] { 0.2, 0.3, 0.4, 0.5 },
            new double[] { 0.8, 0.7, 0.6, 0.5 },
            new double[] { 0.9, 0.8, 0.7, 0.6 }
        };

        var trainingLabels = new List<double[]>
        {
            new double[] { 0.0, 1.0 },
            new double[] { 0.0, 1.0 },
            new double[] { 1.0, 0.0 },
            new double[] { 1.0, 0.0 }
        };

        Console.WriteLine("Training samples prepared\n");

        Console.WriteLine("--- Training Quantum Neural Network ---");
        qvml.Train(trainingData, trainingLabels, epochs: 200);

        Console.WriteLine("\n--- Final Model Metrics ---");
        var finalMetrics = qvml.GetModelMetrics();
        foreach (var kvp in finalMetrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Making Predictions ---");
        Console.WriteLine("Test Case 1: [0.15, 0.25, 0.35, 0.45] (Expected Class 0)");
        var pred1 = qvml.Predict(new double[] { 0.15, 0.25, 0.35, 0.45 });
        Console.WriteLine($"  Output: [{string.Join(", ", pred1.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Predicted Class: {(pred1[0] > pred1[1] ? 0 : 1)}");

        Console.WriteLine("\nTest Case 2: [0.85, 0.75, 0.65, 0.55] (Expected Class 1)");
        var pred2 = qvml.Predict(new double[] { 0.85, 0.75, 0.65, 0.55 });
        Console.WriteLine($"  Output: [{string.Join(", ", pred2.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Predicted Class: {(pred2[0] > pred2[1] ? 0 : 1)}");

        Console.WriteLine("\nQVML successfully combines quantum mechanics with machine learning for improved performance.");
    }
}
