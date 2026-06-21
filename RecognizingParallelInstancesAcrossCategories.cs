using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingParallelInstancesAcrossCategories
{
    public class Category
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryType { get; set; }
        public List<string> InstanceIds { get; set; }
        public int TotalInstances { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Instance
    {
        public string InstanceId { get; set; }
        public string InstanceName { get; set; }
        public string CategoryId { get; set; }
        public Dictionary<string, double> Properties { get; set; }
        public double MagnitudeNorm { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ParallelMapping
    {
        public string MappingId { get; set; }
        public string SourceInstanceId { get; set; }
        public string TargetInstanceId { get; set; }
        public string SourceCategoryId { get; set; }
        public string TargetCategoryId { get; set; }
        public double SimilarityScore { get; set; }
        public List<string> MatchedProperties { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SimilarityScore
    {
        public string ScoreId { get; set; }
        public string InstancePair { get; set; }
        public double CosineSimilarity { get; set; }
        public double EuclideanDistance { get; set; }
        public double PropertyMatchPercentage { get; set; }
        public string SimilarityRating { get; set; }
        public DateTime ScoredDate { get; set; }
    }

    public class InstanceCluster
    {
        public string ClusterId { get; set; }
        public List<string> InstanceIds { get; set; }
        public List<string> CategoryIds { get; set; }
        public double AverageSimilarity { get; set; }
        public int ParallelInstanceCount { get; set; }
        public string ClusterPattern { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ParallelInstanceEngine
    {
        private Dictionary<string, Category> categories;
        private Dictionary<string, Instance> instances;
        private Dictionary<string, ParallelMapping> mappings;
        private Dictionary<string, SimilarityScore> scores;
        private Dictionary<string, InstanceCluster> clusters;

        public ParallelInstanceEngine()
        {
            categories = new Dictionary<string, Category>();
            instances = new Dictionary<string, Instance>();
            mappings = new Dictionary<string, ParallelMapping>();
            scores = new Dictionary<string, SimilarityScore>();
            clusters = new Dictionary<string, InstanceCluster>();
        }

        public void RegisterCategory(string categoryId, string categoryName, string categoryType)
        {
            var category = new Category
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                CategoryType = categoryType,
                InstanceIds = new List<string>(),
                TotalInstances = 0,
                CreatedDate = DateTime.Now
            };
            categories[categoryId] = category;
        }

        public void CreateInstance(string instanceId, string instanceName, string categoryId, Dictionary<string, double> properties)
        {
            if (!categories.ContainsKey(categoryId)) return;

            var instance = new Instance
            {
                InstanceId = instanceId,
                InstanceName = instanceName,
                CategoryId = categoryId,
                Properties = new Dictionary<string, double>(properties),
                MagnitudeNorm = 0.0,
                CreatedDate = DateTime.Now
            };

            double sumOfSquares = 0.0;
            foreach (var value in properties.Values)
            {
                sumOfSquares += value * value;
            }
            instance.MagnitudeNorm = Math.Sqrt(sumOfSquares);

            instances[instanceId] = instance;
            categories[categoryId].InstanceIds.Add(instanceId);
            categories[categoryId].TotalInstances++;
        }

        public void FindParallelInstances(string sourceInstanceId, string targetInstanceId)
        {
            if (!instances.ContainsKey(sourceInstanceId) || !instances.ContainsKey(targetInstanceId))
                return;

            var source = instances[sourceInstanceId];
            var target = instances[targetInstanceId];

            double cosineSim = CalculateCosineSimilarity(source, target);
            double euclideanDist = CalculateEuclideanDistance(source, target);

            var matchedProperties = FindMatchedProperties(source, target);
            double propertyMatch = source.Properties.Count > 0 ?
                (double)matchedProperties.Count / source.Properties.Count : 0.0;

            string mappingId = $"Parallel-{sourceInstanceId}-{targetInstanceId}";
            var mapping = new ParallelMapping
            {
                MappingId = mappingId,
                SourceInstanceId = sourceInstanceId,
                TargetInstanceId = targetInstanceId,
                SourceCategoryId = source.CategoryId,
                TargetCategoryId = target.CategoryId,
                SimilarityScore = cosineSim,
                MatchedProperties = matchedProperties,
                CreatedDate = DateTime.Now
            };
            mappings[mappingId] = mapping;

            var score = new SimilarityScore
            {
                ScoreId = $"Score-{sourceInstanceId}-{targetInstanceId}",
                InstancePair = $"{sourceInstanceId}↔{targetInstanceId}",
                CosineSimilarity = cosineSim,
                EuclideanDistance = euclideanDist,
                PropertyMatchPercentage = propertyMatch,
                SimilarityRating = GetSimilarityRating(cosineSim),
                ScoredDate = DateTime.Now
            };
            scores[score.ScoreId] = score;
        }

        private double CalculateCosineSimilarity(Instance inst1, Instance inst2)
        {
            double dotProduct = 0.0;
            var commonProps = inst1.Properties.Keys.Intersect(inst2.Properties.Keys);

            foreach (var prop in commonProps)
            {
                dotProduct += inst1.Properties[prop] * inst2.Properties[prop];
            }

            if (inst1.MagnitudeNorm == 0 || inst2.MagnitudeNorm == 0)
                return 0.0;

            return dotProduct / (inst1.MagnitudeNorm * inst2.MagnitudeNorm);
        }

        private double CalculateEuclideanDistance(Instance inst1, Instance inst2)
        {
            double sumOfSquares = 0.0;
            var commonProps = inst1.Properties.Keys.Intersect(inst2.Properties.Keys);

            foreach (var prop in commonProps)
            {
                double diff = inst1.Properties[prop] - inst2.Properties[prop];
                sumOfSquares += diff * diff;
            }

            return Math.Sqrt(sumOfSquares);
        }

        private List<string> FindMatchedProperties(Instance inst1, Instance inst2)
        {
            var matched = new List<string>();
            var commonProps = inst1.Properties.Keys.Intersect(inst2.Properties.Keys);

            foreach (var prop in commonProps)
            {
                double diff = Math.Abs(inst1.Properties[prop] - inst2.Properties[prop]);
                if (diff < 0.5)
                {
                    matched.Add(prop);
                }
            }

            return matched;
        }

        private string GetSimilarityRating(double cosineSim)
        {
            return cosineSim switch
            {
                >= 0.9 => "Nearly Identical - Parallel instances",
                >= 0.75 => "Very Similar - Strong parallel pattern",
                >= 0.6 => "Similar - Moderate parallel correspondence",
                >= 0.4 => "Somewhat Similar - Weak parallel pattern",
                _ => "Dissimilar - No parallel correspondence"
            };
        }

        public void ClusterParallelInstances(string clusterId, List<string> instanceIds)
        {
            var cluster = new InstanceCluster
            {
                ClusterId = clusterId,
                InstanceIds = new List<string>(instanceIds),
                CategoryIds = new List<string>(),
                AverageSimilarity = 0.0,
                ParallelInstanceCount = instanceIds.Count,
                ClusterPattern = "",
                CreatedDate = DateTime.Now
            };

            var categorySet = new HashSet<string>();
            double totalSimilarity = 0.0;
            int similarityCount = 0;

            foreach (var instanceId in instanceIds)
            {
                if (instances.ContainsKey(instanceId))
                {
                    categorySet.Add(instances[instanceId].CategoryId);
                }
            }

            for (int i = 0; i < instanceIds.Count - 1; i++)
            {
                for (int j = i + 1; j < instanceIds.Count; j++)
                {
                    var scoreKey = $"Score-{instanceIds[i]}-{instanceIds[j]}";
                    if (scores.ContainsKey(scoreKey))
                    {
                        totalSimilarity += scores[scoreKey].CosineSimilarity;
                        similarityCount++;
                    }
                }
            }

            cluster.CategoryIds = categorySet.ToList();
            cluster.AverageSimilarity = similarityCount > 0 ? totalSimilarity / similarityCount : 0.0;
            cluster.ClusterPattern = $"Parallel instances across {categorySet.Count} categories";

            clusters[clusterId] = cluster;
        }

        public void DisplayInstance(string instanceId)
        {
            if (!instances.ContainsKey(instanceId)) return;

            var instance = instances[instanceId];
            Console.WriteLine($"\n  Instance: {instance.InstanceName}");
            Console.WriteLine($"  ID: {instance.InstanceId}");
            Console.WriteLine($"  Category: {instance.CategoryId}");
            Console.WriteLine($"  Properties: {instance.Properties.Count}");
            Console.WriteLine($"  Magnitude: {instance.MagnitudeNorm:F3}");
        }

        public void DisplayParallelMapping(string mappingId)
        {
            if (!mappings.ContainsKey(mappingId)) return;

            var mapping = mappings[mappingId];
            Console.WriteLine($"\n  Parallel Mapping: {mapping.MappingId}");
            Console.WriteLine($"  From: {mapping.SourceInstanceId} ({mapping.SourceCategoryId})");
            Console.WriteLine($"  To: {mapping.TargetInstanceId} ({mapping.TargetCategoryId})");
            Console.WriteLine($"  Similarity: {mapping.SimilarityScore * 100:F1}%");
            Console.WriteLine($"  Matched Properties: {mapping.MatchedProperties.Count}");
        }

        public void DisplayCluster(string clusterId)
        {
            if (!clusters.ContainsKey(clusterId)) return;

            var cluster = clusters[clusterId];
            Console.WriteLine($"\n  Instance Cluster: {cluster.ClusterId}");
            Console.WriteLine($"  Instances in Cluster: {cluster.ParallelInstanceCount}");
            Console.WriteLine($"  Categories Spanned: {cluster.CategoryIds.Count}");
            Console.WriteLine($"  Average Similarity: {cluster.AverageSimilarity * 100:F1}%");
            Console.WriteLine($"  Pattern: {cluster.ClusterPattern}");
        }

        public int GetTotalCategories()
        {
            return categories.Count;
        }

        public int GetTotalInstances()
        {
            return instances.Count;
        }

        public int GetTotalParallelMappings()
        {
            return mappings.Count;
        }

        public int GetTotalClusters()
        {
            return clusters.Count;
        }

        public List<(string, string, double)> GetHighestSimilarityPairs()
        {
            return scores.Values
                .Select(s => (s.InstancePair, s.SimilarityRating, s.CosineSimilarity))
                .OrderByDescending(x => x.Item3)
                .Take(5)
                .ToList();
        }

        public double GetAverageParallelSimilarity()
        {
            return scores.Count > 0 ? scores.Values.Average(s => s.CosineSimilarity) : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Recognizing Parallel Instances across Many Categories       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new ParallelInstanceEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Categories]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterCategory("CAT-001", "Nature", "Physical");
        engine.RegisterCategory("CAT-002", "Architecture", "Structural");
        engine.RegisterCategory("CAT-003", "Music", "Artistic");
        engine.RegisterCategory("CAT-004", "Science", "Academic");

        Console.WriteLine("  ✓ Registered 4 categories");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Instances across Categories]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateInstance("INST-001", "Mountain Range", "CAT-001",
            new Dictionary<string, double> { {"Height", 8.5}, {"Complexity", 8.0}, {"Symmetry", 6.0} });

        engine.CreateInstance("INST-002", "Cathedral", "CAT-002",
            new Dictionary<string, double> { {"Height", 8.0}, {"Complexity", 8.5}, {"Symmetry", 8.5} });

        engine.CreateInstance("INST-003", "Symphony", "CAT-003",
            new Dictionary<string, double> { {"Duration", 7.5}, {"Complexity", 8.0}, {"Harmony", 8.0} });

        engine.CreateInstance("INST-004", "Molecular Structure", "CAT-004",
            new Dictionary<string, double> { {"Layers", 8.0}, {"Complexity", 8.5}, {"Order", 7.5} });

        engine.CreateInstance("INST-005", "Waterfall", "CAT-001",
            new Dictionary<string, double> { {"Height", 8.0}, {"Complexity", 7.5}, {"Symmetry", 7.0} });

        engine.CreateInstance("INST-006", "Bridge", "CAT-002",
            new Dictionary<string, double> { {"Height", 7.0}, {"Complexity", 8.0}, {"Symmetry", 8.5} });

        engine.CreateInstance("INST-007", "Concerto", "CAT-003",
            new Dictionary<string, double> { {"Duration", 8.0}, {"Complexity", 8.5}, {"Harmony", 7.5} });

        engine.CreateInstance("INST-008", "Crystal Lattice", "CAT-004",
            new Dictionary<string, double> { {"Layers", 8.5}, {"Complexity", 8.0}, {"Order", 8.5} });

        Console.WriteLine("  ✓ Created 8 instances across 4 categories");
        engine.DisplayInstance("INST-001");
        engine.DisplayInstance("INST-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Finding Parallel Instances]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.FindParallelInstances("INST-001", "INST-002");
        engine.FindParallelInstances("INST-001", "INST-005");
        engine.FindParallelInstances("INST-002", "INST-006");
        engine.FindParallelInstances("INST-003", "INST-007");
        engine.FindParallelInstances("INST-004", "INST-008");
        engine.FindParallelInstances("INST-002", "INST-003");

        Console.WriteLine("  ✓ Found 6 parallel instance mappings");
        engine.DisplayParallelMapping("Parallel-INST-001-INST-002");
        engine.DisplayParallelMapping("Parallel-INST-003-INST-007");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Clustering Parallel Instances]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ClusterParallelInstances("CLUSTER-001", new List<string> { "INST-001", "INST-002", "INST-005" });
        engine.ClusterParallelInstances("CLUSTER-002", new List<string> { "INST-002", "INST-006" });
        engine.ClusterParallelInstances("CLUSTER-003", new List<string> { "INST-003", "INST-007" });

        Console.WriteLine("  ✓ Created 3 parallel instance clusters");
        engine.DisplayCluster("CLUSTER-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Similarity Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var topPairs = engine.GetHighestSimilarityPairs();
        Console.WriteLine("  Most Similar Parallel Instances:");
        int rank = 1;
        foreach (var (pair, rating, similarity) in topPairs)
        {
            Console.WriteLine($"    {rank}. {pair}: {similarity * 100:F1}% - {rating}");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Parallel Instance Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  System Overview:");
        Console.WriteLine($"    Total Categories: {engine.GetTotalCategories()}");
        Console.WriteLine($"    Total Instances: {engine.GetTotalInstances()}");
        Console.WriteLine($"    Parallel Mappings: {engine.GetTotalParallelMappings()}");
        Console.WriteLine($"    Instance Clusters: {engine.GetTotalClusters()}");
        Console.WriteLine($"    Average Parallel Similarity: {engine.GetAverageParallelSimilarity() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Parallel Instance Recognition Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Parallel Instance Architecture:");
        Console.WriteLine("    Layer 1: Category Definition (establish domain categories)");
        Console.WriteLine("    Layer 2: Instance Creation (define instances with properties)");
        Console.WriteLine("    Layer 3: Cross-Category Comparison (find similar instances)");
        Console.WriteLine("    Layer 4: Similarity Calculation (measure parallel correspondence)");
        Console.WriteLine("    Layer 5: Property Matching (identify common attributes)");
        Console.WriteLine("    Layer 6: Parallel Mapping (record category-spanning relationships)");
        Console.WriteLine("    Layer 7: Cluster Formation (group parallel instances)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register instances across multiple categories");
        Console.WriteLine("    ✓ Calculate cosine similarity between instances");
        Console.WriteLine("    ✓ Compute Euclidean distance metrics");
        Console.WriteLine("    ✓ Find matched properties across instances");
        Console.WriteLine("    ✓ Map parallel instances across category boundaries");
        Console.WriteLine("    ✓ Cluster similar instances from different categories");
        Console.WriteLine("    ✓ Analyze cross-category parallel patterns");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Parallel instance recognition system complete");
        Console.ResetColor();
    }
}
