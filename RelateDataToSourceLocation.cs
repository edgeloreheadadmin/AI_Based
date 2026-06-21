using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RelateDataToSourceLocation
{
    public class SourceLocation
    {
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GeographicRegion { get; set; }
        public int DataItemsOriginated { get; set; }
        public DateTime DiscoveredDate { get; set; }
    }

    public class DataItem
    {
        public string DataId { get; set; }
        public string Content { get; set; }
        public string SourceLocationId { get; set; }
        public double OriginationConfidence { get; set; }
        public List<string> RelatedSourceIds { get; set; }
        public string GeographicOrigin { get; set; }
        public DateTime CollectedDate { get; set; }
    }

    public class SourceLineage
    {
        public string LineageId { get; set; }
        public string DataId { get; set; }
        public List<string> LocationPath { get; set; }
        public List<double> ConfidenceScores { get; set; }
        public double OverallConfidence { get; set; }
        public string OriginationPoint { get; set; }
        public DateTime TraceDate { get; set; }
    }

    public class DataSourceEngine
    {
        private Dictionary<string, SourceLocation> locations;
        private Dictionary<string, DataItem> dataItems;
        private Dictionary<string, SourceLineage> lineages;
        private Dictionary<string, List<string>> locationDataMap;

        public DataSourceEngine()
        {
            locations = new Dictionary<string, SourceLocation>();
            dataItems = new Dictionary<string, DataItem>();
            lineages = new Dictionary<string, SourceLineage>();
            locationDataMap = new Dictionary<string, List<string>>();
        }

        public void RegisterSourceLocation(string locationId, string name, string type,
                                         double lat, double lon, string region)
        {
            var location = new SourceLocation
            {
                LocationId = locationId,
                LocationName = name,
                LocationType = type,
                Latitude = lat,
                Longitude = lon,
                GeographicRegion = region,
                DataItemsOriginated = 0,
                DiscoveredDate = DateTime.Now
            };
            locations[locationId] = location;
            locationDataMap[locationId] = new List<string>();
        }

        public void RecordDataItem(string dataId, string content, string sourceLocationId)
        {
            if (!locations.ContainsKey(sourceLocationId)) return;

            var dataItem = new DataItem
            {
                DataId = dataId,
                Content = content,
                SourceLocationId = sourceLocationId,
                OriginationConfidence = 0.85,
                RelatedSourceIds = new List<string>(),
                GeographicOrigin = locations[sourceLocationId].GeographicRegion,
                CollectedDate = DateTime.Now
            };

            dataItems[dataId] = dataItem;
            locationDataMap[sourceLocationId].Add(dataId);
            locations[sourceLocationId].DataItemsOriginated++;
        }

        public void LinkRelatedSources(string dataId, List<string> relatedSourceIds)
        {
            if (!dataItems.ContainsKey(dataId)) return;

            var dataItem = dataItems[dataId];
            foreach (var sourceId in relatedSourceIds)
            {
                if (!dataItem.RelatedSourceIds.Contains(sourceId))
                {
                    dataItem.RelatedSourceIds.Add(sourceId);
                }
            }
        }

        public void TraceDataLineage(string dataId)
        {
            if (!dataItems.ContainsKey(dataId)) return;

            var dataItem = dataItems[dataId];
            var lineage = new SourceLineage
            {
                LineageId = $"Lineage-{dataId}",
                DataId = dataId,
                LocationPath = new List<string>(),
                ConfidenceScores = new List<double>(),
                OverallConfidence = 0.0,
                OriginationPoint = dataItem.SourceLocationId,
                TraceDate = DateTime.Now
            };

            lineage.LocationPath.Add(dataItem.SourceLocationId);
            lineage.ConfidenceScores.Add(dataItem.OriginationConfidence);

            foreach (var relatedId in dataItem.RelatedSourceIds)
            {
                lineage.LocationPath.Add(relatedId);
                lineage.ConfidenceScores.Add(0.7);
            }

            lineage.OverallConfidence = lineage.ConfidenceScores.Count > 0 ?
                lineage.ConfidenceScores.Average() : 0.0;

            lineages[lineage.LineageId] = lineage;
        }

        public double CalculateGeographicDistance(string locationId1, string locationId2)
        {
            if (!locations.ContainsKey(locationId1) || !locations.ContainsKey(locationId2))
                return 0.0;

            var loc1 = locations[locationId1];
            var loc2 = locations[locationId2];

            double latDiff = loc2.Latitude - loc1.Latitude;
            double lonDiff = loc2.Longitude - loc1.Longitude;

            return Math.Sqrt(latDiff * latDiff + lonDiff * lonDiff);
        }

        public List<string> GetDataFromLocation(string locationId)
        {
            if (!locationDataMap.ContainsKey(locationId))
                return new List<string>();

            return locationDataMap[locationId];
        }

        public void DisplaySourceLocation(string locationId)
        {
            if (!locations.ContainsKey(locationId)) return;

            var location = locations[locationId];
            Console.WriteLine($"\n  Source Location: {location.LocationName}");
            Console.WriteLine($"  Location ID: {location.LocationId}");
            Console.WriteLine($"  Type: {location.LocationType}");
            Console.WriteLine($"  Coordinates: ({location.Latitude:F3}, {location.Longitude:F3})");
            Console.WriteLine($"  Region: {location.GeographicRegion}");
            Console.WriteLine($"  Data Items Originated: {location.DataItemsOriginated}");
        }

        public void DisplayDataItemOrigin(string dataId)
        {
            if (!dataItems.ContainsKey(dataId)) return;

            var dataItem = dataItems[dataId];
            Console.WriteLine($"\n  Data Item: {dataItem.DataId}");
            Console.WriteLine($"  Content: {dataItem.Content}");
            Console.WriteLine($"  Primary Source: {dataItem.SourceLocationId}");
            Console.WriteLine($"  Geographic Origin: {dataItem.GeographicOrigin}");
            Console.WriteLine($"  Origination Confidence: {dataItem.OriginationConfidence * 100:F1}%");
            if (dataItem.RelatedSourceIds.Count > 0)
            {
                Console.WriteLine($"  Related Sources: {string.Join(", ", dataItem.RelatedSourceIds)}");
            }
        }

        public void DisplayLineage(string lineageId)
        {
            if (!lineages.ContainsKey(lineageId)) return;

            var lineage = lineages[lineageId];
            Console.WriteLine($"\n  Lineage: {lineage.LineageId}");
            Console.WriteLine($"  Data ID: {lineage.DataId}");
            Console.WriteLine($"  Origination Point: {lineage.OriginationPoint}");
            Console.WriteLine($"  Location Path: {string.Join(" → ", lineage.LocationPath)}");
            Console.WriteLine($"  Confidence Scores:");
            for (int i = 0; i < lineage.ConfidenceScores.Count; i++)
            {
                Console.WriteLine($"    Step {i + 1}: {lineage.ConfidenceScores[i] * 100:F1}%");
            }
            Console.WriteLine($"  Overall Confidence: {lineage.OverallConfidence * 100:F1}%");
        }

        public Dictionary<string, int> GetLocationDataCounts()
        {
            return locationDataMap.OrderByDescending(x => x.Value.Count)
                .ToDictionary(x => x.Key, x => x.Value.Count);
        }

        public List<(string, double)> GetDataProvenanceScores()
        {
            var scores = new List<(string, double)>();
            foreach (var dataItem in dataItems.Values)
            {
                scores.Add((dataItem.DataId, dataItem.OriginationConfidence));
            }
            return scores.OrderByDescending(x => x.Item2).ToList();
        }

        public double GetAverageOriginationConfidence()
        {
            return dataItems.Count > 0 ? dataItems.Values.Average(d => d.OriginationConfidence) : 0.0;
        }

        public string GetSourceReliability(double confidence)
        {
            return confidence switch
            {
                >= 0.9 => "Extremely Reliable - High confidence in origin",
                >= 0.75 => "Reliable - Good source verification",
                >= 0.6 => "Moderate - Reasonable confidence",
                >= 0.4 => "Low - Limited verification",
                _ => "Unreliable - Questionable origin"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Relate Data to Original Source Location                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new DataSourceEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Source Locations]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSourceLocation("LOC-001", "Central Repository", "Database", 40.7128, -74.0060, "North America");
        engine.RegisterSourceLocation("LOC-002", "Field Station Alpha", "Research Site", 51.5074, -0.1278, "Europe");
        engine.RegisterSourceLocation("LOC-003", "Archive Center", "Physical Archive", 35.6762, 139.6503, "Asia");
        engine.RegisterSourceLocation("LOC-004", "Distributed Network", "Cloud Service", 37.7749, -122.4194, "North America");

        Console.WriteLine("  ✓ Registered 4 source locations with geographic coordinates");
        engine.DisplaySourceLocation("LOC-001");
        engine.DisplaySourceLocation("LOC-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Recording Data Items with Source Attribution]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordDataItem("DATA-001", "Temperature readings from field site", "LOC-002");
        engine.RecordDataItem("DATA-002", "Historical climate records", "LOC-003");
        engine.RecordDataItem("DATA-003", "Real-time sensor data", "LOC-004");
        engine.RecordDataItem("DATA-004", "Compiled analysis dataset", "LOC-001");
        engine.RecordDataItem("DATA-005", "Supplementary measurements", "LOC-002");

        Console.WriteLine("  ✓ Recorded 5 data items with source location attribution");
        engine.DisplayDataItemOrigin("DATA-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Linking Related Sources]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkRelatedSources("DATA-001", new List<string> { "LOC-001", "LOC-004" });
        engine.LinkRelatedSources("DATA-002", new List<string> { "LOC-001", "LOC-003" });
        engine.LinkRelatedSources("DATA-004", new List<string> { "LOC-002", "LOC-003", "LOC-004" });

        Console.WriteLine("  ✓ Linked data items to related source locations");
        engine.DisplayDataItemOrigin("DATA-004");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Tracing Data Lineage]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.TraceDataLineage("DATA-001");
        engine.TraceDataLineage("DATA-002");
        engine.TraceDataLineage("DATA-004");

        Console.WriteLine("  ✓ Traced lineage for 3 data items");
        engine.DisplayLineage("Lineage-DATA-001");
        engine.DisplayLineage("Lineage-DATA-004");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Geographic Relationship Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Geographic distances between source locations:");
        double dist_001_002 = engine.CalculateGeographicDistance("LOC-001", "LOC-002");
        double dist_001_003 = engine.CalculateGeographicDistance("LOC-001", "LOC-003");
        double dist_002_003 = engine.CalculateGeographicDistance("LOC-002", "LOC-003");

        Console.WriteLine($"    LOC-001 to LOC-002: {dist_001_002:F2} units");
        Console.WriteLine($"    LOC-001 to LOC-003: {dist_001_003:F2} units");
        Console.WriteLine($"    LOC-002 to LOC-003: {dist_002_003:F2} units");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Source Attribution Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var dataCounts = engine.GetLocationDataCounts();
        Console.WriteLine("  Data Items by Source Location:");
        foreach (var kvp in dataCounts)
        {
            Console.WriteLine($"    {kvp.Key}: {kvp.Value} items");
        }

        var provenanceScores = engine.GetDataProvenanceScores();
        Console.WriteLine("\n  Data Provenance Scores:");
        foreach (var (dataId, score) in provenanceScores.Take(5))
        {
            Console.WriteLine($"    {dataId}: {score * 100:F1}%");
        }

        double avgConfidence = engine.GetAverageOriginationConfidence();
        Console.WriteLine($"\n  Average Origination Confidence: {avgConfidence * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Source Reliability Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Data Source Relationship Model:");
        Console.WriteLine("    Layer 1: Source Location Registry (geographic points)");
        Console.WriteLine("    Layer 2: Data Item Attribution (source assignment)");
        Console.WriteLine("    Layer 3: Related Sources (multi-location links)");
        Console.WriteLine("    Layer 4: Lineage Tracing (path from origin)");
        Console.WriteLine("    Layer 5: Confidence Scoring (reliability assessment)");
        Console.WriteLine("    Layer 6: Geographic Analysis (location relationships)");
        Console.WriteLine("    Layer 7: Provenance Verification (source validation)");
        Console.WriteLine("\n  Key Mechanisms:");
        Console.WriteLine("    ✓ Track original source location for each data item");
        Console.WriteLine("    ✓ Link data to multiple related sources");
        Console.WriteLine("    ✓ Maintain complete lineage from origination point");
        Console.WriteLine("    ✓ Calculate confidence in source attribution");
        Console.WriteLine("    ✓ Analyze geographic relationships between sources");
        Console.WriteLine("    ✓ Verify data provenance and reliability");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Data source location tracking system complete");
        Console.ResetColor();
    }
}
