using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingMultipleFormsOfNumericalPatterns
{
    public class NumericalPattern
    {
        public string PatternId { get; set; }
        public string PatternName { get; set; }
        public List<double> Sequence { get; set; }
        public string PatternType { get; set; }
        public double CommonRatio { get; set; }
        public double CommonDifference { get; set; }
        public double PatternStrength { get; set; }
        public DateTime IdentifiedDate { get; set; }
    }

    public class PatternType
    {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public List<string> Characteristics { get; set; }
        public string MathematicalFormula { get; set; }
        public int PatternsOfThisType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SequenceAnalysis
    {
        public string AnalysisId { get; set; }
        public string PatternId { get; set; }
        public int SequenceLength { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double MeanValue { get; set; }
        public double StandardDeviation { get; set; }
        public List<double> Differences { get; set; }
        public List<double> Ratios { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class PatternMatch
    {
        public string MatchId { get; set; }
        public string PatternId { get; set; }
        public string MatchedTypeId { get; set; }
        public double MatchConfidence { get; set; }
        public List<string> MatchedCharacteristics { get; set; }
        public string MatchQuality { get; set; }
        public DateTime MatchedDate { get; set; }
    }

    public class NumericalPatternEngine
    {
        private Dictionary<string, NumericalPattern> patterns;
        private Dictionary<string, PatternType> patternTypes;
        private Dictionary<string, SequenceAnalysis> analyses;
        private Dictionary<string, PatternMatch> matches;

        public NumericalPatternEngine()
        {
            patterns = new Dictionary<string, NumericalPattern>();
            patternTypes = new Dictionary<string, PatternType>();
            analyses = new Dictionary<string, SequenceAnalysis>();
            matches = new Dictionary<string, PatternMatch>();
        }

        public void RegisterPatternType(string typeId, string typeName, string description,
                                       List<string> characteristics, string formula)
        {
            var patternType = new PatternType
            {
                TypeId = typeId,
                TypeName = typeName,
                TypeDescription = description,
                Characteristics = new List<string>(characteristics),
                MathematicalFormula = formula,
                PatternsOfThisType = 0,
                CreatedDate = DateTime.Now
            };
            patternTypes[typeId] = patternType;
        }

        public void RegisterNumericalPattern(string patternId, string patternName, List<double> sequence)
        {
            var pattern = new NumericalPattern
            {
                PatternId = patternId,
                PatternName = patternName,
                Sequence = new List<double>(sequence),
                PatternType = "",
                CommonRatio = 0.0,
                CommonDifference = 0.0,
                PatternStrength = 0.0,
                IdentifiedDate = DateTime.Now
            };

            if (sequence.Count > 1)
            {
                pattern.CommonDifference = sequence[1] - sequence[0];
                if (sequence[0] != 0)
                {
                    pattern.CommonRatio = sequence[1] / sequence[0];
                }
            }

            patterns[patternId] = pattern;
        }

        public void AnalyzeSequence(string patternId)
        {
            if (!patterns.ContainsKey(patternId)) return;

            var pattern = patterns[patternId];
            var analysis = new SequenceAnalysis
            {
                AnalysisId = $"Analysis-{patternId}",
                PatternId = patternId,
                SequenceLength = pattern.Sequence.Count,
                MinValue = pattern.Sequence.Count > 0 ? pattern.Sequence.Min() : 0.0,
                MaxValue = pattern.Sequence.Count > 0 ? pattern.Sequence.Max() : 0.0,
                MeanValue = pattern.Sequence.Count > 0 ? pattern.Sequence.Average() : 0.0,
                StandardDeviation = 0.0,
                Differences = new List<double>(),
                Ratios = new List<double>(),
                AnalyzedDate = DateTime.Now
            };

            if (pattern.Sequence.Count > 1)
            {
                double sumSquaredDiff = 0.0;
                for (int i = 1; i < pattern.Sequence.Count; i++)
                {
                    analysis.Differences.Add(pattern.Sequence[i] - pattern.Sequence[i - 1]);
                    if (pattern.Sequence[i - 1] != 0)
                    {
                        analysis.Ratios.Add(pattern.Sequence[i] / pattern.Sequence[i - 1]);
                    }
                    sumSquaredDiff += Math.Pow(pattern.Sequence[i] - analysis.MeanValue, 2);
                }
                analysis.StandardDeviation = Math.Sqrt(sumSquaredDiff / pattern.Sequence.Count);
            }

            analyses[analysis.AnalysisId] = analysis;
        }

        public void IdentifyPatternType(string patternId)
        {
            if (!patterns.ContainsKey(patternId) || !analyses.ContainsKey($"Analysis-{patternId}"))
                return;

            var pattern = patterns[patternId];
            var analysis = analyses[$"Analysis-{patternId}"];

            string bestMatchTypeId = "";
            double bestConfidence = 0.0;
            var matchedCharacteristics = new List<string>();

            foreach (var patternType in patternTypes.Values)
            {
                double typeConfidence = 0.0;
                var typeMatches = new List<string>();

                if (patternType.TypeName == "Arithmetic" && analysis.Differences.Count > 0)
                {
                    double diffVariance = analysis.Differences.Max() - analysis.Differences.Min();
                    if (diffVariance < 0.1)
                    {
                        typeConfidence = 0.95;
                        typeMatches.Add("Constant difference");
                    }
                }

                if (patternType.TypeName == "Geometric" && analysis.Ratios.Count > 0)
                {
                    double ratioVariance = analysis.Ratios.Max() - analysis.Ratios.Min();
                    if (ratioVariance < 0.1)
                    {
                        typeConfidence = 0.95;
                        typeMatches.Add("Constant ratio");
                    }
                }

                if (patternType.TypeName == "Fibonacci" && pattern.Sequence.Count >= 3)
                {
                    bool isFibonacci = true;
                    for (int i = 2; i < pattern.Sequence.Count; i++)
                    {
                        if (Math.Abs(pattern.Sequence[i] - (pattern.Sequence[i - 1] + pattern.Sequence[i - 2])) > 0.1)
                        {
                            isFibonacci = false;
                        }
                    }
                    if (isFibonacci)
                    {
                        typeConfidence = 0.98;
                        typeMatches.Add("Fibonacci sequence");
                    }
                }

                if (patternType.TypeName == "Polynomial" && analysis.Differences.Count > 1)
                {
                    var secondDifferences = new List<double>();
                    for (int i = 1; i < analysis.Differences.Count; i++)
                    {
                        secondDifferences.Add(analysis.Differences[i] - analysis.Differences[i - 1]);
                    }
                    double secondDiffVariance = secondDifferences.Max() - secondDifferences.Min();
                    if (secondDiffVariance < 0.1)
                    {
                        typeConfidence = 0.85;
                        typeMatches.Add("Constant second differences");
                    }
                }

                if (typeConfidence > bestConfidence)
                {
                    bestConfidence = typeConfidence;
                    bestMatchTypeId = patternType.TypeId;
                    matchedCharacteristics = typeMatches;
                }
            }

            if (bestConfidence > 0)
            {
                pattern.PatternType = bestMatchTypeId;
                pattern.PatternStrength = bestConfidence;

                var match = new PatternMatch
                {
                    MatchId = $"Match-{patternId}",
                    PatternId = patternId,
                    MatchedTypeId = bestMatchTypeId,
                    MatchConfidence = bestConfidence,
                    MatchedCharacteristics = matchedCharacteristics,
                    MatchQuality = GetMatchQuality(bestConfidence),
                    MatchedDate = DateTime.Now
                };
                matches[match.MatchId] = match;

                if (patternTypes.ContainsKey(bestMatchTypeId))
                {
                    patternTypes[bestMatchTypeId].PatternsOfThisType++;
                }
            }
        }

        private string GetMatchQuality(double confidence)
        {
            return confidence switch
            {
                >= 0.95 => "Excellent - Clear pattern match",
                >= 0.85 => "Good - Strong pattern recognition",
                >= 0.75 => "Fair - Moderate pattern match",
                >= 0.6 => "Weak - Possible pattern",
                _ => "Very Weak - Uncertain match"
            };
        }

        public void DisplayPatternType(string typeId)
        {
            if (!patternTypes.ContainsKey(typeId)) return;

            var patternType = patternTypes[typeId];
            Console.WriteLine($"\n  Pattern Type: {patternType.TypeName}");
            Console.WriteLine($"  Description: {patternType.TypeDescription}");
            Console.WriteLine($"  Formula: {patternType.MathematicalFormula}");
            Console.WriteLine($"  Characteristics: {string.Join(", ", patternType.Characteristics)}");
            Console.WriteLine($"  Patterns Found: {patternType.PatternsOfThisType}");
        }

        public void DisplayPattern(string patternId)
        {
            if (!patterns.ContainsKey(patternId)) return;

            var pattern = patterns[patternId];
            Console.WriteLine($"\n  Numerical Pattern: {pattern.PatternName}");
            Console.WriteLine($"  ID: {pattern.PatternId}");
            Console.WriteLine($"  Sequence: {string.Join(", ", pattern.Sequence.Take(8))}");
            if (pattern.Sequence.Count > 8) Console.WriteLine($"    ... ({pattern.Sequence.Count} total elements)");
            Console.WriteLine($"  Type: {pattern.PatternType}");
            Console.WriteLine($"  Pattern Strength: {pattern.PatternStrength * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Sequence Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Sequence Length: {analysis.SequenceLength}");
            Console.WriteLine($"  Min Value: {analysis.MinValue:F3}");
            Console.WriteLine($"  Max Value: {analysis.MaxValue:F3}");
            Console.WriteLine($"  Mean Value: {analysis.MeanValue:F3}");
            Console.WriteLine($"  Standard Deviation: {analysis.StandardDeviation:F3}");
            if (analysis.Differences.Count > 0)
            {
                Console.WriteLine($"  Common Difference Range: [{analysis.Differences.Min():F3}, {analysis.Differences.Max():F3}]");
            }
        }

        public int GetTotalPatternTypes()
        {
            return patternTypes.Count;
        }

        public int GetTotalPatterns()
        {
            return patterns.Count;
        }

        public List<(string, int)> GetPatternTypeDistribution()
        {
            return patternTypes.Values
                .Where(pt => pt.PatternsOfThisType > 0)
                .Select(pt => (pt.TypeName, pt.PatternsOfThisType))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public double GetAveragePatternStrength()
        {
            return patterns.Count > 0 ? patterns.Values.Average(p => p.PatternStrength) : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Recognizing Multiple Forms of Numerical Patterns          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new NumericalPatternEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Pattern Types]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterPatternType("TYPE-001", "Arithmetic",
            "A sequence with constant differences between consecutive terms",
            new List<string> { "Linear growth", "Constant difference", "Predictable" },
            "a_n = a_1 + (n-1)d");

        engine.RegisterPatternType("TYPE-002", "Geometric",
            "A sequence with constant ratio between consecutive terms",
            new List<string> { "Exponential growth", "Constant ratio", "Multiplicative" },
            "a_n = a_1 * r^(n-1)");

        engine.RegisterPatternType("TYPE-003", "Fibonacci",
            "A sequence where each term is the sum of the two preceding terms",
            new List<string> { "Self-referential", "Natural growth", "Golden ratio" },
            "a_n = a_(n-1) + a_(n-2)");

        engine.RegisterPatternType("TYPE-004", "Polynomial",
            "A sequence with constant second differences",
            new List<string> { "Quadratic", "Higher order", "Accelerating growth" },
            "a_n = An^2 + Bn + C");

        engine.RegisterPatternType("TYPE-005", "Prime Numbers",
            "A sequence of numbers with exactly two divisors",
            new List<string> { "Irregular spacing", "Natural numbers", "Fundamental" },
            "Numbers divisible only by 1 and themselves");

        Console.WriteLine("  ✓ Registered 5 pattern types");
        engine.DisplayPatternType("TYPE-001");
        engine.DisplayPatternType("TYPE-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Numerical Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterNumericalPattern("PAT-001", "Simple Arithmetic",
            new List<double> { 2, 4, 6, 8, 10, 12, 14, 16 });

        engine.RegisterNumericalPattern("PAT-002", "Powers of Two",
            new List<double> { 1, 2, 4, 8, 16, 32, 64, 128 });

        engine.RegisterNumericalPattern("PAT-003", "Fibonacci Sequence",
            new List<double> { 1, 1, 2, 3, 5, 8, 13, 21, 34 });

        engine.RegisterNumericalPattern("PAT-004", "Quadratic Sequence",
            new List<double> { 1, 4, 9, 16, 25, 36, 49, 64 });

        engine.RegisterNumericalPattern("PAT-005", "Mixed Pattern",
            new List<double> { 3, 6, 9, 12, 15, 18, 21, 24 });

        Console.WriteLine("  ✓ Registered 5 numerical patterns");
        engine.DisplayPattern("PAT-001");
        engine.DisplayPattern("PAT-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Analyzing Sequence Properties]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 1; i <= 5; i++)
        {
            engine.AnalyzeSequence($"PAT-00{i}");
        }

        Console.WriteLine("  ✓ Analyzed all 5 patterns");
        engine.DisplayAnalysis("Analysis-PAT-001");
        engine.DisplayAnalysis("Analysis-PAT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Identifying Pattern Types]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 1; i <= 5; i++)
        {
            engine.IdentifyPatternType($"PAT-00{i}");
        }

        Console.WriteLine("  ✓ Identified pattern types for all sequences");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Pattern Distribution Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var distribution = engine.GetPatternTypeDistribution();
        Console.WriteLine("  Detected Pattern Types:");
        foreach (var (typeName, count) in distribution)
        {
            Console.WriteLine($"    {typeName}: {count} pattern(s)");
        }

        Console.WriteLine($"\n  Average Pattern Strength: {engine.GetAveragePatternStrength() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Pattern Recognition Summary:");
        Console.WriteLine($"    Total Pattern Types: {engine.GetTotalPatternTypes()}");
        Console.WriteLine($"    Total Patterns Analyzed: {engine.GetTotalPatterns()}");
        Console.WriteLine($"    Pattern Types Detected: {distribution.Count}");
        Console.WriteLine($"    Average Confidence: {engine.GetAveragePatternStrength() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Numerical Pattern Recognition Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Pattern Recognition Architecture:");
        Console.WriteLine("    Layer 1: Pattern Type Definition (establish classification types)");
        Console.WriteLine("    Layer 2: Sequence Registration (input numerical sequences)");
        Console.WriteLine("    Layer 3: Statistical Analysis (calculate sequence properties)");
        Console.WriteLine("    Layer 4: Difference/Ratio Calculation (identify commonalities)");
        Console.WriteLine("    Layer 5: Pattern Matching (compare to known types)");
        Console.WriteLine("    Layer 6: Confidence Scoring (measure match certainty)");
        Console.WriteLine("    Layer 7: Distribution Analysis (summarize pattern occurrences)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Recognize arithmetic progressions");
        Console.WriteLine("    ✓ Identify geometric sequences");
        Console.WriteLine("    ✓ Detect Fibonacci and self-referential patterns");
        Console.WriteLine("    ✓ Find polynomial progressions");
        Console.WriteLine("    ✓ Calculate statistical measures");
        Console.WriteLine("    ✓ Classify sequences with confidence scoring");
        Console.WriteLine("    ✓ Generate pattern distribution reports");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Numerical pattern recognition system complete");
        Console.ResetColor();
    }
}
