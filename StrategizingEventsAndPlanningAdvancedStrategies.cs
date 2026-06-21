using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class StrategizingEventsAndPlanningAdvancedStrategies
{
    public class StrategicObjective
    {
        public string ObjectiveId { get; set; }
        public string ObjectiveName { get; set; }
        public string Description { get; set; }
        public double ImportanceLevel { get; set; }
        public int Priority { get; set; }
        public List<string> SuccessCriteria { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StrategicEvent
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public DateTime PlannedDate { get; set; }
        public List<string> RelatedObjectives { get; set; }
        public string EventPhase { get; set; }
        public double SuccessProbability { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ActionPlan
    {
        public string PlanId { get; set; }
        public string PlanName { get; set; }
        public List<string> ActionSteps { get; set; }
        public Dictionary<string, double> RiskFactors { get; set; }
        public double OverallRiskLevel { get; set; }
        public List<string> ContingencyPlans { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StrategyAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalObjectives { get; set; }
        public int TotalEvents { get; set; }
        public int TotalPlans { get; set; }
        public double AverageSuccessProbability { get; set; }
        public double AverageRiskLevel { get; set; }
        public List<string> CriticalPathItems { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class StrategyPlanningEngine
    {
        private Dictionary<string, StrategicObjective> objectives;
        private Dictionary<string, StrategicEvent> events;
        private Dictionary<string, ActionPlan> plans;
        private Dictionary<string, StrategyAnalysis> analyses;

        public StrategyPlanningEngine()
        {
            objectives = new Dictionary<string, StrategicObjective>();
            events = new Dictionary<string, StrategicEvent>();
            plans = new Dictionary<string, ActionPlan>();
            analyses = new Dictionary<string, StrategyAnalysis>();
        }

        public void DefineObjective(string objectiveId, string objectiveName, string description,
                                   double importance, int priority, List<string> criteria)
        {
            var objective = new StrategicObjective
            {
                ObjectiveId = objectiveId,
                ObjectiveName = objectiveName,
                Description = description,
                ImportanceLevel = importance,
                Priority = priority,
                SuccessCriteria = new List<string>(criteria),
                CreatedDate = DateTime.Now
            };
            objectives[objectiveId] = objective;
        }

        public void PlanStrategicEvent(string eventId, string eventName, DateTime plannedDate,
                                      List<string> objectiveIds, string phase)
        {
            var strategicEvent = new StrategicEvent
            {
                EventId = eventId,
                EventName = eventName,
                PlannedDate = plannedDate,
                RelatedObjectives = new List<string>(objectiveIds),
                EventPhase = phase,
                SuccessProbability = CalculateEventProbability(objectiveIds),
                CreatedDate = DateTime.Now
            };
            events[eventId] = strategicEvent;
        }

        private double CalculateEventProbability(List<string> objectiveIds)
        {
            if (objectiveIds.Count == 0) return 0.5;

            double totalImportance = 0.0;
            foreach (var objId in objectiveIds)
            {
                if (objectives.ContainsKey(objId))
                {
                    totalImportance += objectives[objId].ImportanceLevel;
                }
            }

            return Math.Min(totalImportance / objectiveIds.Count, 1.0);
        }

        public void CreateActionPlan(string planId, string planName, List<string> actionSteps,
                                    Dictionary<string, double> risks, List<string> contingencies)
        {
            var plan = new ActionPlan
            {
                PlanId = planId,
                PlanName = planName,
                ActionSteps = new List<string>(actionSteps),
                RiskFactors = new Dictionary<string, double>(risks),
                OverallRiskLevel = CalculateOverallRisk(risks),
                ContingencyPlans = new List<string>(contingencies),
                CreatedDate = DateTime.Now
            };
            plans[planId] = plan;
        }

        private double CalculateOverallRisk(Dictionary<string, double> risks)
        {
            if (risks.Count == 0) return 0.0;

            double totalRisk = 0.0;
            foreach (var risk in risks.Values)
            {
                totalRisk += risk;
            }

            return Math.Min(totalRisk / risks.Count, 1.0);
        }

        public void AnalyzeStrategy(string analysisId)
        {
            var analysis = new StrategyAnalysis
            {
                AnalysisId = analysisId,
                TotalObjectives = objectives.Count,
                TotalEvents = events.Count,
                TotalPlans = plans.Count,
                AverageSuccessProbability = events.Count > 0 ? events.Values.Average(e => e.SuccessProbability) : 0.0,
                AverageRiskLevel = plans.Count > 0 ? plans.Values.Average(p => p.OverallRiskLevel) : 0.0,
                CriticalPathItems = IdentifyCriticalPath(),
                AnalyzedDate = DateTime.Now
            };
            analyses[analysisId] = analysis;
        }

        private List<string> IdentifyCriticalPath()
        {
            var criticalItems = new List<string>();

            var highPriorityObjectives = objectives.Values.Where(o => o.Priority <= 2).ToList();
            foreach (var obj in highPriorityObjectives)
            {
                criticalItems.Add(obj.ObjectiveName);
            }

            var highRiskPlans = plans.Values.Where(p => p.OverallRiskLevel > 0.6).ToList();
            foreach (var plan in highRiskPlans)
            {
                criticalItems.Add(plan.PlanName);
            }

            return criticalItems;
        }

        public void DisplayObjective(string objectiveId)
        {
            if (!objectives.ContainsKey(objectiveId)) return;

            var obj = objectives[objectiveId];
            Console.WriteLine($"\n  Strategic Objective: {obj.ObjectiveName}");
            Console.WriteLine($"  Priority: {obj.Priority}");
            Console.WriteLine($"  Importance: {obj.ImportanceLevel * 100:F0}%");
            Console.WriteLine($"  Success Criteria: {string.Join(", ", obj.SuccessCriteria.Take(2))}");
        }

        public void DisplayEvent(string eventId)
        {
            if (!events.ContainsKey(eventId)) return;

            var evt = events[eventId];
            Console.WriteLine($"\n  Strategic Event: {evt.EventName}");
            Console.WriteLine($"  Planned Date: {evt.PlannedDate:yyyy-MM-dd}");
            Console.WriteLine($"  Phase: {evt.EventPhase}");
            Console.WriteLine($"  Success Probability: {evt.SuccessProbability * 100:F0}%");
        }

        public void DisplayPlan(string planId)
        {
            if (!plans.ContainsKey(planId)) return;

            var plan = plans[planId];
            Console.WriteLine($"\n  Action Plan: {plan.PlanName}");
            Console.WriteLine($"  Action Steps: {plan.ActionSteps.Count}");
            Console.WriteLine($"  Overall Risk: {plan.OverallRiskLevel * 100:F0}%");
            Console.WriteLine($"  Contingencies: {plan.ContingencyPlans.Count}");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Strategy Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Objectives: {analysis.TotalObjectives}");
            Console.WriteLine($"  Total Events: {analysis.TotalEvents}");
            Console.WriteLine($"  Total Plans: {analysis.TotalPlans}");
            Console.WriteLine($"  Average Success Probability: {analysis.AverageSuccessProbability * 100:F0}%");
            Console.WriteLine($"  Average Risk Level: {analysis.AverageRiskLevel * 100:F0}%");
            Console.WriteLine($"  Critical Items: {analysis.CriticalPathItems.Count}");
        }

        public int GetTotalObjectives()
        {
            return objectives.Count;
        }

        public int GetTotalEvents()
        {
            return events.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Strategizing Events and Planning Advanced Strategies         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new StrategyPlanningEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Strategic Objectives]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DefineObjective("OBJ-001", "Market Expansion",
            "Expand to three new markets", 0.95, 1,
            new List<string> { "Revenue increase 40%", "Market share 25%" });

        engine.DefineObjective("OBJ-002", "Product Development",
            "Launch new product line", 0.90, 2,
            new List<string> { "Product released on time", "Feature complete" });

        engine.DefineObjective("OBJ-003", "Team Growth",
            "Expand team capabilities", 0.85, 3,
            new List<string> { "Hire 10 engineers", "Training complete" });

        Console.WriteLine("  ✓ Defined 3 strategic objectives");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Objectives]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayObjective("OBJ-001");
        engine.DisplayObjective("OBJ-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Planning Strategic Events]");
        Console.ResetColor();
        Thread.Sleep(500);

        var q2Date = DateTime.Now.AddMonths(3);
        var q3Date = DateTime.Now.AddMonths(6);

        engine.PlanStrategicEvent("EVT-001", "Q2 Market Launch",
            q2Date, new List<string> { "OBJ-001" }, "Execution");

        engine.PlanStrategicEvent("EVT-002", "Q3 Product Release",
            q3Date, new List<string> { "OBJ-002", "OBJ-003" }, "Release");

        Console.WriteLine("  ✓ Planned 2 strategic events");
        engine.DisplayEvent("EVT-001");
        engine.DisplayEvent("EVT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Creating Action Plans]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateActionPlan("PLAN-001", "Market Expansion Plan",
            new List<string> { "Research markets", "Identify partners", "Deploy resources", "Monitor progress" },
            new Dictionary<string, double> { { "Market resistance", 0.3 }, { "Resource constraints", 0.4 } },
            new List<string> { "Rapid scaling", "Alternative markets" });

        engine.CreateActionPlan("PLAN-002", "Product Development Plan",
            new List<string> { "Gather requirements", "Design architecture", "Implement features", "QA testing" },
            new Dictionary<string, double> { { "Schedule delay", 0.5 }, { "Technical complexity", 0.6 } },
            new List<string> { "Phased release", "Extended timeline" });

        Console.WriteLine("  ✓ Created 2 action plans");
        engine.DisplayPlan("PLAN-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Strategy]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeStrategy("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Strategy Planning Summary:");
        Console.WriteLine($"    Strategic Objectives: {engine.GetTotalObjectives()}");
        Console.WriteLine($"    Planned Events: {engine.GetTotalEvents()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Strategic Planning Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Strategy Planning Architecture:");
        Console.WriteLine("    Layer 1: Objective Definition (establish goals)");
        Console.WriteLine("    Layer 2: Priority Assignment (rank objectives)");
        Console.WriteLine("    Layer 3: Event Planning (schedule strategic events)");
        Console.WriteLine("    Layer 4: Probability Assessment (evaluate success)");
        Console.WriteLine("    Layer 5: Action Planning (define steps)");
        Console.WriteLine("    Layer 6: Risk Analysis (identify risks)");
        Console.WriteLine("    Layer 7: Critical Path Identification (highlight key items)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define strategic objectives with success criteria");
        Console.WriteLine("    ✓ Plan strategic events with probability assessment");
        Console.WriteLine("    ✓ Create detailed action plans with steps");
        Console.WriteLine("    ✓ Identify and quantify risk factors");
        Console.WriteLine("    ✓ Develop contingency strategies");
        Console.WriteLine("    ✓ Analyze critical path items");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Strategic planning system complete");
        Console.ResetColor();
    }
}
