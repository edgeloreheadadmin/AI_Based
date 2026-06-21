using System;
using System.Collections.Generic;

/// <summary>
/// Qubits - Basic Unit of Quantum Information
/// Can exist in superposition of states |0⟩ and |1⟩ simultaneously
/// Collapses to definite state upon measurement
/// </summary>
public class Qubit
{
    private double amplitudeZero;
    private double amplitudeOne;
    private Random random;
    private bool measured;
    private int measuredValue;

    public Qubit(double ampZero = 1.0, double ampOne = 0.0)
    {
        this.random = new Random();
        this.measured = false;
        this.measuredValue = -1;

        NormalizeAmplitudes(ampZero, ampOne);
    }

    private void NormalizeAmplitudes(double amp0, double amp1)
    {
        double norm = Math.Sqrt(amp0 * amp0 + amp1 * amp1);

        if (norm > 0)
        {
            this.amplitudeZero = amp0 / norm;
            this.amplitudeOne = amp1 / norm;
        }
        else
        {
            this.amplitudeZero = 1.0;
            this.amplitudeOne = 0.0;
        }
    }

    public double GetProbabilityZero()
    {
        return amplitudeZero * amplitudeZero;
    }

    public double GetProbabilityOne()
    {
        return amplitudeOne * amplitudeOne;
    }

    public string GetSuperpositionState()
    {
        return $"{amplitudeZero:F3}|0⟩ + {amplitudeOne:F3}|1⟩";
    }

    public int Measure()
    {
        double probZero = GetProbabilityZero();
        double randomValue = random.NextDouble();

        if (randomValue < probZero)
        {
            measured = true;
            measuredValue = 0;
            return 0;
        }
        else
        {
            measured = true;
            measuredValue = 1;
            return 1;
        }
    }

    public int GetMeasuredValue()
    {
        return measuredValue;
    }

    public bool IsMeasured()
    {
        return measured;
    }

    public void ApplyPauliX()
    {
        double temp = amplitudeZero;
        amplitudeZero = amplitudeOne;
        amplitudeOne = temp;
    }

    public void ApplyHadamard()
    {
        double sqrt2 = Math.Sqrt(2);
        double newAmp0 = (amplitudeZero + amplitudeOne) / sqrt2;
        double newAmp1 = (amplitudeZero - amplitudeOne) / sqrt2;

        amplitudeZero = newAmp0;
        amplitudeOne = newAmp1;
    }

    public static void Main()
    {
        Console.WriteLine("=== Qubit Superposition ===\n");

        Console.WriteLine("1. Pure State |0⟩:");
        var qubit0 = new Qubit(1.0, 0.0);
        Console.WriteLine($"   State: {qubit0.GetSuperpositionState()}");
        Console.WriteLine($"   P(0) = {qubit0.GetProbabilityZero():F3}, P(1) = {qubit0.GetProbabilityOne():F3}");

        Console.WriteLine("\n2. Pure State |1⟩:");
        var qubit1 = new Qubit(0.0, 1.0);
        Console.WriteLine($"   State: {qubit1.GetSuperpositionState()}");
        Console.WriteLine($"   P(0) = {qubit1.GetProbabilityZero():F3}, P(1) = {qubit1.GetProbabilityOne():F3}");

        Console.WriteLine("\n3. Equal Superposition (Hadamard):");
        var qubHad = new Qubit(1.0, 0.0);
        qubHad.ApplyHadamard();
        Console.WriteLine($"   State: {qubHad.GetSuperpositionState()}");
        Console.WriteLine($"   P(0) = {qubHad.GetProbabilityZero():F3}, P(1) = {qubHad.GetProbabilityOne():F3}");

        Console.WriteLine("\n4. Measurement Collapse:");
        var qubMeasure = new Qubit(1.0, 1.0);
        Console.WriteLine($"   Before Measurement: {qubMeasure.GetSuperpositionState()}");
        int result = qubMeasure.Measure();
        Console.WriteLine($"   Measured Value: {result}");
        Console.WriteLine($"   State Collapsed to: |{result}⟩");

        Console.WriteLine("\n5. Multiple Measurements (100 trials, equal superposition):");
        int countZero = 0, countOne = 0;
        for (int i = 0; i < 100; i++)
        {
            var qub = new Qubit(1.0, 1.0);
            if (qub.Measure() == 0)
                countZero++;
            else
                countOne++;
        }
        Console.WriteLine($"   Results: 0→{countZero}, 1→{countOne}");
    }
}
