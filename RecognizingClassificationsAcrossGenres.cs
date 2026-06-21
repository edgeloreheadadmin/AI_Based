using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingClassificationsAcrossGenres
{
    public class GenreDefinition
    {
        public string GenreId { get; set; }
        public string GenreName { get; set; }
        public List<string> MetaClassificationIds { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MetaClassification
    {
        public string ClassificationId { get; set; }
        public string ClassificationName { get; set; }
        public string ClassificationType { get; set; }
        public List<string> GenreIds { get; set; }
        public double UniversalApplicability { get; set; }
        public int GenresSpanned { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GenreMapping
    {
        public string MappingId { get; set; }
        public string GenreId { get; set; }
        public string MetaClassificationId { get; set; }
        public double ApplicabilityScore { get; set; }
        public string InterpretationVariant { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassificationBridge
    {
        public string BridgeId { get; set; }
        public string MetaClassificationId { get; set; }
        public List<(string, string)> GenreInterpretationPairs { get; set; }
        public double CrossGenreConsistency { get; set; }
        public int TotalGenresLinked { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CrossGenreAnalysis
    {
        public string AnalysisId { get; set; }
        public List<string> GenresAnalyzed { get; set; }
        public List<string> SharedClassifications { get; set; }
        public Dictionary<string, double> ClassificationFrequency { get; set; }
        public double UniversalityIndex { get; set; }
        public List<string> UniqueToGenres { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class CrossGenreClassificationEngine
    {
        private Dictionary<string, GenreDefinition> genres;
        private Dictionary<string, MetaClassification> metaClassifications;
        private Dictionary<string, GenreMapping> genreMappings;
        private Dictionary<string, ClassificationBridge> bridges;
        private Dictionary<string, CrossGenreAnalysis> analyses;

        public CrossGenreClassificationEngine()
        {
            genres = new Dictionary<string, GenreDefinition>();
            metaClassifications = new Dictionary<string, MetaClassification>();
            genreMappings = new Dictionary<string, GenreMapping>();
            bridges = new Dictionary<string, ClassificationBridge>();
            analyses = new Dictionary<string, CrossGenreAnalysis>();
        }

        public void RegisterGenre(string genreId, string genreName)
        {
            var genre = new GenreDefinition
            {
                GenreId = genreId,
                GenreName = genreName,
                MetaClassificationIds = new List<string>(),
                CreatedDate = DateTime.Now
            };
            genres[genreId] = genre;
        }

        public void CreateMetaClassification(string classId, string className, string classType)
        {
            var metaClass = new MetaClassification
            {
                ClassificationId = classId,
                ClassificationName = className,
                ClassificationType = classType,
                GenreIds = new List<string>(),
                UniversalApplicability = 0.0,
                GenresSpanned = 0,
                CreatedDate = DateTime.Now
            };
            metaClassifications[classId] = metaClass;
        }

        public void MapMetaClassificationToGenre(string genreId, string classificationId,
                                                 double applicability, string variant)
        {
            if (!genres.ContainsKey(genreId) || !metaClassifications.ContainsKey(classificationId))
                return;

            string mappingId = $"Mapping-{genreId}-{classificationId}";
            var mapping = new GenreMapping
            {
                MappingId = mappingId,
                GenreId = genreId,
                MetaClassificationId = classificationId,
                ApplicabilityScore = applicability,
                InterpretationVariant = variant,
                CreatedDate = DateTime.Now
            };

            genreMappings[mappingId] = mapping;

            var genre = genres[genreId];
            if (!genre.MetaClassificationIds.Contains(classificationId))
            {
                genre.MetaClassificationIds.Add(classificationId);
            }

            var metaClass = metaClassifications[classificationId];
            if (!metaClass.GenreIds.Contains(genreId))
            {
                metaClass.GenreIds.Add(genreId);
            }
        }

        public void CreateClassificationBridge(string bridgeId, string metaClassificationId)
        {
            if (!metaClassifications.ContainsKey(metaClassificationId)) return;

            var metaClass = metaClassifications[metaClassificationId];
            var bridge = new ClassificationBridge
            {
                BridgeId = bridgeId,
                MetaClassificationId = metaClassificationId,
                GenreInterpretationPairs = new List<(string, string)>(),
                CrossGenreConsistency = 0.0,
                TotalGenresLinked = 0,
                CreatedDate = DateTime.Now
            };

            double totalApplicability = 0.0;
            int genreCount = 0;

            foreach (var genreId in metaClass.GenreIds)
            {
                var mapping = genreMappings.Values
                    .FirstOrDefault(m => m.GenreId == genreId && m.MetaClassificationId == metaClassificationId);

                if (mapping != null && genres.ContainsKey(genreId))
                {
                    bridge.GenreInterpretationPairs.Add((genres[genreId].GenreName, mapping.InterpretationVariant));
                    totalApplicability += mapping.ApplicabilityScore;
                    genreCount++;
                }
            }

            bridge.TotalGenresLinked = genreCount;
            bridge.CrossGenreConsistency = genreCount > 0 ? totalApplicability / genreCount : 0.0;

            bridges[bridgeId] = bridge;
            metaClass.GenresSpanned = genreCount;
            metaClass.UniversalApplicability = bridge.CrossGenreConsistency;
        }

        public void AnalyzeCrossGenreClassifications(string analysisId, List<string> genreIds)
        {
            var analysis = new CrossGenreAnalysis
            {
                AnalysisId = analysisId,
                GenresAnalyzed = new List<string>(genreIds),
                SharedClassifications = new List<string>(),
                ClassificationFrequency = new Dictionary<string, double>(),
                UniversalityIndex = 0.0,
                UniqueToGenres = new List<string>(),
                AnalyzedDate = DateTime.Now
            };

            var classificationCounts = new Dictionary<string, int>();

            foreach (var genreId in genreIds)
            {
                if (genres.ContainsKey(genreId))
                {
                    var genre = genres[genreId];
                    foreach (var classId in genre.MetaClassificationIds)
                    {
                        if (!classificationCounts.ContainsKey(classId))
                        {
                            classificationCounts[classId] = 0;
                        }
                        classificationCounts[classId]++;
                    }
                }
            }

            foreach (var kvp in classificationCounts)
            {
                double frequency = (double)kvp.Value / genreIds.Count;
                analysis.ClassificationFrequency[kvp.Key] = frequency;

                if (kvp.Value == genreIds.Count)
                {
                    analysis.SharedClassifications.Add(kvp.Key);
                }
                else if (kvp.Value == 1)
                {
                    analysis.UniqueToGenres.Add(kvp.Key);
                }
            }

            analysis.UniversalityIndex = classificationCounts.Count > 0 ?
                (double)analysis.SharedClassifications.Count / classificationCounts.Count : 0.0;

            analyses[analysisId] = analysis;
        }

        public void DisplayGenre(string genreId)
        {
            if (!genres.ContainsKey(genreId)) return;

            var genre = genres[genreId];
            Console.WriteLine($"\n  Genre: {genre.GenreName}");
            Console.WriteLine($"  ID: {genre.GenreId}");
            Console.WriteLine($"  Meta-Classifications: {genre.MetaClassificationIds.Count}");
            if (genre.MetaClassificationIds.Count > 0)
            {
                Console.WriteLine($"  Classifications: {string.Join(", ", genre.MetaClassificationIds.Take(3))}");
            }
        }

        public void DisplayMetaClassification(string classificationId)
        {
            if (!metaClassifications.ContainsKey(classificationId)) return;

            var metaClass = metaClassifications[classificationId];
            Console.WriteLine($"\n  Meta-Classification: {metaClass.ClassificationName}");
            Console.WriteLine($"  ID: {metaClass.ClassificationId}");
            Console.WriteLine($"  Type: {metaClass.ClassificationType}");
            Console.WriteLine($"  Genres Spanned: {metaClass.GenresSpanned}");
            Console.WriteLine($"  Universal Applicability: {metaClass.UniversalApplicability * 100:F1}%");
        }

        public void DisplayClassificationBridge(string bridgeId)
        {
            if (!bridges.ContainsKey(bridgeId)) return;

            var bridge = bridges[bridgeId];
            Console.WriteLine($"\n  Classification Bridge: {bridge.BridgeId}");
            Console.WriteLine($"  Genres Linked: {bridge.TotalGenresLinked}");
            Console.WriteLine($"  Cross-Genre Consistency: {bridge.CrossGenreConsistency * 100:F1}%");
            Console.WriteLine($"  Genre Interpretations:");
            foreach (var (genreName, variant) in bridge.GenreInterpretationPairs)
            {
                Console.WriteLine($"    {genreName}: {variant}");
            }
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Cross-Genre Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Genres Analyzed: {analysis.GenresAnalyzed.Count}");
            Console.WriteLine($"  Shared Classifications: {analysis.SharedClassifications.Count}");
            Console.WriteLine($"  Universality Index: {analysis.UniversalityIndex * 100:F1}%");
            if (analysis.UniqueToGenres.Count > 0)
            {
                Console.WriteLine($"  Unique Classifications: {analysis.UniqueToGenres.Count}");
            }
        }

        public int GetTotalGenres()
        {
            return genres.Count;
        }

        public int GetTotalMetaClassifications()
        {
            return metaClassifications.Count;
        }

        public int GetTotalMappings()
        {
            return genreMappings.Count;
        }

        public List<string> GetUniversalClassifications()
        {
            return metaClassifications.Values
                .Where(m => m.GenresSpanned == genres.Count && genres.Count > 0)
                .Select(m => m.ClassificationId)
                .ToList();
        }

        public List<(string, int)> GetClassificationSpreadByGenre()
        {
            return genres.Values
                .Select(g => (g.GenreName, g.MetaClassificationIds.Count))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public double GetAverageGenreApplicability()
        {
            return genreMappings.Count > 0 ? genreMappings.Values.Average(m => m.ApplicabilityScore) : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Recognizing Classifications across Genres                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CrossGenreClassificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Genres]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterGenre("G-001", "Science Fiction");
        engine.RegisterGenre("G-002", "Fantasy");
        engine.RegisterGenre("G-003", "Mystery");
        engine.RegisterGenre("G-004", "Romance");

        Console.WriteLine("  ✓ Registered 4 genre definitions");
        engine.DisplayGenre("G-001");
        engine.DisplayGenre("G-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Meta-Classifications]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateMetaClassification("META-001", "Character Development", "Universal");
        engine.CreateMetaClassification("META-002", "Conflict Resolution", "Universal");
        engine.CreateMetaClassification("META-003", "World Building", "Universal");
        engine.CreateMetaClassification("META-004", "Emotional Depth", "Universal");
        engine.CreateMetaClassification("META-005", "Tension and Suspense", "Universal");

        Console.WriteLine("  ✓ Created 5 meta-classifications");
        engine.DisplayMetaClassification("META-001");
        engine.DisplayMetaClassification("META-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Mapping Classifications to Genres]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.MapMetaClassificationToGenre("G-001", "META-001", 0.95, "Tech-enhanced protagonists");
        engine.MapMetaClassificationToGenre("G-002", "META-001", 0.90, "Magical character evolution");
        engine.MapMetaClassificationToGenre("G-003", "META-001", 0.85, "Detective growth and change");
        engine.MapMetaClassificationToGenre("G-004", "META-001", 0.92, "Emotional protagonist journey");

        engine.MapMetaClassificationToGenre("G-001", "META-002", 0.88, "Technological solutions");
        engine.MapMetaClassificationToGenre("G-002", "META-002", 0.92, "Magical battles");
        engine.MapMetaClassificationToGenre("G-003", "META-002", 0.90, "Case closure");
        engine.MapMetaClassificationToGenre("G-004", "META-002", 0.88, "Relationship commitment");

        engine.MapMetaClassificationToGenre("G-001", "META-003", 0.95, "Future societies");
        engine.MapMetaClassificationToGenre("G-002", "META-003", 0.98, "Fantasy realms");
        engine.MapMetaClassificationToGenre("G-003", "META-003", 0.65, "Minimal world-building");
        engine.MapMetaClassificationToGenre("G-004", "META-003", 0.60, "Contemporary settings");

        engine.MapMetaClassificationToGenre("G-001", "META-004", 0.75, "Philosophical themes");
        engine.MapMetaClassificationToGenre("G-002", "META-004", 0.80, "Heroic emotions");
        engine.MapMetaClassificationToGenre("G-003", "META-004", 0.88, "Psychological depth");
        engine.MapMetaClassificationToGenre("G-004", "META-004", 0.98, "Deep feelings");

        engine.MapMetaClassificationToGenre("G-001", "META-005", 0.80, "Space-based threats");
        engine.MapMetaClassificationToGenre("G-002", "META-005", 0.85, "Dark magic menace");
        engine.MapMetaClassificationToGenre("G-003", "META-005", 0.98, "Mystery clues");
        engine.MapMetaClassificationToGenre("G-004", "META-005", 0.70, "Romantic uncertainty");

        Console.WriteLine("  ✓ Created 20 classification-to-genre mappings");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Building Classification Bridges]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 1; i <= 5; i++)
        {
            engine.CreateClassificationBridge($"Bridge-{i}", $"META-{i:D3}");
        }

        Console.WriteLine("  ✓ Created 5 classification bridges");
        engine.DisplayClassificationBridge("Bridge-1");
        engine.DisplayClassificationBridge("Bridge-3");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Cross-Genre Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeCrossGenreClassifications("ANALYSIS-001", new List<string> { "G-001", "G-002", "G-003", "G-004" });

        Console.WriteLine("  ✓ Analyzed cross-genre classification patterns");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Cross-Genre Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        var classificationSpread = engine.GetClassificationSpreadByGenre();
        Console.WriteLine("  Classifications per Genre:");
        foreach (var (genreName, count) in classificationSpread)
        {
            Console.WriteLine($"    {genreName}: {count} meta-classifications");
        }

        Console.WriteLine($"\n  System Statistics:");
        Console.WriteLine($"    Total Genres: {engine.GetTotalGenres()}");
        Console.WriteLine($"    Total Meta-Classifications: {engine.GetTotalMetaClassifications()}");
        Console.WriteLine($"    Total Mappings: {engine.GetTotalMappings()}");
        Console.WriteLine($"    Average Applicability: {engine.GetAverageGenreApplicability() * 100:F1}%");

        var universal = engine.GetUniversalClassifications();
        Console.WriteLine($"    Universal Classifications (all genres): {universal.Count}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Cross-Genre Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Cross-Genre Analysis Architecture:");
        Console.WriteLine("    Layer 1: Genre Definition (establish genre identities)");
        Console.WriteLine("    Layer 2: Meta-Classification Creation (define universal concepts)");
        Console.WriteLine("    Layer 3: Genre Mapping (link concepts to genres with variants)");
        Console.WriteLine("    Layer 4: Applicability Scoring (measure concept relevance)");
        Console.WriteLine("    Layer 5: Bridge Formation (connect genres through shared concepts)");
        Console.WriteLine("    Layer 6: Pattern Analysis (identify universal vs. unique classifications)");
        Console.WriteLine("    Layer 7: Universality Assessment (measure cross-genre spread)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define universal meta-classifications spanning multiple genres");
        Console.WriteLine("    ✓ Map same classification to different genres with unique variants");
        Console.WriteLine("    ✓ Calculate applicability scores for each genre interpretation");
        Console.WriteLine("    ✓ Identify truly universal classifications present in all genres");
        Console.WriteLine("    ✓ Find genre-specific classification variations");
        Console.WriteLine("    ✓ Analyze cross-genre consistency and patterns");
        Console.WriteLine("    ✓ Measure universality index across genres");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Cross-genre classification system complete");
        Console.ResetColor();
    }
}
