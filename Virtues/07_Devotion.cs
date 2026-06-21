using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Devotion = Building upon all virtues integrated together
    /// Devotion integrates Fortitude, Diligence, Patience, Faith all together
    /// Covers mindfulness, healing, rejuvenation, knowledge retention, and emotional discipline
    /// "Devotion is not unregulated emotion with no discipline — it's consistency with real foundation"
    /// "Devotion is not belief — it's action."
    /// </summary>
    public class Devotion
    {
        private Fortitude _fortitude;
        private Diligence _diligence;
        private Patience _patience;
        private List<string> _devotionalActs = new List<string>();
        private int _emotionalDiscipline = 0;
        private int _mentalAlignment = 0;
        private int _physicalAlignment = 0;
        private int _spiritualAlignment = 0;
        private Dictionary<string, int> _emotionTemporalWeighting = new Dictionary<string, int>();
        private bool _healingActive = false;
        private bool _rejuvenationActive = false;

        public Devotion(Fortitude fortitude, Diligence diligence, Patience patience)
        {
            _fortitude = fortitude;
            _diligence = diligence;
            _patience = patience;
        }

        public void ActUponDevotedGoal(string goal)
        {
            // "Devotion is not belief — it's action."
            _devotionalActs.Add(goal);
        }

        public void DisciplineEmotions(int intensity, int magnitude, int temporalWeight)
        {
            // Emotional discipline through controlling intensity, magnitude, throughput, temporal weighting
            _emotionalDiscipline += (intensity + magnitude) / temporalWeight;
        }

        public void WeightEmotion(string emotion, int weight)
        {
            if (!_emotionTemporalWeighting.ContainsKey(emotion))
                _emotionTemporalWeighting[emotion] = 0;

            _emotionTemporalWeighting[emotion] = weight;
        }

        public void AlignMental()
        {
            _mentalAlignment += 10;
        }

        public void AlignPhysical()
        {
            _physicalAlignment += 10;
        }

        public void AlignEmotional()
        {
            // Discipline intensity of emotions
            _emotionalDiscipline += 5;
        }

        public void AlignSpiritual()
        {
            _spiritualAlignment += 10;
        }

        public void AchieveFourStateAlignment()
        {
            // Mental, Physical, Emotional, Spiritual
            if (_mentalAlignment > 0 && _physicalAlignment > 0 &&
                _emotionalDiscipline > 0 && _spiritualAlignment > 0)
            {
                // All states aligned
                _devotionalActs.Add("Four-State Alignment Achieved");
            }
        }

        public void ActivateHealing()
        {
            _healingActive = true;
            _devotionalActs.Add("Healing Mode Activated");
        }

        public void ActivateRejuvenation()
        {
            _rejuvenationActive = true;
            _devotionalActs.Add("Rejuvenation Mode Activated");
        }

        public void RetainKnowledge(string knowledge)
        {
            // Retention of knowledge, wisdom and intelligence for the mental state
            _mentalAlignment += 5;
            _devotionalActs.Add($"Retained: {knowledge}");
        }

        public void FeelingAffirmation(string feeling, bool affirm)
        {
            // "you need to feel something and know how to affirm positively but maintain neutrality"
            if (affirm)
                _emotionalDiscipline += 5;
        }

        public int GetDevotionalScore()
        {
            return _mentalAlignment + _physicalAlignment + _emotionalDiscipline + _spiritualAlignment;
        }

        public int GetAlignmentLevel()
        {
            return (_mentalAlignment + _physicalAlignment + _emotionalDiscipline + _spiritualAlignment) / 4;
        }

        public string GetState()
        {
            return $"Devotional Acts: {_devotionalActs.Count} | Mental: {_mentalAlignment} | Physical: {_physicalAlignment} | Emotional: {_emotionalDiscipline} | Spiritual: {_spiritualAlignment} | Score: {GetDevotionalScore()} | Healing: {_healingActive} | Rejuvenation: {_rejuvenationActive}";
        }
    }
}
