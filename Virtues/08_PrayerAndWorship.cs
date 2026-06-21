using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Prayer & Worship = Through understanding, offerings, blessings, divinity, and salvation
    /// Prayer is not for oneself, but to pray for others who are weaker
    /// "Don't wait for a voice to speak. Speak to silence first."
    /// "Worship isn't something you do once a week. It's how you live."
    /// </summary>
    public class PrayerAndWorship
    {
        private List<string> _prayersForOthers = new List<string>();
        private List<string> _offerings = new List<string>();
        private List<string> _blessings = new List<string>();
        private Dictionary<string, bool> _worshipConditions = new Dictionary<string, bool>();
        private int _purityScore = 100;
        private bool _filledMind = false;
        private bool _negativeEnergy = false;
        private bool _sinfulDesires = false;
        private bool _safePassageKnown = false;
        private bool _alignmentAchieved = false;
        private int _spiritualCleanliness = 100;

        public void PrayForOthersWeaker(string person, string need)
        {
            _prayersForOthers.Add($"Prayer for {person}: {need}");
            _purityScore += 10;
        }

        public void HealAndSupport(string person)
        {
            // "to pray for others who are weaker and it is for healing and supporting those who cannot protect and defend themselves"
            _prayersForOthers.Add($"Healing and Support for {person}");
            _purityScore += 15;
        }

        public void MakeOffering(string offering, string meaning)
        {
            _offerings.Add($"{offering}: {meaning}");
        }

        public void Bless(string blessing)
        {
            _blessings.Add(blessing);
            _purityScore += 5;
        }

        public void CheckPrayerConditions()
        {
            _worshipConditions["FilledMind"] = !_filledMind;
            _worshipConditions["NegativeEnergy"] = !_negativeEnergy;
            _worshipConditions["SinfulDesires"] = !_sinfulDesires;
        }

        public void SpeakToSilenceFirst()
        {
            // "Don't wait for a voice to speak. Speak to silence first."
            _purityScore += 20;
        }

        public void LivePrayerInExperience()
        {
            // "You speak through Living it, Experiencing it"
            _purityScore += 10;
        }

        public void MaintainCompatibility()
        {
            // "Maintaining Compatibility with Through Devotion & Silence"
            _purityScore += 5;
        }

        public void PrayForGuidanceNotCrutch(string pressure)
        {
            // "Prayer for oneself is only for guidance towards pressure or the point of no return, it is not a crutch"
            _prayersForOthers.Add($"Self-Guidance: {pressure}");
            _purityScore += 8;
        }

        public void EstablishSafePassage()
        {
            _safePassageKnown = true;
            _worshipConditions["SafePassage"] = true;
        }

        public void ClearMind()
        {
            _filledMind = false;
            _spiritualCleanliness += 10;
        }

        public void CalmBrain()
        {
            _worshipConditions["CalmMind"] = true;
            _spiritualCleanliness += 10;
        }

        public void AchieveAlignment()
        {
            // Mental, Physical, Emotional, Spiritual
            _alignmentAchieved = true;
            _spiritualCleanliness += 20;
        }

        public void ExpelSin()
        {
            _sinfulDesires = false;
            _purityScore += 25;
            _spiritualCleanliness += 25;
        }

        public void ExpelSelfishDesires()
        {
            _worshipConditions["SelfishDesires"] = false;
            _purityScore += 15;
        }

        public void Purify()
        {
            _spiritualCleanliness = 100;
        }

        public void LightCandle()
        {
            // "Light a candle → It's a prayer."
            _prayersForOthers.Add("Candle Lit - Prayer Made");
        }

        public void CookFood()
        {
            // "Cook food → It's a reminder to the most natural of cycles."
            _blessings.Add("Food Prepared - Cycle Honored");
        }

        public void SitInSilence()
        {
            // "Sit in silence → It's communion."
            _purityScore += 30;
        }

        public void NoticeTheNextBreath()
        {
            // "You don't need a plan. You just need to notice the next breath."
            _purityScore += 5;
        }

        public bool CanWorshipNow()
        {
            CheckPrayerConditions();
            return _safePassageKnown && _alignmentAchieved && _spiritualCleanliness >= 70 && _purityScore >= 50;
        }

        public string GetState()
        {
            return $"Prayers for Others: {_prayersForOthers.Count} | Offerings: {_offerings.Count} | Blessings: {_blessings.Count} | Purity: {_purityScore} | Spiritual Cleanliness: {_spiritualCleanliness} | Can Worship: {CanWorshipNow()} | Aligned: {_alignmentAchieved}";
        }
    }
}
