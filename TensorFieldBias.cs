using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tensor Field Bias
/// Bias towards creation of tensor fields with multiple spin and direction components
/// Preferentially generates and evolves rank-2 tensor structures
/// </summary>
public class TensorFieldBias
{
    private struct TensorElement
    {
        public double[,] Component;

        public TensorElement()
        {
            Component = new double[4, 4];
        }

        public double Determinant()
        {
            double[,] m = Component;
            return m[0, 0] * (m[1, 1] * (m[2, 2] * m[3, 3] - m[2, 3] * m[3, 2]) -
                              m[1, 2] * (m[2, 1] * m[3, 3] - m[2, 3] * m[3, 1]) +
                              m[1, 3] * (m[2, 1] * m[3, 2] - m[2, 2] * m[3, 1])) -
                   m[0, 1] * (m[1, 0] * (m[2, 2] * m[3, 3] - m[2, 3] * m[3, 2]) -
                              m[1, 2] * (m[2, 0] * m[3, 3] - m[2, 3] * m[3, 0]) +
                              m[1, 3] * (m[2, 0] * m[3, 2] - m[2, 2] * m[3, 0])) +
                   m[0, 2] * (m[1, 0] * (m[2, 1] * m[3, 3] - m[2, 3] * m[3, 1]) -
                              m[1, 1] * (m[2, 0] * m[3, 3] - m[2, 3] * m[3, 0]) +
                              m[1, 3] * (m[2, 0] * m[3, 1] - m[2, 1] * m[3, 0])) -
                   m[0, 3] * (m[1, 0] * (m[2, 1] * m[3, 2] - m[2, 2] * m[3, 1]) -
                              m[1, 1] * (m[2, 0] * m[3, 2] - m[2, 2] * m[3, 0]) +
                              m[1, 2] * (m[2, 0] * m[3, 1] - m[2, 1] * m[3, 0]));
        }

        public double Norm()
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sum += Component[i, j] * Component[i, j];
                }
            }
            return Math.Sqrt(sum);
        }

        public int GetComponentCount()
        {
            return 16;
        }
    }

    private List<TensorElement> tensorFields;
    private int fieldCount;
    private Random random;

    public TensorFieldBias(int fieldCount = 15)
    {
        this.fieldCount = fieldCount;
        this.tensorFields = new List<TensorElement>();
        this.random = new Random();

        InitializeTensorFields();
    }

    private void InitializeTensorFields()
    {
        for (int i = 0; i < fieldCount; i++)
        {
            TensorElement tensor = new TensorElement();

            tensor.Component[0, 0] = -1.0;
            tensor.Component[1, 1] = 1.0;
            tensor.Component[2, 2] = 1.0;
            tensor.Component[3, 3] = 1.0;

            for (int j = 0; j < 4; j++)
            {
                for (int k = j + 1; k < 4; k++)
                {
                    double value = random.NextDouble() * 0.1;
                    tensor.Component[j, k] = value;
                    tensor.Component[k, j] = value;
                }
            }

            tensorFields.Add(tensor);
        }
    }

    public void GenerateTensorFieldsPreferentially(int count)
    {
        Console.WriteLine($"\nGenerating {count} tensor fields preferentially...\n");

        for (int i = 0; i < count; i++)
        {
            TensorElement tensor = new TensorElement();

            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    tensor.Component[j, k] = (random.NextDouble() - 0.5) * 2.0;
                }
            }

            tensorFields.Add(tensor);
        }

        Console.WriteLine($"Created {count} additional tensor fields. Total: {tensorFields.Count}");
    }

    public double MeasureTensorCharacter()
    {
        if (tensorFields.Count == 0)
            return 0.0;

        double totalTensorScore = 0.0;

        foreach (var tensor in tensorFields)
        {
            double norm = tensor.Norm();
            int componentCount = tensor.GetComponentCount();

            double tensorScore = norm * componentCount / 16.0;
            totalTensorScore += tensorScore;
        }

        return totalTensorScore / tensorFields.Count;
    }

    public void EnforceTensorStructure()
    {
        Console.WriteLine("\nEnforcing tensor structure on all fields...\n");

        for (int idx = 0; idx < tensorFields.Count; idx++)
        {
            TensorElement tensor = tensorFields[idx];

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    tensor.Component[i, j] *= Math.Sqrt(tensor.Norm());
                }
            }

            tensorFields[idx] = tensor;
        }

        Console.WriteLine("Tensor structure enforcement complete.");
    }

    public void EvolveWithTensorBias(int steps, double biasMagnitude)
    {
        Console.WriteLine($"\nEvolving tensor fields with tensor bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            for (int idx = 0; idx < tensorFields.Count; idx++)
            {
                TensorElement tensor = tensorFields[idx];
                double tensorCharacter = MeasureTensorCharacter();

                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        double perturbation = (random.NextDouble() - 0.5) * 0.1;
                        tensor.Component[i, j] += biasMagnitude * tensorCharacter * perturbation;
                    }
                }

                tensorFields[idx] = tensor;
            }

            if (step % (steps / 3) == 0)
            {
                double tensorCharacter = MeasureTensorCharacter();
                Console.WriteLine($"Step {step}: Tensor Character Score = {tensorCharacter:F6}");
            }
        }
    }

    public double CalculateMultiComponentDensity()
    {
        double totalComponentDensity = 0.0;

        foreach (var tensor in tensorFields)
        {
            totalComponentDensity += tensor.GetComponentCount() * tensor.Norm();
        }

        return tensorFields.Count > 0 ? totalComponentDensity / tensorFields.Count : 0.0;
    }

    public Dictionary<string, object> GetTensorBiasMetrics()
    {
        double tensorCharacter = MeasureTensorCharacter();
        double componentDensity = CalculateMultiComponentDensity();

        double maxDeterminant = 0.0;
        foreach (var tensor in tensorFields)
        {
            maxDeterminant = Math.Max(maxDeterminant, Math.Abs(tensor.Determinant()));
        }

        return new Dictionary<string, object>
        {
            { "TensorFieldCount", tensorFields.Count },
            { "TensorCharacter", tensorCharacter },
            { "ComponentDensity", componentDensity },
            { "MaxDeterminant", maxDeterminant },
            { "ComponentsPerField", 16 }
        };
    }

    public void PrintTensorSummary(int limit = 5)
    {
        Console.WriteLine("\nTensor Field Summary:");
        for (int i = 0; i < Math.Min(limit, tensorFields.Count); i++)
        {
            var tensor = tensorFields[i];
            Console.WriteLine($"  Tensor {i}: Norm={tensor.Norm():F4}, Determinant={tensor.Determinant():F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Tensor Field Bias - Preferential Tensor Creation ===\n");

        var bias = new TensorFieldBias(fieldCount: 15);

        Console.WriteLine("--- Initial Tensor Fields ---");
        bias.PrintTensorSummary(5);

        Console.WriteLine("\n--- Initial Tensor Bias Metrics ---");
        var metricsInitial = bias.GetTensorBiasMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Generating Additional Tensor Fields ---");
        bias.GenerateTensorFieldsPreferentially(count: 10);

        Console.WriteLine("\n--- Enforcing Tensor Structure ---");
        bias.EnforceTensorStructure();

        Console.WriteLine("\n--- Evolving with Tensor Bias ---");
        bias.EvolveWithTensorBias(steps: 20, biasMagnitude: 0.4);

        bias.PrintTensorSummary(8);

        Console.WriteLine("\n--- Final Tensor Bias Metrics ---");
        var metricsFinal = bias.GetTensorBiasMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nTensor Field Bias preferentially creates fields with multiple spin/direction components.");
    }
}
