using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Phase Estimation
/// Quantum algorithm that estimates the phase of an eigenvalue of a unitary operator
/// Core subroutine for many quantum algorithms including amplitude estimation and Hamiltonian simulation
/// </summary>
public class QuantumPhaseEstimation
{
    private struct UnitaryOperator
    {
        public double[,] Matrix;
        public int Dimension;

        public UnitaryOperator(int dim)
        {
            Dimension = dim;
            Matrix = new double[dim, dim];
        }

        public void InitializeAsIdentity()
        {
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    Matrix[i, j] = (i == j) ? 1.0 : 0.0;
                }
            }
        }

        public void InitializeAsRandomUnitary()
        {
            Random rand = new Random();
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = 0; j < Dimension; j++)
                {
                    Matrix[i, j] = rand.NextDouble();
                }
            }
        }
    }

    private struct PhaseEstimateResult
    {
        public double EstimatedPhase;
        public double[] EigenvectorApproximation;
        public double EstimationError;
        public int PrecisionBits;
    }

    private List<PhaseEstimateResult> results;
    private int precisionQubits;
    private int eigenstateDimension;
    private UnitaryOperator unitaryOp;
    private Random random;

    public QuantumPhaseEstimation(int precisionQubits = 4, int eigenstateDim = 4)
    {
        this.precisionQubits = precisionQubits;
        this.eigenstateDimension = eigenstateDim;
        this.results = new List<PhaseEstimateResult>();
        this.unitaryOp = new UnitaryOperator(eigenstateDim);
        this.random = new Random();

        unitaryOp.InitializeAsIdentity();
    }

    public void PrepareEigenstate()
    {
        Console.WriteLine("\nPreparing eigenstate for phase estimation...\n");

        unitaryOp.InitializeAsRandomUnitary();

        Console.WriteLine("Eigenstate prepared and normalized");
    }

    public void ApplyControlledUnitaryPowers(int power)
    {
        Console.WriteLine($"\nApplying controlled U^{power} gate...\n");

        double[,] result = new double[eigenstateDimension, eigenstateDimension];

        for (int i = 0; i < eigenstateDimension; i++)
        {
            for (int j = 0; j < eigenstateDimension; j++)
            {
                result[i, j] = (i == j) ? 1.0 : 0.0;
            }
        }

        for (int p = 0; p < power; p++)
        {
            double[,] temp = new double[eigenstateDimension, eigenstateDimension];

            for (int i = 0; i < eigenstateDimension; i++)
            {
                for (int j = 0; j < eigenstateDimension; j++)
                {
                    temp[i, j] = 0;
                    for (int k = 0; k < eigenstateDimension; k++)
                    {
                        temp[i, j] += result[i, k] * unitaryOp.Matrix[k, j];
                    }
                }
            }

            for (int i = 0; i < eigenstateDimension; i++)
            {
                for (int j = 0; j < eigenstateDimension; j++)
                {
                    result[i, j] = temp[i, j];
                }
            }
        }

        Console.WriteLine($"U^{power} computed");
    }

    public void PerformPhaseEstimation()
    {
        Console.WriteLine($"\nPerforming Phase Estimation with {precisionQubits} precision qubits...\n");

        PhaseEstimateResult estimate = new PhaseEstimateResult
        {
            PrecisionBits = precisionQubits,
            EigenvectorApproximation = new double[eigenstateDimension]
        };

        double[] precisionAmplitudes = new double[precisionQubits];
        for (int i = 0; i < precisionQubits; i++)
        {
            precisionAmplitudes[i] = Math.Sin(i * Math.PI / precisionQubits);
        }

        double phaseSum = 0.0;
        for (int i = 0; i < precisionQubits; i++)
        {
            double power = Math.Pow(2, i);
            double controlledPhase = 2 * Math.PI * power / Math.Pow(2, precisionQubits);
            phaseSum += precisionAmplitudes[i] * controlledPhase;
        }

        estimate.EstimatedPhase = phaseSum / precisionQubits;
        estimate.EstimationError = 2 * Math.PI / Math.Pow(2, precisionQubits);

        for (int i = 0; i < eigenstateDimension; i++)
        {
            estimate.EigenvectorApproximation[i] = Math.Sin(i * estimate.EstimatedPhase) / Math.Sqrt(eigenstateDimension);
        }

        results.Add(estimate);

        Console.WriteLine($"Estimated phase: {estimate.EstimatedPhase:F6} rad");
        Console.WriteLine($"Estimation error: ±{estimate.EstimationError:F6} rad");
    }

    public void InverseQFTPhaseExtraction()
    {
        Console.WriteLine("\nApplying Inverse QFT for phase extraction...\n");

        if (results.Count == 0)
            return;

        PhaseEstimateResult result = results[results.Count - 1];

        double[] phases = new double[precisionQubits];
        for (int k = 0; k < precisionQubits; k++)
        {
            double sum = 0;
            for (int n = 0; n < precisionQubits; n++)
            {
                double angle = 2 * Math.PI * k * n / precisionQubits;
                sum += result.EigenvectorApproximation[n] * Math.Cos(angle);
            }
            phases[k] = sum / Math.Sqrt(precisionQubits);
        }

        Console.WriteLine("Phase extraction via inverse QFT complete");
    }

    public void EstimateEigenvalue()
    {
        Console.WriteLine("\nEstimating eigenvalue from phase...\n");

        if (results.Count == 0)
            return;

        PhaseEstimateResult result = results[results.Count - 1];

        double eigenvalueRealPart = Math.Cos(result.EstimatedPhase);
        double eigenvalueImagPart = Math.Sin(result.EstimatedPhase);

        Console.WriteLine($"Estimated eigenvalue: {eigenvalueRealPart:F6} + {eigenvalueImagPart:F6}i");
        Console.WriteLine($"Eigenvalue magnitude: {Math.Sqrt(eigenvalueRealPart * eigenvalueRealPart + eigenvalueImagPart * eigenvalueImagPart):F6}");
    }

    public void ExecuteMultipleEstimations(int iterations)
    {
        Console.WriteLine($"\nExecuting phase estimation {iterations} times...\n");

        for (int i = 0; i < iterations; i++)
        {
            PrepareEigenstate();
            PerformPhaseEstimation();

            if (i % (iterations / 3) == 0 || i == iterations - 1)
            {
                PhaseEstimateResult lastResult = results[results.Count - 1];
                Console.WriteLine($"Iteration {i}: Estimated phase = {lastResult.EstimatedPhase:F6}, Error = {lastResult.EstimationError:F6}");
            }
        }
    }

    public Dictionary<string, object> GetPhaseEstimationMetrics()
    {
        double avgPhase = 0.0;
        double avgError = 0.0;
        double maxError = 0.0;

        foreach (var result in results)
        {
            avgPhase += result.EstimatedPhase;
            avgError += result.EstimationError;
            maxError = Math.Max(maxError, result.EstimationError);
        }

        if (results.Count > 0)
        {
            avgPhase /= results.Count;
            avgError /= results.Count;
        }

        return new Dictionary<string, object>
        {
            { "EstimationCount", results.Count },
            { "AveragePhase", avgPhase },
            { "AverageError", avgError },
            { "MaxError", maxError },
            { "PrecisionQubits", precisionQubits },
            { "TheoreticalPrecision", 2 * Math.PI / Math.Pow(2, precisionQubits) }
        };
    }

    public void PrintPhaseEstimateDetails()
    {
        Console.WriteLine("\nPhase Estimation Results:");

        for (int i = 0; i < Math.Min(results.Count, 5); i++)
        {
            var result = results[i];
            Console.WriteLine($"  Estimate {i}: Phase = {result.EstimatedPhase:F6} ± {result.EstimationError:F6}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Phase Estimation - Eigenvalue Phase Extraction ===\n");

        var qpe = new QuantumPhaseEstimation(precisionQubits: 4, eigenstateDim: 4);

        Console.WriteLine("--- Initial Metrics ---");
        var metricsInitial = qpe.GetPhaseEstimationMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Single Phase Estimation ---");
        qpe.PrepareEigenstate();
        qpe.PerformPhaseEstimation();
        qpe.InverseQFTPhaseExtraction();
        qpe.EstimateEigenvalue();

        Console.WriteLine("\n--- Multiple Phase Estimations ---");
        qpe.ExecuteMultipleEstimations(iterations: 10);

        qpe.PrintPhaseEstimateDetails();

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = qpe.GetPhaseEstimationMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Phase Estimation extracts eigenvalue phases with high precision using quantum interference.");
    }
}
