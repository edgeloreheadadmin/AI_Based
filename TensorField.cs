using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tensor Field
/// A field with multiple spin and direction components
/// Rank-2 tensor (4x4 matrix) at each spacetime point
/// </summary>
public class TensorField
{
    private struct Tensor
    {
        public double[,] Components;

        public Tensor()
        {
            Components = new double[4, 4];
        }

        public double Determinant()
        {
            double[,] m = Components;
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

        public double Trace()
        {
            return Components[0, 0] + Components[1, 1] + Components[2, 2] + Components[3, 3];
        }

        public double Norm()
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sum += Components[i, j] * Components[i, j];
                }
            }
            return Math.Sqrt(sum);
        }

        public void Normalize()
        {
            double norm = Norm();
            if (norm > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        Components[i, j] /= norm;
                    }
                }
            }
        }
    }

    private Dictionary<(int, int, int, int), Tensor> fieldTensors;
    private int spatialSize;
    private int temporalSize;
    private Random random;

    public TensorField(int spatialSize = 8, int temporalSize = 10)
    {
        this.spatialSize = spatialSize;
        this.temporalSize = temporalSize;
        this.fieldTensors = new Dictionary<(int, int, int, int), Tensor>();
        this.random = new Random();

        InitializeField();
    }

    private void InitializeField()
    {
        for (int t = 0; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                for (int y = 0; y < spatialSize; y++)
                {
                    for (int z = 0; z < spatialSize; z++)
                    {
                        Tensor tensor = new Tensor();

                        double angle = x * Math.PI / spatialSize + y * Math.PI / spatialSize;
                        double scale = Math.Sin(z * Math.PI / spatialSize);

                        tensor.Components[0, 0] = -1.0;
                        tensor.Components[1, 1] = 1.0;
                        tensor.Components[2, 2] = 1.0;
                        tensor.Components[3, 3] = 1.0;

                        tensor.Components[0, 1] = scale * Math.Sin(angle) * 0.1;
                        tensor.Components[0, 2] = scale * Math.Cos(angle) * 0.1;
                        tensor.Components[0, 3] = scale * 0.1;
                        tensor.Components[1, 0] = tensor.Components[0, 1];
                        tensor.Components[2, 0] = tensor.Components[0, 2];
                        tensor.Components[3, 0] = tensor.Components[0, 3];

                        tensor.Normalize();
                        fieldTensors[(t, x, y, z)] = tensor;
                    }
                }
            }
        }
    }

    public Tensor GetFieldTensor(int t, int x, int y, int z)
    {
        if (fieldTensors.ContainsKey((t, x, y, z)))
            return fieldTensors[(t, x, y, z)];
        return new Tensor();
    }

    public void SetFieldTensor(int t, int x, int y, int z, Tensor tensor)
    {
        tensor.Normalize();
        fieldTensors[(t, x, y, z)] = tensor;
    }

    public double CalculateRicciScalar(int t, int x, int y, int z)
    {
        Tensor tensor = GetFieldTensor(t, x, y, z);
        return tensor.Trace() * tensor.Trace() - tensor.Norm() * tensor.Norm();
    }

    public double CalculateCurvature(int t, int x, int y, int z)
    {
        Tensor center = GetFieldTensor(t, x, y, z);
        Tensor x_plus = GetFieldTensor(t, x + 1, y, z);
        Tensor x_minus = GetFieldTensor(t, x - 1, y, z);

        double curvature = 0;
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                double dx = (x_plus.Components[i, j] - x_minus.Components[i, j]) / 2.0;
                curvature += dx * dx;
            }
        }

        return Math.Sqrt(curvature);
    }

    public void EvolveField(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving tensor field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            var newTensors = new Dictionary<(int, int, int, int), Tensor>(fieldTensors);

            foreach (var kvp in fieldTensors)
            {
                var (t, x, y, z) = kvp.Key;
                Tensor tensor = kvp.Value;

                double curvature = CalculateCurvature(t, x, y, z);
                double ricciScalar = CalculateRicciScalar(t, x, y, z);

                Tensor evolved = new Tensor();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        evolved.Components[i, j] = tensor.Components[i, j] +
                                                  couplingStrength * curvature * ricciScalar * 0.001;

                        if (i == j && i > 0)
                            evolved.Components[i, j] += couplingStrength * 0.01;
                    }
                }
                evolved.Normalize();

                newTensors[(t, x, y, z)] = evolved;
            }

            fieldTensors = newTensors;

            if (step % (steps / 3) == 0)
            {
                double totalCurvature = 0;
                for (int x = 0; x < spatialSize; x++)
                {
                    for (int y = 0; y < spatialSize; y++)
                    {
                        for (int z = 0; z < spatialSize; z++)
                        {
                            totalCurvature += CalculateCurvature(0, x, y, z);
                        }
                    }
                }
                Console.WriteLine($"Step {step}: Total Curvature = {totalCurvature:F6}");
            }
        }
    }

    public Dictionary<string, object> GetFieldMetrics()
    {
        double maxTrace = double.MinValue;
        double minTrace = double.MaxValue;
        double avgNorm = 0;
        double maxDeterminant = double.MinValue;

        foreach (var tensor in fieldTensors.Values)
        {
            maxTrace = Math.Max(maxTrace, tensor.Trace());
            minTrace = Math.Min(minTrace, tensor.Trace());
            avgNorm += tensor.Norm();
            maxDeterminant = Math.Max(maxDeterminant, Math.Abs(tensor.Determinant()));
        }

        avgNorm /= fieldTensors.Count;

        return new Dictionary<string, object>
        {
            { "GridPoints", fieldTensors.Count },
            { "MaxTrace", maxTrace },
            { "MinTrace", minTrace },
            { "AverageNorm", avgNorm },
            { "MaxDeterminant", maxDeterminant }
        };
    }

    public void PrintTensorSlice(int t, int z)
    {
        Console.WriteLine($"\nTensor Field Slice (t={t}, z={z}) - Trace:");
        for (int y = 0; y < spatialSize; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < spatialSize; x++)
            {
                Tensor tensor = GetFieldTensor(t, x, y, z);
                double trace = tensor.Trace();
                char symbol = trace > 0.5 ? '█' : trace > 0 ? '▓' : trace > -0.5 ? '░' : ' ';
                Console.Write(symbol);
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Tensor Field - Multiple Spin and Direction Components ===\n");

        var field = new TensorField(spatialSize: 8, temporalSize: 10);

        Console.WriteLine("--- Initial Field State ---");
        field.PrintTensorSlice(0, 4);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = field.GetFieldMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Tensor Field ---");
        field.EvolveField(steps: 20, couplingStrength: 0.2);

        field.PrintTensorSlice(0, 4);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = field.GetFieldMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nTensor Field represents fields with multiple spin and directional components.");
    }
}
