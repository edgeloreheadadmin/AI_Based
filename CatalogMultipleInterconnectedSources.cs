using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class CatalogMultipleInterconnectedSources
{
    public class InformationSource
    {
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public int TotalItems { get; set; }
        public List<string> CategorizedTopics { get; set; }
        public List<string> LinkedSources { get; set; }
        public double ReliabilityScore { get; set; }
        public DateTime RegisteredDate { get; set; }
    }

    public class CatalogEntry
    {
        public string EntryId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public List<string> Tags { get; set; }
        public List<string> SourceIds { get; set; }
        public Dictionary<string, string> CrossReferences { get; set; }
        public double RelevanceScore { get; set; }
        public DateTime IndexedDate { get; set; }
    }

    public class InformationNetwork
    {
        public string NetworkId { get; set; }
        public Dictionary<string, InformationSource> Sources { get; set; }
        public List<CatalogEntry> Catalog { get; set; }
        public Dictionary<string, List<string>> TopicIndex { get; set; }
        public int TotalConnections { get; set; }
        public double NetworkDensity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CatalogingEngine
    {
        private Dictionary<string, InformationNetwork> networks;
        private Dictionary<string, InformationSource> allSources;
        private List<(string, string, double)> crossSourceLinks;

        public CatalogingEngine()
        {
            networks = new Dictionary<string, InformationNetwork>();
            allSources = new Dictionary<string, InformationSource>();
            crossSourceLinks = new List<(string, string, double)>();
        }

        public void CreateInformationNetwork(string networkId)
        {
            var network = new InformationNetwork
            {
                NetworkId = networkId,
                Sources = new Dictionary<string, InformationSource>(),
                Catalog = new List<CatalogEntry>(),
                TopicIndex = new Dictionary<string, List<string>>(),
                TotalConnections = 0,
                NetworkDensity = 0.0,
                CreatedDate = DateTime.Now
            };
            networks[networkId] = network;
        }

        public void RegisterSource(string networkId, string sourceId, string sourceName,
                                  string sourceType, List<string> topics)
        {
            if (!networks.ContainsKey(networkId)) return;

            var source = new InformationSource
            {
                SourceId = sourceId,
                SourceName = sourceName,
                SourceType = sourceType,
                TotalItems = 0,
                CategorizedTopics = new List<string>(topics),
                LinkedSources = new List<string>(),
                ReliabilityScore = 0.8,
                RegisteredDate = DateTime.Now
            };

            networks[networkId].Sources[sourceId] = source;
            allSources[sourceId] = source;
        }

        public void LinkSources(string sourceId1, string sourceId2, double strength)
        {
            if (!allSources.ContainsKey(sourceId1) || !allSources.ContainsKey(sourceId2))
                return;

            var source1 = allSources[sourceId1];
            var source2 = allSources[sourceId2];

            if (!source1.LinkedSources.Contains(sourceId2))
                source1.LinkedSources.Add(sourceId2);

            if (!source2.LinkedSources.Contains(sourceId1))
                source2.LinkedSources.Add(sourceId1);

            crossSourceLinks.Add((sourceId1, sourceId2, strength));
        }

        public void AddCatalogEntry(string networkId, string entryId, string title,
                                   string description, List<string> sourceIds, List<string> tags)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            var entry = new CatalogEntry
            {
                EntryId = entryId,
                Title = title,
                Description = description,
                Tags = new List<string>(tags),
                SourceIds = new List<string>(sourceIds),
                CrossReferences = new Dictionary<string, string>(),
                RelevanceScore = CalculateRelevance(sourceIds, network),
                IndexedDate = DateTime.Now
            };

            network.Catalog.Add(entry);

            foreach (var sourceId in sourceIds)
            {
                if (network.Sources.ContainsKey(sourceId))
                {
                    network.Sources[sourceId].TotalItems++;
                }
            }

            UpdateTopicIndex(networkId, tags, entryId);
        }

        private double CalculateRelevance(List<string> sourceIds, InformationNetwork network)
        {
            if (sourceIds.Count == 0) return 0.0;

            double totalReliability = 0.0;
            foreach (var sourceId in sourceIds)
            {
                if (network.Sources.ContainsKey(sourceId))
                {
                    totalReliability += network.Sources[sourceId].ReliabilityScore;
                }
            }

            return totalReliability / sourceIds.Count;
        }

        private void UpdateTopicIndex(string networkId, List<string> topics, string entryId)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            foreach (var topic in topics)
            {
                if (!network.TopicIndex.ContainsKey(topic))
                {
                    network.TopicIndex[topic] = new List<string>();
                }

                if (!network.TopicIndex[topic].Contains(entryId))
                {
                    network.TopicIndex[topic].Add(entryId);
                }
            }
        }

        public void CreateCrossReferences(string networkId, string entryId1, string entryId2)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            var entry1 = network.Catalog.FirstOrDefault(e => e.EntryId == entryId1);
            var entry2 = network.Catalog.FirstOrDefault(e => e.EntryId == entryId2);

            if (entry1 != null && entry2 != null)
            {
                entry1.CrossReferences[entryId2] = "Related Entry";
                entry2.CrossReferences[entryId1] = "Related Entry";
                network.TotalConnections++;
            }
        }

        public void CalculateNetworkMetrics(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            int possibleConnections = network.Sources.Count * (network.Sources.Count - 1) / 2;
            int actualConnections = crossSourceLinks.Count;

            network.NetworkDensity = possibleConnections > 0 ?
                (double)actualConnections / possibleConnections : 0.0;
        }

        public List<CatalogEntry> SearchByTopic(string networkId, string topic)
        {
            if (!networks.ContainsKey(networkId)) return new List<CatalogEntry>();

            var network = networks[networkId];
            var results = new List<CatalogEntry>();

            if (network.TopicIndex.ContainsKey(topic))
            {
                foreach (var entryId in network.TopicIndex[topic])
                {
                    var entry = network.Catalog.FirstOrDefault(e => e.EntryId == entryId);
                    if (entry != null) results.Add(entry);
                }
            }

            return results;
        }

        public void DisplayNetworkStatus(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            Console.WriteLine($"\n  Network: {network.NetworkId}");
            Console.WriteLine($"  Sources: {network.Sources.Count}");
            Console.WriteLine($"  Catalog Entries: {network.Catalog.Count}");
            Console.WriteLine($"  Topics Indexed: {network.TopicIndex.Count}");
            Console.WriteLine($"  Total Connections: {network.TotalConnections}");
            Console.WriteLine($"  Network Density: {network.NetworkDensity * 100:F1}%");
        }

        public void DisplaySource(string sourceId)
        {
            if (!allSources.ContainsKey(sourceId)) return;

            var source = allSources[sourceId];
            Console.WriteLine($"\n  Source: {source.SourceName} ({source.SourceId})");
            Console.WriteLine($"  Type: {source.SourceType}");
            Console.WriteLine($"  Catalog Items: {source.TotalItems}");
            Console.WriteLine($"  Reliability: {source.ReliabilityScore * 100:F1}%");
            Console.WriteLine($"  Topics: {string.Join(", ", source.CategorizedTopics)}");
            if (source.LinkedSources.Count > 0)
            {
                Console.WriteLine($"  Linked Sources: {string.Join(", ", source.LinkedSources)}");
            }
        }

        public void DisplayCatalogEntry(string entryId, string networkId)
        {
            if (!networks.ContainsKey(networkId)) return;

            var network = networks[networkId];
            var entry = network.Catalog.FirstOrDefault(e => e.EntryId == entryId);
            if (entry == null) return;

            Console.WriteLine($"\n  Catalog Entry: {entry.Title}");
            Console.WriteLine($"  ID: {entry.EntryId}");
            Console.WriteLine($"  Description: {entry.Description}");
            Console.WriteLine($"  Sources: {string.Join(", ", entry.SourceIds)}");
            Console.WriteLine($"  Tags: {string.Join(", ", entry.Tags)}");
            Console.WriteLine($"  Relevance: {entry.RelevanceScore * 100:F1}%");
            if (entry.CrossReferences.Count > 0)
            {
                Console.WriteLine($"  Cross-References: {string.Join(", ", entry.CrossReferences.Keys)}");
            }
        }

        public Dictionary<string, int> GetSourceStatistics(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return new Dictionary<string, int>();

            var network = networks[networkId];
            return network.Sources.OrderByDescending(s => s.Value.TotalItems)
                .ToDictionary(s => s.Key, s => s.Value.TotalItems);
        }

        public Dictionary<string, int> GetTopicStatistics(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return new Dictionary<string, int>();

            var network = networks[networkId];
            return network.TopicIndex.OrderByDescending(t => t.Value.Count)
                .Take(10).ToDictionary(t => t.Key, t => t.Value.Count);
        }

        public int GetTotalCatalogSize(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return 0;
            return networks[networkId].Catalog.Count;
        }

        public int GetTotalSourceCount(string networkId)
        {
            if (!networks.ContainsKey(networkId)) return 0;
            return networks[networkId].Sources.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Catalog Information Across Multiple Interconnected Sources║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CatalogingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Information Networks]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateInformationNetwork("ResearchNetwork");
        engine.CreateInformationNetwork("EducationalNetwork");

        Console.WriteLine("  ✓ Created 2 information networks");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Information Sources]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSource("ResearchNetwork", "SRC-001", "Journal Database", "Academic",
            new List<string> { "Science", "Technology", "Medicine" });
        engine.RegisterSource("ResearchNetwork", "SRC-002", "Archive System", "Repository",
            new List<string> { "History", "Archives", "Documents" });
        engine.RegisterSource("ResearchNetwork", "SRC-003", "Online Repository", "Digital",
            new List<string> { "Research", "Data", "Publications" });

        engine.RegisterSource("EducationalNetwork", "SRC-004", "Learning Platform", "Educational",
            new List<string> { "Courses", "Materials", "Assessments" });
        engine.RegisterSource("EducationalNetwork", "SRC-005", "Book Collection", "Library",
            new List<string> { "Literature", "Reference", "Study" });

        Console.WriteLine("  ✓ Registered 5 sources across 2 networks");
        engine.DisplaySource("SRC-001");
        engine.DisplaySource("SRC-004");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Source Interconnections]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkSources("SRC-001", "SRC-002", 0.85);
        engine.LinkSources("SRC-001", "SRC-003", 0.90);
        engine.LinkSources("SRC-002", "SRC-003", 0.75);
        engine.LinkSources("SRC-004", "SRC-005", 0.80);

        Console.WriteLine("  ✓ Created 4 source interconnections");
        engine.DisplaySource("SRC-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Adding Catalog Entries]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AddCatalogEntry("ResearchNetwork", "CAT-001",
            "Quantum Computing Advances",
            "Recent breakthroughs in quantum computing technology",
            new List<string> { "SRC-001", "SRC-003" },
            new List<string> { "Technology", "Science", "Computing" });

        engine.AddCatalogEntry("ResearchNetwork", "CAT-002",
            "Historical Scientific Documents",
            "Collection of important historical scientific papers",
            new List<string> { "SRC-002" },
            new List<string> { "History", "Science", "Archives" });

        engine.AddCatalogEntry("EducationalNetwork", "CAT-003",
            "Advanced Mathematics Course",
            "Comprehensive course on advanced mathematics",
            new List<string> { "SRC-004", "SRC-005" },
            new List<string> { "Mathematics", "Education", "Courses" });

        engine.AddCatalogEntry("EducationalNetwork", "CAT-004",
            "Research Methodology Guide",
            "Guide to conducting effective research",
            new List<string> { "SRC-004", "SRC-001" },
            new List<string> { "Research", "Methodology", "Guide" });

        Console.WriteLine("  ✓ Added 4 catalog entries across networks");
        engine.DisplayCatalogEntry("CAT-001", "ResearchNetwork");
        engine.DisplayCatalogEntry("CAT-003", "EducationalNetwork");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Creating Cross-References]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateCrossReferences("ResearchNetwork", "CAT-001", "CAT-002");
        engine.CreateCrossReferences("EducationalNetwork", "CAT-003", "CAT-004");

        Console.WriteLine("  ✓ Created cross-references between entries");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Network Metrics and Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CalculateNetworkMetrics("ResearchNetwork");
        engine.CalculateNetworkMetrics("EducationalNetwork");

        engine.DisplayNetworkStatus("ResearchNetwork");
        engine.DisplayNetworkStatus("EducationalNetwork");

        var researchSources = engine.GetSourceStatistics("ResearchNetwork");
        Console.WriteLine("\n  Research Network Source Statistics:");
        foreach (var kvp in researchSources)
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} items");
        }

        var topics = engine.GetTopicStatistics("ResearchNetwork");
        Console.WriteLine("\n  Top Topics in Research Network:");
        foreach (var kvp in topics)
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} entries");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Source Cataloging Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Total Catalog Size (Research): {engine.GetTotalCatalogSize("ResearchNetwork")} entries");
        Console.WriteLine($"  Total Sources (Research): {engine.GetTotalSourceCount("ResearchNetwork")}");
        Console.WriteLine($"  Total Catalog Size (Educational): {engine.GetTotalCatalogSize("EducationalNetwork")} entries");
        Console.WriteLine($"  Total Sources (Educational): {engine.GetTotalSourceCount("EducationalNetwork")}");

        Console.WriteLine("\n  Multi-Source Cataloging Architecture:");
        Console.WriteLine("    Layer 1: Network Creation (establish information domains)");
        Console.WriteLine("    Layer 2: Source Registration (add information sources)");
        Console.WriteLine("    Layer 3: Source Linking (create interconnections)");
        Console.WriteLine("    Layer 4: Catalog Entries (index information)");
        Console.WriteLine("    Layer 5: Topic Indexing (organize by topic)");
        Console.WriteLine("    Layer 6: Cross-References (link related entries)");
        Console.WriteLine("    Layer 7: Network Analysis (measure density and connectivity)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Multiple independent information networks");
        Console.WriteLine("    ✓ Diverse source types and categories");
        Console.WriteLine("    ✓ Interconnected source relationships");
        Console.WriteLine("    ✓ Topic-based indexing and retrieval");
        Console.WriteLine("    ✓ Cross-reference tracking");
        Console.WriteLine("    ✓ Network density analysis");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-source cataloging system complete");
        Console.ResetColor();
    }
}
