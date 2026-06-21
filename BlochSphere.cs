using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Bloch Sphere
/// Geometric representation of qubit states as points on a unit sphere
/// Each point represents a unique qubit state defined by amplitude, phase, and rotation angles
/// </summary>
public class BlochSphere
{
    private struct BlochPoint
    {
        public double X;
        public double Y;
        public double Z;

        public double GetRadius()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public double GetTheta()
        {
            return Math.Acos(Z);
        }

        public double GetPhi()
        {
            return Math.Atan2(Y, X);
        }

        public void NormalizeToUnitSphere()
        {
            double radius = GetRadius();
            if (radius > 0)
            {
                X /= radius;
                Y /= radius;
                Z /= radius;
            }
        }

        public (double alpha_real, double alpha_imag, double beta_real, double beta_imag) ToQubitState()
        {
            double theta = GetTheta();
            double phi = GetPhi();

            double alpha_real = Math.Cos(theta / 2);
            double alpha_imag = 0;
            double beta_real = Math.Sin(theta / 2) * Math.Cos(phi);
            double beta_imag = Math.Sin(theta / 2) * Math.Sin(phi);

            return (alpha_real, alpha_imag, beta_real, beta_imag);
        }
    }

    private List<BlochPoint> qubitStates;
    private Random random;

    public BlochSphere()
    {
        this.qubitStates = new List<BlochPoint>();
        this.random = new Random();
    }

    public void CreateQubitState(double theta, double phi)
    {
        Console.WriteLine($"\nCreating qubit state at θ={theta:F4}, φ={phi:F4}...\n");

        BlochPoint point = new BlochPoint
        {
            X = Math.Sin(theta) * Math.Cos(phi),
            Y = Math.Sin(theta) * Math.Sin(phi),
            Z = Math.Cos(theta)
        };

        point.NormalizeToUnitSphere();
        qubitStates.Add(point);

        Console.WriteLine($"Bloch point created: ({point.X:F4}, {point.Y:F4}, {point.Z:F4})");
    }

    public void RandomizeQubitState()
    {
        double theta = random.NextDouble() * Math.PI;
        double phi = random.NextDouble() * 2 * Math.PI;

        CreateQubitState(theta, phi);
    }

    public void ApplyPauliX(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine("\nApplying Pauli-X rotation (π around X-axis)...\n");

        BlochPoint point = qubitStates[stateIndex];
        point.Y = -point.Y;
        point.Z = -point.Z;

        qubitStates[stateIndex] = point;
    }

    public void ApplyPauliY(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine("\nApplying Pauli-Y rotation (π around Y-axis)...\n");

        BlochPoint point = qubitStates[stateIndex];
        point.X = -point.X;
        point.Z = -point.Z;

        qubitStates[stateIndex] = point;
    }

    public void ApplyPauliZ(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine("\nApplying Pauli-Z rotation (π around Z-axis)...\n");

        BlochPoint point = qubitStates[stateIndex];
        point.X = -point.X;
        point.Y = -point.Y;

        qubitStates[stateIndex] = point;
    }

    public void ApplyArbitraryRotation(int stateIndex, double theta, double phi, double rotationAngle)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        Console.WriteLine($"\nApplying arbitrary rotation: θ={theta:F4}, φ={phi:F4}, angle={rotationAngle:F4}...\n");

        BlochPoint point = qubitStates[stateIndex];

        double nx = Math.Sin(theta) * Math.Cos(phi);
        double ny = Math.Sin(theta) * Math.Sin(phi);
        double nz = Math.Cos(theta);

        double cosAngle = Math.Cos(rotationAngle / 2);
        double sinAngle = Math.Sin(rotationAngle / 2);

        double x = point.X;
        double y = point.Y;
        double z = point.Z;

        point.X = (nx * nx * (1 - cosAngle) + cosAngle) * x +
                  (nx * ny * (1 - cosAngle) - nz * sinAngle) * y +
                  (nx * nz * (1 - cosAngle) + ny * sinAngle) * z;

        point.Y = (nx * ny * (1 - cosAngle) + nz * sinAngle) * x +
                  (ny * ny * (1 - cosAngle) + cosAngle) * y +
                  (ny * nz * (1 - cosAngle) - nx * sinAngle) * z;

        point.Z = (nx * nz * (1 - cosAngle) - ny * sinAngle) * x +
                  (ny * nz * (1 - cosAngle) + nx * sinAngle) * y +
                  (nz * nz * (1 - cosAngle) + cosAngle) * z;

        point.NormalizeToUnitSphere();
        qubitStates[stateIndex] = point;
    }

    public double GetMeasurementProbability0(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return 0.5;

        BlochPoint point = qubitStates[stateIndex];
        return (1 + point.Z) / 2.0;
    }

    public double GetMeasurementProbability1(int stateIndex)
    {
        return 1.0 - GetMeasurementProbability0(stateIndex);
    }

    public void PrintBlochSphere(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        BlochPoint point = qubitStates[stateIndex];
        double theta = point.GetTheta();
        double phi = point.GetPhi();
        double prob0 = GetMeasurementProbability0(stateIndex);

        Console.WriteLine($"\nBloch Sphere Representation:");
        Console.WriteLine($"  Cartesian: X={point.X:F4}, Y={point.Y:F4}, Z={point.Z:F4}");
        Console.WriteLine($"  Spherical: θ={theta:F4} rad, φ={phi:F4} rad");
        Console.WriteLine($"  Radius: {point.GetRadius():F4}");
        Console.WriteLine($"  P(|0⟩) = {prob0:F4}, P(|1⟩) = {1 - prob0:F4}");

        var (alpha_real, alpha_imag, beta_real, beta_imag) = point.ToQubitState();
        Console.WriteLine($"  Qubit State: |ψ⟩ = {alpha_real:F4}|0⟩ + ({beta_real:F4} + {beta_imag:F4}i)|1⟩");
    }

    public void VisualizeBlochPoint(int stateIndex)
    {
        if (stateIndex < 0 || stateIndex >= qubitStates.Count)
            return;

        BlochPoint point = qubitStates[stateIndex];
        Console.WriteLine("\nBloch Sphere Visualization:");

        Console.WriteLine("       |0⟩");
        Console.WriteLine("        ↑ (Z+)");
        Console.WriteLine("        |");
        Console.WriteLine("  |1⟩—→ • (point)");
        Console.WriteLine("        |");
        Console.WriteLine("        ↓ (-Z)");
        Console.WriteLine("       |1⟩");
        Console.WriteLine();

        int zLevel = (int)(point.Z * 5) + 5;
        Console.WriteLine($"  Z-level: {zLevel}/10 (|0⟩ heavy: {point.Z > 0})");
    }

    public Dictionary<string, object> GetBlochMetrics()
    {
        double avgX = 0, avgY = 0, avgZ = 0;
        double maxDeviation = 0;

        foreach (var point in qubitStates)
        {
            avgX += point.X;
            avgY += point.Y;
            avgZ += point.Z;

            double deviation = Math.Abs(1.0 - point.GetRadius());
            maxDeviation = Math.Max(maxDeviation, deviation);
        }

        if (qubitStates.Count > 0)
        {
            avgX /= qubitStates.Count;
            avgY /= qubitStates.Count;
            avgZ /= qubitStates.Count;
        }

        return new Dictionary<string, object>
        {
            { "QubitCount", qubitStates.Count },
            { "AverageX", avgX },
            { "AverageY", avgY },
            { "AverageZ", avgZ },
            { "MaxRadiusDeviation", maxDeviation }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Bloch Sphere - Qubit State Geometry ===\n");

        var bloch = new BlochSphere();

        Console.WriteLine("--- Creating Initial Qubit States ---");
        bloch.CreateQubitState(theta: 0, phi: 0);
        bloch.CreateQubitState(theta: Math.PI / 2, phi: 0);
        bloch.CreateQubitState(theta: Math.PI / 2, phi: Math.PI / 2);

        Console.WriteLine("\n--- Initial Bloch Metrics ---");
        var metricsInitial = bloch.GetBlochMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Displaying Bloch Point ---");
        bloch.PrintBlochSphere(0);
        bloch.VisualizeBlochPoint(0);

        Console.WriteLine("\n--- Applying Pauli Rotations ---");
        bloch.ApplyPauliX(0);
        bloch.PrintBlochSphere(0);

        bloch.ApplyPauliY(0);
        bloch.PrintBlochSphere(0);

        Console.WriteLine("\n--- Arbitrary Rotation ---");
        bloch.ApplyArbitraryRotation(0, theta: Math.PI / 4, phi: Math.PI / 3, rotationAngle: Math.PI / 3);
        bloch.PrintBlochSphere(0);

        Console.WriteLine("\n--- Final Bloch Metrics ---");
        var metricsFinal = bloch.GetBlochMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nBloch Sphere provides geometric intuition for single-qubit states and operations.");
    }
}
