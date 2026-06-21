using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Multi-Qubit Quantum Computing
/// Uses multiple qubits to perform quantum computations
/// Demonstrates entanglement, superposition, and quantum operations on multiple qubits
/// </summary>
public class MultiQubitQuantumComputing
{
    private List<double> stateAmplitudes;
    private int qubitCount;
    private Random random;

    public MultiQubitQuantumComputing(int numQubits)
    {
        this.qubitCount = numQubits;
        this.random = new Random();

        int stateSpaceSize = (int)Math.Pow(2, numQubits);
        this.stateAmplitudes = new List<double>(new double[stateSpaceSize]);

        stateAmplitudes[0] = 1.0;
    }

    public void InitializeToState(string binaryState)
    {
        int stateIndex = Convert.ToInt32(binaryState, 2);
        for (int i = 0; i < stateAmplitudes.Count; i++)
        {
            stateAmplitudes[i] = (i == stateIndex) ? 1.0 : 0.0;
        }
    }

    public void ApplyHadamardToAll()
    {
        int newSize = stateAmplitudes.Count;
        List<double> newAmplitudes = new List<double>(new double[newSize]);

        for (int i = 0; i < stateAmplitudes.Count; i++)
        {
            for (int j = 0; j < stateAmplitudes.Count; j++)
            {
                int hammingDistance = CountBits(i ^ j);
                double sign = (hammingDistance % 2 == 0) ? 1.0 : -1.0;
                newAmplitudes[j] += (1.0 / Math.Sqrt(newSize)) * stateAmplitudes[i] * sign;
            }
        }

        this.stateAmplitudes = newAmplitudes;
    }

    private int CountBits(int n)
    {
        int count = 0;
        while (n > 0)
        {
            count += n & 1;
            n >>= 1;
        }
        return count;
    }

    public void ApplyCNOT(int controlQubit, int targetQubit)
    {
        int stateSpaceSize = stateAmplitudes.Count;

        for (int state = 0; state < stateSpaceSize; state++)
        {
            int controlBit = (state >> controlQubit) & 1;

            if (controlBit == 1)
            {
                int targetBit = (state >> targetQubit) & 1;
                int flippedTargetBit = 1 - targetBit;

                int newState = state ^ (targetBit ^ flippedTargetBit) << targetQubit;

                double temp = stateAmplitudes[state];
                stateAmplitudes[state] = stateAmplitudes[newState];
                stateAmplitudes[newState] = temp;
            }
        }
    }

    public int Measure()
    {
        double[] probabilities = stateAmplitudes.Select(a => a * a).ToArray();

        double randomValue = random.NextDouble();
        double cumulativeProbability = 0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulativeProbability += probabilities[i];
            if (randomValue < cumulativeProbability)
                return i;
        }

        return probabilities.Length - 1;
    }

    public Dictionary<string, double> GetStateProabilities()
    {
        Dictionary<string, double> probs = new Dictionary<string, double>();

        for (int i = 0; i < stateAmplitudes.Count; i++)
        {
            string binary = Convert.ToString(i, 2).PadLeft(qubitCount, '0');
            double prob = stateAmplitudes[i] * stateAmplitudes[i];

            if (prob > 0.0001)
                probs[binary] = prob;
        }

        return probs;
    }

    public void PrintState()
    {
        var probs = GetStateProabilities();

        Console.WriteLine($"Quantum State ({qubitCount}-qubit system):");
        foreach (var kvp in probs.OrderByDescending(p => p.Value))
        {
            Console.WriteLine($"  |{kvp.Key}⟩: {kvp.Value:P2}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Multi-Qubit Quantum Computing ===\n");

        Console.WriteLine("1. Two-Qubit System - Initial State |00⟩");
        var twoQubit = new MultiQubitQuantumComputing(2);
        twoQubit.PrintState();

        Console.WriteLine("\n2. Apply Hadamard to All Qubits (Equal Superposition)");
        twoQubit.ApplyHadamardToAll();
        twoQubit.PrintState();

        Console.WriteLine("\n3. Three-Qubit System with CNOT Gate");
        var threeQubit = new MultiQubitQuantumComputing(3);
        threeQubit.InitializeToState("000");
        threeQubit.ApplyHadamardToAll();
        threeQubit.ApplyCNOT(0, 1);
        threeQubit.PrintState();

        Console.WriteLine("\n4. Entangled Bell State (2-qubit)");
        var bellState = new MultiQubitQuantumComputing(2);
        bellState.InitializeToState("00");
        bellState.ApplyHadamardToAll();
        bellState.ApplyCNOT(0, 1);
        bellState.PrintState();

        Console.WriteLine("\n5. Measurement Results (1000 trials on |+⟩|+⟩)");
        var measureTest = new MultiQubitQuantumComputing(2);
        measureTest.ApplyHadamardToAll();

        Dictionary<int, int> results = new Dictionary<int, int>();
        for (int i = 0; i < 1000; i++)
        {
            int result = measureTest.Measure();
            if (!results.ContainsKey(result))
                results[result] = 0;
            results[result]++;
        }

        foreach (var kvp in results.OrderBy(r => r.Key))
        {
            string binary = Convert.ToString(kvp.Key, 2).PadLeft(2, '0');
            Console.WriteLine($"  |{binary}⟩: {kvp.Value / 10.0:F1}%");
        }
    }
}
