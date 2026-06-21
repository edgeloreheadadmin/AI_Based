using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lamb Shift
/// Small shift in energy levels of hydrogen atoms due to electron interaction with vacuum
/// Demonstrates precision quantum electrodynamics effects
/// </summary>
public class LambShift
{
    private struct HydrogenAtom
    {
        public int AtomId;
        public int PrincipalQuantumNumber;
        public int OrbitalAngularMomentum;
        public double NominalEnergy;
        public double VacuumFluctuationCorrection;
        public double LambShiftEnergy;
        public double FinalEnergy;

        public HydrogenAtom(int id, int n, int l)
        {
            AtomId = id;
            PrincipalQuantumNumber = n;
            OrbitalAngularMomentum = l;
            NominalEnergy = CalculateNominalEnergy(n);
            VacuumFluctuationCorrection = 0.0;
            LambShiftEnergy = 0.0;
            FinalEnergy = NominalEnergy;
        }

        private static double CalculateNominalEnergy(int n)
        {
            const double eV = 1.602e-19;
            const double RydbergEnergy = 13.6;
            return -RydbergEnergy * eV / (n * n);
        }
    }

    private List<HydrogenAtom> atoms;
    private double fineStructureConstant;
    private double electronMass;
    private double reducedPlanck;
    private Random random;

    public LambShift()
    {
        this.atoms = new List<HydrogenAtom>();
        this.fineStructureConstant = 1.0 / 137.036;
        this.electronMass = 9.109e-31;
        this.reducedPlanck = 1.054e-34;
        this.random = new Random();
    }

    public void CreateHydrogenAtoms(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int n = random.Next(2, 5);
            int l = random.Next(0, n);
            atoms.Add(new HydrogenAtom(i, n, l));
        }
        Console.WriteLine($"Created {count} hydrogen atoms");
    }

    public void CalculateLambShiftCorrection()
    {
        foreach (var atom in atoms)
        {
            double vacuumCorrection = CalculateVacuumFluctuationCorrection(atom);
            double lambShift = CalculateLambShift(atom, vacuumCorrection);

            atom.VacuumFluctuationCorrection = vacuumCorrection;
            atom.LambShiftEnergy = lambShift;
            atom.FinalEnergy = atom.NominalEnergy + lambShift;

            int idx = atoms.IndexOf(atom);
            atoms[idx] = atom;
        }
    }

    private double CalculateVacuumFluctuationCorrection(HydrogenAtom atom)
    {
        const double c = 3e8;
        const double eV = 1.602e-19;

        double n = atom.PrincipalQuantumNumber;
        double l = atom.OrbitalAngularMomentum;

        double alphaCorrection = fineStructureConstant * fineStructureConstant;
        double energyCorrection = 4.0 * eV / (3 * Math.PI);

        double vacuumEffect = alphaCorrection * energyCorrection *
                             Math.Log(1.0 / (fineStructureConstant * fineStructureConstant)) /
                             (n * n);

        return vacuumEffect;
    }

    private double CalculateLambShift(HydrogenAtom atom, double vacuumCorrection)
    {
        const double c = 3e8;
        const double eV = 1.602e-19;

        double n = atom.PrincipalQuantumNumber;
        double l = atom.OrbitalAngularMomentum;

        double lambShift = fineStructureConstant * eV * vacuumCorrection / 1000.0;

        if (n == 2 && l == 0)
        {
            lambShift = 1.058e-6 * eV;
        }
        else if (n == 2 && l == 1)
        {
            lambShift = 0.0;
        }
        else
        {
            lambShift *= Math.Pow(n, -3) * (1 + l * 0.5);
        }

        return lambShift;
    }

    public void ApplyQuantumElectrodynamicsCorrection()
    {
        foreach (var atom in atoms)
        {
            double qedCorrection = CalculateQEDCorrection(atom);
            atom.FinalEnergy += qedCorrection;

            int idx = atoms.IndexOf(atom);
            atoms[idx] = atom;
        }
    }

    private double CalculateQEDCorrection(HydrogenAtom atom)
    {
        const double eV = 1.602e-19;

        double n = atom.PrincipalQuantumNumber;
        double l = atom.OrbitalAngularMomentum;

        double alphaFour = Math.Pow(fineStructureConstant, 4);
        double correction = alphaFour * eV / (n * n * n);

        return correction * (1.5 + l);
    }

    public Dictionary<string, object> GetLambShiftMetrics()
    {
        double avgLambShift = atoms.Average(a => a.LambShiftEnergy);
        double maxLambShift = atoms.Max(a => a.LambShiftEnergy);
        double minLambShift = atoms.Min(a => a.LambShiftEnergy);
        double totalEnergyShift = atoms.Sum(a => a.LambShiftEnergy);

        return new Dictionary<string, object>
        {
            { "TotalAtoms", atoms.Count },
            { "AverageLambShift", avgLambShift },
            { "MaxLambShift", maxLambShift },
            { "MinLambShift", minLambShift },
            { "TotalEnergyShift", totalEnergyShift },
            { "FineStructureConstant", fineStructureConstant }
        };
    }

    public void PrintAtomEnergyComparison(int limit = 10)
    {
        Console.WriteLine("Atom Energy Level Comparison:");
        Console.WriteLine("ID  | State   | Nominal Energy | Lamb Shift    | Final Energy");
        Console.WriteLine("----|---------|----------------|---------------|---------------");

        for (int i = 0; i < Math.Min(limit, atoms.Count); i++)
        {
            var atom = atoms[i];
            string state = $"n={atom.PrincipalQuantumNumber},l={atom.OrbitalAngularMomentum}";
            Console.WriteLine($"{i,-3} | {state,-7} | {atom.NominalEnergy:E11} | {atom.LambShiftEnergy:E11} | {atom.FinalEnergy:E11}");
        }
    }

    public Dictionary<string, int> GetStateDistribution()
    {
        Dictionary<string, int> distribution = new Dictionary<string, int>();

        foreach (var atom in atoms)
        {
            string state = $"n={atom.PrincipalQuantumNumber}";
            if (!distribution.ContainsKey(state))
                distribution[state] = 0;
            distribution[state]++;
        }

        return distribution.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    public void PrintLambShiftDistribution()
    {
        Console.WriteLine("Lamb Shift Distribution:");

        var sortedShifts = atoms.Select(a => a.LambShiftEnergy).OrderBy(x => x).ToList();
        double minShift = sortedShifts.First();
        double maxShift = sortedShifts.Last();
        double binWidth = (maxShift - minShift) / 8;

        for (int i = 0; i < 8; i++)
        {
            double binStart = minShift + i * binWidth;
            double binEnd = binStart + binWidth;

            int count = sortedShifts.Count(x => x >= binStart && x < binEnd);
            string bar = new string('█', count);
            Console.WriteLine($"  [{binStart:E3}]: {bar} ({count})");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Lamb Shift - Vacuum Fluctuation Energy Correction ===\n");

        var lambShift = new LambShift();

        Console.WriteLine("Creating hydrogen atoms...");
        lambShift.CreateHydrogenAtoms(50);

        Console.WriteLine("\n--- Calculating Lamb Shift Corrections ---");
        lambShift.CalculateLambShiftCorrection();

        lambShift.PrintAtomEnergyComparison(8);

        Console.WriteLine("\n--- Applying QED Corrections ---");
        lambShift.ApplyQuantumElectrodynamicsCorrection();

        Console.WriteLine("\n--- Updated Energy Levels ---");
        lambShift.PrintAtomEnergyComparison(8);

        Console.WriteLine("\n--- Energy Level Distribution ---");
        var distribution = lambShift.GetStateDistribution();
        foreach (var kvp in distribution)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value} atoms");
        }

        Console.WriteLine("\n--- Lamb Shift Metrics ---");
        var metrics = lambShift.GetLambShiftMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Lamb Shift Distribution ---");
        lambShift.PrintLambShiftDistribution();

        Console.WriteLine("\nLamb Shift successfully demonstrates vacuum fluctuation corrections.");
    }
}
