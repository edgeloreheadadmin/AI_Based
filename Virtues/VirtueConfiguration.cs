using System;
using System.Collections.Generic;
using System.Text;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Advanced configuration and utilities for the Virtues System
    /// Provides scoring algorithms, advanced metrics, and system configuration
    /// </summary>
    public class VirtueConfiguration
    {
        // Virtue weight factors - allows customization of how much each virtue contributes
        public Dictionary<string, double> VirtueWeights = new Dictionary<string, double>
        {
            { "Fortitude", 1.0 },
            { "Chastity", 1.0 },
            { "Diligence", 1.2 },  // Weighted higher - foundation for growth
            { "Grace", 0.9 },
            { "Honesty", 1.3 },    // Weighted higher - enables all other virtues
            { "Patience", 1.0 },
            { "Devotion", 1.5 },   // Weighted highest - integration point
            { "Prayer", 1.1 },
            { "Divinity", 1.2 },
            { "Will", 1.1 },
            { "Deviation", 0.8 },
            { "Sabbath", 1.0 },
            { "Matrimony", 1.3 }   // Weighted high - profound expression of all virtues
        };

        // Thresholds for virtue mastery
        public Dictionary<string, int> MasteryThresholds = new Dictionary<string, int>
        {
            { "Beginner", 25 },
            { "Intermediate", 50 },
            { "Advanced", 75 },
            { "Mastery", 100 },
            { "Exemplar", 150 }
        };

        /// <summary>
        /// Advanced scoring algorithm considering all virtues with weights
        /// </summary>
        public static int CalculateAdvancedHarmonyScore(VirtuesSystem system, VirtueConfiguration config)
        {
            double weightedSum = 0;
            double totalWeight = 0;

            // Fortitude scoring
            if (config.VirtueWeights.TryGetValue("Fortitude", out double fortitudeWeight))
            {
                int fortitudeScore = Math.Max(0, system.GetFortitude().GetState().GetHashCode() % 100);
                weightedSum += fortitudeScore * fortitudeWeight;
                totalWeight += fortitudeWeight;
            }

            // Chastity scoring
            if (config.VirtueWeights.TryGetValue("Chastity", out double chastityWeight))
            {
                int chastityScore = Math.Min(100, system.GetChastity().GetDisciplineStrength());
                weightedSum += chastityScore * chastityWeight;
                totalWeight += chastityWeight;
            }

            // Diligence scoring
            if (config.VirtueWeights.TryGetValue("Diligence", out double diligenceWeight))
            {
                int diligenceScore = Math.Min(100, system.GetDiligence().GetWisdomScore());
                weightedSum += diligenceScore * diligenceWeight;
                totalWeight += diligenceWeight;
            }

            // Grace scoring
            if (config.VirtueWeights.TryGetValue("Grace", out double graceWeight))
            {
                int graceScore = Math.Min(100, system.GetGrace().GetEleganceScore());
                weightedSum += graceScore * graceWeight;
                totalWeight += graceWeight;
            }

            // Honesty scoring
            if (config.VirtueWeights.TryGetValue("Honesty", out double honestyWeight))
            {
                int honestyScore = Math.Min(100, system.GetHonesty().GetIntegrityScore());
                weightedSum += honestyScore * honestyWeight;
                totalWeight += honestyWeight;
            }

            // Patience scoring
            if (config.VirtueWeights.TryGetValue("Patience", out double patienceWeight))
            {
                int patienceScore = Math.Min(100, system.GetPatience().GetMindStillness());
                weightedSum += patienceScore * patienceWeight;
                totalWeight += patienceWeight;
            }

            // Devotion scoring
            if (config.VirtueWeights.TryGetValue("Devotion", out double devotionWeight))
            {
                int devotionScore = Math.Min(100, system.GetDevotion().GetAlignmentLevel());
                weightedSum += devotionScore * devotionWeight;
                totalWeight += devotionWeight;
            }

            // Prayer scoring
            if (config.VirtueWeights.TryGetValue("Prayer", out double prayerWeight))
            {
                int prayerScore = system.GetPrayer().CanWorshipNow() ? 100 : 50;
                weightedSum += prayerScore * prayerWeight;
                totalWeight += prayerWeight;
            }

            // Divinity scoring
            if (config.VirtueWeights.TryGetValue("Divinity", out double divinityWeight))
            {
                int divinityScore = Math.Min(100, system.GetDivinity().GetInterconnectednessScore());
                weightedSum += divinityScore * divinityWeight;
                totalWeight += divinityWeight;
            }

            // Will scoring
            if (config.VirtueWeights.TryGetValue("Will", out double willWeight))
            {
                int willScore = Math.Min(100, system.GetWill().GetWillStrength() / 2);
                weightedSum += willScore * willWeight;
                totalWeight += willWeight;
            }

            // Deviation scoring
            if (config.VirtueWeights.TryGetValue("Deviation", out double deviationWeight))
            {
                int deviationScore = Math.Min(100, system.GetDeviation().GetAdaptationScore());
                weightedSum += deviationScore * deviationWeight;
                totalWeight += deviationWeight;
            }

            // Sabbath scoring
            if (config.VirtueWeights.TryGetValue("Sabbath", out double sabbathWeight))
            {
                int sabbathScore = Math.Min(100, system.GetSabbath().GetTotalRestScore() / 3);
                weightedSum += sabbathScore * sabbathWeight;
                totalWeight += sabbathWeight;
            }

            // Matrimony scoring
            if (config.VirtueWeights.TryGetValue("Matrimony", out double matrimonyWeight))
            {
                int matrimonyScore = Math.Min(100, system.GetMatrimony().GetMatrimonyScore() / 2);
                weightedSum += matrimonyScore * matrimonyWeight;
                totalWeight += matrimonyWeight;
            }

            if (totalWeight == 0) return 0;
            return (int)(weightedSum / totalWeight);
        }

        /// <summary>
        /// Get mastery level for a score
        /// </summary>
        public string GetMasteryLevel(int score)
        {
            if (score >= MasteryThresholds["Exemplar"])
                return "Exemplar";
            if (score >= MasteryThresholds["Mastery"])
                return "Mastery";
            if (score >= MasteryThresholds["Advanced"])
                return "Advanced";
            if (score >= MasteryThresholds["Intermediate"])
                return "Intermediate";
            if (score >= MasteryThresholds["Beginner"])
                return "Beginner";
            return "Novice";
        }

        /// <summary>
        /// Get progress percentage to next level
        /// </summary>
        public double GetProgressToNextLevel(int score)
        {
            if (score >= MasteryThresholds["Exemplar"])
                return 100; // Already at highest

            int currentThreshold = 0;
            int nextThreshold = MasteryThresholds["Exemplar"];

            foreach (var threshold in MasteryThresholds)
            {
                if (score >= threshold.Value && threshold.Value > currentThreshold)
                    currentThreshold = threshold.Value;
                if (score < threshold.Value && threshold.Value < nextThreshold)
                    nextThreshold = threshold.Value;
            }

            if (nextThreshold == currentThreshold)
                return 100;

            double progress = ((double)(score - currentThreshold) / (nextThreshold - currentThreshold)) * 100;
            return Math.Min(100, Math.Max(0, progress));
        }

        /// <summary>
        /// Generate a detailed virtue profile
        /// </summary>
        public static string GenerateVirtueProfile(VirtuesSystem system, VirtueConfiguration config)
        {
            StringBuilder profile = new StringBuilder();

            profile.AppendLine("\n╔════════════════════════════════════════════════════════════╗");
            profile.AppendLine("║              COMPREHENSIVE VIRTUE PROFILE                 ║");
            profile.AppendLine("╚════════════════════════════════════════════════════════════╝\n");

            int advancedScore = CalculateAdvancedHarmonyScore(system, config);
            string masteryLevel = config.GetMasteryLevel(advancedScore);
            double progress = config.GetProgressToNextLevel(advancedScore);

            profile.AppendLine($"Overall Harmony Score: {advancedScore}/150");
            profile.AppendLine($"Mastery Level: {masteryLevel}");
            profile.AppendLine($"Progress to Next Level: {progress:F1}%");
            profile.AppendLine();

            // Individual virtue scores with weights
            profile.AppendLine("Weighted Virtue Scores:");
            profile.AppendLine($"  Fortitude:  {Math.Min(100, system.GetFortitude().GetState().GetHashCode() % 100):D3} (Weight: {config.VirtueWeights["Fortitude"]})");
            profile.AppendLine($"  Chastity:   {Math.Min(100, system.GetChastity().GetDisciplineStrength()):D3} (Weight: {config.VirtueWeights["Chastity"]})");
            profile.AppendLine($"  Diligence:  {Math.Min(100, system.GetDiligence().GetWisdomScore()):D3} (Weight: {config.VirtueWeights["Diligence"]})");
            profile.AppendLine($"  Grace:      {Math.Min(100, system.GetGrace().GetEleganceScore()):D3} (Weight: {config.VirtueWeights["Grace"]})");
            profile.AppendLine($"  Honesty:    {Math.Min(100, system.GetHonesty().GetIntegrityScore()):D3} (Weight: {config.VirtueWeights["Honesty"]})");
            profile.AppendLine($"  Patience:   {Math.Min(100, system.GetPatience().GetMindStillness()):D3} (Weight: {config.VirtueWeights["Patience"]})");
            profile.AppendLine($"  Devotion:   {Math.Min(100, system.GetDevotion().GetAlignmentLevel()):D3} (Weight: {config.VirtueWeights["Devotion"]})");
            profile.AppendLine($"  Prayer:     {(system.GetPrayer().CanWorshipNow() ? 100 : 50):D3} (Weight: {config.VirtueWeights["Prayer"]})");
            profile.AppendLine($"  Divinity:   {Math.Min(100, system.GetDivinity().GetInterconnectednessScore()):D3} (Weight: {config.VirtueWeights["Divinity"]})");
            profile.AppendLine($"  Will:       {Math.Min(100, system.GetWill().GetWillStrength() / 2):D3} (Weight: {config.VirtueWeights["Will"]})");
            profile.AppendLine($"  Deviation:  {Math.Min(100, system.GetDeviation().GetAdaptationScore()):D3} (Weight: {config.VirtueWeights["Deviation"]})");
            profile.AppendLine($"  Sabbath:    {Math.Min(100, system.GetSabbath().GetTotalRestScore() / 3):D3} (Weight: {config.VirtueWeights["Sabbath"]})");
            profile.AppendLine($"  Matrimony:  {Math.Min(100, system.GetMatrimony().GetMatrimonyScore() / 2):D3} (Weight: {config.VirtueWeights["Matrimony"]})");
            profile.AppendLine();

            profile.AppendLine("╔════════════════════════════════════════════════════════════╗");
            profile.AppendLine("║                   END OF PROFILE                         ║");
            profile.AppendLine("╚════════════════════════════════════════════════════════════╝\n");

            return profile.ToString();
        }

        /// <summary>
        /// Recommend areas for focus based on current state
        /// </summary>
        public static List<string> GetRecommendations(VirtuesSystem system, VirtueConfiguration config)
        {
            List<string> recommendations = new List<string>();

            // Check each virtue and provide recommendations
            if (system.GetFortitude().GetState().Contains("Foundation: 0"))
                recommendations.Add("Fortitude: Build your foundation through small acts of courage");

            if (system.GetChastity().GetDisciplineStrength() < 10)
                recommendations.Add("Chastity: Establish one clear personal law and practice holding it");

            if (system.GetDiligence().GetWisdomScore() < 15)
                recommendations.Add("Diligence: Commit to one daily practice for 30 days");

            if (system.GetGrace().GetEleganceScore() < 10)
                recommendations.Add("Grace: Practice forgiving one person (including yourself)");

            if (system.GetHonesty().GetIntegrityScore() < 10)
                recommendations.Add("Honesty: Have one honest conversation you've been avoiding");

            if (system.GetPatience().GetMindStillness() < 5)
                recommendations.Add("Patience: Start with 5 minutes of daily silence");

            if (system.GetDevotion().GetAlignmentLevel() < 5)
                recommendations.Add("Devotion: Work on aligning one dimension (mental, physical, emotional, spiritual)");

            if (!system.GetPrayer().CanWorshipNow())
                recommendations.Add("Prayer: Clear your mind and begin praying for others");

            if (system.GetDivinity().GetInnateWisdomScore() < 15)
                recommendations.Add("Divinity: Reflect on what wisdom you can pass to the next generation");

            if (system.GetWill().GetWillStrength() < 60)
                recommendations.Add("Constitution of Will: Do one thing no one else will do");

            if (system.GetSabbath().GetTotalRestScore() < 50)
                recommendations.Add("Sabbath & Rest: Take one full day of rest this week");

            return recommendations;
        }
    }
}
