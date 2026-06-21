using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Tracks long-term virtue development and progression over time
    /// Provides metrics on growth, milestones, and overall evolution
    /// </summary>
    public class VirtueProgressionTracker
    {
        private VirtuesSystem _system;
        private Dictionary<DateTime, int> _harmonyHistory = new Dictionary<DateTime, int>();
        private Dictionary<string, List<int>> _virtueHistory = new Dictionary<string, List<int>>();
        private DateTime _trackingStart;
        private List<string> _milestones = new List<string>();
        private int _totalPracticeDays = 0;
        private int _streakDays = 0;

        public VirtueProgressionTracker()
        {
            _system = new VirtuesSystem();
            _trackingStart = DateTime.Now;
            InitializeVirtueHistories();
        }

        private void InitializeVirtueHistories()
        {
            _virtueHistory["Fortitude"] = new List<int>();
            _virtueHistory["Chastity"] = new List<int>();
            _virtueHistory["Diligence"] = new List<int>();
            _virtueHistory["Grace"] = new List<int>();
            _virtueHistory["Honesty"] = new List<int>();
            _virtueHistory["Patience"] = new List<int>();
            _virtueHistory["Devotion"] = new List<int>();
            _virtueHistory["Prayer"] = new List<int>();
            _virtueHistory["Divinity"] = new List<int>();
            _virtueHistory["Will"] = new List<int>();
            _virtueHistory["Deviation"] = new List<int>();
            _virtueHistory["Sabbath"] = new List<int>();
            _virtueHistory["Matrimony"] = new List<int>();
        }

        public void RecordDailyPractice()
        {
            _totalPracticeDays++;
            _streakDays++;

            int currentHarmony = _system.GetTotalHarmony();
            _harmonyHistory[DateTime.Now] = currentHarmony;

            // Record virtue snapshots
            _virtueHistory["Fortitude"].Add(_system.GetFortitude().GetState().GetHashCode() % 100);
            _virtueHistory["Chastity"].Add(_system.GetChastity().GetDisciplineStrength());
            _virtueHistory["Diligence"].Add(_system.GetDiligence().GetWisdomScore());
            _virtueHistory["Grace"].Add(_system.GetGrace().GetEleganceScore());
            _virtueHistory["Honesty"].Add(_system.GetHonesty().GetIntegrityScore());
            _virtueHistory["Patience"].Add(_system.GetPatience().GetMindStillness());
            _virtueHistory["Devotion"].Add(_system.GetDevotion().GetDevotionalScore());
            _virtueHistory["Prayer"].Add(_system.GetPrayer().CanWorshipNow() ? 50 : 0);
            _virtueHistory["Divinity"].Add(_system.GetDivinity().GetInnateWisdomScore());
            _virtueHistory["Will"].Add(_system.GetWill().GetWillStrength());
            _virtueHistory["Deviation"].Add(_system.GetDeviation().GetAdaptationScore());
            _virtueHistory["Sabbath"].Add(_system.GetSabbath().GetTotalRestScore());
            _virtueHistory["Matrimony"].Add(_system.GetMatrimony().GetMatrimonyScore());
        }

        public void ResetStreak()
        {
            _streakDays = 0;
        }

        public void AddMilestone(string milestone)
        {
            _milestones.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {milestone}");
        }

        public double GetAverageHarmony()
        {
            if (_harmonyHistory.Count == 0) return 0;

            int sum = 0;
            foreach (var harmony in _harmonyHistory.Values)
            {
                sum += harmony;
            }
            return (double)sum / _harmonyHistory.Count;
        }

        public int GetHighestHarmony()
        {
            if (_harmonyHistory.Count == 0) return 0;

            int highest = 0;
            foreach (var harmony in _harmonyHistory.Values)
            {
                if (harmony > highest)
                    highest = harmony;
            }
            return highest;
        }

        public int GetLowestHarmony()
        {
            if (_harmonyHistory.Count == 0) return 100;

            int lowest = 100;
            foreach (var harmony in _harmonyHistory.Values)
            {
                if (harmony < lowest)
                    lowest = harmony;
            }
            return lowest;
        }

        public double GetGrowthRate()
        {
            if (_harmonyHistory.Count < 2) return 0;

            var values = new List<int>(_harmonyHistory.Values);
            int initial = values[0];
            int latest = values[values.Count - 1];

            if (initial == 0) return 0;
            return ((double)(latest - initial) / initial) * 100;
        }

        public string GetStrongestVirtue()
        {
            string strongest = "None";
            int highest = 0;

            foreach (var virtue in _virtueHistory)
            {
                if (virtue.Value.Count > 0)
                {
                    int sum = 0;
                    foreach (var score in virtue.Value)
                        sum += score;

                    int average = sum / virtue.Value.Count;
                    if (average > highest)
                    {
                        highest = average;
                        strongest = virtue.Key;
                    }
                }
            }

            return strongest;
        }

        public string GetWeakestVirtue()
        {
            string weakest = "None";
            int lowest = int.MaxValue;

            foreach (var virtue in _virtueHistory)
            {
                if (virtue.Value.Count > 0)
                {
                    int sum = 0;
                    foreach (var score in virtue.Value)
                        sum += score;

                    int average = sum / virtue.Value.Count;
                    if (average < lowest && average >= 0)
                    {
                        lowest = average;
                        weakest = virtue.Key;
                    }
                }
            }

            return weakest;
        }

        public TimeSpan GetTrackingDuration()
        {
            return DateTime.Now - _trackingStart;
        }

        public void PrintProgressReport()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           VIRTUE PROGRESSION TRACKER - REPORT              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine($"Tracking Duration: {GetTrackingDuration().TotalDays:F1} days");
            Console.WriteLine($"Total Practice Days: {_totalPracticeDays}");
            Console.WriteLine($"Current Streak: {_streakDays} days");
            Console.WriteLine($"\nHarmony Metrics:");
            Console.WriteLine($"  Average Harmony: {GetAverageHarmony():F1}");
            Console.WriteLine($"  Highest Harmony: {GetHighestHarmony()}");
            Console.WriteLine($"  Lowest Harmony: {GetLowestHarmony()}");
            Console.WriteLine($"  Growth Rate: {GetGrowthRate():F1}%");
            Console.WriteLine($"\nVirtue Analysis:");
            Console.WriteLine($"  Strongest: {GetStrongestVirtue()}");
            Console.WriteLine($"  Weakest: {GetWeakestVirtue()}");
            Console.WriteLine($"\nMilestones ({_milestones.Count}):");
            foreach (var milestone in _milestones)
            {
                Console.WriteLine($"  • {milestone}");
            }

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   END OF PROGRESSION REPORT                 ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");
        }

        public string GetProgressSummary()
        {
            return $"Progress Summary:\n" +
                   $"Days Tracked: {GetTrackingDuration().TotalDays:F1}\n" +
                   $"Avg Harmony: {GetAverageHarmony():F1}\n" +
                   $"Growth: {GetGrowthRate():F1}%\n" +
                   $"Strongest: {GetStrongestVirtue()}\n" +
                   $"Needs Work: {GetWeakestVirtue()}";
        }
    }
}
