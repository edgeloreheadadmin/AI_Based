using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Flux
/// Flow of quantum quantity across a given surface
/// Demonstrates flux calculations and topological properties
/// </summary>
public class QuantumFlux
{
    private struct QuantumSurface
    {
        public int SurfaceId;
        public double[][] FieldValues;
        public double[][] FluxDensity;
        public int GridSize;
        public double TotalFlux;
        public double AverageFlux;

        public QuantumSurface(int id, int size)
        {
            SurfaceId = id;
            GridSize = size;
            FieldValues = new double[size][];
            FluxDensity = new double[size][];
            TotalFlux = 0.0;
            AverageFlux = 0.0;

            for (int i = 0; i < size; i++)
            {
                FieldValues[i] = new double[size];
                FluxDensity[i] = new double[size];
            }
        }
    }

    private List<QuantumSurface> surfaces;
    private Random random;
    private double gaussConstant = 8.854e-12;

    public QuantumFlux()
    {
        this.surfaces = new List<QuantumSurface>();
        this.random = new Random();
    }

    public void CreateQuantumSurface(int size)
    {
        var surface = new QuantumSurface(surfaces.Count, size);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                surface.FieldValues[i][j] = GenerateQuantumField(i, j, size);
            }
        }

        surfaces.Add(surface);
        Console.WriteLine($"Created quantum surface {surface.SurfaceId} ({size}x{size})");
    }

    private double GenerateQuantumField(int i, int j, int size)
    {
        double x = (double)i / size * Math.PI * 2;
        double y = (double)j / size * Math.PI * 2;

        return Math.Sin(x) * Math.Cos(y) + 0.5 * Math.Sin(2 * x) * Math.Cos(2 * y);
    }

    public void CalculateFluxThroughSurface(int surfaceId)
    {
        if (surfaceId >= surfaces.Count)
            return;

        var surface = surfaces[surfaceId];
        double totalFlux = 0;

        for (int i = 0; i < surface.GridSize; i++)
        {
            for (int j = 0; j < surface.GridSize; j++)
            {
                double fx = 0, fy = 0;

                if (i > 0 && i < surface.GridSize - 1)
                    fx = (surface.FieldValues[i + 1][j] - surface.FieldValues[i - 1][j]) / 2.0;

                if (j > 0 && j < surface.GridSize - 1)
                    fy = (surface.FieldValues[i][j + 1] - surface.FieldValues[i][j - 1]) / 2.0;

                double magnitude = Math.Sqrt(fx * fx + fy * fy);
                surface.FluxDensity[i][j] = magnitude;
                totalFlux += magnitude;
            }
        }

        surface.TotalFlux = totalFlux;
        surface.AverageFlux = totalFlux / (surface.GridSize * surface.GridSize);
        surfaces[surfaceId] = surface;
    }

    public void ApplyGaussLaw()
    {
        foreach (var surface in surfaces)
        {
            double enclosedCharge = CalculateEnclosedCharge(surface);
            double fluxIntegral = IntegrateFlux(surface);

            double chargeToFluxRatio = Math.Abs(fluxIntegral) > 0 ?
                enclosedCharge / fluxIntegral : 0;

            Console.WriteLine($"Surface {surface.SurfaceId}: Charge={enclosedCharge:E4}, " +
                            $"Flux={fluxIntegral:E4}, Ratio={chargeToFluxRatio:E4}");
        }
    }

    private double CalculateEnclosedCharge(QuantumSurface surface)
    {
        double charge = 0;

        for (int i = 1; i < surface.GridSize - 1; i++)
        {
            for (int j = 1; j < surface.GridSize - 1; j++)
            {
                double laplacian = surface.FieldValues[i + 1][j] +
                                 surface.FieldValues[i - 1][j] +
                                 surface.FieldValues[i][j + 1] +
                                 surface.FieldValues[i][j - 1] -
                                 4 * surface.FieldValues[i][j];

                charge += laplacian;
            }
        }

        return charge / gaussConstant;
    }

    private double IntegrateFlux(QuantumSurface surface)
    {
        double fluxSum = 0;

        for (int i = 0; i < surface.GridSize; i++)
        {
            for (int j = 0; j < surface.GridSize; j++)
            {
                fluxSum += surface.FluxDensity[i][j];
            }
        }

        return fluxSum;
    }

    public void CalculateTopologicalCharge()
    {
        Console.WriteLine("\nCalculating Topological Charge (Chern Number)...\n");

        foreach (var surface in surfaces)
        {
            double chernNumber = 0;

            for (int i = 1; i < surface.GridSize - 1; i++)
            {
                for (int j = 1; j < surface.GridSize - 1; j++)
                {
                    double curvature = surface.FieldValues[i + 1][j + 1] -
                                     surface.FieldValues[i + 1][j - 1] -
                                     surface.FieldValues[i - 1][j + 1] +
                                     surface.FieldValues[i - 1][j - 1];

                    chernNumber += curvature;
                }
            }

            chernNumber /= (2 * Math.PI * surface.GridSize * surface.GridSize);
            Console.WriteLine($"Surface {surface.SurfaceId}: Chern Number = {chernNumber:F6}");
        }
    }

    public Dictionary<string, object> GetFluxMetrics()
    {
        double totalFlux = surfaces.Sum(s => s.TotalFlux);
        double avgFlux = surfaces.Average(s => s.AverageFlux);
        double maxFlux = surfaces.Max(s => s.TotalFlux);

        return new Dictionary<string, object>
        {
            { "SurfaceCount", surfaces.Count },
            { "TotalFlux", totalFlux },
            { "AverageFlux", avgFlux },
            { "MaxFlux", maxFlux },
            { "GaussConstant", gaussConstant }
        };
    }

    public void PrintFluxVisualization(int surfaceId, int limit = 10)
    {
        if (surfaceId >= surfaces.Count)
            return;

        var surface = surfaces[surfaceId];
        Console.WriteLine($"Flux Density Map (Surface {surfaceId}):");

        for (int i = 0; i < Math.Min(limit, surface.GridSize); i++)
        {
            Console.Write("  ");
            for (int j = 0; j < Math.Min(limit, surface.GridSize); j++)
            {
                double flux = surface.FluxDensity[i][j];
                if (flux < 0.25)
                    Console.Write(". ");
                else if (flux < 0.5)
                    Console.Write("+ ");
                else if (flux < 0.75)
                    Console.Write("* ");
                else
                    Console.Write("# ");
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Flux - Surface Flux Calculations ===\n");

        var qf = new QuantumFlux();

        Console.WriteLine("--- Creating Quantum Surfaces ---");
        qf.CreateQuantumSurface(16);
        qf.CreateQuantumSurface(20);

        Console.WriteLine("\n--- Calculating Flux Through Surfaces ---");
        qf.CalculateFluxThroughSurface(0);
        qf.CalculateFluxThroughSurface(1);

        Console.WriteLine("\n--- Applying Gauss's Law ---");
        qf.ApplyGaussLaw();

        Console.WriteLine("\n--- Calculating Topological Charge ---");
        qf.CalculateTopologicalCharge();

        Console.WriteLine("\n--- Flux Metrics ---");
        var metrics = qf.GetFluxMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Flux Visualization ---");
        qf.PrintFluxVisualization(0, 12);

        Console.WriteLine("\nQuantum Flux successfully calculates surface flux and topological properties.");
    }
}
