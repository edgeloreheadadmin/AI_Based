using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DevelopCognitiveSkillsThroughIntent
{
    public class SkillProfile
    {
        public string SkillName { get; set; }
        public double CurrentProficiency { get; set; }
        public double IntentionStrength { get; set; }
        public int PracticeHours { get; set; }
        public List<double> ProgressHistory { get; set; }
        public DateTime AcquiredDate { get; set; }
        public string Domain { get; set; }
    }

    public class CognitiveIntention
    {
        public string IntentionStatement { get; set; }
        public double Clarity { get; set; }
        public double Motivation { get; set; }
        public double FocusLevel { get; set; }
        public DateTime SetDate { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class PracticeSession
    {
        public string SkillTarget { get; set; }
        public double SessionQuality { get; set; }
        public int MinutesDuration { get; set; }
        public double FeedbackScore { get; set; }
        public string Notes { get; set; }
        public DateTime SessionDate { get; set; }
    }

    public class SkillDevelopmentEngine
    {
        private Dictionary<string, SkillProfile> skillProfiles;
        private List<PracticeSession> practiceSessions;
        private List<CognitiveIntention> activeIntentions;

        public SkillDevelopmentEngine()
        {
            skillProfiles = new Dictionary<string, SkillProfile>();
            practiceSessions = new List<PracticeSession>();
            activeIntentions = new List<CognitiveIntention>();
        }

        public void InitializeSkill(string skillName, string domain)
        {
            if (!skillProfiles.ContainsKey(skillName))
            {
                skillProfiles[skillName] = new SkillProfile
                {
                    SkillName = skillName,
                    CurrentProficiency = 0.1,
                    IntentionStrength = 0.0,
                    PracticeHours = 0,
                    ProgressHistory = new List<double> { 0.1 },
                    AcquiredDate = DateTime.Now,
                    Domain = domain
                };
            }
        }

        public void SetCognitiveIntention(string skillName, double clarity, double motivation)
        {
            var intention = new CognitiveIntention
            {
                IntentionStatement = $"Master {skillName} with deliberate practice",
                Clarity = Math.Min(clarity, 1.0),
                Motivation = Math.Min(motivation, 1.0),
                FocusLevel = (clarity + motivation) / 2.0,
                SetDate = DateTime.Now,
                Duration = TimeSpan.FromHours(8)
            };
            activeIntentions.Add(intention);
        }

        public void RecordPracticeSession(string skillName, int minutes, double quality, double feedback)
        {
            if (skillProfiles.ContainsKey(skillName))
            {
                var session = new PracticeSession
                {
                    SkillTarget = skillName,
                    SessionQuality = Math.Min(quality, 1.0),
                    MinutesDuration = minutes,
                    FeedbackScore = Math.Min(feedback, 1.0),
                    Notes = $"Practice session for {skillName}",
                    SessionDate = DateTime.Now
                };
                practiceSessions.Add(session);
                UpdateSkillProficiency(skillName, minutes, quality, feedback);
            }
        }

        private void UpdateSkillProficiency(string skillName, int minutes, double quality, double feedback)
        {
            var profile = skillProfiles[skillName];
            double intentionBoost = 1.0;

            var relevantIntention = activeIntentions.FirstOrDefault(i =>
                i.IntentionStatement.Contains(skillName));

            if (relevantIntention != null)
            {
                intentionBoost = 1.0 + (relevantIntention.FocusLevel * 0.5);
            }

            double improvement = (minutes / 60.0) * (0.05 + quality * 0.15 + feedback * 0.1) * intentionBoost;
            profile.CurrentProficiency = Math.Min(profile.CurrentProficiency + improvement, 0.99);
            profile.PracticeHours += minutes / 60;
            profile.ProgressHistory.Add(profile.CurrentProficiency);

            if (relevantIntention != null)
            {
                relevantIntention.Motivation = Math.Min(relevantIntention.Motivation + 0.02, 1.0);
            }
        }

        public void DisplaySkillStatus(string skillName)
        {
            if (skillProfiles.ContainsKey(skillName))
            {
                var profile = skillProfiles[skillName];
                Console.WriteLine($"\n  Skill: {profile.SkillName} ({profile.Domain})");
                Console.WriteLine($"  Proficiency: {profile.CurrentProficiency * 100:F1}%");
                Console.WriteLine($"  Practice Hours: {profile.PracticeHours:F1}");
                Console.WriteLine($"  Sessions Recorded: {practiceSessions.Count(p => p.SkillTarget == skillName)}");
            }
        }

        public double CalculateLearningCurve(string skillName)
        {
            if (skillProfiles.ContainsKey(skillName))
            {
                var profile = skillProfiles[skillName];
                if (profile.ProgressHistory.Count < 2) return 0.0;

                double totalImprovement = profile.ProgressHistory.Last() - profile.ProgressHistory.First();
                double averageImprovement = totalImprovement / profile.ProgressHistory.Count;
                return averageImprovement;
            }
            return 0.0;
        }

        public Dictionary<string, double> GetSkillRankings()
        {
            return skillProfiles.OrderByDescending(x => x.Value.CurrentProficiency)
                .ToDictionary(x => x.Key, x => x.Value.CurrentProficiency);
        }

        public void DisplayIntentionAnalysis()
        {
            Console.WriteLine("\n  Active Intentions:");
            foreach (var intention in activeIntentions)
            {
                Console.WriteLine($"    Intent: {intention.IntentionStatement}");
                Console.WriteLine($"    Clarity: {intention.Clarity * 100:F0}% | Motivation: {intention.Motivation * 100:F0}% | Focus: {intention.FocusLevel * 100:F0}%");
            }
        }

        public List<string> RecommendNextPractices()
        {
            var recommendations = new List<string>();
            var lowestSkills = skillProfiles.OrderBy(x => x.Value.CurrentProficiency).Take(3);

            foreach (var skill in lowestSkills)
            {
                recommendations.Add($"Focus on {skill.Key}: currently at {skill.Value.CurrentProficiency * 100:F1}%");
            }
            return recommendations;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Cognitive Skills Development Through Intent and Practice      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new SkillDevelopmentEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Skill Initialization Through Intent]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.InitializeSkill("MemoryPalace", "Cognitive");
        engine.InitializeSkill("FastReading", "Perceptual");
        engine.InitializeSkill("MentalArithmetic", "Analytical");
        engine.InitializeSkill("LanguageAcquisition", "Linguistic");

        Console.WriteLine("  ✓ Initialized 4 cognitive skills");
        Console.WriteLine("  ✓ Each skill begins at baseline 10% proficiency");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Setting Cognitive Intentions with Clarity]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.SetCognitiveIntention("MemoryPalace", clarity: 0.95, motivation: 0.85);
        engine.SetCognitiveIntention("FastReading", clarity: 0.88, motivation: 0.72);
        engine.SetCognitiveIntention("MentalArithmetic", clarity: 0.92, motivation: 0.78);

        Console.WriteLine("  ✓ Set 3 cognitive intentions with varying clarity/motivation levels");
        Console.WriteLine("  ✓ Intentions provide focus boost to practice sessions");
        engine.DisplayIntentionAnalysis();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Practice Sessions with Feedback Integration]");
        Console.ResetColor();
        Thread.Sleep(500);

        var practiceData = new[] {
            ("MemoryPalace", 45, 0.9, 0.85),
            ("MemoryPalace", 60, 0.92, 0.90),
            ("FastReading", 30, 0.75, 0.70),
            ("FastReading", 45, 0.82, 0.80),
            ("MentalArithmetic", 50, 0.88, 0.85),
            ("LanguageAcquisition", 40, 0.80, 0.78)
        };

        foreach (var (skill, minutes, quality, feedback) in practiceData)
        {
            engine.RecordPracticeSession(skill, minutes, quality, feedback);
            Console.WriteLine($"  ✓ {skill}: {minutes} min session (Quality: {quality * 100:F0}%, Feedback: {feedback * 100:F0}%)");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Learning Curve Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var skills = new[] { "MemoryPalace", "FastReading", "MentalArithmetic", "LanguageAcquisition" };
        foreach (var skill in skills)
        {
            double curve = engine.CalculateLearningCurve(skill);
            Console.WriteLine($"  {skill}: Learning rate = {curve * 100:F2}% per session");
            engine.DisplaySkillStatus(skill);
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Skill Rankings and Progress Comparison]");
        Console.ResetColor();
        Thread.Sleep(500);

        var rankings = engine.GetSkillRankings();
        int rank = 1;
        foreach (var kvp in rankings)
        {
            Console.WriteLine($"  #{rank}: {kvp.Key} - {kvp.Value * 100:F1}% proficiency");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Adaptive Recommendation System]");
        Console.ResetColor();
        Thread.Sleep(500);

        var recommendations = engine.RecommendNextPractices();
        Console.WriteLine("  Next Focus Areas:");
        foreach (var rec in recommendations)
        {
            Console.WriteLine($"    • {rec}");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Intent-Based Skill Amplification Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Intent Amplification Factors:");
        Console.WriteLine("    • Clear intention: +0% to +50% improvement rate");
        Console.WriteLine("    • Strong motivation: +0% to +25% retention bonus");
        Console.WriteLine("    • Combined focus effect: +20% to +75% overall acceleration");
        Console.WriteLine("  ");
        Console.WriteLine("  Cognitive Development Pipeline:");
        Console.WriteLine("    1. Set clear intention → Focus attention → Select skill");
        Console.WriteLine("    2. Practice with quality → Receive feedback → Update proficiency");
        Console.WriteLine("    3. Track progress → Adjust motivation → Continue deliberate practice");
        Console.WriteLine("    4. Achieve mastery → Form new intentions → Expand skill set");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Cognitive skills development system complete");
        Console.ResetColor();
    }
}
