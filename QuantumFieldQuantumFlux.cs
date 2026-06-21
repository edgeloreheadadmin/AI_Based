using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Field Quantum Flux
/// Flow of energy or particles across a given surface
/// Models particle currents and energy transport in quantum fields
/// </summary>
public class QuantumFieldQuantumFlux
{
    private struct FluxPoint
    {
        public double X, Y;
        public double EnergyDensity;
        public double ParticleCount;
        public double VelocityX, VelocityY;
        public double FluxMagnitude;

        public FluxPoint(double x, double y)
        {
            X = x;
            Y = y;
            EnergyDensity = 0.0;
            ParticleCount = 0.0;
            VelocityX = 0.0;
            VelocityY = 0.0;
            FluxMagnitude = 0.0;
        }
    }

    private struct Surface
    {
        public int SurfaceId;
        public FluxPoint[] Points;
        public double TotalEnergyFlux;
        public double TotalParticleFlux;
        public int GridSize;

        public Surface(int id, int size)
        {
            SurfaceId = id;
            GridSize = size;
            Points = new FluxPoint[size * size];
            TotalEnergyFlux = 0.0;
            TotalParticleFlux = 0.0;

            for (int i = 0; i < size * size; i++)
            {
                Points[i] = new FluxPoint();
            }
        }
    }

    private List<Surface> surfaces;
    private Random random;
    private double planckConstant = 6.626e-34;

    public QuantumFieldQuantumFlux()
    {
        this.surfaces = new List<Surface>();
        this.random = new Random();
    }

    public void CreateSurface(int gridSize)
    {
        var surface = new Surface(surfaces.Count, gridSize);

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int idx = i * gridSize + j;
                surface.Points[idx] = new FluxPoint(i, j);
                InitializeFluxPoint(ref surface.Points[idx], i, j, gridSize);
            }
        }

        surfaces.Add(surface);
        Console.WriteLine($"Created surface {surface.SurfaceId} ({gridSize}x{gridSize})");
    }

    private void InitializeFluxPoint(ref FluxPoint point, int i, int j, int gridSize)
    {
        double x = (double)i / gridSize * Math.PI * 2;
        double y = (double)j / gridSize * Math.PI * 2;

        point.EnergyDensity = Math.Abs(Math.Sin(x) * Math.Cos(y)) * 1e-10;
        point.ParticleCount = Math.Abs(Math.Cos(x) * Math.Sin(y)) * 1e5;

        point.VelocityX = Math.Sin(x + point.EnergyDensity);
        point.VelocityY = Math.Cos(y + point.EnergyDensity);

        double speed = Math.Sqrt(point.VelocityX * point.VelocityX +
                                point.VelocityY * point.VelocityY);

        point.FluxMagnitude = point.EnergyDensity * speed;
    }

    public void CalculateFluxCurrents(int surfaceId)
    {
        if (surfaceId >= surfaces.Count)
            return;

        var surface = surfaces[surfaceId];
        double totalEnergyFlux = 0;
        double totalParticleFlux = 0;

        for (int i = 1; i < surface.GridSize - 1; i++)
        {
            for (int j = 1; j < surface.GridSize - 1; j++)
            {
                int idx = i * surface.GridSize + j;

                double dE_dx = (surface.Points[i + 1].EnergyDensity -
                               surface.Points[i - 1].EnergyDensity) / 2.0;
                double dN_dy = (surface.Points[i + j + 1].ParticleCount -
                               surface.Points[i + j - 1].ParticleCount) / 2.0;

                double energyFlow = surface.Points[idx].VelocityX * dE_dx;
                double particleFlow = surface.Points[idx].VelocityY * dN_dy;

                totalEnergyFlux += Math.Abs(energyFlow);
                totalParticleFlux += Math.Abs(particleFlow);
            }
        }

        surface.TotalEnergyFlux = totalEnergyFlux;
        surface.TotalParticleFlux = totalParticleFlux;
        surfaces[surfaceId] = surface;
    }

    public void ApplyFieldDynamics()
    {
        foreach (var surface in surfaces)
        {
            for (int i = 1; i < surface.GridSize - 1; i++)
            {
                for (int j = 1; j < surface.GridSize - 1; j++)
                {
                    int idx = i * surface.GridSize + j;
                    int idxUp = (i - 1) * surface.GridSize + j;
                    int idxDown = (i + 1) * surface.GridSize + j;
                    int idxLeft = i * surface.GridSize + (j - 1);
                    int idxRight = i * surface.GridSize + (j + 1);

                    double laplacian = (surface.Points[idxUp].EnergyDensity +
                                      surface.Points[idxDown].EnergyDensity +
                                      surface.Points[idxLeft].EnergyDensity +
                                      surface.Points[idxRight].EnergyDensity -
                                      4 * surface.Points[idx].EnergyDensity);

                    surface.Points[idx].EnergyDensity += 0.01 * laplacian;
                    surface.Points[idx].VelocityX += 0.005 * laplacian * Math.Sin(surface.Points[idx].Y);
                    surface.Points[idx].VelocityY += 0.005 * laplacian * Math.Cos(surface.Points[idx].X);

                    surface.Points[idx].FluxMagnitude =
                        surface.Points[idx].EnergyDensity *
                        Math.Sqrt(surface.Points[idx].VelocityX * surface.Points[idx].VelocityX +
                                 surface.Points[idx].VelocityY * surface.Points[idx].VelocityY);
                }
            }

            int surfaceIdx = surfaces.IndexOf(surface);
            surfaces[surfaceIdx] = surface;
        }
    }

    public Dictionary<string, object> GetFluxMetrics()
    {
        double totalEnergy = surfaces.Sum(s => s.TotalEnergyFlux);
        double totalParticles = surfaces.Sum(s => s.TotalParticleFlux);
        double avgEnergy = surfaces.Average(s => s.TotalEnergyFlux);

        return new Dictionary<string, object>
        {
            { "SurfaceCount", surfaces.Count },
            { "TotalEnergyFlux", totalEnergy },
            { "TotalParticleFlux", totalParticles },
            { "AverageEnergyFlux", avgEnergy },
            { "PlanckConstant", planckConstant }
        };
    }

    public void PrintFluxField(int surfaceId, int limit = 10)
    {
        if (surfaceId >= surfaces.Count)
            return;

        var surface = surfaces[surfaceId];
        Console.WriteLine($"Quantum Field Flux (Surface {surfaceId}):");

        for (int i = 0; i < Math.Min(limit, surface.GridSize); i++)
        {
            Console.Write("  ");
            for (int j = 0; j < Math.Min(limit, surface.GridSize); j++)
            {
                int idx = i * surface.GridSize + j;
                double flux = surface.Points[idx].FluxMagnitude;

                if (flux < 1e-12)
                    Console.Write(". ");
                else if (flux < 1e-11)
                    Console.Write("+ ");
                else if (flux < 1e-10)
                    Console.Write("* ");
                else
                    Console.Write("# ");
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Field Quantum Flux - Energy & Particle Flow ===\n");

        var qfqf = new QuantumFieldQuantumFlux();

        Console.WriteLine("--- Creating Surfaces ---");
        qfqf.CreateSurface(16);
        qfqf.CreateSurface(20);

        Console.WriteLine("\n--- Calculating Initial Flux Currents ---");
        qfqf.CalculateFluxCurrents(0);
        qfqf.CalculateFluxCurrents(1);

        qfqf.PrintFluxField(0, 12);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = qfqf.GetFluxMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Field Dynamics ---");
        for (int step = 0; step < 20; step++)
        {
            qfqf.ApplyFieldDynamics();
            if (step % 5 == 0)
            {
                qfqf.CalculateFluxCurrents(0);
                Console.WriteLine($"Step {step}: Energy Flux = {qfqf.surfaces[0].TotalEnergyFlux:E4}");
            }
        }

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = qfqf.GetFluxMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Field Quantum Flux models energy and particle transport.");
    }
}
