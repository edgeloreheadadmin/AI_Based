using System;
using System.Collections.Generic;
using System.Linq;

namespace BiorhythmDecisionSystem
{
    /// <summary>
    /// Different cognitive thinking patterns and mental processing styles
    /// </summary>
    public enum ThinkingStyle
    {
        AnalyticalSequential,    // Step-by-step logical processing
        HolisticSynthetic,       // Big picture, pattern recognition
        IntuitiveFast,           // Rapid subconscious processing
        DetailOrientedPrecise,   // Meticulous, exacting
        CreativeAssociative,     // Novel connections, divergent thinking
        SystemicStructured,      // Organized frameworks, formal logic
        AdaptiveFluid,           // Flexible, responsive to changes
        AbstractConceptual,      // Theory-based, principles-focused
        ConcreteExperiential,    // Practical, hands-on, real-world focused
        IntegrativeSynthesis     // Combining multiple perspectives
    }

    /// <summary>
    /// Cognitive state describing current mental processing capacity
    /// </summary>
    public class CognitiveState
    {
        public ThinkingStyle PrimaryStyle { get; set; }
        public ThinkingStyle SecondaryStyle { get; set; }
        public double MentalClarity { get; set; } // 0-1: how clear/sharp thinking is
        public double FocusIntensity { get; set; } // 0-1: depth of concentration
        public double CreativeCapacity { get; set; } // 0-1: ability for novel ideas
        public double LogicalCapacity { get; set; } // 0-1: ability for analysis
        public double ProcessingSpeed { get; set; } // 0-1: mental quickness
        public double IntuitivePower { get; set; } // 0-1: subconscious insight access
        public bool InFlowState { get; set; } // Is in optimal mental flow
        public int MinutesUntilCognitiveShift { get; set; } // When thinking style changes
        public List<string> OptimalTasks { get; set; }
        public List<string> SuboptimalTasks { get; set; }
        public string CognitiveQualityLevel { get; set; } // Poor, Acceptable, Good, Excellent, Peak
    }

    /// <summary>
    /// Maps thinking styles to their optimal characteristics and capabilities
    /// </summary>
    public class ThinkingStyleProfile
    {
        public ThinkingStyle Style { get; set; }
        public string Description { get; set; }
        public List<string> Strengths { get; set; }
        public List<string> Weaknesses { get; set; }
        public List<string> OptimalFor { get; set; }
        public List<string> SuboptimalFor { get; set; }
        public Dictionary<string, double> ProcessCapabilities { get; set; }
    }

    /// <summary>
    /// Analyzes cognitive state from biorhythm and mindset patterns
    /// </summary>
    public class CognitiveStateAnalyzer
    {
        private readonly BiorhythmCalculator _calculator;
        private readonly MindsetAlignmentAnalyzer _mindsetAnalyzer;

        public CognitiveStateAnalyzer(DateTime birthDate)
        {
            _calculator = new BiorhythmCalculator(birthDate);
            _mindsetAnalyzer = new MindsetAlignmentAnalyzer(birthDate);
        }

        /// <summary>
        /// Analyze current cognitive state and thinking patterns
        /// </summary>
        public CognitiveState AnalyzeCognitiveState(DateTime targetDate)
        {
            var profile = _calculator.GetProfile(targetDate);
            var mindset = _mindsetAnalyzer.AnalyzeMindset(targetDate);

            var primaryStyle = DeterminePrimaryThinkingStyle(profile, mindset);
            var secondaryStyle = DetermineSecondaryThinkingStyle(profile, mindset, primaryStyle);

            var cognitiveState = new CognitiveState
            {
                PrimaryStyle = primaryStyle,
                SecondaryStyle = secondaryStyle,
                MentalClarity = CalculateMentalClarity(profile, mindset),
                FocusIntensity = CalculateFocusIntensity(profile, mindset),
                CreativeCapacity = CalculateCreativeCapacity(profile, mindset),
                LogicalCapacity = CalculateLogicalCapacity(profile, mindset),
                ProcessingSpeed = CalculateProcessingSpeed(profile, mindset),
                IntuitivePower = CalculateIntuitivePower(profile, mindset),
                InFlowState = DetectFlowState(profile, mindset),
                MinutesUntilCognitiveShift = CalculateTimeToShift(profile),
                OptimalTasks = GetOptimalTasks(primaryStyle, secondaryStyle, profile),
                SuboptimalTasks = GetSuboptimalTasks(primaryStyle, secondaryStyle),
                CognitiveQualityLevel = AssessCognitiveQuality(profile, mindset)
            };

            return cognitiveState;
        }

        private ThinkingStyle DeterminePrimaryThinkingStyle(BiorhythmProfile profile, MindsetState mindset)
        {
            double phys = profile.Physical.Value;
            double emot = profile.Emotional.Value;
            double intel = profile.Intellectual.Value;

            // Match mindset to thinking style
            return mindset.Type switch
            {
                MindsetType.AnalyticalLogical => ThinkingStyle.AnalyticalSequential,
                MindsetType.CreativeExpressive => ThinkingStyle.CreativeAssociative,
                MindsetType.ActionOriented => ThinkingStyle.ConcreteExperiential,
                MindsetType.EmotionalIntuitive => ThinkingStyle.IntuitiveFast,
                MindsetType.BalancedHarmonious => ThinkingStyle.IntegrativeSynthesis,
                MindsetType.LowEnergyReflective => ThinkingStyle.AbstractConceptual,
                MindsetType.StressedFractured => ThinkingStyle.AdaptiveFluid,
                MindsetType.FocusedDetermined => ThinkingStyle.DetailOrientedPrecise,
                MindsetType.PlayfulSpontaneous => ThinkingStyle.HolisticSynthetic,
                MindsetType.CautiousConservative => ThinkingStyle.SystemicStructured,
                _ => ThinkingStyle.AnalyticalSequential
            };
        }

        private ThinkingStyle DetermineSecondaryThinkingStyle(BiorhythmProfile profile, MindsetState mindset, ThinkingStyle primary)
        {
            double intel = profile.Intellectual.Value;
            double emot = profile.Emotional.Value;

            // Secondary style based on cycle balance
            if (Math.Abs(intel - emot) < 0.2)
                return ThinkingStyle.IntegrativeSynthesis;

            if (intel > emot)
                return ThinkingStyle.SystemicStructured;

            if (emot > intel && primary != ThinkingStyle.CreativeAssociative)
                return ThinkingStyle.IntuitiveFast;

            return Math.Abs(profile.Physical.Value) > 0.5
                ? ThinkingStyle.ConcreteExperiential
                : ThinkingStyle.AbstractConceptual;
        }

        private double CalculateMentalClarity(BiorhythmProfile profile, MindsetState mindset)
        {
            // Clarity based on stability and intellectual cycle
            double intellectualFactor = (profile.Intellectual.Value + 1) / 2;
            double stabilityFactor = mindset.Stability;
            double criticalPenalty = profile.Intellectual.Phase == CyclePhase.Critical ? 0.3 : 0.0;

            return Math.Max(0.1, (intellectualFactor * 0.6 + stabilityFactor * 0.4) - criticalPenalty);
        }

        private double CalculateFocusIntensity(BiorhythmProfile profile, MindsetState mindset)
        {
            // Focus based on emotional stability (low emotional variation = high focus)
            double emotionalStability = 1.0 - Math.Abs(profile.Emotional.Value);
            double intellectualPower = (profile.Intellectual.Value + 1) / 2;
            double physicalSupport = Math.Max(0, profile.Physical.Value) / 2;

            return (emotionalStability * 0.4 + intellectualPower * 0.4 + physicalSupport * 0.2);
        }

        private double CalculateCreativeCapacity(BiorhythmProfile profile, MindsetState mindset)
        {
            // Creativity peak during high emotional and moderate intellectual
            double emotionalPower = (profile.Emotional.Value + 1) / 2;
            double intellectualBalance = 1.0 - Math.Abs(profile.Intellectual.Value - profile.Emotional.Value) / 2;

            return (emotionalPower * 0.6 + intellectualBalance * 0.4);
        }

        private double CalculateLogicalCapacity(BiorhythmProfile profile, MindsetState mindset)
        {
            // Logic based on high intellectual and low emotional interference
            double intellectualPower = (profile.Intellectual.Value + 1) / 2;
            double emotionalCalmness = Math.Max(0, 1.0 - Math.Abs(profile.Emotional.Value));

            return (intellectualPower * 0.7 + emotionalCalmness * 0.3);
        }

        private double CalculateProcessingSpeed(BiorhythmProfile profile, MindsetState mindset)
        {
            // Speed based on physical cycle and mental clarity
            double physicalVitality = (profile.Physical.Value + 1) / 2;
            double mentalClarity = CalculateMentalClarity(profile, mindset);

            return (physicalVitality * 0.4 + mentalClarity * 0.6);
        }

        private double CalculateIntuitivePower(BiorhythmProfile profile, MindsetState mindset)
        {
            // Intuition peak during high emotional cycle
            double emotionalPower = (profile.Emotional.Value + 1) / 2;
            double subconsciousPower = mindset.Stability * 0.5;

            return (emotionalPower * 0.6 + subconsciousPower * 0.4);
        }

        private bool DetectFlowState(BiorhythmProfile profile, MindsetState mindset)
        {
            // Flow state: high intensity, high stability, balanced cycles, focused mindset
            bool highIntensity = mindset.Intensity > 0.75;
            bool highStability = mindset.Stability > 0.75;
            bool balancedCycles = Math.Abs(profile.Physical.Value - profile.Intellectual.Value) < 0.3 &&
                                 Math.Abs(profile.Emotional.Value - profile.Intellectual.Value) < 0.3;
            bool focusedMindset = mindset.Type == MindsetType.FocusedDetermined ||
                                 mindset.Type == MindsetType.BalancedHarmonious ||
                                 mindset.Type == MindsetType.CreativeExpressive;

            return highIntensity && highStability && balancedCycles && focusedMindset;
        }

        private int CalculateTimeToShift(BiorhythmProfile profile)
        {
            // Estimate minutes until cognitive shift (when cycle phases change)
            double cycleDistances = new[]
            {
                Math.Abs(profile.Physical.Value),
                Math.Abs(profile.Emotional.Value),
                Math.Abs(profile.Intellectual.Value)
            }.Min();

            // Rough estimate: closer to zero crossing = sooner shift
            if (cycleDistances < 0.1)
                return 60; // ~1 hour
            if (cycleDistances < 0.3)
                return 180; // ~3 hours
            if (cycleDistances < 0.6)
                return 480; // ~8 hours

            return 1440; // ~24 hours
        }

        private List<string> GetOptimalTasks(ThinkingStyle primary, ThinkingStyle secondary, BiorhythmProfile profile)
        {
            var profiles = GetThinkingStyleProfiles();

            var primaryTasks = profiles[primary].OptimalFor.Take(3).ToList();
            var secondaryTasks = profiles[secondary].OptimalFor.Take(2).ToList();

            return primaryTasks.Union(secondaryTasks).ToList();
        }

        private List<string> GetSuboptimalTasks(ThinkingStyle primary, ThinkingStyle secondary)
        {
            var profiles = GetThinkingStyleProfiles();

            var primarySuboptimal = profiles[primary].SuboptimalFor.Take(2).ToList();
            var secondarySuboptimal = profiles[secondary].SuboptimalFor.Take(2).ToList();

            return primarySuboptimal.Union(secondarySuboptimal).ToList();
        }

        private string AssessCognitiveQuality(BiorhythmProfile profile, MindsetState mindset)
        {
            double overallScore = (CalculateMentalClarity(profile, mindset) +
                                 CalculateFocusIntensity(profile, mindset) +
                                 mindset.Stability) / 3;

            return overallScore switch
            {
                > 0.85 => "Peak",
                > 0.70 => "Excellent",
                > 0.55 => "Good",
                > 0.40 => "Acceptable",
                _ => "Poor"
            };
        }

        private Dictionary<ThinkingStyle, ThinkingStyleProfile> GetThinkingStyleProfiles()
        {
            return new Dictionary<ThinkingStyle, ThinkingStyleProfile>
            {
                {
                    ThinkingStyle.AnalyticalSequential, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.AnalyticalSequential,
                        Description = "Step-by-step logical deduction",
                        Strengths = new List<string> { "Structured reasoning", "Root cause analysis", "Systematic problem-solving" },
                        Weaknesses = new List<string> { "Can miss big picture", "May be slow", "Overlooks intuition" },
                        OptimalFor = new List<string> { "Debugging", "Math", "Legal analysis", "Strategy", "Process design", "Technical documentation" },
                        SuboptimalFor = new List<string> { "Brainstorming", "Rapid decisions", "Creative work" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.95 }, { "speed", 0.6 }, { "creativity", 0.4 } }
                    }
                },
                {
                    ThinkingStyle.HolisticSynthetic, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.HolisticSynthetic,
                        Description = "Pattern recognition and big-picture thinking",
                        Strengths = new List<string> { "Pattern recognition", "Systems thinking", "Strategic insight" },
                        Weaknesses = new List<string> { "Misses details", "Vague conclusions", "Lacks precision" },
                        OptimalFor = new List<string> { "Organizational strategy", "Market analysis", "Project overview", "Vision setting", "Trend forecasting" },
                        SuboptimalFor = new List<string> { "Detail work", "Quality assurance", "Fine-tuning" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.6 }, { "speed", 0.8 }, { "creativity", 0.85 } }
                    }
                },
                {
                    ThinkingStyle.IntuitiveFast, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.IntuitiveFast,
                        Description = "Rapid subconscious pattern matching",
                        Strengths = new List<string> { "Quick decisions", "Gut accuracy", "Subconscious insight" },
                        Weaknesses = new List<string> { "Hard to justify", "May be biased", "Lacks rigor" },
                        OptimalFor = new List<string> { "Rapid decisions", "People reading", "Negotiation", "Risk assessment", "Crisis management" },
                        SuboptimalFor = new List<string> { "Complex analysis", "Documentation", "Step-by-step work" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.5 }, { "speed", 0.95 }, { "creativity", 0.7 } }
                    }
                },
                {
                    ThinkingStyle.DetailOrientedPrecise, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.DetailOrientedPrecise,
                        Description = "Meticulous examination and exactness",
                        Strengths = new List<string> { "Precision", "Quality control", "Error detection" },
                        Weaknesses = new List<string> { "Perfectionism", "Slow progress", "May miss deadline" },
                        OptimalFor = new List<string> { "QA testing", "Editing", "Audit", "Fine-tuning", "Specification writing" },
                        SuboptimalFor = new List<string> { "Fast iteration", "Creative exploration", "Strategic planning" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.85 }, { "speed", 0.4 }, { "creativity", 0.3 } }
                    }
                },
                {
                    ThinkingStyle.CreativeAssociative, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.CreativeAssociative,
                        Description = "Novel associations and divergent thinking",
                        Strengths = new List<string> { "Innovation", "Novel solutions", "Originality" },
                        Weaknesses = new List<string> { "Impractical ideas", "Unfocused", "Lacks followthrough" },
                        OptimalFor = new List<string> { "Ideation", "Art", "Innovation", "Content creation", "Marketing" },
                        SuboptimalFor = new List<string> { "Execution", "Analysis", "Detail work" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.4 }, { "speed", 0.7 }, { "creativity", 0.98 } }
                    }
                },
                {
                    ThinkingStyle.SystemicStructured, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.SystemicStructured,
                        Description = "Organized frameworks and formal logic",
                        Strengths = new List<string> { "Structure", "Organization", "Formal reasoning" },
                        Weaknesses = new List<string> { "Rigidity", "Slow adaptation", "Overthinking" },
                        OptimalFor = new List<string> { "Architecture", "Planning", "Policy", "Risk management", "Process documentation" },
                        SuboptimalFor = new List<string> { "Rapid changes", "Improvisation", "Artistic work" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.9 }, { "speed", 0.5 }, { "creativity", 0.35 } }
                    }
                },
                {
                    ThinkingStyle.AdaptiveFluid, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.AdaptiveFluid,
                        Description = "Flexible, responsive to changing conditions",
                        Strengths = new List<string> { "Adaptability", "Problem-pivoting", "Crisis response" },
                        Weaknesses = new List<string> { "Inconsistency", "Scattered thinking", "Lack direction" },
                        OptimalFor = new List<string> { "Crisis management", "Improvisation", "Change management", "Troubleshooting" },
                        SuboptimalFor = new List<string> { "Long-term planning", "Consistency", "Precision work" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.55 }, { "speed", 0.8 }, { "creativity", 0.65 } }
                    }
                },
                {
                    ThinkingStyle.AbstractConceptual, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.AbstractConceptual,
                        Description = "Theory-based and principle-focused thinking",
                        Strengths = new List<string> { "Theory building", "Conceptual clarity", "Abstraction" },
                        Weaknesses = new List<string> { "Disconnected from reality", "Hard to implement", "Impractical" },
                        OptimalFor = new List<string> { "Philosophy", "Research", "Theory development", "Academic work", "Conceptualization" },
                        SuboptimalFor = new List<string> { "Practical execution", "Real-world application", "Action items" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.8 }, { "speed", 0.5 }, { "creativity", 0.75 } }
                    }
                },
                {
                    ThinkingStyle.ConcreteExperiential, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.ConcreteExperiential,
                        Description = "Practical, hands-on, real-world focused",
                        Strengths = new List<string> { "Practical wisdom", "Real-world focus", "Experiential learning" },
                        Weaknesses = new List<string> { "Limited theory", "Difficulty generalizing", "Short-term focus" },
                        OptimalFor = new List<string> { "Implementation", "Hands-on work", "Training", "Mentoring", "Execution" },
                        SuboptimalFor = new List<string> { "Theory work", "Abstraction", "Strategic planning" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.6 }, { "speed", 0.75 }, { "creativity", 0.5 } }
                    }
                },
                {
                    ThinkingStyle.IntegrativeSynthesis, new ThinkingStyleProfile
                    {
                        Style = ThinkingStyle.IntegrativeSynthesis,
                        Description = "Combining multiple perspectives and approaches",
                        Strengths = new List<string> { "Integration", "Holistic solutions", "Consensus building" },
                        Weaknesses = new List<string> { "Can be compromising", "Time-consuming", "Complex to explain" },
                        OptimalFor = new List<string> { "Strategic decisions", "Cross-functional work", "Consensus", "Complexity management", "Innovation" },
                        SuboptimalFor = new List<string> { "Speed", "Simplicity", "Single perspective" },
                        ProcessCapabilities = new Dictionary<string, double> { { "logic", 0.75 }, { "speed", 0.6 }, { "creativity", 0.8 } }
                    }
                }
            };
        }

        public ThinkingStyleProfile GetThinkingStyleInfo(ThinkingStyle style)
        {
            var profiles = GetThinkingStyleProfiles();
            return profiles.ContainsKey(style) ? profiles[style] : profiles[ThinkingStyle.AnalyticalSequential];
        }
    }

    /// <summary>
    /// Aligns decisions with optimal thinking styles and cognitive processes
    /// </summary>
    public class ThoughtProcessAlignmentMaker
    {
        private readonly BiorhythmicDecisionEngine _decisionEngine;
        private readonly MindsetAlignedDecisionMaker _mindsetMaker;
        private readonly CognitiveStateAnalyzer _cognitiveAnalyzer;

        public ThoughtProcessAlignmentMaker(DateTime birthDate)
        {
            _decisionEngine = new BiorhythmicDecisionEngine(birthDate);
            _mindsetMaker = new MindsetAlignedDecisionMaker(birthDate);
            _cognitiveAnalyzer = new CognitiveStateAnalyzer(birthDate);
        }

        /// <summary>
        /// Get decision recommendation aligned with current thought process capability
        /// </summary>
        public (DecisionRecommendation decision, MindsetState mindset, CognitiveState cognition)
            GetThoughtAlignedRecommendation(string decisionType, DateTime targetDate)
        {
            var cognitive = _cognitiveAnalyzer.AnalyzeCognitiveState(targetDate);
            var (baseDecision, mindset) = _mindsetMaker.GetMindsetAlignedRecommendation(decisionType, targetDate);

            // Further adjust based on cognitive state
            double cognitiveAdjustment = CalculateCognitiveAlignment(decisionType, cognitive);
            baseDecision.RecommendationScore = baseDecision.RecommendationScore * (0.8 + (0.2 * cognitiveAdjustment));

            // Add cognitive insights
            var cognitiveInsights = GenerateCognitiveInsights(decisionType, cognitive);
            baseDecision.Warnings.InsertRange(0, cognitiveInsights);

            return (baseDecision, mindset, cognitive);
        }

        /// <summary>
        /// Get tasks optimized for current thinking style
        /// </summary>
        public List<(string task, double suitability, ThinkingStyle requiredStyle)>
            GetCognitiveOptimalTasks(DateTime targetDate, int taskCount = 10)
        {
            var cognitive = _cognitiveAnalyzer.AnalyzeCognitiveState(targetDate);
            var profile1 = _cognitiveAnalyzer.GetThinkingStyleInfo(cognitive.PrimaryStyle);
            var profile2 = _cognitiveAnalyzer.GetThinkingStyleInfo(cognitive.SecondaryStyle);

            var allTasks = new List<(string, double, ThinkingStyle)>();

            // Primary style tasks
            foreach (var task in profile1.OptimalFor)
            {
                allTasks.Add((task, cognitive.MentalClarity * 0.9, cognitive.PrimaryStyle));
            }

            // Secondary style tasks
            foreach (var task in profile2.OptimalFor)
            {
                allTasks.Add((task, cognitive.MentalClarity * 0.7, cognitive.SecondaryStyle));
            }

            return allTasks.OrderByDescending(t => t.Item2).Take(taskCount).ToList();
        }

        /// <summary>
        /// Detect if currently in flow state and suggest flow activities
        /// </summary>
        public (bool inFlow, List<string> flowActivities, string flowAdvice) GetFlowStateAnalysis(DateTime targetDate)
        {
            var cognitive = _cognitiveAnalyzer.AnalyzeCognitiveState(targetDate);

            if (!cognitive.InFlowState)
            {
                return (false, new List<string>(),
                    "Not in optimal flow state. Avoid complex tasks requiring deep immersion.");
            }

            var flowActivities = new List<string>
            {
                "Deep focused work",
                "Creative problem-solving",
                "Learning new complex material",
                "Artistic creation",
                "Strategic thinking",
                "Code refactoring",
                "Writing",
                "Designing"
            };

            var advice = $"🔥 FLOW STATE DETECTED - Peak mental condition for immersive work. " +
                        $"Quality level: {cognitive.CognitiveQualityLevel}. Window remains open for ~{cognitive.MinutesUntilCognitiveShift} minutes.";

            return (true, flowActivities, advice);
        }

        /// <summary>
        /// Get comprehensive cognitive health assessment
        /// </summary>
        public string GetCognitiveHealthAssessment(DateTime targetDate)
        {
            var cognitive = _cognitiveAnalyzer.AnalyzeCognitiveState(targetDate);

            var assessment = new System.Text.StringBuilder();
            assessment.AppendLine("╔═══ Cognitive Health Assessment ═══╗\n");

            assessment.AppendLine($"Mental Clarity:     {cognitive.MentalClarity:P0}");
            assessment.AppendLine($"Focus Intensity:    {cognitive.FocusIntensity:P0}");
            assessment.AppendLine($"Creative Capacity:  {cognitive.CreativeCapacity:P0}");
            assessment.AppendLine($"Logical Capacity:   {cognitive.LogicalCapacity:P0}");
            assessment.AppendLine($"Processing Speed:   {cognitive.ProcessingSpeed:P0}");
            assessment.AppendLine($"Intuitive Power:    {cognitive.IntuitivePower:P0}");
            assessment.AppendLine($"\nOverall Quality:    {cognitive.CognitiveQualityLevel}");
            assessment.AppendLine($"Flow State:         {(cognitive.InFlowState ? "YES 🔥" : "No")}");
            assessment.AppendLine($"Shift in:           ~{cognitive.MinutesUntilCognitiveShift} minutes");

            assessment.AppendLine($"\nPrimary Style:      {cognitive.PrimaryStyle}");
            assessment.AppendLine($"Secondary Style:    {cognitive.SecondaryStyle}");

            assessment.AppendLine($"\nBest Activities:");
            foreach (var task in cognitive.OptimalTasks.Take(5))
                assessment.AppendLine($"  ✓ {task}");

            assessment.AppendLine($"\nActivities to Avoid:");
            foreach (var task in cognitive.SuboptimalTasks.Take(3))
                assessment.AppendLine($"  ✗ {task}");

            return assessment.ToString();
        }

        private double CalculateCognitiveAlignment(string decisionType, CognitiveState cognitive)
        {
            var alignments = new Dictionary<string, (ThinkingStyle ideal, double weight)>
            {
                { "analytical", (ThinkingStyle.AnalyticalSequential, cognitive.LogicalCapacity) },
                { "creative", (ThinkingStyle.CreativeAssociative, cognitive.CreativeCapacity) },
                { "athletic", (ThinkingStyle.ConcreteExperiential, cognitive.ProcessingSpeed) },
                { "business", (ThinkingStyle.SystemicStructured, cognitive.MentalClarity) },
                { "interpersonal", (ThinkingStyle.IntuitiveFast, cognitive.IntuitivePower) },
                { "health", (ThinkingStyle.IntegrativeSynthesis, (cognitive.MentalClarity + cognitive.LogicalCapacity) / 2) }
            };

            if (alignments.TryGetValue(decisionType.ToLower(), out var alignment))
            {
                // Check if current primary or secondary style matches ideal
                bool isPrimaryMatch = cognitive.PrimaryStyle == alignment.ideal;
                bool isSecondaryMatch = cognitive.SecondaryStyle == alignment.ideal;

                double matchScore = isPrimaryMatch ? 1.0 : isSecondaryMatch ? 0.7 : 0.4;
                return matchScore * alignment.weight;
            }

            return 0.5;
        }

        private List<string> GenerateCognitiveInsights(string decisionType, CognitiveState cognitive)
        {
            var insights = new List<string>();

            // Quality assessment
            if (cognitive.CognitiveQualityLevel == "Peak")
                insights.Add($"🎯 PEAK COGNITION - Excellent mental state for {decisionType} decisions");
            else if (cognitive.CognitiveQualityLevel == "Poor")
                insights.Add($"⚠️ COGNITION ALERT - Mental fatigue detected, defer {decisionType} if possible");

            // Flow state
            if (cognitive.InFlowState)
                insights.Add("🔥 FLOW STATE - Optimal immersion for complex cognitive tasks");

            // Clarity assessment
            if (cognitive.MentalClarity < 0.4)
                insights.Add("💭 Mental clarity is low - avoid precision work and complex decisions");
            else if (cognitive.MentalClarity > 0.8)
                insights.Add("✨ Sharp mental clarity - ideal window for critical thinking");

            // Style-specific
            var styleAdvice = GetStyleSpecificAdvice(cognitive.PrimaryStyle, decisionType);
            if (!string.IsNullOrEmpty(styleAdvice))
                insights.Add(styleAdvice);

            // Capacity warnings
            if (cognitive.LogicalCapacity < 0.3 && decisionType == "analytical")
                insights.Add("⚠️ Logical capacity is low - defer analytical decisions");

            if (cognitive.CreativeCapacity > 0.8 && decisionType == "creative")
                insights.Add("🎨 Creative peak - seize this window for innovative work");

            return insights;
        }

        private string GetStyleSpecificAdvice(ThinkingStyle style, string decisionType)
        {
            return style switch
            {
                ThinkingStyle.AnalyticalSequential => "Your thinking is systematic - break complex problems into steps",
                ThinkingStyle.HolisticSynthetic => "Your thinking sees patterns - trust your big-picture perspective",
                ThinkingStyle.IntuitiveFast => "Your thinking is rapid - but verify conclusions with logic",
                ThinkingStyle.DetailOrientedPrecise => "Your thinking is exact - watch for perfectionism delays",
                ThinkingStyle.CreativeAssociative => "Your thinking is novel - balance creativity with feasibility",
                ThinkingStyle.SystemicStructured => "Your thinking is organized - ensure frameworks are flexible",
                ThinkingStyle.AdaptiveFluid => "Your thinking is responsive - maintain some consistency",
                ThinkingStyle.AbstractConceptual => "Your thinking is theoretical - connect to practical outcomes",
                ThinkingStyle.ConcreteExperiential => "Your thinking is practical - validate with real-world data",
                ThinkingStyle.IntegrativeSynthesis => "Your thinking integrates perspectives - ideal for complex decisions",
                _ => ""
            };
        }
    }

    /// <summary>
    /// Tracks cognitive patterns and thinking style evolution
    /// </summary>
    public class CognitivePatternTracker
    {
        private readonly List<(DateTime date, CognitiveState state)> _history;
        private readonly CognitiveStateAnalyzer _analyzer;

        public CognitivePatternTracker(DateTime birthDate)
        {
            _analyzer = new CognitiveStateAnalyzer(birthDate);
            _history = new List<(DateTime, CognitiveState)>();
        }

        /// <summary>
        /// Record cognitive state at a specific time
        /// </summary>
        public void RecordCognitiveState(DateTime date)
        {
            var cognitive = _analyzer.AnalyzeCognitiveState(date);
            _history.Add((date, cognitive));
        }

        /// <summary>
        /// Get dominant thinking styles by day of week
        /// </summary>
        public Dictionary<DayOfWeek, ThinkingStyle> GetThinkingStylesByDayOfWeek()
        {
            var stylesByDay = new Dictionary<DayOfWeek, List<ThinkingStyle>>();

            foreach (var (date, cognitive) in _history)
            {
                var day = date.DayOfWeek;
                if (!stylesByDay.ContainsKey(day))
                    stylesByDay[day] = new List<ThinkingStyle>();
                stylesByDay[day].Add(cognitive.PrimaryStyle);
            }

            var result = new Dictionary<DayOfWeek, ThinkingStyle>();
            foreach (var (day, styles) in stylesByDay)
            {
                result[day] = styles.GroupBy(s => s)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
            }

            return result;
        }

        /// <summary>
        /// Get cognitive quality trends
        /// </summary>
        public string GetCognitiveQualityTrends(int days = 14)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var recentStates = _history.Where(h => h.date >= cutoffDate).ToList();

            if (!recentStates.Any())
                return "No cognitive history to analyze.";

            var avgClarity = recentStates.Average(s => s.state.MentalClarity);
            var avgFocus = recentStates.Average(s => s.state.FocusIntensity);
            var avgCreative = recentStates.Average(s => s.state.CreativeCapacity);
            var avgLogic = recentStates.Average(s => s.state.LogicalCapacity);

            var trends = new System.Text.StringBuilder();
            trends.AppendLine($"Cognitive Trends (Last {days} days)\n");
            trends.AppendLine($"Mental Clarity Average:   {avgClarity:P0}");
            trends.AppendLine($"Focus Intensity Average:  {avgFocus:P0}");
            trends.AppendLine($"Creative Average:         {avgCreative:P0}");
            trends.AppendLine($"Logical Average:          {avgLogic:P0}");

            // Quality assessment
            var qualityScores = recentStates.Select(s => s.state.CognitiveQualityLevel).GroupBy(q => q);
            trends.AppendLine($"\nQuality Distribution:");
            foreach (var group in qualityScores)
                trends.AppendLine($"  {group.Key}: {group.Count()}x");

            return trends.ToString();
        }

        /// <summary>
        /// Predict optimal cognitive conditions for an activity
        /// </summary>
        public (ThinkingStyle idealStyle, string timing, List<string> optimizationTips)
            PredictOptimalConditionsForActivity(string activity)
        {
            var styleMap = new Dictionary<string, ThinkingStyle>
            {
                { "coding", ThinkingStyle.AnalyticalSequential },
                { "design", ThinkingStyle.CreativeAssociative },
                { "strategy", ThinkingStyle.HolisticSynthetic },
                { "negotiation", ThinkingStyle.IntuitiveFast },
                { "editing", ThinkingStyle.DetailOrientedPrecise },
                { "brainstorm", ThinkingStyle.CreativeAssociative },
                { "analysis", ThinkingStyle.AnalyticalSequential },
                { "planning", ThinkingStyle.SystemicStructured },
                { "learning", ThinkingStyle.AbstractConceptual },
                { "execution", ThinkingStyle.ConcreteExperiential }
            };

            var ideal = styleMap.ContainsKey(activity.ToLower())
                ? styleMap[activity.ToLower()]
                : ThinkingStyle.AnalyticalSequential;

            var tips = new List<string>
            {
                "Record outcomes with cognitive state data",
                "Batch similar activities requiring the same thinking style",
                "Build buffer time before mode switches",
                "Use the cognitive health assessment before starting"
            };

            return (ideal, "Check predicted cognitive state before scheduling", tips);
        }
    }
}
