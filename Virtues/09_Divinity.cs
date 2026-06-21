using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Divinity = "The Future is the next generations that are the seeds to protect and watch grow and prosper"
    /// Divinity is interconnectedness, understanding, comprehension across generations
    /// "Divinity is not a destination. It's your birth certificate."
    /// "You don't pray to become divine. You understand and develop innate wisdom"
    /// </summary>
    public class Divinity
    {
        private List<string> _nextGenerationKnowledge = new List<string>();
        private Dictionary<string, int> _inheritedWisdom = new Dictionary<string, int>();
        private int _interconnectednessLevel = 0;
        private int _innateWisdomLevel = 0;
        private List<string> _growthAcrossBiorhythms = new List<string>();
        private bool _selfEffortApplied = false;
        private bool _sinWashedAway = false;
        private int _generationalGrowth = 0;
        private DateTime _birthMoment = DateTime.Now;

        public void ProtectNextGeneration(string knowledge, string guidance)
        {
            _nextGenerationKnowledge.Add($"{knowledge} - {guidance}");
        }

        public void WatchGrowth(string seedName, bool thriving)
        {
            if (thriving)
                _generationalGrowth += 10;
        }

        public void InheritWisdom(string wisdomType, int magnitude)
        {
            if (!_inheritedWisdom.ContainsKey(wisdomType))
                _inheritedWisdom[wisdomType] = 0;

            _inheritedWisdom[wisdomType] += magnitude;
            _interconnectednessLevel += 2;
        }

        public void UnderstandInterconnectedness()
        {
            // "Divinity is not belief — it's interconnectedness, understanding, comprehension"
            _interconnectednessLevel += 20;
        }

        public void DevelopInnateWisdom(string concept, bool understood)
        {
            if (understood)
            {
                _innateWisdomLevel += 5;
                _interconnectednessLevel += 2;
            }
        }

        public void SurroundBiologicalLimits(string limitation, bool surpassed)
        {
            if (surpassed)
                _innateWisdomLevel += 15;
        }

        public void RecognizeWill()
        {
            // "recognized not through oneself but through the will itself when understood"
            _interconnectednessLevel += 10;
        }

        public void UnderstandSoulSpiritConnection()
        {
            // "The soul and the spirit are interconnected"
            _interconnectednessLevel += 15;
        }

        public void ApplySelfEffort()
        {
            _selfEffortApplied = true;
            _innateWisdomLevel += 20;
        }

        public void WashAwayNeglect()
        {
            // "Salvation comes through ones own effort, saving oneself, washing away neglect and sin"
            _sinWashedAway = true;
            _innateWisdomLevel += 10;
        }

        public void DevelopDiscipline(string discipline)
        {
            _growthAcrossBiorhythms.Add($"Discipline: {discipline}");
            _innateWisdomLevel += 5;
        }

        public void EvolveBeyondBiorhythms()
        {
            // "biorhythmic changes, the timing, scheduling, events outcomes become understood and evolve"
            _generationalGrowth += 20;
        }

        public void BuildFoundationalTruths()
        {
            // "The foundational truths/forms of honesty and forms of logic are built"
            _innateWisdomLevel += 25;
        }

        public void ReinforceElementary()
        {
            // "elementary logic is built upon and constantly and deeply reinforced"
            _innateWisdomLevel += 10;
        }

        public void AllowMiraclesNaturally()
        {
            // "You don't ask for miracles — you allow them to form naturally"
            _generationalGrowth += 15;
        }

        public void BornInTheMoment()
        {
            // "You were born in the moment — you didn't choose to come here"
            _interconnectednessLevel += 5;
        }

        public int GetInnateWisdomScore()
        {
            return _innateWisdomLevel;
        }

        public int GetInterconnectednessScore()
        {
            return _interconnectednessLevel;
        }

        public int GetGenerationalGrowthScore()
        {
            return _generationalGrowth;
        }

        public string GetState()
        {
            return $"Innate Wisdom: {_innateWisdomLevel} | Interconnectedness: {_interconnectednessLevel} | Generational Growth: {_generationalGrowth} | Next Gen Knowledge: {_nextGenerationKnowledge.Count} | Self Effort Applied: {_selfEffortApplied} | Sin Washed: {_sinWashedAway} | Inherited Wisdoms: {_inheritedWisdom.Count}";
        }
    }
}
