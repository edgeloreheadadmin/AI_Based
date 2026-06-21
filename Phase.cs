using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Phase
/// Refers to the relative phase of a qubit's state, represented as a complex number
/// Phase determines measurement probability and enables quantum interference
/// </summary>
public class Phase
{
    private struct ComplexAmplitude
    {
        public double Real;
        public double Imaginary;

        public double Magnitude()
        {
            return Math.Sqrt(Real * Real + Imaginary * Imaginary);
        }

        public double Phase()
        {
            return Math.Atan2(Imaginary, Real);
        }

        public double Probability()
        {
            return Magnitude() * Magnitude();
        }

        public void ApplyPhaseShift(double phaseAngle)
        {
            double cosPhase = Math.Cos(phaseAngle);
            double sinPhase = Math.Sin(phaseAngle);

            double oldReal = Real;
            Real = oldReal * cosPhase - Imaginary * sinPhase;
            Imaginary = oldReal * sinPhase + Imaginary * cosPhase;
        }
    }

    private struct QubitPhaseState
    {
        public ComplexAmplitude Alpha;
        public ComplexAmplitude Beta;

        public double GetRelativePhase()
        {
            double alphaPhase = Alpha.Phase();
            double betaPhase = Beta.Phase();
            return betaPhase - alphaPhase;
        }

        public double GetGlobalPhase()
        {
            return Alpha.Phase();
        }

        public void Normalize()
        {
            double norm = Math.Sqrt(Alpha.Probability() + Beta.Probability());
            if (norm > 0)
            {
                Alpha.Real /= norm;
                Alpha.Imaginary /= norm;
                Beta.Real /= norm;
                Beta.Imaginary /= norm;
            }
        }
    }

    private List<QubitPhaseState> qubitStates;
    private Random random;

    public Phase()
    {
        this.qubitStates = new List<QubitPhaseState>();
        this.random = new Random();
    }

    public void CreateQubitWithPhase(double phase0, double phase1, double prob0)
    {
        Console.WriteLine($"\nCreating qubit with phases...\n");

        double amp0 = Math.Sqrt(prob0);
        double amp1 = Math.Sqrt(1.0 - prob0);

        QubitPhaseState state = new QubitPhaseState
        {
            Alpha = new ComplexAmplitude
            {
                Real = amp0 * Math.Cos(phase0),
                Imaginary = amp0 * Math.Sin(phase0)
            },
            Beta = new ComplexAmplitude
            {
                Real = amp1 * Math.Cos(phase1),
                Imaginary = amp1 * Math.Sin(phase1)
            }
        };

        state.Normalize();
        qubitStates.Add(state);

        Console.WriteLine($"Qubit created with relative phase: {state.GetRelativePhase():F4} rad");
    }

    public void ApplyGlobalPhaseShift(int stateIndex, double phaseShift)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine($"\nApplying global phase shift of {phaseShift:F4} rad...\n");

        QubitPhaseState state = qubitStates[stateIndex];

        state.Alpha.ApplyPhaseShift(phaseShift);
        state.Beta.ApplyPhaseShift(phaseShift);

        qubitStates[stateIndex] = state;
    }

    public void ApplyRelativePhaseShift(int stateIndex, double phaseShift)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine($"\nApplying relative phase shift to |1⟩ of {phaseShift:F4} rad...\n");

        QubitPhaseState state = qubitStates[stateIndex];

        state.Beta.ApplyPhaseShift(phaseShift);

        qubitStates[stateIndex] = state;
    }

    public void SimulateQuantumInterference(int stateIndex, int steps)
    {
        Console.WriteLine($"\nSimulating quantum interference for {steps} steps...\n");

        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        for (int step = 0; step < steps; step++)
        {
            QubitPhaseState state = qubitStates[stateIndex];

            double phase0 = state.Alpha.Phase();
            double phase1 = state.Beta.Phase();
            double relativePhase = phase1 - phase0;

            double interferedProbability = state.Alpha.Probability() + state.Beta.Probability() +
                                          2 * state.Alpha.Magnitude() * state.Beta.Magnitude() *
                                          Math.Cos(relativePhase);

            double phaseDrift = 0.1 * step * Math.PI / steps;
            state.Beta.ApplyPhaseShift(phaseDrift);

            qubitStates[stateIndex] = state;

            if (step % (steps / 3) == 0)
            {
                double prob0 = state.Alpha.Probability();
                double prob1 = state.Beta.Probability();
                Console.WriteLine($"Step {step}: P(|0⟩)={prob0:F6}, P(|1⟩)={prob1:F6}");
            }
        }
    }

    public double MeasurePhaseCoherence(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return 0.0;

        QubitPhaseState state = qubitStates[stateIndex];

        double prob0 = state.Alpha.Probability();
        double prob1 = state.Beta.Probability();
        double mag0 = state.Alpha.Magnitude();
        double mag1 = state.Beta.Magnitude();

        if (mag0 > 0 && mag1 > 0)
        {
            double phase0 = state.Alpha.Phase();
            double phase1 = state.Beta.Phase();
            double phaseCoherence = Math.Abs(Math.Cos(phase1 - phase0));
            return phaseCoherence;
        }

        return 0.0;
    }

    public void PrintPhaseInformation(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        QubitPhaseState state = qubitStates[stateIndex];

        Console.WriteLine($"\nPhase Information:");
        Console.WriteLine($"  |0⟩ amplitude: {state.Alpha.Real:F4} + {state.Alpha.Imaginary:F4}i");
        Console.WriteLine($"  |0⟩ magnitude: {state.Alpha.Magnitude():F4}");
        Console.WriteLine($"  |0⟩ phase: {state.Alpha.Phase():F4} rad ({state.Alpha.Phase() * 180 / Math.PI:F2}°)");
        Console.WriteLine($"  |0⟩ probability: {state.Alpha.Probability():F6}");
        Console.WriteLine();
        Console.WriteLine($"  |1⟩ amplitude: {state.Beta.Real:F4} + {state.Beta.Imaginary:F4}i");
        Console.WriteLine($"  |1⟩ magnitude: {state.Beta.Magnitude():F4}");
        Console.WriteLine($"  |1⟩ phase: {state.Beta.Phase():F4} rad ({state.Beta.Phase() * 180 / Math.PI:F2}°)");
        Console.WriteLine($"  |1⟩ probability: {state.Beta.Probability():F6}");
        Console.WriteLine();
        Console.WriteLine($"  Relative phase: {state.GetRelativePhase():F4} rad ({state.GetRelativePhase() * 180 / Math.PI:F2}°)");
        Console.WriteLine($"  Global phase: {state.GetGlobalPhase():F4} rad ({state.GetGlobalPhase() * 180 / Math.PI:F2}°)");
        Console.WriteLine($"  Phase coherence: {MeasurePhaseCoherence(stateIndex):F6}");
    }

    public Dictionary<string, object> GetPhaseMetrics()
    {
        double avgRelativePhase = 0.0;
        double avgCoherence = 0.0;

        for (int i = 0; i < qubitStates.Count; i++)
        {
            avgRelativePhase += qubitStates[i].GetRelativePhase();
            avgCoherence += MeasurePhaseCoherence(i);
        }

        if (qubitStates.Count > 0)
        {
            avgRelativePhase /= qubitStates.Count;
            avgCoherence /= qubitStates.Count;
        }

        return new Dictionary<string, object>
        {
            { "QubitCount", qubitStates.Count },
            { "AverageRelativePhase", avgRelativePhase },
            { "AverageCoherence", avgCoherence },
            { "MaxRelativePhase", 2 * Math.PI }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Phase - Quantum State Phase Effects ===\n");

        var phase = new Phase();

        Console.WriteLine("--- Creating Qubit States with Specific Phases ---");
        phase.CreateQubitWithPhase(phase0: 0, phase1: Math.PI / 4, prob0: 0.5);
        phase.CreateQubitWithPhase(phase0: 0, phase1: Math.PI / 2, prob0: 0.3);

        Console.WriteLine("\n--- Initial Phase Metrics ---");
        var metricsInitial = phase.GetPhaseMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Phase Information ---");
        phase.PrintPhaseInformation(0);

        Console.WriteLine("\n--- Applying Global Phase Shift ---");
        phase.ApplyGlobalPhaseShift(0, phaseShift: Math.PI / 3);
        phase.PrintPhaseInformation(0);

        Console.WriteLine("\n--- Applying Relative Phase Shift ---");
        phase.ApplyRelativePhaseShift(1, phaseShift: Math.PI / 6);
        phase.PrintPhaseInformation(1);

        Console.WriteLine("\n--- Simulating Quantum Interference ---");
        phase.SimulateQuantumInterference(stateIndex: 0, steps: 15);

        Console.WriteLine("\n--- Final Phase Metrics ---");
        var metricsFinal = phase.GetPhaseMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nPhase determines quantum interference and measurement probabilities.");
    }
}
