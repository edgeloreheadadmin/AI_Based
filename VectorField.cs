using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Vector Field
/// A field with direction components, but no spin
/// Three-component vector at each spacetime point
/// </summary>
public class VectorField
{
    private struct VectorValue
    {
        public double X;
        public double Y;
        public double Z;

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public void Normalize()
        {
            double mag = Magnitude();
            if (mag > 0)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
            }
        }
    }

    private Dictionary<(int, int, int, int), VectorValue> fieldVectors;
    private int spatialSize;
    private int temporalSize;
    private Random random;

    public VectorField(int spatialSize = 8, int temporalSize = 10)
    {
        this.spatialSize = spatialSize;
        this.temporalSize = temporalSize;
        this.fieldVectors = new Dictionary<(int, int, int, int), VectorValue>();
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
                        VectorValue vec = new VectorValue
                        {
                            X = Math.Sin(x * Math.PI / spatialSize) * Math.Cos(y * Math.PI / spatialSize),
                            Y = Math.Cos(x * Math.PI / spatialSize) * Math.Sin(z * Math.PI / spatialSize),
                            Z = Math.Sin(y * Math.PI / spatialSize) * Math.Sin(z * Math.PI / spatialSize)
                        };
                        vec.Normalize();
                        fieldVectors[(t, x, y, z)] = vec;
                    }
                }
            }
        }
    }

    public VectorValue GetFieldVector(int t, int x, int y, int z)
    {
        if (fieldVectors.ContainsKey((t, x, y, z)))
            return fieldVectors[(t, x, y, z)];
        return new VectorValue { X = 0, Y = 0, Z = 0 };
    }

    public void SetFieldVector(int t, int x, int y, int z, double vx, double vy, double vz)
    {
        VectorValue vec = new VectorValue { X = vx, Y = vy, Z = vz };
        vec.Normalize();
        fieldVectors[(t, x, y, z)] = vec;
    }

    public double CalculateDivergence(int t, int x, int y, int z)
    {
        VectorValue center = GetFieldVector(t, x, y, z);
        VectorValue vx_plus = GetFieldVector(t, x + 1, y, z);
        VectorValue vx_minus = GetFieldVector(t, x - 1, y, z);
        VectorValue vy_plus = GetFieldVector(t, x, y + 1, z);
        VectorValue vy_minus = GetFieldVector(t, x, y - 1, z);
        VectorValue vz_plus = GetFieldVector(t, x, y, z + 1);
        VectorValue vz_minus = GetFieldVector(t, x, y, z - 1);

        double div_x = (vx_plus.X - vx_minus.X) / 2.0;
        double div_y = (vy_plus.Y - vy_minus.Y) / 2.0;
        double div_z = (vz_plus.Z - vz_minus.Z) / 2.0;

        return div_x + div_y + div_z;
    }

    public double CalculateCurl(int t, int x, int y, int z)
    {
        VectorValue vy_plus_z = GetFieldVector(t, x, y, z + 1);
        VectorValue vy_minus_z = GetFieldVector(t, x, y, z - 1);
        VectorValue vz_plus_y = GetFieldVector(t, x, y + 1, z);
        VectorValue vz_minus_y = GetFieldVector(t, x, y - 1, z);
        VectorValue vx_plus_z = GetFieldVector(t, x, y, z + 1);
        VectorValue vx_minus_z = GetFieldVector(t, x, y, z - 1);
        VectorValue vz_plus_x = GetFieldVector(t, x + 1, y, z);
        VectorValue vz_minus_x = GetFieldVector(t, x - 1, y, z);
        VectorValue vx_plus_y = GetFieldVector(t, x, y + 1, z);
        VectorValue vx_minus_y = GetFieldVector(t, x, y - 1, z);
        VectorValue vy_plus_x = GetFieldVector(t, x + 1, y, z);
        VectorValue vy_minus_x = GetFieldVector(t, x - 1, y, z);

        double curl_x = (vy_plus_z.Y - vy_minus_z.Y) / 2.0 - (vz_plus_y.Z - vz_minus_y.Z) / 2.0;
        double curl_y = (vz_plus_x.Z - vz_minus_x.Z) / 2.0 - (vx_plus_z.X - vx_minus_z.X) / 2.0;
        double curl_z = (vx_plus_y.X - vx_minus_y.X) / 2.0 - (vy_plus_x.Y - vy_minus_x.Y) / 2.0;

        return Math.Sqrt(curl_x * curl_x + curl_y * curl_y + curl_z * curl_z);
    }

    public void EvolveField(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving vector field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            var newVectors = new Dictionary<(int, int, int, int), VectorValue>(fieldVectors);

            foreach (var kvp in fieldVectors)
            {
                var (t, x, y, z) = kvp.Key;
                VectorValue vec = kvp.Value;

                double div = CalculateDivergence(t, x, y, z);
                double curl = CalculateCurl(t, x, y, z);

                VectorValue evolved = new VectorValue
                {
                    X = vec.X + couplingStrength * div * 0.01,
                    Y = vec.Y + couplingStrength * curl * 0.01,
                    Z = vec.Z + couplingStrength * (div + curl) * 0.01
                };
                evolved.Normalize();

                newVectors[(t, x, y, z)] = evolved;
            }

            fieldVectors = newVectors;

            if (step % (steps / 3) == 0)
            {
                double totalMagnitude = fieldVectors.Values.Sum(v => v.Magnitude());
                Console.WriteLine($"Step {step}: Total Vector Magnitude = {totalMagnitude:F6}");
            }
        }
    }

    public Dictionary<string, object> GetFieldMetrics()
    {
        double maxMagnitude = fieldVectors.Values.Max(v => v.Magnitude());
        double minMagnitude = fieldVectors.Values.Min(v => v.Magnitude());
        double avgMagnitude = fieldVectors.Values.Average(v => v.Magnitude());
        double totalDivergence = 0;

        for (int t = 0; t < temporalSize; t++)
        {
            for (int x = 0; x < spatialSize; x++)
            {
                for (int y = 0; y < spatialSize; y++)
                {
                    for (int z = 0; z < spatialSize; z++)
                    {
                        totalDivergence += Math.Abs(CalculateDivergence(t, x, y, z));
                    }
                }
            }
        }

        return new Dictionary<string, object>
        {
            { "GridPoints", fieldVectors.Count },
            { "MaxMagnitude", maxMagnitude },
            { "MinMagnitude", minMagnitude },
            { "AverageMagnitude", avgMagnitude },
            { "TotalDivergence", totalDivergence }
        };
    }

    public void PrintFieldSlice(int t, int z)
    {
        Console.WriteLine($"\nVector Field Slice (t={t}, z={z}):");
        for (int y = 0; y < spatialSize; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < spatialSize; x++)
            {
                VectorValue vec = GetFieldVector(t, x, y, z);
                double angle = Math.Atan2(vec.Y, vec.X);
                char symbol = ' ';
                if (angle > -Math.PI / 8 && angle <= Math.PI / 8) symbol = '→';
                else if (angle > Math.PI / 8 && angle <= 3 * Math.PI / 8) symbol = '↗';
                else if (angle > 3 * Math.PI / 8 && angle <= 5 * Math.PI / 8) symbol = '↑';
                else if (angle > 5 * Math.PI / 8 && angle <= 7 * Math.PI / 8) symbol = '↖';
                else if (angle > 7 * Math.PI / 8 || angle <= -7 * Math.PI / 8) symbol = '←';
                else if (angle > -7 * Math.PI / 8 && angle <= -5 * Math.PI / 8) symbol = '↙';
                else if (angle > -5 * Math.PI / 8 && angle <= -3 * Math.PI / 8) symbol = '↓';
                else if (angle > -3 * Math.PI / 8 && angle <= -Math.PI / 8) symbol = '↘';

                Console.Write(symbol);
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Vector Field - Direction, No Spin ===\n");

        var field = new VectorField(spatialSize: 8, temporalSize: 10);

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

        Console.WriteLine("\n--- Evolving Vector Field ---");
        field.EvolveField(steps: 20, couplingStrength: 0.3);

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

        Console.WriteLine("\nVector Field represents fields with directional components but no spin.");
    }
}
