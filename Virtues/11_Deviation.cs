using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Deviation = "Breaking Rules Is A Test of Alliance"
    /// Deviation isn't a sin — it's evolution
    /// "Deviation is — it's intelligence, wisdom, building upon ones constitution and reinforcement"
    /// Breaking rules requires knowing what you're protecting or losing through equivalence
    /// </summary>
    public class Deviation
    {
        private List<string> _rulesFollowed = new List<string>();
        private List<string> _rulesBroken = new List<string>();
        private Dictionary<string, string> _justifications = new Dictionary<string, string>();
        private int _intelligenceApplied = 0;
        private int _wisdomApplied = 0;
        private List<string> _alliances = new List<string>();
        private bool _isEvolution = false;
        private int _adaptationScore = 0;

        public void FollowRule(string rule)
        {
            _rulesFollowed.Add(rule);
        }

        public void BreakRuleIntelligently(string rule, string justification, string protectingValue)
        {
            // Breaking rules is a test of alliance
            if (!string.IsNullOrEmpty(justification))
            {
                _rulesBroken.Add(rule);
                _justifications[rule] = $"{justification} (Protecting: {protectingValue})";
                _intelligenceApplied += 15;
            }
        }

        public void AdaptToPlanChange()
        {
            // "If plans change or time adjusts? Do you adapt and or do you switch and adjust towards?"
            _adaptationScore += 10;
        }

        public void Recalibrate(string reason)
        {
            // "If your team stops working? That's Patience And Realignment — it's recalibration"
            _adaptationScore += 15;
            _intelligenceApplied += 10;
        }

        public void TestAlliance(string allianceWith)
        {
            // "Breaking rules is less about the reason and what you are protecting or potentially losing"
            _alliances.Add(allianceWith);
            _wisdomApplied += 20;
        }

        public void RecognizeEvolution()
        {
            // "Deviation is — it's intelligence, wisdom, building upon ones constitution"
            _isEvolution = true;
            _intelligenceApplied += 20;
            _wisdomApplied += 20;
        }

        public void CalculateEquivalenceAndWorth(string what, string why)
        {
            // Understanding what's being gained vs what's being lost
            _justifications[$"Equivalence: {what}"] = why;
            _wisdomApplied += 25;
        }

        public void UnderstandSacrifice(string sacrifice, string gain)
        {
            // "When you break the rules, its less about the reason and what you are protecting or potentially losing in order to gain or lose something else through equivalence, worth and sacrifice"
            _wisdomApplied += 30;
        }

        public void AvoidBlindFaith()
        {
            // "The only thing more dangerous than failure is obedience/dependency & blind faith"
            _intelligenceApplied += 20;
        }

        public void PracticeDeviation()
        {
            // Deviation improves through practice and wisdom
            _wisdomApplied += 5;
        }

        public int GetIntelligenceScore()
        {
            return _intelligenceApplied;
        }

        public int GetWisdomScore()
        {
            return _wisdomApplied;
        }

        public int GetAdaptationScore()
        {
            return _adaptationScore;
        }

        public int GetRulesBrokenCount()
        {
            return _rulesBroken.Count;
        }

        public int GetAlliancesFormed()
        {
            return _alliances.Count;
        }

        public string GetState()
        {
            return $"Rules Followed: {_rulesFollowed.Count} | Rules Broken: {_rulesBroken.Count} | Intelligence Applied: {_intelligenceApplied} | Wisdom Applied: {_wisdomApplied} | Adaptation Score: {_adaptationScore} | Alliances: {_alliances.Count} | Is Evolution: {_isEvolution} | Justifications: {_justifications.Count}";
        }
    }
}
