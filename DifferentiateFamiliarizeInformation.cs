using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DifferentiateFamiliarizeInformation
{
    public class InformationItem
    {
        public string ItemId { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public double Familiarity { get; set; }
        public double Distinctness { get; set; }
        public int ExposureCount { get; set; }
        public List<string> DistinguishingFeatures { get; set; }
        public DateTime FirstEncountered { get; set; }
    }

    public class InformationCluster
    {
        public string ClusterId { get; set; }
        public List<string> MemberItems { get; set; }
        public double SimilarityScore { get; set; }
        public double CohesionStrength { get; set; }
        public List<string> CommonCharacteristics { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DifferentiationMap
    {
        public string MapName { get; set; }
        public Dictionary<string, double> DifferentiationScores { get; set; }
        public List<(string, string, double)> ContrastPairs { get; set; }
        public double DiscriminabilityIndex { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InformationProcessingEngine
    {
        private Dictionary<string, InformationItem> items;
        private List<InformationCluster> clusters;
        private Dictionary<string, DifferentiationMap> maps;
        private Dictionary<string, List<string>> familiarityHistory;

        public InformationProcessingEngine()
        {
            items = new Dictionary<string, InformationItem>();
            clusters = new List<InformationCluster>();
            maps = new Dictionary<string, DifferentiationMap>();
            familiarityHistory = new Dictionary<string, List<string>>();
        }

        public void IntroduceInformation(string itemId, string content, string category)
        {
            var item = new InformationItem
            {
                ItemId = itemId,
                Content = content,
                Category = category,
                Familiarity = 0.1,
                Distinctness = 0.5,
                ExposureCount = 1,
                DistinguishingFeatures = new List<string>(),
                FirstEncountered = DateTime.Now
            };
            items[itemId] = item;
            familiarityHistory[itemId] = new List<string> { content };
        }

        public void FamiliarizeWithInformation(string itemId, double familiarityBoost)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            item.Familiarity = Math.Min(item.Familiarity + familiarityBoost, 0.99);
            item.ExposureCount++;
            familiarityHistory[itemId].Add($"Exposure {item.ExposureCount}");
        }

        public void IdentifyDistinguishingFeatures(string itemId, List<string> features)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            foreach (var feature in features)
            {
                if (!item.DistinguishingFeatures.Contains(feature))
                {
                    item.DistinguishingFeatures.Add(feature);
                }
            }

            item.Distinctness = Math.Min(item.Distinctness + (features.Count * 0.1), 0.99);
        }

        public double CalculateSimilarity(string itemId1, string itemId2)
        {
            if (!items.ContainsKey(itemId1) || !items.ContainsKey(itemId2))
                return 0.0;

            var item1 = items[itemId1];
            var item2 = items[itemId2];

            if (item1.Category != item2.Category) return 0.0;

            int commonFeatures = item1.DistinguishingFeatures
                .Intersect(item2.DistinguishingFeatures).Count();

            int totalFeatures = item1.DistinguishingFeatures
                .Union(item2.DistinguishingFeatures).Count();

            if (totalFeatures == 0) return 0.5;

            return (double)commonFeatures / totalFeatures;
        }

        public void CreateInformationCluster(string clusterId, List<string> memberItemIds)
        {
            var cluster = new InformationCluster
            {
                ClusterId = clusterId,
                MemberItems = new List<string>(memberItemIds),
                SimilarityScore = 0.0,
                CohesionStrength = 0.0,
                CommonCharacteristics = new List<string>(),
                CreatedDate = DateTime.Now
            };

            double totalSimilarity = 0.0;
            int pairCount = 0;

            for (int i = 0; i < memberItemIds.Count - 1; i++)
            {
                for (int j = i + 1; j < memberItemIds.Count; j++)
                {
                    totalSimilarity += CalculateSimilarity(memberItemIds[i], memberItemIds[j]);
                    pairCount++;
                }
            }

            cluster.SimilarityScore = pairCount > 0 ? totalSimilarity / pairCount : 0.5;
            cluster.CohesionStrength = cluster.SimilarityScore;

            ExtractCommonCharacteristics(cluster);
            clusters.Add(cluster);
        }

        private void ExtractCommonCharacteristics(InformationCluster cluster)
        {
            if (cluster.MemberItems.Count == 0) return;

            var commonFeatures = new HashSet<string>();
            if (items.ContainsKey(cluster.MemberItems[0]))
            {
                commonFeatures = new HashSet<string>(items[cluster.MemberItems[0]].DistinguishingFeatures);
            }

            for (int i = 1; i < cluster.MemberItems.Count; i++)
            {
                if (items.ContainsKey(cluster.MemberItems[i]))
                {
                    commonFeatures.IntersectWith(items[cluster.MemberItems[i]].DistinguishingFeatures);
                }
            }

            cluster.CommonCharacteristics = commonFeatures.ToList();
        }

        public void CreateDifferentiationMap(string mapName, List<string> itemIds)
        {
            var map = new DifferentiationMap
            {
                MapName = mapName,
                DifferentiationScores = new Dictionary<string, double>(),
                ContrastPairs = new List<(string, string, double)>(),
                DiscriminabilityIndex = 0.0,
                CreatedDate = DateTime.Now
            };

            for (int i = 0; i < itemIds.Count; i++)
            {
                string id = itemIds[i];
                if (!items.ContainsKey(id)) continue;

                double uniqueFeatures = items[id].DistinguishingFeatures.Count;
                double distinctiveness = items[id].Distinctness;

                map.DifferentiationScores[id] = (uniqueFeatures / 10.0) * distinctiveness;
            }

            for (int i = 0; i < itemIds.Count - 1; i++)
            {
                for (int j = i + 1; j < itemIds.Count; j++)
                {
                    double dissimilarity = 1.0 - CalculateSimilarity(itemIds[i], itemIds[j]);
                    map.ContrastPairs.Add((itemIds[i], itemIds[j], dissimilarity));
                }
            }

            map.DiscriminabilityIndex = map.DifferentiationScores.Count > 0 ?
                map.DifferentiationScores.Values.Average() : 0.0;

            maps[mapName] = map;
        }

        public void DisplayItemStatus(string itemId)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            Console.WriteLine($"\n  Information Item: {item.ItemId}");
            Console.WriteLine($"  Content: {item.Content}");
            Console.WriteLine($"  Category: {item.Category}");
            Console.WriteLine($"  Familiarity: {item.Familiarity * 100:F1}%");
            Console.WriteLine($"  Distinctness: {item.Distinctness * 100:F1}%");
            Console.WriteLine($"  Exposures: {item.ExposureCount}");
            if (item.DistinguishingFeatures.Count > 0)
            {
                Console.WriteLine($"  Distinguishing Features: {string.Join(", ", item.DistinguishingFeatures.Take(5))}");
            }
        }

        public void DisplayClusterStatus(string clusterId)
        {
            var cluster = clusters.FirstOrDefault(c => c.ClusterId == clusterId);
            if (cluster == null) return;

            Console.WriteLine($"\n  Information Cluster: {cluster.ClusterId}");
            Console.WriteLine($"  Members: {cluster.MemberItems.Count}");
            Console.WriteLine($"  Similarity Score: {cluster.SimilarityScore * 100:F1}%");
            Console.WriteLine($"  Cohesion Strength: {cluster.CohesionStrength * 100:F1}%");
            Console.WriteLine($"  Member Items: {string.Join(", ", cluster.MemberItems)}");
            if (cluster.CommonCharacteristics.Count > 0)
            {
                Console.WriteLine($"  Common Characteristics: {string.Join(", ", cluster.CommonCharacteristics)}");
            }
        }

        public void DisplayDifferentiationMap(string mapName)
        {
            if (!maps.ContainsKey(mapName)) return;

            var map = maps[mapName];
            Console.WriteLine($"\n  Differentiation Map: {map.MapName}");
            Console.WriteLine($"  Discriminability Index: {map.DiscriminabilityIndex * 100:F1}%");
            Console.WriteLine($"  Item Differentiation Scores:");
            foreach (var kvp in map.DifferentiationScores.OrderByDescending(x => x.Value).Take(5))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F1}%");
            }
            Console.WriteLine($"  Top Contrast Pairs:");
            foreach (var pair in map.ContrastPairs.OrderByDescending(x => x.Item3).Take(3))
            {
                Console.WriteLine($"    {pair.Item1} vs {pair.Item2}: {pair.Item3 * 100:F0}% dissimilarity");
            }
        }

        public Dictionary<string, double> GetFamiliarityRankings()
        {
            return items.OrderByDescending(i => i.Value.Familiarity)
                .ToDictionary(i => i.Key, i => i.Value.Familiarity);
        }

        public Dictionary<string, double> GetDistinctnessRankings()
        {
            return items.OrderByDescending(i => i.Value.Distinctness)
                .ToDictionary(i => i.Key, i => i.Value.Distinctness);
        }

        public double GetAverageFamiliarity()
        {
            return items.Count > 0 ? items.Values.Average(i => i.Familiarity) : 0.0;
        }

        public double GetAverageDistinctness()
        {
            return items.Count > 0 ? items.Values.Average(i => i.Distinctness) : 0.0;
        }

        public string GetCognitiveDevelopmentLevel(double familiarity, double distinctness)
        {
            double combined = (familiarity + distinctness) / 2.0;
            return combined switch
            {
                >= 0.85 => "Expertly Differentiated - Perfect distinction and deep familiarity",
                >= 0.70 => "Well Differentiated - Clear distinctions, familiar patterns",
                >= 0.55 => "Moderately Differentiated - Some distinctions emerging",
                >= 0.40 => "Developing - Learning distinctions, building familiarity",
                _ => "Emerging - Initial exposure, basic patterns"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Differentiate and Familiarize Information                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new InformationProcessingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Introducing Information]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.IntroduceInformation("Animal-Dog", "Four-legged domestic canine", "Animals");
        engine.IntroduceInformation("Animal-Cat", "Four-legged domestic feline", "Animals");
        engine.IntroduceInformation("Animal-Bird", "Winged avian creature", "Animals");
        engine.IntroduceInformation("Animal-Fish", "Aquatic gill-breathing creature", "Animals");
        engine.IntroduceInformation("Color-Red", "Warm color on visual spectrum", "Colors");
        engine.IntroduceInformation("Color-Blue", "Cool color on visual spectrum", "Colors");

        Console.WriteLine("  ✓ Introduced 6 information items across 2 categories");
        engine.DisplayItemStatus("Animal-Dog");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Identifying Distinguishing Features]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.IdentifyDistinguishingFeatures("Animal-Dog",
            new List<string> { "Barks", "Loyal", "Domesticated", "FourLegs", "Fur" });
        engine.IdentifyDistinguishingFeatures("Animal-Cat",
            new List<string> { "Meows", "Independent", "Domesticated", "FourLegs", "Fur" });
        engine.IdentifyDistinguishingFeatures("Animal-Bird",
            new List<string> { "Sings", "Wings", "Feathers", "TwoLegs", "Lays_Eggs" });
        engine.IdentifyDistinguishingFeatures("Animal-Fish",
            new List<string> { "Aquatic", "Gills", "Scales", "Fins", "NoLegs" });

        Console.WriteLine("  ✓ Identified distinguishing features for each item");
        engine.DisplayItemStatus("Animal-Dog");
        engine.DisplayItemStatus("Animal-Bird");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Building Familiarity Through Exposure]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Building familiarity through repeated exposure:");
        for (int cycle = 0; cycle < 5; cycle++)
        {
            engine.FamiliarizeWithInformation("Animal-Dog", 0.15);
            engine.FamiliarizeWithInformation("Animal-Cat", 0.12);
            engine.FamiliarizeWithInformation("Animal-Bird", 0.10);
            engine.FamiliarizeWithInformation("Animal-Fish", 0.08);
            Console.WriteLine($"  Exposure cycle {cycle + 1} completed");
        }

        Console.WriteLine("  ✓ Familiarity significantly increased through exposure");
        var familiarities = engine.GetFamiliarityRankings();
        foreach (var kvp in familiarities.Take(3))
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F1}% familiar");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Creating Information Clusters]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateInformationCluster("DomesticAnimals",
            new List<string> { "Animal-Dog", "Animal-Cat" });

        engine.CreateInformationCluster("WildAnimals",
            new List<string> { "Animal-Bird", "Animal-Fish" });

        Console.WriteLine("  ✓ Created 2 information clusters based on similarity");
        engine.DisplayClusterStatus("DomesticAnimals");
        engine.DisplayClusterStatus("WildAnimals");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Building Differentiation Maps]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateDifferentiationMap("AnimalDifferentiation",
            new List<string> { "Animal-Dog", "Animal-Cat", "Animal-Bird", "Animal-Fish" });

        Console.WriteLine("  ✓ Created differentiation map for animal category");
        engine.DisplayDifferentiationMap("AnimalDifferentiation");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Comparative Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var distinctness = engine.GetDistinctnessRankings();
        Console.WriteLine("  Distinctness Rankings:");
        int rank = 1;
        foreach (var kvp in distinctness.Take(6))
        {
            Console.WriteLine($"  #{rank}: {kvp.Key} - {kvp.Value * 100:F1}%");
            rank++;
        }

        Console.WriteLine($"\n  System Metrics:");
        Console.WriteLine($"    Average Familiarity: {engine.GetAverageFamiliarity() * 100:F1}%");
        Console.WriteLine($"    Average Distinctness: {engine.GetAverageDistinctness() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Information Processing Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        double avgFam = engine.GetAverageFamiliarity();
        double avgDist = engine.GetAverageDistinctness();
        string level = engine.GetCognitiveDevelopmentLevel(avgFam, avgDist);

        Console.WriteLine($"  Cognitive Development Level: {level}");

        Console.WriteLine("\n  Differentiation Framework:");
        Console.WriteLine("    Layer 1: Information Introduction (initial exposure)");
        Console.WriteLine("    Layer 2: Feature Identification (what makes it unique)");
        Console.WriteLine("    Layer 3: Familiarization (repeated exposure)");
        Console.WriteLine("    Layer 4: Similarity Analysis (finding relationships)");
        Console.WriteLine("    Layer 5: Clustering (grouping by similarity)");
        Console.WriteLine("    Layer 6: Differentiation (mapping distinctions)");
        Console.WriteLine("    Layer 7: Expert Recognition (fluent differentiation)");
        Console.WriteLine("\n  Key Processes:");
        Console.WriteLine("    ✓ Distinguishing Features - Identify unique characteristics");
        Console.WriteLine("    ✓ Similarity Scoring - Compare across dimensions");
        Console.WriteLine("    ✓ Cluster Formation - Group related information");
        Console.WriteLine("    ✓ Differentiation Mapping - Create distinction matrices");
        Console.WriteLine("    ✓ Familiarity Building - Increase through exposure");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Information differentiation system complete");
        Console.ResetColor();
    }
}
