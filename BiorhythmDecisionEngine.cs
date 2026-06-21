using System;
using System.Collections.Generic;
using System.Linq;

namespace BiorhythmDecisionSystem
{
    /// <summary>
    /// Represents the three biorhythmic cycles
    /// </summary>
    public enum CycleType
    {
        Physical = 23,  // 23-day cycle
        Emotional = 28, // 28-day cycle
        Intellectual = 33 // 33-day cycle
    }

    /// <summary>
    /// Biorhythm state indicating the current phase in a cycle
    /// </summary>
    public enum CyclePhase
    {
        HighPhase,      // 50% to 100% of cycle (peak performance)
        TransitionHigh, // High to Low transition (caution zone)
        LowPhase,       // 0% to 50% of cycle (low performance)
        TransitionLow,  // Low to High transition (critical day)
        Critical        // Crossing zero line (unstable, avoid decisions)
    }

    /// <summary>
    /// Represents a single biorhythm measurement
    /// </summary>
    public class BiorhythmReading
    {
        public CycleType Type { get; set; }
        public double Value { get; set; } // -1.0 to 1.0
        public CyclePhase Phase { get; set; }
        public int DaysInCycle { get; set; }
        public double PercentageComplete { get; set; }
    }

    /// <summary>
    /// Complete biorhythm profile for a given date
    /// </summary>
    public class BiorhythmProfile
    {
        public DateTime Date { get; set; }
        public BiorhythmReading Physical { get; set; }
        public BiorhythmReading Emotional { get; set; }
        public BiorhythmReading Intellectual { get; set; }
        public double HarmonicIndex { get; set; } // Overall alignment score
    }

    /// <summary>
    /// Calculates biorhythm values based on the user's birth date
    /// </summary>
    public class BiorhythmCalculator
    {
        private readonly DateTime _birthDate;

        public BiorhythmCalculator(DateTime birthDate)
        {
            _birthDate = birthDate;
        }

        /// <summary>
        /// Calculate biorhythm value for a specific cycle on a given date
        /// </summary>
        public double CalculateCycleValue(DateTime targetDate, CycleType cycle)
        {
            int daysSinceBirth = (int)(targetDate - _birthDate).TotalDays;
            int cycleDays = (int)cycle;
            double dayInCycle = (daysSinceBirth % cycleDays) / (double)cycleDays;

            // Sine wave calculation: sin(2π * dayInCycle)
            return Math.Sin(2 * Math.PI * dayInCycle);
        }

        /// <summary>
        /// Determine which phase of the cycle we're in
        /// </summary>
        private CyclePhase DetermineCyclePhase(double value)
        {
            const double criticalThreshold = 0.1;

            if (Math.Abs(value) < criticalThreshold)
                return CyclePhase.Critical;

            if (value > 0.7)
                return CyclePhase.HighPhase;
            else if (value > 0)
                return CyclePhase.TransitionHigh;
            else if (value > -0.7)
                return CyclePhase.TransitionLow;
            else
                return CyclePhase.LowPhase;
        }

        /// <summary>
        /// Get the complete biorhythm profile for a specific date
        /// </summary>
        public BiorhythmProfile GetProfile(DateTime targetDate)
        {
            var profile = new BiorhythmProfile { Date = targetDate };

            // Calculate each cycle
            profile.Physical = GetCycleReading(targetDate, CycleType.Physical);
            profile.Emotional = GetCycleReading(targetDate, CycleType.Emotional);
            profile.Intellectual = GetCycleReading(targetDate, CycleType.Intellectual);

            // Calculate harmonic index (overall alignment)
            double avgValue = (profile.Physical.Value + profile.Emotional.Value + profile.Intellectual.Value) / 3;
            profile.HarmonicIndex = (avgValue + 1) / 2; // Normalize to 0-1 range

            return profile;
        }

        private BiorhythmReading GetCycleReading(DateTime targetDate, CycleType cycle)
        {
            double value = CalculateCycleValue(targetDate, cycle);
            int cycleDays = (int)cycle;
            int daysSinceBirth = (int)(targetDate - _birthDate).TotalDays;
            int daysInCurrentCycle = daysSinceBirth % cycleDays;

            return new BiorhythmReading
            {
                Type = cycle,
                Value = value,
                Phase = DetermineCyclePhase(value),
                DaysInCycle = cycleDays,
                PercentageComplete = (daysInCurrentCycle / (double)cycleDays) * 100
            };
        }
    }

    /// <summary>
    /// Decision recommendations based on biorhythmic alignment
    /// </summary>
    public class DecisionRecommendation
    {
        public string DecisionType { get; set; }
        public double RecommendationScore { get; set; } // 0-100
        public string Recommendation { get; set; }
        public List<string> Warnings { get; set; }
        public List<string> OptimalTiming { get; set; }
    }

    /// <summary>
    /// Core decision-making engine that factors in biorhythmic data
    /// </summary>
    public class BiorhythmicDecisionEngine
    {
        private readonly BiorhythmCalculator _calculator;

        // Decision type requirements mapping
        private readonly Dictionary<string, (CycleType primary, CycleType secondary, CycleType tertiary)> _decisionCycles =
            new Dictionary<string, (CycleType, CycleType, CycleType)>
            {
                { "athletic", (CycleType.Physical, CycleType.Emotional, CycleType.Intellectual) },
                { "creative", (CycleType.Emotional, CycleType.Intellectual, CycleType.Physical) },
                { "analytical", (CycleType.Intellectual, CycleType.Physical, CycleType.Emotional) },
                { "business", (CycleType.Intellectual, CycleType.Emotional, CycleType.Physical) },
                { "interpersonal", (CycleType.Emotional, CycleType.Intellectual, CycleType.Physical) },
                { "health", (CycleType.Physical, CycleType.Emotional, CycleType.Intellectual) }
            };

        public BiorhythmicDecisionEngine(DateTime birthDate)
        {
            _calculator = new BiorhythmCalculator(birthDate);
        }

        /// <summary>
        /// Get a decision recommendation for a specific activity type on a given date
        /// </summary>
        public DecisionRecommendation GetDecisionRecommendation(string decisionType, DateTime targetDate)
        {
            if (!_decisionCycles.ContainsKey(decisionType.ToLower()))
            {
                return new DecisionRecommendation
                {
                    DecisionType = decisionType,
                    Recommendation = $"Unknown decision type: {decisionType}",
                    RecommendationScore = 0,
                    Warnings = new List<string> { "Invalid decision type" },
                    OptimalTiming = new List<string>()
                };
            }

            var profile = _calculator.GetProfile(targetDate);
            var (primary, secondary, tertiary) = _decisionCycles[decisionType.ToLower()];

            // Calculate weighted score
            double score = CalculateDecisionScore(profile, primary, secondary, tertiary);

            var recommendation = new DecisionRecommendation
            {
                DecisionType = decisionType,
                RecommendationScore = score,
                Warnings = GenerateWarnings(profile, primary, secondary, tertiary),
                OptimalTiming = GenerateOptimalTiming(decisionType, profile)
            };

            recommendation.Recommendation = GenerateRecommendationText(score, recommendation.Warnings);

            return recommendation;
        }

        private double CalculateDecisionScore(BiorhythmProfile profile, CycleType primary, CycleType secondary, CycleType tertiary)
        {
            var readings = new Dictionary<CycleType, BiorhythmReading>
            {
                { CycleType.Physical, profile.Physical },
                { CycleType.Emotional, profile.Emotional },
                { CycleType.Intellectual, profile.Intellectual }
            };

            // Weighted average: primary (50%), secondary (30%), tertiary (20%)
            double primaryValue = (readings[primary].Value + 1) / 2 * 0.50;
            double secondaryValue = (readings[secondary].Value + 1) / 2 * 0.30;
            double tertiaryValue = (readings[tertiary].Value + 1) / 2 * 0.20;

            return (primaryValue + secondaryValue + tertiaryValue) * 100;
        }

        private List<string> GenerateWarnings(BiorhythmProfile profile, CycleType primary, CycleType secondary, CycleType tertiary)
        {
            var warnings = new List<string>();
            var cycles = new[] {
                (primary, profile.Physical),
                (secondary, profile.Emotional),
                (tertiary, profile.Intellectual)
            };

            foreach (var (cycleType, reading) in cycles)
            {
                if (reading.Phase == CyclePhase.Critical)
                    warnings.Add($"⚠️ CRITICAL: {cycleType} cycle is at critical point - avoid major decisions");
                else if (reading.Phase == CyclePhase.LowPhase)
                    warnings.Add($"⚠️ {cycleType} cycle is in low phase - reduced capability");
                else if (reading.Phase == CyclePhase.TransitionLow)
                    warnings.Add($"⚠️ {cycleType} cycle is transitioning down - proceed with caution");
            }

            return warnings;
        }

        private List<string> GenerateOptimalTiming(string decisionType, BiorhythmProfile profile)
        {
            var timing = new List<string>();

            if (profile.HarmonicIndex > 0.75)
                timing.Add($"✓ Excellent timing for {decisionType} decisions");
            else if (profile.HarmonicIndex > 0.60)
                timing.Add($"✓ Good timing for {decisionType} decisions");

            // Find next high phase days
            var nextHighDays = FindNextOptimalDays(decisionType, profile.Date, 5);
            if (nextHighDays.Any())
            {
                timing.Add($"📅 Next optimal days: {string.Join(", ", nextHighDays.Select(d => d.ToString("MMM dd")))}");
            }

            return timing;
        }

        private List<DateTime> FindNextOptimalDays(string decisionType, DateTime startDate, int daysToCheck)
        {
            var optimalDays = new List<DateTime>();
            var (primary, _, _) = _decisionCycles[decisionType.ToLower()];

            for (int i = 1; i <= daysToCheck * 7; i++) // Check up to 5 weeks
            {
                var checkDate = startDate.AddDays(i);
                var profile = _calculator.GetProfile(checkDate);
                var readings = new Dictionary<CycleType, BiorhythmReading>
                {
                    { CycleType.Physical, profile.Physical },
                    { CycleType.Emotional, profile.Emotional },
                    { CycleType.Intellectual, profile.Intellectual }
                };

                if (readings[primary].Phase == CyclePhase.HighPhase)
                {
                    optimalDays.Add(checkDate);
                    if (optimalDays.Count >= daysToCheck)
                        break;
                }
            }

            return optimalDays;
        }

        private string GenerateRecommendationText(double score, List<string> warnings)
        {
            if (warnings.Any(w => w.Contains("CRITICAL")))
                return "🚫 NOT RECOMMENDED - Wait for more favorable alignment";

            if (score >= 80)
                return "✅ HIGHLY RECOMMENDED - Excellent conditions for this decision";
            else if (score >= 60)
                return "✓ RECOMMENDED - Good conditions, proceed with standard precautions";
            else if (score >= 40)
                return "⚠️ PROCEED WITH CAUTION - Suboptimal conditions, extra care advised";
            else
                return "❌ NOT RECOMMENDED - Poor biorhythmic alignment, consider postponing";
        }
    }

    /// <summary>
    /// Example usage and demonstration
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            // Initialize with a birth date
            var birthDate = new DateTime(1990, 5, 15);
            var engine = new BiorhythmicDecisionEngine(birthDate);

            Console.WriteLine("=== Biorhythmic Decision Engine ===\n");
            Console.WriteLine($"Birth Date: {birthDate:MMMM dd, yyyy}\n");

            // Get recommendation for different decision types
            var targetDate = DateTime.Now;
            var decisionTypes = new[] { "athletic", "creative", "analytical", "business", "interpersonal", "health" };

            foreach (var decisionType in decisionTypes)
            {
                var recommendation = engine.GetDecisionRecommendation(decisionType, targetDate);
                PrintRecommendation(recommendation);
                Console.WriteLine();
            }
        }

        private static void PrintRecommendation(DecisionRecommendation rec)
        {
            Console.WriteLine($"Decision Type: {rec.DecisionType.ToUpper()}");
            Console.WriteLine($"Score: {rec.RecommendationScore:F1}/100");
            Console.WriteLine($"Recommendation: {rec.Recommendation}");

            if (rec.Warnings.Any())
            {
                Console.WriteLine("Warnings:");
                foreach (var warning in rec.Warnings)
                    Console.WriteLine($"  {warning}");
            }

            if (rec.OptimalTiming.Any())
            {
                Console.WriteLine("Timing Info:");
                foreach (var timing in rec.OptimalTiming)
                    Console.WriteLine($"  {timing}");
            }
        }
    }
}
