using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AnalyzingComplexDetailedStructures
{
    public class StructuralComponent
    {
        public string ComponentId { get; set; }
        public string ComponentName { get; set; }
        public string ComponentType { get; set; }
        public List<string> SubComponents { get; set; }
        public int HierarchicalLevel { get; set; }
        public double Complexity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class HierarchicalLevel
    {
        public int LevelDepth { get; set; }
        public string LevelName { get; set; }
        public List<string> ComponentsAtLevel { get; set; }
        public double AverageComplexity { get; set; }
        public int TotalComponents { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class ComponentRelationship
    {
        public string RelationshipId { get; set; }
        public string SourceComponentId { get; set; }
        public string TargetComponentId { get; set; }
        public string RelationshipType { get; set; }
        public double RelationshipStrength { get; set; }
        public bool IsBidirectional { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StructuralMetrics
    {
        public string MetricsId { get; set; }
        public string StructureId { get; set; }
        public int TotalComponents { get; set; }
        public int MaxDepth { get; set; }
        public double AverageComplexity { get; set; }
        public int TotalRelationships { get; set; }
        public double StructuralDensity { get; set; }
        public string ComplexityRating { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class ComplexStructureAnalyzer
    {
        private Dictionary<string, StructuralComponent> components;
        private Dictionary<string, ComponentRelationship> relationships;
        private Dictionary<string, HierarchicalLevel> hierarchyLevels;
        private Dictionary<string, StructuralMetrics> metrics;
        private List<(string, string, double)> analysisLog;

        public ComplexStructureAnalyzer()
        {
            components = new Dictionary<string, StructuralComponent>();
            relationships = new Dictionary<string, ComponentRelationship>();
            hierarchyLevels = new Dictionary<string, HierarchicalLevel>();
            metrics = new Dictionary<string, StructuralMetrics>();
            analysisLog = new List<(string, string, double)>();
        }

        public void RegisterComponent(string componentId, string name, string type, int level, double complexity)
        {
            var component = new StructuralComponent
            {
                ComponentId = componentId,
                ComponentName = name,
                ComponentType = type,
                SubComponents = new List<string>(),
                HierarchicalLevel = level,
                Complexity = complexity,
                CreatedDate = DateTime.Now
            };
            components[componentId] = component;

            if (!hierarchyLevels.ContainsKey(level.ToString()))
            {
                hierarchyLevels[level.ToString()] = new HierarchicalLevel
                {
                    LevelDepth = level,
                    LevelName = $"Level-{level}",
                    ComponentsAtLevel = new List<string>(),
                    AverageComplexity = 0.0,
                    TotalComponents = 0,
                    AnalyzedDate = DateTime.Now
                };
            }
            hierarchyLevels[level.ToString()].ComponentsAtLevel.Add(componentId);
        }

        public void CreateComponentHierarchy(string parentId, string childId)
        {
            if (components.ContainsKey(parentId) && components.ContainsKey(childId))
            {
                if (!components[parentId].SubComponents.Contains(childId))
                {
                    components[parentId].SubComponents.Add(childId);
                }
            }
        }

        public void CreateRelationship(string sourceId, string targetId, string type, double strength, bool bidirectional)
        {
            if (!components.ContainsKey(sourceId) || !components.ContainsKey(targetId))
                return;

            string relationshipId = $"Rel-{sourceId}-{targetId}";
            var relationship = new ComponentRelationship
            {
                RelationshipId = relationshipId,
                SourceComponentId = sourceId,
                TargetComponentId = targetId,
                RelationshipType = type,
                RelationshipStrength = strength,
                IsBidirectional = bidirectional,
                CreatedDate = DateTime.Now
            };
            relationships[relationshipId] = relationship;
            analysisLog.Add((sourceId, targetId, strength));
        }

        public void AnalyzeStructure(string rootComponentId)
        {
            if (!components.ContainsKey(rootComponentId)) return;

            var metrics = new StructuralMetrics
            {
                MetricsId = $"Metrics-{rootComponentId}",
                StructureId = rootComponentId,
                TotalComponents = 0,
                MaxDepth = 0,
                AverageComplexity = 0.0,
                TotalRelationships = 0,
                StructuralDensity = 0.0,
                ComplexityRating = "",
                AnalyzedDate = DateTime.Now
            };

            var visited = new HashSet<string>();
            CalculateStructureMetrics(rootComponentId, metrics, visited, 0);

            double totalComplexity = 0.0;
            foreach (var component in components.Values)
            {
                totalComplexity += component.Complexity;
            }
            metrics.AverageComplexity = components.Count > 0 ? totalComplexity / components.Count : 0.0;

            metrics.TotalRelationships = relationships.Count;
            metrics.StructuralDensity = components.Count > 1 ?
                (double)relationships.Count / (components.Count * (components.Count - 1)) : 0.0;

            metrics.ComplexityRating = GetComplexityRating(metrics.AverageComplexity, metrics.MaxDepth);

            this.metrics[metrics.MetricsId] = metrics;
        }

        private void CalculateStructureMetrics(string componentId, StructuralMetrics metrics,
                                              HashSet<string> visited, int currentDepth)
        {
            if (visited.Contains(componentId)) return;
            visited.Add(componentId);

            metrics.TotalComponents++;
            if (currentDepth > metrics.MaxDepth)
                metrics.MaxDepth = currentDepth;

            if (components.ContainsKey(componentId))
            {
                foreach (var subComponentId in components[componentId].SubComponents)
                {
                    CalculateStructureMetrics(subComponentId, metrics, visited, currentDepth + 1);
                }
            }
        }

        private string GetComplexityRating(double avgComplexity, int maxDepth)
        {
            double complexityScore = (avgComplexity * 10) + maxDepth;
            return complexityScore switch
            {
                >= 50 => "Extremely Complex - Intricate multi-level structure",
                >= 35 => "Highly Complex - Deep nested relationships",
                >= 20 => "Moderately Complex - Multiple interconnections",
                >= 10 => "Simple - Basic hierarchical structure",
                _ => "Very Simple - Minimal complexity"
            };
        }

        public void DisplayComponent(string componentId)
        {
            if (!components.ContainsKey(componentId)) return;

            var component = components[componentId];
            Console.WriteLine($"\n  Component: {component.ComponentName}");
            Console.WriteLine($"  ID: {component.ComponentId}");
            Console.WriteLine($"  Type: {component.ComponentType}");
            Console.WriteLine($"  Level: {component.HierarchicalLevel}");
            Console.WriteLine($"  Complexity: {component.Complexity:F2}");
            if (component.SubComponents.Count > 0)
            {
                Console.WriteLine($"  Sub-Components: {string.Join(", ", component.SubComponents)}");
            }
        }

        public void DisplayMetrics(string metricsId)
        {
            if (!metrics.ContainsKey(metricsId)) return;

            var m = metrics[metricsId];
            Console.WriteLine($"\n  Structural Metrics: {m.MetricsId}");
            Console.WriteLine($"  Total Components: {m.TotalComponents}");
            Console.WriteLine($"  Maximum Depth: {m.MaxDepth}");
            Console.WriteLine($"  Average Complexity: {m.AverageComplexity:F2}");
            Console.WriteLine($"  Total Relationships: {m.TotalRelationships}");
            Console.WriteLine($"  Structural Density: {m.StructuralDensity:F3}");
            Console.WriteLine($"  Rating: {m.ComplexityRating}");
        }

        public int GetTotalComponents()
        {
            return components.Count;
        }

        public int GetTotalRelationships()
        {
            return relationships.Count;
        }

        public int GetMaxHierarchicalDepth()
        {
            return hierarchyLevels.Count > 0 ? hierarchyLevels.Values.Max(h => h.LevelDepth) : 0;
        }

        public double GetAverageComponentComplexity()
        {
            return components.Count > 0 ? components.Values.Average(c => c.Complexity) : 0.0;
        }

        public List<string> GetComponentsByLevel(int level)
        {
            return components.Values
                .Where(c => c.HierarchicalLevel == level)
                .Select(c => c.ComponentId)
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Analyzing Complex and Detailed Structures               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var analyzer = new ComplexStructureAnalyzer();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Structural Components]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.RegisterComponent("ROOT", "System Root", "Foundation", 0, 0.3);
        analyzer.RegisterComponent("MODULE-A", "Core Module A", "Module", 1, 0.6);
        analyzer.RegisterComponent("MODULE-B", "Core Module B", "Module", 1, 0.7);
        analyzer.RegisterComponent("SUBMOD-A1", "Sub-Module A1", "SubModule", 2, 0.8);
        analyzer.RegisterComponent("SUBMOD-A2", "Sub-Module A2", "SubModule", 2, 0.75);
        analyzer.RegisterComponent("SUBMOD-B1", "Sub-Module B1", "SubModule", 2, 0.85);
        analyzer.RegisterComponent("COMP-A1-1", "Component A1.1", "Component", 3, 0.9);
        analyzer.RegisterComponent("COMP-A1-2", "Component A1.2", "Component", 3, 0.88);
        analyzer.RegisterComponent("COMP-A2-1", "Component A2.1", "Component", 3, 0.92);
        analyzer.RegisterComponent("COMP-B1-1", "Component B1.1", "Component", 3, 0.95);

        Console.WriteLine("  ✓ Registered 10 structural components across 4 hierarchical levels");
        analyzer.DisplayComponent("ROOT");
        analyzer.DisplayComponent("MODULE-A");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Building Hierarchical Structure]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.CreateComponentHierarchy("ROOT", "MODULE-A");
        analyzer.CreateComponentHierarchy("ROOT", "MODULE-B");
        analyzer.CreateComponentHierarchy("MODULE-A", "SUBMOD-A1");
        analyzer.CreateComponentHierarchy("MODULE-A", "SUBMOD-A2");
        analyzer.CreateComponentHierarchy("MODULE-B", "SUBMOD-B1");
        analyzer.CreateComponentHierarchy("SUBMOD-A1", "COMP-A1-1");
        analyzer.CreateComponentHierarchy("SUBMOD-A1", "COMP-A1-2");
        analyzer.CreateComponentHierarchy("SUBMOD-A2", "COMP-A2-1");
        analyzer.CreateComponentHierarchy("SUBMOD-B1", "COMP-B1-1");

        Console.WriteLine("  ✓ Built hierarchical tree structure");
        analyzer.DisplayComponent("MODULE-A");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Component Relationships]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.CreateRelationship("MODULE-A", "MODULE-B", "Collaboration", 0.8, true);
        analyzer.CreateRelationship("SUBMOD-A1", "SUBMOD-A2", "Sequential", 0.9, false);
        analyzer.CreateRelationship("COMP-A1-1", "COMP-A1-2", "Dependency", 0.85, true);
        analyzer.CreateRelationship("COMP-A1-2", "COMP-B1-1", "Integration", 0.75, true);
        analyzer.CreateRelationship("SUBMOD-A1", "SUBMOD-B1", "Communication", 0.7, true);

        Console.WriteLine("  ✓ Created 5 relationships between components");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Analyzing Structure Metrics]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.AnalyzeStructure("ROOT");

        Console.WriteLine("  ✓ Analyzed complete structure metrics");
        analyzer.DisplayMetrics("Metrics-ROOT");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Hierarchical Level Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Components by Hierarchical Level:");
        for (int level = 0; level <= analyzer.GetMaxHierarchicalDepth(); level++)
        {
            var componentsAtLevel = analyzer.GetComponentsByLevel(level);
            Console.WriteLine($"    Level {level}: {componentsAtLevel.Count} components");
        }

        Console.WriteLine($"\n  Maximum Hierarchical Depth: {analyzer.GetMaxHierarchicalDepth()}");
        Console.WriteLine($"  Average Component Complexity: {analyzer.GetAverageComponentComplexity():F2}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Structural Analysis Summary:");
        Console.WriteLine($"    Total Components: {analyzer.GetTotalComponents()}");
        Console.WriteLine($"    Total Relationships: {analyzer.GetTotalRelationships()}");
        Console.WriteLine($"    Hierarchical Levels: {analyzer.GetMaxHierarchicalDepth() + 1}");
        Console.WriteLine($"    Average Complexity: {analyzer.GetAverageComponentComplexity():F3}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Complex Structure Analysis Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Structural Analysis Architecture:");
        Console.WriteLine("    Layer 1: Component Registration (define structural units)");
        Console.WriteLine("    Layer 2: Hierarchical Organization (establish parent-child relationships)");
        Console.WriteLine("    Layer 3: Relationship Mapping (create cross-component connections)");
        Console.WriteLine("    Layer 4: Depth Analysis (measure hierarchical complexity)");
        Console.WriteLine("    Layer 5: Density Calculation (evaluate interconnection density)");
        Console.WriteLine("    Layer 6: Complexity Rating (assess overall structure intricacy)");
        Console.WriteLine("    Layer 7: Detailed Decomposition (analyze multi-level components)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register components at multiple hierarchical levels");
        Console.WriteLine("    ✓ Build complex nested structures");
        Console.WriteLine("    ✓ Create cross-component relationships and dependencies");
        Console.WriteLine("    ✓ Calculate structural metrics and complexity scores");
        Console.WriteLine("    ✓ Analyze hierarchical depth and breadth");
        Console.WriteLine("    ✓ Measure structural density and interconnectedness");
        Console.WriteLine("    ✓ Generate detailed component decomposition");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Complex structure analysis system complete");
        Console.ResetColor();
    }
}
