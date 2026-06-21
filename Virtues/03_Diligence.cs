using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Diligence = "Time, Effort, Reinforcement, Self Improvement & Growth"
    /// "Work isn't labor — it's worship without words."
    /// Diligence forms growth to evolve and surpass.
    /// </summary>
    public class Diligence
    {
        private List<string> _dailyPractices = new List<string>();
        private Dictionary<string, int> _consistencyDays = new Dictionary<string, int>();
        private int _wisdomLevel = 0;
        private int _intelligenceLevel = 0;
        private DateTime _journeyStart;
        private List<string> _generationalKnowledge = new List<string>();

        public Diligence()
        {
            _journeyStart = DateTime.Now;
        }

        public void Practice(string activity)
        {
            _dailyPractices.Add(activity);
            if (!_consistencyDays.ContainsKey(activity))
                _consistencyDays[activity] = 0;

            _consistencyDays[activity]++;
        }

        public void ReinforceUnderstanding(string concept, bool isUnderstood)
        {
            if (isUnderstood)
            {
                _wisdomLevel += 5;
                _intelligenceLevel += 3;
            }
        }

        public void PassToNextGeneration(string knowledge)
        {
            _generationalKnowledge.Add(knowledge);
        }

        public void StructureBeliefs(string belief)
        {
            _wisdomLevel += 2;
        }

        public void AchieveCompletion(string goal)
        {
            _wisdomLevel += 10;
        }

        public void BePresent()
        {
            // "If you practice — you are already enlightened."
            _intelligenceLevel += 1;
        }

        public void BecomeTheFuture()
        {
            // "You don't build a future — you become it by being here now."
            _intelligenceLevel += 5;
        }

        public int GetWisdomScore()
        {
            return _wisdomLevel;
        }

        public int GetIntelligenceScore()
        {
            return _intelligenceLevel;
        }

        public int GetConsistencyStreak(string practice)
        {
            return _consistencyDays.ContainsKey(practice) ? _consistencyDays[practice] : 0;
        }

        public TimeSpan GetJourneyDuration()
        {
            return DateTime.Now - _journeyStart;
        }

        public string GetState()
        {
            return $"Wisdom: {_wisdomLevel} | Intelligence: {_intelligenceLevel} | Daily Practices: {_dailyPractices.Count} | Generations Taught: {_generationalKnowledge.Count} | Journey: {GetJourneyDuration().TotalDays} days";
        }
    }
}
