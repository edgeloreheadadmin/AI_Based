using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Spinor Field
/// A field with both spin and direction components
/// Four-component complex-valued spinor at each spacetime point
/// </summary>
public class SpinorField
{
    private struct ComplexNumber
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
    }

    private struct Spinor
    {
        public ComplexNumber[] Components;

        public Spinor()
        {
            Components = new ComplexNumber[4];
            for (int i = 0; i < 4; i++)
            {
                Components[i] = new ComplexNumber { Real = 0, Imaginary = 0 };
            }
        }

        public double Norm()
        {
            double sum = 0;
            for (int i = 0; i < 4; i++)
            {
                sum += Components[i].Magnitude() * Components[i].Magnitude();
            }
            return Math.Sqrt(sum);
        }

        public void Normalize()
        {
            double norm = Norm();
            if (norm > 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    Components[i].Real /= norm;
                    Components[i].Imaginary /= norm;
                }
            }
        }

        public double Chirality()
        {
            return (Components[0].Magnitude() + Components[1].Magnitude()) -
                   (Components[2].Magnitude() + Components[3].Magnitude());
        }
    }

    private Dictionary<(int, int, int, int), Spinor> fieldSpinors;
    private int spatialSize;
    private int temporalSize;
    private Random random;
    private const double HBAR = 1.054e-34;

    public SpinorField(int spatialSize = 8, int temporalSize = 10)
    {
        this.spatialSize = spatialSize;
        this.temporalSize = temporalSize;
        this.fieldSpinors = new Dictionary<(int, int, int, int), Spinor>();
        this.random = new Random();

        InitializeField();
    }

    private void InitializeField()
    {
        for (int t = 0; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                for (int y = 0; y < spatialSize; y++)
                {
                    for (int z = 0; z < spatialSize; z++)
                    {
                        Spinor spinor = new Spinor();

                        double phase = x * Math.PI / spatialSize + y * Math.PI / spatialSize;
                        double mag = 1.0 / 2.0;

                        spinor.Components[0].Real = mag * Math.Cos(phase);
                        spinor.Components[0].Imaginary = mag * Math.Sin(phase);
                        spinor.Components[1].Real = mag * Math.Cos(phase + Math.PI / 2);
                        spinor.Components[1].Imaginary = mag * Math.Sin(phase + Math.PI / 2);
                        spinor.Components[2].Real = mag * Math.Cos(phase + Math.PI);
                        spinor.Components[2].Imaginary = mag * Math.Sin(phase + Math.PI);
                        spinor.Components[3].Real = mag * Math.Cos(phase + 3 * Math.PI / 2);
                        spinor.Components[3].Imaginary = mag * Math.Sin(phase + 3 * Math.PI / 2);

                        spinor.Normalize();
                        fieldSpinors[(t, x, y, z)] = spinor;
                    }
                }
            }
        }
    }

    public Spinor GetFieldSpinor(int t, int x, int y, int z)
    {
        if (fieldSpinors.ContainsKey((t, x, y, z)))
            return fieldSpinors[(t, x, y, z)];
        return new Spinor();
    }

    public void SetFieldSpinor(int t, int x, int y, int z, Spinor spinor)
    {
        spinor.Normalize();
        fieldSpinors[(t, x, y, z)] = spinor;
    }

    public double CalculateSpinDensity(int t, int x, int y, int z)
    {
        Spinor spinor = GetFieldSpinor(t, x, y, z);
        return spinor.Chirality();
    }

    public double CalculateProbabilityDensity(int t, int x, int y, int z)
    {
        Spinor spinor = GetFieldSpinor(t, x, y, z);
        return spinor.Norm() * spinor.Norm();
    }

    public double CalculatePhaseGradient(int t, int x, int y, int z)
    {
        Spinor center = GetFieldSpinor(t, x, y, z);
        Spinor x_plus = GetFieldSpinor(t, x + 1, y, z);
        Spinor y_plus = GetFieldSpinor(t, x, y + 1, z);
        Spinor z_plus = GetFieldSpinor(t, x, y, z + 1);

        double phase_center = center.Components[0].Phase();
        double phase_xp = x_plus.Components[0].Phase();
        double phase_yp = y_plus.Components[0].Phase();
        double phase_zp = z_plus.Components[0].Phase();

        double grad_x = (phase_xp - phase_center);
        double grad_y = (phase_yp - phase_center);
        double grad_z = (phase_zp - phase_center);

        return Math.Sqrt(grad_x * grad_x + grad_y * grad_y + grad_z * grad_z);
    }

    public void EvolveField(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving spinor field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            var newSpinors = new Dictionary<(int, int, int, int), Spinor>(fieldSpinors);

            foreach (var kvp in fieldSpinors)
            {
                var (t, x, y, z) = kvp.Key;
                Spinor spinor = kvp.Value;

                double phaseGrad = CalculatePhaseGradient(t, x, y, z);
                double momentum = HBAR * phaseGrad;

                Spinor evolved = new Spinor();
                for (int i = 0; i < 4; i++)
                {
                    double rotationAngle = couplingStrength * momentum * 0.01;
                    evolved.Components[i].Real = spinor.Components[i].Real * Math.Cos(rotationAngle) -
                                                 spinor.Components[i].Imaginary * Math.Sin(rotationAngle);
                    evolved.Components[i].Imaginary = spinor.Components[i].Real * Math.Sin(rotationAngle) +
                                                      spinor.Components[i].Imaginary * Math.Cos(rotationAngle);
                }
                evolved.Normalize();

                newSpinors[(t, x, y, z)] = evolved;
            }

            fieldSpinors = newSpinors;

            if (step % (steps / 3) == 0)
            {
                double totalProbability = fieldSpinors.Values.Sum(s => s.Norm() * s.Norm());
                Console.WriteLine($"Step {step}: Total Probability = {totalProbability:F6}");
            }
        }
    }

    public Dictionary<string, object> GetFieldMetrics()
    {
        double totalProbability = fieldSpinors.Values.Sum(s => s.Norm() * s.Norm());
        double avgChirality = fieldSpinors.Values.Average(s => Math.Abs(s.Chirality()));
        double maxSpinDensity = fieldSpinors.Values.Max(s => Math.Abs(s.Chirality()));

        double totalAngularMomentum = 0;
        foreach (var spinor in fieldSpinors.Values)
        {
            for (int i = 0; i < 4; i++)
            {
                totalAngularMomentum += spinor.Components[i].Magnitude();
            }
        }

        return new Dictionary<string, object>
        {
            { "GridPoints", fieldSpinors.Count },
            { "TotalProbability", totalProbability },
            { "AverageChirality", avgChirality },
            { "MaxSpinDensity", maxSpinDensity },
            { "TotalAngularMomentum", totalAngularMomentum }
        };
    }

    public void PrintFieldSlice(int t, int z)
    {
        Console.WriteLine($"\nSpinor Field Slice (t={t}, z={z}) - Chirality:");
        for (int y = 0; y < spatialSize; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < spatialSize; x++)
            {
                double chirality = CalculateSpinDensity(t, x, y, z);
                char symbol = chirality > 0.3 ? '↑' : chirality < -0.3 ? '↓' : '○';
                Console.Write(symbol);
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Spinor Field - Spin and Direction ===\n");

        var field = new SpinorField(spatialSize: 8, temporalSize: 10);

        Console.WriteLine("--- Initial Field State ---");
        field.PrintFieldSlice(0, 4);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = field.GetFieldMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Spinor Field ---");
        field.EvolveField(steps: 20, couplingStrength: 0.4);

        field.PrintFieldSlice(0, 4);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = field.GetFieldMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSpinor Field represents fields with both spin and directional components.");
    }
}
