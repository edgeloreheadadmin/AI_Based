using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingWhereSomethingBeginsAndEnds
{
    public class Signal
    {
        public string SignalId { get; set; }
        public List<double> SignalData { get; set; }
        public double Baseline { get; set; }
        public double Threshold { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class Boundary
    {
        public string BoundaryId { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string BoundaryType { get; set; }
        public double StartValue { get; set; }
        public double EndValue { get; set; }
        public double BoundaryCertainty { get; set; }
        public DateTime DetectedDate { get; set; }
    }

    public class InputOutputPair
    {
        public string PairId { get; set; }
        public List<double> InputSignal { get; set; }
        public List<double> OutputSignal { get; set; }
        public int InputStartIndex { get; set; }
        public int InputEndIndex { get; set; }
        public int OutputStartIndex { get; set; }
        public int OutputEndIndex { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BoundaryAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalBoundariesDetected { get; set; }
        public List<(int, int)> BoundaryRanges { get; set; }
        public double AverageBoundaryClarity { get; set; }
        public string SignalPattern { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class BoundaryDetectionEngine
    {
        private Dictionary<string, Signal> signals;
        private Dictionary<string, Boundary> boundaries;
        private Dictionary<string, InputOutputPair> pairs;
        private Dictionary<string, BoundaryAnalysis> analyses;

        public BoundaryDetectionEngine()
        {
            signals = new Dictionary<string, Signal>();
            boundaries = new Dictionary<string, Boundary>();
            pairs = new Dictionary<string, InputOutputPair>();
            analyses = new Dictionary<string, BoundaryAnalysis>();
        }

        public void RegisterSignal(string signalId, List<double> data, double baseline, double threshold)
        {
            var signal = new Signal
            {
                SignalId = signalId,
                SignalData = new List<double>(data),
                Baseline = baseline,
                Threshold = threshold,
                RecordedDate = DateTime.Now
            };
            signals[signalId] = signal;
        }

        public void DetectBoundaries(string signalId)
        {
            if (!signals.ContainsKey(signalId)) return;

            var signal = signals[signalId];
            int boundaryCount = 0;

            for (int i = 1; i < signal.SignalData.Count; i++)
            {
                bool previousBelowThreshold = signal.SignalData[i - 1] < signal.Baseline + signal.Threshold;
                bool currentBelowThreshold = signal.SignalData[i] < signal.Baseline + signal.Threshold;

                if (previousBelowThreshold != currentBelowThreshold)
                {
                    string boundaryType = currentBelowThreshold ? "Start" : "End";
                    double certainty = Math.Abs(signal.SignalData[i] - signal.Baseline) / signal.Threshold;
                    certainty = Math.Min(certainty, 1.0);

                    var boundary = new Boundary
                    {
                        BoundaryId = $"Bound-{signalId}-{boundaryCount}",
                        StartIndex = i - 1,
                        EndIndex = i,
                        BoundaryType = boundaryType,
                        StartValue = signal.SignalData[i - 1],
                        EndValue = signal.SignalData[i],
                        BoundaryCertainty = certainty,
                        DetectedDate = DateTime.Now
                    };

                    boundaries[boundary.BoundaryId] = boundary;
                    boundaryCount++;
                }
            }
        }

        public void CreateInputOutputPair(string pairId, List<double> input, List<double> output,
                                         int inputStart, int inputEnd, int outputStart, int outputEnd)
        {
            var pair = new InputOutputPair
            {
                PairId = pairId,
                InputSignal = new List<double>(input),
                OutputSignal = new List<double>(output),
                InputStartIndex = inputStart,
                InputEndIndex = inputEnd,
                OutputStartIndex = outputStart,
                OutputEndIndex = outputEnd,
                CreatedDate = DateTime.Now
            };
            pairs[pairId] = pair;
        }

        public void AnalyzeBoundaries(string signalId)
        {
            var analysis = new BoundaryAnalysis
            {
                AnalysisId = $"Analysis-{signalId}",
                TotalBoundariesDetected = 0,
                BoundaryRanges = new List<(int, int)>(),
                AverageBoundaryClarity = 0.0,
                SignalPattern = "",
                AnalyzedDate = DateTime.Now
            };

            var signalBoundaries = boundaries.Values
                .Where(b => b.BoundaryId.Contains(signalId))
                .ToList();

            analysis.TotalBoundariesDetected = signalBoundaries.Count;

            double totalCertainty = 0.0;
            for (int i = 0; i < signalBoundaries.Count; i += 2)
            {
                if (i + 1 < signalBoundaries.Count)
                {
                    analysis.BoundaryRanges.Add((signalBoundaries[i].StartIndex, signalBoundaries[i + 1].EndIndex));
                    totalCertainty += (signalBoundaries[i].BoundaryCertainty + signalBoundaries[i + 1].BoundaryCertainty) / 2;
                }
            }

            int regionCount = analysis.BoundaryRanges.Count;
            analysis.AverageBoundaryClarity = regionCount > 0 ? totalCertainty / regionCount : 0.0;
            analysis.SignalPattern = GetSignalPattern(analysis.TotalBoundariesDetected, analysis.AverageBoundaryClarity);

            analyses[analysis.AnalysisId] = analysis;
        }

        private string GetSignalPattern(int boundaryCount, double clarity)
        {
            if (boundaryCount == 0)
                return "No boundaries detected - continuous signal";
            if (boundaryCount <= 2)
                return $"Single transition with {clarity * 100:F0}% clarity";
            if (boundaryCount <= 6)
                return $"Multiple transitions ({boundaryCount} boundaries)";
            return "Complex pattern with many boundaries";
        }

        public void DisplayBoundary(string boundaryId)
        {
            if (!boundaries.ContainsKey(boundaryId)) return;

            var boundary = boundaries[boundaryId];
            Console.WriteLine($"\n  Boundary: {boundary.BoundaryId}");
            Console.WriteLine($"  Type: {boundary.BoundaryType}");
            Console.WriteLine($"  Position: [{boundary.StartIndex}, {boundary.EndIndex}]");
            Console.WriteLine($"  Values: {boundary.StartValue:F3} → {boundary.EndValue:F3}");
            Console.WriteLine($"  Certainty: {boundary.BoundaryCertainty * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Boundary Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Boundaries: {analysis.TotalBoundariesDetected}");
            Console.WriteLine($"  Regions Identified: {analysis.BoundaryRanges.Count}");
            Console.WriteLine($"  Average Clarity: {analysis.AverageBoundaryClarity * 100:F1}%");
            Console.WriteLine($"  Pattern: {analysis.SignalPattern}");
        }

        public int GetTotalBoundaries()
        {
            return boundaries.Count;
        }

        public List<(int, int)> GetAllBoundaryRanges()
        {
            var ranges = new List<(int, int)>();
            foreach (var analysis in analyses.Values)
            {
                ranges.AddRange(analysis.BoundaryRanges);
            }
            return ranges;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Recognizing Where Something Begins and Ends by I/O           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new BoundaryDetectionEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Signals]");
        Console.ResetColor();
        Thread.Sleep(500);

        var signal1 = new List<double> { 1, 2, 2, 3, 5, 8, 12, 10, 8, 5, 3, 2, 2, 1 };
        var signal2 = new List<double> { 0.5, 0.5, 1, 2, 3, 5, 4, 3, 2, 1, 0.5, 0.5 };

        engine.RegisterSignal("SIGNAL-001", signal1, 2.0, 1.5);
        engine.RegisterSignal("SIGNAL-002", signal2, 0.5, 1.0);

        Console.WriteLine("  ✓ Registered 2 signals");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Detecting Boundaries]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DetectBoundaries("SIGNAL-001");
        engine.DetectBoundaries("SIGNAL-002");

        Console.WriteLine("  ✓ Detected boundaries in signals");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Analyzing Boundary Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeBoundaries("SIGNAL-001");
        engine.AnalyzeBoundaries("SIGNAL-002");

        Console.WriteLine("  ✓ Analyzed boundary patterns");
        engine.DisplayAnalysis("Analysis-SIGNAL-001");
        engine.DisplayAnalysis("Analysis-SIGNAL-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Input/Output Pair Definition]");
        Console.ResetColor();
        Thread.Sleep(500);

        var inputData = new List<double> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        var outputData = new List<double> { 0, 1, 4, 9, 16, 25, 36, 49, 64, 81 };

        engine.CreateInputOutputPair("PAIR-001", inputData, outputData, 0, 5, 0, 5);

        Console.WriteLine("  ✓ Created input/output mapping");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Boundary Detection Summary:");
        Console.WriteLine($"    Total Boundaries Detected: {engine.GetTotalBoundaries()}");
        var ranges = engine.GetAllBoundaryRanges();
        Console.WriteLine($"    Total Regions: {ranges.Count}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Boundary Detection Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Boundary Detection Architecture:");
        Console.WriteLine("    Layer 1: Signal Registration (input signal data)");
        Console.WriteLine("    Layer 2: Baseline Definition (establish reference level)");
        Console.WriteLine("    Layer 3: Threshold Setting (define sensitivity)");
        Console.WriteLine("    Layer 4: Transition Detection (identify crossing points)");
        Console.WriteLine("    Layer 5: Boundary Classification (start vs. end markers)");
        Console.WriteLine("    Layer 6: Certainty Calculation (measure clarity)");
        Console.WriteLine("    Layer 7: Pattern Analysis (identify signal structure)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Detect signal transitions and state changes");
        Console.WriteLine("    ✓ Identify begin and end boundaries");
        Console.WriteLine("    ✓ Measure boundary detection certainty");
        Console.WriteLine("    ✓ Link input/output relationships");
        Console.WriteLine("    ✓ Analyze signal patterns");
        Console.WriteLine("    ✓ Generate boundary statistics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Boundary detection system complete");
        Console.ResetColor();
    }
}
