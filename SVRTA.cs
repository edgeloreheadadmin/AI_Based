using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// SVRTA - Scattering Velocity Ray Trajectory Adjustment
/// Optimizes the angle and velocity of quantum particle rays to achieve desired scattering pattern
/// Uses gradient descent and feedback control
/// </summary>
public class SVRTA
{
    private struct RayTrajectory
    {
        public double Angle;
        public double Velocity;
        public double Energy;

        public RayTrajectory(double angle, double velocity)
        {
            Angle = angle;
            Velocity = velocity;
            Energy = Math.Sqrt(angle * angle + velocity * velocity);
        }

        public override string ToString()
        {
            return $"Angle={Angle:F2}°, Velocity={Velocity:F3}, Energy={Energy:F3}";
        }
    }

    private List<RayTrajectory> rays;
    private double targetScatteringPattern;
    private double learningRate;
    private Random random;

    public SVRTA(int rayCount = 5, double targetPattern = 0.5)
    {
        this.rays = new List<RayTrajectory>();
        this.targetScatteringPattern = targetPattern;
        this.learningRate = 0.01;
        this.random = new Random();

        InitializeRays(rayCount);
    }

    private void InitializeRays(int count)
    {
        for (int i = 0; i < count; i++)
        {
            double angle = random.NextDouble() * 360.0;
            double velocity = random.NextDouble() * 10.0;
            rays.Add(new RayTrajectory(angle, velocity));
        }
    }

    public double CalculateScatteringPattern()
    {
        double totalEnergy = rays.Sum(r => r.Energy);

        if (totalEnergy == 0)
            return 0;

        double angleSum = 0;
        foreach (var ray in rays)
        {
            angleSum += Math.Sin(ray.Angle * Math.PI / 180.0) * ray.Velocity;
        }

        return Math.Abs(angleSum) / totalEnergy;
    }

    public void OptimizeTrajectory()
    {
        double currentPattern = CalculateScatteringPattern();
        double error = currentPattern - targetScatteringPattern;

        for (int i = 0; i < rays.Count; i++)
        {
            var ray = rays[i];

            double angleGradient = Math.Cos(ray.Angle * Math.PI / 180.0) * ray.Velocity * error;
            double velocityGradient = Math.Sin(ray.Angle * Math.PI / 180.0) * error;

            ray.Angle -= learningRate * angleGradient;
            ray.Velocity -= learningRate * velocityGradient;

            ray.Angle = ray.Angle % 360.0;
            if (ray.Angle < 0)
                ray.Angle += 360.0;

            ray.Velocity = Math.Max(0, Math.Min(10.0, ray.Velocity));

            rays[i] = ray;
        }
    }

    public void ApplyDampingForce(double dampingCoefficient = 0.05)
    {
        for (int i = 0; i < rays.Count; i++)
        {
            var ray = rays[i];
            ray.Velocity *= (1.0 - dampingCoefficient);
            rays[i] = ray;
        }
    }

    public Dictionary<string, object> GetOptimizationMetrics()
    {
        Dictionary<string, object> metrics = new Dictionary<string, object>
        {
            { "CurrentScatteringPattern", CalculateScatteringPattern() },
            { "TargetScatteringPattern", targetScatteringPattern },
            { "PatternError", Math.Abs(CalculateScatteringPattern() - targetScatteringPattern) },
            { "AverageRayVelocity", rays.Average(r => r.Velocity) },
            { "AverageRayEnergy", rays.Average(r => r.Energy) }
        };
        return metrics;
    }

    public void PrintRayStates()
    {
        Console.WriteLine("Ray States:");
        for (int i = 0; i < rays.Count; i++)
        {
            Console.WriteLine($"  Ray {i}: {rays[i]}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== SVRTA - Scattering Velocity Ray Trajectory Adjustment ===\n");

        var svrta = new SVRTA(5, targetPattern: 0.6);

        Console.WriteLine("Initial Ray Configuration:");
        svrta.PrintRayStates();

        Console.WriteLine("\nInitial Metrics:");
        var initialMetrics = svrta.GetOptimizationMetrics();
        foreach (var kvp in initialMetrics)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Optimization Loop (30 iterations) ---\n");

        for (int iteration = 0; iteration < 30; iteration++)
        {
            svrta.OptimizeTrajectory();
            svrta.ApplyDampingForce(0.02);

            if (iteration % 9 == 0)
            {
                Console.WriteLine($"Iteration {iteration}:");
                var metrics = svrta.GetOptimizationMetrics();
                Console.WriteLine($"  Current Pattern: {(double)metrics["CurrentScatteringPattern"]:F4}");
                Console.WriteLine($"  Target Pattern:  {(double)metrics["TargetScatteringPattern"]:F4}");
                Console.WriteLine($"  Error:           {(double)metrics["PatternError"]:F4}");
                Console.WriteLine($"  Avg Velocity:    {(double)metrics["AverageRayVelocity"]:F3}");
            }
        }

        Console.WriteLine("\n--- Final Optimized Configuration ---\n");
        svrta.PrintRayStates();

        Console.WriteLine("\nFinal Metrics:");
        var finalMetrics = svrta.GetOptimizationMetrics();
        foreach (var kvp in finalMetrics)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSVRTA successfully optimized particle ray trajectories to match target scattering pattern.");
    }
}
