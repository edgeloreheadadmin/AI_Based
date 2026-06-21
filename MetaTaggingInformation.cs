using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class MetaTaggingInformation
{
    public class MetaTag
    {
        public string TagId { get; set; }
        public string TagName { get; set; }
        public string TagCategory { get; set; }
        public string Description { get; set; }
        public double Relevance { get; set; }
        public int UsageCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TaggedContent
    {
        public string ContentId { get; set; }
        public string ContentDescription { get; set; }
        public List<string> AssignedTags { get; set; }
        public Dictionary<string, double> TagConfidence { get; set; }
        public string PrimaryTag { get; set; }
        public double OverallTagQuality { get; set; }
        public DateTime TaggedDate { get; set; }
    }

    public class TagHierarchy
    {
        public string HierarchyId { get; set; }
        public string RootTag { get; set; }
        public Dictionary<string, List<string>> TagTree { get; set; }
        public int TotalTagsInHierarchy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TaggingAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalContentTagged { get; set; }
        public int TotalUniqueTagsUsed { get; set; }
        public Dictionary<string, int> TagCategoryDistribution { get; set; }
        public double AverageTagsPerContent { get; set; }
        public double AverageConfidence { get; set; }
        public List<string> MostFrequentTags { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class MetaTaggingEngine
    {
        private Dictionary<string, MetaTag> tags;
        private Dictionary<string, TaggedContent> taggedContent;
        private Dictionary<string, TagHierarchy> hierarchies;
        private Dictionary<string, TaggingAnalysis> analyses;

        public MetaTaggingEngine()
        {
            tags = new Dictionary<string, MetaTag>();
            taggedContent = new Dictionary<string, TaggedContent>();
            hierarchies = new Dictionary<string, TagHierarchy>();
            analyses = new Dictionary<string, TaggingAnalysis>();
        }

        public void RegisterMetaTag(string tagId, string tagName, string category, string description, double relevance)
        {
            var tag = new MetaTag
            {
                TagId = tagId,
                TagName = tagName,
                TagCategory = category,
                Description = description,
                Relevance = relevance,
                UsageCount = 0,
                CreatedDate = DateTime.Now
            };
            tags[tagId] = tag;
        }

        public void CreateTagHierarchy(string hierarchyId, string rootTag, Dictionary<string, List<string>> tagTree)
        {
            var hierarchy = new TagHierarchy
            {
                HierarchyId = hierarchyId,
                RootTag = rootTag,
                TagTree = new Dictionary<string, List<string>>(tagTree),
                TotalTagsInHierarchy = CalculateTotalTags(tagTree),
                CreatedDate = DateTime.Now
            };
            hierarchies[hierarchyId] = hierarchy;
        }

        private int CalculateTotalTags(Dictionary<string, List<string>> tagTree)
        {
            int total = 1;
            foreach (var subTags in tagTree.Values)
            {
                total += subTags.Count;
            }
            return total;
        }

        public void TagContent(string contentId, string contentDesc, List<string> tagIds)
        {
            var taggedItem = new TaggedContent
            {
                ContentId = contentId,
                ContentDescription = contentDesc,
                AssignedTags = new List<string>(tagIds),
                TagConfidence = new Dictionary<string, double>(),
                PrimaryTag = "",
                OverallTagQuality = 0.0,
                TaggedDate = DateTime.Now
            };

            double totalConfidence = 0.0;
            double highestConfidence = 0.0;

            foreach (var tagId in tagIds)
            {
                if (tags.ContainsKey(tagId))
                {
                    double confidence = CalculateTagConfidence(tagId, contentDesc);
                    taggedItem.TagConfidence[tagId] = confidence;
                    totalConfidence += confidence;
                    tags[tagId].UsageCount++;

                    if (confidence > highestConfidence)
                    {
                        highestConfidence = confidence;
                        taggedItem.PrimaryTag = tagId;
                    }
                }
            }

            taggedItem.OverallTagQuality = tagIds.Count > 0 ? totalConfidence / tagIds.Count : 0.0;

            taggedContent[contentId] = taggedItem;
        }

        private double CalculateTagConfidence(string tagId, string contentDesc)
        {
            if (!tags.ContainsKey(tagId)) return 0.0;

            var tag = tags[tagId];
            double confidence = tag.Relevance * 0.7;

            if (contentDesc.Contains(tag.TagName, StringComparison.OrdinalIgnoreCase))
                confidence += 0.2;

            if (contentDesc.Length > 50)
                confidence += 0.1;

            return Math.Min(confidence, 1.0);
        }

        public void AnalyzeTaggingPatterns(string analysisId)
        {
            var analysis = new TaggingAnalysis
            {
                AnalysisId = analysisId,
                TotalContentTagged = taggedContent.Count,
                TotalUniqueTagsUsed = tags.Count,
                TagCategoryDistribution = new Dictionary<string, int>(),
                AverageTagsPerContent = CalculateAverageTagsPerContent(),
                AverageConfidence = CalculateAverageConfidence(),
                MostFrequentTags = new List<string>(),
                AnalyzedDate = DateTime.Now
            };

            foreach (var tag in tags.Values)
            {
                if (!analysis.TagCategoryDistribution.ContainsKey(tag.TagCategory))
                    analysis.TagCategoryDistribution[tag.TagCategory] = 0;
                analysis.TagCategoryDistribution[tag.TagCategory]++;
            }

            analysis.MostFrequentTags = tags.Values
                .OrderByDescending(t => t.UsageCount)
                .Take(5)
                .Select(t => t.TagName)
                .ToList();

            analyses[analysisId] = analysis;
        }

        private double CalculateAverageTagsPerContent()
        {
            if (taggedContent.Count == 0) return 0.0;
            return taggedContent.Values.Average(c => c.AssignedTags.Count);
        }

        private double CalculateAverageConfidence()
        {
            if (taggedContent.Count == 0) return 0.0;
            return taggedContent.Values.Average(c => c.OverallTagQuality);
        }

        public void DisplayTag(string tagId)
        {
            if (!tags.ContainsKey(tagId)) return;

            var tag = tags[tagId];
            Console.WriteLine($"\n  Meta Tag: {tag.TagName}");
            Console.WriteLine($"  Category: {tag.TagCategory}");
            Console.WriteLine($"  Relevance: {tag.Relevance * 100:F0}%");
            Console.WriteLine($"  Usage Count: {tag.UsageCount}");
            Console.WriteLine($"  Description: {tag.Description}");
        }

        public void DisplayTaggedContent(string contentId)
        {
            if (!taggedContent.ContainsKey(contentId)) return;

            var content = taggedContent[contentId];
            Console.WriteLine($"\n  Tagged Content: {content.ContentId}");
            Console.WriteLine($"  Description: {content.ContentDescription}");
            Console.WriteLine($"  Tags Assigned: {content.AssignedTags.Count}");
            Console.WriteLine($"  Primary Tag: {content.PrimaryTag}");
            Console.WriteLine($"  Overall Quality: {content.OverallTagQuality * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Tagging Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Content Tagged: {analysis.TotalContentTagged}");
            Console.WriteLine($"  Unique Tags Used: {analysis.TotalUniqueTagsUsed}");
            Console.WriteLine($"  Average Tags per Content: {analysis.AverageTagsPerContent:F1}");
            Console.WriteLine($"  Average Confidence: {analysis.AverageConfidence * 100:F1}%");
            Console.WriteLine($"  Top Tags: {string.Join(", ", analysis.MostFrequentTags.Take(3))}");
        }

        public int GetTotalTags()
        {
            return tags.Count;
        }

        public int GetTotalTaggedContent()
        {
            return taggedContent.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Meta Tagging Information                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new MetaTaggingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Meta Tags]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterMetaTag("TAG-001", "Technical", "Domain", "Technical content", 0.95);
        engine.RegisterMetaTag("TAG-002", "Business", "Domain", "Business content", 0.90);
        engine.RegisterMetaTag("TAG-003", "Urgent", "Priority", "Time-sensitive", 0.85);
        engine.RegisterMetaTag("TAG-004", "Reviewed", "Status", "Quality assured", 0.80);
        engine.RegisterMetaTag("TAG-005", "API", "Technology", "API-related", 0.92);

        Console.WriteLine("  ✓ Registered 5 meta tags");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Tag Hierarchies]");
        Console.ResetColor();
        Thread.Sleep(500);

        var techHierarchy = new Dictionary<string, List<string>>
        {
            { "TAG-001", new List<string> { "TAG-005", "Database", "Security" } }
        };

        engine.CreateTagHierarchy("HIERARCHY-001", "TAG-001", techHierarchy);

        Console.WriteLine("  ✓ Created 1 tag hierarchy");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Displaying Tags]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayTag("TAG-001");
        engine.DisplayTag("TAG-005");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Tagging Content]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.TagContent("CONTENT-001", "API documentation for REST endpoints",
            new List<string> { "TAG-001", "TAG-005", "TAG-004" });

        engine.TagContent("CONTENT-002", "Quarterly business strategy review",
            new List<string> { "TAG-002", "TAG-003", "TAG-004" });

        engine.TagContent("CONTENT-003", "Database optimization techniques",
            new List<string> { "TAG-001", "TAG-005" });

        Console.WriteLine("  ✓ Tagged 3 content items");
        engine.DisplayTaggedContent("CONTENT-001");
        engine.DisplayTaggedContent("CONTENT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Tagging Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeTaggingPatterns("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Meta Tagging Summary:");
        Console.WriteLine($"    Total Tags: {engine.GetTotalTags()}");
        Console.WriteLine($"    Tagged Content Items: {engine.GetTotalTaggedContent()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Meta Tagging Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Meta Tagging Architecture:");
        Console.WriteLine("    Layer 1: Tag Registration (define meta tags)");
        Console.WriteLine("    Layer 2: Category Assignment (organize tags)");
        Console.WriteLine("    Layer 3: Relevance Scoring (measure tag importance)");
        Console.WriteLine("    Layer 4: Hierarchy Creation (build tag relationships)");
        Console.WriteLine("    Layer 5: Content Tagging (assign tags to content)");
        Console.WriteLine("    Layer 6: Confidence Calculation (measure tag accuracy)");
        Console.WriteLine("    Layer 7: Pattern Analysis (track tagging trends)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register and organize meta tags");
        Console.WriteLine("    ✓ Create hierarchical tag structures");
        Console.WriteLine("    ✓ Assign multiple tags to content");
        Console.WriteLine("    ✓ Calculate tag confidence scores");
        Console.WriteLine("    ✓ Track tag usage frequency");
        Console.WriteLine("    ✓ Analyze tagging patterns");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Meta tagging system complete");
        Console.ResetColor();
    }
}
