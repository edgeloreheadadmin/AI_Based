using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class MakeNewProcessesThroughRepetition
{
    public class ProcessDefinition
    {
        public string ProcessName { get; set; }
        public List<string> Steps { get; set; }
        public double EfficiencyRating { get; set; }
        public int ExecutionCount { get; set; }
        public List<double> PerformanceHistory { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Category { get; set; }
    }

    public class AdaptationRecord
    {
        public string ProcessName { get; set; }
        public int ExecutionNumber { get; set; }
        public List<string> ModifiedSteps { get; set; }
        public double ImprovementPercentage { get; set; }
        public string AdaptationReason { get; set; }
        public DateTime AdaptationDate { get; set; }
    }

    public class RepetitionPattern
    {
        public string PatternName { get; set; }
        public int RepetitionCount { get; set; }
        public double ConsolidationStrength { get; set; }
        public List<DateTime> ExecutionDates { get; set; }
        public double AutomatizationLevel { get; set; }
    }

    public class ProcessDevelopmentEngine
    {
        private Dictionary<string, ProcessDefinition> processes;
        private List<AdaptationRecord> adaptations;
        private Dictionary<string, RepetitionPattern> patterns;

        public ProcessDevelopmentEngine()
        {
            processes = new Dictionary<string, ProcessDefinition>();
            adaptations = new List<AdaptationRecord>();
            patterns = new Dictionary<string, RepetitionPattern>();
        }

        public void CreateNewProcess(string processName, List<string> initialSteps, string category)
        {
            if (!processes.ContainsKey(processName))
            {
                processes[processName] = new ProcessDefinition
                {
                    ProcessName = processName,
                    Steps = new List<string>(initialSteps),
                    EfficiencyRating = 0.5,
                    ExecutionCount = 0,
                    PerformanceHistory = new List<double> { 0.5 },
                    CreatedDate = DateTime.Now,
                    Category = category
                };

                patterns[processName] = new RepetitionPattern
                {
                    PatternName = processName,
                    RepetitionCount = 0,
                    ConsolidationStrength = 0.0,
                    ExecutionDates = new List<DateTime>(),
                    AutomatizationLevel = 0.0
                };
            }
        }

        public void ExecuteProcess(string processName, double performanceScore)
        {
            if (processes.ContainsKey(processName))
            {
                var process = processes[processName];
                var pattern = patterns[processName];

                process.ExecutionCount++;
                pattern.RepetitionCount++;
                pattern.ExecutionDates.Add(DateTime.Now);

                double consolidation = CalculateConsolidation(pattern.RepetitionCount);
                pattern.ConsolidationStrength = consolidation;
                pattern.AutomatizationLevel = Math.Min(consolidation * performanceScore, 0.95);

                double improvedScore = performanceScore * (1.0 + consolidation * 0.3);
                process.EfficiencyRating = Math.Min(improvedScore, 0.99);
                process.PerformanceHistory.Add(process.EfficiencyRating);
            }
        }

        private double CalculateConsolidation(int repetitions)
        {
            if (repetitions == 0) return 0.0;
            return Math.Tanh(repetitions / 10.0);
        }

        public void AdaptProcessBasedOnFeedback(string processName, List<string> newSteps, string reason)
        {
            if (processes.ContainsKey(processName))
            {
                var process = processes[processName];
                double beforeEfficiency = process.EfficiencyRating;

                process.Steps = new List<string>(newSteps);

                double improvement = process.EfficiencyRating * 0.15;
                process.EfficiencyRating = Math.Min(process.EfficiencyRating + improvement, 0.98);

                var adaptation = new AdaptationRecord
                {
                    ProcessName = processName,
                    ExecutionNumber = process.ExecutionCount,
                    ModifiedSteps = newSteps,
                    ImprovementPercentage = (process.EfficiencyRating - beforeEfficiency) * 100,
                    AdaptationReason = reason,
                    AdaptationDate = DateTime.Now
                };
                adaptations.Add(adaptation);
            }
        }

        public void DisplayProcessStatus(string processName)
        {
            if (processes.ContainsKey(processName))
            {
                var process = processes[processName];
                var pattern = patterns[processName];

                Console.WriteLine($"\n  Process: {process.ProcessName} ({process.Category})");
                Console.WriteLine($"  Executions: {process.ExecutionCount}");
                Console.WriteLine($"  Efficiency: {process.EfficiencyRating * 100:F1}%");
                Console.WriteLine($"  Consolidation: {pattern.ConsolidationStrength * 100:F1}%");
                Console.WriteLine($"  Automatization: {pattern.AutomatizationLevel * 100:F1}%");
                Console.WriteLine($"  Current Steps: {process.Steps.Count}");
            }
        }

        public int CountAdaptations(string processName)
        {
            return adaptations.Count(a => a.ProcessName == processName);
        }

        public double GetAverageImprovement(string processName)
        {
            var processAdaptations = adaptations.Where(a => a.ProcessName == processName).ToList();
            if (processAdaptations.Count == 0) return 0.0;
            return processAdaptations.Average(a => a.ImprovementPercentage);
        }

        public Dictionary<string, double> GetProcessRankings()
        {
            return processes.OrderByDescending(x => x.Value.EfficiencyRating)
                .ToDictionary(x => x.Key, x => x.Value.EfficiencyRating);
        }

        public List<string> IdentifyOptimizationOpportunities()
        {
            var opportunities = new List<string>();

            foreach (var process in processes.Values)
            {
                if (process.ExecutionCount < 5)
                {
                    opportunities.Add($"{process.ProcessName}: Only {process.ExecutionCount} executions - needs more repetition");
                }
                if (process.EfficiencyRating < 0.75)
                {
                    opportunities.Add($"{process.ProcessName}: Efficiency below target ({process.EfficiencyRating * 100:F0}%) - consider adaptation");
                }
            }

            return opportunities;
        }

        public void DisplayAdaptationHistory(string processName)
        {
            var processAdaptations = adaptations.Where(a => a.ProcessName == processName).ToList();
            Console.WriteLine($"\n  Adaptations for {processName}:");
            foreach (var adaptation in processAdaptations)
            {
                Console.WriteLine($"    • After execution {adaptation.ExecutionNumber}: {adaptation.AdaptationReason}");
                Console.WriteLine($"      Improvement: +{adaptation.ImprovementPercentage:F2}%");
            }
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Process Creation Through Repetition and Adaptation         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new ProcessDevelopmentEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating New Processes from Scratch]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateNewProcess("DataProcessing",
            new List<string> { "Read data", "Validate input", "Transform", "Output" }, "Data");
        engine.CreateNewProcess("DecisionMaking",
            new List<string> { "Gather info", "Analyze options", "Evaluate", "Choose" }, "Cognitive");
        engine.CreateNewProcess("ProblemSolving",
            new List<string> { "Define problem", "Brainstorm", "Test solutions", "Implement" }, "Analytical");

        Console.WriteLine("  ✓ Created 3 new processes with initial step definitions");
        Console.WriteLine("  ✓ Each process starts with 50% baseline efficiency");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Repetition Cycles and Consolidation]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Executing DataProcessing multiple times:");
        for (int i = 0; i < 8; i++)
        {
            double baseScore = 0.65 + (i * 0.03);
            engine.ExecuteProcess("DataProcessing", baseScore);
            Console.WriteLine($"    Run {i + 1}: Score = {baseScore:F2} → Consolidation building");
        }
        engine.DisplayProcessStatus("DataProcessing");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Adaptation Triggered by Feedback]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Initial DecisionMaking process:");
        engine.ExecuteProcess("DecisionMaking", 0.60);
        engine.DisplayProcessStatus("DecisionMaking");

        Console.WriteLine("\n  Adaptation #1 - Feedback: Add risk assessment");
        engine.AdaptProcessBasedOnFeedback("DecisionMaking",
            new List<string> { "Gather info", "Assess risks", "Analyze options", "Evaluate", "Choose" },
            "Risk assessment added for better decisions");

        engine.ExecuteProcess("DecisionMaking", 0.75);
        engine.DisplayProcessStatus("DecisionMaking");

        Console.WriteLine("\n  Adaptation #2 - Feedback: Add stakeholder input");
        engine.AdaptProcessBasedOnFeedback("DecisionMaking",
            new List<string> { "Gather info", "Consult stakeholders", "Assess risks", "Analyze options", "Evaluate", "Choose" },
            "Stakeholder consultation improves outcomes");

        for (int i = 0; i < 3; i++)
        {
            engine.ExecuteProcess("DecisionMaking", 0.82 + (i * 0.02));
        }
        engine.DisplayProcessStatus("DecisionMaking");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Automatization Through Repetition]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  ProblemSolving process automation tracking:");
        for (int i = 0; i < 6; i++)
        {
            double score = 0.60 + (i * 0.05);
            engine.ExecuteProcess("ProblemSolving", score);
        }
        engine.DisplayProcessStatus("ProblemSolving");

        Console.WriteLine("\n  Automatization progression:");
        Console.WriteLine("    0-3 repetitions:   0-30% automatized (conscious effort)");
        Console.WriteLine("    4-7 repetitions:   30-60% automatized (semi-automatic)");
        Console.WriteLine("    8+ repetitions:    60%+ automatized (habitual execution)");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Comparative Process Performance]");
        Console.ResetColor();
        Thread.Sleep(500);

        var rankings = engine.GetProcessRankings();
        int rank = 1;
        foreach (var kvp in rankings)
        {
            int adaptCount = engine.CountAdaptations(kvp.Key);
            double avgImprovement = engine.GetAverageImprovement(kvp.Key);
            Console.WriteLine($"  #{rank}: {kvp.Key}");
            Console.WriteLine($"         Efficiency: {kvp.Value * 100:F1}% | Adaptations: {adaptCount} | Avg improvement: +{avgImprovement:F2}%");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Adaptation History and Learning]");
        Console.ResetColor();
        Thread.Sleep(500);

        foreach (var process in new[] { "DataProcessing", "DecisionMaking", "ProblemSolving" })
        {
            engine.DisplayAdaptationHistory(process);
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Process Optimization Recommendations]");
        Console.ResetColor();
        Thread.Sleep(500);

        var opportunities = engine.IdentifyOptimizationOpportunities();
        Console.WriteLine("  Optimization Opportunities:");
        if (opportunities.Count == 0)
        {
            Console.WriteLine("    ✓ All processes are well-consolidated and efficient");
        }
        else
        {
            foreach (var opp in opportunities)
            {
                Console.WriteLine($"    • {opp}");
            }
        }

        Console.WriteLine("\n  Process Development Model:");
        Console.WriteLine("    Stage 1: Create process with initial steps");
        Console.WriteLine("    Stage 2: Execute repeatedly, gather performance data");
        Console.WriteLine("    Stage 3: Analyze feedback, identify inefficiencies");
        Console.WriteLine("    Stage 4: Adapt steps based on learning");
        Console.WriteLine("    Stage 5: Consolidate through more repetitions");
        Console.WriteLine("    Stage 6: Achieve automatization and mastery");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Process development system complete");
        Console.ResetColor();
    }
}
