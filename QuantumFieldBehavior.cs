using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Field Behavior
/// Fields are quantized and can only exist in discrete states
/// Demonstrates quantization consequences on field dynamics
/// </summary>
public class QuantumFieldBehavior
{
    private struct QuantumField
    {
        public int FieldId;
        public double Position;
        public int[] DiscreteStates;
        public double[] AmplitudeAtStates;
        public double CurrentEnergy;
        public int CurrentQuantumNumber;

        public QuantumField(int id, int numStates = 5)
        {
            FieldId = id;
            Position = 0.0;
            DiscreteStates = new int[numStates];
            AmplitudeAtStates = new double[numStates];
            CurrentEnergy = 0.0;
            CurrentQuantumNumber = 0;

            for (int i = 0; i < numStates; i++)
            {
                DiscreteStates[i] = i;
                AmplitudeAtStates[i] = 1.0 / Math.Sqrt(numStates);
            }
        }
    }

    private List<QuantumField> fields;
    private double[,] spaceTimeGrid;
    private int gridSize;
    private Random random;
    private double planckConstant;
    private double reducedPlanck;

    public QuantumFieldBehavior(int gridSize = 32)
    {
        this.fields = new List<QuantumField>();
        this.gridSize = gridSize;
        this.spaceTimeGrid = new double[gridSize, gridSize];
        this.random = new Random();
        this.planckConstant = 6.626e-34;
        this.reducedPlanck = 1.054e-34;

        InitializeFields();
    }

    private void InitializeFields()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                spaceTimeGrid[i, j] = 0.0;
            }
        }
    }

    public void CreateQuantumField(int fieldId)
    {
        var field = new QuantumField(fieldId, numStates: 8);
        fields.Add(field);
    }

    public void CreateMultipleFields(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateQuantumField(i);
        }
        Console.WriteLine($"Created {count} quantum fields");
    }

    public void ExciteField(int fieldId, int targetState)
    {
        if (fieldId >= fields.Count || targetState >= fields[fieldId].DiscreteStates.Length)
            return;

        var field = fields[fieldId];
        field.CurrentQuantumNumber = targetState;
        field.CurrentEnergy = CalculateFieldEnergy(targetState);

        for (int i = 0; i < field.AmplitudeAtStates.Length; i++)
        {
            field.AmplitudeAtStates[i] = (i == targetState) ? 1.0 : 0.0;
        }

        fields[fieldId] = field;
    }

    private double CalculateFieldEnergy(int quantumNumber)
    {
        return (quantumNumber + 0.5) * reducedPlanck * 1e15;
    }

    public void ApplyQuantumTransition()
    {
        foreach (var field in fields)
        {
            int stateCount = field.DiscreteStates.Length;
            double[] transitionAmplitudes = new double[stateCount];

            for (int i = 0; i < stateCount; i++)
            {
                transitionAmplitudes[i] = 0;
                for (int j = 0; j < stateCount; j++)
                {
                    double couplingStrength = CalculateCoupling(i, j);
                    transitionAmplitudes[i] += field.AmplitudeAtStates[j] * couplingStrength;
                }
            }

            double sum = transitionAmplitudes.Sum(x => x * x);
            for (int i = 0; i < stateCount; i++)
            {
                transitionAmplitudes[i] = Math.Sqrt(Math.Abs(transitionAmplitudes[i]) / (sum + 1e-10));
            }

            field.AmplitudeAtStates = transitionAmplitudes;
            int newState = FindMostProbableState(transitionAmplitudes);
            field.CurrentQuantumNumber = newState;
            field.CurrentEnergy = CalculateFieldEnergy(newState);
        }
    }

    private double CalculateCoupling(int state1, int state2)
    {
        if (Math.Abs(state1 - state2) == 1)
            return Math.Sqrt(Math.Min(state1, state2) + 1) * 0.1;
        return 0.0;
    }

    private int FindMostProbableState(double[] amplitudes)
    {
        double maxProb = 0;
        int maxState = 0;
        for (int i = 0; i < amplitudes.Length; i++)
        {
            double prob = amplitudes[i] * amplitudes[i];
            if (prob > maxProb)
            {
                maxProb = prob;
                maxState = i;
            }
        }
        return maxState;
    }

    public void EvolveFieldsInTime(int steps)
    {
        for (int step = 0; step < steps; step++)
        {
            ApplyQuantumTransition();
            PropagateFieldInSpaceTime();

            if (step % 10 == 0)
            {
                Console.WriteLine($"Step {step}: Fields evolved, Average Energy = " +
                                $"{fields.Average(f => f.CurrentEnergy):E4}");
            }
        }
    }

    private void PropagateFieldInSpaceTime()
    {
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                double contribution = 0;
                foreach (var field in fields)
                {
                    double distance = Math.Sqrt(Math.Pow(i - gridSize / 2, 2) +
                                               Math.Pow(j - gridSize / 2, 2));
                    contribution += field.CurrentEnergy * Math.Exp(-distance * 0.1);
                }
                spaceTimeGrid[i, j] = contribution;
            }
        }
    }

    public Dictionary<string, object> GetFieldMetrics()
    {
        double totalEnergy = fields.Sum(f => f.CurrentEnergy);
        double avgEnergy = fields.Average(f => f.CurrentEnergy);
        double maxEnergy = fields.Max(f => f.CurrentEnergy);
        int totalQuantumNumber = fields.Sum(f => f.CurrentQuantumNumber);

        return new Dictionary<string, object>
        {
            { "FieldCount", fields.Count },
            { "TotalEnergy", totalEnergy },
            { "AverageEnergy", avgEnergy },
            { "MaxEnergy", maxEnergy },
            { "TotalQuantumNumber", totalQuantumNumber },
            { "AvgQuantumNumber", fields.Average(f => f.CurrentQuantumNumber) }
        };
    }

    public void PrintFieldConfiguration()
    {
        Console.WriteLine("Quantum Field Configuration:");
        for (int i = 0; i < Math.Min(5, fields.Count); i++)
        {
            var field = fields[i];
            Console.WriteLine($"  Field {i}: State={field.CurrentQuantumNumber}, " +
                            $"Energy={field.CurrentEnergy:E4}");
        }
    }

    public void PrintSpaceTimeSnapshot()
    {
        Console.WriteLine("Space-Time Field Snapshot:");
        for (int i = 0; i < Math.Min(10, gridSize); i++)
        {
            Console.Write("  ");
            for (int j = 0; j < Math.Min(10, gridSize); j++)
            {
                double val = spaceTimeGrid[i, j];
                if (val < 1e-20)
                    Console.Write(". ");
                else if (val < 1e-10)
                    Console.Write("+ ");
                else if (val < 1e-5)
                    Console.Write("* ");
                else
                    Console.Write("# ");
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Field Behavior - Discretized States ===\n");

        var qfb = new QuantumFieldBehavior(gridSize: 32);

        Console.WriteLine("--- Creating Quantum Fields ---");
        qfb.CreateMultipleFields(10);

        Console.WriteLine("\n--- Exciting Fields to Different States ---");
        for (int i = 0; i < 5; i++)
        {
            qfb.ExciteField(i, i % 8);
        }

        qfb.PrintFieldConfiguration();

        Console.WriteLine("\n--- Initial Field Metrics ---");
        var metricsInitial = qfb.GetFieldMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Fields in Time ---");
        qfb.EvolveFieldsInTime(50);

        Console.WriteLine("\n--- Final Field Configuration ---");
        qfb.PrintFieldConfiguration();

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = qfb.GetFieldMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Space-Time Field Visualization ---");
        qfb.PrintSpaceTimeSnapshot();

        Console.WriteLine("\nQuantum Field Behavior demonstrates discrete quantization states.");
    }
}
