using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Grace = Elegance in motion, timing, flow, and autonomic understanding
    /// "You don't earn grace — you remember it."
    /// Grace is moving in sync and alignment, well-timed and natural.
    /// </summary>
    public class Grace
    {
        private decimal _flowRhythm = 1.0m; // 0-2.0 scale
        private decimal _timing = 0.5m; // alignment timing
        private decimal _elasticity = 1.0m; // flexibility
        private List<string> _forgivenessActs = new List<string>();
        private int _eleganceLevel = 0;
        private bool _isMovingNaturally = false;
        private int _autonomicDecisions = 0;

        public void RememberGrace()
        {
            // Grace is not earned, but remembered
            _eleganceLevel += 20;
            _isMovingNaturally = true;
        }

        public void MoveWithFlow()
        {
            _flowRhythm = Math.Min(2.0m, _flowRhythm + 0.1m);
        }

        public void AlignWithRhythm()
        {
            _timing = Math.Min(1.0m, _timing + 0.05m);
        }

        public void BecomElastic()
        {
            _elasticity = Math.Min(2.0m, _elasticity + 0.1m);
        }

        public void Breathe()
        {
            // Grace is breathless (literally) - finding stillness in motion
            _autonomicDecisions++;
        }

        public void ForgiveWhoHurtYou(string person, string hurt)
        {
            _forgivenessActs.Add($"Forgave {person} for {hurt}");
            _eleganceLevel += 15;

            // Forgiveness is not kindness - it's allowing recovery
            // and dispersing negativity
            CastAwayNegative();
        }

        private void CastAwayNegative()
        {
            _eleganceLevel += 5;
        }

        public void ReclaimSoul()
        {
            // A moment of grace can be the moment you stop waiting for permission
            _eleganceLevel += 25;
        }

        public void AcceptAutonomicControl()
        {
            // Automatic decision making from intuition and inner guidance
            _autonomicDecisions += 1;
        }

        public decimal GetFlowState()
        {
            return _flowRhythm * _timing * _elasticity;
        }

        public int GetEleganceScore()
        {
            return _eleganceLevel;
        }

        public int GetForgivenessCount()
        {
            return _forgivenessActs.Count;
        }

        public string GetState()
        {
            return $"Elegance: {_eleganceLevel} | Flow Rhythm: {_flowRhythm:F2} | Timing: {_timing:F2} | Elasticity: {_elasticity:F2} | Autonomic Decisions: {_autonomicDecisions} | Forgiven: {_forgivenessActs.Count}";
        }
    }
}
