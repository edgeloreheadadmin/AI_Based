using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// The Sabbath Day & 7-Day Creation & Day of Rest
    /// "Rest is considered repentance, reconciliation, discipline to the memory, mind & brain"
    /// "Rest is the breath restabilizing"
    /// "Rest is little to no effort nor work to focus on the inner parts of the brain and or mind"
    /// </summary>
    public class SabbathAndRest
    {
        private Dictionary<int, bool> _sevenDayCycle = new Dictionary<int, bool>();
        private int _restDays = 0;
        private int _memoryStrength = 100;
        private int _mindClearness = 100;
        private int _brainRecovery = 100;
        private int _energyRestoration = 100;
        private int _chemistryBalance = 100;
        private List<string> _repentanceActs = new List<string>();
        private List<string> _reconciliationActs = new List<string>();
        private bool _inRestCycle = false;
        private DateTime _restStartTime;

        public SabbathAndRest()
        {
            // Initialize 7-day cycle
            for (int i = 1; i <= 7; i++)
            {
                _sevenDayCycle[i] = false; // Day not completed yet
            }
        }

        public void StartSabbathDay()
        {
            _inRestCycle = true;
            _restStartTime = DateTime.Now;
            _restDays++;
        }

        public void EndSabbathDay()
        {
            _inRestCycle = false;
        }

        public void RestAndRepent(string what)
        {
            // "Rest is considered repentance, reconciliation, discipline"
            _repentanceActs.Add(what);
            _memoryStrength += 5;
        }

        public void RestAndReconcile(string with)
        {
            // Reconciliation through rest
            _reconciliationActs.Add(with);
            _memoryStrength += 10;
        }

        public void DisciplineMemory()
        {
            // "discipline to the memory, mind & brain through both strict and disciplined"
            _memoryStrength = Math.Min(150, _memoryStrength + 10);
        }

        public void RestoreBrainEnergy()
        {
            // "Rest is the breath restabilizing"
            _brainRecovery = Math.Min(150, _brainRecovery + 15);
            _energyRestoration = Math.Min(150, _energyRestoration + 15);
        }

        public void ReduceEffortAndWork()
        {
            // "Rest is little to no effort nor work to focus on the inner parts of the brain and or mind"
            _brainRecovery += 20;
        }

        public void ReinforceEnergyChemistry()
        {
            // "to reinforce energy and chemistry and restabilize oneself and realign inner desires"
            _energyRestoration = Math.Min(150, _energyRestoration + 10);
            _chemistryBalance = Math.Min(150, _chemistryBalance + 10);
        }

        public void Realign()
        {
            // Realign inner desires through rest
            _energyRestoration += 5;
            _chemistryBalance += 5;
        }

        public void CompleteDay(int dayOfWeek)
        {
            if (dayOfWeek >= 1 && dayOfWeek <= 7)
            {
                _sevenDayCycle[dayOfWeek] = true;
            }
        }

        public void Rest(int minutes)
        {
            // Generic rest action
            _brainRecovery += (minutes / 10);
            _energyRestoration += (minutes / 10);
            _mindClearness += (minutes / 10);
        }

        public void ClearMind()
        {
            _mindClearness = Math.Min(150, _mindClearness + 20);
        }

        public int GetWeeklyCycleCompletion()
        {
            int completed = 0;
            for (int i = 1; i <= 7; i++)
            {
                if (_sevenDayCycle[i]) completed++;
            }
            return completed;
        }

        public int GetTotalRestScore()
        {
            return _memoryStrength + _mindClearness + _brainRecovery + _energyRestoration + _chemistryBalance;
        }

        public string GetState()
        {
            return $"Rest Days Taken: {_restDays} | In Rest Cycle: {_inRestCycle} | Memory: {_memoryStrength} | Mind Clearness: {_mindClearness} | Brain Recovery: {_brainRecovery} | Energy: {_energyRestoration} | Chemistry Balance: {_chemistryBalance} | Repentance Acts: {_repentanceActs.Count} | Reconciliation Acts: {_reconciliationActs.Count} | Weekly Progress: {GetWeeklyCycleCompletion()}/7";
        }
    }
}
