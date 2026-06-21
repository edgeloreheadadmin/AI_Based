using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Constitution of Will = "Will" is not chosen — it's inherited
    /// "The Constitution of Will isn't written — it's lit like a torch and strengthens through generations"
    /// "Your will is not something you choose — it's something you become"
    /// </summary>
    public class ConstitutionOfWill
    {
        private int _inheritedStrength = 50; // Starting point from generations
        private List<string> _generationalTorch = new List<string>();
        private int _willRefinement = 0;
        private bool _loveChosenOverFear = false;
        private bool _disciplined = false;
        private List<string> _actionsNoOneElseTakes = new List<string>();
        private int _fearUnderstanding = 0;
        private int _willBecomeProcess = 0;

        public ConstitutionOfWill()
        {
            // Will is inherited
            _inheritedStrength = 50;
        }

        public void LightTorchForNextGeneration(string lightValue)
        {
            // "The Constitution of Will isn't written — it's lit like a torch"
            _generationalTorch.Add(lightValue);
            _inheritedStrength += 10;
        }

        public void StrengthenThroughGenerations()
        {
            // "strengthens through generations"
            _inheritedStrength += 20;
        }

        public void DontNeedPermission()
        {
            // "You don't need permission to be strong"
            _willRefinement += 15;
        }

        public void DoWhatNoOneElseWill(string action)
        {
            // "You just need to do what no one else will"
            _actionsNoOneElseTakes.Add(action);
            _willRefinement += 20;
        }

        public void ChooseLoveOverFear()
        {
            _loveChosenOverFear = true;
            _willRefinement += 25;
        }

        public void UnderstandFear()
        {
            // "understanding fear leads to understanding how fear forms forms weathering of the Will, Soul, Spirit"
            _fearUnderstanding += 10;
        }

        public void AcceptFearWeathering()
        {
            // Fear weathering is not for weak minded
            if (_disciplined)
                _willRefinement += 15;
        }

        public void BuildDiscipline()
        {
            _disciplined = true;
            _willRefinement += 20;
        }

        public void DiscoverWill()
        {
            // "Will" isn't chosen — it's discovered
            _willBecomeProcess += 20;
        }

        public void LiveTheConstitution()
        {
            // "The Constitution of Will isn't written — it's lived"
            _willBecomeProcess += 30;
        }

        public void NoticeNextBreath()
        {
            // "You just need to notice the next breath"
            _willBecomeProcess += 5;
        }

        public void BecomeYourWill()
        {
            // "Your will is not something you choose — it's something you become"
            _willBecomeProcess += 40;
        }

        public void BreatheMeansLive()
        {
            // "If you don't live — you aren't breathing"
            if (_willBecomeProcess > 0)
                _willRefinement += 10;
        }

        public int GetWillStrength()
        {
            return _inheritedStrength + _willRefinement + _willBecomeProcess;
        }

        public int GetWillRefinement()
        {
            return _willRefinement;
        }

        public int GetWillBecomingProgress()
        {
            return _willBecomeProcess;
        }

        public int GetGenerationalTorchCount()
        {
            return _generationalTorch.Count;
        }

        public string GetState()
        {
            return $"Total Will Strength: {GetWillStrength()} | Inherited: {_inheritedStrength} | Refinement: {_willRefinement} | Becoming Progress: {_willBecomeProcess} | Actions No One Else Takes: {_actionsNoOneElseTakes.Count} | Fear Understanding: {_fearUnderstanding} | Love Over Fear: {_loveChosenOverFear} | Disciplined: {_disciplined} | Torch Passed: {_generationalTorch.Count}";
        }
    }
}
