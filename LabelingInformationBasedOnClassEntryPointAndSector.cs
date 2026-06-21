using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class LabelingInformationBasedOnClassEntryPointAndSector
{
    public class InfoLabel
    {
        public string LabelId { get; set; }
        public string LabelName { get; set; }
        public string ClassCategory { get; set; }
        public string EntryPoint { get; set; }
        public string Sector { get; set; }
        public List<string> AssociatedTags { get; set; }
        public double RelevanceScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassDefinition
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string Description { get; set; }
        public List<string> SubClasses { get; set; }
        public int ItemsInClass { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SectorMapping
    {
        public string SectorId { get; set; }
        public string SectorName { get; set; }
        public List<string> EntryPoints { get; set; }
        public Dictionary<string, List<string>> ClassToLabelMap { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LabeledInformation
    {
        public string InformationId { get; set; }
        public string Content { get; set; }
        public string AssignedClass { get; set; }
        public string AssignedEntryPoint { get; set; }
        public string AssignedSector { get; set; }
        public string PrimaryLabel { get; set; }
        public List<string> SecondaryLabels { get; set; }
        public double LabelConfidence { get; set; }
        public DateTime LabeledDate { get; set; }
    }

    public class LabelingAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalLabeled { get; set; }
        public Dictionary<string, int> ClassDistribution { get; set; }
        public Dictionary<string, int> SectorDistribution { get; set; }
        public Dictionary<string, int> EntryPointDistribution { get; set; }
        public double AverageLabelConfidence { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class LabelingEngine
    {
        private Dictionary<string, InfoLabel> labels;
        private Dictionary<string, ClassDefinition> classes;
        private Dictionary<string, SectorMapping> sectors;
        private Dictionary<string, LabeledInformation> labeled;
        private Dictionary<string, LabelingAnalysis> analyses;

        public LabelingEngine()
        {
            labels = new Dictionary<string, InfoLabel>();
            classes = new Dictionary<string, ClassDefinition>();
            sectors = new Dictionary<string, SectorMapping>();
            labeled = new Dictionary<string, LabeledInformation>();
            analyses = new Dictionary<string, LabelingAnalysis>();
        }

        public void RegisterClass(string classId, string className, string description, List<string> subClasses)
        {
            var classDef = new ClassDefinition
            {
                ClassId = classId,
                ClassName = className,
                Description = description,
                SubClasses = new List<string>(subClasses),
                ItemsInClass = 0,
                CreatedDate = DateTime.Now
            };
            classes[classId] = classDef;
        }

        public void RegisterSector(string sectorId, string sectorName, List<string> entryPoints)
        {
            var sector = new SectorMapping
            {
                SectorId = sectorId,
                SectorName = sectorName,
                EntryPoints = new List<string>(entryPoints),
                ClassToLabelMap = new Dictionary<string, List<string>>(),
                CreatedDate = DateTime.Now
            };
            sectors[sectorId] = sector;
        }

        public void RegisterLabel(string labelId, string labelName, string classCategory, string entryPoint,
                                 string sector, List<string> tags, double relevance)
        {
            var label = new InfoLabel
            {
                LabelId = labelId,
                LabelName = labelName,
                ClassCategory = classCategory,
                EntryPoint = entryPoint,
                Sector = sector,
                AssociatedTags = new List<string>(tags),
                RelevanceScore = relevance,
                CreatedDate = DateTime.Now
            };
            labels[labelId] = label;
        }

        public void LabelInformation(string infoId, string content, string classCategory,
                                    string entryPoint, string sector)
        {
            var labeledInfo = new LabeledInformation
            {
                InformationId = infoId,
                Content = content,
                AssignedClass = classCategory,
                AssignedEntryPoint = entryPoint,
                AssignedSector = sector,
                PrimaryLabel = "",
                SecondaryLabels = new List<string>(),
                LabelConfidence = 0.0,
                LabeledDate = DateTime.Now
            };

            var matchingLabels = labels.Values.Where(l =>
                l.ClassCategory == classCategory &&
                l.EntryPoint == entryPoint &&
                l.Sector == sector).ToList();

            if (matchingLabels.Count > 0)
            {
                var primaryLabel = matchingLabels.OrderByDescending(l => l.RelevanceScore).First();
                labeledInfo.PrimaryLabel = primaryLabel.LabelName;
                labeledInfo.LabelConfidence = primaryLabel.RelevanceScore;

                foreach (var label in matchingLabels.Skip(1).Take(3))
                {
                    labeledInfo.SecondaryLabels.Add(label.LabelName);
                }
            }

            if (classes.ContainsKey(classCategory))
            {
                classes[classCategory].ItemsInClass++;
            }

            labeled[infoId] = labeledInfo;
        }

        public void AnalyzeLabelDistribution(string analysisId)
        {
            var analysis = new LabelingAnalysis
            {
                AnalysisId = analysisId,
                TotalLabeled = labeled.Count,
                ClassDistribution = new Dictionary<string, int>(),
                SectorDistribution = new Dictionary<string, int>(),
                EntryPointDistribution = new Dictionary<string, int>(),
                AverageLabelConfidence = 0.0,
                AnalyzedDate = DateTime.Now
            };

            foreach (var labeledInfo in labeled.Values)
            {
                if (!analysis.ClassDistribution.ContainsKey(labeledInfo.AssignedClass))
                    analysis.ClassDistribution[labeledInfo.AssignedClass] = 0;
                analysis.ClassDistribution[labeledInfo.AssignedClass]++;

                if (!analysis.SectorDistribution.ContainsKey(labeledInfo.AssignedSector))
                    analysis.SectorDistribution[labeledInfo.AssignedSector] = 0;
                analysis.SectorDistribution[labeledInfo.AssignedSector]++;

                if (!analysis.EntryPointDistribution.ContainsKey(labeledInfo.AssignedEntryPoint))
                    analysis.EntryPointDistribution[labeledInfo.AssignedEntryPoint] = 0;
                analysis.EntryPointDistribution[labeledInfo.AssignedEntryPoint]++;
            }

            analysis.AverageLabelConfidence = labeled.Count > 0 ?
                labeled.Values.Average(l => l.LabelConfidence) : 0.0;

            analyses[analysisId] = analysis;
        }

        public void DisplayLabel(string labelId)
        {
            if (!labels.ContainsKey(labelId)) return;

            var label = labels[labelId];
            Console.WriteLine($"\n  Label: {label.LabelName}");
            Console.WriteLine($"  Class: {label.ClassCategory}");
            Console.WriteLine($"  Entry Point: {label.EntryPoint}");
            Console.WriteLine($"  Sector: {label.Sector}");
            Console.WriteLine($"  Relevance: {label.RelevanceScore * 100:F0}%");
            Console.WriteLine($"  Tags: {string.Join(", ", label.AssociatedTags)}");
        }

        public void DisplayLabeledInfo(string infoId)
        {
            if (!labeled.ContainsKey(infoId)) return;

            var info = labeled[infoId];
            Console.WriteLine($"\n  Labeled Information: {info.InformationId}");
            Console.WriteLine($"  Class: {info.AssignedClass}");
            Console.WriteLine($"  Entry Point: {info.AssignedEntryPoint}");
            Console.WriteLine($"  Sector: {info.AssignedSector}");
            Console.WriteLine($"  Primary Label: {info.PrimaryLabel}");
            Console.WriteLine($"  Confidence: {info.LabelConfidence * 100:F0}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Labeling Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Labeled: {analysis.TotalLabeled}");
            Console.WriteLine($"  Average Confidence: {analysis.AverageLabelConfidence * 100:F0}%");
            Console.WriteLine($"  Classes: {analysis.ClassDistribution.Count}");
            Console.WriteLine($"  Sectors: {analysis.SectorDistribution.Count}");
        }

        public int GetTotalLabels()
        {
            return labels.Count;
        }

        public int GetTotalLabeledInformation()
        {
            return labeled.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Labeling Information Based on Class, Entry Point, Sector      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new LabelingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Information Classes]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterClass("CLASS-001", "Technical",
            "Information related to technical systems and processes",
            new List<string> { "Software", "Hardware", "Network" });

        engine.RegisterClass("CLASS-002", "Business",
            "Information related to business operations",
            new List<string> { "Finance", "Operations", "Strategy" });

        Console.WriteLine("  ✓ Registered 2 information classes");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Sectors]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSector("SECTOR-001", "Software Development",
            new List<string> { "Frontend", "Backend", "Database" });

        engine.RegisterSector("SECTOR-002", "Operations",
            new List<string> { "Planning", "Execution", "Monitoring" });

        Console.WriteLine("  ✓ Registered 2 sectors with entry points");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Registering Labels]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterLabel("LABEL-001", "API Architecture",
            "CLASS-001", "Backend", "SECTOR-001",
            new List<string> { "REST", "Microservices", "Integration" }, 0.95);

        engine.RegisterLabel("LABEL-002", "Database Design",
            "CLASS-001", "Database", "SECTOR-001",
            new List<string> { "Schema", "Optimization", "Performance" }, 0.90);

        engine.RegisterLabel("LABEL-003", "Project Planning",
            "CLASS-002", "Planning", "SECTOR-002",
            new List<string> { "Timeline", "Resources", "Budget" }, 0.85);

        Console.WriteLine("  ✓ Registered 3 labels");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Labeling Information]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LabelInformation("INFO-001", "Design RESTful API endpoints",
            "CLASS-001", "Backend", "SECTOR-001");

        engine.LabelInformation("INFO-002", "Optimize database queries",
            "CLASS-001", "Database", "SECTOR-001");

        engine.LabelInformation("INFO-003", "Create project timeline",
            "CLASS-002", "Planning", "SECTOR-002");

        Console.WriteLine("  ✓ Labeled 3 pieces of information");
        engine.DisplayLabeledInfo("INFO-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Displaying Labels]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayLabel("LABEL-001");
        engine.DisplayLabel("LABEL-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Label Distribution]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeLabelDistribution("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Information Labeling Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Labeling Architecture:");
        Console.WriteLine("    Layer 1: Class Definition (establish categories)");
        Console.WriteLine("    Layer 2: Sector Definition (define domain areas)");
        Console.WriteLine("    Layer 3: Entry Point Mapping (establish access paths)");
        Console.WriteLine("    Layer 4: Label Registration (create label definitions)");
        Console.WriteLine("    Layer 5: Label Matching (find applicable labels)");
        Console.WriteLine("    Layer 6: Confidence Calculation (measure label fit)");
        Console.WriteLine("    Layer 7: Distribution Analysis (track label usage)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define information classes with subcategories");
        Console.WriteLine("    ✓ Map sectors and entry points");
        Console.WriteLine("    ✓ Create multi-dimensional labels");
        Console.WriteLine("    ✓ Automatically label information");
        Console.WriteLine("    ✓ Track label confidence");
        Console.WriteLine("    ✓ Analyze label distribution");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Information labeling system complete");
        Console.ResetColor();
    }
}
