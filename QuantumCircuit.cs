using System;
using System.Collections.Generic;

/// <summary>
/// Quantum Circuit
/// A sequence of quantum gates that operate on quantum bits
/// Simulates circuit execution with multiple gate types
/// </summary>
public class QuantumCircuit
{
    private struct QuantumGate
    {
        public string Name;
        public int Target;
        public int Control;
        public bool HasControl;

        public QuantumGate(string name, int target, int control = -1)
        {
            Name = name;
            Target = target;
            Control = control;
            HasControl = control >= 0;
        }

        public override string ToString()
        {
            if (HasControl)
                return $"{Name}(Control: Q{Control}, Target: Q{Target})";
            else
                return $"{Name}(Q{Target})";
        }
    }

    private int qubitCount;
    private List<QuantumGate> gateSequence;
    private double[] stateAmplitudes;
    private Random random;

    public QuantumCircuit(int numQubits)
    {
        this.qubitCount = numQubits;
        this.gateSequence = new List<QuantumGate>();
        this.stateAmplitudes = new double[(int)Math.Pow(2, numQubits)];
        this.stateAmplitudes[0] = 1.0;
        this.random = new Random();
    }

    public void AddGate(string gateName, int targetQubit, int controlQubit = -1)
    {
        if (targetQubit < 0 || targetQubit >= qubitCount)
            throw new ArgumentException("Invalid target qubit");

        if (controlQubit >= 0 && controlQubit >= qubitCount)
            throw new ArgumentException("Invalid control qubit");

        gateSequence.Add(new QuantumGate(gateName, targetQubit, controlQubit));
    }

    public void ExecuteCircuit()
    {
        foreach (var gate in gateSequence)
        {
            switch (gate.Name.ToUpper())
            {
                case "H":
                    ApplyHadamardGate(gate.Target);
                    break;
                case "X":
                    ApplyPauliX(gate.Target);
                    break;
                case "Z":
                    ApplyPauliZ(gate.Target);
                    break;
                case "CNOT":
                    ApplyCNOT(gate.Control, gate.Target);
                    break;
                case "SWAP":
                    ApplySWAP(gate.Control, gate.Target);
                    break;
            }
        }
    }

    private void ApplyHadamardGate(int qubit)
    {
        int stateSize = stateAmplitudes.Length;
        double[] newAmplitudes = new double[stateSize];

        for (int i = 0; i < stateSize; i++)
        {
            for (int j = 0; j < stateSize; j++)
            {
                int bitI = (i >> qubit) & 1;
                int bitJ = (j >> qubit) & 1;

                if ((i ^ j) == (1 << qubit) || i == j)
                {
                    double factor = (bitI == bitJ) ? 1.0 : -1.0;
                    newAmplitudes[j] += stateAmplitudes[i] * factor / Math.Sqrt(2);
                }
            }
        }

        Array.Copy(newAmplitudes, stateAmplitudes, stateSize);
    }

    private void ApplyPauliX(int qubit)
    {
        int flipMask = 1 << qubit;

        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            int j = i ^ flipMask;
            if (i < j)
            {
                double temp = stateAmplitudes[i];
                stateAmplitudes[i] = stateAmplitudes[j];
                stateAmplitudes[j] = temp;
            }
        }
    }

    private void ApplyPauliZ(int qubit)
    {
        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            int bit = (i >> qubit) & 1;
            if (bit == 1)
                stateAmplitudes[i] *= -1;
        }
    }

    private void ApplyCNOT(int control, int target)
    {
        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            int controlBit = (i >> control) & 1;
            if (controlBit == 1)
            {
                int j = i ^ (1 << target);
                double temp = stateAmplitudes[i];
                stateAmplitudes[i] = stateAmplitudes[j];
                stateAmplitudes[j] = temp;
            }
        }
    }

    private void ApplySWAP(int qubit1, int qubit2)
    {
        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            int bit1 = (i >> qubit1) & 1;
            int bit2 = (i >> qubit2) & 1;

            if (bit1 != bit2)
            {
                int j = i ^ (1 << qubit1) ^ (1 << qubit2);
                if (i < j)
                {
                    double temp = stateAmplitudes[i];
                    stateAmplitudes[i] = stateAmplitudes[j];
                    stateAmplitudes[j] = temp;
                }
            }
        }
    }

    public int Measure()
    {
        double[] probabilities = new double[stateAmplitudes.Length];
        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            probabilities[i] = stateAmplitudes[i] * stateAmplitudes[i];
        }

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

    public void PrintCircuit()
    {
        Console.WriteLine("Quantum Circuit Sequence:");
        for (int i = 0; i < gateSequence.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {gateSequence[i]}");
        }
    }

    public void PrintState()
    {
        Console.WriteLine("Quantum State:");
        for (int i = 0; i < stateAmplitudes.Length; i++)
        {
            if (Math.Abs(stateAmplitudes[i]) > 0.01)
            {
                string binary = Convert.ToString(i, 2).PadLeft(qubitCount, '0');
                double prob = stateAmplitudes[i] * stateAmplitudes[i];
                Console.WriteLine($"  |{binary}⟩: amplitude={stateAmplitudes[i]:F4}, probability={prob:F4}");
            }
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Circuit Simulation ===\n");

        Console.WriteLine("Circuit 1: Simple Hadamard on 1 Qubit");
        var circuit1 = new QuantumCircuit(1);
        circuit1.AddGate("H", 0);
        circuit1.PrintCircuit();
        circuit1.ExecuteCircuit();
        circuit1.PrintState();

        Console.WriteLine("\n\nCircuit 2: Bell State (2-qubit entanglement)");
        var circuit2 = new QuantumCircuit(2);
        circuit2.AddGate("H", 0);
        circuit2.AddGate("CNOT", 1, 0);
        circuit2.PrintCircuit();
        circuit2.ExecuteCircuit();
        circuit2.PrintState();

        Console.WriteLine("\n\nCircuit 3: GHZ State (3-qubit entanglement)");
        var circuit3 = new QuantumCircuit(3);
        circuit3.AddGate("H", 0);
        circuit3.AddGate("CNOT", 1, 0);
        circuit3.AddGate("CNOT", 2, 0);
        circuit3.PrintCircuit();
        circuit3.ExecuteCircuit();
        circuit3.PrintState();

        Console.WriteLine("\n\nCircuit 4: Quantum Fourier Transform (3 qubits)");
        var circuit4 = new QuantumCircuit(3);
        circuit4.AddGate("H", 0);
        circuit4.AddGate("H", 1);
        circuit4.AddGate("H", 2);
        circuit4.AddGate("SWAP", 0, 2);
        circuit4.PrintCircuit();
        circuit4.ExecuteCircuit();
        circuit4.PrintState();

        Console.WriteLine("\n\nCircuit 5: Measurement Statistics (1000 trials)");
        var circuit5 = new QuantumCircuit(2);
        circuit5.AddGate("H", 0);
        circuit5.AddGate("H", 1);
        var measurements = new Dictionary<int, int>();

        for (int i = 0; i < 1000; i++)
        {
            var c = new QuantumCircuit(2);
            c.AddGate("H", 0);
            c.AddGate("H", 1);
            c.ExecuteCircuit();
            int result = c.Measure();
            if (!measurements.ContainsKey(result))
                measurements[result] = 0;
            measurements[result]++;
        }

        Console.WriteLine("Measurement Results:");
        foreach (var kvp in measurements)
        {
            string binary = Convert.ToString(kvp.Key, 2).PadLeft(2, '0');
            Console.WriteLine($"  |{binary}⟩: {kvp.Value / 10.0:F1}%");
        }
    }
}
