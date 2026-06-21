using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Superposition Quantum Flux
/// Quantization of magnetic flux in a superconducting loop
/// Demonstrates flux quantization and Josephson effects
/// </summary>
public class SuperpositionQuantumFlux
{
    private struct SuperconductingLoop
    {
        public int LoopId;
        public double Inductance;
        public double Resistance;
        public double Temperature;
        public double MagneticFlux;
        public int FluxQuantumNumber;
        public double[] QuantumAmplitudes;
        public double[] PhaseAngles;
        public double CriticalCurrent;
        public double ActualCurrent;

        public SuperconductingLoop(int id)
        {
            LoopId = id;
            Inductance = 1e-9;
            Resistance = 0.0;
            Temperature = 0.1;
            MagneticFlux = 0.0;
            FluxQuantumNumber = 0;
            QuantumAmplitudes = new double[5];
            PhaseAngles = new double[5];
            CriticalCurrent = 1.0e-3;
            ActualCurrent = 0.0;

            for (int i = 0; i < 5; i++)
            {
                QuantumAmplitudes[i] = 1.0 / Math.Sqrt(5);
                PhaseAngles[i] = (i * 2 * Math.PI) / 5;
            }
        }
    }

    private List<SuperconductingLoop> loops;
    private const double FluxQuantum = 2.067833848e-15;
    private Random random;

    public SuperpositionQuantumFlux()
    {
        this.loops = new List<SuperconductingLoop>();
        this.random = new Random();
    }

    public void CreateSuperconductingLoop()
    {
        var loop = new SuperconductingLoop(loops.Count);
        loops.Add(loop);
        Console.WriteLine($"Created superconducting loop {loop.LoopId}");
    }

    public void ApplyMagneticField(int loopId, double externalFlux)
    {
        if (loopId >= loops.Count)
            return;

        var loop = loops[loopId];
        loop.MagneticFlux = externalFlux;

        int quantumNumber = (int)Math.Round(externalFlux / FluxQuantum);
        loop.FluxQuantumNumber = quantumNumber;

        for (int i = 0; i < 5; i++)
        {
            double energyDifference = Math.Abs(i - quantumNumber) * FluxQuantum * 1e6;
            loop.QuantumAmplitudes[i] = Math.Exp(-energyDifference / (1.38e-23 * loop.Temperature));
        }

        double sum = loop.QuantumAmplitudes.Sum();
        for (int i = 0; i < 5; i++)
        {
            loop.QuantumAmplitudes[i] /= sum;
        }

        loops[loopId] = loop;
    }

    public void QuantizeFlux()
    {
        foreach (var loop in loops)
        {
            double quantizedFlux = loop.FluxQuantumNumber * FluxQuantum;

            for (int i = 0; i < 5; i++)
            {
                loop.PhaseAngles[i] = (2 * Math.PI * quantizedFlux) / FluxQuantum +
                                     (i * 2 * Math.PI) / 5;
            }

            int idx = loops.IndexOf(loop);
            loops[idx] = loop;
        }
    }

    public void ApplyJosephsonEffect()
    {
        for (int i = 0; i < loops.Count - 1; i++)
        {
            var loop1 = loops[i];
            var loop2 = loops[i + 1];

            double phaseDifference = loop1.PhaseAngles[0] - loop2.PhaseAngles[0];
            double josephsonCurrent = Math.Sin(phaseDifference) * loop1.CriticalCurrent;

            loop1.ActualCurrent = josephsonCurrent;
            loop2.ActualCurrent = -josephsonCurrent;

            if (Math.Abs(loop1.ActualCurrent) > loop1.CriticalCurrent * 0.9)
            {
                loop1.Resistance = 1e-6;
            }
            else
            {
                loop1.Resistance = 0.0;
            }

            loops[i] = loop1;
            loops[i + 1] = loop2;
        }
    }

    public void MeasureFlux(int loopId)
    {
        if (loopId >= loops.Count)
            return;

        var loop = loops[loopId];

        double randomValue = random.NextDouble();
        double cumulativeProbability = 0;

        for (int i = 0; i < 5; i++)
        {
            cumulativeProbability += loop.QuantumAmplitudes[i];
            if (randomValue < cumulativeProbability)
            {
                loop.FluxQuantumNumber = i;
                loop.MagneticFlux = i * FluxQuantum;
                break;
            }
        }

        loops[loopId] = loop;
    }

    public Dictionary<string, object> GetFluxQuantizationMetrics()
    {
        double totalFlux = loops.Sum(l => l.MagneticFlux);
        double totalCurrent = loops.Sum(l => l.ActualCurrent);
        double avgQuantumNumber = loops.Average(l => l.FluxQuantumNumber);

        return new Dictionary<string, object>
        {
            { "LoopCount", loops.Count },
            { "TotalFlux", totalFlux },
            { "TotalCurrent", totalCurrent },
            { "AverageFluxQuantumNumber", avgQuantumNumber },
            { "FluxQuantum", FluxQuantum },
            { "AverageTemperature", loops.Average(l => l.Temperature) }
        };
    }

    public void PrintFluxStates(int limit = 5)
    {
        Console.WriteLine("Superconducting Loop Flux States:");
        Console.WriteLine("Loop | Quantum# | Flux      | Current   | Phase");
        Console.WriteLine("-----|----------|-----------|-----------|--------");

        for (int i = 0; i < Math.Min(limit, loops.Count); i++)
        {
            var loop = loops[i];
            Console.WriteLine($"{loop.LoopId,-4} | {loop.FluxQuantumNumber,-8} | " +
                            $"{loop.MagneticFlux:E8} | {loop.ActualCurrent:E8} | " +
                            $"{loop.PhaseAngles[0]:F4}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Superposition Quantum Flux - Magnetic Flux Quantization ===\n");

        var sqf = new SuperpositionQuantumFlux();

        Console.WriteLine("--- Creating Superconducting Loops ---");
        for (int i = 0; i < 4; i++)
        {
            sqf.CreateSuperconductingLoop();
        }

        Console.WriteLine("\n--- Applying External Magnetic Fields ---");
        sqf.ApplyMagneticField(0, 5e-15);
        sqf.ApplyMagneticField(1, 10e-15);
        sqf.ApplyMagneticField(2, 15e-15);
        sqf.ApplyMagneticField(3, 20e-15);

        Console.WriteLine("\n--- Quantizing Flux ---");
        sqf.QuantizeFlux();

        sqf.PrintFluxStates(4);

        Console.WriteLine("\n--- Applying Josephson Effect ---");
        sqf.ApplyJosephsonEffect();

        Console.WriteLine("\nUpdated States:");
        sqf.PrintFluxStates(4);

        Console.WriteLine("\n--- Measurement Simulation ---");
        Console.WriteLine("Measuring flux through loops:");
        for (int i = 0; i < sqf.loops.Count; i++)
        {
            sqf.MeasureFlux(i);
            var loop = sqf.loops[i];
            Console.WriteLine($"  Loop {i}: Measured quantum number = {loop.FluxQuantumNumber}");
        }

        Console.WriteLine("\n--- Flux Quantization Metrics ---");
        var metrics = sqf.GetFluxQuantizationMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSuperposition Quantum Flux demonstrates magnetic flux quantization.");
    }
}
