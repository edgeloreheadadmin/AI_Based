using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Spontaneous Emission
/// Concept where an excited atom emits a photon of light
/// Models energy level transitions and photon emission
/// </summary>
public class SpontaneousEmission
{
    private struct ExcitedAtom
    {
        public int AtomId;
        public int EnergyLevel;
        public double ExcitationEnergy;
        public double LifeTime;
        public double ElapsedTime;
        public bool Excited;
        public List<double> EmittedPhotons;

        public ExcitedAtom(int id)
        {
            AtomId = id;
            EnergyLevel = 0;
            ExcitationEnergy = 0.0;
            LifeTime = 0.0;
            ElapsedTime = 0.0;
            Excited = false;
            EmittedPhotons = new List<double>();
        }
    }

    private List<ExcitedAtom> atoms;
    private Random random;
    private double planckConstant = 6.626e-34;
    private double totalPhotonEnergy;
    private int totalPhotonsEmitted;

    public SpontaneousEmission(int atomCount = 100)
    {
        this.atoms = new List<ExcitedAtom>();
        this.random = new Random();
        this.totalPhotonEnergy = 0.0;
        this.totalPhotonsEmitted = 0;

        InitializeAtoms(atomCount);
    }

    private void InitializeAtoms(int count)
    {
        for (int i = 0; i < count; i++)
        {
            atoms.Add(new ExcitedAtom(i));
        }
        Console.WriteLine($"Initialized {count} atoms");
    }

    public void ExciteAtom(int atomId, int targetLevel)
    {
        if (atomId >= atoms.Count || targetLevel < 1)
            return;

        var atom = atoms[atomId];

        atom.EnergyLevel = targetLevel;
        atom.ExcitationEnergy = targetLevel * 2.5;
        atom.LifeTime = CalculateLifeTime(targetLevel);
        atom.ElapsedTime = 0.0;
        atom.Excited = true;

        atoms[atomId] = atom;
    }

    private double CalculateLifeTime(int energyLevel)
    {
        return Math.Pow(energyLevel, -3) * 1e-8;
    }

    public void ExciteRandomAtoms(int count, int maxLevel = 3)
    {
        for (int i = 0; i < count; i++)
        {
            int atomId = random.Next(atoms.Count);
            int level = random.Next(1, maxLevel + 1);
            ExciteAtom(atomId, level);
        }

        Console.WriteLine($"Excited {count} random atoms");
    }

    public void SimulateEmissionProcess(double timeStep)
    {
        var excitedAtomsList = atoms.Where((a, idx) => a.Excited)
            .Select((a, idx) => (idx, a))
            .ToList();

        List<int> atomsToUpdate = new List<int>();

        foreach (var (idx, atom) in excitedAtomsList)
        {
            if (!atom.Excited)
                continue;

            atom.ElapsedTime += timeStep;

            double decayProbability = 1.0 - Math.Exp(-atom.ElapsedTime / atom.LifeTime);

            if (random.NextDouble() < decayProbability)
            {
                EmitPhoton(idx);
                atomsToUpdate.Add(idx);
            }
        }

        foreach (int idx in atomsToUpdate)
        {
            var atom = atoms[idx];
            atom.Excited = false;
            atom.EnergyLevel = 0;
            atom.ExcitationEnergy = 0.0;
            atoms[idx] = atom;
        }
    }

    private void EmitPhoton(int atomId)
    {
        var atom = atoms[atomId];

        double photonEnergy = atom.ExcitationEnergy * random.NextDouble();
        double frequency = photonEnergy / planckConstant;
        double wavelength = 3e8 / frequency;

        atom.EmittedPhotons.Add(photonEnergy);
        totalPhotonEnergy += photonEnergy;
        totalPhotonsEmitted++;

        atoms[atomId] = atom;
    }

    public void RunEmissionSimulation(int timeSteps, double timeStepSize)
    {
        Console.WriteLine($"\nRunning spontaneous emission simulation for {timeSteps} time steps...\n");

        for (int step = 0; step < timeSteps; step++)
        {
            if (step % 10 == 0 && step > 0)
            {
                int excitedCount = atoms.Count(a => a.Excited);
                Console.WriteLine($"Step {step}: {excitedCount} atoms still excited, " +
                                $"{totalPhotonsEmitted} photons emitted");
            }

            SimulateEmissionProcess(timeStepSize);
        }
    }

    public Dictionary<string, object> GetEmissionStatistics()
    {
        int excitedCount = atoms.Count(a => a.Excited);
        int atomsWithPhotons = atoms.Count(a => a.EmittedPhotons.Count > 0);
        double avgPhotonsPerAtom = atomsWithPhotons > 0 ?
            (double)totalPhotonsEmitted / atomsWithPhotons : 0;

        return new Dictionary<string, object>
        {
            { "TotalAtoms", atoms.Count },
            { "ExcitedAtoms", excitedCount },
            { "AtomsEmittedPhotons", atomsWithPhotons },
            { "TotalPhotonsEmitted", totalPhotonsEmitted },
            { "TotalPhotonEnergy", totalPhotonEnergy },
            { "AveragePhotonsPerAtom", avgPhotonsPerAtom },
            { "AveragePhotonEnergy", totalPhotonsEmitted > 0 ? totalPhotonEnergy / totalPhotonsEmitted : 0 }
        };
    }

    public void PrintAtomDetails(int limit = 5)
    {
        Console.WriteLine("Atom Emission Details:");
        int count = 0;

        foreach (var atom in atoms)
        {
            if (atom.EmittedPhotons.Count > 0 && count < limit)
            {
                Console.WriteLine($"  Atom {atom.AtomId}:");
                Console.WriteLine($"    Photons Emitted: {atom.EmittedPhotons.Count}");
                Console.WriteLine($"    Total Energy: {atom.EmittedPhotons.Sum():E4}");
                Console.WriteLine($"    Avg Photon Energy: {atom.EmittedPhotons.Average():E4}");
                count++;
            }
        }
    }

    public Dictionary<int, int> GetEnergyLevelDistribution()
    {
        Dictionary<int, int> distribution = new Dictionary<int, int>();

        foreach (var atom in atoms)
        {
            int photonCount = atom.EmittedPhotons.Count;
            if (!distribution.ContainsKey(photonCount))
                distribution[photonCount] = 0;
            distribution[photonCount]++;
        }

        return distribution.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public static void Main()
    {
        Console.WriteLine("=== Spontaneous Emission - Photon Emission Process ===\n");

        var emission = new SpontaneousEmission(atomCount: 100);

        Console.WriteLine("--- Exciting Atoms ---");
        emission.ExciteRandomAtoms(count: 50, maxLevel: 5);

        Console.WriteLine("\n--- Initial Statistics ---");
        var statsInitial = emission.GetEmissionStatistics();
        foreach (var kvp in statsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Simulating Emission ---");
        emission.RunEmissionSimulation(timeSteps: 100, timeStepSize: 0.001);

        Console.WriteLine("\n--- Final Statistics ---");
        var statsFinal = emission.GetEmissionStatistics();
        foreach (var kvp in statsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Top Photon Emitters ---");
        emission.PrintAtomDetails(5);

        Console.WriteLine("\n--- Photon Count Distribution ---");
        var distribution = emission.GetEnergyLevelDistribution();
        foreach (var kvp in distribution)
        {
            string bar = new string('█', kvp.Value / 2);
            Console.WriteLine($"  {kvp.Key} photons: {bar} ({kvp.Value} atoms)");
        }

        Console.WriteLine("\nSpontaneous emission successfully demonstrates photon emission from excited atoms.");
    }
}
