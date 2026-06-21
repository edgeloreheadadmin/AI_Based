using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Deconstructor
/// Disassembles matter at atomic level using quantum entanglement
/// Creates superposition of states where atoms are both present and absent
/// </summary>
public class QuantumDeconstructor
{
    private struct Atom
    {
        public int AtomId;
        public string Element;
        public double MassNumber;
        public double PresenceAmplitude;
        public double AbsenceAmplitude;
        public double Position;
        public bool Deconstructed;
        public double EntanglementStrength;

        public Atom(int id, string element, double mass)
        {
            AtomId = id;
            Element = element;
            MassNumber = mass;
            PresenceAmplitude = 1.0 / Math.Sqrt(2);
            AbsenceAmplitude = 1.0 / Math.Sqrt(2);
            Position = id * 0.1;
            Deconstructed = false;
            EntanglementStrength = 0.0;
        }
    }

    private struct Matter
    {
        public int MatterId;
        public List<Atom> Atoms;
        public double TotalMass;
        public string MaterialType;
        public double DeconstructionProgress;
        public int OriginalAtomCount;

        public Matter(int id, string material)
        {
            MatterId = id;
            Atoms = new List<Atom>();
            TotalMass = 0.0;
            MaterialType = material;
            DeconstructionProgress = 0.0;
            OriginalAtomCount = 0;
        }
    }

    private List<Matter> materials;
    private double planckConstant = 6.626e-34;
    private double reducedPlanck = 1.054e-34;
    private Random random;

    public QuantumDeconstructor()
    {
        this.materials = new List<Matter>();
        this.random = new Random();
    }

    public void CreateMaterial(string materialType, int atomCount)
    {
        var matter = new Matter(materials.Count, materialType);
        matter.OriginalAtomCount = atomCount;

        string[] elements = { "C", "O", "N", "H", "Si", "Fe", "Ca" };

        for (int i = 0; i < atomCount; i++)
        {
            string element = elements[random.Next(elements.Length)];
            double mass = GetAtomicMass(element);
            var atom = new Atom(i, element, mass);
            matter.Atoms.Add(atom);
            matter.TotalMass += mass;
        }

        materials.Add(matter);
        Console.WriteLine($"Created {materialType} with {atomCount} atoms, mass={matter.TotalMass:F4}");
    }

    private double GetAtomicMass(string element)
    {
        return element switch
        {
            "C" => 12.011,
            "O" => 15.999,
            "N" => 14.007,
            "H" => 1.008,
            "Si" => 28.085,
            "Fe" => 55.845,
            "Ca" => 40.078,
            _ => 10.0
        };
    }

    public void InitiateDeconstructionField(int materialId)
    {
        if (materialId >= materials.Count)
            return;

        var matter = materials[materialId];

        foreach (var atom in matter.Atoms)
        {
            double deconstructionEnergy = CalculateDeconstructionEnergy(atom);
            double entanglementStrength = CalculateEntanglementStrength(atom, deconstructionEnergy);

            atom.PresenceAmplitude = Math.Cos(entanglementStrength);
            atom.AbsenceAmplitude = Math.Sin(entanglementStrength);
            atom.EntanglementStrength = entanglementStrength;
            atom.Deconstructed = false;

            int idx = matter.Atoms.IndexOf(atom);
            matter.Atoms[idx] = atom;
        }

        materials[materialId] = matter;
    }

    private double CalculateDeconstructionEnergy(Atom atom)
    {
        return Math.Log(atom.MassNumber) * reducedPlanck * 1e20;
    }

    private double CalculateEntanglementStrength(Atom atom, double energy)
    {
        const double c = 3e8;
        return Math.Atan(energy / (atom.MassNumber * c * c));
    }

    public void DeconstructMatters(int steps)
    {
        Console.WriteLine($"\nDeconstructing materials in {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            foreach (var matter in materials)
            {
                ApplyQuantumEntanglement(ref matter);

                int deconstructedCount = matter.Atoms.Count(a => a.Deconstructed);
                matter.DeconstructionProgress = (double)deconstructedCount / matter.OriginalAtomCount;

                if (step % (steps / 5) == 0 && step > 0)
                {
                    Console.WriteLine($"Step {step}: {matter.MaterialType} - " +
                                    $"Progress={matter.DeconstructionProgress:P1}, " +
                                    $"Remaining={matter.Atoms.Count(a => !a.Deconstructed)}");
                }

                int matterIdx = materials.IndexOf(matter);
                materials[matterIdx] = matter;
            }
        }
    }

    private void ApplyQuantumEntanglement(ref Matter matter)
    {
        foreach (var atom in matter.Atoms)
        {
            if (atom.Deconstructed)
                continue;

            double probPresent = atom.PresenceAmplitude * atom.PresenceAmplitude;
            double probAbsent = atom.AbsenceAmplitude * atom.AbsenceAmplitude;

            if (random.NextDouble() < probAbsent)
            {
                atom.Deconstructed = true;
            }

            double timeFactor = atom.EntanglementStrength * 0.01;
            atom.PresenceAmplitude *= Math.Cos(timeFactor);
            atom.AbsenceAmplitude *= Math.Sin(timeFactor);

            int idx = matter.Atoms.IndexOf(atom);
            matter.Atoms[idx] = atom;
        }
    }

    public void ReconstructMatter(int materialId)
    {
        if (materialId >= materials.Count)
            return;

        var matter = materials[materialId];

        foreach (var atom in matter.Atoms)
        {
            atom.Deconstructed = false;
            atom.PresenceAmplitude = 1.0 / Math.Sqrt(2);
            atom.AbsenceAmplitude = 1.0 / Math.Sqrt(2);
            atom.EntanglementStrength = 0.0;

            int idx = matter.Atoms.IndexOf(atom);
            matter.Atoms[idx] = atom;
        }

        matter.DeconstructionProgress = 0.0;
        materials[materialId] = matter;

        Console.WriteLine($"\nReconstructed {matter.MaterialType}");
    }

    public Dictionary<string, object> GetDeconstructionMetrics()
    {
        double totalAtoms = materials.Sum(m => m.OriginalAtomCount);
        double totalDeconstructed = materials.Sum(m =>
            m.Atoms.Count(a => a.Deconstructed));
        double avgProgress = materials.Average(m => m.DeconstructionProgress);

        return new Dictionary<string, object>
        {
            { "MaterialCount", materials.Count },
            { "TotalAtoms", totalAtoms },
            { "TotalDeconstructed", totalDeconstructed },
            { "DeconstructionRate", totalAtoms > 0 ? totalDeconstructed / totalAtoms : 0 },
            { "AverageProgress", avgProgress }
        };
    }

    public void PrintMaterials()
    {
        Console.WriteLine("Materials Status:");
        foreach (var matter in materials)
        {
            int remaining = matter.Atoms.Count(a => !a.Deconstructed);
            Console.WriteLine($"  {matter.MaterialType}: " +
                            $"Progress={matter.DeconstructionProgress:P1}, " +
                            $"Atoms Remaining={remaining}/{matter.OriginalAtomCount}");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Deconstructor - Atomic-Level Matter Disassembly ===\n");

        var deconstructor = new QuantumDeconstructor();

        Console.WriteLine("--- Creating Materials ---");
        deconstructor.CreateMaterial("Carbon Sample", 20);
        deconstructor.CreateMaterial("Water Molecule", 15);
        deconstructor.CreateMaterial("Iron Alloy", 25);

        Console.WriteLine("\n--- Initiating Deconstruction Field ---");
        for (int i = 0; i < deconstructor.materials.Count; i++)
        {
            deconstructor.InitiateDeconstructionField(i);
        }

        Console.WriteLine("\n--- Initial Status ---");
        deconstructor.PrintMaterials();

        Console.WriteLine("\n--- Deconstructing Materials ---");
        deconstructor.DeconstructMatters(40);

        Console.WriteLine("\n--- Final Status ---");
        deconstructor.PrintMaterials();

        Console.WriteLine("\n--- Deconstruction Metrics ---");
        var metrics = deconstructor.GetDeconstructionMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Attempting Reconstruction ---");
        deconstructor.ReconstructMatter(0);
        deconstructor.PrintMaterials();

        Console.WriteLine("\nQuantum Deconstructor demonstrates matter disassembly via entanglement.");
    }
}
