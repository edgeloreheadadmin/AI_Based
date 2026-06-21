using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// QPAGA - Quantum Parallel Algorithm for Acceleration and Gradient Aggregation
/// Exploits parallelism of quantum techniques to perform calculations simultaneously
/// Simulates parallel quantum computations with distributed gradient aggregation
/// </summary>
public class QPAGA
{
    private struct QuantumTask
    {
        public int TaskId;
        public double[] DataSet;
        public double[] Gradients;
        public double AccumulatedCost;
        public bool Completed;

        public QuantumTask(int id, double[] data)
        {
            TaskId = id;
            DataSet = data;
            Gradients = new double[data.Length];
            AccumulatedCost = 0;
            Completed = false;
        }
    }

    private List<QuantumTask> parallelTasks;
    private double[] aggregatedGradients;
    private int maxParallelTasks;
    private Random random;
    private double learningRate;

    public QPAGA(int maxParallelTasks = 4, double learningRate = 0.01)
    {
        this.maxParallelTasks = maxParallelTasks;
        this.learningRate = learningRate;
        this.parallelTasks = new List<QuantumTask>();
        this.random = new Random();
    }

    public void AddTask(double[] dataset)
    {
        var task = new QuantumTask(parallelTasks.Count, dataset);
        aggregatedGradients = new double[dataset.Length];
        parallelTasks.Add(task);
    }

    public void ExecuteParallelComputation()
    {
        int batchSize = Math.Min(maxParallelTasks, parallelTasks.Count);
        var tasks = new Task[batchSize];

        for (int i = 0; i < batchSize; i++)
        {
            int taskIndex = i;
            tasks[i] = Task.Run(() => ExecuteQuantumTask(taskIndex));
        }

        Task.WaitAll(tasks);
    }

    private void ExecuteQuantumTask(int taskIndex)
    {
        if (taskIndex >= parallelTasks.Count)
            return;

        var task = parallelTasks[taskIndex];

        for (int i = 0; i < task.DataSet.Length; i++)
        {
            double perturbation = (random.NextDouble() - 0.5) * 0.01;
            task.Gradients[i] = task.DataSet[i] * Math.Sin(task.DataSet[i] + perturbation);
            task.AccumulatedCost += Math.Pow(task.Gradients[i], 2);
        }

        lock (aggregatedGradients)
        {
            for (int i = 0; i < task.Gradients.Length; i++)
            {
                aggregatedGradients[i] += task.Gradients[i];
            }
        }

        task.Completed = true;
        parallelTasks[taskIndex] = task;
    }

    public void AggregateGradients()
    {
        int completedTasks = 0;
        foreach (var task in parallelTasks)
        {
            if (task.Completed)
                completedTasks++;
        }

        if (completedTasks > 0)
        {
            for (int i = 0; i < aggregatedGradients.Length; i++)
            {
                aggregatedGradients[i] /= completedTasks;
            }
        }
    }

    public void UpdateWeights(double[] weights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] -= learningRate * aggregatedGradients[i];
        }
    }

    public Dictionary<string, object> GetComputationMetrics()
    {
        double totalCost = 0;
        int completedCount = 0;

        foreach (var task in parallelTasks)
        {
            if (task.Completed)
            {
                totalCost += task.AccumulatedCost;
                completedCount++;
            }
        }

        double avgCost = completedCount > 0 ? totalCost / completedCount : 0;

        return new Dictionary<string, object>
        {
            { "TotalTasks", parallelTasks.Count },
            { "CompletedTasks", completedCount },
            { "MaxParallelism", maxParallelTasks },
            { "AverageCost", avgCost },
            { "GradientNorm", CalculateGradientNorm() },
            { "LearningRate", learningRate }
        };
    }

    private double CalculateGradientNorm()
    {
        double norm = 0;
        foreach (double grad in aggregatedGradients)
        {
            norm += grad * grad;
        }
        return Math.Sqrt(norm);
    }

    public static void Main()
    {
        Console.WriteLine("=== QPAGA - Quantum Parallel Acceleration & Gradient Aggregation ===\n");

        var qpaga = new QPAGA(maxParallelTasks: 4, learningRate: 0.01);

        Console.WriteLine("Creating parallel quantum tasks...");
        for (int i = 0; i < 8; i++)
        {
            double[] dataset = new double[5];
            for (int j = 0; j < dataset.Length; j++)
            {
                dataset[j] = (i * 0.1 + j * 0.05) % 1.0;
            }
            qpaga.AddTask(dataset);
        }

        Console.WriteLine($"Total Tasks: 8, Max Parallel: 4\n");

        Console.WriteLine("--- Executing Parallel Computation (Batch 1) ---");
        qpaga.ExecuteParallelComputation();
        qpaga.AggregateGradients();

        var metrics1 = qpaga.GetComputationMetrics();
        Console.WriteLine("Metrics after Batch 1:");
        foreach (var kvp in metrics1)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Simulating Weight Updates ---");
        double[] weights = new double[5];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = random.NextDouble();
        }

        Console.WriteLine("Weights Before Update:");
        for (int i = 0; i < weights.Length; i++)
            Console.WriteLine($"  W{i}: {weights[i]:F4}");

        qpaga.UpdateWeights(weights);

        Console.WriteLine("\nWeights After Update:");
        for (int i = 0; i < weights.Length; i++)
            Console.WriteLine($"  W{i}: {weights[i]:F4}");

        Console.WriteLine("\n--- Second Parallel Execution ---");
        qpaga.ExecuteParallelComputation();
        qpaga.AggregateGradients();

        var metrics2 = qpaga.GetComputationMetrics();
        Console.WriteLine("Metrics after Batch 2:");
        foreach (var kvp in metrics2)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQPAGA demonstrates parallel quantum computation with distributed gradient aggregation.");

        var random = new Random();
    }
}
