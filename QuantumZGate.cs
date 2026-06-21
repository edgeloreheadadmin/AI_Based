using System;
using System.Collections.Generic;

/// <summary>
/// Quantum ZGate - Toffoli Gate Implementation
/// Universal quantum gate that can construct any quantum circuit
/// Three-qubit gate: 2 control qubits + 1 target qubit
/// If both control qubits are |1⟩, the target qubit is flipped
/// </summary>
public class QuantumZGate
{
    public struct QuantumBit
    {
        public int Value { get; set; }

        public QuantumBit(int val)
        {
            Value = val & 1;
        }

        public override string ToString()
        {
            return Value == 0 ? "|0⟩" : "|1⟩";
        }
    }

    private QuantumBit controlQubit1;
    private QuantumBit controlQubit2;
    private QuantumBit targetQubit;

    public QuantumZGate(int control1, int control2, int target)
    {
        this.controlQubit1 = new QuantumBit(control1);
        this.controlQubit2 = new QuantumBit(control2);
        this.targetQubit = new QuantumBit(target);
    }

    public void ApplyGate()
    {
        if (controlQubit1.Value == 1 && controlQubit2.Value == 1)
        {
            targetQubit.Value = (targetQubit.Value + 1) % 2;
        }
    }

    public QuantumBit GetTargetQubit()
    {
        return targetQubit;
    }

    public QuantumBit GetControlQubit1()
    {
        return controlQubit1;
    }

    public QuantumBit GetControlQubit2()
    {
        return controlQubit2;
    }

    public Dictionary<string, string> GetState()
    {
        Dictionary<string, string> state = new Dictionary<string, string>
        {
            { "Control1", controlQubit1.ToString() },
            { "Control2", controlQubit2.ToString() },
            { "Target", targetQubit.ToString() }
        };
        return state;
    }

    public static void Main()
    {
        Console.WriteLine("=== Toffoli Gate (ZGate) Simulation ===\n");

        var testCases = new List<(int, int, int)>
        {
            (0, 0, 0),
            (0, 0, 1),
            (0, 1, 0),
            (0, 1, 1),
            (1, 0, 0),
            (1, 0, 1),
            (1, 1, 0),
            (1, 1, 1)
        };

        Console.WriteLine("Truth Table: Toffoli Gate");
        Console.WriteLine("Control1 | Control2 | Target_In | Target_Out");
        Console.WriteLine("---------|----------|-----------|----------");

        foreach (var (c1, c2, target) in testCases)
        {
            var zgate = new QuantumZGate(c1, c2, target);
            int targetBefore = target;
            zgate.ApplyGate();
            int targetAfter = zgate.GetTargetQubit().Value;

            Console.WriteLine($"   {c1}    |    {c2}    |     {targetBefore}     |    {targetAfter}");
        }

        Console.WriteLine("\n=== Controlled AND Logic ===");
        Console.WriteLine("The Toffoli gate implements controlled AND:");
        Console.WriteLine("- If Control1 AND Control2 are both |1⟩, flip Target");
        Console.WriteLine("- Otherwise, Target remains unchanged");

        Console.WriteLine("\n=== Example: Building AND Gate with Toffoli ===");
        var andGate = new QuantumZGate(1, 1, 0);
        Console.WriteLine($"Before: {string.Join(", ", andGate.GetState().Values)}");
        andGate.ApplyGate();
        Console.WriteLine($"After:  Control1={andGate.GetControlQubit1()}, Control2={andGate.GetControlQubit2()}, Target={andGate.GetTargetQubit()}");
    }
}
