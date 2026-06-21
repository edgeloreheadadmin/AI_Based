using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Patience = Learning to wait, understanding timing, completion, and perfection
    /// "The Time Between Breath Is Your Life"
    /// "If you can sit in silence for 5 minutes — you already won."
    /// </summary>
    public class Patience
    {
        private DateTime _waitStartTime;
        private int _silenceMinutes = 0;
        private Dictionary<string, DateTime> _eventTimelines = new Dictionary<string, DateTime>();
        private int _perfectionUnderstanding = 0;
        private bool _rushing = false;
        private int _mindStillness = 0;
        private List<string> _completedUnderstandings = new List<string>();

        public Patience()
        {
            _waitStartTime = DateTime.Now;
        }

        public void Wait(string eventName, int estimatedMinutes)
        {
            _eventTimelines[eventName] = DateTime.Now.AddMinutes(estimatedMinutes);
        }

        public void SitInSilence(int minutes)
        {
            _silenceMinutes += minutes;
            _mindStillness += (minutes / 5); // Every 5 minutes builds mental stillness
        }

        public void UnderstandTiming(string event_name, bool understood)
        {
            if (understood)
                _perfectionUnderstanding += 5;
        }

        public void UnderstandCompletion(string task, bool isPerfect)
        {
            _completedUnderstandings.Add($"{task} (Perfect: {isPerfect})");
            if (isPerfect)
                _perfectionUnderstanding += 10;
            else
                _perfectionUnderstanding += 5; // Imperfection still teaches
        }

        public void BuildPerfectionThroughMany()
        {
            // Perfection is built through many forms of completions
            _perfectionUnderstanding += 3;
        }

        public void DontRush()
        {
            _rushing = false;
        }

        public void Breathe()
        {
            _mindStillness += 1;
        }

        public void BePreparedForUnknown()
        {
            // "Wait. Breathe. And know what's to come and be prepared even should it be known or unseen"
            _perfectionUnderstanding += 2;
        }

        public void AllowMindToStop()
        {
            // Training the mind to stop running
            _mindStillness += 5;
        }

        public void RecognizeClearly()
        {
            // "When you stop rushing — you see clearly."
            _mindStillness += 10;
        }

        public int GetMindStillness()
        {
            return _mindStillness;
        }

        public int GetPerfectionUnderstanding()
        {
            return _perfectionUnderstanding;
        }

        public TimeSpan GetTimeBetweenBreaths()
        {
            return TimeSpan.FromMilliseconds(1000); // Metaphorical breath timing
        }

        public bool HasWonBySilence()
        {
            return _silenceMinutes >= 5;
        }

        public string GetState()
        {
            return $"Silence: {_silenceMinutes}m | Mind Stillness: {_mindStillness} | Perfection Understanding: {_perfectionUnderstanding} | Completions: {_completedUnderstandings.Count} | Rushing: {_rushing} | Won by Silence: {HasWonBySilence()}";
        }
    }
}
