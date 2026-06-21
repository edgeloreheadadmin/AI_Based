using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class LookingUpInformationAndWhereItIsAndOrWhereItCameFrom
{
    public class InformationSource
    {
        public string SourceId { get; set; }
        public string SourceName { get; set; }
        public string SourceType { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public double ReliabilityScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class InformationRecord
    {
        public string RecordId { get; set; }
        public string Content { get; set; }
        public string Topic { get; set; }
        public string SourceId { get; set; }
        public string LocationPath { get; set; }
        public DateTime OriginalDate { get; set; }
        public double AgeInDays { get; set; }
        public DateTime RetrievedDate { get; set; }
    }

    public class LocationMapping
    {
        public string MappingId { get; set; }
        public string LocationName { get; set; }
        public string LocationPath { get; set; }
        public string LocationType { get; set; }
        public Dictionary<string, List<string>> ContainedRecords { get; set; }
        public int TotalRecords { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LookupResult
    {
        public string ResultId { get; set; }
        public string Query { get; set; }
        public List<string> FoundRecordIds { get; set; }
        public List<string> SourceIds { get; set; }
        public List<string> Locations { get; set; }
        public double RelevanceScore { get; set; }
        public int ResultCount { get; set; }
        public DateTime LookedUpDate { get; set; }
    }

    public class LookupAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalRecords { get; set; }
        public int TotalSources { get; set; }
        public int TotalLocations { get; set; }
        public int TotalLookups { get; set; }
        public Dictionary<string, int> SourceDistribution { get; set; }
        public Dictionary<string, int> LocationDistribution { get; set; }
        public double AverageResultsPerLookup { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class InformationLookupEngine
    {
        private Dictionary<string, InformationSource> sources;
        private Dictionary<string, InformationRecord> records;
        private Dictionary<string, LocationMapping> locations;
        private Dictionary<string, LookupResult> results;
        private Dictionary<string, LookupAnalysis> analyses;

        public InformationLookupEngine()
        {
            sources = new Dictionary<string, InformationSource>();
            records = new Dictionary<string, InformationRecord>();
            locations = new Dictionary<string, LocationMapping>();
            results = new Dictionary<string, LookupResult>();
            analyses = new Dictionary<string, LookupAnalysis>();
        }

        public void RegisterSource(string sourceId, string sourceName, string sourceType,
                                  string location, string description, double reliability)
        {
            var source = new InformationSource
            {
                SourceId = sourceId,
                SourceName = sourceName,
                SourceType = sourceType,
                Location = location,
                Description = description,
                ReliabilityScore = reliability,
                CreatedDate = DateTime.Now
            };
            sources[sourceId] = source;
        }

        public void RegisterLocation(string locationId, string locationName, string locationPath, string locationType)
        {
            var location = new LocationMapping
            {
                MappingId = locationId,
                LocationName = locationName,
                LocationPath = locationPath,
                LocationType = locationType,
                ContainedRecords = new Dictionary<string, List<string>>(),
                TotalRecords = 0,
                CreatedDate = DateTime.Now
            };
            locations[locationId] = location;
        }

        public void StoreRecord(string recordId, string content, string topic, string sourceId, string locationPath)
        {
            if (!sources.ContainsKey(sourceId)) return;

            var record = new InformationRecord
            {
                RecordId = recordId,
                Content = content,
                Topic = topic,
                SourceId = sourceId,
                LocationPath = locationPath,
                OriginalDate = DateTime.Now.AddDays(-new Random().Next(0, 365)),
                AgeInDays = new Random().Next(0, 365),
                RetrievedDate = DateTime.Now
            };

            records[recordId] = record;

            var relevantLocations = locations.Values.Where(l => locationPath.Contains(l.LocationPath)).ToList();
            foreach (var loc in relevantLocations)
            {
                if (!loc.ContainedRecords.ContainsKey(topic))
                    loc.ContainedRecords[topic] = new List<string>();
                loc.ContainedRecords[topic].Add(recordId);
                loc.TotalRecords++;
            }
        }

        public void LookupInformation(string resultId, string query)
        {
            var result = new LookupResult
            {
                ResultId = resultId,
                Query = query,
                FoundRecordIds = new List<string>(),
                SourceIds = new List<string>(),
                Locations = new List<string>(),
                RelevanceScore = 0.0,
                ResultCount = 0,
                LookedUpDate = DateTime.Now
            };

            foreach (var record in records.Values)
            {
                if (record.Content.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                    record.Topic.Contains(query, StringComparison.OrdinalIgnoreCase))
                {
                    result.FoundRecordIds.Add(record.RecordId);

                    if (!result.SourceIds.Contains(record.SourceId))
                        result.SourceIds.Add(record.SourceId);

                    if (!result.Locations.Contains(record.LocationPath))
                        result.Locations.Add(record.LocationPath);

                    result.RelevanceScore += 1.0;
                }
            }

            if (result.FoundRecordIds.Count > 0)
            {
                result.RelevanceScore = Math.Min(result.RelevanceScore / result.FoundRecordIds.Count, 1.0);
            }

            result.ResultCount = result.FoundRecordIds.Count;

            results[resultId] = result;
        }

        public void AnalyzeLookupPatterns(string analysisId)
        {
            var analysis = new LookupAnalysis
            {
                AnalysisId = analysisId,
                TotalRecords = records.Count,
                TotalSources = sources.Count,
                TotalLocations = locations.Count,
                TotalLookups = results.Count,
                SourceDistribution = new Dictionary<string, int>(),
                LocationDistribution = new Dictionary<string, int>(),
                AverageResultsPerLookup = results.Count > 0 ? results.Values.Average(r => r.ResultCount) : 0.0,
                AnalyzedDate = DateTime.Now
            };

            foreach (var record in records.Values)
            {
                if (!analysis.SourceDistribution.ContainsKey(record.SourceId))
                    analysis.SourceDistribution[record.SourceId] = 0;
                analysis.SourceDistribution[record.SourceId]++;
            }

            foreach (var location in locations.Values)
            {
                if (location.TotalRecords > 0)
                {
                    if (!analysis.LocationDistribution.ContainsKey(location.LocationName))
                        analysis.LocationDistribution[location.LocationName] = 0;
                    analysis.LocationDistribution[location.LocationName] = location.TotalRecords;
                }
            }

            analyses[analysisId] = analysis;
        }

        public void DisplaySource(string sourceId)
        {
            if (!sources.ContainsKey(sourceId)) return;

            var source = sources[sourceId];
            Console.WriteLine($"\n  Information Source: {source.SourceName}");
            Console.WriteLine($"  Type: {source.SourceType}");
            Console.WriteLine($"  Location: {source.Location}");
            Console.WriteLine($"  Reliability: {source.ReliabilityScore * 100:F0}%");
        }

        public void DisplayLocation(string locationId)
        {
            if (!locations.ContainsKey(locationId)) return;

            var location = locations[locationId];
            Console.WriteLine($"\n  Location: {location.LocationName}");
            Console.WriteLine($"  Path: {location.LocationPath}");
            Console.WriteLine($"  Type: {location.LocationType}");
            Console.WriteLine($"  Records Stored: {location.TotalRecords}");
        }

        public void DisplayLookupResult(string resultId)
        {
            if (!results.ContainsKey(resultId)) return;

            var result = results[resultId];
            Console.WriteLine($"\n  Lookup Result: {result.ResultId}");
            Console.WriteLine($"  Query: {result.Query}");
            Console.WriteLine($"  Records Found: {result.ResultCount}");
            Console.WriteLine($"  Sources: {result.SourceIds.Count}");
            Console.WriteLine($"  Locations: {result.Locations.Count}");
            Console.WriteLine($"  Relevance: {result.RelevanceScore * 100:F0}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Lookup Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Records: {analysis.TotalRecords}");
            Console.WriteLine($"  Total Sources: {analysis.TotalSources}");
            Console.WriteLine($"  Total Locations: {analysis.TotalLocations}");
            Console.WriteLine($"  Total Lookups: {analysis.TotalLookups}");
            Console.WriteLine($"  Average Results per Lookup: {analysis.AverageResultsPerLookup:F1}");
        }

        public int GetTotalRecords()
        {
            return records.Count;
        }

        public int GetTotalSources()
        {
            return sources.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Looking Up Information - Where It Is and Where It Came From     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new InformationLookupEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Information Sources]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSource("SRC-001", "Technical Documentation", "Database", "/docs/technical",
            "Primary technical reference", 0.95);

        engine.RegisterSource("SRC-002", "User Manual", "Repository", "/docs/manuals",
            "User-facing documentation", 0.90);

        engine.RegisterSource("SRC-003", "API Reference", "Web Service", "https://api.example.com",
            "Live API documentation", 0.92);

        Console.WriteLine("  ✓ Registered 3 information sources");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Locations]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterLocation("LOC-001", "Technical Docs Root", "/docs/technical", "Directory");
        engine.RegisterLocation("LOC-002", "User Manuals Root", "/docs/manuals", "Directory");
        engine.RegisterLocation("LOC-003", "API Documentation", "https://api.example.com", "Web");

        Console.WriteLine("  ✓ Registered 3 storage locations");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Storing Information Records]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.StoreRecord("REC-001", "How to deploy the application", "Deployment", "SRC-001", "/docs/technical");
        engine.StoreRecord("REC-002", "API authentication endpoints", "Authentication", "SRC-003", "https://api.example.com");
        engine.StoreRecord("REC-003", "Getting started with the system", "Setup", "SRC-002", "/docs/manuals");
        engine.StoreRecord("REC-004", "Configuration parameters guide", "Configuration", "SRC-001", "/docs/technical");

        Console.WriteLine("  ✓ Stored 4 information records");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Displaying Sources and Locations]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplaySource("SRC-001");
        engine.DisplayLocation("LOC-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Looking Up Information]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LookupInformation("LOOKUP-001", "deployment");
        engine.LookupInformation("LOOKUP-002", "authentication");
        engine.LookupInformation("LOOKUP-003", "configuration");

        Console.WriteLine("  ✓ Executed 3 information lookups");
        engine.DisplayLookupResult("LOOKUP-001");
        engine.DisplayLookupResult("LOOKUP-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Lookup Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeLookupPatterns("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Information Lookup Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Information Lookup Architecture:");
        Console.WriteLine("    Layer 1: Source Registration (identify information sources)");
        Console.WriteLine("    Layer 2: Location Mapping (establish storage paths)");
        Console.WriteLine("    Layer 3: Record Storage (store information with provenance)");
        Console.WriteLine("    Layer 4: Location Tracking (maintain location references)");
        Console.WriteLine("    Layer 5: Query Processing (search for information)");
        Console.WriteLine("    Layer 6: Source Identification (track origin)");
        Console.WriteLine("    Layer 7: Pattern Analysis (analyze lookup behavior)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple information sources");
        Console.WriteLine("    ✓ Define storage locations");
        Console.WriteLine("    ✓ Store records with location tracking");
        Console.WriteLine("    ✓ Look up information by content");
        Console.WriteLine("    ✓ Identify source origin");
        Console.WriteLine("    ✓ Track storage locations");
        Console.WriteLine("    ✓ Analyze lookup patterns");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Information lookup system complete");
        Console.ResetColor();
    }
}
