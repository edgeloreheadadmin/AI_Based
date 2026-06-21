using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Scalar Field Behavior
/// Used to describe fundamental particles and bosons (Higgs, pions)
/// Models spontaneous symmetry breaking and mass generation
/// </summary>
public class ScalarFieldBehavior
{
    private struct ScalarBoson
    {
        public int BosonId;
        public string ParticleType;
        public double[] FieldAmplitudes;
        public double VacuumExpectationValue;
        public double Mass;
        public double SelfCoupling;
        public double PotentialEnergy;

        public ScalarBoson(int id, string type)
        {
            BosonId = id;
            ParticleType = type;
            FieldAmplitudes = new double[10];
            VacuumExpectationValue = 0.0;
            Mass = 0.0;
            SelfCoupling = 0.0;
            PotentialEnergy = 0.0;

            for (int i = 0; i < 10; i++)
            {
                FieldAmplitudes[i] = Math.Sin(i * Math.PI / 10) / Math.Sqrt(10);
            }
        }
    }

    private List<ScalarBoson> bosons;
    private Random random;
    private const double HIGGS_VEV = 246e9 * 1.602e-19;
    private const double c = 3e8;

    public ScalarFieldBehavior()
    {
        this.bosons = new List<ScalarBoson>();
        this.random = new Random();
    }

    public void CreateScalarBoson(string particleType)
    {
        var boson = new ScalarBoson(bosons.Count, particleType);
        boson.Mass = GetParticleMass(particleType);
        boson.SelfCoupling = random.NextDouble() * 0.5;
        bosons.Add(boson);
        Console.WriteLine($"Created {particleType} boson {boson.BosonId}");
    }

    private double GetParticleMass(string particleType)
    {
        return particleType switch
        {
            "Higgs" => 125e9 * 1.602e-19,
            "pion" => 140e6 * 1.602e-19,
            "kaon" => 494e6 * 1.602e-19,
            _ => 1e-20
        };
    }

    public void CalculatePotential()
    {
        foreach (var boson in bosons)
        {
            double potential = 0;

            for (int i = 0; i < boson.FieldAmplitudes.Length; i++)
            {
                double phi = boson.FieldAmplitudes[i];
                potential += Math.Pow(phi, 2) +
                           boson.SelfCoupling * Math.Pow(phi, 4);
            }

            boson.PotentialEnergy = potential;
            int idx = bosons.IndexOf(boson);
            bosons[idx] = boson;
        }
    }

    public void SimulateSpontaneousSymmetryBreaking()
    {
        Console.WriteLine("\nSimulating spontaneous symmetry breaking...\n");

        foreach (var boson in bosons)
        {
            if (boson.ParticleType == "Higgs")
            {
                double temperature = random.NextDouble() * 1000;
                Console.WriteLine($"Higgs field at T = {temperature:F1}K");

                if (temperature < 160)
                {
                    boson.VacuumExpectationValue = HIGGS_VEV;
                    for (int i = 0; i < boson.FieldAmplitudes.Length; i++)
                    {
                        boson.FieldAmplitudes[i] = HIGGS_VEV / Math.Sqrt(boson.FieldAmplitudes.Length);
                    }
                    Console.WriteLine("  → Symmetry broken, VEV ≠ 0");
                }
                else
                {
                    Console.WriteLine("  → Symmetric phase, VEV = 0");
                }
            }

            int idx = bosons.IndexOf(boson);
            bosons[idx] = boson;
        }
    }

    public void GenerateMasses()
    {
        Console.WriteLine("\nGenerating particle masses via Higgs coupling...\n");

        var higgsBoson = bosons.FirstOrDefault(b => b.ParticleType == "Higgs");

        if (higgsBoson.BosonId >= 0)
        {
            foreach (var boson in bosons)
            {
                if (boson.ParticleType != "Higgs")
                {
                    double yukawaCoupling = boson.SelfCoupling;
                    double generatedMass = yukawaCoupling * higgsBoson.VacuumExpectationValue /
                                         Math.Sqrt(2) / c;

                    boson.Mass += generatedMass;

                    int idx = bosons.IndexOf(boson);
                    bosons[idx] = boson;

                    Console.WriteLine($"{boson.ParticleType}: Generated mass = {generatedMass:E4}");
                }
            }
        }
    }

    public void EvolveScalarConfiguration(int steps)
    {
        Console.WriteLine($"\nEvolving scalar field for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            foreach (var boson in bosons)
            {
                for (int i = 0; i < boson.FieldAmplitudes.Length; i++)
                {
                    boson.FieldAmplitudes[i] += boson.SelfCoupling *
                                               Math.Sin(boson.FieldAmplitudes[i]) * 0.01;
                }

                int idx = bosons.IndexOf(boson);
                bosons[idx] = boson;
            }

            CalculatePotential();

            if (step % (steps / 3) == 0)
            {
                double totalPotential = bosons.Sum(b => b.PotentialEnergy);
                Console.WriteLine($"Step {step}: Total Potential = {totalPotential:F6}");
            }
        }
    }

    public Dictionary<string, object> GetScalarMetrics()
    {
        double totalPotential = bosons.Sum(b => b.PotentialEnergy);
        double avgVEV = bosons.Average(b => b.VacuumExpectationValue);
        double avgMass = bosons.Average(b => b.Mass);

        return new Dictionary<string, object>
        {
            { "BosonCount", bosons.Count },
            { "TotalPotential", totalPotential },
            { "AverageVEV", avgVEV },
            { "AverageMass", avgMass },
            { "HiggsVEV", HIGGS_VEV }
        };
    }

    public void PrintBosonState(int bosonId)
    {
        if (bosonId >= bosons.Count)
            return;

        var boson = bosons[bosonId];
        Console.WriteLine($"\n{boson.ParticleType} Boson State:");
        Console.WriteLine($"  Mass: {boson.Mass:E4}");
        Console.WriteLine($"  VEV: {boson.VacuumExpectationValue:E4}");
        Console.WriteLine($"  Potential: {boson.PotentialEnergy:F6}");
        Console.WriteLine($"  Self-coupling λ: {boson.SelfCoupling:F4}");
    }

    public static void Main()
    {
        Console.WriteLine("=== Scalar Field Behavior - Fundamental Bosons ===\n");

        var sfb = new ScalarFieldBehavior();

        Console.WriteLine("--- Creating Scalar Bosons ---");
        sfb.CreateScalarBoson("Higgs");
        sfb.CreateScalarBoson("pion");
        sfb.CreateScalarBoson("kaon");

        sfb.PrintBosonState(0);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = sfb.GetScalarMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Simulating Symmetry Breaking ---");
        sfb.SimulateSpontaneousSymmetryBreaking();

        Console.WriteLine("\n--- Generating Masses ---");
        sfb.GenerateMasses();

        sfb.PrintBosonState(1);

        Console.WriteLine("\n--- Evolving Scalar Configuration ---");
        sfb.EvolveScalarConfiguration(steps: 30);

        sfb.PrintBosonState(0);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = sfb.GetScalarMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nScalar Field Behavior models Higgs mechanism and mass generation.");
    }
}
