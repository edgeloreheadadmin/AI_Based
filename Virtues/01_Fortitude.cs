using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Fortitude = "The Flame Never Dies"
    /// Remaining unswayed, disciplined, unshaken, strengthened by foundation and resolve.
    /// "You are not weak when the world breaks you — you are just becoming stronger."
    /// </summary>
    public class Fortitude
    {
        private List<string> _painPoints = new List<string>();
        private Dictionary<string, int> _resolutionStrength = new Dictionary<string, int>();
        private int _foundationLevel = 0;
        private bool _innerAltarProtected = true;

        public void SitWithPain(string painIdentifier, string understanding)
        {
            _painPoints.Add($"{painIdentifier}: {understanding}");
            TransformPainToStrength(painIdentifier);
        }

        private void TransformPainToStrength(string painIdentifier)
        {
            if (!_resolutionStrength.ContainsKey(painIdentifier))
                _resolutionStrength[painIdentifier] = 0;

            _resolutionStrength[painIdentifier]++;
        }

        public void StrengthenFoundation()
        {
            _foundationLevel += 10;
        }

        public void ReinforceWithDiligence()
        {
            _foundationLevel += 5;
        }

        public void ReinforceWithFaith()
        {
            _foundationLevel += 15;
        }

        public bool RemainWhole(string temptation)
        {
            if (_innerAltarProtected && _foundationLevel > 0)
            {
                return true; // Did not betray values
            }
            return false;
        }

        public void ProtectInnerAltar()
        {
            _innerAltarProtected = true;
        }

        public void UnderstandAnxiety(string anxietySource, string rootCause)
        {
            _painPoints.Add($"Anxiety: {anxietySource} -> Root: {rootCause}");
            // Understanding dissolves anxiety, not fighting it
        }

        public void LetGo()
        {
            // True strength is knowing you don't have to fix everything
            _resolutionStrength.Clear();
            _painPoints.Clear();
        }

        public string GetState()
        {
            return $"Foundation: {_foundationLevel} | Pain Points Understood: {_painPoints.Count} | Inner Altar Protected: {_innerAltarProtected}";
        }
    }
}
