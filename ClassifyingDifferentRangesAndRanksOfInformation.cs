using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ClassifyingDifferentRangesAndRanksOfInformation
{
    public class RangeBand
    {
        public string BandId { get; set; }
        public string BandName { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public string BandDescription { get; set; }
        public int ItemsInBand { get; set; }
        public double AverageValue { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class RankHierarchy
    {
        public string HierarchyId { get; set; }
        public string HierarchyName { get; set; }
        public List<string> RankLevels { get; set; }
        public Dictionary<int, string> RankDescriptions { get; set; }
        public int TotalRanks { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DataPoint
    {
        public string DataId { get; set; }
        public double Value { get; set; }
        public string ClassifiedBand { get; set; }
        public int ClassifiedRank { get; set; }
        public string RankLabel { get; set; }
        public double BandPercentile { get; set; }
        public DateTime ClassifiedDate { get; set; }
    }

    public class RangeAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalDataPoints { get; set; }
        public Dictionary<string, int> BandDistribution { get; set; }
        public Dictionary<int, int> RankDistribution { get; set; }
        public double AverageValue { get; set; }
        public double MedianValue { get; set; }
        public double StandardDeviation { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class RangeClassificationEngine
    {
        private Dictionary<string, RangeBand> rangeBands;
        private Dictionary<string, RankHierarchy> rankHierarchies;
        private Dictionary<string, DataPoint> dataPoints;
        private Dictionary<string, RangeAnalysis> analyses;

        public RangeClassificationEngine()
        {
            rangeBands = new Dictionary<string, RangeBand>();
            rankHierarchies = new Dictionary<string, RankHierarchy>();
            dataPoints = new Dictionary<string, DataPoint>();
            analyses = new Dictionary<string, RangeAnalysis>();
        }

        public void RegisterRangeBand(string bandId, string bandName, double minValue, double maxValue, string description)
        {
            var band = new RangeBand
            {
                BandId = bandId,
                BandName = bandName,
                MinValue = minValue,
                MaxValue = maxValue,
                BandDescription = description,
                ItemsInBand = 0,
                AverageValue = 0.0,
                CreatedDate = DateTime.Now
            };
            rangeBands[bandId] = band;
        }

        public void RegisterRankHierarchy(string hierarchyId, string hierarchyName, List<string> ranks)
        {
            var hierarchy = new RankHierarchy
            {
                HierarchyId = hierarchyId,
                HierarchyName = hierarchyName,
                RankLevels = new List<string>(ranks),
                RankDescriptions = new Dictionary<int, string>(),
                TotalRanks = ranks.Count,
                CreatedDate = DateTime.Now
            };

            for (int i = 0; i < ranks.Count; i++)
            {
                hierarchy.RankDescriptions[i] = ranks[i];
            }

            rankHierarchies[hierarchyId] = hierarchy;
        }

        public void ClassifyDataPoint(string dataId, double value)
        {
            var dataPoint = new DataPoint
            {
                DataId = dataId,
                Value = value,
                ClassifiedBand = "",
                ClassifiedRank = 0,
                RankLabel = "",
                BandPercentile = 0.0,
                ClassifiedDate = DateTime.Now
            };

            foreach (var band in rangeBands.Values)
            {
                if (value >= band.MinValue && value <= band.MaxValue)
                {
                    dataPoint.ClassifiedBand = band.BandId;
                    band.ItemsInBand++;
                    band.AverageValue = (band.AverageValue * (band.ItemsInBand - 1) + value) / band.ItemsInBand;
                    break;
                }
            }

            int rank = (int)((value - GetMinValue()) / (GetMaxValue() - GetMinValue()) * 10);
            rank = Math.Max(0, Math.Min(rank, 9));
            dataPoint.ClassifiedRank = rank;

            var firstHierarchy = rankHierarchies.Values.FirstOrDefault();
            if (firstHierarchy != null && firstHierarchy.RankDescriptions.ContainsKey(rank / (10 / firstHierarchy.TotalRanks)))
            {
                int hierarchyRank = rank / (10 / firstHierarchy.TotalRanks);
                dataPoint.RankLabel = firstHierarchy.RankDescriptions[hierarchyRank];
            }

            dataPoint.BandPercentile = (value - GetMinValue()) / (GetMaxValue() - GetMinValue());

            dataPoints[dataId] = dataPoint;
        }

        private double GetMinValue()
        {
            return rangeBands.Count > 0 ? rangeBands.Values.Min(b => b.MinValue) : 0.0;
        }

        private double GetMaxValue()
        {
            return rangeBands.Count > 0 ? rangeBands.Values.Max(b => b.MaxValue) : 100.0;
        }

        public void AnalyzeRanges()
        {
            var analysis = new RangeAnalysis
            {
                AnalysisId = $"Analysis-{Guid.NewGuid().ToString().Substring(0, 8)}",
                TotalDataPoints = dataPoints.Count,
                BandDistribution = new Dictionary<string, int>(),
                RankDistribution = new Dictionary<int, int>(),
                AverageValue = 0.0,
                MedianValue = 0.0,
                StandardDeviation = 0.0,
                AnalyzedDate = DateTime.Now
            };

            if (dataPoints.Count == 0) return;

            var values = dataPoints.Values.Select(d => d.Value).OrderBy(v => v).ToList();
            analysis.AverageValue = values.Average();
            analysis.MedianValue = values.Count % 2 == 0 ?
                (values[values.Count / 2 - 1] + values[values.Count / 2]) / 2 :
                values[values.Count / 2];

            double variance = values.Sum(v => Math.Pow(v - analysis.AverageValue, 2)) / values.Count;
            analysis.StandardDeviation = Math.Sqrt(variance);

            foreach (var dataPoint in dataPoints.Values)
            {
                if (!string.IsNullOrEmpty(dataPoint.ClassifiedBand))
                {
                    if (!analysis.BandDistribution.ContainsKey(dataPoint.ClassifiedBand))
                        analysis.BandDistribution[dataPoint.ClassifiedBand] = 0;
                    analysis.BandDistribution[dataPoint.ClassifiedBand]++;
                }

                if (!analysis.RankDistribution.ContainsKey(dataPoint.ClassifiedRank))
                    analysis.RankDistribution[dataPoint.ClassifiedRank] = 0;
                analysis.RankDistribution[dataPoint.ClassifiedRank]++;
            }

            analyses[analysis.AnalysisId] = analysis;
        }

        public void DisplayRangeBand(string bandId)
        {
            if (!rangeBands.ContainsKey(bandId)) return;

            var band = rangeBands[bandId];
            Console.WriteLine($"\n  Range Band: {band.BandName}");
            Console.WriteLine($"  Range: {band.MinValue:F1} - {band.MaxValue:F1}");
            Console.WriteLine($"  Description: {band.BandDescription}");
            Console.WriteLine($"  Items in Band: {band.ItemsInBand}");
            if (band.ItemsInBand > 0)
                Console.WriteLine($"  Average Value: {band.AverageValue:F3}");
        }

        public void DisplayRankHierarchy(string hierarchyId)
        {
            if (!rankHierarchies.ContainsKey(hierarchyId)) return;

            var hierarchy = rankHierarchies[hierarchyId];
            Console.WriteLine($"\n  Rank Hierarchy: {hierarchy.HierarchyName}");
            Console.WriteLine($"  Total Ranks: {hierarchy.TotalRanks}");
            Console.WriteLine($"  Rank Levels:");
            for (int i = 0; i < hierarchy.RankLevels.Count; i++)
            {
                Console.WriteLine($"    {i}: {hierarchy.RankLevels[i]}");
            }
        }

        public void DisplayAnalysis()
        {
            if (analyses.Count == 0) return;

            var analysis = analyses.Values.Last();
            Console.WriteLine($"\n  Range Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Data Points: {analysis.TotalDataPoints}");
            Console.WriteLine($"  Average Value: {analysis.AverageValue:F3}");
            Console.WriteLine($"  Median Value: {analysis.MedianValue:F3}");
            Console.WriteLine($"  Standard Deviation: {analysis.StandardDeviation:F3}");
            Console.WriteLine($"  Band Distribution: {string.Join(", ", analysis.BandDistribution.Select(x => $"{x.Key}:{x.Value}"))}");
        }

        public int GetTotalRangeBands()
        {
            return rangeBands.Count;
        }

        public int GetTotalDataPoints()
        {
            return dataPoints.Count;
        }

        public List<(string, int)> GetBandDistribution()
        {
            return rangeBands.Values
                .Select(b => (b.BandName, b.ItemsInBand))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Classifying Different Ranges and Ranks of Information         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new RangeClassificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Range Bands]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterRangeBand("BAND-001", "Very Low", 0, 20, "Minimal or critical low values");
        engine.RegisterRangeBand("BAND-002", "Low", 20, 40, "Below average range");
        engine.RegisterRangeBand("BAND-003", "Moderate", 40, 60, "Average range");
        engine.RegisterRangeBand("BAND-004", "High", 60, 80, "Above average range");
        engine.RegisterRangeBand("BAND-005", "Very High", 80, 100, "Excellent high values");

        Console.WriteLine("  ✓ Registered 5 range bands");
        engine.DisplayRangeBand("BAND-001");
        engine.DisplayRangeBand("BAND-005");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Rank Hierarchies]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterRankHierarchy("HIER-001", "Performance Ranks",
            new List<string> { "Critical", "Poor", "Fair", "Good", "Excellent" });

        Console.WriteLine("  ✓ Registered 1 rank hierarchy with 5 levels");
        engine.DisplayRankHierarchy("HIER-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Classifying Data Points into Ranges]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ClassifyDataPoint("DATA-001", 15.5);
        engine.ClassifyDataPoint("DATA-002", 35.2);
        engine.ClassifyDataPoint("DATA-003", 52.8);
        engine.ClassifyDataPoint("DATA-004", 75.3);
        engine.ClassifyDataPoint("DATA-005", 92.1);
        engine.ClassifyDataPoint("DATA-006", 48.6);
        engine.ClassifyDataPoint("DATA-007", 61.2);
        engine.ClassifyDataPoint("DATA-008", 28.9);

        Console.WriteLine("  ✓ Classified 8 data points into ranges and ranks");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Analyzing Range Distribution]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeRanges();

        var bandDistribution = engine.GetBandDistribution();
        Console.WriteLine("  Range Band Distribution:");
        foreach (var (bandName, count) in bandDistribution)
        {
            Console.WriteLine($"    {bandName}: {count} items");
        }

        engine.DisplayAnalysis();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Classification Summary:");
        Console.WriteLine($"    Total Range Bands: {engine.GetTotalRangeBands()}");
        Console.WriteLine($"    Total Data Points: {engine.GetTotalDataPoints()}");
        Console.WriteLine($"    Bands with Data: {bandDistribution.Count}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Range Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Range and Rank Classification Architecture:");
        Console.WriteLine("    Layer 1: Range Band Definition (establish value ranges)");
        Console.WriteLine("    Layer 2: Rank Hierarchy Setup (create ranking levels)");
        Console.WriteLine("    Layer 3: Data Point Classification (assign to ranges)");
        Console.WriteLine("    Layer 4: Rank Assignment (determine rank level)");
        Console.WriteLine("    Layer 5: Percentile Calculation (measure relative position)");
        Console.WriteLine("    Layer 6: Distribution Analysis (analyze spread)");
        Console.WriteLine("    Layer 7: Statistical Measurement (calculate metrics)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define multiple range bands with boundaries");
        Console.WriteLine("    ✓ Create hierarchical ranking systems");
        Console.WriteLine("    ✓ Classify data into ranges automatically");
        Console.WriteLine("    ✓ Calculate percentile positions");
        Console.WriteLine("    ✓ Analyze distribution across bands");
        Console.WriteLine("    ✓ Generate statistical summaries");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Range and rank classification system complete");
        Console.ResetColor();
    }
}
