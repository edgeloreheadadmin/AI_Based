using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class GroupingMultipleGroupsIntoStructures
{
    public class Group
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string GroupType { get; set; }
        public List<string> Members { get; set; }
        public int MemberCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Structure
    {
        public string StructureId { get; set; }
        public string StructureName { get; set; }
        public string StructureType { get; set; }
        public List<string> GroupIds { get; set; }
        public Dictionary<string, List<string>> Interconnections { get; set; }
        public int TotalGroups { get; set; }
        public double ConnectivityDensity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InterconnectionMapping
    {
        public string MappingId { get; set; }
        public string SourceGroupId { get; set; }
        public string TargetGroupId { get; set; }
        public string RelationshipType { get; set; }
        public double RelationshipStrength { get; set; }
        public bool IsBidirectional { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StructuralAnalysis
    {
        public string AnalysisId { get; set; }
        public string StructureId { get; set; }
        public int TotalGroups { get; set; }
        public int TotalInterconnections { get; set; }
        public double NetworkDensity { get; set; }
        public List<string> IsolatedGroups { get; set; }
        public string StructuralComplexity { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class GroupStructureEngine
    {
        private Dictionary<string, Group> groups;
        private Dictionary<string, Structure> structures;
        private Dictionary<string, InterconnectionMapping> interconnections;
        private Dictionary<string, StructuralAnalysis> analyses;

        public GroupStructureEngine()
        {
            groups = new Dictionary<string, Group>();
            structures = new Dictionary<string, Structure>();
            interconnections = new Dictionary<string, InterconnectionMapping>();
            analyses = new Dictionary<string, StructuralAnalysis>();
        }

        public void RegisterGroup(string groupId, string groupName, string groupType, List<string> members)
        {
            var group = new Group
            {
                GroupId = groupId,
                GroupName = groupName,
                GroupType = groupType,
                Members = new List<string>(members),
                MemberCount = members.Count,
                CreatedDate = DateTime.Now
            };
            groups[groupId] = group;
        }

        public void CreateStructure(string structureId, string structureName, string structureType, List<string> groupIds)
        {
            var structure = new Structure
            {
                StructureId = structureId,
                StructureName = structureName,
                StructureType = structureType,
                GroupIds = new List<string>(groupIds),
                Interconnections = new Dictionary<string, List<string>>(),
                TotalGroups = groupIds.Count,
                ConnectivityDensity = 0.0,
                CreatedDate = DateTime.Now
            };

            foreach (var groupId in groupIds)
            {
                structure.Interconnections[groupId] = new List<string>();
            }

            structures[structureId] = structure;
        }

        public void CreateInterconnection(string sourceGroupId, string targetGroupId,
                                         string relationshipType, double strength, bool bidirectional)
        {
            if (!groups.ContainsKey(sourceGroupId) || !groups.ContainsKey(targetGroupId))
                return;

            string mappingId = $"Map-{sourceGroupId}-{targetGroupId}";
            var mapping = new InterconnectionMapping
            {
                MappingId = mappingId,
                SourceGroupId = sourceGroupId,
                TargetGroupId = targetGroupId,
                RelationshipType = relationshipType,
                RelationshipStrength = strength,
                IsBidirectional = bidirectional,
                CreatedDate = DateTime.Now
            };

            interconnections[mappingId] = mapping;

            foreach (var structure in structures.Values)
            {
                if (structure.GroupIds.Contains(sourceGroupId) && structure.GroupIds.Contains(targetGroupId))
                {
                    if (!structure.Interconnections[sourceGroupId].Contains(targetGroupId))
                    {
                        structure.Interconnections[sourceGroupId].Add(targetGroupId);
                    }
                    if (bidirectional && !structure.Interconnections[targetGroupId].Contains(sourceGroupId))
                    {
                        structure.Interconnections[targetGroupId].Add(sourceGroupId);
                    }
                }
            }
        }

        public void AnalyzeStructure(string structureId)
        {
            if (!structures.ContainsKey(structureId)) return;

            var structure = structures[structureId];
            var analysis = new StructuralAnalysis
            {
                AnalysisId = $"Analysis-{structureId}",
                StructureId = structureId,
                TotalGroups = structure.TotalGroups,
                TotalInterconnections = 0,
                NetworkDensity = 0.0,
                IsolatedGroups = new List<string>(),
                StructuralComplexity = "",
                AnalyzedDate = DateTime.Now
            };

            int connectionCount = 0;
            foreach (var connections in structure.Interconnections.Values)
            {
                connectionCount += connections.Count;
            }
            analysis.TotalInterconnections = connectionCount;

            int maxPossibleConnections = structure.TotalGroups * (structure.TotalGroups - 1);
            analysis.NetworkDensity = maxPossibleConnections > 0 ?
                (double)connectionCount / maxPossibleConnections : 0.0;

            foreach (var groupId in structure.GroupIds)
            {
                if (structure.Interconnections[groupId].Count == 0)
                {
                    analysis.IsolatedGroups.Add(groupId);
                }
            }

            analysis.StructuralComplexity = GetComplexityLevel(analysis.NetworkDensity, structure.TotalGroups);

            analyses[$"Analysis-{structureId}"] = analysis;
        }

        private string GetComplexityLevel(double density, int groupCount)
        {
            double complexityScore = density * groupCount;
            return complexityScore switch
            {
                >= 50 => "Highly Complex - Dense interconnected network",
                >= 20 => "Complex - Multiple interconnections",
                >= 10 => "Moderate - Some interconnections",
                >= 3 => "Simple - Sparse connections",
                _ => "Minimal - Few or no interconnections"
            };
        }

        public void DisplayGroup(string groupId)
        {
            if (!groups.ContainsKey(groupId)) return;

            var group = groups[groupId];
            Console.WriteLine($"\n  Group: {group.GroupName}");
            Console.WriteLine($"  Group ID: {group.GroupId}");
            Console.WriteLine($"  Type: {group.GroupType}");
            Console.WriteLine($"  Member Count: {group.MemberCount}");
            Console.WriteLine($"  Members: {string.Join(", ", group.Members)}");
        }

        public void DisplayStructure(string structureId)
        {
            if (!structures.ContainsKey(structureId)) return;

            var structure = structures[structureId];
            Console.WriteLine($"\n  Structure: {structure.StructureName}");
            Console.WriteLine($"  Structure ID: {structure.StructureId}");
            Console.WriteLine($"  Type: {structure.StructureType}");
            Console.WriteLine($"  Total Groups: {structure.TotalGroups}");
            Console.WriteLine($"  Groups: {string.Join(", ", structure.GroupIds)}");
            Console.WriteLine($"  Connectivity Density: {structure.ConnectivityDensity * 100:F1}%");
        }

        public void DisplayInterconnectionMap(string structureId)
        {
            if (!structures.ContainsKey(structureId)) return;

            var structure = structures[structureId];
            Console.WriteLine($"\n  Interconnection Map for {structure.StructureName}:");
            foreach (var kvp in structure.Interconnections)
            {
                if (kvp.Value.Count > 0)
                {
                    Console.WriteLine($"    {kvp.Key} → {string.Join(", ", kvp.Value)}");
                }
            }
        }

        public void DisplayAnalysis(string structureId)
        {
            string analysisKey = $"Analysis-{structureId}";
            if (!analyses.ContainsKey(analysisKey)) return;

            var analysis = analyses[analysisKey];
            Console.WriteLine($"\n  Structural Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Groups: {analysis.TotalGroups}");
            Console.WriteLine($"  Total Interconnections: {analysis.TotalInterconnections}");
            Console.WriteLine($"  Network Density: {analysis.NetworkDensity * 100:F1}%");
            Console.WriteLine($"  Structural Complexity: {analysis.StructuralComplexity}");
            if (analysis.IsolatedGroups.Count > 0)
            {
                Console.WriteLine($"  Isolated Groups: {string.Join(", ", analysis.IsolatedGroups)}");
            }
        }

        public int GetTotalGroups()
        {
            return groups.Count;
        }

        public int GetTotalStructures()
        {
            return structures.Count;
        }

        public int GetTotalInterconnections()
        {
            return interconnections.Count;
        }

        public List<string> GetConnectedGroups(string structureId, string groupId)
        {
            if (!structures.ContainsKey(structureId)) return new List<string>();
            var structure = structures[structureId];
            if (!structure.Interconnections.ContainsKey(groupId))
                return new List<string>();
            return structure.Interconnections[groupId];
        }

        public double CalculateStructureIntegration(string structureId)
        {
            if (!structures.ContainsKey(structureId)) return 0.0;
            var structure = structures[structureId];

            double totalConnections = 0;
            foreach (var connections in structure.Interconnections.Values)
            {
                totalConnections += connections.Count;
            }

            return structure.TotalGroups > 0 ? totalConnections / structure.TotalGroups : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Grouping Multiple Groups into Interconnected Structures        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new GroupStructureEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Groups]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterGroup("GRP-001", "Engineering Team", "Department", new List<string> { "Alice", "Bob", "Charlie" });
        engine.RegisterGroup("GRP-002", "Design Team", "Department", new List<string> { "Diana", "Eve", "Frank" });
        engine.RegisterGroup("GRP-003", "Research Team", "Department", new List<string> { "Grace", "Henry", "Iris" });
        engine.RegisterGroup("GRP-004", "Operations Team", "Department", new List<string> { "Jack", "Karen", "Leo" });
        engine.RegisterGroup("GRP-005", "Advisory Board", "Council", new List<string> { "Marcus", "Nancy", "Oliver" });

        Console.WriteLine("  ✓ Registered 5 groups across different types");
        engine.DisplayGroup("GRP-001");
        engine.DisplayGroup("GRP-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Organizational Structures]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateStructure("STRUCT-001", "Integrated Operations", "Interconnected",
            new List<string> { "GRP-001", "GRP-002", "GRP-003", "GRP-004" });

        engine.CreateStructure("STRUCT-002", "Independent Divisions", "Independent",
            new List<string> { "GRP-001", "GRP-002" });

        engine.CreateStructure("STRUCT-003", "Governance Framework", "Hierarchical",
            new List<string> { "GRP-005", "GRP-001", "GRP-002", "GRP-003", "GRP-004" });

        Console.WriteLine("  ✓ Created 3 structures with different organizational models");
        engine.DisplayStructure("STRUCT-001");
        engine.DisplayStructure("STRUCT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Interconnections]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateInterconnection("GRP-001", "GRP-002", "Collaboration", 0.85, true);
        engine.CreateInterconnection("GRP-002", "GRP-003", "Knowledge Transfer", 0.75, true);
        engine.CreateInterconnection("GRP-001", "GRP-003", "Project Sharing", 0.80, true);
        engine.CreateInterconnection("GRP-004", "GRP-001", "Resource Support", 0.90, true);
        engine.CreateInterconnection("GRP-004", "GRP-002", "Infrastructure", 0.70, true);
        engine.CreateInterconnection("GRP-005", "GRP-001", "Oversight", 0.65, false);
        engine.CreateInterconnection("GRP-005", "GRP-004", "Policy Direction", 0.75, false);

        Console.WriteLine("  ✓ Created 7 interconnections between groups");
        engine.DisplayInterconnectionMap("STRUCT-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Analyzing Structural Properties]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeStructure("STRUCT-001");
        engine.AnalyzeStructure("STRUCT-002");
        engine.AnalyzeStructure("STRUCT-003");

        Console.WriteLine("  ✓ Analyzed structural characteristics");
        engine.DisplayAnalysis("STRUCT-001");
        engine.DisplayAnalysis("STRUCT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Connectivity Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Connected groups:");
        var connectedToGRP001 = engine.GetConnectedGroups("STRUCT-001", "GRP-001");
        var connectedToGRP004 = engine.GetConnectedGroups("STRUCT-001", "GRP-004");

        Console.WriteLine($"    GRP-001 connects to: {string.Join(", ", connectedToGRP001)}");
        Console.WriteLine($"    GRP-004 connects to: {string.Join(", ", connectedToGRP004)}");

        double integrationScore = engine.CalculateStructureIntegration("STRUCT-001");
        Console.WriteLine($"    Structure Integration Score: {integrationScore:F2}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  System Overview:");
        Console.WriteLine($"    Total Groups: {engine.GetTotalGroups()}");
        Console.WriteLine($"    Total Structures: {engine.GetTotalStructures()}");
        Console.WriteLine($"    Total Interconnections: {engine.GetTotalInterconnections()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Group Structure Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Multi-Group Organizational Architecture:");
        Console.WriteLine("    Layer 1: Group Definition (establish group identity and membership)");
        Console.WriteLine("    Layer 2: Structure Creation (organize groups into configurations)");
        Console.WriteLine("    Layer 3: Interconnection Mapping (define relationships between groups)");
        Console.WriteLine("    Layer 4: Bidirectional Linking (support single and mutual connections)");
        Console.WriteLine("    Layer 5: Density Analysis (measure connectivity and integration)");
        Console.WriteLine("    Layer 6: Isolation Detection (identify independent components)");
        Console.WriteLine("    Layer 7: Complexity Assessment (evaluate structural intricacy)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Create multiple independent and interconnected groups");
        Console.WriteLine("    ✓ Build hierarchical and flat organizational structures");
        Console.WriteLine("    ✓ Support bidirectional and unidirectional relationships");
        Console.WriteLine("    ✓ Measure network density and connectivity");
        Console.WriteLine("    ✓ Identify structural complexity levels");
        Console.WriteLine("    ✓ Track integration and collaboration metrics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Group structure organization system complete");
        Console.ResetColor();
    }
}
