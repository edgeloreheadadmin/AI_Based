using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Cosmic Quantum Flux
/// Fluctuations in the energy density of the universe
/// Simulates cosmic structure, dark energy, and quantum vacuum effects
/// </summary>
public class CosmicQuantumFlux
{
    private struct UniverseRegion
    {
        public int RegionId;
        public double X, Y, Z;
        public double EnergyDensity;
        public double QuantumVacuumEnergy;
        public double CurvatureParameter;
        public double DarkEnergyDensity;
        public double MatterDensity;

        public UniverseRegion(int id, double x, double y, double z)
        {
            RegionId = id;
            X = x;
            Y = y;
            Z = z;
            EnergyDensity = 0.0;
            QuantumVacuumEnergy = 0.0;
            CurvatureParameter = 0.0;
            DarkEnergyDensity = 0.0;
            MatterDensity = 0.0;
        }
    }

    private struct CosmicFluctuationMode
    {
        public int ModeId;
        public double WavelengthScale;
        public double AmplitudeModulation;
        public double Phase;
        public double GrowthRate;

        public CosmicFluctuationMode(int id, double scale)
        {
            ModeId = id;
            WavelengthScale = scale;
            AmplitudeModulation = 0.0;
            Phase = 0.0;
            GrowthRate = 0.0;
        }
    }

    private List<UniverseRegion> regions;
    private List<CosmicFluctuationMode> fluctuationModes;
    private double hubbleConstant;
    private double cosmologicalConstant;
    private double matterDensity;
    private Random random;
    private double cosmicTime;

    public CosmicQuantumFlux()
    {
        this.regions = new List<UniverseRegion>();
        this.fluctuationModes = new List<CosmicFluctuationMode>();
        this.hubbleConstant = 67.4;
        this.cosmologicalConstant = 1.11e-52;
        this.matterDensity = 0.27;
        this.random = new Random();
        this.cosmicTime = 0.0;
    }

    public void InitializeUniverseRegions(int gridSize)
    {
        regions.Clear();

        double spacing = 1.0 / gridSize;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                for (int k = 0; k < gridSize; k++)
                {
                    double x = i * spacing;
                    double y = j * spacing;
                    double z = k * spacing;

                    var region = new UniverseRegion(regions.Count, x, y, z);
                    regions.Add(region);
                }
            }
        }

        Console.WriteLine($"Initialized {regions.Count} universe regions in {gridSize}³ grid");
    }

    public void GenerateQuantumFluctuations()
    {
        foreach (var region in regions)
        {
            double vacuumEnergy = GenerateVacuumEnergyFluctuation(region);
            double darkEnergyDensity = cosmologicalConstant * 1e-52;
            double matterComponent = CalculateMatterDensity(region);

            region.QuantumVacuumEnergy = vacuumEnergy;
            region.DarkEnergyDensity = darkEnergyDensity;
            region.MatterDensity = matterComponent;
            region.EnergyDensity = vacuumEnergy + darkEnergyDensity + matterComponent;
            region.CurvatureParameter = CalculateSpaceTimeCurvature(region);

            int idx = regions.IndexOf(region);
            regions[idx] = region;
        }
    }

    private double GenerateVacuumEnergyFluctuation(UniverseRegion region)
    {
        double baseEnergy = 1.0e-10;
        double fluctuation = Math.Sin(region.X * Math.PI * 10) *
                            Math.Cos(region.Y * Math.PI * 10) *
                            Math.Sin(region.Z * Math.PI * 10);

        return baseEnergy * (1 + fluctuation);
    }

    private double CalculateMatterDensity(UniverseRegion region)
    {
        double criticalDensity = 1.88e-26;
        double clusteringFactor = 1 + Math.Sin(region.X * 5) * Math.Cos(region.Y * 5);

        return matterDensity * criticalDensity * clusteringFactor;
    }

    private double CalculateSpaceTimeCurvature(UniverseRegion region)
    {
        const double c = 3e8;
        const double G = 6.674e-11;

        double curvature = (8 * Math.PI * G / (3 * c * c)) *
                          region.EnergyDensity;

        return curvature;
    }

    public void CreateCosmicFluctuationModes()
    {
        fluctuationModes.Clear();

        for (int mode = 0; mode < 12; mode++)
        {
            double wavelength = Math.Pow(2, mode) * 0.01;
            var flucMode = new CosmicFluctuationMode(mode, wavelength)
            {
                AmplitudeModulation = Math.Pow(wavelength, -2),
                Phase = random.NextDouble() * 2 * Math.PI,
                GrowthRate = CalculateGrowthRate(wavelength)
            };

            fluctuationModes.Add(flucMode);
        }

        Console.WriteLine($"Created {fluctuationModes.Count} cosmic fluctuation modes");
    }

    private double CalculateGrowthRate(double wavelength)
    {
        double cosmicAge = 1.38e10;
        double expandionRate = hubbleConstant / (3.086e19);

        return Math.Exp(expandionRate * cosmicAge / wavelength);
    }

    public void EvolveCosmos(int timeSteps)
    {
        Console.WriteLine($"\nEvolving cosmos for {timeSteps} time steps...\n");

        for (int step = 0; step < timeSteps; step++)
        {
            cosmicTime += 0.01;

            foreach (var mode in fluctuationModes)
            {
                double evolutionFactor = Math.Exp(mode.GrowthRate * cosmicTime);
                mode.AmplitudeModulation *= (1 + 0.001 * evolutionFactor);
                mode.Phase += mode.GrowthRate * 0.01;
            }

            GenerateQuantumFluctuations();

            if (step % (timeSteps / 5) == 0)
            {
                double avgEnergy = regions.Average(r => r.EnergyDensity);
                double avgMatter = regions.Average(r => r.MatterDensity);
                double avgDarkEnergy = regions.Average(r => r.DarkEnergyDensity);

                Console.WriteLine($"Step {step}: AvgEnergy={avgEnergy:E4}, " +
                                $"Matter={avgMatter:E4}, DarkEnergy={avgDarkEnergy:E4}");
            }
        }
    }

    public Dictionary<string, object> GetCosmicMetrics()
    {
        double totalEnergyDensity = regions.Sum(r => r.EnergyDensity);
        double totalMatterDensity = regions.Sum(r => r.MatterDensity);
        double totalVacuumEnergy = regions.Sum(r => r.QuantumVacuumEnergy);
        double avgCurvature = regions.Average(r => r.CurvatureParameter);

        double omegaMatter = totalMatterDensity / (totalEnergyDensity + 1e-20);
        double omegaLambda = cosmologicalConstant / (totalEnergyDensity + 1e-20);

        return new Dictionary<string, object>
        {
            { "TotalRegions", regions.Count },
            { "CosmicTime", cosmicTime },
            { "TotalEnergyDensity", totalEnergyDensity },
            { "TotalMatterDensity", totalMatterDensity },
            { "TotalVacuumEnergy", totalVacuumEnergy },
            { "AverageSpaceTimeCurvature", avgCurvature },
            { "OmegaMatter", omegaMatter },
            { "OmegaLambda", omegaLambda },
            { "HubbleConstant", hubbleConstant }
        };
    }

    public void PrintFluctuationModeSpectrum()
    {
        Console.WriteLine("Cosmic Fluctuation Mode Spectrum:");
        Console.WriteLine("Mode | Wavelength | Amplitude | Growth Rate");
        Console.WriteLine("-----|------------|-----------|------------");

        foreach (var mode in fluctuationModes)
        {
            Console.WriteLine($"{mode.ModeId,-4} | {mode.WavelengthScale:E8} | " +
                            $"{mode.AmplitudeModulation:E8} | {mode.GrowthRate:E8}");
        }
    }

    public void PrintEnergyDensityDistribution()
    {
        Console.WriteLine("Energy Density Distribution:");

        var sortedDensities = regions.Select(r => r.EnergyDensity).OrderBy(x => x).ToList();
        double minDensity = sortedDensities.First();
        double maxDensity = sortedDensities.Last();
        double binWidth = (maxDensity - minDensity) / 10;

        for (int i = 0; i < 10; i++)
        {
            double binStart = minDensity + i * binWidth;
            double binEnd = binStart + binWidth;

            int count = sortedDensities.Count(x => x >= binStart && x < binEnd);
            string bar = new string('█', count / Math.Max(1, regions.Count / 50));
            Console.WriteLine($"  [{binStart:E3}]: {bar} ({count})");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Cosmic Quantum Flux - Universe Energy Fluctuations ===\n");

        var cosmicFlux = new CosmicQuantumFlux();

        Console.WriteLine("--- Initializing Universe Regions ---");
        cosmicFlux.InitializeUniverseRegions(8);

        Console.WriteLine("\n--- Generating Initial Quantum Fluctuations ---");
        cosmicFlux.GenerateQuantumFluctuations();

        Console.WriteLine("\n--- Creating Cosmic Fluctuation Modes ---");
        cosmicFlux.CreateCosmicFluctuationModes();

        cosmicFlux.PrintFluctuationModeSpectrum();

        Console.WriteLine("\n--- Initial Cosmic Metrics ---");
        var metricsInitial = cosmicFlux.GetCosmicMetrics();
        foreach (var kvp in metricsInitial)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Evolving Cosmos ---");
        cosmicFlux.EvolveCosmos(timeSteps: 50);

        Console.WriteLine("\n--- Final Cosmic Metrics ---");
        var metricsFinal = cosmicFlux.GetCosmicMetrics();
        foreach (var kvp in metricsFinal)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:E4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\n--- Energy Density Distribution ---");
        cosmicFlux.PrintEnergyDensityDistribution();

        Console.WriteLine("\nCosmic Quantum Flux demonstrates universe-scale quantum fluctuations.");
    }
}
