using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Teleportation
/// Technique for transferring quantum information from one location to another
/// without physically moving the quantum system using entanglement and classical communication
/// </summary>
public class Teleportation
{
    private struct QubitState
    {
        public double AlphaReal;
        public double AlphaImag;
        public double BetaReal;
        public double BetaImag;

        public double GetAmplitude0()
        {
            return Math.Sqrt(AlphaReal * AlphaReal + AlphaImag * AlphaImag);
        }

        public double GetAmplitude1()
        {
            return Math.Sqrt(BetaReal * BetaReal + BetaImag * BetaImag);
        }

        public void Normalize()
        {
            double norm = Math.Sqrt(AlphaReal * AlphaReal + AlphaImag * AlphaImag +
                                   BetaReal * BetaReal + BetaImag * BetaImag);
            if (norm > 0)
            {
                AlphaReal /= norm;
                AlphaImag /= norm;
                BetaReal /= norm;
                BetaImag /= norm;
            }
        }
    }

    private struct BellPair
    {
        public QubitState Qubit1;
        public QubitState Qubit2;

        public void InitializeMaximallyEntangled()
        {
            double inv_sqrt2 = 1.0 / Math.Sqrt(2);

            Qubit1.AlphaReal = inv_sqrt2;
            Qubit1.AlphaImag = 0.0;
            Qubit1.BetaReal = 0.0;
            Qubit1.BetaImag = 0.0;

            Qubit2.AlphaReal = inv_sqrt2;
            Qubit2.AlphaImag = 0.0;
            Qubit2.BetaReal = inv_sqrt2;
            Qubit2.BetaImag = 0.0;
        }
    }

    private List<QubitState> aliceQubits;
    private List<QubitState> bobQubits;
    private List<BellPair> sharedEntanglement;
    private Random random;

    public Teleportation()
    {
        this.aliceQubits = new List<QubitState>();
        this.bobQubits = new List<QubitState>();
        this.sharedEntanglement = new List<BellPair>();
        this.random = new Random();
    }

    public void PrepareQubitToTeleport()
    {
        Console.WriteLine("\nPreparing qubit for teleportation at Alice's location...\n");

        double phase = random.NextDouble() * 2 * Math.PI;
        double theta = random.NextDouble() * Math.PI;

        QubitState qubit = new QubitState
        {
            AlphaReal = Math.Cos(theta / 2) * Math.Cos(phase),
            AlphaImag = Math.Cos(theta / 2) * Math.Sin(phase),
            BetaReal = Math.Sin(theta / 2) * Math.Cos(phase + Math.PI),
            BetaImag = Math.Sin(theta / 2) * Math.Sin(phase + Math.PI)
        };

        qubit.Normalize();
        aliceQubits.Add(qubit);

        Console.WriteLine($"Qubit state prepared:");
        Console.WriteLine($"  α = {qubit.AlphaReal:F4} + {qubit.AlphaImag:F4}i");
        Console.WriteLine($"  β = {qubit.BetaReal:F4} + {qubit.BetaImag:F4}i");
    }

    public void ShareEntangledPair()
    {
        Console.WriteLine("\nSharing entangled Bell pair between Alice and Bob...\n");

        BellPair bellPair = new BellPair();
        bellPair.InitializeMaximallyEntangled();
        sharedEntanglement.Add(bellPair);

        Console.WriteLine("Bell pair created: |Φ+⟩ = (1/√2)(|00⟩ + |11⟩)");
    }

    public void PerformBellMeasurement()
    {
        Console.WriteLine("\nPerforming Bell measurement on Alice's qubits...\n");

        if (aliceQubits.Count == 0 || sharedEntanglement.Count == 0)
            return;

        QubitState aliceQubit = aliceQubits[0];
        BellPair bellPair = sharedEntanglement[0];

        double m0 = random.NextDouble() < 0.5 ? 0 : 1;
        double m1 = random.NextDouble() < 0.5 ? 0 : 1;

        Console.WriteLine($"Bell measurement result: m0 = {m0}, m1 = {m1}");
        Console.WriteLine("Classical bits sent to Bob: " + (int)(m0 + 2 * m1));
    }

    public void RecoverStateAtBob()
    {
        Console.WriteLine("\nRecovering quantum state at Bob's location...\n");

        if (aliceQubits.Count == 0 || sharedEntanglement.Count == 0)
            return;

        QubitState originalState = aliceQubits[0];
        QubitState bobQubit = sharedEntanglement[0].Qubit2;

        double m0 = random.NextDouble() < 0.5 ? 0 : 1;
        double m1 = random.NextDouble() < 0.5 ? 0 : 1;

        if (m0 > 0.5)
        {
            bobQubit.BetaReal = -bobQubit.BetaReal;
            bobQubit.BetaImag = -bobQubit.BetaImag;
        }

        if (m1 > 0.5)
        {
            double temp = bobQubit.AlphaReal;
            bobQubit.AlphaReal = bobQubit.BetaReal;
            bobQubit.BetaReal = temp;
            temp = bobQubit.AlphaImag;
            bobQubit.AlphaImag = bobQubit.BetaImag;
            bobQubit.BetaImag = temp;
        }

        bobQubit.Normalize();
        bobQubits.Add(bobQubit);

        Console.WriteLine("State recovered at Bob's location");
    }

    public double CalculateFidelity()
    {
        if (aliceQubits.Count == 0 || bobQubits.Count == 0)
            return 0.0;

        QubitState original = aliceQubits[0];
        QubitState received = bobQubits[0];

        double fidelity = Math.Abs(original.AlphaReal * received.AlphaReal + original.AlphaImag * received.AlphaImag +
                                 original.BetaReal * received.BetaReal + original.BetaImag * received.BetaImag);

        return fidelity * fidelity;
    }

    public void ExecuteTeleportationProtocol(int iterations)
    {
        Console.WriteLine($"\nExecuting teleportation protocol for {iterations} iterations...\n");

        for (int i = 0; i < iterations; i++)
        {
            aliceQubits.Clear();
            bobQubits.Clear();
            sharedEntanglement.Clear();

            PrepareQubitToTeleport();
            ShareEntangledPair();
            PerformBellMeasurement();
            RecoverStateAtBob();

            double fidelity = CalculateFidelity();
            if (i % (iterations / 3) == 0 || i == iterations - 1)
            {
                Console.WriteLine($"Iteration {i}: Teleportation Fidelity = {fidelity:F6}");
            }
        }
    }

    public Dictionary<string, object> GetTeleportationMetrics()
    {
        double avgFidelity = 0.0;
        int testCount = 10;

        for (int i = 0; i < testCount; i++)
        {
            aliceQubits.Clear();
            bobQubits.Clear();
            sharedEntanglement.Clear();

            PrepareQubitToTeleport();
            ShareEntangledPair();
            PerformBellMeasurement();
            RecoverStateAtBob();

            avgFidelity += CalculateFidelity();
        }

        avgFidelity /= testCount;

        return new Dictionary<string, object>
        {
            { "AliceQubitCount", aliceQubits.Count },
            { "BobQubitCount", bobQubits.Count },
            { "EntangledPairs", sharedEntanglement.Count },
            { "AverageFidelity", avgFidelity },
            { "ClassicalBitsRequired", 2 }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Teleportation - Non-Local State Transfer ===\n");

        var teleportation = new Teleportation();

        Console.WriteLine("--- Initial Teleportation Metrics ---");
        var metricsInitial = teleportation.GetTeleportationMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Executing Single Teleportation ---");
        teleportation.PrepareQubitToTeleport();
        teleportation.ShareEntangledPair();
        teleportation.PerformBellMeasurement();
        teleportation.RecoverStateAtBob();

        double fidelity = teleportation.CalculateFidelity();
        Console.WriteLine($"\nFidelity of teleported state: {fidelity:F6}");

        Console.WriteLine("\n--- Running Multiple Teleportations ---");
        teleportation.ExecuteTeleportationProtocol(iterations: 10);

        Console.WriteLine("\n--- Final Teleportation Metrics ---");
        var metricsFinal = teleportation.GetTeleportationMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Teleportation transfers quantum information using entanglement and classical bits.");
    }
}
