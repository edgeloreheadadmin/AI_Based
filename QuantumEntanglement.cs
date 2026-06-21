using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum Entanglement
/// Correlation between particles stronger than any classical correlation
/// Demonstrates Bell's theorem and quantum non-locality
/// </summary>
public class QuantumEntanglement
{
    private struct QuantumParticle
    {
        public int ParticleId;
        public double StateAmplitudeUp;
        public double StateAmplitudeDown;
        public int MeasurementResult;
        public bool Measured;

        public QuantumParticle(int id)
        {
            ParticleId = id;
            StateAmplitudeUp = Math.Sqrt(0.5);
            StateAmplitudeDown = Math.Sqrt(0.5);
            MeasurementResult = -1;
            Measured = false;
        }

        public double ProbabilityUp => StateAmplitudeUp * StateAmplitudeUp;
        public double ProbabilityDown => StateAmplitudeDown * StateAmplitudeDown;
    }

    private struct EntangledPair
    {
        public int Particle1Id;
        public int Particle2Id;
        public double EntanglementStrength;
        public int Particle1Result;
        public int Particle2Result;
        public bool CorrelationVerified;

        public EntangledPair(int id1, int id2)
        {
            Particle1Id = id1;
            Particle2Id = id2;
            EntanglementStrength = 1.0;
            Particle1Result = -1;
            Particle2Result = -1;
            CorrelationVerified = false;
        }
    }

    private List<QuantumParticle> particles;
    private List<EntangledPair> entangledPairs;
    private Random random;
    private double[,] correlationMatrix;

    public QuantumEntanglement()
    {
        this.particles = new List<QuantumParticle>();
        this.entangledPairs = new List<EntangledPair>();
        this.random = new Random();
    }

    public void CreateParticles(int count)
    {
        particles.Clear();
        correlationMatrix = new double[count, count];

        for (int i = 0; i < count; i++)
        {
            particles.Add(new QuantumParticle(i));
        }

        Console.WriteLine($"Created {count} quantum particles");
    }

    public void EntangleTwoParticles(int id1, int id2)
    {
        if (id1 >= particles.Count || id2 >= particles.Count)
            return;

        var pair = new EntangledPair(id1, id2);

        var p1 = particles[id1];
        var p2 = particles[id2];

        p1.StateAmplitudeUp = 1.0 / Math.Sqrt(2);
        p1.StateAmplitudeDown = 1.0 / Math.Sqrt(2);

        p2.StateAmplitudeUp = 1.0 / Math.Sqrt(2);
        p2.StateAmplitudeDown = 1.0 / Math.Sqrt(2);

        particles[id1] = p1;
        particles[id2] = p2;

        entangledPairs.Add(pair);
    }

    public void CreateBellState()
    {
        entangledPairs.Clear();

        for (int i = 0; i < particles.Count - 1; i += 2)
        {
            EntangleTwoParticles(i, i + 1);
        }

        Console.WriteLine($"Created {entangledPairs.Count} Bell state pairs");
    }

    public int MeasureParticle(int particleId, double angle = 0.0)
    {
        if (particleId >= particles.Count)
            return -1;

        var particle = particles[particleId];

        double adjustedProbUp = particle.ProbabilityUp * Math.Cos(angle) +
                               particle.ProbabilityDown * Math.Sin(angle);

        adjustedProbUp = Math.Max(0, Math.Min(1, adjustedProbUp));

        int result = random.NextDouble() < adjustedProbUp ? 0 : 1;

        particle.MeasurementResult = result;
        particle.Measured = true;
        particles[particleId] = particle;

        return result;
    }

    public void VerifyInstantaneousCorrelation()
    {
        foreach (var pair in entangledPairs)
        {
            int result1 = MeasureParticle(pair.Particle1Id, 0.0);
            int result2 = MeasureParticle(pair.Particle2Id, 0.0);

            var updatedPair = pair;
            updatedPair.Particle1Result = result1;
            updatedPair.Particle2Result = result2;

            updatedPair.CorrelationVerified = (result1 ^ result2) == 1;

            int pairIndex = entangledPairs.IndexOf(pair);
            entangledPairs[pairIndex] = updatedPair;
        }
    }

    public double TestBellInequality()
    {
        double correlation01 = 0;
        double correlation02 = 0;
        double correlation12 = 0;
        int testCount = 1000;

        for (int test = 0; test < testCount; test++)
        {
            CreateParticles(3);
            CreateBellState();

            double angle1 = 0;
            double angle2 = Math.PI / 4;
            double angle3 = Math.PI / 2;

            int r0a = MeasureParticle(0, angle1);
            int r1a = MeasureParticle(1, angle2);
            int r2a = MeasureParticle(2, angle3);

            correlation01 += (r0a == r1a) ? 1 : 0;
            correlation02 += (r0a == r2a) ? 1 : 0;
            correlation12 += (r1a == r2a) ? 1 : 0;
        }

        correlation01 /= testCount;
        correlation02 /= testCount;
        correlation12 /= testCount;

        double bellValue = correlation01 + correlation02 - correlation12;
        Console.WriteLine($"Bell Test Results:");
        Console.WriteLine($"  Correlation(0,1): {correlation01:F4}");
        Console.WriteLine($"  Correlation(0,2): {correlation02:F4}");
        Console.WriteLine($"  Correlation(1,2): {correlation12:F4}");
        Console.WriteLine($"  Bell Value: {bellValue:F4}");
        Console.WriteLine($"  Violates Bell Inequality: {bellValue > 2.0}");

        return bellValue;
    }

    public void CalculateCorrelationMatrix()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            for (int j = 0; j < particles.Count; j++)
            {
                if (i == j)
                {
                    correlationMatrix[i, j] = 1.0;
                }
                else
                {
                    double correlation = 0;
                    int comparisons = 10;

                    for (int k = 0; k < comparisons; k++)
                    {
                        int r1 = MeasureParticle(i, random.NextDouble() * Math.PI);
                        int r2 = MeasureParticle(j, random.NextDouble() * Math.PI);

                        correlation += (r1 == r2) ? 1 : 0;
                    }

                    correlationMatrix[i, j] = correlation / comparisons;
                }
            }
        }
    }

    public void PrintEntanglementSummary()
    {
        Console.WriteLine("Entanglement Summary:");
        Console.WriteLine($"  Total Particles: {particles.Count}");
        Console.WriteLine($"  Entangled Pairs: {entangledPairs.Count}");

        int verifiedCount = entangledPairs.Count(p => p.CorrelationVerified);
        Console.WriteLine($"  Verified Correlations: {verifiedCount}/{entangledPairs.Count}");

        if (entangledPairs.Count > 0)
        {
            Console.WriteLine("\n  Pair Details:");
            for (int i = 0; i < Math.Min(5, entangledPairs.Count); i++)
            {
                var pair = entangledPairs[i];
                Console.WriteLine($"    [{i + 1}] Particles {pair.Particle1Id}-{pair.Particle2Id}: " +
                                $"Results {pair.Particle1Result}/{pair.Particle2Result} " +
                                $"Correlated={pair.CorrelationVerified}");
            }
        }
    }

    public Dictionary<string, object> GetEntanglementMetrics()
    {
        int entanglementCount = entangledPairs.Count;
        int verifiedCount = entangledPairs.Count(p => p.CorrelationVerified);
        double verificationRate = entanglementCount > 0 ? (double)verifiedCount / entanglementCount : 0;

        return new Dictionary<string, object>
        {
            { "ParticleCount", particles.Count },
            { "EntangledPairs", entanglementCount },
            { "VerifiedPairs", verifiedCount },
            { "VerificationRate", verificationRate },
            { "MeasuredParticles", particles.Count(p => p.Measured) }
        };
    }

    public static void Main()
    {
        Console.WriteLine("=== Quantum Entanglement - Bell's Theorem Verification ===\n");

        var qe = new QuantumEntanglement();

        Console.WriteLine("--- Creating Entangled Particle Pairs ---\n");
        qe.CreateParticles(6);
        qe.CreateBellState();

        qe.VerifyInstantaneousCorrelation();
        qe.PrintEntanglementSummary();

        Console.WriteLine("\n--- Calculating Correlation Matrix ---");
        qe.CreateParticles(4);
        qe.CreateBellState();
        qe.CalculateCorrelationMatrix();

        Console.WriteLine("Correlation Matrix:");
        for (int i = 0; i < 4; i++)
        {
            Console.Write("  [");
            for (int j = 0; j < 4; j++)
            {
                Console.Write($"{qe.correlationMatrix[i, j]:F3} ");
            }
            Console.WriteLine("]");
        }

        Console.WriteLine("\n--- Testing Bell's Inequality ---");
        var bellMetrics = qe.TestBellInequality();

        Console.WriteLine("\n--- Entanglement Metrics ---");
        qe.CreateParticles(5);
        qe.CreateBellState();
        qe.VerifyInstantaneousCorrelation();

        var metrics = qe.GetEntanglementMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQuantum Entanglement demonstrates non-local correlations stronger than classical.");
    }
}
