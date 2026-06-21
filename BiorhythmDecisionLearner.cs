using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace BiorhythmDecisionSystem
{
    /// <summary>
    /// Records historical decisions and their outcomes for learning
    /// </summary>
    public class DecisionRecord
    {
        public DateTime DecisionDate { get; set; }
        public string DecisionType { get; set; }
        public double BiorhythmScore { get; set; }
        public string Outcome { get; set; } // "success", "failure", "neutral"
        public int OutcomeRating { get; set; } // 1-10 scale
        public string Notes { get; set; }
    }

    /// <summary>
    /// Learning model that adapts recommendations based on historical performance
    /// </summary>
    public class BiorhythmDecisionLearner
    {
        private readonly List<DecisionRecord> _history;
        private readonly BiorhythmicDecisionEngine _engine;
        private const string DataFile = "decision_history.json";

        public BiorhythmDecisionLearner(BiorhythmicDecisionEngine engine)
        {
            _engine = engine;
            _history = LoadHistory();
        }

        /// <summary>
        /// Record a decision outcome
        /// </summary>
        public void RecordDecision(string decisionType, DateTime decisionDate, string outcome, int rating, string notes = "")
        {
            var record = new DecisionRecord
            {
                DecisionDate = decisionDate,
                DecisionType = decisionType.ToLower(),
                Outcome = outcome.ToLower(),
                OutcomeRating = Math.Max(1, Math.Min(10, rating)),
                Notes = notes,
                BiorhythmScore = 0 // Would be populated from engine
            };

            _history.Add(record);
            SaveHistory();
        }

        /// <summary>
        /// Get adaptive recommendation based on historical performance
        /// </summary>
        public DecisionRecommendation GetAdaptiveRecommendation(string decisionType, DateTime targetDate)
        {
            var baseRecommendation = _engine.GetDecisionRecommendation(decisionType, targetDate);

            // Analyze historical performance
            var decisionHistory = _history.Where(h => h.DecisionType == decisionType.ToLower()).ToList();

            if (decisionHistory.Count >= 3)
            {
                var adjustedScore = AdjustScoreBasedOnHistory(baseRecommendation.RecommendationScore, decisionHistory);
                baseRecommendation.RecommendationScore = adjustedScore;

                // Add learned insights
                var insights = GenerateLearnedInsights(decisionType, decisionHistory);
                baseRecommendation.Warnings.InsertRange(0, insights);
            }

            return baseRecommendation;
        }

        private double AdjustScoreBasedOnHistory(double baseScore, List<DecisionRecord> history)
        {
            // Calculate success rate
            double successRate = history
                .Where(h => h.Outcome == "success")
                .Average(h => h.OutcomeRating) / 10.0;

            // Weight high biorhythm scores vs low scores
            var highScoreDecisions = history.Where(h => h.BiorhythmScore >= 70).ToList();
            var lowScoreDecisions = history.Where(h => h.BiorhythmScore < 70).ToList();

            double highScoreSuccessRate = highScoreDecisions.Any()
                ? highScoreDecisions.Count(h => h.Outcome == "success") / (double)highScoreDecisions.Count
                : 0.5;

            double lowScoreSuccessRate = lowScoreDecisions.Any()
                ? lowScoreDecisions.Count(h => h.Outcome == "success") / (double)lowScoreDecisions.Count
                : 0.5;

            // Adaptive multiplier
            double confidence = (highScoreSuccessRate - lowScoreSuccessRate) * 0.5 + 0.75;

            return baseScore * confidence;
        }

        private List<string> GenerateLearnedInsights(string decisionType, List<DecisionRecord> history)
        {
            var insights = new List<string>();

            var avgRating = history.Average(h => h.OutcomeRating);
            if (avgRating >= 7)
                insights.Add($"📊 Historical data: {decisionType} decisions perform well overall (avg rating: {avgRating:F1}/10)");
            else if (avgRating < 4)
                insights.Add($"📊 Historical data: {decisionType} decisions underperform (avg rating: {avgRating:F1}/10) - extra caution advised");

            // Find best timing patterns
            var successfulDecisions = history.Where(h => h.Outcome == "success").ToList();
            if (successfulDecisions.Any())
            {
                insights.Add($"📈 Success rate: {(successfulDecisions.Count / (double)history.Count * 100):F0}%");
            }

            return insights;
        }

        /// <summary>
        /// Get performance statistics for a decision type
        /// </summary>
        public Dictionary<string, object> GetPerformanceStats(string decisionType)
        {
            var decisions = _history.Where(h => h.DecisionType == decisionType.ToLower()).ToList();

            if (!decisions.Any())
                return new Dictionary<string, object> { { "error", "No historical data" } };

            return new Dictionary<string, object>
            {
                { "total_decisions", decisions.Count },
                { "success_count", decisions.Count(h => h.Outcome == "success") },
                { "failure_count", decisions.Count(h => h.Outcome == "failure") },
                { "neutral_count", decisions.Count(h => h.Outcome == "neutral") },
                { "success_rate", decisions.Count(h => h.Outcome == "success") / (double)decisions.Count },
                { "average_rating", decisions.Average(h => h.OutcomeRating) },
                { "best_rating", decisions.Max(h => h.OutcomeRating) },
                { "worst_rating", decisions.Min(h => h.OutcomeRating) }
            };
        }

        private void SaveHistory()
        {
            var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DataFile, json);
        }

        private List<DecisionRecord> LoadHistory()
        {
            if (!File.Exists(DataFile))
                return new List<DecisionRecord>();

            try
            {
                var json = File.ReadAllText(DataFile);
                return JsonSerializer.Deserialize<List<DecisionRecord>>(json) ?? new List<DecisionRecord>();
            }
            catch
            {
                return new List<DecisionRecord>();
            }
        }

        /// <summary>
        /// Export decision history to CSV for analysis
        /// </summary>
        public string ExportToCsv()
        {
            if (!_history.Any())
                return "No data to export";

            var csv = "DecisionDate,DecisionType,BiorhythmScore,Outcome,OutcomeRating,Notes\n";
            foreach (var record in _history)
            {
                csv += $"{record.DecisionDate:yyyy-MM-dd},{record.DecisionType},{record.BiorhythmScore:F2},{record.Outcome},{record.OutcomeRating},\"{record.Notes}\"\n";
            }

            return csv;
        }
    }

    /// <summary>
    /// Advanced analytics for biorhythm-decision correlation
    /// </summary>
    public class BiorhythmAnalytics
    {
        private readonly BiorhythmCalculator _calculator;

        public BiorhythmAnalytics(DateTime birthDate)
        {
            _calculator = new BiorhythmCalculator(birthDate);
        }

        /// <summary>
        /// Find the best days in the next N days for a specific activity
        /// </summary>
        public List<(DateTime date, double score)> FindOptimalDays(string decisionType, int daysToAnalyze = 30)
        {
            var today = DateTime.Now;
            var scores = new List<(DateTime, double)>();

            for (int i = 0; i < daysToAnalyze; i++)
            {
                var date = today.AddDays(i);
                var profile = _calculator.GetProfile(date);

                // Score based on harmonic index
                scores.Add((date, profile.HarmonicIndex * 100));
            }

            return scores.OrderByDescending(s => s.score).Take(10).ToList();
        }

        /// <summary>
        /// Predict critical days (avoid major decisions)
        /// </summary>
        public List<DateTime> FindCriticalDays(int daysToAnalyze = 30)
        {
            var today = DateTime.Now;
            var criticalDays = new List<DateTime>();

            for (int i = 0; i < daysToAnalyze; i++)
            {
                var date = today.AddDays(i);
                var profile = _calculator.GetProfile(date);

                // Critical if all three cycles are in critical zone
                if (profile.Physical.Phase == CyclePhase.Critical &&
                    profile.Emotional.Phase == CyclePhase.Critical &&
                    profile.Intellectual.Phase == CyclePhase.Critical)
                {
                    criticalDays.Add(date);
                }
            }

            return criticalDays;
        }

        /// <summary>
        /// Get detailed phase analysis for planning
        /// </summary>
        public string GetPhaseAnalysis(DateTime date)
        {
            var profile = _calculator.GetProfile(date);
            var analysis = $"Biorhythm Analysis for {date:MMMM dd, yyyy}\n";
            analysis += "============================================\n";

            analysis += $"\nPhysical Cycle ({(int)CycleType.Physical} days):\n";
            analysis += FormatCycleInfo(profile.Physical);

            analysis += $"\nEmotional Cycle ({(int)CycleType.Emotional} days):\n";
            analysis += FormatCycleInfo(profile.Emotional);

            analysis += $"\nIntellectual Cycle ({(int)CycleType.Intellectual} days):\n";
            analysis += FormatCycleInfo(profile.Intellectual);

            analysis += $"\nOverall Harmonic Index: {profile.HarmonicIndex:P0}\n";

            return analysis;
        }

        private string FormatCycleInfo(BiorhythmReading reading)
        {
            return $"  Phase: {reading.Phase}\n" +
                   $"  Value: {reading.Value:+0.00;-0.00}\n" +
                   $"  Progress: {reading.PercentageComplete:F1}% ({reading.DaysInCycle}/{reading.DaysInCycle} days)\n";
        }
    }
}
