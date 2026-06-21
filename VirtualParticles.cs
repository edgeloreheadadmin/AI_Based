using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Virtual Particles
/// Pairs of particles created and annihilated out of vacuum for short periods
/// Cannot be directly observed but affect behavior of other particles
/// Simulates vacuum fluctuations and Casimir effect
/// </summary>
public class VirtualParticles
{
    private struct VirtualPair
    {
        public int PairId;
        public string ParticleType;
        public double CreationEnergy;
        public double LifeTime;
        public double ElapsedTime;
        public bool Active;
        public double InfluenceField;
        public double PositionX, PositionY;

        public VirtualPair(int id, string type, double energy)
        {
            PairId = id;
            ParticleType = type;
            CreationEnergy = energy;
            LifeTime = CalculateLifeTime(energy);
            ElapsedTime = 0.0;
            Active = true;
            InfluenceField = CalculateInfluence(energy);
            PositionX = 0.0;
            PositionY = 0.0;
        }

        private static double CalculateLifeTime(double energy)
        {
            const double hBar = 1.054e-34;
            return hBar / energy;
        }

        private static double CalculateInfluence(double energy)
        {
            return Math.Exp(-energy / 1e6);
        }
    }

    private List<VirtualPair> virtualPairs;
    private double[,] vacuumField;
    private int fieldSize;
    private Random random;
    private double cumulativeEnergy;

    public VirtualParticles(int fieldResolution = 32)
    {
        this.virtualPairs = new List<VirtualPair>();
        this.fieldSize = fieldResolution;
        this.vacuumField = new double[fieldSize, fieldSize];
        this.random = new Random();
        this.cumulativeEnergy = 0.0;

        InitializeVacuumField();
    }

    private void InitializeVacuumField()
    {
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                vacuumField[i, j] = 0.0;
            }
        }
    }

    public void CreateVirtualPairFluctuation()
    {
        double fluctuationEnergy = random.NextDouble() * 1e5;

        string[] particleTypes = { "electron-positron", "photon-photon", "quark-gluon" };
        string type = particleTypes[random.Next(particleTypes.Length)];

        var pair = new VirtualPair(virtualPairs.Count, type, fluctuationEnergy);

        pair.PositionX = random.NextDouble() * fieldSize;
        pair.PositionY = random.NextDouble() * fieldSize;

        virtualPairs.Add(pair);
        cumulativeEnergy += fluctuationEnergy;
    }

    public void CreateMultiplePairs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            CreateVirtualPairFluctuation();
        }

        Console.WriteLine($"Created {count} virtual particle pairs");
    }

    public void EvolutionStep(double timeStep)
    {
        var pairsToRemove = new List<int>();

        for (int i = 0; i < virtualPairs.Count; i++)
        {
            var pair = virtualPairs[i];

            if (!pair.Active)
                continue;

            pair.ElapsedTime += timeStep;

            double decayProbability = (pair.ElapsedTime / pair.LifeTime);
            decayProbability = Math.Min(1.0, decayProbability);

            if (random.NextDouble() < decayProbability)
            {
                pairsToRemove.Add(i);
                pair.Active = false;
            }

            int gridX = (int)(pair.PositionX);
            int gridY = (int)(pair.PositionY);

            if (gridX >= 0 && gridX < fieldSize && gridY >= 0 && gridY < fieldSize)
            {
                vacuumField[gridX, gridY] += pair.InfluenceField;
            }

            virtualPairs[i] = pair;
        }

        for (int i = pairsToRemove.Count - 1; i >= 0; i--)
        {
            virtualPairs.RemoveAt(pairsToRemove[i]);
        }
    }

    public void SimulateCasimirEffect()
    {
        Console.WriteLine("\nSimulating Casimir Effect (Virtual Particles between plates)...\n");

        InitializeVacuumField();
        virtualPairs.Clear();
        cumulativeEnergy = 0.0;

        int platePosition1 = 5;
        int platePosition2 = fieldSize - 5;

        CreateMultiplePairs(50);

        for (int step = 0; step < 100; step++)
        {
            EvolutionStep(0.01);

            if (step % 20 == 0)
            {
                double fieldStrengthInside = CalculateAverageFieldStrength(platePosition1, platePosition2);
                double fieldStrengthOutside = CalculateAverageFieldStrength(0, platePosition1);

                Console.WriteLine($"Step {step}: Inside={fieldStrengthInside:F6}, " +
                                $"Outside={fieldStrengthOutside:F6}, " +
                                $"Active Pairs={virtualPairs.Count}");
            }
        }
    }

    private double CalculateAverageFieldStrength(int start, int end)
    {
        double sum = 0;
        int count = 0;

        for (int i = start; i < Math.Min(end, fieldSize); i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                sum += Math.Abs(vacuumField[i, j]);
                count++;
            }
        }

        return count > 0 ? sum / count : 0;
    }

    public Dictionary<string, int> GetParticleTypeDistribution()
    {
        Dictionary<string, int> distribution = new Dictionary<string, int>();

        foreach (var pair in virtualPairs)
        {
            if (!distribution.ContainsKey(pair.ParticleType))
                distribution[pair.ParticleType] = 0;
            distribution[pair.ParticleType]++;
        }

        return distribution;
    }

    public Dictionary<string, object> GetVacuumFluctuationMetrics()
    {
        int activePairs = virtualPairs.Count(p => p.Active);
        int annihilatedPairs = virtualPairs.Count - activePairs;
        double maxFieldStrength = GetMaxFieldStrength();
        double avgFieldStrength = GetAverageFieldStrength();

        return new Dictionary<string, object>
        {
            { "TotalPairsCreated", virtualPairs.Count },
            { "ActivePairs", activePairs },
            { "AnnihilatedPairs", annihilatedPairs },
            { "CumulativeEnergy", cumulativeEnergy },
            { "MaxFieldStrength", maxFieldStrength },
            { "AvgFieldStrength", avgFieldStrength }
        };
    }

    private double GetMaxFieldStrength()
    {
        double max = 0;
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                max = Math.Max(max, Math.Abs(vacuumField[i, j]));
            }
        }
        return max;
    }

    private double GetAverageFieldStrength()
    {
        double sum = 0;
        for (int i = 0; i < fieldSize; i++)
        {
            for (int j = 0; j < fieldSize; j++)
            {
                sum += Math.Abs(vacuumField[i, j]);
            }
        }
        return sum / (fieldSize * fieldSize);
    }

    public void PrintVacuumFieldSnapshot()
    {
        Console.WriteLine("Vacuum Field Snapshot (sample):");
        for (int i = 0; i < Math.Min(8, fieldSize); i++)
        {
            Console.Write("  ");
            for (int j = 0; j < Math.Min(8, fieldSize); j++)
            {
                double val = vacuumField[i, j];
                if (val < 0.001)
                    Console.Write(". ");
                else if (val < 0.01)
                    Console.Write("+ ");
                else if (val < 0.1)
                    Console.Write("* ");
                else
                    Console.Write("# ");
            }
            Console.WriteLine();
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Virtual Particles - Vacuum Fluctuations ===\n");

        var vacuum = new VirtualParticles(fieldResolution: 32);

        Console.WriteLine("--- Creating Virtual Particle Pairs ---");
        vacuum.CreateMultiplePairs(100);

        var distribution = vacuum.GetParticleTypeDistribution();
        Console.WriteLine("\nParticle Type Distribution:");
        foreach (var kvp in distribution)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value} pairs");
        }

        Console.WriteLine("\n--- Evolution of Virtual Particles ---");
        for (int step = 0; step < 50; step++)
        {
            vacuum.EvolutionStep(0.01);

            if (step % 10 == 0)
            {
                Console.WriteLine($"Step {step}: {vacuum.virtualPairs.Count} pairs still active");
            }
        }

        Console.WriteLine("\n--- Metrics After Evolution ---");
        var metrics = vacuum.GetVacuumFluctuationMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        vacuum.PrintVacuumFieldSnapshot();

        Console.WriteLine("\n--- Simulating Casimir Effect ---");
        var vacuum2 = new VirtualParticles(fieldResolution: 16);
        vacuum2.SimulateCasimirEffect();

        Console.WriteLine("\nVirtual particles successfully simulated vacuum fluctuations.");
    }
}
