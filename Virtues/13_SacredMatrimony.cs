using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// "Thou Shall Not Commit Adultery..." = Sacred Bonds, Lifelong Commitment
    /// "Love is not a vow — it's a choice to build something lasting"
    /// "Holy Matrimony" is sacred and it's a lifelong bond blessed with "No Divorce" as a reflection of permanence
    /// Requires: Chastity & Diligence, Faith and Virtue for commitment
    /// </summary>
    public class SacredMatrimony
    {
        private string _partnerName = "";
        private DateTime _matrimonyDate;
        private bool _vowsExchanged = false;
        private bool _divorceExcluded = false;
        private int _devotionScore = 0;
        private int _trustScore = 0;
        private int _respectScore = 0;
        private List<string> _growthJourney = new List<string>();
        private bool _sickAndHealthAccepted = false;
        private bool _independenceBalanced = false;
        private int _chasityDiscipline = 0;
        private int _diligentEffort = 0;
        private List<string> _sharedValues = new List<string>();
        private Dictionary<string, int> _conflictResolutions = new Dictionary<string, int>();

        public void MakeVows(string partnerName, string vow)
        {
            _partnerName = partnerName;
            _matrimonyDate = DateTime.Now;
            _vowsExchanged = true;
            _devotionScore = 50; // Starting devotion from commitment
        }

        public void DeclareLoveWithoutWords()
        {
            // "I will love you not because I am free — But because it is my responsibility"
            _devotionScore += 20;
        }

        public void ChooseResponsibility()
        {
            // "Because it is my responsibility to show you what love means"
            _devotionScore += 15;
        }

        public void DemandRespect()
        {
            // "Because respect is demanded and consented upon"
            _respectScore += 25;
        }

        public void ChooseFreeWill()
        {
            // "Because it is in my will and my devotion towards meaning. Because it is my free will."
            _devotionScore += 20;
        }

        public void ExcludeDivorce()
        {
            // "Divorce is not allowed — And requires discipline to hold"
            _divorceExcluded = true;
            _devotionScore += 50;
            _chasityDiscipline += 30;
        }

        public void UndertakeHealing(string issue)
        {
            // "Divorce is not allowed — And requires discipline to hold and with deep self and equivalent other understanding and deep self healing & maturity"
            _growthJourney.Add($"Healing: {issue}");
            _devotionScore += 10;
        }

        public void GrowTogetherSimilarly(string field)
        {
            // Both sides should continually grow and improve and evolve
            _growthJourney.Add($"Growth (Similar): {field}");
            _diligentEffort += 10;
        }

        public void GrowTogetherDifferently(string field)
        {
            // In more than one field both similar and different
            _growthJourney.Add($"Growth (Different): {field}");
            _diligentEffort += 10;
        }

        public void CommitTillDeath()
        {
            // "Till the day I die, until death do us part holds the contract and the binding"
            _devotionScore += 100;
        }

        public void AcceptInSicknessAndHealth()
        {
            // "In sickness and in health is to accept each other through the good and the bad"
            _sickAndHealthAccepted = true;
            _devotionScore += 30;
        }

        public void EncourageImprovement()
        {
            // "to encourage each other to improve complexion, beauty, take care of oneself"
            _devotionScore += 15;
        }

        public void LookAfterEachOther()
        {
            // "and look after each other and requires independence to an extent"
            _trustScore += 20;
        }

        public void BuildTrustWhenApart()
        {
            // "learning more not to become too dependent and build trust and respect for each other when apart"
            _trustScore += 25;
        }

        public void ResolveConflict(string conflict, bool resolved)
        {
            if (resolved)
            {
                _conflictResolutions[conflict] = 1;
                _devotionScore += 15;
            }
        }

        public void ProtectSoul()
        {
            // "Maiden names Are There and connects to the body through the body language"
            // "but the vow to protect your soul remains"
            _devotionScore += 20;
        }

        public void RespectIndividuality()
        {
            // Understanding that each person has their own identity
            _respectScore += 20;
        }

        public int GetMatrimonyScore()
        {
            return _devotionScore + _trustScore + _respectScore + _chasityDiscipline + _diligentEffort;
        }

        public int GetDevotionScore()
        {
            return _devotionScore;
        }

        public int GetTrustScore()
        {
            return _trustScore;
        }

        public int GetRespectScore()
        {
            return _respectScore;
        }

        public TimeSpan GetMatrimonyDuration()
        {
            return _matrimonyDate == default ? TimeSpan.Zero : DateTime.Now - _matrimonyDate;
        }

        public string GetState()
        {
            return $"Partner: {_partnerName} | Vows Exchanged: {_vowsExchanged} | Divorce Excluded: {_divorceExcluded} | Duration: {GetMatrimonyDuration().TotalDays} days | Devotion: {_devotionScore} | Trust: {_trustScore} | Respect: {_respectScore} | Chastity Discipline: {_chasityDiscipline} | Diligent Effort: {_diligentEffort} | Total Score: {GetMatrimonyScore()} | Growth Milestones: {_growthJourney.Count} | Conflict Resolutions: {_conflictResolutions.Count}";
        }
    }
}
