using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Hawking Radiation
/// Thermal radiation emitted by black holes due to virtual particle creation near event horizon
/// Demonstrates black hole evaporation process
/// </summary>
public class HawkingRadiation
{
    private struct VirtualParticlePair
    {
        public int PairId;
        public double Energy;
        public double CreationTime;
        public double LifeTime;
        public bool CapturedInHorizon;
        public double InformationContent;

        public VirtualParticlePair(int id, double energy, double time)
        {
            PairId = id;
            Energy = energy;
            CreationTime = time;
            LifeTime = CalculateLifeTime(energy);
            CapturedInHorizon = false;
            InformationContent = Math.Log(energy + 1);
        }

        private static double CalculateLifeTime(double energy)
        {
            const double hBar = 1.054e-34;
            return hBar / energy;
        }
    }

    private struct BlackHole
    {
        public double Mass;
        public double SchwarzschildRadius;
        public double Temperature;
        public double EvaporationRate;
        public double TotalRadiationEmitted;
        public int PairsCreated;
        public int ParticlesEscaped;

        public BlackHole(double initialMass)
        {
            Mass = initialMass;
            SchwarzschildRadius = CalculateSchwarzschild(initialMass);
            Temperature = CalculateHawkingTemperature(initialMass);
            EvaporationRate = CalculateEvaporationRate(initialMass);
            TotalRadiationEmitted = 0.0;
            PairsCreated = 0;
            ParticlesEscaped = 0;
        }

        private static double CalculateSchwarzschild(double mass)
        {
            const double G = 6.674e-11;
            const double c = 3e8;
            return 2 * G * mass / (c * c);
        }

        private static double CalculateHawkingTemperature(double mass)
        {
            const double hBar = 1.054e-34;
            const double c = 3e8;
            const double G = 6.674e-11;
            const double k = 1.380649e-23;

            return (hBar * c * c * c) / (8 * Math.PI * G * mass * k);
        }

        private static double CalculateEvaporationRate(double mass)
        {
            return 1.0 / Math.Pow(mass, 2);
        }
    }

    private BlackHole blackHole;
    private List<VirtualParticlePair> virtualPairs;
    private List<double> radiationSpectrum;
    private Random random;
    private double simulationTime;

    public HawkingRadiation(double blackHoleMass = 1e30)
    {
        this.blackHole = new BlackHole(blackHoleMass);
        this.virtualPairs = new List<VirtualParticlePair>();
        this.radiationSpectrum = new List<double>();
        this.random = new Random();
        this.simulationTime = 0.0;

        Console.WriteLine($"Created Black Hole:");
        Console.WriteLine($"  Mass: {blackHole.Mass:E4} kg");
        Console.WriteLine($"  Schwarzschild Radius: {blackHole.SchwarzschildRadius:E4} m");
        Console.WriteLine($"  Hawking Temperature: {blackHole.Temperature:E4} K");
    }

    public void CreateVirtualPairNearHorizon()
    {
        double creationProbability = blackHole.EvaporationRate * 0.1;

        if (random.NextDouble() < creationProbability)
        {
            double energy = random.NextDouble() * blackHole.Temperature * 1e-20;
            var pair = new VirtualParticlePair(virtualPairs.Count, energy, simulationTime);

            virtualPairs.Add(pair);
            blackHole.PairsCreated++;
        }
    }

    public void SimulateParticleDestiny()
    {
        var pairsToRemove = new List<int>();

        for (int i = 0; i < virtualPairs.Count; i++)
        {
            var pair = virtualPairs[i];
            double timeElapsed = simulationTime - pair.CreationTime;

            double horizonDistance = blackHole.SchwarzschildRadius + random.NextDouble() * 0.1;
            double escapeProbability = Math.Exp(-horizonDistance / (blackHole.SchwarzschildRadius * 0.5));

            if (random.NextDouble() < escapeProbability && timeElapsed < pair.LifeTime)
            {
                pair.CapturedInHorizon = false;
                blackHole.TotalRadiationEmitted += pair.Energy;
                radiationSpectrum.Add(pair.Energy);
                blackHole.ParticlesEscaped++;
                pairsToRemove.Add(i);
            }
            else if (timeElapsed > pair.LifeTime)
            {
                if (pair.CapturedInHorizon)
                {
                    blackHole.Mass -= pair.Energy / Math.Pow(3e8, 2);
                }
                pairsToRemove.Add(i);
            }
            else
            {
                pair.CapturedInHorizon = random.NextDouble() < 0.5;
                virtualPairs[i] = pair;
            }
        }

        for (int i = pairsToRemove.Count - 1; i >= 0; i--)
        {
            virtualPairs.RemoveAt(pairsToRemove[i]);
        }
    }

    public void SimulateHawkingEvaporation(int timeSteps)
    {
        Console.WriteLine($"\nSimulating Black Hole Evaporation ({timeSteps} time steps)...\n");

        for (int step = 0; step < timeSteps; step++)
        {
            simulationTime += 0.001;

            CreateVirtualPairNearHorizon();
            SimulateParticleDestiny();

            if (step % 20 == 0)
            {
                Console.WriteLine($"Time {simulationTime:F4}: Mass={blackHole.Mass:E4}, " +
                                $"Radiation={blackHole.TotalRadiationEmitted:E4}, " +
                                $"Pairs Created={blackHole.PairsCreated}, " +
                                $"Escaped={blackHole.ParticlesEscaped}");
            }

            if (blackHole.Mass < 1e10)
                break;
        }
    }

    public Dictionary<string, object> GetEvaporationMetrics()
    {
        double avgRadiation = radiationSpectrum.Count > 0 ?
            radiationSpectrum.Average() : 0;
        double maxRadiation = radiationSpectrum.Count > 0 ?
            radiationSpectrum.Max() : 0;

        return new Dictionary<string, object>
        {
            { "CurrentMass", blackHole.Mass },
            { "SchwarzschildRadius", blackHole.SchwarzschildRadius },
            { "Temperature", blackHole.Temperature },
            { "TotalRadiationEmitted", blackHole.TotalRadiationEmitted },
            { "PairsCreated", blackHole.PairsCreated },
            { "ParticlesEscaped", blackHole.ParticlesEscaped },
            { "AverageRadiation", avgRadiation },
            { "MaxRadiation", maxRadiation }
        };
    }

    public void PrintRadiationSpectrum()
    {
        if (radiationSpectrum.Count == 0)
            return;

        Console.WriteLine("Radiation Spectrum Distribution:");
        var sorted = radiationSpectrum.OrderBy(x => x).ToList();

        int bins = 10;
        double minVal = sorted.First();
        double maxVal = sorted.Last();
        double binWidth = (maxVal - minVal) / bins;

        for (int i = 0; i < bins; i++)
        {
            double binStart = minVal + i * binWidth;
            double binEnd = binStart + binWidth;

            int count = sorted.Count(x => x >= binStart && x < binEnd);
            string bar = new string('█', count);
            Console.WriteLine($"  [{binStart:E3}]: {bar} ({count})");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Hawking Radiation - Black Hole Evaporation ===\n");

        var hawking = new HawkingRadiation(blackHoleMass: 1e30);

        Console.WriteLine("\n--- Simulating Hawking Radiation ---");
        hawking.SimulateHawkingEvaporation(timeSteps: 200);

        Console.WriteLine("\n--- Evaporation Metrics ---");
        var metrics = hawking.GetEvaporationMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Radiation Spectrum ---");
        hawking.PrintRadiationSpectrum();

        Console.WriteLine("\nHawking Radiation successfully demonstrates black hole evaporation.");
    }
}
