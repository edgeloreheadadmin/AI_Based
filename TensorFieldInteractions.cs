using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tensor Field Interactions
/// Fields interact through tensor couplings
/// Used to describe gravitational and higher-order interactions
/// </summary>
public class TensorFieldInteractions
{
    private struct TensorField
    {
        public int FieldId;
        public double[,] TensorComponents;
        public double Coupling;
        public int Rank;
        public double Energy;

        public TensorField(int id, int rank = 2)
        {
            FieldId = id;
            Rank = rank;
            int size = rank == 2 ? 4 : (rank == 3 ? 4 : 16);
            TensorComponents = new double[size, size];
            Coupling = 0.0;
            Energy = 0.0;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    TensorComponents[i, j] = Math.Sin(i * 0.1) * Math.Cos(j * 0.1);
                }
            }
        }
    }

    private List<TensorField> fields;
    private double[,] couplingMatrix;
    private Random random;

    public TensorFieldInteractions()
    {
        this.fields = new List<TensorField>();
        this.random = new Random();
    }

    public void CreateTensorField(int rank)
    {
        var field = new TensorField(fields.Count, rank);
        field.Coupling = random.NextDouble() * 0.5;
        fields.Add(field);
        Console.WriteLine($"Created tensor field {field.FieldId} with rank {rank}");
    }

    public void CreateCouplingMatrix()
    {
        couplingMatrix = new double[fields.Count, fields.Count];

        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = 0; j < fields.Count; j++)
            {
                if (i == j)
                {
                    couplingMatrix[i, j] = fields[i].Coupling;
                }
                else
                {
                    double distance = Math.Abs(i - j);
                    couplingMatrix[i, j] = fields[i].Coupling * fields[j].Coupling /
                                          (distance * distance + 1);
                }
            }
        }

        Console.WriteLine("Coupling matrix created");
    }

    public void CalculateTensorContractions()
    {
        foreach (var field in fields)
        {
            double trace = 0;
            int size = field.TensorComponents.GetLength(0);

            for (int i = 0; i < size; i++)
            {
                trace += field.TensorComponents[i, i];
            }

            field.Energy = Math.Abs(trace) * 1e-10;
            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplyTensorCouplings()
    {
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = i + 1; j < fields.Count; j++)
            {
                double coupling = couplingMatrix[i, j];

                for (int a = 0; a < 4; a++)
                {
                    for (int b = 0; b < 4; b++)
                    {
                        fields[i].TensorComponents[a, b] +=
                            coupling * fields[j].TensorComponents[a, b] * 0.1;

                        fields[j].TensorComponents[a, b] +=
                            coupling * fields[i].TensorComponents[a, b] * 0.1;
                    }
                }
            }
        }
    }

    public void EvolveTensorFields(int steps)
    {
        Console.WriteLine($"\nEvolving tensor fields for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            ApplyTensorCouplings();
            CalculateTensorContractions();

            if (step % (steps / 3) == 0)
            {
                double totalEnergy = fields.Sum(f => f.Energy);
                Console.WriteLine($"Step {step}: Total Energy = {totalEnergy:E4}");
            }
        }
    }

    public Dictionary<string, object> GetTensorMetrics()
    {
        double totalEnergy = fields.Sum(f => f.Energy);
        double avgCoupling = fields.Average(f => f.Coupling);
        int totalRank = fields.Sum(f => f.Rank);

        return new Dictionary<string, object>
        {
            { "FieldCount", fields.Count },
            { "TotalEnergy", totalEnergy },
            { "AverageCoupling", avgCoupling },
            { "TotalRank", totalRank }
        };
    }

    public void PrintTensorComponents(int fieldId)
    {
        if (fieldId >= fields.Count)
            return;

        Console.WriteLine($"\nTensor Field {fieldId} Components (4x4):");
        for (int i = 0; i < 4; i++)
        {
            Console.Write("  ");
            for (int j = 0; j < 4; j++)
            {
                Console.Write($"{fields[fieldId].TensorComponents[i, j]:F3} ");
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Tensor Field Interactions ===\n");

        var tfi = new TensorFieldInteractions();

        Console.WriteLine("--- Creating Tensor Fields ---");
        tfi.CreateTensorField(rank: 2);
        tfi.CreateTensorField(rank: 2);
        tfi.CreateTensorField(rank: 2);

        Console.WriteLine("\n--- Creating Coupling Matrix ---");
        tfi.CreateCouplingMatrix();

        Console.WriteLine("\n--- Initial Tensor Components ---");
        tfi.PrintTensorComponents(0);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = tfi.GetTensorMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Tensor Fields ---");
        tfi.EvolveTensorFields(steps: 30);

        Console.WriteLine("\n--- Final Tensor Components ---");
        tfi.PrintTensorComponents(0);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = tfi.GetTensorMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nTensor Field Interactions model gravitational coupling.");
    }
}
