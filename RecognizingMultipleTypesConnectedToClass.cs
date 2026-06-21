using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingMultipleTypesConnectedToClass
{
    public class ClassSubject
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassDescription { get; set; }
        public List<string> RelatedTypeIds { get; set; }
        public int TypeCount { get; set; }
        public double ClassRelevance { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TypeRelation
    {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string TypeCategory { get; set; }
        public List<string> RelatedClassIds { get; set; }
        public double Specificity { get; set; }
        public int ClassConnections { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TypeHierarchy
    {
        public string HierarchyId { get; set; }
        public string RootTypeId { get; set; }
        public List<string> SubtypeIds { get; set; }
        public int HierarchyDepth { get; set; }
        public double HierarchyComplexity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SubtypeMapping
    {
        public string MappingId { get; set; }
        public string ClassId { get; set; }
        public string ParentTypeId { get; set; }
        public string SubtypeId { get; set; }
        public double InheritanceStrength { get; set; }
        public List<string> DistinguishingFeatures { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TypeAnalysis
    {
        public string AnalysisId { get; set; }
        public string ClassId { get; set; }
        public int TotalTypes { get; set; }
        public int TotalSubtypes { get; set; }
        public int MaxHierarchyDepth { get; set; }
        public double AverageSpecificity { get; set; }
        public List<string> TypeCategories { get; set; }
        public string ClassificationPattern { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class TypeClassificationEngine
    {
        private Dictionary<string, ClassSubject> classes;
        private Dictionary<string, TypeRelation> types;
        private Dictionary<string, TypeHierarchy> hierarchies;
        private Dictionary<string, SubtypeMapping> mappings;
        private Dictionary<string, TypeAnalysis> analyses;

        public TypeClassificationEngine()
        {
            classes = new Dictionary<string, ClassSubject>();
            types = new Dictionary<string, TypeRelation>();
            hierarchies = new Dictionary<string, TypeHierarchy>();
            mappings = new Dictionary<string, SubtypeMapping>();
            analyses = new Dictionary<string, TypeAnalysis>();
        }

        public void RegisterClass(string classId, string className, string description)
        {
            var classSubject = new ClassSubject
            {
                ClassId = classId,
                ClassName = className,
                ClassDescription = description,
                RelatedTypeIds = new List<string>(),
                TypeCount = 0,
                ClassRelevance = 0.0,
                CreatedDate = DateTime.Now
            };
            classes[classId] = classSubject;
        }

        public void RegisterType(string typeId, string typeName, string typeCategory)
        {
            var type = new TypeRelation
            {
                TypeId = typeId,
                TypeName = typeName,
                TypeCategory = typeCategory,
                RelatedClassIds = new List<string>(),
                Specificity = 0.5,
                ClassConnections = 0,
                CreatedDate = DateTime.Now
            };
            types[typeId] = type;
        }

        public void LinkTypeToClass(string classId, string typeId, double relevance)
        {
            if (!classes.ContainsKey(classId) || !types.ContainsKey(typeId))
                return;

            var classSubject = classes[classId];
            var type = types[typeId];

            if (!classSubject.RelatedTypeIds.Contains(typeId))
            {
                classSubject.RelatedTypeIds.Add(typeId);
                classSubject.TypeCount++;
            }

            if (!type.RelatedClassIds.Contains(classId))
            {
                type.RelatedClassIds.Add(classId);
                type.ClassConnections++;
            }

            classSubject.ClassRelevance = Math.Max(classSubject.ClassRelevance, relevance);
        }

        public void CreateTypeHierarchy(string hierarchyId, string rootTypeId, List<string> subtypeIds)
        {
            if (!types.ContainsKey(rootTypeId)) return;

            var hierarchy = new TypeHierarchy
            {
                HierarchyId = hierarchyId,
                RootTypeId = rootTypeId,
                SubtypeIds = new List<string>(subtypeIds),
                HierarchyDepth = 1,
                HierarchyComplexity = subtypeIds.Count * 0.5,
                CreatedDate = DateTime.Now
            };

            foreach (var subtypeId in subtypeIds)
            {
                if (types.ContainsKey(subtypeId))
                {
                    types[subtypeId].Specificity = 0.8;
                }
            }

            hierarchies[hierarchyId] = hierarchy;
        }

        public void CreateSubtypeMapping(string classId, string parentTypeId, string subtypeId,
                                         double inheritanceStrength, List<string> features)
        {
            if (!classes.ContainsKey(classId) || !types.ContainsKey(parentTypeId) || !types.ContainsKey(subtypeId))
                return;

            string mappingId = $"Map-{classId}-{subtypeId}";
            var mapping = new SubtypeMapping
            {
                MappingId = mappingId,
                ClassId = classId,
                ParentTypeId = parentTypeId,
                SubtypeId = subtypeId,
                InheritanceStrength = inheritanceStrength,
                DistinguishingFeatures = new List<string>(features),
                CreatedDate = DateTime.Now
            };
            mappings[mappingId] = mapping;

            LinkTypeToClass(classId, subtypeId, inheritanceStrength);
        }

        public void AnalyzeClassTypes(string classId)
        {
            if (!classes.ContainsKey(classId)) return;

            var classSubject = classes[classId];
            var analysis = new TypeAnalysis
            {
                AnalysisId = $"Analysis-{classId}",
                ClassId = classId,
                TotalTypes = classSubject.TypeCount,
                TotalSubtypes = 0,
                MaxHierarchyDepth = 0,
                AverageSpecificity = 0.0,
                TypeCategories = new List<string>(),
                ClassificationPattern = "",
                AnalyzedDate = DateTime.Now
            };

            double totalSpecificity = 0.0;
            var categorySet = new HashSet<string>();

            foreach (var typeId in classSubject.RelatedTypeIds)
            {
                if (types.ContainsKey(typeId))
                {
                    var type = types[typeId];
                    totalSpecificity += type.Specificity;
                    categorySet.Add(type.TypeCategory);

                    var hierarchiesForType = hierarchies.Values
                        .Where(h => h.SubtypeIds.Contains(typeId))
                        .ToList();

                    foreach (var hierarchy in hierarchiesForType)
                    {
                        analysis.TotalSubtypes += hierarchy.SubtypeIds.Count;
                        analysis.MaxHierarchyDepth = Math.Max(analysis.MaxHierarchyDepth, hierarchy.HierarchyDepth);
                    }
                }
            }

            analysis.AverageSpecificity = classSubject.TypeCount > 0 ? totalSpecificity / classSubject.TypeCount : 0.0;
            analysis.TypeCategories = categorySet.ToList();
            analysis.ClassificationPattern = GetClassificationPattern(analysis.TotalTypes, analysis.TotalSubtypes);

            analyses[analysis.AnalysisId] = analysis;
        }

        private string GetClassificationPattern(int types, int subtypes)
        {
            double complexity = types + (subtypes * 0.5);
            return complexity switch
            {
                >= 15 => "Highly Complex - Multiple type categories with deep subtypes",
                >= 10 => "Complex - Multiple types with several subtypes",
                >= 5 => "Moderate - Several types with some subtypes",
                >= 2 => "Simple - Few types with minimal hierarchy",
                _ => "Very Simple - Single or dual type structure"
            };
        }

        public void DisplayClass(string classId)
        {
            if (!classes.ContainsKey(classId)) return;

            var classSubject = classes[classId];
            Console.WriteLine($"\n  Class: {classSubject.ClassName}");
            Console.WriteLine($"  ID: {classSubject.ClassId}");
            Console.WriteLine($"  Description: {classSubject.ClassDescription}");
            Console.WriteLine($"  Related Types: {classSubject.TypeCount}");
            Console.WriteLine($"  Relevance Score: {classSubject.ClassRelevance:F2}");
        }

        public void DisplayType(string typeId)
        {
            if (!types.ContainsKey(typeId)) return;

            var type = types[typeId];
            Console.WriteLine($"\n  Type: {type.TypeName}");
            Console.WriteLine($"  ID: {type.TypeId}");
            Console.WriteLine($"  Category: {type.TypeCategory}");
            Console.WriteLine($"  Specificity: {type.Specificity:F2}");
            Console.WriteLine($"  Connected Classes: {type.ClassConnections}");
        }

        public void DisplayTypeHierarchy(string hierarchyId)
        {
            if (!hierarchies.ContainsKey(hierarchyId)) return;

            var hierarchy = hierarchies[hierarchyId];
            Console.WriteLine($"\n  Type Hierarchy: {hierarchy.HierarchyId}");
            Console.WriteLine($"  Root Type: {hierarchy.RootTypeId}");
            Console.WriteLine($"  Subtypes: {hierarchy.SubtypeIds.Count}");
            Console.WriteLine($"  Depth: {hierarchy.HierarchyDepth}");
            Console.WriteLine($"  Complexity: {hierarchy.HierarchyComplexity:F2}");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Type Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Class: {analysis.ClassId}");
            Console.WriteLine($"  Total Types: {analysis.TotalTypes}");
            Console.WriteLine($"  Total Subtypes: {analysis.TotalSubtypes}");
            Console.WriteLine($"  Max Hierarchy Depth: {analysis.MaxHierarchyDepth}");
            Console.WriteLine($"  Average Specificity: {analysis.AverageSpecificity:F2}");
            Console.WriteLine($"  Type Categories: {string.Join(", ", analysis.TypeCategories)}");
            Console.WriteLine($"  Pattern: {analysis.ClassificationPattern}");
        }

        public int GetTotalClasses()
        {
            return classes.Count;
        }

        public int GetTotalTypes()
        {
            return types.Count;
        }

        public int GetTotalHierarchies()
        {
            return hierarchies.Count;
        }

        public List<(string, int)> GetTypeCountByClass()
        {
            return classes.Values
                .Select(c => (c.ClassName, c.TypeCount))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public List<string> GetCrossConnectedTypes()
        {
            return types.Values
                .Where(t => t.ClassConnections > 1)
                .Select(t => t.TypeId)
                .ToList();
        }

        public double GetAverageTypeSpecificity()
        {
            return types.Count > 0 ? types.Values.Average(t => t.Specificity) : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Recognizing Multiple Types Connected to a Class or Subject     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new TypeClassificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Classes and Subjects]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterClass("CLASS-001", "Vehicle", "A mode of transportation with wheels");
        engine.RegisterClass("CLASS-002", "Animal", "A living organism with specific characteristics");
        engine.RegisterClass("CLASS-003", "Architecture", "Structural design and buildings");
        engine.RegisterClass("CLASS-004", "Literature", "Written work and written expression");

        Console.WriteLine("  ✓ Registered 4 class subjects");
        engine.DisplayClass("CLASS-001");
        engine.DisplayClass("CLASS-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Types]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterType("TYPE-001", "Car", "Land Vehicle");
        engine.RegisterType("TYPE-002", "Truck", "Land Vehicle");
        engine.RegisterType("TYPE-003", "Bicycle", "Human-Powered");
        engine.RegisterType("TYPE-004", "Sedan", "Car Subtype");
        engine.RegisterType("TYPE-005", "SUV", "Car Subtype");

        engine.RegisterType("TYPE-006", "Dog", "Mammal");
        engine.RegisterType("TYPE-007", "Cat", "Mammal");
        engine.RegisterType("TYPE-008", "Bird", "Avian");
        engine.RegisterType("TYPE-009", "Terrier", "Dog Subtype");

        engine.RegisterType("TYPE-010", "Castle", "Large Structure");
        engine.RegisterType("TYPE-011", "House", "Residential");
        engine.RegisterType("TYPE-012", "Bridge", "Infrastructure");
        engine.RegisterType("TYPE-013", "Mansion", "House Subtype");

        engine.RegisterType("TYPE-014", "Novel", "Long Fiction");
        engine.RegisterType("TYPE-015", "Poetry", "Verse Form");
        engine.RegisterType("TYPE-016", "Drama", "Theatrical");
        engine.RegisterType("TYPE-017", "Fantasy Novel", "Novel Subtype");

        Console.WriteLine("  ✓ Registered 17 types");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Linking Types to Classes]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkTypeToClass("CLASS-001", "TYPE-001", 0.95);
        engine.LinkTypeToClass("CLASS-001", "TYPE-002", 0.92);
        engine.LinkTypeToClass("CLASS-001", "TYPE-003", 0.88);

        engine.LinkTypeToClass("CLASS-002", "TYPE-006", 0.98);
        engine.LinkTypeToClass("CLASS-002", "TYPE-007", 0.96);
        engine.LinkTypeToClass("CLASS-002", "TYPE-008", 0.94);

        engine.LinkTypeToClass("CLASS-003", "TYPE-010", 0.90);
        engine.LinkTypeToClass("CLASS-003", "TYPE-011", 0.95);
        engine.LinkTypeToClass("CLASS-003", "TYPE-012", 0.85);

        engine.LinkTypeToClass("CLASS-004", "TYPE-014", 0.95);
        engine.LinkTypeToClass("CLASS-004", "TYPE-015", 0.90);
        engine.LinkTypeToClass("CLASS-004", "TYPE-016", 0.92);

        Console.WriteLine("  ✓ Created 12 type-to-class links");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Creating Type Hierarchies]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateTypeHierarchy("HIER-001", "TYPE-001", new List<string> { "TYPE-004", "TYPE-005" });
        engine.CreateTypeHierarchy("HIER-002", "TYPE-006", new List<string> { "TYPE-009" });
        engine.CreateTypeHierarchy("HIER-003", "TYPE-011", new List<string> { "TYPE-013" });
        engine.CreateTypeHierarchy("HIER-004", "TYPE-014", new List<string> { "TYPE-017" });

        Console.WriteLine("  ✓ Created 4 type hierarchies");
        engine.DisplayTypeHierarchy("HIER-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Creating Subtype Mappings]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateSubtypeMapping("CLASS-001", "TYPE-001", "TYPE-004", 0.90, new List<string> { "4-doors", "comfort-focused" });
        engine.CreateSubtypeMapping("CLASS-001", "TYPE-001", "TYPE-005", 0.92, new List<string> { "high-clearance", "spacious" });

        engine.CreateSubtypeMapping("CLASS-002", "TYPE-006", "TYPE-009", 0.95, new List<string> { "small", "energetic" });

        engine.CreateSubtypeMapping("CLASS-003", "TYPE-011", "TYPE-013", 0.88, new List<string> { "luxury", "ornate" });

        engine.CreateSubtypeMapping("CLASS-004", "TYPE-014", "TYPE-017", 0.85, new List<string> { "magical-elements", "adventure" });

        Console.WriteLine("  ✓ Created 5 subtype mappings");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Class Type Relationships]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeClassTypes("CLASS-001");
        engine.AnalyzeClassTypes("CLASS-002");
        engine.AnalyzeClassTypes("CLASS-003");
        engine.AnalyzeClassTypes("CLASS-004");

        Console.WriteLine("  ✓ Analyzed type relationships for all classes");
        engine.DisplayAnalysis("Analysis-CLASS-001");
        engine.DisplayAnalysis("Analysis-CLASS-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: System Statistics and Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var typeCountByClass = engine.GetTypeCountByClass();
        Console.WriteLine("  Type Distribution by Class:");
        foreach (var (className, count) in typeCountByClass)
        {
            Console.WriteLine($"    {className}: {count} types");
        }

        Console.WriteLine($"\n  System Overview:");
        Console.WriteLine($"    Total Classes: {engine.GetTotalClasses()}");
        Console.WriteLine($"    Total Types: {engine.GetTotalTypes()}");
        Console.WriteLine($"    Type Hierarchies: {engine.GetTotalHierarchies()}");
        Console.WriteLine($"    Cross-Connected Types: {engine.GetCrossConnectedTypes().Count}");
        Console.WriteLine($"    Average Type Specificity: {engine.GetAverageTypeSpecificity():F2}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 8: Multiple Type Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Multi-Type Recognition Architecture:");
        Console.WriteLine("    Layer 1: Class Registration (define subject classes)");
        Console.WriteLine("    Layer 2: Type Registration (establish type definitions)");
        Console.WriteLine("    Layer 3: Type-Class Linking (connect types to classes)");
        Console.WriteLine("    Layer 4: Hierarchy Creation (organize types hierarchically)");
        Console.WriteLine("    Layer 5: Subtype Mapping (define inheritance relationships)");
        Console.WriteLine("    Layer 6: Feature Identification (extract distinguishing features)");
        Console.WriteLine("    Layer 7: Pattern Analysis (recognize multi-type relationships)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple types connected to single class");
        Console.WriteLine("    ✓ Create hierarchical relationships between types");
        Console.WriteLine("    ✓ Map subtypes with distinguishing features");
        Console.WriteLine("    ✓ Calculate inheritance strength scores");
        Console.WriteLine("    ✓ Analyze type specificity and complexity");
        Console.WriteLine("    ✓ Identify cross-connected types across classes");
        Console.WriteLine("    ✓ Generate classification patterns and statistics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multiple type recognition system complete");
        Console.ResetColor();
    }
}
