using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Chastity = Self-discipline, restraint, integrity of intention
    /// "Chastity isn't repression — it's control over desire."
    /// Held by conditions, restrictions, and self-imposed laws for months to years to decades.
    /// </summary>
    public class Chastity
    {
        private List<string> _selfImposedLaws = new List<string>();
        private Dictionary<string, int> _daysHeld = new Dictionary<string, int>();
        private DateTime _commitmentStartDate;
        private int _disciplineLevel = 0;
        private bool _neverTurnBack = true;

        public Chastity()
        {
            _commitmentStartDate = DateTime.Now;
        }

        public void EstablishLaw(string law, int durationInDays)
        {
            _selfImposedLaws.Add(law);
            _daysHeld[law] = 0;
            _disciplineLevel += 5;
        }

        public void ResistDesire(string desireType)
        {
            // Say no. Deny. Resist.
            _disciplineLevel += 2;
        }

        public void HoldLaw(string law)
        {
            if (_selfImposedLaws.Contains(law) && _neverTurnBack)
            {
                _daysHeld[law]++;
                if (_daysHeld[law] % 7 == 0)
                    _disciplineLevel += 1;
            }
        }

        public bool MaintainIntegrity(string intention)
        {
            // Integrity in intention - not just action
            return _neverTurnBack && _disciplineLevel > 0;
        }

        public void NeverChangeMind()
        {
            _neverTurnBack = true;
        }

        public void ChannelDesireIntoConstraint(string constraintGoal)
        {
            // Control over desire = directing it toward something meaningful
            _disciplineLevel += 10;
        }

        public int GetDisciplineStrength()
        {
            return _disciplineLevel;
        }

        public TimeSpan GetCommitmentDuration()
        {
            return DateTime.Now - _commitmentStartDate;
        }

        public string GetState()
        {
            return $"Laws: {_selfImposedLaws.Count} | Discipline: {_disciplineLevel} | Commitment: {GetCommitmentDuration().TotalDays} days | Never Turn Back: {_neverTurnBack}";
        }
    }
}
