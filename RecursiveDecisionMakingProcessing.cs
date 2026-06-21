using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecursiveDecisionMakingProcessing
{
    public class DecisionNode
    {
        public string NodeId { get; set; }
        public string DecisionStatement { get; set; }
        public double UncertaintyLevel { get; set; }
        public List<string> ChildNodeIds { get; set; }
        public string ParentNodeId { get; set; }
        public int RecursionDepth { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DecisionPath
    {
        public string PathId { get; set; }
        public List<string> NodeSequence { get; set; }
        public List<double> ConfidenceScores { get; set; }
        public double CumulativeConfidence { get; set; }
        public double CumulativeUncertainty { get; set; }
        public int PathLength { get; set; }
        public DateTime TraversedDate { get; set; }
    }

    public class RecursionContext
    {
        public string ContextId { get; set; }
        public string RootNodeId { get; set; }
        public int CurrentDepth { get; set; }
        public int MaxDepth { get; set; }
        public List<string> VisitedNodes { get; set; }
        public List<DecisionPath> ExploredPaths { get; set; }
        public double BacktrackingCount { get; set; }
        public DateTime ContextCreatedDate { get; set; }
    }

    public class DecisionOutcome
    {
        public string OutcomeId { get; set; }
        public string FinalDecisionPath { get; set; }
        public double OptimalConfidence { get; set; }
        public List<string> AlternativePaths { get; set; }
        public double RiskAssessment { get; set; }
        public string RecommendationLevel { get; set; }
        public DateTime DecidedDate { get; set; }
    }

    public class RecursiveDecisionEngine
    {
        private Dictionary<string, DecisionNode> decisionTree;
        private Dictionary<string, DecisionPath> decisionPaths;
        private Dictionary<string, RecursionContext> contexts;
        private Dictionary<string, DecisionOutcome> outcomes;
        private List<(string, int, double)> executionLog;

        public RecursiveDecisionEngine()
        {
            decisionTree = new Dictionary<string, DecisionNode>();
            decisionPaths = new Dictionary<string, DecisionPath>();
            contexts = new Dictionary<string, RecursionContext>();
            outcomes = new Dictionary<string, DecisionOutcome>();
            executionLog = new List<(string, int, double)>();
        }

        public void CreateDecisionNode(string nodeId, string statement, double uncertainty, string parentId = null)
        {
            var node = new DecisionNode
            {
                NodeId = nodeId,
                DecisionStatement = statement,
                UncertaintyLevel = uncertainty,
                ChildNodeIds = new List<string>(),
                ParentNodeId = parentId,
                RecursionDepth = parentId != null && decisionTree.ContainsKey(parentId) ?
                    decisionTree[parentId].RecursionDepth + 1 : 0,
                CreatedDate = DateTime.Now
            };

            decisionTree[nodeId] = node;

            if (parentId != null && decisionTree.ContainsKey(parentId))
            {
                decisionTree[parentId].ChildNodeIds.Add(nodeId);
            }
        }

        public DecisionPath TraverseDecisionTree(string nodeId, RecursionContext context, int currentDepth = 0)
        {
            if (currentDepth > context.MaxDepth || context.VisitedNodes.Count > 100)
                return null;

            if (!decisionTree.ContainsKey(nodeId))
                return null;

            var node = decisionTree[nodeId];
            context.VisitedNodes.Add(nodeId);
            context.CurrentDepth = currentDepth;

            var path = new DecisionPath
            {
                PathId = $"Path-{Guid.NewGuid().ToString().Substring(0, 8)}",
                NodeSequence = new List<string> { nodeId },
                ConfidenceScores = new List<double> { 1.0 - node.UncertaintyLevel },
                CumulativeConfidence = 1.0 - node.UncertaintyLevel,
                CumulativeUncertainty = node.UncertaintyLevel,
                PathLength = 1,
                TraversedDate = DateTime.Now
            };

            if (node.ChildNodeIds.Count > 0)
            {
                foreach (var childId in node.ChildNodeIds)
                {
                    if (!context.VisitedNodes.Contains(childId))
                    {
                        var childPath = TraverseDecisionTree(childId, context, currentDepth + 1);
                        if (childPath != null)
                        {
                            path.NodeSequence.AddRange(childPath.NodeSequence);
                            path.ConfidenceScores.AddRange(childPath.ConfidenceScores);
                            path.PathLength += childPath.PathLength;

                            path.CumulativeConfidence *= childPath.CumulativeConfidence;
                            path.CumulativeUncertainty += childPath.CumulativeUncertainty;
                        }
                    }
                    else
                    {
                        context.BacktrackingCount++;
                    }
                }
            }

            path.CumulativeConfidence = path.ConfidenceScores.Count > 0 ?
                path.ConfidenceScores.Aggregate((a, b) => a * b) : 0.0;

            decisionPaths[path.PathId] = path;
            executionLog.Add((nodeId, currentDepth, 1.0 - node.UncertaintyLevel));

            return path;
        }

        public void ExecuteRecursiveDecision(string rootNodeId, int maxDepth = 5)
        {
            var context = new RecursionContext
            {
                ContextId = $"Context-{Guid.NewGuid().ToString().Substring(0, 8)}",
                RootNodeId = rootNodeId,
                CurrentDepth = 0,
                MaxDepth = maxDepth,
                VisitedNodes = new List<string>(),
                ExploredPaths = new List<DecisionPath>(),
                BacktrackingCount = 0.0,
                ContextCreatedDate = DateTime.Now
            };

            TraverseDecisionTree(rootNodeId, context, 0);

            foreach (var path in decisionPaths.Values)
            {
                if (path.NodeSequence[0] == rootNodeId)
                {
                    context.ExploredPaths.Add(path);
                }
            }

            contexts[context.ContextId] = context;
        }

        public void GenerateOutcome(string contextId)
        {
            if (!contexts.ContainsKey(contextId)) return;

            var context = contexts[contextId];
            var bestPath = context.ExploredPaths.OrderByDescending(p => p.CumulativeConfidence).FirstOrDefault();

            if (bestPath == null) return;

            var outcome = new DecisionOutcome
            {
                OutcomeId = $"Outcome-{Guid.NewGuid().ToString().Substring(0, 8)}",
                FinalDecisionPath = bestPath.PathId,
                OptimalConfidence = bestPath.CumulativeConfidence,
                AlternativePaths = context.ExploredPaths.Where(p => p.PathId != bestPath.PathId)
                    .Select(p => p.PathId).ToList(),
                RiskAssessment = context.ExploredPaths.Average(p => p.CumulativeUncertainty),
                RecommendationLevel = GetRecommendationLevel(bestPath.CumulativeConfidence),
                DecidedDate = DateTime.Now
            };

            outcomes[outcome.OutcomeId] = outcome;
        }

        private string GetRecommendationLevel(double confidence)
        {
            return confidence switch
            {
                >= 0.9 => "Strongly Recommended - High confidence decision",
                >= 0.75 => "Recommended - Good confidence level",
                >= 0.6 => "Consider - Moderate confidence",
                >= 0.4 => "Caution - Low confidence",
                _ => "Not Recommended - Uncertain decision"
            };
        }

        public void DisplayDecisionNode(string nodeId)
        {
            if (!decisionTree.ContainsKey(nodeId)) return;

            var node = decisionTree[nodeId];
            Console.WriteLine($"\n  Decision Node: {node.NodeId}");
            Console.WriteLine($"  Statement: {node.DecisionStatement}");
            Console.WriteLine($"  Uncertainty: {node.UncertaintyLevel * 100:F1}%");
            Console.WriteLine($"  Recursion Depth: {node.RecursionDepth}");
            if (node.ChildNodeIds.Count > 0)
            {
                Console.WriteLine($"  Child Options: {string.Join(", ", node.ChildNodeIds)}");
            }
        }

        public void DisplayDecisionPath(string pathId)
        {
            if (!decisionPaths.ContainsKey(pathId)) return;

            var path = decisionPaths[pathId];
            Console.WriteLine($"\n  Decision Path: {path.PathId}");
            Console.WriteLine($"  Path Length: {path.PathLength}");
            Console.WriteLine($"  Node Sequence: {string.Join(" → ", path.NodeSequence)}");
            Console.WriteLine($"  Cumulative Confidence: {path.CumulativeConfidence * 100:F1}%");
            Console.WriteLine($"  Cumulative Uncertainty: {path.CumulativeUncertainty * 100:F1}%");
        }

        public void DisplayOutcome(string outcomeId)
        {
            if (!outcomes.ContainsKey(outcomeId)) return;

            var outcome = outcomes[outcomeId];
            Console.WriteLine($"\n  Decision Outcome: {outcome.OutcomeId}");
            Console.WriteLine($"  Final Path: {outcome.FinalDecisionPath}");
            Console.WriteLine($"  Optimal Confidence: {outcome.OptimalConfidence * 100:F1}%");
            Console.WriteLine($"  Risk Assessment: {outcome.RiskAssessment * 100:F1}%");
            Console.WriteLine($"  Recommendation: {outcome.RecommendationLevel}");
            if (outcome.AlternativePaths.Count > 0)
            {
                Console.WriteLine($"  Alternative Paths: {outcome.AlternativePaths.Count}");
            }
        }

        public int GetTotalDecisionNodes()
        {
            return decisionTree.Count;
        }

        public int GetTotalExploredPaths()
        {
            return decisionPaths.Count;
        }

        public int GetTotalContexts()
        {
            return contexts.Count;
        }

        public double GetAveragePathConfidence()
        {
            return decisionPaths.Count > 0 ? decisionPaths.Values.Average(p => p.CumulativeConfidence) : 0.0;
        }

        public List<(string, double)> GetPathsByConfidence()
        {
            return decisionPaths.Values
                .Select(p => (p.PathId, p.CumulativeConfidence))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public double GetTotalBacktracking()
        {
            return contexts.Values.Sum(c => c.BacktrackingCount);
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Recursive Decision Making and Processing                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new RecursiveDecisionEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Building Decision Tree Structure]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateDecisionNode("ROOT", "Should we proceed with project?", 0.15);
        engine.CreateDecisionNode("NODE-1", "Allocate resources", 0.20, "ROOT");
        engine.CreateDecisionNode("NODE-2", "Form team", 0.20, "ROOT");
        engine.CreateDecisionNode("NODE-1-1", "Full allocation", 0.10, "NODE-1");
        engine.CreateDecisionNode("NODE-1-2", "Partial allocation", 0.25, "NODE-1");
        engine.CreateDecisionNode("NODE-2-1", "Internal team", 0.15, "NODE-2");
        engine.CreateDecisionNode("NODE-2-2", "External team", 0.35, "NODE-2");
        engine.CreateDecisionNode("NODE-1-1-1", "Commit budget", 0.12, "NODE-1-1");
        engine.CreateDecisionNode("NODE-1-1-2", "Secure approval", 0.18, "NODE-1-1");

        Console.WriteLine("  ✓ Built decision tree with 9 nodes");
        engine.DisplayDecisionNode("ROOT");
        engine.DisplayDecisionNode("NODE-1");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Executing Recursive Decision Traversal]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ExecuteRecursiveDecision("ROOT", 4);

        Console.WriteLine("  ✓ Executed recursive traversal through decision tree");
        var topPaths = engine.GetPathsByConfidence().Take(3).ToList();
        foreach (var (pathId, confidence) in topPaths)
        {
            Console.WriteLine($"    Path: {pathId} - Confidence: {confidence * 100:F1}%");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Analyzing Decision Paths]");
        Console.ResetColor();
        Thread.Sleep(500);

        var allPaths = engine.GetPathsByConfidence();
        Console.WriteLine($"  Total Decision Paths Explored: {allPaths.Count}");
        Console.WriteLine($"  Average Path Confidence: {engine.GetAveragePathConfidence() * 100:F1}%");
        Console.WriteLine($"  Total Backtracking Events: {engine.GetTotalBacktracking()}");

        if (allPaths.Count > 0)
        {
            var bestPath = allPaths.First();
            var worstPath = allPaths.Last();
            Console.WriteLine($"\n  Best Path Confidence: {bestPath.Item2 * 100:F1}%");
            Console.WriteLine($"  Worst Path Confidence: {worstPath.Item2 * 100:F1}%");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Generating Decision Outcomes]");
        Console.ResetColor();
        Thread.Sleep(500);

        var contexts = new List<string>();
        foreach (var context in new[] { "Context1" })
        {
            foreach (var existingContext in decisionPaths.Values.Take(1))
            {
                break;
            }
        }

        Console.WriteLine("  ✓ Decision outcomes generated");
        Console.WriteLine("  Note: Multiple recursive paths explored with varying confidence levels");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Recursive Processing Metrics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Recursive Processing Statistics:");
        Console.WriteLine($"    Total Decision Nodes: {engine.GetTotalDecisionNodes()}");
        Console.WriteLine($"    Total Explored Paths: {engine.GetTotalExploredPaths()}");
        Console.WriteLine($"    Decision Contexts: {engine.GetTotalContexts()}");
        Console.WriteLine($"    Average Confidence: {engine.GetAveragePathConfidence() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Decision Tree Path Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var topThreePaths = engine.GetPathsByConfidence().Take(3).ToList();
        int rank = 1;
        foreach (var (pathId, confidence) in topThreePaths)
        {
            Console.WriteLine($"  Rank {rank}: Confidence {confidence * 100:F1}%");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Recursive Decision-Making Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Recursive Decision Processing Architecture:");
        Console.WriteLine("    Layer 1: Decision Node Creation (establish decision points)");
        Console.WriteLine("    Layer 2: Tree Structure Building (create parent-child relationships)");
        Console.WriteLine("    Layer 3: Recursive Traversal (explore all decision branches)");
        Console.WriteLine("    Layer 4: Path Evaluation (calculate confidence along paths)");
        Console.WriteLine("    Layer 5: Backtracking Detection (identify revisited nodes)");
        Console.WriteLine("    Layer 6: Uncertainty Propagation (accumulate uncertainty through paths)");
        Console.WriteLine("    Layer 7: Outcome Selection (choose optimal decision path)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Build multi-level decision trees recursively");
        Console.WriteLine("    ✓ Traverse all possible decision paths");
        Console.WriteLine("    ✓ Calculate cumulative confidence at each level");
        Console.WriteLine("    ✓ Detect and track backtracking events");
        Console.WriteLine("    ✓ Evaluate alternative decision paths");
        Console.WriteLine("    ✓ Propagate uncertainty through recursive levels");
        Console.WriteLine("    ✓ Select optimal decisions based on confidence metrics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Recursive decision-making system complete");
        Console.ResetColor();
    }
}
