using System;
using System.Collections.Generic;
using System.Linq;
using BiorhythmDecisionSystem;

namespace BiorhythmDecisionSystem.Tests
{
    /// <summary>
    /// Unit tests for the Biorhythmic Decision System
    /// </summary>
    public class BiorhythmTests
    {
        private const bool RunTests = true;

        public static void Main()
        {
            if (!RunTests)
                return;

            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║   Biorhythm System Tests                   ║");
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            var birthDate = new DateTime(1990, 5, 15);

            RunCalculatorTests(birthDate);
            RunCyclePhaseTests(birthDate);
            RunDecisionEngineTests(birthDate);
            RunAnalyticsTests(birthDate);
            RunLearnerTests(birthDate);

            Console.WriteLine("\n✓ All tests completed!");
        }

        static void RunCalculatorTests(DateTime birthDate)
        {
            Console.WriteLine("╔ Calculator Tests ╗\n");
            var calculator = new BiorhythmCalculator(birthDate);

            // Test 1: Verify value range
            var testDate = DateTime.Now;
            double physicalValue = calculator.CalculateCycleValue(testDate, CycleType.Physical);
            double emotionalValue = calculator.CalculateCycleValue(testDate, CycleType.Emotional);
            double intellectualValue = calculator.CalculateCycleValue(testDate, CycleType.Intellectual);

            Assert(physicalValue >= -1 && physicalValue <= 1, "Physical value in range [-1, 1]");
            Assert(emotionalValue >= -1 && emotionalValue <= 1, "Emotional value in range [-1, 1]");
            Assert(intellectualValue >= -1 && intellectualValue <= 1, "Intellectual value in range [-1, 1]");

            // Test 2: Get profile
            var profile = calculator.GetProfile(testDate);
            Assert(profile != null, "Profile created successfully");
            Assert(profile.Physical != null, "Physical reading exists");
            Assert(profile.Emotional != null, "Emotional reading exists");
            Assert(profile.Intellectual != null, "Intellectual reading exists");

            // Test 3: Harmonic index range
            Assert(profile.HarmonicIndex >= 0 && profile.HarmonicIndex <= 1, "Harmonic index in range [0, 1]");

            // Test 4: Cycle day calculation
            Assert(profile.Physical.DaysInCycle == 23, "Physical cycle is 23 days");
            Assert(profile.Emotional.DaysInCycle == 28, "Emotional cycle is 28 days");
            Assert(profile.Intellectual.DaysInCycle == 33, "Intellectual cycle is 33 days");

            Console.WriteLine("✓ All calculator tests passed!\n");
        }

        static void RunCyclePhaseTests(DateTime birthDate)
        {
            Console.WriteLine("╔ Cycle Phase Tests ╗\n");
            var calculator = new BiorhythmCalculator(birthDate);

            // Find a critical day (cycle crossing near zero)
            int daysTested = 0;
            bool foundCritical = false;

            for (int i = 0; i < 50 && !foundCritical; i++)
            {
                var date = DateTime.Now.AddDays(i);
                var profile = calculator.GetProfile(date);
                daysTested++;

                if (profile.Physical.Phase == CyclePhase.Critical ||
                    profile.Emotional.Phase == CyclePhase.Critical ||
                    profile.Intellectual.Phase == CyclePhase.Critical)
                {
                    foundCritical = true;
                    Console.WriteLine($"Found critical day: {date:MMM dd, yyyy}");
                }
            }

            Assert(foundCritical, $"Critical phase found within {daysTested} days");

            // Test phase consistency
            var testDate = DateTime.Now;
            var profile = calculator.GetProfile(testDate);
            var validPhases = new[] { CyclePhase.Critical, CyclePhase.HighPhase, CyclePhase.LowPhase,
                                     CyclePhase.TransitionHigh, CyclePhase.TransitionLow };

            Assert(validPhases.Contains(profile.Physical.Phase), "Physical phase is valid");
            Assert(validPhases.Contains(profile.Emotional.Phase), "Emotional phase is valid");
            Assert(validPhases.Contains(profile.Intellectual.Phase), "Intellectual phase is valid");

            Console.WriteLine("✓ All phase tests passed!\n");
        }

        static void RunDecisionEngineTests(DateTime birthDate)
        {
            Console.WriteLine("╔ Decision Engine Tests ╗\n");
            var engine = new BiorhythmicDecisionEngine(birthDate);

            // Test valid decision types
            string[] decisionTypes = { "athletic", "creative", "analytical", "business", "interpersonal", "health" };

            foreach (var decisionType in decisionTypes)
            {
                var recommendation = engine.GetDecisionRecommendation(decisionType, DateTime.Now);
                Assert(recommendation != null, $"Recommendation created for {decisionType}");
                Assert(recommendation.RecommendationScore >= 0 && recommendation.RecommendationScore <= 100,
                    $"{decisionType} score in range [0, 100]");
                Assert(!string.IsNullOrEmpty(recommendation.Recommendation), $"{decisionType} has recommendation text");
            }

            Console.WriteLine($"✓ All {decisionTypes.Length} decision types tested!");

            // Test invalid decision type
            var invalidRec = engine.GetDecisionRecommendation("invalid_type", DateTime.Now);
            Assert(invalidRec.RecommendationScore == 0, "Invalid type returns score 0");
            Assert(invalidRec.Warnings.Any(w => w.Contains("Unknown")), "Invalid type has warning");

            Console.WriteLine("✓ Invalid type handling works!\n");
        }

        static void RunAnalyticsTests(DateTime birthDate)
        {
            Console.WriteLine("╔ Analytics Tests ╗\n");
            var analytics = new BiorhythmAnalytics(birthDate);

            // Test optimal days finding
            var optimalDays = analytics.FindOptimalDays("creative", 30);
            Assert(optimalDays.Count > 0, "Found optimal days");
            Assert(optimalDays.Count <= 10, "Returned at most 10 days");

            // Verify days are sorted by score descending
            bool sorted = optimalDays.Zip(optimalDays.Skip(1))
                .All(pair => pair.First.score >= pair.Second.score);
            Assert(sorted, "Optimal days are sorted by score descending");

            // Test critical days finding
            var criticalDays = analytics.FindCriticalDays(60);
            // Critical days may be empty, which is valid
            Assert(criticalDays.Count <= 60, "Critical days within search range");

            // Test phase analysis text
            var analysis = analytics.GetPhaseAnalysis(DateTime.Now);
            Assert(!string.IsNullOrEmpty(analysis), "Phase analysis generated");
            Assert(analysis.Contains("Physical"), "Analysis includes physical cycle");
            Assert(analysis.Contains("Emotional"), "Analysis includes emotional cycle");
            Assert(analysis.Contains("Intellectual"), "Analysis includes intellectual cycle");

            Console.WriteLine("✓ All analytics tests passed!\n");
        }

        static void RunLearnerTests(DateTime birthDate)
        {
            Console.WriteLine("╔ Decision Learner Tests ╗\n");
            var engine = new BiorhythmicDecisionEngine(birthDate);
            var learner = new BiorhythmDecisionLearner(engine);

            // Test recording decisions
            var testDate = DateTime.Now.AddDays(-5);
            learner.RecordDecision("business", testDate, "success", 8, "Test decision");
            learner.RecordDecision("business", testDate.AddDays(-1), "failure", 3, "Test decision 2");
            learner.RecordDecision("business", testDate.AddDays(-2), "success", 7, "Test decision 3");

            // Get stats
            var stats = learner.GetPerformanceStats("business");
            Assert(stats.ContainsKey("total_decisions"), "Stats contain total decisions");
            Assert((int)stats["total_decisions"] == 3, "Correct number of decisions recorded");
            Assert((int)stats["success_count"] == 2, "Correct success count");
            Assert((double)stats["success_rate"] == 2.0 / 3, "Correct success rate");

            // Test adaptive recommendation
            var adaptive = learner.GetAdaptiveRecommendation("business", DateTime.Now);
            Assert(adaptive != null, "Adaptive recommendation generated");
            Assert(adaptive.Warnings.Any(), "Adaptive recommendation includes insights from history");

            Console.WriteLine("✓ All learner tests passed!\n");
        }

        static void Assert(bool condition, string message)
        {
            if (condition)
                Console.WriteLine($"  ✓ {message}");
            else
            {
                Console.WriteLine($"  ✗ FAILED: {message}");
                throw new Exception($"Test failed: {message}");
            }
        }
    }
}
