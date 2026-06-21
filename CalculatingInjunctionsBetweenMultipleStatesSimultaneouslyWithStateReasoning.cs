using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CalculatingInjunctionsBetweenMultipleStatesSimultaneouslyWithStateReasoning
{
    public class SystemState
    {
        public string StateId { get; set; }
        public string StateName { get; set; }
        public Dictionary<string, double> StateAttributes { get; set; }
        public double StateEnergy { get; set; }
        public double Stability { get; set; }
        public List<string> PossibleTransitions { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StateInjunction
    {
        public string InjunctionId { get; set; }
        public string SourceStateId { get; set; }
        public string TargetStateId { get; set; }
        public string InjunctionType { get; set; }
        public double InjunctionStrength { get; set; }
        public double ProbabilityScore { get; set; }
        public List<string> RequiredConditions { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StateReasoning
    {
        public string ReasoningId { get; set; }
        public string StateId { get; set; }
        public List<string> LogicalRules { get; set; }
        public Dictionary<string, double> RuleWeights { get; set; }
        public string ReasoningConclusion { get; set; }
        public double ConfidenceLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SimultaneousStateAnalysis
    {
        public string AnalysisId { get; set; }
        public List<string> ActiveStates { get; set; }
        public int TotalInjunctions { get; set; }
        public Dictionary<string, int> InjunctionTypeDistribution { get; set; }
        public double AverageInjunctionStrength { get; set; }
        public double SystemComplexity { get; set; }
        public List<string> ConflictingStates { get; set; }
        public string SystemState { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class StateInjunctionEngine
    {
        private Dictionary<string, SystemState> states;
        private Dictionary<string, StateInjunction> injunctions;
        private Dictionary<string, StateReasoning> reasonings;
        private Dictionary<string, SimultaneousStateAnalysis> analyses;

        public StateInjunctionEngine()
        {
            states = new Dictionary<string, SystemState>();
            injunctions = new Dictionary<string, StateInjunction>();
            reasonings = new Dictionary<string, StateReasoning>();
            analyses = new Dictionary<string, SimultaneousStateAnalysis>();
        }

        public void RegisterState(string stateId, string stateName, Dictionary<string, double> attributes,
                                 double energy, List<string> transitions)
        {
            var state = new SystemState
            {
                StateId = stateId,
                StateName = stateName,
                StateAttributes = new Dictionary<string, double>(attributes),
                StateEnergy = energy,
                Stability = CalculateStability(attributes),
                PossibleTransitions = new List<string>(transitions),
                CreatedDate = DateTime.Now
            };
            states[stateId] = state;
        }

        private double CalculateStability(Dictionary<string, double> attributes)
        {
            if (attributes.Count == 0) return 0.5;

            double average = attributes.Values.Average();
            double variance = attributes.Values.Sum(v => Math.Pow(v - average, 2)) / attributes.Count;
            double standardDeviation = Math.Sqrt(variance);

            return Math.Max(1.0 - standardDeviation, 0.0);
        }

        public void CreateInjunction(string injunctionId, string sourceStateId, string targetStateId,
                                    string injunctionType, List<string> conditions)
        {
            if (!states.ContainsKey(sourceStateId) || !states.ContainsKey(targetStateId)) return;

            var injunction = new StateInjunction
            {
                InjunctionId = injunctionId,
                SourceStateId = sourceStateId,
                TargetStateId = targetStateId,
                InjunctionType = injunctionType,
                InjunctionStrength = CalculateInjunctionStrength(sourceStateId, targetStateId),
                ProbabilityScore = CalculateTransitionProbability(sourceStateId, targetStateId),
                RequiredConditions = new List<string>(conditions),
                CreatedDate = DateTime.Now
            };

            injunctions[injunctionId] = injunction;
            states[sourceStateId].PossibleTransitions.Add(targetStateId);
        }

        private double CalculateInjunctionStrength(string sourceId, string targetId)
        {
            var source = states[sourceId];
            var target = states[targetId];

            double strength = 0.5;
            strength += (source.StateEnergy - target.StateEnergy) * 0.3;
            strength += target.Stability * 0.2;
            return Math.Min(Math.Max(strength, 0.0), 1.0);
        }

        private double CalculateTransitionProbability(string sourceId, string targetId)
        {
            var source = states[sourceId];
            var target = states[targetId];

            double probability = 0.5;
            probability += (1.0 - Math.Abs(source.StateEnergy - target.StateEnergy)) * 0.3;
            probability += target.Stability * 0.2;
            return Math.Min(probability, 1.0);
        }

        public void ApplyStateReasoning(string reasoningId, string stateId, List<string> logicalRules,
                                       Dictionary<string, double> ruleWeights)
        {
            if (!states.ContainsKey(stateId)) return;

            var reasoning = new StateReasoning
            {
                ReasoningId = reasoningId,
                StateId = stateId,
                LogicalRules = new List<string>(logicalRules),
                RuleWeights = new Dictionary<string, double>(ruleWeights),
                ReasoningConclusion = DeriveConclusion(logicalRules, states[stateId]),
                ConfidenceLevel = CalculateReasoningConfidence(ruleWeights),
                CreatedDate = DateTime.Now
            };

            reasonings[reasoningId] = reasoning;
        }

        private string DeriveConclusion(List<string> rules, SystemState state)
        {
            if (rules.Count == 0) return "No conclusion";

            bool allConditionsMet = rules.All(r => r.Length > 0);
            return allConditionsMet ? $"{state.StateName} is valid" : $"{state.StateName} requires conditions";
        }

        private double CalculateReasoningConfidence(Dictionary<string, double> weights)
        {
            if (weights.Count == 0) return 0.5;
            return Math.Min(weights.Values.Average(), 1.0);
        }

        public void AnalyzeSimultaneousStates(string analysisId, List<string> stateIds)
        {
            var analysis = new SimultaneousStateAnalysis
            {
                AnalysisId = analysisId,
                ActiveStates = new List<string>(stateIds),
                TotalInjunctions = 0,
                InjunctionTypeDistribution = new Dictionary<string, int>(),
                AverageInjunctionStrength = 0.0,
                SystemComplexity = 0.0,
                ConflictingStates = new List<string>(),
                SystemState = "Unknown",
                AnalyzedDate = DateTime.Now
            };

            var relevantInjunctions = injunctions.Values.Where(i =>
                stateIds.Contains(i.SourceStateId) && stateIds.Contains(i.TargetStateId)).ToList();

            analysis.TotalInjunctions = relevantInjunctions.Count;

            foreach (var injunction in relevantInjunctions)
            {
                if (!analysis.InjunctionTypeDistribution.ContainsKey(injunction.InjunctionType))
                    analysis.InjunctionTypeDistribution[injunction.InjunctionType] = 0;
                analysis.InjunctionTypeDistribution[injunction.InjunctionType]++;
            }

            analysis.AverageInjunctionStrength = relevantInjunctions.Count > 0 ?
                relevantInjunctions.Average(i => i.InjunctionStrength) : 0.0;

            analysis.SystemComplexity = CalculateSystemComplexity(stateIds);
            analysis.ConflictingStates = IdentifyConflicts(stateIds);
            analysis.SystemState = DetermineSystemState(analysis.ConflictingStates.Count, analysis.SystemComplexity);

            analyses[analysisId] = analysis;
        }

        private double CalculateSystemComplexity(List<string> stateIds)
        {
            double complexity = 0.0;
            foreach (var stateId in stateIds)
            {
                if (states.ContainsKey(stateId))
                    complexity += states[stateId].StateAttributes.Count * 0.1;
            }
            return Math.Min(complexity, 1.0);
        }

        private List<string> IdentifyConflicts(List<string> stateIds)
        {
            var conflicts = new List<string>();

            for (int i = 0; i < stateIds.Count; i++)
            {
                for (int j = i + 1; j < stateIds.Count; j++)
                {
                    var state1 = states[stateIds[i]];
                    var state2 = states[stateIds[j]];

                    double energyDiff = Math.Abs(state1.StateEnergy - state2.StateEnergy);
                    if (energyDiff > 0.5)
                    {
                        conflicts.Add($"{state1.StateName}-{state2.StateName}");
                    }
                }
            }

            return conflicts;
        }

        private string DetermineSystemState(int conflictCount, double complexity)
        {
            if (conflictCount > 0) return "Conflicted";
            if (complexity > 0.7) return "Complex";
            if (complexity > 0.4) return "Moderate";
            return "Stable";
        }

        public void DisplayState(string stateId)
        {
            if (!states.ContainsKey(stateId)) return;

            var state = states[stateId];
            Console.WriteLine($"\n  System State: {state.StateName}");
            Console.WriteLine($"  Energy Level: {state.StateEnergy * 100:F0}%");
            Console.WriteLine($"  Stability: {state.Stability * 100:F1}%");
            Console.WriteLine($"  Possible Transitions: {state.PossibleTransitions.Count}");
        }

        public void DisplayInjunction(string injunctionId)
        {
            if (!injunctions.ContainsKey(injunctionId)) return;

            var injunction = injunctions[injunctionId];
            Console.WriteLine($"\n  State Injunction: {injunction.InjunctionId}");
            Console.WriteLine($"  From: {injunction.SourceStateId} → To: {injunction.TargetStateId}");
            Console.WriteLine($"  Type: {injunction.InjunctionType}");
            Console.WriteLine($"  Strength: {injunction.InjunctionStrength * 100:F0}%");
            Console.WriteLine($"  Probability: {injunction.ProbabilityScore * 100:F0}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Simultaneous State Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Active States: {analysis.ActiveStates.Count}");
            Console.WriteLine($"  Total Injunctions: {analysis.TotalInjunctions}");
            Console.WriteLine($"  Average Strength: {analysis.AverageInjunctionStrength * 100:F0}%");
            Console.WriteLine($"  System Complexity: {analysis.SystemComplexity * 100:F1}%");
            Console.WriteLine($"  Conflicts: {analysis.ConflictingStates.Count}");
            Console.WriteLine($"  System State: {analysis.SystemState}");
        }

        public int GetTotalStates()
        {
            return states.Count;
        }

        public int GetTotalInjunctions()
        {
            return injunctions.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Calculating Injunctions Between Multiple States Simultaneously ║");
        Console.WriteLine("║                   With State Reasoning                         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new StateInjectionEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering System States]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterState("STATE-001", "Idle",
            new Dictionary<string, double> { { "Activity", 0.1 }, { "Stability", 0.9 } },
            0.2, new List<string> { "STATE-002" });

        engine.RegisterState("STATE-002", "Active",
            new Dictionary<string, double> { { "Activity", 0.8 }, { "Stability", 0.6 } },
            0.7, new List<string> { "STATE-001", "STATE-003" });

        engine.RegisterState("STATE-003", "Peak",
            new Dictionary<string, double> { { "Activity", 0.95 }, { "Stability", 0.3 } },
            0.95, new List<string> { "STATE-002" });

        engine.RegisterState("STATE-004", "Transition",
            new Dictionary<string, double> { { "Activity", 0.5 }, { "Stability", 0.5 } },
            0.5, new List<string> { "STATE-002", "STATE-003" });

        Console.WriteLine("  ✓ Registered 4 system states");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying States]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayState("STATE-001");
        engine.DisplayState("STATE-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating State Injunctions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateInjunction("INJ-001", "STATE-001", "STATE-002",
            "Activation", new List<string> { "Energy available", "Trigger received" });

        engine.CreateInjunction("INJ-002", "STATE-002", "STATE-003",
            "Escalation", new List<string> { "Sustained activity", "Conditions met" });

        engine.CreateInjunction("INJ-003", "STATE-003", "STATE-002",
            "De-escalation", new List<string> { "Resource limit", "Stability threshold" });

        engine.CreateInjunction("INJ-004", "STATE-002", "STATE-001",
            "Deactivation", new List<string> { "No activity", "Timeout reached" });

        Console.WriteLine("  ✓ Created 4 state injunctions");
        engine.DisplayInjunction("INJ-001");
        engine.DisplayInjunction("INJ-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Applying State Reasoning]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ApplyStateReasoning("REASON-001", "STATE-001",
            new List<string> { "System ready", "No conflicts" },
            new Dictionary<string, double> { { "Ready", 0.95 }, { "Conflict-free", 0.90 } });

        engine.ApplyStateReasoning("REASON-002", "STATE-002",
            new List<string> { "Resources available", "Stable operation" },
            new Dictionary<string, double> { { "Resources", 0.85 }, { "Stability", 0.75 } });

        Console.WriteLine("  ✓ Applied state reasoning");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Simultaneous States]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeSimultaneousStates("ANALYSIS-001",
            new List<string> { "STATE-001", "STATE-002", "STATE-003" });

        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  State Injection Summary:");
        Console.WriteLine($"    Total States: {engine.GetTotalStates()}");
        Console.WriteLine($"    Total Injunctions: {engine.GetTotalInjunctions()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: State Injunction Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  State Injunction Architecture:");
        Console.WriteLine("    Layer 1: State Registration (define system states)");
        Console.WriteLine("    Layer 2: Attribute Definition (establish state properties)");
        Console.WriteLine("    Layer 3: Stability Calculation (measure state stability)");
        Console.WriteLine("    Layer 4: Injunction Creation (define state transitions)");
        Console.WriteLine("    Layer 5: Probability Calculation (assess transition likelihood)");
        Console.WriteLine("    Layer 6: Reasoning Application (apply logical rules)");
        Console.WriteLine("    Layer 7: Simultaneous Analysis (analyze multi-state scenarios)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register and characterize system states");
        Console.WriteLine("    ✓ Define state transition injunctions");
        Console.WriteLine("    ✓ Calculate transition probabilities");
        Console.WriteLine("    ✓ Apply state reasoning and logical rules");
        Console.WriteLine("    ✓ Analyze multiple simultaneous states");
        Console.WriteLine("    ✓ Identify state conflicts");
        Console.WriteLine("    ✓ Measure system complexity");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ State injunction system complete");
        Console.ResetColor();
    }
}
