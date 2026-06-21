using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CrossReferencingInformation
{
    public class InformationItem
    {
        public string ItemId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public List<string> CrossReferences { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Reference
    {
        public string ReferenceId { get; set; }
        public string SourceItemId { get; set; }
        public string TargetItemId { get; set; }
        public string ReferenceType { get; set; }
        public string Relationship { get; set; }
        public double RelevanceScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReferenceNetwork
    {
        public string NetworkId { get; set; }
        public List<string> ItemIds { get; set; }
        public int TotalNodes { get; set; }
        public int TotalEdges { get; set; }
        public double NetworkConnectivity { get; set; }
        public List<string> KeyItems { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CrossReferenceAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalItems { get; set; }
        public int TotalReferences { get; set; }
        public Dictionary<string, int> ReferenceTypeDistribution { get; set; }
        public Dictionary<string, int> ItemCitationCount { get; set; }
        public double AverageRelevance { get; set; }
        public List<string> MostCitedItems { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class CrossReferenceEngine
    {
        private Dictionary<string, InformationItem> items;
        private Dictionary<string, Reference> references;
        private Dictionary<string, ReferenceNetwork> networks;
        private Dictionary<string, CrossReferenceAnalysis> analyses;

        public CrossReferenceEngine()
        {
            items = new Dictionary<string, InformationItem>();
            references = new Dictionary<string, Reference>();
            networks = new Dictionary<string, ReferenceNetwork>();
            analyses = new Dictionary<string, CrossReferenceAnalysis>();
        }

        public void RegisterInformationItem(string itemId, string title, string content, string category)
        {
            var item = new InformationItem
            {
                ItemId = itemId,
                Title = title,
                Content = content,
                Category = category,
                CrossReferences = new List<string>(),
                CreatedDate = DateTime.Now
            };
            items[itemId] = item;
        }

        public void CreateCrossReference(string referenceId, string sourceId, string targetId,
                                        string refType, string relationship)
        {
            if (!items.ContainsKey(sourceId) || !items.ContainsKey(targetId)) return;

            var reference = new Reference
            {
                ReferenceId = referenceId,
                SourceItemId = sourceId,
                TargetItemId = targetId,
                ReferenceType = refType,
                Relationship = relationship,
                RelevanceScore = CalculateRelevance(sourceId, targetId),
                CreatedDate = DateTime.Now
            };

            items[sourceId].CrossReferences.Add(targetId);
            references[referenceId] = reference;
        }

        private double CalculateRelevance(string sourceId, string targetId)
        {
            var source = items[sourceId];
            var target = items[targetId];

            double relevance = 0.5;

            if (source.Category == target.Category)
                relevance += 0.3;

            if (source.Content.Contains(target.Title, StringComparison.OrdinalIgnoreCase))
                relevance += 0.2;

            return Math.Min(relevance, 1.0);
        }

        public void BuildReferenceNetwork(string networkId, List<string> itemIds)
        {
            var network = new ReferenceNetwork
            {
                NetworkId = networkId,
                ItemIds = new List<string>(itemIds),
                TotalNodes = itemIds.Count,
                TotalEdges = 0,
                NetworkConnectivity = 0.0,
                KeyItems = new List<string>(),
                CreatedDate = DateTime.Now
            };

            int edgeCount = 0;
            foreach (var itemId in itemIds)
            {
                if (items.ContainsKey(itemId))
                {
                    edgeCount += items[itemId].CrossReferences.Count(cr => itemIds.Contains(cr));
                }
            }

            network.TotalEdges = edgeCount;
            network.NetworkConnectivity = CalculateConnectivity(itemIds, edgeCount);
            network.KeyItems = IdentifyKeyItems(itemIds);

            networks[networkId] = network;
        }

        private double CalculateConnectivity(List<string> itemIds, int edgeCount)
        {
            int possibleEdges = (itemIds.Count * (itemIds.Count - 1)) / 2;
            return possibleEdges > 0 ? (double)edgeCount / possibleEdges : 0.0;
        }

        private List<string> IdentifyKeyItems(List<string> itemIds)
        {
            var citationCounts = new Dictionary<string, int>();

            foreach (var itemId in itemIds)
            {
                int count = 0;
                foreach (var item in items.Values)
                {
                    if (itemIds.Contains(item.ItemId) && item.CrossReferences.Contains(itemId))
                        count++;
                }
                citationCounts[itemId] = count;
            }

            return citationCounts
                .OrderByDescending(x => x.Value)
                .Take(3)
                .Select(x => x.Key)
                .ToList();
        }

        public void AnalyzeCrossReferences(string analysisId)
        {
            var analysis = new CrossReferenceAnalysis
            {
                AnalysisId = analysisId,
                TotalItems = items.Count,
                TotalReferences = references.Count,
                ReferenceTypeDistribution = new Dictionary<string, int>(),
                ItemCitationCount = new Dictionary<string, int>(),
                AverageRelevance = references.Count > 0 ? references.Values.Average(r => r.RelevanceScore) : 0.0,
                MostCitedItems = new List<string>(),
                AnalyzedDate = DateTime.Now
            };

            foreach (var reference in references.Values)
            {
                if (!analysis.ReferenceTypeDistribution.ContainsKey(reference.ReferenceType))
                    analysis.ReferenceTypeDistribution[reference.ReferenceType] = 0;
                analysis.ReferenceTypeDistribution[reference.ReferenceType]++;

                if (!analysis.ItemCitationCount.ContainsKey(reference.TargetItemId))
                    analysis.ItemCitationCount[reference.TargetItemId] = 0;
                analysis.ItemCitationCount[reference.TargetItemId]++;
            }

            analysis.MostCitedItems = analysis.ItemCitationCount
                .OrderByDescending(x => x.Value)
                .Take(5)
                .Select(x => x.Key)
                .ToList();

            analyses[analysisId] = analysis;
        }

        public void DisplayItem(string itemId)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            Console.WriteLine($"\n  Information Item: {item.Title}");
            Console.WriteLine($"  ID: {item.ItemId}");
            Console.WriteLine($"  Category: {item.Category}");
            Console.WriteLine($"  Cross References: {item.CrossReferences.Count}");
        }

        public void DisplayReference(string referenceId)
        {
            if (!references.ContainsKey(referenceId)) return;

            var reference = references[referenceId];
            Console.WriteLine($"\n  Cross Reference: {reference.ReferenceId}");
            Console.WriteLine($"  From: {reference.SourceItemId}");
            Console.WriteLine($"  To: {reference.TargetItemId}");
            Console.WriteLine($"  Type: {reference.ReferenceType}");
            Console.WriteLine($"  Relevance: {reference.RelevanceScore * 100:F0}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Cross-Reference Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Items: {analysis.TotalItems}");
            Console.WriteLine($"  Total References: {analysis.TotalReferences}");
            Console.WriteLine($"  Average Relevance: {analysis.AverageRelevance * 100:F0}%");
            Console.WriteLine($"  Reference Types: {analysis.ReferenceTypeDistribution.Count}");
            Console.WriteLine($"  Most Cited: {string.Join(", ", analysis.MostCitedItems.Take(3))}");
        }

        public int GetTotalItems()
        {
            return items.Count;
        }

        public int GetTotalReferences()
        {
            return references.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Cross Referencing Information                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CrossReferenceEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Information Items]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterInformationItem("ITEM-001", "Software Architecture Patterns",
            "Overview of architectural patterns for software design", "Technical");

        engine.RegisterInformationItem("ITEM-002", "Microservices Design",
            "Deep dive into microservices architecture patterns", "Technical");

        engine.RegisterInformationItem("ITEM-003", "API Design Principles",
            "Best practices for designing REST APIs", "Technical");

        engine.RegisterInformationItem("ITEM-004", "Database Optimization",
            "Techniques for optimizing database performance", "Technical");

        engine.RegisterInformationItem("ITEM-005", "Project Management",
            "Strategies for effective project management", "Business");

        Console.WriteLine("  ✓ Registered 5 information items");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Items]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayItem("ITEM-001");
        engine.DisplayItem("ITEM-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Cross References]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateCrossReference("REF-001", "ITEM-002", "ITEM-001",
            "Related", "Microservices is an architectural pattern");

        engine.CreateCrossReference("REF-002", "ITEM-003", "ITEM-001",
            "Related", "API design follows architectural principles");

        engine.CreateCrossReference("REF-003", "ITEM-002", "ITEM-004",
            "Depends", "Microservices require efficient databases");

        engine.CreateCrossReference("REF-004", "ITEM-001", "ITEM-005",
            "Context", "Architecture decisions are management concerns");

        Console.WriteLine("  ✓ Created 4 cross references");
        engine.DisplayReference("REF-001");
        engine.DisplayReference("REF-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Building Reference Networks]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.BuildReferenceNetwork("NETWORK-001",
            new List<string> { "ITEM-001", "ITEM-002", "ITEM-003", "ITEM-004" });

        Console.WriteLine("  ✓ Built 1 reference network");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Cross References]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeCrossReferences("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Cross Reference Summary:");
        Console.WriteLine($"    Total Items: {engine.GetTotalItems()}");
        Console.WriteLine($"    Total References: {engine.GetTotalReferences()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Cross Referencing Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Cross-Reference Architecture:");
        Console.WriteLine("    Layer 1: Item Registration (define information items)");
        Console.WriteLine("    Layer 2: Category Assignment (organize items)");
        Console.WriteLine("    Layer 3: Reference Creation (link related items)");
        Console.WriteLine("    Layer 4: Relevance Calculation (measure connection strength)");
        Console.WriteLine("    Layer 5: Network Building (create item networks)");
        Console.WriteLine("    Layer 6: Connectivity Analysis (measure network density)");
        Console.WriteLine("    Layer 7: Citation Analysis (identify key items)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register information items");
        Console.WriteLine("    ✓ Create cross-references between items");
        Console.WriteLine("    ✓ Calculate reference relevance");
        Console.WriteLine("    ✓ Build reference networks");
        Console.WriteLine("    ✓ Analyze citation patterns");
        Console.WriteLine("    ✓ Identify key/central items");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Cross-referencing system complete");
        Console.ResetColor();
    }
}
