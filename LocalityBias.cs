using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Locality Bias
/// Bias towards creating local fields where interactions occur only at nearby points
/// Fields only interact with neighboring lattice sites, no action at a distance
/// </summary>
public class LocalityBias
{
    private double[,,] fieldLattice;
    private int latticeSize;
    private Random random;

    public LocalityBias(int latticeSize = 10)
    {
        this.latticeSize = latticeSize;
        this.fieldLattice = new double[latticeSize, latticeSize, latticeSize];
        this.random = new Random();

        InitializeLattice();
    }

    private void InitializeLattice()
    {
        for (int x = 0; x < latticeSize; x++)
        {
            for (int y = 0; y < latticeSize; y++)
            {
                for (int z = 0; z < latticeSize; z++)
                {
                    fieldLattice[x, y, z] = random.NextDouble();
                }
            }
        }
    }

    public double GetFieldValue(int x, int y, int z)
    {
        if (x < 0 || x >= latticeSize || y < 0 || y >= latticeSize || z < 0 || z >= latticeSize)
            return 0.0;

        return fieldLattice[x, y, z];
    }

    public void SetFieldValue(int x, int y, int z, double value)
    {
        if (x >= 0 && x < latticeSize && y >= 0 && y < latticeSize && z >= 0 && z < latticeSize)
        {
            fieldLattice[x, y, z] = value;
        }
    }

    public double CalculateLocalNeighborhood(int x, int y, int z)
    {
        double neighborSum = 0.0;
        int neighborCount = 0;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    if (dx == 0 && dy == 0 && dz == 0)
                        continue;

                    double neighborValue = GetFieldValue(x + dx, y + dy, z + dz);
                    neighborSum += neighborValue;
                    neighborCount++;
                }
            }
        }

        return neighborCount > 0 ? neighborSum / neighborCount : 0.0;
    }

    public double MeasureLocality()
    {
        double totalLocalityScore = 0.0;

        for (int x = 0; x < latticeSize; x++)
        {
            for (int y = 0; y < latticeSize; y++)
            {
                for (int z = 0; z < latticeSize; z++)
                {
                    double fieldValue = GetFieldValue(x, y, z);
                    double neighborAvg = CalculateLocalNeighborhood(x, y, z);

                    double localityScore = 1.0 / (1.0 + Math.Abs(fieldValue - neighborAvg));
                    totalLocalityScore += localityScore;
                }
            }
        }

        return totalLocalityScore / (latticeSize * latticeSize * latticeSize);
    }

    public double CalculateNonLocalInteraction(int x1, int y1, int z1, int x2, int y2, int z2)
    {
        double distanceSquared = Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2) + Math.Pow(z1 - z2, 2);
        double distance = Math.Sqrt(distanceSquared);

        double field1 = GetFieldValue(x1, y1, z1);
        double field2 = GetFieldValue(x2, y2, z2);

        if (distance > 0)
        {
            return (field1 * field2) / distance;
        }
        return 0.0;
    }

    public void EnforceLocality()
    {
        Console.WriteLine("\nEnforcing locality on field lattice...\n");

        for (int x = 0; x < latticeSize; x++)
        {
            for (int y = 0; y < latticeSize; y++)
            {
                for (int z = 0; z < latticeSize; z++)
                {
                    double neighborAvg = CalculateLocalNeighborhood(x, y, z);
                    double currentValue = GetFieldValue(x, y, z);

                    double localizedValue = 0.7 * currentValue + 0.3 * neighborAvg;
                    SetFieldValue(x, y, z, localizedValue);
                }
            }
        }

        Console.WriteLine("Locality enforcement complete.");
    }

    public void EvolveWithLocalityBias(int steps, double couplingStrength)
    {
        Console.WriteLine($"\nEvolving field lattice with locality bias for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            double[,,] newLattice = new double[latticeSize, latticeSize, latticeSize];

            for (int x = 0; x < latticeSize; x++)
            {
                for (int y = 0; y < latticeSize; y++)
                {
                    for (int z = 0; z < latticeSize; z++)
                    {
                        double currentValue = GetFieldValue(x, y, z);
                        double neighborAvg = CalculateLocalNeighborhood(x, y, z);

                        newLattice[x, y, z] = currentValue + couplingStrength * (neighborAvg - currentValue);
                    }
                }
            }

            fieldLattice = newLattice;

            if (step % (steps / 3) == 0)
            {
                double localityScore = MeasureLocality();
                Console.WriteLine($"Step {step}: Locality Score = {localityScore:F6}");
            }
        }
    }

    public Dictionary<string, object> GetLocalityMetrics()
    {
        double localityScore = MeasureLocality();

        double maxNonLocality = 0.0;
        for (int x = 0; x < latticeSize; x++)
        {
            for (int y = 0; y < latticeSize; y++)
            {
                for (int z = 0; z < latticeSize; z++)
                {
                    for (int x2 = x + 2; x2 < Math.Min(x + 5, latticeSize); x2++)
                    {
                        double nonLocal = CalculateNonLocalInteraction(x, y, z, x2, y, z);
                        maxNonLocality = Math.Max(maxNonLocality, Math.Abs(nonLocal));
                    }
                }
            }
        }

        return new Dictionary<string, object>
        {
            { "LatticeSize", latticeSize },
            { "LocalityScore", localityScore },
            { "MaxNonLocalInteraction", maxNonLocality },
            { "TotalSites", latticeSize * latticeSize * latticeSize }
        };
    }

    public void PrintLatticeSlice(int z)
    {
        Console.WriteLine($"\nField Lattice Slice (z={z}) - Locality:");
        for (int y = 0; y < latticeSize; y++)
        {
            Console.Write("  ");
            for (int x = 0; x < latticeSize; x++)
            {
                double value = GetFieldValue(x, y, z);
                double neighborAvg = CalculateLocalNeighborhood(x, y, z);
                double difference = Math.Abs(value - neighborAvg);

                char symbol = difference < 0.1 ? '█' : difference < 0.3 ? '▓' : difference < 0.6 ? '░' : ' ';
                Console.Write(symbol);
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Locality Bias - Local Field Interactions ===\n");

        var bias = new LocalityBias(latticeSize: 10);

        Console.WriteLine("--- Initial Field Lattice ---");
        bias.PrintLatticeSlice(5);

        Console.WriteLine("\n--- Initial Locality Metrics ---");
        var metricsInitial = bias.GetLocalityMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Enforcing Locality ---");
        bias.EnforceLocality();

        Console.WriteLine("\n--- Evolving with Locality Bias ---");
        bias.EvolveWithLocalityBias(steps: 20, couplingStrength: 0.5);

        bias.PrintLatticeSlice(5);

        Console.WriteLine("\n--- Final Locality Metrics ---");
        var metricsFinal = bias.GetLocalityMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nLocality Bias ensures field interactions occur only at nearby lattice points.");
    }
}
