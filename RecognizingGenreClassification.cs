using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingGenreClassification
{
    public class GenreProfile
    {
        public string GenreId { get; set; }
        public string GenreName { get; set; }
        public List<string> CharacteristicIds { get; set; }
        public Dictionary<string, double> CharacteristicWeights { get; set; }
        public int ItemsClassified { get; set; }
        public double AverageConfidence { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class GenreCharacteristic
    {
        public string CharacteristicId { get; set; }
        public string CharacteristicName { get; set; }
        public string CharacteristicType { get; set; }
        public double TypicalValue { get; set; }
        public double ValueRange { get; set; }
        public List<string> AssociatedGenres { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ContentItem
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public Dictionary<string, double> CharacteristicValues { get; set; }
        public string ClassifiedGenre { get; set; }
        public double ClassificationConfidence { get; set; }
        public List<(string, double)> GenreAffinities { get; set; }
        public DateTime ClassifiedDate { get; set; }
    }

    public class ClassificationScore
    {
        public string ScoreId { get; set; }
        public string ItemId { get; set; }
        public string GenreId { get; set; }
        public double MatchScore { get; set; }
        public List<double> CharacteristicMatches { get; set; }
        public double AverageMatch { get; set; }
        public string MatchQuality { get; set; }
        public DateTime ScoredDate { get; set; }
    }

    public class GenreClassificationEngine
    {
        private Dictionary<string, GenreProfile> genres;
        private Dictionary<string, GenreCharacteristic> characteristics;
        private Dictionary<string, ContentItem> items;
        private Dictionary<string, ClassificationScore> scores;
        private List<(string, string, double)> classificationLog;

        public GenreClassificationEngine()
        {
            genres = new Dictionary<string, GenreProfile>();
            characteristics = new Dictionary<string, GenreCharacteristic>();
            items = new Dictionary<string, ContentItem>();
            scores = new Dictionary<string, ClassificationScore>();
            classificationLog = new List<(string, string, double)>();
        }

        public void RegisterGenre(string genreId, string genreName)
        {
            var genre = new GenreProfile
            {
                GenreId = genreId,
                GenreName = genreName,
                CharacteristicIds = new List<string>(),
                CharacteristicWeights = new Dictionary<string, double>(),
                ItemsClassified = 0,
                AverageConfidence = 0.0,
                CreatedDate = DateTime.Now
            };
            genres[genreId] = genre;
        }

        public void RegisterCharacteristic(string charId, string charName, string charType,
                                          double typicalValue, double valueRange)
        {
            var characteristic = new GenreCharacteristic
            {
                CharacteristicId = charId,
                CharacteristicName = charName,
                CharacteristicType = charType,
                TypicalValue = typicalValue,
                ValueRange = valueRange,
                AssociatedGenres = new List<string>(),
                CreatedDate = DateTime.Now
            };
            characteristics[charId] = characteristic;
        }

        public void LinkCharacteristicToGenre(string genreId, string charId, double weight)
        {
            if (genres.ContainsKey(genreId) && characteristics.ContainsKey(charId))
            {
                var genre = genres[genreId];
                if (!genre.CharacteristicIds.Contains(charId))
                {
                    genre.CharacteristicIds.Add(charId);
                }
                genre.CharacteristicWeights[charId] = weight;

                characteristics[charId].AssociatedGenres.Add(genreId);
            }
        }

        public void RegisterContentItem(string itemId, string itemName, Dictionary<string, double> charValues)
        {
            var item = new ContentItem
            {
                ItemId = itemId,
                ItemName = itemName,
                CharacteristicValues = new Dictionary<string, double>(charValues),
                ClassifiedGenre = "",
                ClassificationConfidence = 0.0,
                GenreAffinities = new List<(string, double)>(),
                ClassifiedDate = DateTime.Now
            };
            items[itemId] = item;
        }

        public void ClassifyItem(string itemId)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            var genreScores = new List<(string, double)>();

            foreach (var genre in genres.Values)
            {
                double genreMatch = 0.0;
                double totalWeight = 0.0;
                var charMatches = new List<double>();

                foreach (var charId in genre.CharacteristicIds)
                {
                    if (characteristics.ContainsKey(charId) && item.CharacteristicValues.ContainsKey(charId))
                    {
                        var characteristic = characteristics[charId];
                        double itemValue = item.CharacteristicValues[charId];
                        double expectedValue = characteristic.TypicalValue;

                        double difference = Math.Abs(itemValue - expectedValue);
                        double normalizedDiff = Math.Min(difference / characteristic.ValueRange, 1.0);
                        double match = 1.0 - normalizedDiff;
                        charMatches.Add(match);

                        double weight = genre.CharacteristicWeights[charId];
                        genreMatch += match * weight;
                        totalWeight += weight;
                    }
                }

                double finalScore = totalWeight > 0 ? genreMatch / totalWeight : 0.0;
                genreScores.Add((genre.GenreId, finalScore));

                var score = new ClassificationScore
                {
                    ScoreId = $"Score-{itemId}-{genre.GenreId}",
                    ItemId = itemId,
                    GenreId = genre.GenreId,
                    MatchScore = finalScore,
                    CharacteristicMatches = charMatches,
                    AverageMatch = charMatches.Count > 0 ? charMatches.Average() : 0.0,
                    MatchQuality = GetMatchQuality(finalScore),
                    ScoredDate = DateTime.Now
                };
                scores[score.ScoreId] = score;
            }

            var bestMatch = genreScores.OrderByDescending(x => x.Item2).FirstOrDefault();
            item.ClassifiedGenre = bestMatch.Item1;
            item.ClassificationConfidence = bestMatch.Item2;
            item.GenreAffinities = genreScores.OrderByDescending(x => x.Item2).ToList();

            if (genres.ContainsKey(bestMatch.Item1))
            {
                genres[bestMatch.Item1].ItemsClassified++;
            }

            classificationLog.Add((itemId, bestMatch.Item1, bestMatch.Item2));
        }

        private string GetMatchQuality(double score)
        {
            return score switch
            {
                >= 0.9 => "Perfect Match - Excellent fit to genre",
                >= 0.75 => "Strong Match - Good genre alignment",
                >= 0.6 => "Moderate Match - Acceptable fit",
                >= 0.45 => "Weak Match - Questionable classification",
                _ => "Poor Match - Unlikely genre"
            };
        }

        public void DisplayGenre(string genreId)
        {
            if (!genres.ContainsKey(genreId)) return;

            var genre = genres[genreId];
            Console.WriteLine($"\n  Genre: {genre.GenreName}");
            Console.WriteLine($"  ID: {genre.GenreId}");
            Console.WriteLine($"  Characteristics: {genre.CharacteristicIds.Count}");
            Console.WriteLine($"  Items Classified: {genre.ItemsClassified}");
            Console.WriteLine($"  Average Confidence: {genre.AverageConfidence * 100:F1}%");
        }

        public void DisplayContentItem(string itemId)
        {
            if (!items.ContainsKey(itemId)) return;

            var item = items[itemId];
            Console.WriteLine($"\n  Content Item: {item.ItemName}");
            Console.WriteLine($"  ID: {item.ItemId}");
            Console.WriteLine($"  Classified Genre: {item.ClassifiedGenre}");
            Console.WriteLine($"  Classification Confidence: {item.ClassificationConfidence * 100:F1}%");
            Console.WriteLine($"  Top Genre Affinities:");
            foreach (var (genreId, affinity) in item.GenreAffinities.Take(3))
            {
                if (genres.ContainsKey(genreId))
                {
                    Console.WriteLine($"    {genres[genreId].GenreName}: {affinity * 100:F1}%");
                }
            }
        }

        public int GetTotalGenres()
        {
            return genres.Count;
        }

        public int GetTotalCharacteristics()
        {
            return characteristics.Count;
        }

        public int GetTotalClassifiedItems()
        {
            return items.Count;
        }

        public double GetAverageClassificationConfidence()
        {
            return items.Count > 0 ? items.Values.Average(i => i.ClassificationConfidence) : 0.0;
        }

        public List<(string, int)> GetGenreDistribution()
        {
            return genres.Values
                .Select(g => (g.GenreName, g.ItemsClassified))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public List<string> GetMisclassifiedItems(double confidenceThreshold)
        {
            return items.Values
                .Where(i => i.ClassificationConfidence < confidenceThreshold)
                .Select(i => i.ItemId)
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           Recognizing Genre Classification                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new GenreClassificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Genres]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterGenre("GENRE-001", "Science Fiction");
        engine.RegisterGenre("GENRE-002", "Fantasy");
        engine.RegisterGenre("GENRE-003", "Mystery");
        engine.RegisterGenre("GENRE-004", "Romance");
        engine.RegisterGenre("GENRE-005", "Thriller");

        Console.WriteLine("  ✓ Registered 5 genre profiles");
        engine.DisplayGenre("GENRE-001");
        engine.DisplayGenre("GENRE-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Genre Characteristics]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterCharacteristic("CHAR-001", "Technology Level", "Numeric", 8.0, 10.0);
        engine.RegisterCharacteristic("CHAR-002", "Magic Prevalence", "Numeric", 7.0, 10.0);
        engine.RegisterCharacteristic("CHAR-003", "Mystery Elements", "Numeric", 8.0, 10.0);
        engine.RegisterCharacteristic("CHAR-004", "Emotional Intensity", "Numeric", 8.5, 10.0);
        engine.RegisterCharacteristic("CHAR-005", "Action Level", "Numeric", 7.5, 10.0);
        engine.RegisterCharacteristic("CHAR-006", "Pace Speed", "Numeric", 8.0, 10.0);

        Console.WriteLine("  ✓ Registered 6 genre characteristics");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Linking Characteristics to Genres]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkCharacteristicToGenre("GENRE-001", "CHAR-001", 0.95);
        engine.LinkCharacteristicToGenre("GENRE-001", "CHAR-005", 0.70);
        engine.LinkCharacteristicToGenre("GENRE-001", "CHAR-006", 0.75);

        engine.LinkCharacteristicToGenre("GENRE-002", "CHAR-002", 0.95);
        engine.LinkCharacteristicToGenre("GENRE-002", "CHAR-005", 0.65);

        engine.LinkCharacteristicToGenre("GENRE-003", "CHAR-003", 0.95);
        engine.LinkCharacteristicToGenre("GENRE-003", "CHAR-006", 0.80);

        engine.LinkCharacteristicToGenre("GENRE-004", "CHAR-004", 0.90);
        engine.LinkCharacteristicToGenre("GENRE-004", "CHAR-006", 0.70);

        engine.LinkCharacteristicToGenre("GENRE-005", "CHAR-005", 0.95);
        engine.LinkCharacteristicToGenre("GENRE-005", "CHAR-006", 0.85);

        Console.WriteLine("  ✓ Linked 11 characteristic associations");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Registering Content Items]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterContentItem("ITEM-001", "Space Station Mystery",
            new Dictionary<string, double> { {"CHAR-001", 9.0}, {"CHAR-003", 8.0}, {"CHAR-005", 6.5}, {"CHAR-006", 8.5} });

        engine.RegisterContentItem("ITEM-002", "Enchanted Forest Quest",
            new Dictionary<string, double> { {"CHAR-002", 9.0}, {"CHAR-005", 7.5}, {"CHAR-006", 7.0} });

        engine.RegisterContentItem("ITEM-003", "Detective's Dark Secret",
            new Dictionary<string, double> { {"CHAR-003", 9.5}, {"CHAR-005", 7.0}, {"CHAR-006", 8.5} });

        engine.RegisterContentItem("ITEM-004", "Love in the City",
            new Dictionary<string, double> { {"CHAR-004", 9.0}, {"CHAR-006", 7.5} });

        engine.RegisterContentItem("ITEM-005", "High-Speed Chase",
            new Dictionary<string, double> { {"CHAR-005", 9.5}, {"CHAR-006", 9.0} });

        Console.WriteLine("  ✓ Registered 5 content items");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Classifying Items by Genre]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ClassifyItem("ITEM-001");
        engine.ClassifyItem("ITEM-002");
        engine.ClassifyItem("ITEM-003");
        engine.ClassifyItem("ITEM-004");
        engine.ClassifyItem("ITEM-005");

        Console.WriteLine("  ✓ Classified 5 items into genres");
        engine.DisplayContentItem("ITEM-001");
        engine.DisplayContentItem("ITEM-005");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Genre Classification Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var distribution = engine.GetGenreDistribution();
        Console.WriteLine("  Genre Distribution:");
        foreach (var (genreName, count) in distribution)
        {
            Console.WriteLine($"    {genreName}: {count} items");
        }

        Console.WriteLine($"\n  Overall Classification Statistics:");
        Console.WriteLine($"    Total Items Classified: {engine.GetTotalClassifiedItems()}");
        Console.WriteLine($"    Average Confidence: {engine.GetAverageClassificationConfidence() * 100:F1}%");

        var questionableItems = engine.GetMisclassifiedItems(0.7);
        if (questionableItems.Count > 0)
        {
            Console.WriteLine($"    Items with Low Confidence (<70%): {questionableItems.Count}");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Genre Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Genre Recognition Architecture:");
        Console.WriteLine("    Layer 1: Genre Registration (define genre profiles)");
        Console.WriteLine("    Layer 2: Characteristic Definition (establish distinguishing features)");
        Console.WriteLine("    Layer 3: Genre-Characteristic Linking (weight feature importance)");
        Console.WriteLine("    Layer 4: Content Registration (describe items by characteristics)");
        Console.WriteLine("    Layer 5: Similarity Scoring (calculate genre match scores)");
        Console.WriteLine("    Layer 6: Classification (assign primary genre)");
        Console.WriteLine("    Layer 7: Confidence Assessment (evaluate classification quality)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define multiple genre profiles with unique characteristics");
        Console.WriteLine("    ✓ Weight characteristics differently for each genre");
        Console.WriteLine("    ✓ Register content items with multi-dimensional properties");
        Console.WriteLine("    ✓ Calculate match scores for all genres");
        Console.WriteLine("    ✓ Assign items to best-matching genre");
        Console.WriteLine("    ✓ Track classification confidence and quality");
        Console.WriteLine("    ✓ Generate genre distribution statistics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Genre classification system complete");
        Console.ResetColor();
    }
}
