using System;
using System.Collections.Generic;
using System.Linq;

namespace BiorhythmDecisionSystem
{
    /// <summary>
    /// Psychological mindset types aligned with biorhythmic patterns
    /// </summary>
    public enum MindsetType
    {
        AnalyticalLogical,      // High intellectual, moderate physical/emotional
        CreativeExpressive,     // High emotional, high intellectual
        ActionOriented,         // High physical, moderate emotional
        EmotionalIntuitive,     // High emotional, low intellectual
        BalancedHarmonious,     // All cycles in harmony
        LowEnergyReflective,    // All cycles low - introspection time
        StressedFractured,      // Conflicting cycles
        FocusedDetermined,      // High intellectual, low emotional
        PlayfulSpontaneous,     // High physical and emotional, low intellectual
        CautiousConservative    // Low physical, all cycles declining
    }

    /// <summary>
    /// Represents a psychological mindset state
    /// </summary>
    public class MindsetState
    {
        public MindsetType Type { get; set; }
        public double Intensity { get; set; } // 0-1 scale
        public double Stability { get; set; } // 0-1 scale (how consistent the mindset is)
        public List<string> Characteristics { get; set; }
        public List<string> StrengthAreas { get; set; }
        public List<string> WeakAreas { get; set; }
        public List<string> RecommendedActivities { get; set; }
        public List<string> ActivitiesToAvoid { get; set; }
        public int TransitionDaysUntilNext { get; set; }
    }

    /// <summary>
    /// Analyzes biorhythm profile and determines current mindset alignment
    /// </summary>
    public class MindsetAlignmentAnalyzer
    {
        private readonly BiorhythmCalculator _calculator;

        public MindsetAlignmentAnalyzer(DateTime birthDate)
        {
            _calculator = new BiorhythmCalculator(birthDate);
        }

        /// <summary>
        /// Analyze biorhythm profile and determine current mindset
        /// </summary>
        public MindsetState AnalyzeMindset(DateTime targetDate)
        {
            var profile = _calculator.GetProfile(targetDate);
            var mindsetType = DetermineMindsetType(profile);

            return BuildMindsetState(mindsetType, profile, targetDate);
        }

        private MindsetType DetermineMindsetType(BiorhythmProfile profile)
        {
            double phys = profile.Physical.Value;
            double emot = profile.Emotional.Value;
            double intel = profile.Intellectual.Value;

            // Check for critical/fractured state
            int criticalCount = 0;
            if (profile.Physical.Phase == CyclePhase.Critical) criticalCount++;
            if (profile.Emotional.Phase == CyclePhase.Critical) criticalCount++;
            if (profile.Intellectual.Phase == CyclePhase.Critical) criticalCount++;

            if (criticalCount >= 2)
                return MindsetType.StressedFractured;

            // All low - reflective
            if (phys < 0 && emot < 0 && intel < 0)
                return MindsetType.LowEnergyReflective;

            // Harmonic alignment
            if (Math.Abs(phys - emot) < 0.3 && Math.Abs(emot - intel) < 0.3 && Math.Abs(phys - intel) < 0.3)
            {
                if (phys > 0.3)
                    return MindsetType.BalancedHarmonious;
            }

            // Analytical - high intellectual, moderate physical/emotional
            if (intel > 0.5 && phys > -0.3 && emot > -0.3 && intel > phys && intel > emot)
                return MindsetType.AnalyticalLogical;

            // Creative - high emotional and intellectual
            if (emot > 0.4 && intel > 0.4)
                return MindsetType.CreativeExpressive;

            // Action oriented - high physical, moderate others
            if (phys > 0.5 && intel < 0.7 && emot > -0.3)
                return MindsetType.ActionOriented;

            // Emotional intuitive - high emotional, low intellectual
            if (emot > 0.5 && intel < 0.2)
                return MindsetType.EmotionalIntuitive;

            // Focused determined - high intellectual, low emotional
            if (intel > 0.5 && emot < -0.2 && phys > 0)
                return MindsetType.FocusedDetermined;

            // Playful spontaneous - high physical and emotional, low intellectual
            if (phys > 0.4 && emot > 0.4 && intel < 0.3)
                return MindsetType.PlayfulSpontaneous;

            // Cautious conservative - low physical, declining
            if (phys < -0.3 && emot < -0.2)
                return MindsetType.CautiousConservative;

            // Default to analytical
            return MindsetType.AnalyticalLogical;
        }

        private MindsetState BuildMindsetState(MindsetType type, BiorhythmProfile profile, DateTime targetDate)
        {
            var state = new MindsetState
            {
                Type = type,
                Characteristics = GetCharacteristics(type, profile),
                StrengthAreas = GetStrengthAreas(type, profile),
                WeakAreas = GetWeakAreas(type, profile),
                RecommendedActivities = GetRecommendedActivities(type),
                ActivitiesToAvoid = GetActivitiesToAvoid(type),
                TransitionDaysUntilNext = CalculateTransitionDays(profile)
            };

            // Calculate intensity (how pronounced this mindset is)
            state.Intensity = CalculateMindsetIntensity(type, profile);

            // Calculate stability (how consistent/stable this state is)
            state.Stability = CalculateMindsetStability(profile);

            return state;
        }

        private List<string> GetCharacteristics(MindsetType type, BiorhythmProfile profile)
        {
            var chars = new Dictionary<MindsetType, List<string>>
            {
                { MindsetType.AnalyticalLogical, new List<string> { "Clear thinking", "Problem-focused", "Detail-oriented", "Systematic approach" } },
                { MindsetType.CreativeExpressive, new List<string> { "Imaginative", "Expressive", "Intuitive", "Open to new ideas" } },
                { MindsetType.ActionOriented, new List<string> { "Energetic", "Proactive", "Decisive", "Physical vitality" } },
                { MindsetType.EmotionalIntuitive, new List<string> { "Empathetic", "Intuitive", "Relationship-focused", "Emotionally aware" } },
                { MindsetType.BalancedHarmonious, new List<string> { "Centered", "Balanced", "Integrated", "At peace" } },
                { MindsetType.LowEnergyReflective, new List<string> { "Introspective", "Contemplative", "Restorative", "Thoughtful" } },
                { MindsetType.StressedFractured, new List<string> { "Scattered", "Conflicted", "Overwhelmed", "Unstable" } },
                { MindsetType.FocusedDetermined, new List<string> { "Concentrated", "Purposeful", "Driven", "Serious" } },
                { MindsetType.PlayfulSpontaneous, new List<string> { "Lighthearted", "Spontaneous", "Fun-loving", "Quick-moving" } },
                { MindsetType.CautiousConservative, new List<string> { "Cautious", "Risk-averse", "Protective", "Conservative" } }
            };

            return chars.ContainsKey(type) ? chars[type] : new List<string> { "Unique blend" };
        }

        private List<string> GetStrengthAreas(MindsetType type, BiorhythmProfile profile)
        {
            var strengths = new Dictionary<MindsetType, List<string>>
            {
                { MindsetType.AnalyticalLogical, new List<string> { "Problem-solving", "Technical work", "Research", "Strategy" } },
                { MindsetType.CreativeExpressive, new List<string> { "Art & design", "Innovation", "Writing", "Communication" } },
                { MindsetType.ActionOriented, new List<string> { "Physical activities", "Execution", "Leadership", "Competition" } },
                { MindsetType.EmotionalIntuitive, new List<string> { "Counseling", "Negotiation", "Team harmony", "Mentoring" } },
                { MindsetType.BalancedHarmonious, new List<string> { "Holistic thinking", "Integration", "Decision-making", "Wisdom" } },
                { MindsetType.LowEnergyReflective, new List<string> { "Meditation", "Planning", "Self-assessment", "Learning" } },
                { MindsetType.StressedFractured, new List<string> { "Adaptability", "Flexibility", "Crisis management" } },
                { MindsetType.FocusedDetermined, new List<string> { "Deep work", "Specialization", "Excellence", "Mastery" } },
                { MindsetType.PlayfulSpontaneous, new List<string> { "Brainstorming", "Networking", "Entertainment", "Exploration" } },
                { MindsetType.CautiousConservative, new List<string> { "Risk assessment", "Preservation", "Stability", "Continuity" } }
            };

            return strengths.ContainsKey(type) ? strengths[type] : new List<string> { "Unique strengths" };
        }

        private List<string> GetWeakAreas(MindsetType type, BiorhythmProfile profile)
        {
            var weaknesses = new Dictionary<MindsetType, List<string>>
            {
                { MindsetType.AnalyticalLogical, new List<string> { "Emotional intelligence", "Spontaneity", "Physical engagement" } },
                { MindsetType.CreativeExpressive, new List<string> { "Logical analysis", "Practical execution", "Attention to detail" } },
                { MindsetType.ActionOriented, new List<string> { "Reflection", "Deep analysis", "Emotional consideration" } },
                { MindsetType.EmotionalIntuitive, new List<string> { "Logical reasoning", "Technical skills", "Physical endurance" } },
                { MindsetType.BalancedHarmonious, new List<string> { "Sharp focus", "Decisive action", "Rapid response" } },
                { MindsetType.LowEnergyReflective, new List<string> { "Action", "Physical tasks", "Quick decisions" } },
                { MindsetType.StressedFractured, new List<string> { "Clarity", "Stability", "Confidence" } },
                { MindsetType.FocusedDetermined, new List<string> { "Relaxation", "Socializing", "Play" } },
                { MindsetType.PlayfulSpontaneous, new List<string> { "Depth", "Commitment", "Serious focus" } },
                { MindsetType.CautiousConservative, new List<string> { "Innovation", "Taking risks", "Bold action" } }
            };

            return weaknesses.ContainsKey(type) ? weaknesses[type] : new List<string> { "Areas to develop" };
        }

        private List<string> GetRecommendedActivities(MindsetType type)
        {
            var activities = new Dictionary<MindsetType, List<string>>
            {
                { MindsetType.AnalyticalLogical, new List<string> { "Coding", "Data analysis", "Strategic planning", "Research", "Problem solving" } },
                { MindsetType.CreativeExpressive, new List<string> { "Artistic projects", "Writing", "Music", "Design", "Brainstorming" } },
                { MindsetType.ActionOriented, new List<string> { "Exercise", "Sports", "Outdoor activities", "Project execution", "Sales/pitching" } },
                { MindsetType.EmotionalIntuitive, new List<string> { "Mentoring", "Team meetings", "One-on-ones", "Listening", "Counseling" } },
                { MindsetType.BalancedHarmonious, new List<string> { "Major decisions", "Integration work", "Meditation", "Holistic planning" } },
                { MindsetType.LowEnergyReflective, new List<string> { "Journaling", "Meditation", "Light reading", "Planning", "Self-assessment" } },
                { MindsetType.StressedFractured, new List<string> { "Breathing exercises", "Grounding activities", "Postpone decisions", "Seek support" } },
                { MindsetType.FocusedDetermined, new List<string> { "Deep work", "Learning", "Specialized tasks", "Difficult projects" } },
                { MindsetType.PlayfulSpontaneous, new List<string> { "Networking", "Social events", "Games", "Exploration", "Tryouts" } },
                { MindsetType.CautiousConservative, new List<string> { "Risk assessment", "Documentation", "Backup planning", "Maintenance" } }
            };

            return activities.ContainsKey(type) ? activities[type] : new List<string> { "Varied activities" };
        }

        private List<string> GetActivitiesToAvoid(MindsetType type)
        {
            var toAvoid = new Dictionary<MindsetType, List<string>>
            {
                { MindsetType.AnalyticalLogical, new List<string> { "Spontaneous decisions", "Emotional confrontations", "Physical risks" } },
                { MindsetType.CreativeExpressive, new List<string> { "Strict deadlines", "Tedious detail work", "Rigid processes" } },
                { MindsetType.ActionOriented, new List<string> { "Extended meetings", "Paperwork", "Waiting/delays" } },
                { MindsetType.EmotionalIntuitive, new List<string> { "Cold logic tasks", "Isolated work", "Confrontation" } },
                { MindsetType.BalancedHarmonious, new List<string> { "None - good for most activities" } },
                { MindsetType.LowEnergyReflective, new List<string> { "Strenuous activities", "Major decisions", "High-pressure tasks" } },
                { MindsetType.StressedFractured, new List<string> { "Major decisions", "Important meetings", "Risky activities", "Commitment" } },
                { MindsetType.FocusedDetermined, new List<string> { "Interruptions", "Socializing", "Distractions" } },
                { MindsetType.PlayfulSpontaneous, new List<string> { "Boring tasks", "Serious meetings", "Long focus periods" } },
                { MindsetType.CautiousConservative, new List<string> { "Major changes", "Risky ventures", "Bold experiments" } }
            };

            return toAvoid.ContainsKey(type) ? toAvoid[type] : new List<string> { "Assess carefully" };
        }

        private double CalculateMindsetIntensity(MindsetType type, BiorhythmProfile profile)
        {
            double phys = profile.Physical.Value;
            double emot = profile.Emotional.Value;
            double intel = profile.Intellectual.Value;

            return type switch
            {
                MindsetType.AnalyticalLogical => (Math.Abs(intel) + Math.Abs(phys)) / 2,
                MindsetType.CreativeExpressive => (Math.Abs(emot) + Math.Abs(intel)) / 2,
                MindsetType.ActionOriented => Math.Abs(phys),
                MindsetType.EmotionalIntuitive => Math.Abs(emot),
                MindsetType.BalancedHarmonious => 1.0 - Math.Abs((phys + emot + intel) / 3),
                MindsetType.LowEnergyReflective => 1.0 - (Math.Abs(phys) + Math.Abs(emot) + Math.Abs(intel)) / 3,
                MindsetType.StressedFractured => (Math.Abs(phys - emot) + Math.Abs(emot - intel)) / 2,
                MindsetType.FocusedDetermined => Math.Abs(intel),
                MindsetType.PlayfulSpontaneous => (Math.Abs(phys) + Math.Abs(emot)) / 2,
                MindsetType.CautiousConservative => Math.Abs(Math.Min(phys, emot)),
                _ => 0.5
            };
        }

        private double CalculateMindsetStability(BiorhythmProfile profile)
        {
            // Stability is inversely related to cycle conflicts
            double conflict = Math.Abs(profile.Physical.Value - profile.Emotional.Value) +
                             Math.Abs(profile.Emotional.Value - profile.Intellectual.Value) +
                             Math.Abs(profile.Physical.Value - profile.Intellectual.Value);

            return 1.0 - (conflict / 6.0); // Normalize to 0-1
        }

        private int CalculateTransitionDays(BiorhythmProfile profile)
        {
            // Find when the next major mindset shift occurs (when two cycles realign)
            int daysUntilShift = int.MaxValue;

            for (int i = 1; i <= 60; i++)
            {
                var futureDate = DateTime.Now.AddDays(i);
                var futureProfile = _calculator.GetProfile(futureDate);

                // Check if we're transitioning to a new mindset (major phase changes)
                int phaseChanges = 0;
                if (profile.Physical.Phase != futureProfile.Physical.Phase) phaseChanges++;
                if (profile.Emotional.Phase != futureProfile.Emotional.Phase) phaseChanges++;
                if (profile.Intellectual.Phase != futureProfile.Intellectual.Phase) phaseChanges++;

                if (phaseChanges >= 2)
                {
                    daysUntilShift = i;
                    break;
                }
            }

            return daysUntilShift == int.MaxValue ? 30 : daysUntilShift;
        }
    }

    /// <summary>
    /// Mindset-aware decision making that aligns choices with current psychological state
    /// </summary>
    public class MindsetAlignedDecisionMaker
    {
        private readonly BiorhythmicDecisionEngine _decisionEngine;
        private readonly MindsetAlignmentAnalyzer _mindsetAnalyzer;

        public MindsetAlignedDecisionMaker(DateTime birthDate)
        {
            _decisionEngine = new BiorhythmicDecisionEngine(birthDate);
            _mindsetAnalyzer = new MindsetAlignmentAnalyzer(birthDate);
        }

        /// <summary>
        /// Get mindset-aligned decision recommendation
        /// </summary>
        public (DecisionRecommendation decision, MindsetState mindset) GetMindsetAlignedRecommendation(
            string decisionType, DateTime targetDate)
        {
            var mindset = _mindsetAnalyzer.AnalyzeMindset(targetDate);
            var decision = _decisionEngine.GetDecisionRecommendation(decisionType, targetDate);

            // Adjust recommendation based on mindset alignment
            double mindsetAdjustment = CalculateMindsetAlignment(decisionType, mindset);
            decision.RecommendationScore = decision.RecommendationScore * (0.7 + (0.3 * mindsetAdjustment));

            // Add mindset insights
            var mindsetInsights = GenerateMindsetInsights(decisionType, mindset);
            decision.Warnings.InsertRange(0, mindsetInsights);

            return (decision, mindset);
        }

        /// <summary>
        /// Get recommendations for activities matching current mindset
        /// </summary>
        public List<(string activity, double compatibility)> GetMindsetOptimalActivities(DateTime targetDate)
        {
            var mindset = _mindsetAnalyzer.AnalyzeMindset(targetDate);
            var activities = mindset.RecommendedActivities.Select(a =>
                (a, compatibility: mindset.Intensity * mindset.Stability)).ToList();

            return activities.OrderByDescending(a => a.compatibility).ToList();
        }

        private double CalculateMindsetAlignment(string decisionType, MindsetState mindset)
        {
            // Score how well the current mindset aligns with the decision type
            var alignmentScores = new Dictionary<MindsetType, Dictionary<string, double>>
            {
                {
                    MindsetType.AnalyticalLogical, new Dictionary<string, double>
                    {
                        { "analytical", 1.0 }, { "business", 0.9 }, { "health", 0.7 },
                        { "creative", 0.3 }, { "athletic", 0.4 }, { "interpersonal", 0.5 }
                    }
                },
                {
                    MindsetType.CreativeExpressive, new Dictionary<string, double>
                    {
                        { "creative", 1.0 }, { "interpersonal", 0.8 }, { "business", 0.6 },
                        { "analytical", 0.4 }, { "athletic", 0.5 }, { "health", 0.6 }
                    }
                },
                {
                    MindsetType.ActionOriented, new Dictionary<string, double>
                    {
                        { "athletic", 1.0 }, { "business", 0.85 }, { "health", 0.8 },
                        { "creative", 0.5 }, { "analytical", 0.4 }, { "interpersonal", 0.7 }
                    }
                },
                {
                    MindsetType.EmotionalIntuitive, new Dictionary<string, double>
                    {
                        { "interpersonal", 1.0 }, { "creative", 0.85 }, { "health", 0.8 },
                        { "analytical", 0.3 }, { "business", 0.6 }, { "athletic", 0.5 }
                    }
                },
                {
                    MindsetType.BalancedHarmonious, new Dictionary<string, double>
                    {
                        { "business", 1.0 }, { "health", 0.95 }, { "analytical", 0.9 },
                        { "creative", 0.9 }, { "athletic", 0.9 }, { "interpersonal", 0.95 }
                    }
                },
                {
                    MindsetType.LowEnergyReflective, new Dictionary<string, double>
                    {
                        { "health", 0.8 }, { "analytical", 0.7 }, { "creative", 0.6 },
                        { "business", 0.4 }, { "athletic", 0.2 }, { "interpersonal", 0.5 }
                    }
                },
                {
                    MindsetType.StressedFractured, new Dictionary<string, double>
                    {
                        { "health", 0.9 }, { "creative", 0.5 }, { "interpersonal", 0.4 },
                        { "business", 0.2 }, { "analytical", 0.3 }, { "athletic", 0.3 }
                    }
                },
                {
                    MindsetType.FocusedDetermined, new Dictionary<string, double>
                    {
                        { "analytical", 1.0 }, { "business", 0.9 }, { "athletic", 0.8 },
                        { "creative", 0.4 }, { "interpersonal", 0.5 }, { "health", 0.6 }
                    }
                },
                {
                    MindsetType.PlayfulSpontaneous, new Dictionary<string, double>
                    {
                        { "creative", 0.95 }, { "athletic", 0.9 }, { "interpersonal", 0.9 },
                        { "business", 0.5 }, { "analytical", 0.3 }, { "health", 0.6 }
                    }
                },
                {
                    MindsetType.CautiousConservative, new Dictionary<string, double>
                    {
                        { "business", 0.8 }, { "health", 0.9 }, { "analytical", 0.7 },
                        { "creative", 0.3 }, { "athletic", 0.4 }, { "interpersonal", 0.6 }
                    }
                }
            };

            if (alignmentScores.TryGetValue(mindset.Type, out var scores))
            {
                if (scores.TryGetValue(decisionType.ToLower(), out var score))
                    return score;
            }

            return 0.5; // Neutral alignment
        }

        private List<string> GenerateMindsetInsights(string decisionType, MindsetState mindset)
        {
            var insights = new List<string>();

            // Stability warning
            if (mindset.Stability < 0.4)
                insights.Add($"⚠️ MINDSET ALERT: Low stability - avoid major {decisionType} decisions today");
            else if (mindset.Stability > 0.8)
                insights.Add($"✓ MINDSET ADVANTAGE: High stability for {decisionType} decisions");

            // Intensity indicator
            if (mindset.Intensity > 0.85)
                insights.Add($"🔥 Peak {mindset.Type} mindset - excellent opportunity for {decisionType}");
            else if (mindset.Intensity < 0.3)
                insights.Add($"💡 Subtle {mindset.Type} mindset - approach carefully");

            // Transition warning
            if (mindset.TransitionDaysUntilNext < 3)
                insights.Add($"📊 Mindset shift incoming in {mindset.TransitionDaysUntilNext} days");

            return insights;
        }
    }

    /// <summary>
    /// Tracks mindset evolution over time for pattern recognition
    /// </summary>
    public class MindsetTracker
    {
        private readonly List<(DateTime date, MindsetState state)> _history;
        private readonly MindsetAlignmentAnalyzer _analyzer;

        public MindsetTracker(DateTime birthDate)
        {
            _analyzer = new MindsetAlignmentAnalyzer(birthDate);
            _history = new List<(DateTime, MindsetState)>();
        }

        /// <summary>
        /// Record current mindset
        /// </summary>
        public void RecordMindset(DateTime date)
        {
            var mindset = _analyzer.AnalyzeMindset(date);
            _history.Add((date, mindset));
        }

        /// <summary>
        /// Get mindset frequency patterns
        /// </summary>
        public Dictionary<MindsetType, int> GetMindsetFrequency(int days = 60)
        {
            var cutoffDate = DateTime.Now.AddDays(-days);
            var frequencies = new Dictionary<MindsetType, int>();

            foreach (var (date, mindset) in _history.Where(h => h.date >= cutoffDate))
            {
                if (frequencies.ContainsKey(mindset.Type))
                    frequencies[mindset.Type]++;
                else
                    frequencies[mindset.Type] = 1;
            }

            return frequencies;
        }

        /// <summary>
        /// Get dominant mindsets by day of week
        /// </summary>
        public Dictionary<DayOfWeek, MindsetType> GetMindsetsByDayOfWeek()
        {
            var mindsetByDay = new Dictionary<DayOfWeek, List<MindsetType>>();

            foreach (var (date, mindset) in _history)
            {
                var dayOfWeek = date.DayOfWeek;
                if (!mindsetByDay.ContainsKey(dayOfWeek))
                    mindsetByDay[dayOfWeek] = new List<MindsetType>();
                mindsetByDay[dayOfWeek].Add(mindset.Type);
            }

            var result = new Dictionary<DayOfWeek, MindsetType>();
            foreach (var (day, mindsets) in mindsetByDay)
            {
                result[day] = mindsets.GroupBy(m => m)
                    .OrderByDescending(g => g.Count())
                    .First()
                    .Key;
            }

            return result;
        }

        /// <summary>
        /// Predict optimal mindset for an activity based on history
        /// </summary>
        public MindsetType GetOptimalMindsetForActivity(string activity)
        {
            // Simplified prediction - in production would use ML
            var mindsetActivityMap = new Dictionary<string, MindsetType>
            {
                { "coding", MindsetType.FocusedDetermined },
                { "design", MindsetType.CreativeExpressive },
                { "exercise", MindsetType.ActionOriented },
                { "meeting", MindsetType.BalancedHarmonious },
                { "writing", MindsetType.CreativeExpressive },
                { "planning", MindsetType.AnalyticalLogical },
                { "relaxation", MindsetType.LowEnergyReflective },
                { "presentation", MindsetType.ActionOriented },
                { "brainstorm", MindsetType.PlayfulSpontaneous },
                { "analysis", MindsetType.AnalyticalLogical }
            };

            return mindsetActivityMap.TryGetValue(activity.ToLower(), out var mindset)
                ? mindset
                : MindsetType.BalancedHarmonious;
        }

        /// <summary>
        /// Get personalized schedule recommendations
        /// </summary>
        public string GetOptimizedScheduleRecommendation(DateTime startDate, int days = 7)
        {
            var schedule = "=== Optimized Weekly Schedule ===\n\n";

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var mindset = _analyzer.AnalyzeMindset(date);

                schedule += $"{date:ddd MMM dd}\n";
                schedule += $"Mindset: {mindset.Type}\n";
                schedule += $"Recommended:\n";
                foreach (var activity in mindset.RecommendedActivities.Take(3))
                    schedule += $"  • {activity}\n";
                schedule += "\n";
            }

            return schedule;
        }
    }
}
