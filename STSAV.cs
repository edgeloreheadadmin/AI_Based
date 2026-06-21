using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// STSAV - Scattering Target System Accurate Velocity
/// Uses field ray interjection systems to accurately inject field rays into target areas
/// Implements precision targeting and field manipulation
/// </summary>
public class STSAV
{
    private struct FieldRay
    {
        public double X, Y, Z;
        public double Velocity;
        public double Intensity;
        public bool Active;

        public FieldRay(double x, double y, double z, double velocity, double intensity)
        {
            X = x;
            Y = y;
            Z = z;
            Velocity = velocity;
            Intensity = intensity;
            Active = true;
        }

        public double Distance(double targetX, double targetY, double targetZ)
        {
            return Math.Sqrt(
                Math.Pow(X - targetX, 2) +
                Math.Pow(Y - targetY, 2) +
                Math.Pow(Z - targetZ, 2)
            );
        }

        public override string ToString()
        {
            return $"({X:F2}, {Y:F2}, {Z:F2}) V={Velocity:F2} I={Intensity:F3}";
        }
    }

    private List<FieldRay> fieldRays;
    private double targetX, targetY, targetZ;
    private double targetRadius;
    private Random random;
    private int hitsInTarget;

    public STSAV(double targetX, double targetY, double targetZ, double targetRadius = 1.0)
    {
        this.targetX = targetX;
        this.targetY = targetY;
        this.targetZ = targetZ;
        this.targetRadius = targetRadius;
        this.fieldRays = new List<FieldRay>();
        this.random = new Random();
        this.hitsInTarget = 0;
    }

    public void InjectFieldRay(double posX, double posY, double posZ, double velocity, double intensity)
    {
        var ray = new FieldRay(posX, posY, posZ, velocity, intensity);
        fieldRays.Add(ray);
    }

    public void InjectMultipleRays(int count)
    {
        for (int i = 0; i < count; i++)
        {
            double posX = targetX + (random.NextDouble() - 0.5) * 5.0;
            double posY = targetY + (random.NextDouble() - 0.5) * 5.0;
            double posZ = targetZ + (random.NextDouble() - 0.5) * 5.0;
            double velocity = 0.5 + random.NextDouble() * 2.0;
            double intensity = 0.3 + random.NextDouble() * 0.7;

            InjectFieldRay(posX, posY, posZ, velocity, intensity);
        }
    }

    public void PropagateRays(double timeStep)
    {
        for (int i = 0; i < fieldRays.Count; i++)
        {
            if (!fieldRays[i].Active)
                continue;

            var ray = fieldRays[i];

            double dX = targetX - ray.X;
            double dY = targetY - ray.Y;
            double dZ = targetZ - ray.Z;
            double distance = Math.Sqrt(dX * dX + dY * dY + dZ * dZ);

            if (distance > 0.01)
            {
                double moveX = (dX / distance) * ray.Velocity * timeStep;
                double moveY = (dY / distance) * ray.Velocity * timeStep;
                double moveZ = (dZ / distance) * ray.Velocity * timeStep;

                ray.X += moveX;
                ray.Y += moveY;
                ray.Z += moveZ;

                ray.Intensity *= 0.98;

                if (ray.Intensity < 0.01)
                    ray.Active = false;

                fieldRays[i] = ray;

                double distToTarget = ray.Distance(targetX, targetY, targetZ);
                if (distToTarget <= targetRadius && ray.Active)
                {
                    hitsInTarget++;
                    ray.Active = false;
                    fieldRays[i] = ray;
                }
            }
        }
    }

    public Dictionary<string, object> GetSystemMetrics()
    {
        int activeRays = fieldRays.Count(r => r.Active);
        double avgIntensity = fieldRays.Where(r => r.Active).Average(r => r.Intensity);
        double avgDistance = fieldRays.Where(r => r.Active).Average(r => r.Distance(targetX, targetY, targetZ));

        Dictionary<string, object> metrics = new Dictionary<string, object>
        {
            { "TotalRays", fieldRays.Count },
            { "ActiveRays", activeRays },
            { "HitsInTarget", hitsInTarget },
            { "HitAccuracy", fieldRays.Count > 0 ? (double)hitsInTarget / fieldRays.Count : 0.0 },
            { "AvgIntensity", activeRays > 0 ? avgIntensity : 0.0 },
            { "AvgDistanceToTarget", activeRays > 0 ? avgDistance : 0.0 }
        };
        return metrics;
    }

    public void PrintFieldRayStatus()
    {
        Console.WriteLine("Active Field Rays:");
        foreach (var ray in fieldRays.Where(r => r.Active).Take(5))
        {
            double distToTarget = ray.Distance(targetX, targetY, targetZ);
            Console.WriteLine($"  Position: {ray}, Distance to Target: {distToTarget:F3}");
        }

        if (fieldRays.Count(r => r.Active) > 5)
            Console.WriteLine($"  ... and {fieldRays.Count(r => r.Active) - 5} more rays");
    }

    public static void Main()
    {
        Console.WriteLine("=== STSAV - Scattering Target System Accurate Velocity ===\n");

        var stsav = new STSAV(targetX: 0.0, targetY: 0.0, targetZ: 0.0, targetRadius: 0.5);

        Console.WriteLine("Target Position: (0.0, 0.0, 0.0)");
        Console.WriteLine("Target Radius: 0.5\n");

        Console.WriteLine("--- Injecting 15 Field Rays ---");
        stsav.InjectMultipleRays(15);

        Console.WriteLine("\nInitial Metrics:");
        var initialMetrics = stsav.GetSystemMetrics();
        foreach (var kvp in initialMetrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Propagating Rays (20 time steps) ---\n");

        for (int step = 0; step < 20; step++)
        {
            stsav.PropagateRays(timeStep: 0.1);

            if (step % 6 == 0)
            {
                Console.WriteLine($"Step {step}:");
                stsav.PrintFieldRayStatus();
                Console.WriteLine();
            }
        }

        Console.WriteLine("--- Final System State ---\n");
        var finalMetrics = stsav.GetSystemMetrics();
        foreach (var kvp in finalMetrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSTSAV successfully injected field rays into target area with controlled accuracy.");
    }
}
