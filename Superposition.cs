using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Superposition
/// Fundamental quantum principle allowing a system to exist in multiple states simultaneously
/// until measurement collapses the state to a definite eigenstate
/// </summary>
public class Superposition
{
    private struct QuantumState
    {
        public double[] AmplitudesReal;
        public double[] AmplitudesImag;
        public int QubitCount;

        public QuantumState(int qubitCount)
        {
            QubitCount = qubitCount;
            int stateCount = 1 << qubitCount;
            AmplitudesReal = new double[stateCount];
            AmplitudesImag = new double[stateCount];
        }

        public double GetProbability(int basisState)
        {
            if (basisState < 0 || basisState >= AmplitudesReal.Length)
                return 0.0;

            return AmplitudesReal[basisState] * AmplitudesReal[basisState] +
                   AmplitudesImag[basisState] * AmplitudesImag[basisState];
        }

        public double GetNorm()
        {
            double sum = 0.0;
            for (int i = 0; i < AmplitudesReal.Length; i++)
            {
                sum += GetProbability(i);
            }
            return Math.Sqrt(sum);
        }

        public void Normalize()
        {
            double norm = GetNorm();
            if (norm > 0)
            {
                for (int i = 0; i < AmplitudesReal.Length; i++)
                {
                    AmplitudesReal[i] /= norm;
                    AmplitudesImag[i] /= norm;
                }
            }
        }
    }

    private List<QuantumState> superpositions;
    private Random random;

    public Superposition(int qubitCount = 2)
    {
        this.superpositions = new List<QuantumState>();
        this.random = new Random();

        InitializeSuperposition(qubitCount);
    }

    private void InitializeSuperposition(int qubitCount)
    {
        QuantumState state = new QuantumState(qubitCount);
        int stateCount = 1 << qubitCount;

        for (int i = 0; i < stateCount; i++)
        {
            double phase = random.NextDouble() * 2 * Math.PI;
            double magnitude = 1.0 / Math.Sqrt(stateCount);

            state.AmplitudesReal[i] = magnitude * Math.Cos(phase);
            state.AmplitudesImag[i] = magnitude * Math.Sin(phase);
        }

        state.Normalize();
        superpositions.Add(state);
    }

    public void CreateEqualSuperposition(int qubitCount)
    {
        Console.WriteLine($"\nCreating equal superposition of {qubitCount} qubits...\n");

        QuantumState state = new QuantumState(qubitCount);
        int stateCount = 1 << qubitCount;
        double amplitude = 1.0 / Math.Sqrt(stateCount);

        for (int i = 0; i < stateCount; i++)
        {
            state.AmplitudesReal[i] = amplitude;
            state.AmplitudesImag[i] = 0.0;
        }

        superpositions.Add(state);
    }

    public void ApplyPhase(int superpositionIndex, double phase)
    {
        if (superpositionIndex < 0 || superpositionIndex >= superpositions.Count)
            return;

        QuantumState state = superpositions[superpositionIndex];

        for (int i = 0; i < state.AmplitudesReal.Length; i++)
        {
            double realPart = state.AmplitudesReal[i];
            double imagPart = state.AmplitudesImag[i];

            double cosPhase = Math.Cos(phase);
            double sinPhase = Math.Sin(phase);

            state.AmplitudesReal[i] = realPart * cosPhase - imagPart * sinPhase;
            state.AmplitudesImag[i] = realPart * sinPhase + imagPart * cosPhase;
        }

        state.Normalize();
        superpositions[superpositionIndex] = state;
    }

    public int MeasureSuperposition(int superpositionIndex)
    {
        if (superpositionIndex < 0 || superpositionIndex >= superpositions.Count)
            return -1;

        QuantumState state = superpositions[superpositionIndex];
        double randomValue = random.NextDouble();
        double cumulativeProbability = 0.0;

        for (int i = 0; i < state.AmplitudesReal.Length; i++)
        {
            cumulativeProbability += state.GetProbability(i);
            if (randomValue <= cumulativeProbability)
            {
                return i;
            }
        }

        return state.AmplitudesReal.Length - 1;
    }

    public void EvolveSuperposition(int superpositionIndex, int steps, double evolutionRate)
    {
        Console.WriteLine($"\nEvolving superposition for {steps} steps...\n");

        if (superpositionIndex < 0 || superpositionIndex >= superpositions.Count)
            return;

        for (int step = 0; step < steps; step++)
        {
            QuantumState state = superpositions[superpositionIndex];

            for (int i = 0; i < state.AmplitudesReal.Length; i++)
            {
                double phase = evolutionRate * step * (i + 1) * Math.PI / state.AmplitudesReal.Length;

                double realPart = state.AmplitudesReal[i];
                double imagPart = state.AmplitudesImag[i];

                double cosPhase = Math.Cos(phase);
                double sinPhase = Math.Sin(phase);

                state.AmplitudesReal[i] = realPart * cosPhase - imagPart * sinPhase;
                state.AmplitudesImag[i] = realPart * sinPhase + imagPart * cosPhase;
            }

            state.Normalize();
            superpositions[superpositionIndex] = state;

            if (step % (steps / 3) == 0)
            {
                double entropy = CalculateEntanglement(superpositionIndex);
                Console.WriteLine($"Step {step}: Entanglement Entropy = {entropy:F6}");
            }
        }
    }

    public double CalculateEntanglement(int superpositionIndex)
    {
        if (superpositionIndex < 0 || superpositionIndex >= superpositions.Count)
            return 0.0;

        QuantumState state = superpositions[superpositionIndex];
        double entropy = 0.0;

        for (int i = 0; i < state.AmplitudesReal.Length; i++)
        {
            double probability = state.GetProbability(i);
            if (probability > 0)
            {
                entropy -= probability * Math.Log(probability) / Math.Log(2);
            }
        }

        return entropy;
    }

    public void PrintSuperpositionState(int superpositionIndex, int maxStates = 8)
    {
        if (superpositionIndex < 0 || superpositionIndex >= superpositions.Count)
            return;

        QuantumState state = superpositions[superpositionIndex];
        Console.WriteLine($"\nSuperposition State (showing first {maxStates} of {state.AmplitudesReal.Length}):");

        for (int i = 0; i < Math.Min(maxStates, state.AmplitudesReal.Length); i++)
        {
            double probability = state.GetProbability(i);
            double phase = Math.Atan2(state.AmplitudesImag[i], state.AmplitudesReal[i]);

            Console.Write($"  |{i:D2}⟩: ");
            int barLength = (int)(probability * 40);
            for (int k = 0; k < barLength; k++)
                Console.Write("█");
            Console.WriteLine($" P={probability:F4} φ={phase:F3}");
        }
    }

    public Dictionary<string, object> GetSuperpositionMetrics()
    {
        double totalEntanglement = 0.0;
        double maxEntanglement = 0.0;

        foreach (var superposition in superpositions)
        {
            double entropy = 0.0;
            for (int i = 0; i < superposition.AmplitudesReal.Length; i++)
            {
                double prob = superposition.GetProbability(i);
                if (prob > 0)
                    entropy -= prob * Math.Log(prob) / Math.Log(2);
            }
            totalEntanglement += entropy;
            maxEntanglement = Math.Max(maxEntanglement, entropy);
        }

        return new Dictionary<string, object>
        {
            { "SuperpositionCount", superpositions.Count },
            { "AverageEntanglement", superpositions.Count > 0 ? totalEntanglement / superpositions.Count : 0.0 },
            { "MaxEntanglement", maxEntanglement },
            { "MaxPossibleEntanglement", superpositions.Count > 0 ? Math.Log(superpositions[0].AmplitudesReal.Length) / Math.Log(2) : 0.0 }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Superposition - Multiple Simultaneous States ===\n");

        var superposition = new Superposition(qubitCount: 2);

        Console.WriteLine("--- Initial Superposition State ---");
        superposition.PrintSuperpositionState(0, maxStates: 4);

        Console.WriteLine("\n--- Creating Equal Superposition ---");
        superposition.CreateEqualSuperposition(qubitCount: 3);
        superposition.PrintSuperpositionState(1, maxStates: 8);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = superposition.GetSuperpositionMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Applying Phase ---");
        superposition.ApplyPhase(0, phase: Math.PI / 4);

        Console.WriteLine("\n--- Evolving Superposition ---");
        superposition.EvolveSuperposition(0, steps: 20, evolutionRate: 0.5);

        Console.WriteLine("\n--- Measuring Superposition (5 measurements) ---");
        for (int i = 0; i < 5; i++)
        {
            int result = superposition.MeasureSuperposition(0);
            Console.WriteLine($"  Measurement {i + 1}: Collapsed to state |{result}⟩");
        }

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = superposition.GetSuperpositionMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSuperposition allows quantum systems to exist in multiple states until measurement.");
    }
}
