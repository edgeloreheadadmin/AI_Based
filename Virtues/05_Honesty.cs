using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Honesty = "Don't Lie to oneself nor others"
    /// "Honesty isn't truth-telling — it's responsibility."
    /// "Honesty is not truth-telling — it's non-attachment to belief."
    /// </summary>
    public class Honesty
    {
        private List<string> _truthsAcknowledged = new List<string>();
        private List<string> _wrongsAdmitted = new List<string>();
        private Dictionary<string, int> _anglesPerspectives = new Dictionary<string, int>();
        private int _integrityScore = 0;
        private bool _liesToSelf = false;
        private bool _lieToOthers = false;
        private Dictionary<string, string> _beliefs = new Dictionary<string, string>();

        public void AcknowledgeWrong(string wrong, string context)
        {
            _wrongsAdmitted.Add($"{wrong}: {context}");
            _integrityScore += 10;
        }

        public void ReinforceRight(string right)
        {
            _truthsAcknowledged.Add(right);
            _integrityScore += 5;
        }

        public void UnderstandAngle(string perspective)
        {
            if (!_anglesPerspectives.ContainsKey(perspective))
                _anglesPerspectives[perspective] = 0;

            _anglesPerspectives[perspective]++;
            _integrityScore += 2;
        }

        public void RecognizeIllusion(string illusion)
        {
            // Honesty is recognizing illusion, faces, appearances, alterations
            _integrityScore += 8;
        }

        public void UnderstandPersonalities(string person, List<string> personalities)
        {
            // Understanding the many personas a person can hold
            foreach (var personality in personalities)
            {
                _anglesPerspectives[personality] = _anglesPerspectives.ContainsKey(personality) ?
                    _anglesPerspectives[personality] + 1 : 1;
            }
        }

        public void DontLieToSelf(string truth)
        {
            _liesToSelf = false;
            _truthsAcknowledged.Add($"[Self Truth] {truth}");
            _integrityScore += 15;
        }

        public void DontLieToOthers(string truth)
        {
            _lieToOthers = false;
            _truthsAcknowledged.Add($"[Others Truth] {truth}");
            _integrityScore += 15;
        }

        public void DisciplineWithWords(string statement, bool truthful)
        {
            if (truthful)
            {
                _integrityScore += 3;
            }
        }

        public void UnderstandBeliefs(string belief, string origin)
        {
            _beliefs[belief] = origin;
            _integrityScore += 5;
        }

        public void FaceTheTruth(string truth, bool hurts)
        {
            _truthsAcknowledged.Add(truth);
            if (hurts)
                _integrityScore += 20; // Facing painful truth is hardest
            else
                _integrityScore += 10;
        }

        public void DetachFromBelief()
        {
            // Non-attachment to belief allows for honesty
            _integrityScore += 10;
        }

        public int GetIntegrityScore()
        {
            return _integrityScore;
        }

        public int GetAnglePerspectiveCount()
        {
            return _anglesPerspectives.Count;
        }

        public int GetWrongsAdmitted()
        {
            return _wrongsAdmitted.Count;
        }

        public string GetState()
        {
            return $"Integrity: {_integrityScore} | Truths Acknowledged: {_truthsAcknowledged.Count} | Wrongs Admitted: {_wrongsAdmitted.Count} | Perspectives Understood: {_anglesPerspectives.Count} | Lies To Self: {_liesToSelf} | Lies To Others: {_lieToOthers}";
        }
    }
}
