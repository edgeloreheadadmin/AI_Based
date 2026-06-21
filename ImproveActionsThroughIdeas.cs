using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ImproveActionsThroughIdeas
{
    public class Action
    {
        public string ActionName { get; set; }
        public string Description { get; set; }
        public double Effectiveness { get; set; }
        public double Efficiency { get; set; }
        public int ExecutionCount { get; set; }
        public List<string> RelatedIdeas { get; set; }
        public DateTime FirstExecuted { get; set; }
    }

    public class Idea
    {
        public string IdeaTitle { get; set; }
        public string IdeaDescription { get; set; }
        public double Innovativeness { get; set; }
        public double ApplicabilityScore { get; set; }
        public List<string> ImplementationSteps { get; set; }
        public List<string> AffectedActions { get; set; }
        public DateTime Created { get; set; }
    }

    public class Condition
    {
        public string ConditionName { get; set; }
        public string ConditionType { get; set; }
        public double CurrentLevel { get; set; }
        public double OptimalLevel { get; set; }
        public string Impact { get; set; }
        public DateTime AssessedDate { get; set; }
    }

    public class ActionOptimizationEngine
    {
        private Dictionary<string, Action> actions;
        private Dictionary<string, Idea> ideas;
        private Dictionary<string, Condition> conditions;
        private List<(string action, string idea, double improvement)> improvementLog;

        public ActionOptimizationEngine()
        {
            actions = new Dictionary<string, Action>();
            ideas = new Dictionary<string, Idea>();
            conditions = new Dictionary<string, Condition>();
            improvementLog = new List<(string, string, double)>();
        }

        public void DefineAction(string actionName, string description)
        {
            var action = new Action
            {
                ActionName = actionName,
                Description = description,
                Effectiveness = 0.5,
                Efficiency = 0.5,
                ExecutionCount = 0,
                RelatedIdeas = new List<string>(),
                FirstExecuted = DateTime.Now
            };
            actions[actionName] = action;
        }

        public void ExecuteAction(string actionName, double performanceScore)
        {
            if (!actions.ContainsKey(actionName)) return;

            var action = actions[actionName];
            action.ExecutionCount++;

            double baseImprovement = performanceScore * 0.1;
            action.Effectiveness = Math.Min(action.Effectiveness + (baseImprovement * 0.6), 0.99);
            action.Efficiency = Math.Min(action.Efficiency + (baseImprovement * 0.4), 0.99);
        }

        public void GenerateImprovementIdea(string ideaTitle, string description,
                                          double innovativeness, List<string> steps)
        {
            var idea = new Idea
            {
                IdeaTitle = ideaTitle,
                IdeaDescription = description,
                Innovativeness = Math.Min(innovativeness, 1.0),
                ApplicabilityScore = 0.6,
                ImplementationSteps = new List<string>(steps),
                AffectedActions = new List<string>(),
                Created = DateTime.Now
            };
            ideas[ideaTitle] = idea;
        }

        public void ApplyIdeaToAction(string ideaTitle, string actionName)
        {
            if (!ideas.ContainsKey(ideaTitle) || !actions.ContainsKey(actionName))
                return;

            var idea = ideas[ideaTitle];
            var action = actions[actionName];

            if (!action.RelatedIdeas.Contains(ideaTitle))
                action.RelatedIdeas.Add(ideaTitle);

            if (!idea.AffectedActions.Contains(actionName))
                idea.AffectedActions.Add(actionName);

            double improvementBoost = idea.Innovativeness * 0.25;
            action.Effectiveness = Math.Min(action.Effectiveness + improvementBoost, 0.99);
            action.Efficiency = Math.Min(action.Efficiency + (improvementBoost * 0.8), 0.99);

            improvementLog.Add((actionName, ideaTitle, improvementBoost));
        }

        public void SetEnvironmentalCondition(string conditionName, string conditionType,
                                             double currentLevel, double optimalLevel, string impact)
        {
            var condition = new Condition
            {
                ConditionName = conditionName,
                ConditionType = conditionType,
                CurrentLevel = Math.Min(currentLevel, 1.0),
                OptimalLevel = Math.Min(optimalLevel, 1.0),
                Impact = impact,
                AssessedDate = DateTime.Now
            };
            conditions[conditionName] = condition;
        }

        public void OptimizeConditions(string conditionName, double improvementRate)
        {
            if (!conditions.ContainsKey(conditionName)) return;

            var condition = conditions[conditionName];
            double gap = condition.OptimalLevel - condition.CurrentLevel;
            condition.CurrentLevel = Math.Min(condition.CurrentLevel + (gap * improvementRate), condition.OptimalLevel);
        }

        public double CalculateActionOptimization(string actionName)
        {
            if (!actions.ContainsKey(actionName)) return 0.0;

            var action = actions[actionName];
            double baseScore = (action.Effectiveness + action.Efficiency) / 2.0;

            double ideaMultiplier = 1.0 + (action.RelatedIdeas.Count * 0.15);

            double conditionAdjustment = 1.0;
            foreach (var condition in conditions.Values)
            {
                double conditionScore = condition.CurrentLevel / (condition.OptimalLevel > 0 ? condition.OptimalLevel : 1.0);
                conditionAdjustment *= (0.5 + conditionScore * 0.5);
            }

            double optimizedScore = baseScore * ideaMultiplier * conditionAdjustment;
            return Math.Min(optimizedScore, 1.0);
        }

        public void DisplayActionStatus(string actionName)
        {
            if (!actions.ContainsKey(actionName)) return;

            var action = actions[actionName];
            Console.WriteLine($"\n  Action: {action.ActionName}");
            Console.WriteLine($"  Description: {action.Description}");
            Console.WriteLine($"  Effectiveness: {action.Effectiveness * 100:F1}%");
            Console.WriteLine($"  Efficiency: {action.Efficiency * 100:F1}%");
            Console.WriteLine($"  Executions: {action.ExecutionCount}");
            if (action.RelatedIdeas.Count > 0)
            {
                Console.WriteLine($"  Improvement Ideas: {string.Join(", ", action.RelatedIdeas)}");
            }
        }

        public void DisplayIdeaStatus(string ideaTitle)
        {
            if (!ideas.ContainsKey(ideaTitle)) return;

            var idea = ideas[ideaTitle];
            Console.WriteLine($"\n  Idea: {idea.IdeaTitle}");
            Console.WriteLine($"  Description: {idea.IdeaDescription}");
            Console.WriteLine($"  Innovativeness: {idea.Innovativeness * 100:F1}%");
            Console.WriteLine($"  Applicability: {idea.ApplicabilityScore * 100:F1}%");
            Console.WriteLine($"  Implementation Steps:");
            foreach (var step in idea.ImplementationSteps)
            {
                Console.WriteLine($"    • {step}");
            }
            if (idea.AffectedActions.Count > 0)
            {
                Console.WriteLine($"  Affects Actions: {string.Join(", ", idea.AffectedActions)}");
            }
        }

        public void DisplayConditionStatus()
        {
            Console.WriteLine("\n  Environmental Conditions:");
            foreach (var condition in conditions.Values)
            {
                double alignment = condition.CurrentLevel / (condition.OptimalLevel > 0 ? condition.OptimalLevel : 1.0);
                Console.WriteLine($"    {condition.ConditionName} ({condition.ConditionType}):");
                Console.WriteLine($"      Current: {condition.CurrentLevel * 100:F1}% | Optimal: {condition.OptimalLevel * 100:F1}%");
                Console.WriteLine($"      Alignment: {alignment * 100:F0}% | Impact: {condition.Impact}");
            }
        }

        public Dictionary<string, double> GetOptimizedActionRankings()
        {
            var rankings = new Dictionary<string, double>();
            foreach (var action in actions.Values)
            {
                rankings[action.ActionName] = CalculateActionOptimization(action.ActionName);
            }
            return rankings.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public List<string> GetHighestImpactIdeas()
        {
            return ideas.OrderByDescending(i => i.Value.Innovativeness * i.Value.AffectedActions.Count)
                .Select(i => i.Key).Take(5).ToList();
        }

        public double GetSystemOptimization()
        {
            if (actions.Count == 0) return 0.0;
            return actions.Values.Average(a => CalculateActionOptimization(a.ActionName));
        }

        public string GetOptimizationLevel(double optimization)
        {
            return optimization switch
            {
                >= 0.85 => "Peak Optimization - Exceptional action performance",
                >= 0.70 => "Well Optimized - Strong improvement achieved",
                >= 0.55 => "Good Progress - Meaningful enhancement",
                >= 0.40 => "Developing - Noticeable improvements",
                _ => "Beginning - Initial optimization phase"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Improve Actions Through Ideas and Environmental Conditions    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new ActionOptimizationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Core Actions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DefineAction("Communication", "Exchanging information clearly and effectively");
        engine.DefineAction("Problem-Solving", "Identifying and resolving challenges systematically");
        engine.DefineAction("Decision-Making", "Evaluating options and committing to best choice");
        engine.DefineAction("Collaboration", "Working effectively with others toward shared goals");

        Console.WriteLine("  ✓ Defined 4 core actions");
        engine.DisplayActionStatus("Communication");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Setting Environmental Conditions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.SetEnvironmentalCondition("Focus", "Mental", 0.65, 0.95, "Higher focus enables better execution");
        engine.SetEnvironmentalCondition("Energy", "Physical", 0.70, 0.90, "Energy level affects action quality");
        engine.SetEnvironmentalCondition("Clarity", "Cognitive", 0.60, 0.95, "Clarity improves decision-making");
        engine.SetEnvironmentalCondition("Support", "Social", 0.75, 0.90, "Support systems enable collaboration");

        Console.WriteLine("  ✓ Set 4 environmental conditions with optimal levels");
        engine.DisplayConditionStatus();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Generating Improvement Ideas]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.GenerateImprovementIdea("Active Listening",
            "Focus fully on understanding others before responding",
            0.85,
            new List<string> { "Listen without interrupting", "Ask clarifying questions", "Summarize understanding" });

        engine.GenerateImprovementIdea("Structured Thinking",
            "Organize thoughts systematically to improve problem-solving",
            0.80,
            new List<string> { "Define problem clearly", "Break into components", "Analyze each part", "Synthesize solution" });

        engine.GenerateImprovementIdea("Collaborative Frameworks",
            "Use proven frameworks for better team outcomes",
            0.75,
            new List<string> { "Establish clear roles", "Define decision process", "Create feedback loops" });

        Console.WriteLine("  ✓ Generated 3 improvement ideas");
        engine.DisplayIdeaStatus("Active Listening");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Baseline Action Execution]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Executing baseline actions:");
        engine.ExecuteAction("Communication", 0.70);
        engine.ExecuteAction("Problem-Solving", 0.65);
        engine.ExecuteAction("Decision-Making", 0.60);
        engine.ExecuteAction("Collaboration", 0.75);

        Console.WriteLine("  ✓ Baseline performance established");
        engine.DisplayActionStatus("Communication");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Applying Ideas to Actions]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Applying improvement ideas to actions:");
        engine.ApplyIdeaToAction("Active Listening", "Communication");
        engine.ApplyIdeaToAction("Active Listening", "Collaboration");
        engine.ApplyIdeaToAction("Structured Thinking", "Problem-Solving");
        engine.ApplyIdeaToAction("Structured Thinking", "Decision-Making");
        engine.ApplyIdeaToAction("Collaborative Frameworks", "Collaboration");

        Console.WriteLine("  ✓ Ideas applied to target actions");
        engine.DisplayActionStatus("Communication");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Optimizing Environmental Conditions]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Improving conditions to enhance action performance:");
        for (int i = 0; i < 5; i++)
        {
            engine.OptimizeConditions("Focus", 0.15);
            engine.OptimizeConditions("Energy", 0.12);
            engine.OptimizeConditions("Clarity", 0.18);
            engine.OptimizeConditions("Support", 0.10);
        }

        Console.WriteLine("  ✓ Environmental conditions optimized");
        engine.DisplayConditionStatus();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Action Optimization Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var optimizedActions = engine.GetOptimizedActionRankings();
        Console.WriteLine("  Optimized Action Performance:");
        int rank = 1;
        foreach (var kvp in optimizedActions)
        {
            double optimization = kvp.Value;
            string level = engine.GetOptimizationLevel(optimization);
            Console.WriteLine($"  #{rank}: {kvp.Key}");
            Console.WriteLine($"       Optimization: {optimization * 100:F1}%");
            Console.WriteLine($"       Status: {level}");
            rank++;
        }

        double systemOptimization = engine.GetSystemOptimization();
        Console.WriteLine($"\n  System-Wide Optimization: {systemOptimization * 100:F1}%");

        var topIdeas = engine.GetHighestImpactIdeas();
        Console.WriteLine($"\n  Highest Impact Ideas: {string.Join(", ", topIdeas)}");

        Console.WriteLine("\n  Action Improvement Model:");
        Console.WriteLine("    Layer 1: Define Core Actions (base capabilities)");
        Console.WriteLine("    Layer 2: Establish Baseline Performance (current state)");
        Console.WriteLine("    Layer 3: Generate Improvement Ideas (innovation)");
        Console.WriteLine("    Layer 4: Apply Ideas to Actions (implementation)");
        Console.WriteLine("    Layer 5: Optimize Environmental Conditions (support systems)");
        Console.WriteLine("    Layer 6: Measure Integrated Results (action × ideas × conditions)");
        Console.WriteLine("    Layer 7: Achieve Peak Performance (synergistic excellence)");
        Console.WriteLine("\n  Key Principles:");
        Console.WriteLine("    ✓ Ideas amplify action effectiveness");
        Console.WriteLine("    ✓ Conditions create enabling environments");
        Console.WriteLine("    ✓ Synergy multiplies improvement impact");
        Console.WriteLine("    ✓ System optimization exceeds individual gains");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Action improvement system complete");
        Console.ResetColor();
    }
}
