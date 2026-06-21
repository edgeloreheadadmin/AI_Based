using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Spinor Field Interactions
/// Fields interact through spinor couplings
/// Used to describe fermions and matter particles
/// </summary>
public class SpinorFieldInteractions
{
    private struct SpinorField
    {
        public int FieldId;
        public double[] SpinUpAmplitude;
        public double[] SpinDownAmplitude;
        public double ChiralityLeft;
        public double ChiralityRight;
        public double MassParameter;
        public double CouplingStrength;

        public SpinorField(int id, int components = 4)
        {
            FieldId = id;
            SpinUpAmplitude = new double[components];
            SpinDownAmplitude = new double[components];
            ChiralityLeft = 0.0;
            ChiralityRight = 0.0;
            MassParameter = 0.0;
            CouplingStrength = 0.0;

            for (int i = 0; i < components; i++)
            {
                SpinUpAmplitude[i] = Math.Cos(i * Math.PI / components) / Math.Sqrt(components);
                SpinDownAmplitude[i] = Math.Sin(i * Math.PI / components) / Math.Sqrt(components);
            }
        }
    }

    private List<SpinorField> fields;
    private double[][] diracMatrix;
    private Random random;

    public SpinorFieldInteractions()
    {
        this.fields = new List<SpinorField>();
        this.random = new Random();
        InitializeDiracMatrices();
    }

    private void InitializeDiracMatrices()
    {
        diracMatrix = new double[4][];
        for (int i = 0; i < 4; i++)
        {
            diracMatrix[i] = new double[4];
            for (int j = 0; j < 4; j++)
            {
                diracMatrix[i][j] = random.NextDouble() * 0.5;
            }
        }
    }

    public void CreateSpinorField()
    {
        var field = new SpinorField(fields.Count, components: 4);
        field.MassParameter = random.NextDouble() * 0.1;
        field.CouplingStrength = random.NextDouble() * 0.3;
        fields.Add(field);
        Console.WriteLine($"Created spinor field {field.FieldId}");
    }

    public void CalculateChirality()
    {
        foreach (var field in fields)
        {
            double leftHandedness = 0;
            double rightHandedness = 0;

            for (int i = 0; i < field.SpinUpAmplitude.Length; i++)
            {
                leftHandedness += field.SpinUpAmplitude[i] * field.SpinUpAmplitude[i];
                rightHandedness += field.SpinDownAmplitude[i] * field.SpinDownAmplitude[i];
            }

            field.ChiralityLeft = Math.Abs(leftHandedness);
            field.ChiralityRight = Math.Abs(rightHandedness);

            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplyDiracEquation()
    {
        foreach (var field in fields)
        {
            for (int i = 0; i < field.SpinUpAmplitude.Length; i++)
            {
                double newSpinUp = 0;
                double newSpinDown = 0;

                for (int j = 0; j < 4; j++)
                {
                    newSpinUp += diracMatrix[i][j] * field.SpinUpAmplitude[j];
                    newSpinDown += diracMatrix[i][j] * field.SpinDownAmplitude[j];
                }

                newSpinUp += field.MassParameter * field.SpinDownAmplitude[i];
                newSpinDown += field.MassParameter * field.SpinUpAmplitude[i];

                field.SpinUpAmplitude[i] = newSpinUp * 0.5;
                field.SpinDownAmplitude[i] = newSpinDown * 0.5;
            }

            int idx = fields.IndexOf(field);
            fields[idx] = field;
        }
    }

    public void ApplySpinorCouplings()
    {
        for (int i = 0; i < fields.Count; i++)
        {
            for (int j = i + 1; j < fields.Count; j++)
            {
                double coupling = fields[i].CouplingStrength * fields[j].CouplingStrength;

                for (int k = 0; k < fields[i].SpinUpAmplitude.Length; k++)
                {
                    fields[i].SpinUpAmplitude[k] +=
                        coupling * fields[j].SpinDownAmplitude[k] * 0.05;

                    fields[j].SpinDownAmplitude[k] +=
                        coupling * fields[i].SpinUpAmplitude[k] * 0.05;
                }
            }
        }
    }

    public void EvolveFermions(int steps)
    {
        Console.WriteLine($"\nEvolving spinor fields for {steps} steps...\n");

        for (int step = 0; step < steps; step++)
        {
            ApplyDiracEquation();
            ApplySpinorCouplings();
            CalculateChirality();

            if (step % (steps / 3) == 0)
            {
                double avgChirality = fields.Average(f => f.ChiralityLeft);
                Console.WriteLine($"Step {step}: Average Chirality = {avgChirality:F6}");
            }
        }
    }

    public Dictionary<string, object> GetSpinorMetrics()
    {
        double avgChiralityLeft = fields.Average(f => f.ChiralityLeft);
        double avgChiralityRight = fields.Average(f => f.ChiralityRight);
        double avgMass = fields.Average(f => f.MassParameter);
        double avgCoupling = fields.Average(f => f.CouplingStrength);

        return new Dictionary<string, object>
        {
            { "FieldCount", fields.Count },
            { "AvgChiralityLeft", avgChiralityLeft },
            { "AvgChiralityRight", avgChiralityRight },
            { "AvgMassParameter", avgMass },
            { "AvgCouplingStrength", avgCoupling }
        };
    }

    public void PrintSpinorState(int fieldId)
    {
        if (fieldId >= fields.Count)
            return;

        var field = fields[fieldId];
        Console.WriteLine($"\nSpinor Field {fieldId} State:");
        Console.WriteLine($"  Spin Up: [{string.Join(", ", field.SpinUpAmplitude.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Spin Down: [{string.Join(", ", field.SpinDownAmplitude.Select(x => x.ToString("F4")))}]");
        Console.WriteLine($"  Chirality L/R: {field.ChiralityLeft:F4}/{field.ChiralityRight:F4}");
    }

    public static void Main()
    {
        Console.WriteLine("=== Spinor Field Interactions - Fermion Coupling ===\n");

        var sfi = new SpinorFieldInteractions();

        Console.WriteLine("--- Creating Spinor Fields ---");
        for (int i = 0; i < 3; i++)
        {
            sfi.CreateSpinorField();
        }

        sfi.PrintSpinorState(0);

        Console.WriteLine("\n--- Initial Metrics ---");
        var metricsInitial = sfi.GetSpinorMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Fermion Fields ---");
        sfi.EvolveFermions(steps: 30);

        sfi.PrintSpinorState(0);

        Console.WriteLine("\n--- Final Metrics ---");
        var metricsFinal = sfi.GetSpinorMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F6}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nSpinor Field Interactions model fermion behavior.");
    }
}
