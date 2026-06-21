using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ReadingTheMoodOfAnEnvironment
{
    public class EnvironmentalFactor
    {
        public string FactorId { get; set; }
        public string FactorName { get; set; }
        public string FactorCategory { get; set; }
        public double IntensityLevel { get; set; }
        public string InfluenceOnMood { get; set; }
        public DateTime MeasuredDate { get; set; }
    }

    public class MoodIndicator
    {
        public string IndicatorId { get; set; }
        public string IndicatorName { get; set; }
        public List<string> ObservableSignals { get; set; }
        public double CurrentLevel { get; set; }
        public double Sensitivity { get; set; }
        public DateTime AssessedDate { get; set; }
    }

    public class AtmosphereProfile
    {
        public string ProfileId { get; set; }
        public string EnvironmentName { get; set; }
        public Dictionary<string, double> FactorIntensities { get; set; }
        public Dictionary<string, double> MoodIndicators { get; set; }
        public double OverallMoodScore { get; set; }
        public string MoodDescription { get; set; }
        public DateTime ProfiledDate { get; set; }
    }

    public class MoodAssessment
    {
        public string AssessmentId { get; set; }
        public string EnvironmentId { get; set; }
        public double AmbientMood { get; set; }
        public List<string> DetectedEmotions { get; set; }
        public string PrimaryMood { get; set; }
        public string MoodIntensity { get; set; }
        public List<string> ContributingFactors { get; set; }
        public DateTime AssessedDate { get; set; }
    }

    public class MoodAnalysisEngine
    {
        private Dictionary<string, EnvironmentalFactor> factors;
        private Dictionary<string, MoodIndicator> indicators;
        private Dictionary<string, AtmosphereProfile> profiles;
        private Dictionary<string, MoodAssessment> assessments;

        public MoodAnalysisEngine()
        {
            factors = new Dictionary<string, EnvironmentalFactor>();
            indicators = new Dictionary<string, MoodIndicator>();
            profiles = new Dictionary<string, AtmosphereProfile>();
            assessments = new Dictionary<string, MoodAssessment>();
        }

        public void RegisterEnvironmentalFactor(string factorId, string factorName, string category,
                                               double intensity, string influence)
        {
            var factor = new EnvironmentalFactor
            {
                FactorId = factorId,
                FactorName = factorName,
                FactorCategory = category,
                IntensityLevel = intensity,
                InfluenceOnMood = influence,
                MeasuredDate = DateTime.Now
            };
            factors[factorId] = factor;
        }

        public void RegisterMoodIndicator(string indicatorId, string indicatorName,
                                         List<string> signals, double sensitivity)
        {
            var indicator = new MoodIndicator
            {
                IndicatorId = indicatorId,
                IndicatorName = indicatorName,
                ObservableSignals = new List<string>(signals),
                CurrentLevel = 0.5,
                Sensitivity = sensitivity,
                AssessedDate = DateTime.Now
            };
            indicators[indicatorId] = indicator;
        }

        public void UpdateMoodIndicator(string indicatorId, double level)
        {
            if (indicators.ContainsKey(indicatorId))
            {
                indicators[indicatorId].CurrentLevel = level;
            }
        }

        public void AnalyzeEnvironment(string profileId, string environmentName, Dictionary<string, double> factorData)
        {
            var profile = new AtmosphereProfile
            {
                ProfileId = profileId,
                EnvironmentName = environmentName,
                FactorIntensities = new Dictionary<string, double>(factorData),
                MoodIndicators = new Dictionary<string, double>(),
                OverallMoodScore = 0.0,
                MoodDescription = "",
                ProfiledDate = DateTime.Now
            };

            double totalMoodScore = 0.0;
            int factorCount = 0;

            foreach (var kvp in factorData)
            {
                if (factors.ContainsKey(kvp.Key))
                {
                    var factor = factors[kvp.Key];
                    double contribution = kvp.Value * (factor.InfluenceOnMood == "Positive" ? 1.0 : -1.0);
                    totalMoodScore += contribution;
                    factorCount++;
                }
            }

            profile.OverallMoodScore = factorCount > 0 ? totalMoodScore / factorCount : 0.5;
            profile.MoodDescription = DescribeMood(profile.OverallMoodScore);

            profiles[profileId] = profile;
        }

        public void AssessMood(string assessmentId, string environmentId)
        {
            if (!profiles.ContainsKey(environmentId)) return;

            var profile = profiles[environmentId];
            var assessment = new MoodAssessment
            {
                AssessmentId = assessmentId,
                EnvironmentId = environmentId,
                AmbientMood = profile.OverallMoodScore,
                DetectedEmotions = new List<string>(),
                PrimaryMood = "",
                MoodIntensity = "",
                ContributingFactors = new List<string>(),
                AssessedDate = DateTime.Now
            };

            assessment.PrimaryMood = ClassifyMood(profile.OverallMoodScore);
            assessment.MoodIntensity = GetMoodIntensity(Math.Abs(profile.OverallMoodScore - 0.5));

            foreach (var factor in profile.FactorIntensities)
            {
                if (factor.Value > 0.6)
                {
                    assessment.ContributingFactors.Add(factor.Key);
                }
            }

            if (profile.OverallMoodScore > 0.6)
                assessment.DetectedEmotions.AddRange(new[] { "Happy", "Energetic", "Positive" });
            else if (profile.OverallMoodScore < 0.4)
                assessment.DetectedEmotions.AddRange(new[] { "Sad", "Tense", "Negative" });
            else
                assessment.DetectedEmotions.AddRange(new[] { "Neutral", "Calm", "Balanced" });

            assessments[assessmentId] = assessment;
        }

        private string ClassifyMood(double score)
        {
            return score switch
            {
                > 0.7 => "Very Positive",
                > 0.6 => "Positive",
                > 0.5 => "Neutral-Positive",
                > 0.4 => "Neutral-Negative",
                > 0.3 => "Negative",
                _ => "Very Negative"
            };
        }

        private string GetMoodIntensity(double variance)
        {
            return variance switch
            {
                > 0.3 => "Intense",
                > 0.2 => "Strong",
                > 0.1 => "Moderate",
                _ => "Subtle"
            };
        }

        private string DescribeMood(double score)
        {
            if (score > 0.7) return "Vibrant and uplifting atmosphere";
            if (score > 0.6) return "Positive and comfortable mood";
            if (score > 0.5) return "Neutral with slight positivity";
            if (score > 0.4) return "Neutral with slight negativity";
            if (score > 0.3) return "Tense and uncomfortable mood";
            return "Deeply negative atmosphere";
        }

        public void DisplayAssessment(string assessmentId)
        {
            if (!assessments.ContainsKey(assessmentId)) return;

            var assessment = assessments[assessmentId];
            Console.WriteLine($"\n  Mood Assessment: {assessment.AssessmentId}");
            Console.WriteLine($"  Primary Mood: {assessment.PrimaryMood}");
            Console.WriteLine($"  Intensity: {assessment.MoodIntensity}");
            Console.WriteLine($"  Detected Emotions: {string.Join(", ", assessment.DetectedEmotions)}");
            if (assessment.ContributingFactors.Count > 0)
            {
                Console.WriteLine($"  Contributing Factors: {string.Join(", ", assessment.ContributingFactors)}");
            }
        }

        public int GetTotalFactors()
        {
            return factors.Count;
        }

        public int GetTotalIndicators()
        {
            return indicators.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Reading the Mood of an Environment               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new MoodAnalysisEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Environmental Factors]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterEnvironmentalFactor("FACTOR-001", "Lighting", "Physical", 0.8, "Positive");
        engine.RegisterEnvironmentalFactor("FACTOR-002", "Temperature", "Physical", 0.7, "Positive");
        engine.RegisterEnvironmentalFactor("FACTOR-003", "Noise Level", "Audio", 0.4, "Negative");
        engine.RegisterEnvironmentalFactor("FACTOR-004", "Crowding", "Social", 0.5, "Negative");
        engine.RegisterEnvironmentalFactor("FACTOR-005", "Colors", "Visual", 0.9, "Positive");

        Console.WriteLine("  ✓ Registered 5 environmental factors");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Mood Indicators]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterMoodIndicator("IND-001", "Facial Expressions",
            new List<string> { "Smile frequency", "Eye contact", "Tension in face" }, 0.85);

        engine.RegisterMoodIndicator("IND-002", "Body Language",
            new List<string> { "Posture", "Gesture openness", "Movement energy" }, 0.80);

        engine.RegisterMoodIndicator("IND-003", "Verbal Tone",
            new List<string> { "Voice pitch", "Speech rate", "Enthusiasm level" }, 0.75);

        Console.WriteLine("  ✓ Registered 3 mood indicators");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Analyzing Environment Mood]");
        Console.ResetColor();
        Thread.Sleep(500);

        var positiveFactors = new Dictionary<string, double> {
            { "FACTOR-001", 0.9 },
            { "FACTOR-002", 0.8 },
            { "FACTOR-003", 0.3 },
            { "FACTOR-004", 0.4 },
            { "FACTOR-005", 0.95 }
        };

        engine.AnalyzeEnvironment("PROF-001", "Bright Office Space", positiveFactors);

        var negativeFactors = new Dictionary<string, double> {
            { "FACTOR-001", 0.3 },
            { "FACTOR-002", 0.2 },
            { "FACTOR-003", 0.8 },
            { "FACTOR-004", 0.7 },
            { "FACTOR-005", 0.2 }
        };

        engine.AnalyzeEnvironment("PROF-002", "Crowded Basement", negativeFactors);

        Console.WriteLine("  ✓ Analyzed 2 different environments");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Assessing Mood]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AssessMood("ASSESS-001", "PROF-001");
        engine.AssessMood("ASSESS-002", "PROF-002");

        Console.WriteLine("  ✓ Assessed mood for both environments");
        engine.DisplayAssessment("ASSESS-001");
        engine.DisplayAssessment("ASSESS-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Mood Analysis Summary:");
        Console.WriteLine($"    Environmental Factors: {engine.GetTotalFactors()}");
        Console.WriteLine($"    Mood Indicators: {engine.GetTotalIndicators()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Mood Reading Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Environmental Mood Analysis Architecture:");
        Console.WriteLine("    Layer 1: Factor Registration (define environmental elements)");
        Console.WriteLine("    Layer 2: Indicator Registration (establish mood signals)");
        Console.WriteLine("    Layer 3: Data Collection (measure factor intensities)");
        Console.WriteLine("    Layer 4: Environmental Analysis (combine factor data)");
        Console.WriteLine("    Layer 5: Mood Scoring (calculate composite mood)");
        Console.WriteLine("    Layer 6: Emotion Classification (identify emotional tone)");
        Console.WriteLine("    Layer 7: Assessment Report (summarize mood characteristics)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple environmental factors");
        Console.WriteLine("    ✓ Track mood indicators and signals");
        Console.WriteLine("    ✓ Analyze combined environmental impact");
        Console.WriteLine("    ✓ Classify mood intensity and type");
        Console.WriteLine("    ✓ Identify contributing factors");
        Console.WriteLine("    ✓ Generate mood descriptions");
        Console.WriteLine("    ✓ Support multi-environment comparison");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Environmental mood analysis system complete");
        Console.ResetColor();
    }
}
