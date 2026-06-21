using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingMultipleFormsOfNumericalThinking
{
    public class ThinkingStyle
    {
        public string StyleId { get; set; }
        public string StyleName { get; set; }
        public string StyleDescription { get; set; }
        public List<string> CharacteristicTraits { get; set; }
        public List<string> StrengthAreas { get; set; }
        public List<string> ChallengeAreas { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CognitiveApproach
    {
        public string ApproachId { get; set; }
        public string ApproachName { get; set; }
        public string ApproachType { get; set; }
        public List<string> StepSequence { get; set; }
        public double AbstractionLevel { get; set; }
        public double SpeedMetric { get; set; }
        public int ProblemsAddressed { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ThinkingProfile
    {
        public string ProfileId { get; set; }
        public string ProfileName { get; set; }
        public Dictionary<string, double> StyleAffinities { get; set; }
        public Dictionary<string, double> ApproachPreferences { get; set; }
        public string DominantStyle { get; set; }
        public double LogicalStrength { get; set; }
        public double IntuitionStrength { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ApproachAnalysis
    {
        public string AnalysisId { get; set; }
        public string ProfileId { get; set; }
        public List<string> IdentifiedStyles { get; set; }
        public List<string> UsedApproaches { get; set; }
        public double OverallEffectiveness { get; set; }
        public string ThinkingPattern { get; set; }
        public List<string> OptimizationSuggestions { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class NumericalThinkingEngine
    {
        private Dictionary<string, ThinkingStyle> styles;
        private Dictionary<string, CognitiveApproach> approaches;
        private Dictionary<string, ThinkingProfile> profiles;
        private Dictionary<string, ApproachAnalysis> analyses;

        public NumericalThinkingEngine()
        {
            styles = new Dictionary<string, ThinkingStyle>();
            approaches = new Dictionary<string, CognitiveApproach>();
            profiles = new Dictionary<string, ThinkingProfile>();
            analyses = new Dictionary<string, ApproachAnalysis>();
        }

        public void RegisterThinkingStyle(string styleId, string styleName, string description,
                                         List<string> traits, List<string> strengths, List<string> challenges)
        {
            var style = new ThinkingStyle
            {
                StyleId = styleId,
                StyleName = styleName,
                StyleDescription = description,
                CharacteristicTraits = new List<string>(traits),
                StrengthAreas = new List<string>(strengths),
                ChallengeAreas = new List<string>(challenges),
                CreatedDate = DateTime.Now
            };
            styles[styleId] = style;
        }

        public void RegisterCognitiveApproach(string approachId, string approachName, string approachType,
                                             List<string> steps, double abstractionLevel, double speedMetric)
        {
            var approach = new CognitiveApproach
            {
                ApproachId = approachId,
                ApproachName = approachName,
                ApproachType = approachType,
                StepSequence = new List<string>(steps),
                AbstractionLevel = abstractionLevel,
                SpeedMetric = speedMetric,
                ProblemsAddressed = 0,
                CreatedDate = DateTime.Now
            };
            approaches[approachId] = approach;
        }

        public void CreateThinkingProfile(string profileId, string profileName)
        {
            var profile = new ThinkingProfile
            {
                ProfileId = profileId,
                ProfileName = profileName,
                StyleAffinities = new Dictionary<string, double>(),
                ApproachPreferences = new Dictionary<string, double>(),
                DominantStyle = "",
                LogicalStrength = 0.0,
                IntuitionStrength = 0.0,
                CreatedDate = DateTime.Now
            };
            profiles[profileId] = profile;
        }

        public void AssignStyleAffinity(string profileId, string styleId, double affinity)
        {
            if (!profiles.ContainsKey(profileId) || !styles.ContainsKey(styleId))
                return;

            var profile = profiles[profileId];
            profile.StyleAffinities[styleId] = affinity;

            if (string.IsNullOrEmpty(profile.DominantStyle) || affinity > profile.StyleAffinities.Values.Max())
            {
                profile.DominantStyle = styleId;
            }
        }

        public void AssignApproachPreference(string profileId, string approachId, double preference)
        {
            if (!profiles.ContainsKey(profileId) || !approaches.ContainsKey(approachId))
                return;

            var profile = profiles[profileId];
            profile.ApproachPreferences[approachId] = preference;

            if (approaches.ContainsKey(approachId))
            {
                approaches[approachId].ProblemsAddressed++;
            }
        }

        public void AnalyzeThinkingProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            var analysis = new ApproachAnalysis
            {
                AnalysisId = $"Analysis-{profileId}",
                ProfileId = profileId,
                IdentifiedStyles = new List<string>(),
                UsedApproaches = new List<string>(),
                OverallEffectiveness = 0.0,
                ThinkingPattern = "",
                OptimizationSuggestions = new List<string>(),
                AnalyzedDate = DateTime.Now
            };

            double totalLogical = 0.0;
            double totalIntuitive = 0.0;

            foreach (var kvp in profile.StyleAffinities)
            {
                if (kvp.Value > 0.5)
                {
                    analysis.IdentifiedStyles.Add(kvp.Key);

                    if (styles.ContainsKey(kvp.Key))
                    {
                        var style = styles[kvp.Key];
                        if (style.StyleName.Contains("Analytical") || style.StyleName.Contains("Logic"))
                        {
                            totalLogical += kvp.Value;
                        }
                        else if (style.StyleName.Contains("Intuitive") || style.StyleName.Contains("Visual"))
                        {
                            totalIntuitive += kvp.Value;
                        }
                    }
                }
            }

            foreach (var kvp in profile.ApproachPreferences)
            {
                if (kvp.Value > 0.6)
                {
                    analysis.UsedApproaches.Add(kvp.Key);
                }
            }

            profile.LogicalStrength = totalLogical / Math.Max(profile.StyleAffinities.Count, 1);
            profile.IntuitionStrength = totalIntuitive / Math.Max(profile.StyleAffinities.Count, 1);

            double avgEffectiveness = 0.0;
            if (profile.StyleAffinities.Count > 0)
            {
                avgEffectiveness = profile.StyleAffinities.Values.Average();
            }
            if (profile.ApproachPreferences.Count > 0)
            {
                avgEffectiveness = (avgEffectiveness + profile.ApproachPreferences.Values.Average()) / 2;
            }
            analysis.OverallEffectiveness = avgEffectiveness;

            analysis.ThinkingPattern = GetThinkingPattern(profile.LogicalStrength, profile.IntuitionStrength);

            if (profile.LogicalStrength > 0.7)
            {
                analysis.OptimizationSuggestions.Add("Develop more intuitive thinking approaches");
            }
            if (profile.IntuitionStrength > 0.7)
            {
                analysis.OptimizationSuggestions.Add("Strengthen logical reasoning skills");
            }
            if (analysis.UsedApproaches.Count < 3)
            {
                analysis.OptimizationSuggestions.Add("Expand cognitive approach repertoire");
            }

            analyses[analysis.AnalysisId] = analysis;
        }

        private string GetThinkingPattern(double logical, double intuitive)
        {
            if (logical > 0.7 && intuitive < 0.3)
                return "Strongly Analytical - Logic-dominated thinking";
            if (intuitive > 0.7 && logical < 0.3)
                return "Strongly Intuitive - Pattern-based thinking";
            if (Math.Abs(logical - intuitive) < 0.2)
                return "Balanced - Integrating logic and intuition";
            return "Mixed Pattern - Variable approach selection";
        }

        public void DisplayThinkingStyle(string styleId)
        {
            if (!styles.ContainsKey(styleId)) return;

            var style = styles[styleId];
            Console.WriteLine($"\n  Thinking Style: {style.StyleName}");
            Console.WriteLine($"  Description: {style.StyleDescription}");
            Console.WriteLine($"  Traits: {string.Join(", ", style.CharacteristicTraits)}");
            Console.WriteLine($"  Strengths: {string.Join(", ", style.StrengthAreas)}");
            Console.WriteLine($"  Challenges: {string.Join(", ", style.ChallengeAreas)}");
        }

        public void DisplayThinkingProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            Console.WriteLine($"\n  Thinking Profile: {profile.ProfileName}");
            Console.WriteLine($"  ID: {profile.ProfileId}");
            Console.WriteLine($"  Identified Styles: {profile.StyleAffinities.Count}");
            Console.WriteLine($"  Preferred Approaches: {profile.ApproachPreferences.Count}");
            Console.WriteLine($"  Logical Strength: {profile.LogicalStrength * 100:F1}%");
            Console.WriteLine($"  Intuition Strength: {profile.IntuitionStrength * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Thinking Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Identified Styles: {analysis.IdentifiedStyles.Count}");
            Console.WriteLine($"  Used Approaches: {analysis.UsedApproaches.Count}");
            Console.WriteLine($"  Overall Effectiveness: {analysis.OverallEffectiveness * 100:F1}%");
            Console.WriteLine($"  Thinking Pattern: {analysis.ThinkingPattern}");
            if (analysis.OptimizationSuggestions.Count > 0)
            {
                Console.WriteLine($"  Optimization Tips:");
                foreach (var suggestion in analysis.OptimizationSuggestions)
                {
                    Console.WriteLine($"    • {suggestion}");
                }
            }
        }

        public int GetTotalStyles()
        {
            return styles.Count;
        }

        public int GetTotalApproaches()
        {
            return approaches.Count;
        }

        public int GetTotalProfiles()
        {
            return profiles.Count;
        }

        public List<(string, double)> GetMostPreferredStyles()
        {
            var styleStats = new Dictionary<string, double>();
            foreach (var profile in profiles.Values)
            {
                foreach (var kvp in profile.StyleAffinities)
                {
                    if (!styleStats.ContainsKey(kvp.Key))
                    {
                        styleStats[kvp.Key] = 0;
                    }
                    styleStats[kvp.Key] += kvp.Value;
                }
            }
            return styleStats.OrderByDescending(x => x.Value).Take(5)
                .Select(x => (x.Key, x.Value / Math.Max(profiles.Count, 1)))
                .ToList();
        }

        public double GetAverageThinkingBalance()
        {
            double totalBalance = 0.0;
            int count = 0;
            foreach (var profile in profiles.Values)
            {
                double balance = 1.0 - Math.Abs(profile.LogicalStrength - profile.IntuitionStrength);
                totalBalance += balance;
                count++;
            }
            return count > 0 ? totalBalance / count : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Recognizing Multiple Forms of Numerical Thinking          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new NumericalThinkingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Thinking Styles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterThinkingStyle("STYLE-001", "Analytical-Logical",
            "Systematic, step-by-step problem solving with emphasis on proof",
            new List<string> { "Systematic", "Detail-oriented", "Logical progression" },
            new List<string> { "Complex problem solving", "Abstract reasoning", "Pattern recognition" },
            new List<string> { "Creative jumps", "Intuitive leaps", "Ambiguity" });

        engine.RegisterThinkingStyle("STYLE-002", "Intuitive-Visual",
            "Holistic perception using mental imagery and spatial reasoning",
            new List<string> { "Visual", "Holistic", "Pattern-based" },
            new List<string> { "Geometry", "Visualization", "Rapid approximation" },
            new List<string> { "Formal proof", "Detailed steps", "Linear reasoning" });

        engine.RegisterThinkingStyle("STYLE-003", "Sequential-Linear",
            "Following established procedures and standard algorithms",
            new List<string> { "Procedural", "Following rules", "Methodical" },
            new List<string> { "Computations", "Routine problems", "Verification" },
            new List<string> { "Novel problems", "Flexibility", "Abstraction" });

        engine.RegisterThinkingStyle("STYLE-004", "Conceptual-Abstract",
            "Thinking in terms of concepts, structures, and theoretical frameworks",
            new List<string> { "Theoretical", "Structural", "Conceptual" },
            new List<string> { "Theory building", "Generalization", "Model creation" },
            new List<string> { "Concrete calculations", "Practical application", "Details" });

        engine.RegisterThinkingStyle("STYLE-005", "Probabilistic-Statistical",
            "Using probability and statistical thinking for uncertain situations",
            new List<string> { "Probabilistic", "Data-driven", "Risk-aware" },
            new List<string> { "Uncertainty handling", "Data analysis", "Decision making" },
            new List<string> { "Deterministic thinking", "Absolute certainty" });

        Console.WriteLine("  ✓ Registered 5 thinking styles");
        engine.DisplayThinkingStyle("STYLE-001");
        engine.DisplayThinkingStyle("STYLE-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Cognitive Approaches]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterCognitiveApproach("APPROACH-001", "Divide and Conquer",
            "Breaking problem into smaller subproblems",
            new List<string> { "Decompose", "Solve subproblems", "Combine solutions" },
            0.75, 0.8);

        engine.RegisterCognitiveApproach("APPROACH-002", "Inductive Reasoning",
            "Building general principles from specific examples",
            new List<string> { "Examine examples", "Find pattern", "Generalize" },
            0.6, 0.7);

        engine.RegisterCognitiveApproach("APPROACH-003", "Deductive Reasoning",
            "Applying general principles to specific cases",
            new List<string> { "State axioms", "Apply rules", "Derive conclusion" },
            0.85, 0.75);

        engine.RegisterCognitiveApproach("APPROACH-004", "Visualization",
            "Using mental imagery and diagrams",
            new List<string> { "Create mental image", "Manipulate mentally", "Extract pattern" },
            0.65, 0.85);

        Console.WriteLine("  ✓ Registered 4 cognitive approaches");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Thinking Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateThinkingProfile("PROF-001", "Engineer Profile");
        engine.CreateThinkingProfile("PROF-002", "Artist Profile");
        engine.CreateThinkingProfile("PROF-003", "Scientist Profile");
        engine.CreateThinkingProfile("PROF-004", "Balanced Profile");

        Console.WriteLine("  ✓ Created 4 thinking profiles");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Assigning Style Affinities]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AssignStyleAffinity("PROF-001", "STYLE-001", 0.95);
        engine.AssignStyleAffinity("PROF-001", "STYLE-003", 0.85);
        engine.AssignStyleAffinity("PROF-001", "STYLE-004", 0.70);

        engine.AssignStyleAffinity("PROF-002", "STYLE-002", 0.95);
        engine.AssignStyleAffinity("PROF-002", "STYLE-004", 0.80);

        engine.AssignStyleAffinity("PROF-003", "STYLE-001", 0.90);
        engine.AssignStyleAffinity("PROF-003", "STYLE-004", 0.95);
        engine.AssignStyleAffinity("PROF-003", "STYLE-005", 0.85);

        engine.AssignStyleAffinity("PROF-004", "STYLE-001", 0.75);
        engine.AssignStyleAffinity("PROF-004", "STYLE-002", 0.75);
        engine.AssignStyleAffinity("PROF-004", "STYLE-004", 0.75);

        Console.WriteLine("  ✓ Assigned style affinities");
        engine.DisplayThinkingProfile("PROF-001");
        engine.DisplayThinkingProfile("PROF-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Assigning Approach Preferences]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AssignApproachPreference("PROF-001", "APPROACH-001", 0.90);
        engine.AssignApproachPreference("PROF-001", "APPROACH-003", 0.85);

        engine.AssignApproachPreference("PROF-002", "APPROACH-004", 0.95);
        engine.AssignApproachPreference("PROF-002", "APPROACH-002", 0.80);

        engine.AssignApproachPreference("PROF-003", "APPROACH-001", 0.85);
        engine.AssignApproachPreference("PROF-003", "APPROACH-003", 0.95);

        engine.AssignApproachPreference("PROF-004", "APPROACH-001", 0.70);
        engine.AssignApproachPreference("PROF-004", "APPROACH-002", 0.75);
        engine.AssignApproachPreference("PROF-004", "APPROACH-004", 0.75);

        Console.WriteLine("  ✓ Assigned approach preferences");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Thinking Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeThinkingProfile("PROF-001");
        engine.AnalyzeThinkingProfile("PROF-002");
        engine.AnalyzeThinkingProfile("PROF-003");
        engine.AnalyzeThinkingProfile("PROF-004");

        Console.WriteLine("  ✓ Analyzed all thinking profiles");
        engine.DisplayAnalysis("Analysis-PROF-001");
        engine.DisplayAnalysis("Analysis-PROF-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        var preferredStyles = engine.GetMostPreferredStyles();
        Console.WriteLine("  Most Preferred Thinking Styles:");
        int rank = 1;
        foreach (var (styleId, avgAffinity) in preferredStyles)
        {
            Console.WriteLine($"    {rank}. {styleId}: {avgAffinity * 100:F1}% average affinity");
            rank++;
        }

        Console.WriteLine($"\n  Thinking Balance Index: {engine.GetAverageThinkingBalance() * 100:F1}%");
        Console.WriteLine($"  Total Styles: {engine.GetTotalStyles()}");
        Console.WriteLine($"  Total Approaches: {engine.GetTotalApproaches()}");
        Console.WriteLine($"  Total Profiles: {engine.GetTotalProfiles()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 8: Numerical Thinking Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Thinking Style Recognition Architecture:");
        Console.WriteLine("    Layer 1: Style Definition (establish thinking modalities)");
        Console.WriteLine("    Layer 2: Approach Registration (define cognitive strategies)");
        Console.WriteLine("    Layer 3: Profile Creation (establish individual patterns)");
        Console.WriteLine("    Layer 4: Affinity Assignment (measure style preferences)");
        Console.WriteLine("    Layer 5: Approach Mapping (connect strategies to profiles)");
        Console.WriteLine("    Layer 6: Strength Calculation (measure logical vs intuitive)");
        Console.WriteLine("    Layer 7: Pattern Analysis (identify thinking characteristics)");
        Console.WriteLine("    Layer 8: Optimization Recommendations (suggest improvements)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Recognize analytical and intuitive thinking styles");
        Console.WriteLine("    ✓ Identify logical vs. spatial reasoning preferences");
        Console.WriteLine("    ✓ Map cognitive approaches to problem types");
        Console.WriteLine("    ✓ Measure thinking style balance and integration");
        Console.WriteLine("    ✓ Generate optimization recommendations");
        Console.WriteLine("    ✓ Analyze multi-dimensional thinking profiles");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Numerical thinking recognition system complete");
        Console.ResetColor();
    }
}
