using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ClassifyingEmphasisByFormalityLevel
{
    public class FormalityLevel
    {
        public string LevelId { get; set; }
        public string LevelName { get; set; }
        public string Description { get; set; }
        public double FormalityScore { get; set; }
        public List<string> VocabularyExamples { get; set; }
        public List<string> GrammaticalRules { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class EmphasisClassification
    {
        public string ClassificationId { get; set; }
        public string UtteranceText { get; set; }
        public string DetectedFormalityLevel { get; set; }
        public double FormalityConfidence { get; set; }
        public Dictionary<string, double> FormalityScores { get; set; }
        public List<string> FormalityIndicators { get; set; }
        public DateTime ClassifiedDate { get; set; }
    }

    public class SpeakerPattern
    {
        public string PatternId { get; set; }
        public string SpeakerId { get; set; }
        public Dictionary<string, int> FormalityUsageCount { get; set; }
        public string PreferredFormality { get; set; }
        public double FormalltyConsistency { get; set; }
        public int TotalUtterances { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class FormalityAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalClassified { get; set; }
        public Dictionary<string, int> FormalityDistribution { get; set; }
        public string MostCommonFormality { get; set; }
        public double AverageFormalityScore { get; set; }
        public List<string> TransitionPatterns { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class FormalityClassifier
    {
        private Dictionary<string, FormalityLevel> levels;
        private Dictionary<string, EmphasisClassification> classifications;
        private Dictionary<string, SpeakerPattern> patterns;
        private Dictionary<string, FormalityAnalysis> analyses;

        public FormalityClassifier()
        {
            levels = new Dictionary<string, FormalityLevel>();
            classifications = new Dictionary<string, EmphasisClassification>();
            patterns = new Dictionary<string, SpeakerPattern>();
            analyses = new Dictionary<string, FormalityAnalysis>();
        }

        public void RegisterFormalityLevel(string levelId, string levelName, string description,
                                         double formalityScore, List<string> vocabulary, List<string> rules)
        {
            var level = new FormalityLevel
            {
                LevelId = levelId,
                LevelName = levelName,
                Description = description,
                FormalityScore = formalityScore,
                VocabularyExamples = new List<string>(vocabulary),
                GrammaticalRules = new List<string>(rules),
                CreatedDate = DateTime.Now
            };
            levels[levelId] = level;
        }

        public void ClassifyEmphasis(string classificationId, string utterance)
        {
            var classification = new EmphasisClassification
            {
                ClassificationId = classificationId,
                UtteranceText = utterance,
                DetectedFormalityLevel = "",
                FormalityConfidence = 0.0,
                FormalityScores = new Dictionary<string, double>(),
                FormalityIndicators = new List<string>(),
                ClassifiedDate = DateTime.Now
            };

            foreach (var level in levels.Values)
            {
                double score = CalculateFormalityScore(utterance, level.LevelId);
                classification.FormalityScores[level.LevelName] = score;
            }

            if (classification.FormalityScores.Count > 0)
            {
                var highest = classification.FormalityScores.OrderByDescending(x => x.Value).First();
                classification.DetectedFormalityLevel = highest.Key;
                classification.FormalityConfidence = highest.Value;
            }

            classification.FormalityIndicators = ExtractFormalityIndicators(utterance);

            classifications[classificationId] = classification;
        }

        private double CalculateFormalityScore(string text, string levelId)
        {
            if (!levels.ContainsKey(levelId)) return 0.0;

            var level = levels[levelId];
            double score = 0.0;
            int matches = 0;

            foreach (var vocab in level.VocabularyExamples)
            {
                if (text.Contains(vocab, StringComparison.OrdinalIgnoreCase))
                {
                    matches++;
                }
            }

            score = Math.Min((double)matches / level.VocabularyExamples.Count, 1.0);
            score = score * 0.7 + level.FormalityScore * 0.3;

            return Math.Min(score, 1.0);
        }

        private List<string> ExtractFormalityIndicators(string text)
        {
            var indicators = new List<string>();

            if (text.Contains("cannot") || text.Contains("shall"))
                indicators.Add("Formal contractions");

            if (text.Contains("gonna") || text.Contains("wanna"))
                indicators.Add("Casual contractions");

            if (text.Contains("!") || text.Contains("?"))
                indicators.Add("Emotional punctuation");

            if (text.Length > 50)
                indicators.Add("Complex sentence structure");

            return indicators;
        }

        public void CreateSpeakerPattern(string patternId, string speakerId, List<string> levelIds)
        {
            var pattern = new SpeakerPattern
            {
                PatternId = patternId,
                SpeakerId = speakerId,
                FormalityUsageCount = new Dictionary<string, int>(),
                PreferredFormality = "",
                FormalltyConsistency = 0.0,
                TotalUtterances = levelIds.Count,
                CreatedDate = DateTime.Now
            };

            foreach (var levelId in levelIds)
            {
                if (levels.ContainsKey(levelId))
                {
                    var level = levels[levelId];
                    if (!pattern.FormalityUsageCount.ContainsKey(level.LevelName))
                        pattern.FormalityUsageCount[level.LevelName] = 0;
                    pattern.FormalityUsageCount[level.LevelName]++;
                }
            }

            if (pattern.FormalityUsageCount.Count > 0)
            {
                pattern.PreferredFormality = pattern.FormalityUsageCount.OrderByDescending(x => x.Value).First().Key;
            }

            pattern.FormalltyConsistency = CalculateConsistency(pattern.FormalityUsageCount);

            patterns[patternId] = pattern;
        }

        private double CalculateConsistency(Dictionary<string, int> counts)
        {
            if (counts.Count == 0) return 0.0;
            int max = counts.Values.Max();
            int total = counts.Values.Sum();
            return (double)max / total;
        }

        public void AnalyzeFormalityPatterns(string analysisId)
        {
            var analysis = new FormalityAnalysis
            {
                AnalysisId = analysisId,
                TotalClassified = classifications.Count,
                FormalityDistribution = new Dictionary<string, int>(),
                MostCommonFormality = "",
                AverageFormalityScore = 0.0,
                TransitionPatterns = new List<string>(),
                AnalyzedDate = DateTime.Now
            };

            foreach (var classification in classifications.Values)
            {
                if (!string.IsNullOrEmpty(classification.DetectedFormalityLevel))
                {
                    if (!analysis.FormalityDistribution.ContainsKey(classification.DetectedFormalityLevel))
                        analysis.FormalityDistribution[classification.DetectedFormalityLevel] = 0;
                    analysis.FormalityDistribution[classification.DetectedFormalityLevel]++;
                }
            }

            if (analysis.FormalityDistribution.Count > 0)
            {
                analysis.MostCommonFormality = analysis.FormalityDistribution.OrderByDescending(x => x.Value).First().Key;
            }

            analysis.AverageFormalityScore = classifications.Count > 0 ?
                classifications.Values.Average(c => c.FormalityConfidence) : 0.0;

            analyses[analysisId] = analysis;
        }

        public void DisplayFormalityLevel(string levelId)
        {
            if (!levels.ContainsKey(levelId)) return;

            var level = levels[levelId];
            Console.WriteLine($"\n  Formality Level: {level.LevelName}");
            Console.WriteLine($"  Description: {level.Description}");
            Console.WriteLine($"  Formality Score: {level.FormalityScore * 100:F0}%");
            Console.WriteLine($"  Example Vocabulary: {string.Join(", ", level.VocabularyExamples.Take(3))}");
        }

        public void DisplayClassification(string classificationId)
        {
            if (!classifications.ContainsKey(classificationId)) return;

            var classification = classifications[classificationId];
            Console.WriteLine($"\n  Classification: {classification.ClassificationId}");
            Console.WriteLine($"  Utterance: {classification.UtteranceText}");
            Console.WriteLine($"  Detected Formality: {classification.DetectedFormalityLevel}");
            Console.WriteLine($"  Confidence: {classification.FormalityConfidence * 100:F1}%");
            Console.WriteLine($"  Indicators: {string.Join(", ", classification.FormalityIndicators)}");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Formality Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Classified: {analysis.TotalClassified}");
            Console.WriteLine($"  Most Common: {analysis.MostCommonFormality}");
            Console.WriteLine($"  Average Confidence: {analysis.AverageFormalityScore * 100:F1}%");
        }

        public int GetTotalLevels()
        {
            return levels.Count;
        }

        public int GetTotalClassifications()
        {
            return classifications.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Classifying Emphasis by Casual, Formal, or Proper Speaking  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var classifier = new FormalityClassifier();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Formality Levels]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.RegisterFormalityLevel("FORM-001", "Casual",
            "Informal, relaxed communication with colloquialisms",
            0.30,
            new List<string> { "hey", "gonna", "wanna", "cool", "awesome" },
            new List<string> { "Contractions permitted", "Informal vocabulary" });

        classifier.RegisterFormalityLevel("FORM-002", "Formal",
            "Professional communication with standard vocabulary",
            0.70,
            new List<string> { "please", "furthermore", "accordingly", "therefore", "facilitate" },
            new List<string> { "Complete sentences", "Standard vocabulary" });

        classifier.RegisterFormalityLevel("FORM-003", "Proper",
            "Highly formal communication with strict grammar",
            0.95,
            new List<string> { "sir", "madam", "hereby", "whereas", "hereto" },
            new List<string> { "Perfect grammar", "Archaic or formal vocabulary" });

        Console.WriteLine("  ✓ Registered 3 formality levels");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Formality Characteristics]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.DisplayFormalityLevel("FORM-001");
        classifier.DisplayFormalityLevel("FORM-002");
        classifier.DisplayFormalityLevel("FORM-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Classifying Emphasis by Formality]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.ClassifyEmphasis("CLASS-001", "Hey, this is gonna be awesome!");
        classifier.ClassifyEmphasis("CLASS-002", "I would like to inform you of this development.");
        classifier.ClassifyEmphasis("CLASS-003", "Hereby we declare this matter resolved accordingly.");
        classifier.ClassifyEmphasis("CLASS-004", "You know what, that's really cool");
        classifier.ClassifyEmphasis("CLASS-005", "Furthermore, the data suggests significant improvement.");

        Console.WriteLine("  ✓ Classified 5 utterances by formality level");
        classifier.DisplayClassification("CLASS-001");
        classifier.DisplayClassification("CLASS-003");
        classifier.DisplayClassification("CLASS-005");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Creating Speaker Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.CreateSpeakerPattern("PATTERN-001", "Casual Speaker",
            new List<string> { "FORM-001", "FORM-001", "FORM-001", "FORM-002", "FORM-001" });

        classifier.CreateSpeakerPattern("PATTERN-002", "Professional Speaker",
            new List<string> { "FORM-002", "FORM-002", "FORM-002", "FORM-003", "FORM-002" });

        Console.WriteLine("  ✓ Created 2 speaker patterns");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Formality Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.AnalyzeFormalityPatterns("ANALYSIS-001");

        Console.WriteLine("  ✓ Analyzed formality distribution patterns");
        classifier.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Formality Classification Summary:");
        Console.WriteLine($"    Registered Levels: {classifier.GetTotalLevels()}");
        Console.WriteLine($"    Classified Utterances: {classifier.GetTotalClassifications()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Formality Level Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Formality Classification Architecture:");
        Console.WriteLine("    Layer 1: Formality Level Definition (establish tiers)");
        Console.WriteLine("    Layer 2: Vocabulary Mapping (associate words with levels)");
        Console.WriteLine("    Layer 3: Grammar Rules (define grammatical patterns)");
        Console.WriteLine("    Layer 4: Utterance Analysis (evaluate formality score)");
        Console.WriteLine("    Layer 5: Indicator Extraction (identify formality cues)");
        Console.WriteLine("    Layer 6: Confidence Calculation (measure certainty)");
        Console.WriteLine("    Layer 7: Pattern Analysis (track speaker preferences)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define multiple formality levels");
        Console.WriteLine("    ✓ Classify utterances by formality");
        Console.WriteLine("    ✓ Extract formality indicators");
        Console.WriteLine("    ✓ Track speaker formality patterns");
        Console.WriteLine("    ✓ Measure consistency of formality");
        Console.WriteLine("    ✓ Analyze formality distribution");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Formality classification system complete");
        Console.ResetColor();
    }
}
