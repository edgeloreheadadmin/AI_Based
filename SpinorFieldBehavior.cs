using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Spinor Field Behavior
/// Used to describe fermions and matter particles
/// Models particle creation, annihilation, and statistics
/// </summary>
public class SpinorFieldBehavior
{
    private struct FermionField
    {
        public int ParticleId;
        public double[] WaveFunction;
        public string ParticleType;
        public double Energy;
        public double Spin;
        public bool IsCreated;
        public double OccupationNumber;

        public FermionField(int id, string type)
        {
            ParticleId = id;
            ParticleType = type;
            WaveFunction = new double[4];
            Energy = 0.0;
            Spin = 0.5;
            IsCreated = false;
            OccupationNumber = 0.0;

            for (int i = 0; i < 4; i++)
            {
                WaveFunction[i] = 1.0 / 2.0;
            }
        }
    }

    private List<FermionField> fermions;
    private int maxParticles;
    private Random random;
    private const double HBAR = 1.054e-34;
    private const double c = 3e8;

    public SpinorFieldBehavior(int maxFermions = 20)
    {
        this.fermions = new List<FermionField>();
        this.maxParticles = maxFermions;
        this.random = new Random();
    }

    public void CreateFermion(string particleType)
    {
        if (fermions.Count >= maxParticles)
            return;

        var fermion = new FermionField(fermions.Count, particleType);
        fermion.Energy = CalculateFermionEnergy(fermion);
        fermions.Add(fermion);
    }

    private double CalculateFermionEnergy(FermionField fermion)
    {
        double momentum = random.NextDouble() * 1e-20;
        return Math.Sqrt(Math.Pow(momentum * c, 2) + Math.Pow(0.9e8 * c * c, 2));
    }

    public void AnnihilateParticles()
    {
        var particlePairs = new List<(int, int)>();

        for (int i = 0; i < fermions.Count; i++)
        {
            for (int j = i + 1; j < fermions.Count; j++)
            {
                if (fermions[i].ParticleType != fermions[j].ParticleType)
                {
                    double energyDifference = Math.Abs(fermions[i].Energy - fermions[j].Energy);

                    if (energyDifference < 1e10 && fermions[i].IsCreated && fermions[j].IsCreated)
                    {
                        if (random.NextDouble() < 0.3)
                        {
                            particlePairs.Add((i, j));
                        }
                    }
                }
            }
        }

        for (int idx = particlePairs.Count - 1; idx >= 0; idx--)
        {
            var (i, j) = particlePairs[idx];
            fermions.RemoveAt(Math.Max(i, j));
            fermions.RemoveAt(Math.Min(i, j));
        }
    }

    public void ApplyPauliExclusionPrinciple()
    {
        var energyLevels = new Dictionary<int, int>();

        foreach (var fermion in fermions)
        {
            int energyLevel = (int)(fermion.Energy / 1e10);
            if (!energyLevels.ContainsKey(energyLevel))
                energyLevels[energyLevel] = 0;

            energyLevels[energyLevel]++;
        }

        var violatingFermions = new List<int>();

        foreach (var kvp in energyLevels)
        {
            if (kvp.Value > 2)
            {
                int excess = kvp.Value - 2;
                int energyLevel = kvp.Key;

                for (int i = 0; i < excess; i++)
                {
                    int idx = fermions.FindIndex(f => (int)(f.Energy / 1e10) == energyLevel);
                    if (idx >= 0)
                    {
                        violatingFermions.Add(idx);
                    }
                }
            }
        }

        for (int idx = violatingFermions.Count - 1; idx >= 0; idx--)
        {
            fermions.RemoveAt(violatingFermions[idx]);
        }
    }

    public void EvolveQuantumNumbers(int steps)
    {
        Console.WriteLine($"\nEvolving fermion field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            foreach (var fermion in fermions)
            {
                fermion.IsCreated = true;
                fermion.OccupationNumber = random.NextDouble();

                int idx = fermions.IndexOf(fermion);
                fermions[idx] = fermion;
            }

            ApplyPauliExclusionPrinciple();
            AnnihilateParticles();

            if (step % (steps / 3) == 0)
            {
                Console.WriteLine($"Step {step}: Particles = {fermions.Count}, " +
                                $"Total Energy = {fermions.Sum(f => f.Energy):E4}");
            }
        }
    }

    public Dictionary<string, object> GetFermionMetrics()
    {
        var particleTypeCounts = fermions.GroupBy(f => f.ParticleType)
            .ToDictionary(g => g.Key, g => g.Count());

        double totalEnergy = fermions.Sum(f => f.Energy);
        double avgSpin = fermions.Average(f => f.Spin);

        return new Dictionary<string, object>
        {
            { "TotalParticles", fermions.Count },
            { "MaxParticles", maxParticles },
            { "TotalEnergy", totalEnergy },
            { "AverageSpin", avgSpin },
            { "ParticleTypes", string.Join(", ", particleTypeCounts.Keys) }
        };
    }

    public void PrintFermionStates(int limit = 5)
    {
        Console.WriteLine("Fermion States:");
        for (int i = 0; i < Math.Min(limit, fermions.Count); i++)
        {
            var f = fermions[i];
            Console.WriteLine($"  [{i}] {f.ParticleType}: E={f.Energy:E4}, " +
                            $"Occupation={f.OccupationNumber:F3}, " +
                            $"Created={f.IsCreated}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Spinor Field Behavior - Fermionic Matter ===\n");

        var sfb = new SpinorFieldBehavior(maxFermions: 20);

        Console.WriteLine("--- Creating Fermion Field ---");
        string[] particleTypes = { "electron", "positron", "muon", "tau" };
        for (int i = 0; i < 12; i++)
        {
            sfb.CreateFermion(particleTypes[i % particleTypes.Length]);
        }

        Console.WriteLine($"Created {sfb.fermions.Count} fermions");

        sfb.PrintFermionStates(8);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = sfb.GetFermionMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Fermion Field ---");
        sfb.EvolveQuantumNumbers(steps: 30);

        sfb.PrintFermionStates(8);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = sfb.GetFermionMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSpinor Field Behavior models fermion dynamics and interactions.");
    }
}
