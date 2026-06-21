using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingMultipleClassesInterconnectedOrNested
{
    public class ClassDefinition
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassDescription { get; set; }
        public string ParentClassId { get; set; }
        public List<string> ChildClassIds { get; set; }
        public int HierarchyLevel { get; set; }
        public List<string> Properties { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassRelationship
    {
        public string RelationshipId { get; set; }
        public string SourceClassId { get; set; }
        public string TargetClassId { get; set; }
        public string RelationType { get; set; }
        public double RelationshipStrength { get; set; }
        public bool IsBidirectional { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class NestingHierarchy
    {
        public string HierarchyId { get; set; }
        public string RootClassId { get; set; }
        public int MaxDepth { get; set; }
        public int TotalClassesInHierarchy { get; set; }
        public List<List<string>> LevelStructure { get; set; }
        public double HierarchyComplexity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InterconnectionMap
    {
        public string MapId { get; set; }
        public List<string> ClassesInMap { get; set; }
        public int TotalInterconnections { get; set; }
        public double InterconnectionDensity { get; set; }
        public List<(string, string)> ConnectedPairs { get; set; }
        public string MapPattern { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassNestingAnalyzer
    {
        private Dictionary<string, ClassDefinition> classes;
        private Dictionary<string, ClassRelationship> relationships;
        private Dictionary<string, NestingHierarchy> hierarchies;
        private Dictionary<string, InterconnectionMap> maps;

        public ClassNestingAnalyzer()
        {
            classes = new Dictionary<string, ClassDefinition>();
            relationships = new Dictionary<string, ClassRelationship>();
            hierarchies = new Dictionary<string, NestingHierarchy>();
            maps = new Dictionary<string, InterconnectionMap>();
        }

        public void RegisterClass(string classId, string className, string description, string parentId = null)
        {
            var classDefinition = new ClassDefinition
            {
                ClassId = classId,
                ClassName = className,
                ClassDescription = description,
                ParentClassId = parentId,
                ChildClassIds = new List<string>(),
                HierarchyLevel = 0,
                Properties = new List<string>(),
                CreatedDate = DateTime.Now
            };

            if (parentId != null && classes.ContainsKey(parentId))
            {
                classDefinition.HierarchyLevel = classes[parentId].HierarchyLevel + 1;
                classes[parentId].ChildClassIds.Add(classId);
            }

            classes[classId] = classDefinition;
        }

        public void AddPropertyToClass(string classId, string property)
        {
            if (classes.ContainsKey(classId))
            {
                classes[classId].Properties.Add(property);
            }
        }

        public void CreateRelationship(string sourceId, string targetId, string relationType, double strength, bool bidirectional)
        {
            if (!classes.ContainsKey(sourceId) || !classes.ContainsKey(targetId))
                return;

            string relationshipId = $"Rel-{sourceId}-{targetId}";
            var relationship = new ClassRelationship
            {
                RelationshipId = relationshipId,
                SourceClassId = sourceId,
                TargetClassId = targetId,
                RelationType = relationType,
                RelationshipStrength = strength,
                IsBidirectional = bidirectional,
                CreatedDate = DateTime.Now
            };
            relationships[relationshipId] = relationship;
        }

        public void BuildNestingHierarchy(string hierarchyId, string rootClassId)
        {
            if (!classes.ContainsKey(rootClassId)) return;

            var hierarchy = new NestingHierarchy
            {
                HierarchyId = hierarchyId,
                RootClassId = rootClassId,
                MaxDepth = 0,
                TotalClassesInHierarchy = 0,
                LevelStructure = new List<List<string>>(),
                HierarchyComplexity = 0.0,
                CreatedDate = DateTime.Now
            };

            var visited = new HashSet<string>();
            BuildHierarchyRecursive(rootClassId, hierarchy, visited, 0);

            hierarchy.HierarchyComplexity = hierarchy.MaxDepth * hierarchy.TotalClassesInHierarchy / 10.0;

            hierarchies[hierarchyId] = hierarchy;
        }

        private void BuildHierarchyRecursive(string classId, NestingHierarchy hierarchy, HashSet<string> visited, int level)
        {
            if (visited.Contains(classId)) return;
            visited.Add(classId);

            if (level >= hierarchy.LevelStructure.Count)
            {
                hierarchy.LevelStructure.Add(new List<string>());
            }
            hierarchy.LevelStructure[level].Add(classId);
            hierarchy.TotalClassesInHierarchy++;

            if (level > hierarchy.MaxDepth)
            {
                hierarchy.MaxDepth = level;
            }

            if (classes.ContainsKey(classId))
            {
                foreach (var childId in classes[classId].ChildClassIds)
                {
                    BuildHierarchyRecursive(childId, hierarchy, visited, level + 1);
                }
            }
        }

        public void MapInterconnections(string mapId, List<string> classIds)
        {
            var map = new InterconnectionMap
            {
                MapId = mapId,
                ClassesInMap = new List<string>(classIds),
                TotalInterconnections = 0,
                InterconnectionDensity = 0.0,
                ConnectedPairs = new List<(string, string)>(),
                MapPattern = "",
                CreatedDate = DateTime.Now
            };

            int connectionCount = 0;
            foreach (var rel in relationships.Values)
            {
                if (classIds.Contains(rel.SourceClassId) && classIds.Contains(rel.TargetClassId))
                {
                    map.ConnectedPairs.Add((rel.SourceClassId, rel.TargetClassId));
                    connectionCount++;
                }
            }

            map.TotalInterconnections = connectionCount;
            int maxPossibleConnections = classIds.Count * (classIds.Count - 1);
            map.InterconnectionDensity = maxPossibleConnections > 0 ? (double)connectionCount / maxPossibleConnections : 0.0;

            map.MapPattern = GetMapPattern(map.InterconnectionDensity, classIds.Count);

            maps[mapId] = map;
        }

        private string GetMapPattern(double density, int classCount)
        {
            if (density > 0.8)
                return "Highly Interconnected - Dense network of classes";
            if (density > 0.5)
                return "Well Connected - Multiple cross-class relationships";
            if (density > 0.2)
                return "Moderately Connected - Some interconnections";
            return "Sparse - Minimal interconnections";
        }

        public void DisplayClass(string classId)
        {
            if (!classes.ContainsKey(classId)) return;

            var classDefinition = classes[classId];
            Console.WriteLine($"\n  Class: {classDefinition.ClassName}");
            Console.WriteLine($"  ID: {classDefinition.ClassId}");
            Console.WriteLine($"  Description: {classDefinition.ClassDescription}");
            Console.WriteLine($"  Level: {classDefinition.HierarchyLevel}");
            if (!string.IsNullOrEmpty(classDefinition.ParentClassId))
                Console.WriteLine($"  Parent: {classDefinition.ParentClassId}");
            if (classDefinition.ChildClassIds.Count > 0)
                Console.WriteLine($"  Children: {string.Join(", ", classDefinition.ChildClassIds)}");
            if (classDefinition.Properties.Count > 0)
                Console.WriteLine($"  Properties: {string.Join(", ", classDefinition.Properties)}");
        }

        public void DisplayHierarchy(string hierarchyId)
        {
            if (!hierarchies.ContainsKey(hierarchyId)) return;

            var hierarchy = hierarchies[hierarchyId];
            Console.WriteLine($"\n  Nesting Hierarchy: {hierarchy.HierarchyId}");
            Console.WriteLine($"  Root: {hierarchy.RootClassId}");
            Console.WriteLine($"  Max Depth: {hierarchy.MaxDepth}");
            Console.WriteLine($"  Total Classes: {hierarchy.TotalClassesInHierarchy}");
            Console.WriteLine($"  Complexity: {hierarchy.HierarchyComplexity:F2}");
            Console.WriteLine($"  Structure by Level:");
            for (int level = 0; level < hierarchy.LevelStructure.Count; level++)
            {
                Console.WriteLine($"    Level {level}: {hierarchy.LevelStructure[level].Count} classes");
            }
        }

        public void DisplayInterconnectionMap(string mapId)
        {
            if (!maps.ContainsKey(mapId)) return;

            var map = maps[mapId];
            Console.WriteLine($"\n  Interconnection Map: {map.MapId}");
            Console.WriteLine($"  Classes in Map: {map.ClassesInMap.Count}");
            Console.WriteLine($"  Total Interconnections: {map.TotalInterconnections}");
            Console.WriteLine($"  Interconnection Density: {map.InterconnectionDensity * 100:F1}%");
            Console.WriteLine($"  Pattern: {map.MapPattern}");
        }

        public int GetTotalClasses()
        {
            return classes.Count;
        }

        public int GetTotalRelationships()
        {
            return relationships.Count;
        }

        public int GetTotalHierarchies()
        {
            return hierarchies.Count;
        }

        public List<(string, int)> GetClassesByLevel()
        {
            var levels = new Dictionary<int, int>();
            foreach (var classDefinition in classes.Values)
            {
                if (!levels.ContainsKey(classDefinition.HierarchyLevel))
                {
                    levels[classDefinition.HierarchyLevel] = 0;
                }
                levels[classDefinition.HierarchyLevel]++;
            }
            return levels.OrderBy(x => x.Key).Select(x => ($"Level {x.Key}", x.Value)).ToList();
        }

        public List<string> GetChildClasses(string classId)
        {
            if (classes.ContainsKey(classId))
            {
                return classes[classId].ChildClassIds;
            }
            return new List<string>();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Recognizing Multiple Classes Interconnected or Nested        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var analyzer = new ClassNestingAnalyzer();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Classes with Hierarchy]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.RegisterClass("CLASS-ROOT", "Entity", "Base class for all entities");
        analyzer.RegisterClass("CLASS-001", "Vehicle", "Any mode of transportation", "CLASS-ROOT");
        analyzer.RegisterClass("CLASS-002", "Building", "Structural housing", "CLASS-ROOT");
        analyzer.RegisterClass("CLASS-003", "Organism", "Living entity", "CLASS-ROOT");

        analyzer.RegisterClass("CLASS-001-A", "Car", "Four-wheeled vehicle", "CLASS-001");
        analyzer.RegisterClass("CLASS-001-B", "Truck", "Heavy duty vehicle", "CLASS-001");
        analyzer.RegisterClass("CLASS-001-A-1", "Sedan", "Car with 4 doors", "CLASS-001-A");
        analyzer.RegisterClass("CLASS-001-A-2", "SUV", "Sport utility car", "CLASS-001-A");

        analyzer.RegisterClass("CLASS-002-A", "House", "Residential building", "CLASS-002");
        analyzer.RegisterClass("CLASS-002-B", "Office", "Commercial building", "CLASS-002");

        analyzer.RegisterClass("CLASS-003-A", "Animal", "Non-plant organism", "CLASS-003");
        analyzer.RegisterClass("CLASS-003-B", "Plant", "Botanical organism", "CLASS-003");

        Console.WriteLine("  ✓ Registered 12 classes in nested hierarchy");
        analyzer.DisplayClass("CLASS-ROOT");
        analyzer.DisplayClass("CLASS-001");
        analyzer.DisplayClass("CLASS-001-A");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Adding Properties to Classes]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.AddPropertyToClass("CLASS-001", "Wheels");
        analyzer.AddPropertyToClass("CLASS-001", "Engine");
        analyzer.AddPropertyToClass("CLASS-001-A", "Door_count");
        analyzer.AddPropertyToClass("CLASS-002", "Walls");
        analyzer.AddPropertyToClass("CLASS-002", "Roof");

        Console.WriteLine("  ✓ Added properties to classes");
        analyzer.DisplayClass("CLASS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Cross-Class Relationships]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.CreateRelationship("CLASS-001", "CLASS-002", "Occupies", 0.85, false);
        analyzer.CreateRelationship("CLASS-001-A", "CLASS-001-B", "SameCategory", 0.90, true);
        analyzer.CreateRelationship("CLASS-003-A", "CLASS-003-B", "CoexistNaturally", 0.80, true);
        analyzer.CreateRelationship("CLASS-002-A", "CLASS-003-A", "Provides_Shelter", 0.75, false);

        Console.WriteLine("  ✓ Created 4 cross-class relationships");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Building Nesting Hierarchies]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.BuildNestingHierarchy("HIER-001", "CLASS-ROOT");
        analyzer.BuildNestingHierarchy("HIER-002", "CLASS-001");
        analyzer.BuildNestingHierarchy("HIER-003", "CLASS-002");

        Console.WriteLine("  ✓ Built 3 nesting hierarchies");
        analyzer.DisplayHierarchy("HIER-001");
        analyzer.DisplayHierarchy("HIER-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Mapping Interconnections]");
        Console.ResetColor();
        Thread.Sleep(500);

        analyzer.MapInterconnections("MAP-001",
            new List<string> { "CLASS-001", "CLASS-001-A", "CLASS-001-B", "CLASS-001-A-1", "CLASS-001-A-2" });

        analyzer.MapInterconnections("MAP-002",
            new List<string> { "CLASS-002", "CLASS-002-A", "CLASS-002-B" });

        analyzer.MapInterconnections("MAP-003",
            new List<string> { "CLASS-ROOT", "CLASS-001", "CLASS-002", "CLASS-003" });

        Console.WriteLine("  ✓ Created 3 interconnection maps");
        analyzer.DisplayInterconnectionMap("MAP-001");
        analyzer.DisplayInterconnectionMap("MAP-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Hierarchical Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var classesByLevel = analyzer.GetClassesByLevel();
        Console.WriteLine("  Classes by Hierarchy Level:");
        foreach (var (levelName, count) in classesByLevel)
        {
            Console.WriteLine($"    {levelName}: {count} classes");
        }

        Console.WriteLine($"\n  System Statistics:");
        Console.WriteLine($"    Total Classes: {analyzer.GetTotalClasses()}");
        Console.WriteLine($"    Total Relationships: {analyzer.GetTotalRelationships()}");
        Console.WriteLine($"    Total Hierarchies: {analyzer.GetTotalHierarchies()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Class Nesting and Interconnection Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Class Organization Architecture:");
        Console.WriteLine("    Layer 1: Class Definition (establish class identities)");
        Console.WriteLine("    Layer 2: Hierarchical Nesting (create parent-child relationships)");
        Console.WriteLine("    Layer 3: Property Assignment (define class characteristics)");
        Console.WriteLine("    Layer 4: Cross-Class Relationships (create interconnections)");
        Console.WriteLine("    Layer 5: Hierarchy Building (establish full nesting structures)");
        Console.WriteLine("    Layer 6: Interconnection Mapping (analyze cross-class density)");
        Console.WriteLine("    Layer 7: Structural Analysis (measure complexity and patterns)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Create hierarchically nested class structures");
        Console.WriteLine("    ✓ Support multiple levels of class inheritance");
        Console.WriteLine("    ✓ Track parent-child class relationships");
        Console.WriteLine("    ✓ Map cross-class interconnections and dependencies");
        Console.WriteLine("    ✓ Analyze hierarchy complexity and depth");
        Console.WriteLine("    ✓ Measure interconnection density across classes");
        Console.WriteLine("    ✓ Generate hierarchical structural reports");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Class nesting and interconnection system complete");
        Console.ResetColor();
    }
}
