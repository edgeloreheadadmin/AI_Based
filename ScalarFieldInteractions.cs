using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Scalar Field Interactions
/// Fields interact through scalar couplings
/// Used to describe fundamental particles and bosons (Higgs)
/// </summary>
public class ScalarFieldInteractions
{
    private struct ScalarField
    {
        public int FieldId;
        public double[] AmplitudeValues;
        public double LambdaCoupling;
        public double MassParameter;
        public double Potential;
        public double Vacuum;

        public ScalarField(int id, int latticeSize = 16)
        {
            FieldId = id;
            AmplitudeValues = new double[latticeSize];
            LambdaCoupling = 0.0;
            MassParameter = 0.0;
            Potential = 0.0;
            Vacuum = 0.0;

            for (int i = 0; i < latticeSize; i++)
            {
                AmplitudeValues[i] = Math.Sin(i * Math.PI / latticeSize);
            }
        }
    }

    private List<ScalarField> fields;
    private Random random;
    private const double PLANCK = 1.054e-34;

    public ScalarFieldInteractions()
    {
        this.fields = new List<ScalarField>();
        this.random = new Random();
    }

    public void CreateScalarField()
    {
        var field = new ScalarField(fields.Count, latticeSize: 16);
        field.LambdaCoupling = random.NextDouble() * 0.5;
        field.MassParameter = random.NextDouble() * 0.3;
        fields.Add(field);
        Console.WriteLine($"Created scalar field {field.FieldId}");
    }

    public void CalculatePotential()
    {
        foreach (var field in fields)
        {
            double potential = 0;

            for (int i = 0; i < field.AmplitudeValues.Length; i++)
            {
                double phi = field.AmplitudeValues[i];
                potential += Math.Pow(phi, 2) + field.LambdaCoupling * Math.Pow(phi, 4);
            }

            field.Potential = potential;

            double minPotential = double.MaxValue;
            for (int i = 0; i < field.AmplitudeValues.Length; i++)
            {
                double phi = field.AmplitudeValues[i];
                double v_eff = Math.Sqrt(field.MassParameter / (field.LambdaCoupling + 1e-10));
                minPotential = Math.Min(minPotential, Math.Abs(phi - v_eff));
            }

            field.Vacuum = minPotential;
            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplyScalarCouplings()
    {
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = i + 1; j < fields.Count; j++)
            {
                double coupling = fields[i].LambdaCoupling * fields[j].LambdaCoupling;

                for (int k = 0; k < fields[i].AmplitudeValues.Length; k++)
                {
                    double interaction = coupling * fields[j].AmplitudeValues[k];
                    fields[i].AmplitudeValues[k] *= (1 + interaction * 0.05);
                    fields[j].AmplitudeValues[k] *= (1 + interaction * 0.05);
                }
            }
        }

        NormalizeFields();
    }

    private void NormalizeFields()
    {
        foreach (var field in fields)
        {
            double norm = Math.Sqrt(field.AmplitudeValues.Sum(x => x * x));

            if (norm > 0)
            {
                for (int i = 0; i < field.AmplitudeValues.Length; i++)
                {
                    field.AmplitudeValues[i] /= norm;
                }
            }

            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void SimulateSymmetryBreaking()
    {
        Console.WriteLine("\nSimulating spontaneous symmetry breaking...\n");

        foreach (var field in fields)
        {
            double temperature = random.NextDouble() * 10;
            Console.WriteLine($"Field {field.FieldId}: Temperature = {temperature:F3}K");

            if (temperature < 1.0)
            {
                for (int i = 0; i < field.AmplitudeValues.Length; i++)
                {
                    field.AmplitudeValues[i] = Math.Sign(field.AmplitudeValues[i]) *
                                              (Math.Abs(field.AmplitudeValues[i]) + 0.5);
                }
                Console.WriteLine($"  → Symmetry broken, VEV non-zero");
            }
            else
            {
                Console.WriteLine($"  → Symmetric phase, VEV = 0");
            }

            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void EvolveScalarFields(int steps)
    {
        Console.WriteLine($"\nEvolving scalar fields for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            ApplyScalarCouplings();
            CalculatePotential();

            if (step % (steps / 3) == 0)
            {
                double totalPotential = fields.Sum(f => f.Potential);
                Console.WriteLine($"Step {step}: Total Potential = {totalPotential:F6}");
            }
        }
    }

    public Dictionary<string, object> GetScalarMetrics()
    {
        double totalPotential = fields.Sum(f => f.Potential);
        double avgCoupling = fields.Average(f => f.LambdaCoupling);
        double avgMass = fields.Average(f => f.MassParameter);
        double avgVacuum = fields.Average(f => f.Vacuum);

        return new Dictionary<string, object>
        {
            { "FieldCount", fields.Count },
            { "TotalPotential", totalPotential },
            { "AverageLambdaCoupling", avgCoupling },
            { "AverageMassParameter", avgMass },
            { "AverageVacuumExpectationValue", avgVacuum }
        };
    }

    public void PrintFieldState(int fieldId)
    {
        if (fieldId >= fields.Count)
            return;

        var field = fields[fieldId];
        Console.WriteLine($"\nScalar Field {fieldId} State:");
        Console.WriteLine($"  Amplitudes: [{string.Join(", ", field.AmplitudeValues.Take(8).Select(x => x.ToString("F3")))}...]");
        Console.WriteLine($"  Potential: {field.Potential:F6}");
        Console.WriteLine($"  Vacuum: {field.Vacuum:F6}");
        Console.WriteLine($"  λ Coupling: {field.LambdaCoupling:F4}");
    }

    public static void Main()
    {
        Console.WriteLine("=== Scalar Field Interactions - Higgs-like Coupling ===\n");

        var sfi = new ScalarFieldInteractions();

        Console.WriteLine("--- Creating Scalar Fields ---");
        for (int i = 0; i < 3; i++)
        {
            sfi.CreateScalarField();
        }

        sfi.PrintFieldState(0);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = sfi.GetScalarMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Simulating Symmetry Breaking ---");
        sfi.SimulateSymmetryBreaking();

        Console.WriteLine("\n--- Evolving Scalar Fields ---");
        sfi.EvolveScalarFields(steps: 30);

        sfi.PrintFieldState(0);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = sfi.GetScalarMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nScalar Field Interactions model Higgs mechanism.");
    }
}
