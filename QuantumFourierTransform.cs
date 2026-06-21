using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Fourier Transform
/// Quantum algorithm that performs Fourier transform on quantum states
/// Fundamental subroutine for phase estimation, order-finding, and other quantum algorithms
/// </summary>
public class QuantumFourierTransform
{
    private struct ComplexAmplitude
    {
        public double Real;
        public double Imaginary;

        public double Magnitude()
        {
            return Math.Sqrt(Real * Real + Imaginary * Imaginary);
        }

        public ComplexAmplitude Multiply(ComplexAmplitude other)
        {
            return new ComplexAmplitude
            {
                Real = Real * other.Real - Imaginary * other.Imaginary,
                Imaginary = Real * other.Imaginary + Imaginary * other.Real
            };
        }
    }

    private ComplexAmplitude[][] quantumStates;
    private int qubitCount;
    private int stateSize;
    private Random random;

    public QuantumFourierTransform(int qubitCount = 3)
    {
        this.qubitCount = qubitCount;
        this.stateSize = 1 << qubitCount;
        this.quantumStates = new ComplexAmplitude[1][];
        this.random = new Random();

        InitializeQuantumState();
    }

    private void InitializeQuantumState()
    {
        quantumStates[0] = new ComplexAmplitude[stateSize];

        double amplitude = 1.0 / Math.Sqrt(stateSize);
        for (int i = 0; i < stateSize; i++)
        {
            double phase = random.NextDouble() * 2 * Math.PI;
            quantumStates[0][i] = new ComplexAmplitude
            {
                Real = amplitude * Math.Cos(phase),
                Imaginary = amplitude * Math.Sin(phase)
            };
        }
    }

    public void PrepareInitialState(int[] initialAmplitudes)
    {
        Console.WriteLine("\nPreparing initial quantum state...\n");

        quantumStates[0] = new ComplexAmplitude[stateSize];

        double norm = 0.0;
        for (int i = 0; i < Math.Min(initialAmplitudes.Length, stateSize); i++)
        {
            quantumStates[0][i].Real = initialAmplitudes[i];
            norm += initialAmplitudes[i] * initialAmplitudes[i];
        }

        norm = Math.Sqrt(norm);
        if (norm > 0)
        {
            for (int i = 0; i < stateSize; i++)
            {
                quantumStates[0][i].Real /= norm;
            }
        }

        Console.WriteLine("Initial state prepared and normalized");
    }

    public void ApplyQuantumFourierTransform()
    {
        Console.WriteLine("\nApplying Quantum Fourier Transform...\n");

        ComplexAmplitude[] inputState = quantumStates[0];
        ComplexAmplitude[] outputState = new ComplexAmplitude[stateSize];

        for (int k = 0; k < stateSize; k++)
        {
            ComplexAmplitude sum = new ComplexAmplitude { Real = 0, Imaginary = 0 };

            for (int n = 0; n < stateSize; n++)
            {
                double angle = -2 * Math.PI * k * n / stateSize;
                ComplexAmplitude basisVector = new ComplexAmplitude
                {
                    Real = Math.Cos(angle),
                    Imaginary = Math.Sin(angle)
                };

                ComplexAmplitude product = inputState[n].Multiply(basisVector);
                sum.Real += product.Real;
                sum.Imaginary += product.Imaginary;
            }

            sum.Real /= Math.Sqrt(stateSize);
            sum.Imaginary /= Math.Sqrt(stateSize);

            outputState[k] = sum;
        }

        quantumStates[0] = outputState;

        Console.WriteLine("Quantum Fourier Transform complete");
    }

    public void ApplyInverseQuantumFourierTransform()
    {
        Console.WriteLine("\nApplying Inverse Quantum Fourier Transform...\n");

        ComplexAmplitude[] inputState = quantumStates[0];
        ComplexAmplitude[] outputState = new ComplexAmplitude[stateSize];

        for (int n = 0; n < stateSize; n++)
        {
            ComplexAmplitude sum = new ComplexAmplitude { Real = 0, Imaginary = 0 };

            for (int k = 0; k < stateSize; k++)
            {
                double angle = 2 * Math.PI * k * n / stateSize;
                ComplexAmplitude basisVector = new ComplexAmplitude
                {
                    Real = Math.Cos(angle),
                    Imaginary = Math.Sin(angle)
                };

                ComplexAmplitude product = inputState[k].Multiply(basisVector);
                sum.Real += product.Real;
                sum.Imaginary += product.Imaginary;
            }

            sum.Real /= Math.Sqrt(stateSize);
            sum.Imaginary /= Math.Sqrt(stateSize);

            outputState[n] = sum;
        }

        quantumStates[0] = outputState;

        Console.WriteLine("Inverse Quantum Fourier Transform complete");
    }

    public void ApplyControlledPhaseShift(int controlQubit, int targetQubit, double phase)
    {
        Console.WriteLine($"\nApplying controlled phase shift (control: q{controlQubit}, target: q{targetQubit})...\n");

        ComplexAmplitude[] state = quantumStates[0];
        int controlMask = 1 << controlQubit;
        int targetMask = 1 << targetQubit;

        for (int i = 0; i < stateSize; i++)
        {
            if ((i & controlMask) != 0 && (i & targetMask) != 0)
            {
                double cosPhase = Math.Cos(phase);
                double sinPhase = Math.Sin(phase);

                double oldReal = state[i].Real;
                state[i].Real = oldReal * cosPhase - state[i].Imaginary * sinPhase;
                state[i].Imaginary = oldReal * sinPhase + state[i].Imaginary * cosPhase;
            }
        }

        quantumStates[0] = state;
    }

    public void ApplyHadamardGates()
    {
        Console.WriteLine("\nApplying Hadamard gates to all qubits...\n");

        ComplexAmplitude[] inputState = quantumStates[0];
        ComplexAmplitude[] outputState = new ComplexAmplitude[stateSize];

        double inv_sqrt2 = 1.0 / Math.Sqrt(2);

        for (int i = 0; i < stateSize; i++)
        {
            outputState[i] = new ComplexAmplitude { Real = 0, Imaginary = 0 };

            for (int j = 0; j < stateSize; j++)
            {
                int parity = 0;
                for (int k = 0; k < qubitCount; k++)
                {
                    if (((i >> k) & 1) == 1 && ((j >> k) & 1) == 1)
                        parity++;
                }

                double hadamardElement = Math.Pow(inv_sqrt2, qubitCount) * (parity % 2 == 0 ? 1 : -1);

                outputState[i].Real += inputState[j].Real * hadamardElement;
                outputState[i].Imaginary += inputState[j].Imaginary * hadamardElement;
            }
        }

        quantumStates[0] = outputState;

        Console.WriteLine("Hadamard gates applied");
    }

    public double[] GetProbabilityDistribution()
    {
        double[] probabilities = new double[stateSize];

        for (int i = 0; i < stateSize; i++)
        {
            probabilities[i] = quantumStates[0][i].Magnitude() * quantumStates[0][i].Magnitude();
        }

        return probabilities;
    }

    public void PrintProbabilityDistribution()
    {
        Console.WriteLine("\nProbability Distribution:");
        double[] probs = GetProbabilityDistribution();

        for (int i = 0; i < Math.Min(stateSize, 16); i++)
        {
            int barLength = (int)(probs[i] * 40);
            Console.Write($"  |{i:D2}⟩: ");
            for (int k = 0; k < barLength; k++)
                Console.Write("█");
            Console.WriteLine($" {probs[i]:F4}");
        }
    }

    public Dictionary<string, object> GetQFTMetrics()
    {
        double[] probabilities = GetProbabilityDistribution();

        double totalProbability = probabilities.Sum();
        double maxProbability = probabilities.Max();
        double entropy = 0.0;

        for (int i = 0; i < stateSize; i++)
        {
            if (probabilities[i] > 0)
            {
                entropy -= probabilities[i] * Math.Log(probabilities[i]) / Math.Log(2);
            }
        }

        return new Dictionary<string, object>
        {
            { "QubitCount", qubitCount },
            { "StateSize", stateSize },
            { "TotalProbability", totalProbability },
            { "MaxProbability", maxProbability },
            { "ShannonEntropy", entropy },
            { "GateComplexity", qubitCount * (qubitCount + 1) / 2 }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Fourier Transform - Spectral Basis Transform ===\n");

        var qft = new QuantumFourierTransform(qubitCount: 3);

        Console.WriteLine("--- Initial Probability Distribution ---");
        qft.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Initial QFT Metrics ---");
        var metricsInitial = qft.GetQFTMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Applying Quantum Fourier Transform ---");
        qft.ApplyQuantumFourierTransform();
        qft.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Applying Hadamard Gates ---");
        qft.ApplyHadamardGates();
        qft.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Applying Controlled Phase Shifts ---");
        qft.ApplyControlledPhaseShift(0, 1, Math.PI / 4);
        qft.ApplyControlledPhaseShift(1, 2, Math.PI / 2);

        Console.WriteLine("\n--- Applying Inverse QFT ---");
        qft.ApplyInverseQuantumFourierTransform();
        qft.PrintProbabilityDistribution();

        Console.WriteLine("\n--- Final QFT Metrics ---");
        var metricsFinal = qft.GetQFTMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Fourier Transform is essential for many quantum algorithms including Shor's factoring.");
    }
}
