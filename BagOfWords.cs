using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.TextRepresentation
{
    /// <summary>
    /// Represents a single document in the corpus with its word counts.
    /// </summary>
    public class Document
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public Dictionary<string, int> WordCounts { get; set; }
        public string Label { get; set; }

        public Document(string id, string text, string label = "")
        {
            Id = id;
            Text = text;
            Label = label;
            WordCounts = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Tokenizes text into individual words and handles preprocessing.
    /// </summary>
    public class Tokenizer
    {
        private HashSet<string> stopWords;
        private bool removeStopWords;
        private bool convertToLowercase;

        public Tokenizer(bool removeStopWords = true, bool convertToLowercase = true)
        {
            this.removeStopWords = removeStopWords;
            this.convertToLowercase = convertToLowercase;
            this.stopWords = InitializeStopWords();
        }

        private HashSet<string> InitializeStopWords()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "with", "by", "from", "as", "is", "was", "are", "were", "be",
                "been", "being", "have", "has", "had", "do", "does", "did", "will",
                "would", "could", "should", "may", "might", "can", "this", "that",
                "these", "those", "i", "you", "he", "she", "it", "we", "they",
                "what", "which", "who", "when", "where", "why", "how", "all",
                "each", "every", "both", "few", "more", "most", "other", "some",
                "such", "no", "nor", "not", "only", "own", "same", "so", "than",
                "too", "very", "just", "up", "out", "if", "about", "into", "through"
            };
        }

        /// <summary>
        /// Tokenizes text into individual words.
        /// </summary>
        public List<string> Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            if (convertToLowercase)
                text = text.ToLower();

            // Remove punctuation and split into words
            string[] words = Regex.Split(text, @"[^\w]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            List<string> tokens = new List<string>(words);

            if (removeStopWords)
            {
                tokens = tokens.Where(t => !stopWords.Contains(t)).ToList();
            }

            return tokens;
        }

        /// <summary>
        /// Adds custom stop words.
        /// </summary>
        public void AddStopWord(string word)
        {
            stopWords.Add(word.ToLower());
        }

        /// <summary>
        /// Removes a stop word.
        /// </summary>
        public void RemoveStopWord(string word)
        {
            stopWords.Remove(word.ToLower());
        }
    }

    /// <summary>
    /// Creates and manages vocabulary for the corpus.
    /// Maps words to unique indices.
    /// </summary>
    public class Vocabulary
    {
        private Dictionary<string, int> wordToIndex;
        private Dictionary<int, string> indexToWord;
        private int nextIndex;

        public Vocabulary()
        {
            wordToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            indexToWord = new Dictionary<int, string>();
            nextIndex = 0;
        }

        /// <summary>
        /// Adds a word to the vocabulary.
        /// </summary>
        public int AddWord(string word)
        {
            if (wordToIndex.TryGetValue(word, out int index))
                return index;

            index = nextIndex++;
            wordToIndex[word] = index;
            indexToWord[index] = word;
            return index;
        }

        /// <summary>
        /// Gets the index for a word, or -1 if not in vocabulary.
        /// </summary>
        public int GetIndex(string word)
        {
            return wordToIndex.TryGetValue(word, out int index) ? index : -1;
        }

        /// <summary>
        /// Gets the word at a specific index.
        /// </summary>
        public string GetWord(int index)
        {
            return indexToWord.TryGetValue(index, out string word) ? word : null;
        }

        /// <summary>
        /// Gets the total vocabulary size.
        /// </summary>
        public int Size => wordToIndex.Count;

        public IEnumerable<string> GetAllWords() => wordToIndex.Keys;
    }

    /// <summary>
    /// Implements Bag of Words model for text representation.
    /// </summary>
    public class BagOfWordsModel
    {
        private Vocabulary vocabulary;
        private List<Document> documents;
        private Tokenizer tokenizer;
        private bool useTFIDF;

        public BagOfWordsModel(bool useStopWords = true, bool useTFIDF = false)
        {
            this.vocabulary = new Vocabulary();
            this.documents = new List<Document>();
            this.tokenizer = new Tokenizer(useStopWords);
            this.useTFIDF = useTFIDF;
        }

        /// <summary>
        /// Adds a document to the model and builds vocabulary.
        /// </summary>
        public void AddDocument(Document document)
        {
            List<string> tokens = tokenizer.Tokenize(document.Text);

            foreach (string token in tokens)
            {
                vocabulary.AddWord(token);
                if (!document.WordCounts.ContainsKey(token))
                    document.WordCounts[token] = 0;
                document.WordCounts[token]++;
            }

            documents.Add(document);
        }

        /// <summary>
        /// Adds multiple documents to the model.
        /// </summary>
        public void AddDocuments(List<Document> docs)
        {
            foreach (var doc in docs)
            {
                AddDocument(doc);
            }
        }

        /// <summary>
        /// Creates a document-term matrix (rows: documents, columns: words).
        /// </summary>
        public double[][] CreateDocumentTermMatrix()
        {
            double[][] matrix = new double[documents.Count][];

            for (int docIdx = 0; docIdx < documents.Count; docIdx++)
            {
                matrix[docIdx] = new double[vocabulary.Size];

                foreach (string word in documents[docIdx].WordCounts.Keys)
                {
                    int wordIdx = vocabulary.GetIndex(word);
                    if (wordIdx >= 0)
                    {
                        matrix[docIdx][wordIdx] = documents[docIdx].WordCounts[word];
                    }
                }

                if (useTFIDF)
                {
                    matrix[docIdx] = CalculateTFIDF(documents[docIdx], matrix[docIdx]);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Converts a single text into a BoW vector.
        /// </summary>
        public double[] TextToVector(string text)
        {
            double[] vector = new double[vocabulary.Size];
            List<string> tokens = tokenizer.Tokenize(text);

            foreach (string token in tokens)
            {
                int index = vocabulary.GetIndex(token);
                if (index >= 0)
                {
                    vector[index]++;
                }
            }

            if (useTFIDF)
            {
                Document tempDoc = new Document("temp", text);
                foreach (var token in tokens)
                {
                    if (!tempDoc.WordCounts.ContainsKey(token))
                        tempDoc.WordCounts[token] = 0;
                    tempDoc.WordCounts[token]++;
                }
                vector = CalculateTFIDF(tempDoc, vector);
            }

            return vector;
        }

        /// <summary>
        /// Calculates TF-IDF scores for a document.
        /// </summary>
        private double[] CalculateTFIDF(Document document, double[] tfidfVector)
        {
            double[] result = new double[vocabulary.Size];

            int totalWords = document.WordCounts.Values.Sum();

            foreach (var wordEntry in document.WordCounts)
            {
                int wordIndex = vocabulary.GetIndex(wordEntry.Key);
                if (wordIndex >= 0)
                {
                    double tf = wordEntry.Value / (double)totalWords;
                    double idf = Math.Log(documents.Count / (double)(1 + CountDocumentsWithWord(wordEntry.Key)));
                    result[wordIndex] = tf * idf;
                }
            }

            return result;
        }

        /// <summary>
        /// Counts how many documents contain a specific word.
        /// </summary>
        private int CountDocumentsWithWord(string word)
        {
            return documents.Count(d => d.WordCounts.ContainsKey(word));
        }

        public Vocabulary GetVocabulary() => vocabulary;
        public List<Document> GetDocuments() => documents;
        public int GetDocumentCount() => documents.Count;
        public int GetVocabularySize() => vocabulary.Size;
    }

    /// <summary>
    /// Calculates similarity between documents using various metrics.
    /// </summary>
    public class SimilarityCalculator
    {
        /// <summary>
        /// Calculates cosine similarity between two vectors.
        /// </summary>
        public static double CosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Calculates Euclidean distance between two vectors.
        /// </summary>
        public static double EuclideanDistance(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length");

            double sum = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                double diff = vector1[i] - vector2[i];
                sum += diff * diff;
            }

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Calculates Jaccard similarity between two sets of words.
        /// </summary>
        public static double JaccardSimilarity(List<string> words1, List<string> words2)
        {
            var set1 = new HashSet<string>(words1);
            var set2 = new HashSet<string>(words2);

            int intersection = set1.Intersect(set2).Count();
            int union = set1.Union(set2).Count();

            return union == 0 ? 0 : intersection / (double)union;
        }
    }

    /// <summary>
    /// Text classifier using Bag of Words representation.
    /// </summary>
    public class BOWTextClassifier
    {
        private BagOfWordsModel model;
        private Dictionary<string, double[]> classVectors;
        private Dictionary<string, int> classDocumentCounts;

        public BOWTextClassifier(BagOfWordsModel model)
        {
            this.model = model;
            this.classVectors = new Dictionary<string, double[]>();
            this.classDocumentCounts = new Dictionary<string, int>();
            TrainClassifier();
        }

        /// <summary>
        /// Trains the classifier by computing average vectors for each class.
        /// </summary>
        private void TrainClassifier()
        {
            double[][] documentTermMatrix = model.CreateDocumentTermMatrix();
            var documents = model.GetDocuments();
            var vocab = model.GetVocabulary();
            int vocabSize = vocab.Size;

            var groupedByLabel = documents.GroupBy(d => d.Label);

            foreach (var labelGroup in groupedByLabel)
            {
                string label = labelGroup.Key;
                double[] averageVector = new double[vocabSize];

                foreach (var doc in labelGroup)
                {
                    int docIndex = documents.IndexOf(doc);
                    for (int i = 0; i < vocabSize; i++)
                    {
                        averageVector[i] += documentTermMatrix[docIndex][i];
                    }
                }

                for (int i = 0; i < vocabSize; i++)
                {
                    averageVector[i] /= labelGroup.Count();
                }

                classVectors[label] = averageVector;
                classDocumentCounts[label] = labelGroup.Count();
            }
        }

        /// <summary>
        /// Classifies a text and returns the predicted label.
        /// </summary>
        public string Classify(string text)
        {
            double[] vector = model.TextToVector(text);
            string bestLabel = null;
            double bestSimilarity = -1;

            foreach (var classEntry in classVectors)
            {
                double similarity = SimilarityCalculator.CosineSimilarity(vector, classEntry.Value);
                if (similarity > bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestLabel = classEntry.Key;
                }
            }

            return bestLabel;
        }

        /// <summary>
        /// Classifies text and returns probabilities for each class.
        /// </summary>
        public Dictionary<string, double> ClassifyWithProbabilities(string text)
        {
            double[] vector = model.TextToVector(text);
            var similarities = new Dictionary<string, double>();

            foreach (var classEntry in classVectors)
            {
                similarities[classEntry.Key] = SimilarityCalculator.CosineSimilarity(vector, classEntry.Value);
            }

            double totalSimilarity = similarities.Values.Sum();
            var probabilities = new Dictionary<string, double>();

            foreach (var entry in similarities)
            {
                probabilities[entry.Key] = totalSimilarity > 0 ? entry.Value / totalSimilarity : 0;
            }

            return probabilities;
        }
    }

    /// <summary>
    /// Provides analysis and statistics for the BoW model.
    /// </summary>
    public class BagOfWordsAnalyzer
    {
        private BagOfWordsModel model;

        public BagOfWordsAnalyzer(BagOfWordsModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Gets the most frequent words in the corpus.
        /// </summary>
        public List<(string word, int count)> GetMostFrequentWords(int topN = 10)
        {
            var wordFrequencies = new Dictionary<string, int>();

            foreach (var doc in model.GetDocuments())
            {
                foreach (var wordEntry in doc.WordCounts)
                {
                    if (!wordFrequencies.ContainsKey(wordEntry.Key))
                        wordFrequencies[wordEntry.Key] = 0;
                    wordFrequencies[wordEntry.Key] += wordEntry.Value;
                }
            }

            return wordFrequencies
                .OrderByDescending(w => w.Value)
                .Take(topN)
                .Select(w => (w.Key, w.Value))
                .ToList();
        }

        /// <summary>
        /// Calculates average document length.
        /// </summary>
        public double GetAverageDocumentLength()
        {
            if (model.GetDocumentCount() == 0) return 0;

            int totalWords = 0;
            foreach (var doc in model.GetDocuments())
            {
                totalWords += doc.WordCounts.Values.Sum();
            }

            return totalWords / (double)model.GetDocumentCount();
        }

        /// <summary>
        /// Gets vocabulary statistics.
        /// </summary>
        public string GetVocabularyStatistics()
        {
            int vocabSize = model.GetVocabularySize();
            int docCount = model.GetDocumentCount();
            int totalWords = 0;

            foreach (var doc in model.GetDocuments())
            {
                totalWords += doc.WordCounts.Values.Sum();
            }

            return $"Vocabulary Statistics\n" +
                   $"{'='.ToString().PadRight(40, '=')}\n" +
                   $"Vocabulary Size: {vocabSize}\n" +
                   $"Document Count: {docCount}\n" +
                   $"Total Words: {totalWords}\n" +
                   $"Average Words per Document: {GetAverageDocumentLength():F2}";
        }
    }

    /// <summary>
    /// Examples demonstrating Bag of Words usage in NLP tasks.
    /// </summary>
    public static class BagOfWordsExamples
    {
        public static void DemonstrateBasicBagOfWords()
        {
            Console.WriteLine("=== Basic Bag of Words Representation ===\n");

            var doc = new Document("1", "This is a sample sentence");
            var model = new BagOfWordsModel();
            model.AddDocument(doc);

            var vector = model.TextToVector("This is a sample sentence");

            Console.WriteLine($"Original text: 'This is a sample sentence'");
            Console.WriteLine($"\nVocabulary size: {model.GetVocabularySize()}");
            Console.WriteLine("\nVector representation (word counts):");

            var vocab = model.GetVocabulary();
            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] > 0)
                {
                    Console.WriteLine($"  {vocab.GetWord(i)}: {vector[i]}");
                }
            }
        }

        public static void DemonstrateDocumentTermMatrix()
        {
            Console.WriteLine("\n=== Document-Term Matrix ===\n");

            var model = new BagOfWordsModel();

            model.AddDocument(new Document("1", "machine learning is powerful"));
            model.AddDocument(new Document("2", "deep learning neural networks"));
            model.AddDocument(new Document("3", "machine learning algorithms"));

            double[][] matrix = model.CreateDocumentTermMatrix();
            var vocab = model.GetVocabulary();

            Console.WriteLine("Document-Term Matrix:");
            Console.WriteLine("Document | " + string.Join(" | ", vocab.GetAllWords()));
            Console.WriteLine("-".PadRight(50, '-'));

            for (int i = 0; i < matrix.Length; i++)
            {
                Console.Write($"Doc {i + 1}    | ");
                Console.WriteLine(string.Join(" | ", matrix[i].Select(v => v.ToString("F0"))));
            }
        }

        public static void DemonstrateTextClassification()
        {
            Console.WriteLine("\n=== Text Classification with Bag of Words ===\n");

            var model = new BagOfWordsModel();

            // Training documents
            model.AddDocument(new Document("1", "beautiful sunny day perfect weather", "positive"));
            model.AddDocument(new Document("2", "wonderful amazing great excellent", "positive"));
            model.AddDocument(new Document("3", "terrible horrible awful bad", "negative"));
            model.AddDocument(new Document("4", "worst disappointing terrible sad", "negative"));

            var classifier = new BOWTextClassifier(model);

            // Test documents
            string[] testTexts = new string[]
            {
                "amazing and wonderful",
                "horrible and terrible",
                "beautiful perfect day"
            };

            Console.WriteLine("Classification Results:");
            foreach (string text in testTexts)
            {
                string classification = classifier.Classify(text);
                var probabilities = classifier.ClassifyWithProbabilities(text);

                Console.WriteLine($"\nText: '{text}'");
                Console.WriteLine($"Classification: {classification}");
                Console.WriteLine("Probabilities:");
                foreach (var prob in probabilities)
                {
                    Console.WriteLine($"  {prob.Key}: {prob.Value:F4}");
                }
            }
        }

        public static void DemonstrateSimilarityCalculation()
        {
            Console.WriteLine("\n=== Document Similarity Calculation ===\n");

            var model = new BagOfWordsModel();

            model.AddDocument(new Document("1", "cat sits on mat"));
            model.AddDocument(new Document("2", "dog plays in park"));
            model.AddDocument(new Document("3", "cat and dog play"));

            double[][] matrix = model.CreateDocumentTermMatrix();

            Console.WriteLine("Pairwise Document Similarities (Cosine):");
            Console.WriteLine("-".PadRight(40, '-'));

            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = i + 1; j < matrix.Length; j++)
                {
                    double similarity = SimilarityCalculator.CosineSimilarity(matrix[i], matrix[j]);
                    Console.WriteLine($"Doc {i + 1} <-> Doc {j + 1}: {similarity:F4}");
                }
            }
        }

        public static void DemonstrateVocabularyAnalysis()
        {
            Console.WriteLine("\n=== Vocabulary and Corpus Analysis ===\n");

            var model = new BagOfWordsModel();

            model.AddDocument(new Document("1", "natural language processing is powerful"));
            model.AddDocument(new Document("2", "machine learning and artificial intelligence"));
            model.AddDocument(new Document("3", "deep learning networks process information"));

            var analyzer = new BagOfWordsAnalyzer(model);

            Console.WriteLine(analyzer.GetVocabularyStatistics());

            Console.WriteLine("\n\nMost Frequent Words:");
            var topWords = analyzer.GetMostFrequentWords(5);
            foreach (var word in topWords)
            {
                Console.WriteLine($"  {word.word}: {word.count}");
            }
        }

        public static void DemonstrateTFIDFRepresentation()
        {
            Console.WriteLine("\n=== TF-IDF Representation ===\n");

            var model = new BagOfWordsModel(useStopWords: true, useTFIDF: true);

            model.AddDocument(new Document("1", "information retrieval document search"));
            model.AddDocument(new Document("2", "document classification machine learning"));
            model.AddDocument(new Document("3", "text mining information extraction"));

            double[][] tfidfMatrix = model.CreateDocumentTermMatrix();
            var vocab = model.GetVocabulary();

            Console.WriteLine("TF-IDF Matrix (non-zero values only):");
            for (int docIdx = 0; docIdx < tfidfMatrix.Length; docIdx++)
            {
                Console.WriteLine($"\nDocument {docIdx + 1}:");
                for (int wordIdx = 0; wordIdx < tfidfMatrix[docIdx].Length; wordIdx++)
                {
                    if (tfidfMatrix[docIdx][wordIdx] > 0)
                    {
                        Console.WriteLine($"  {vocab.GetWord(wordIdx)}: {tfidfMatrix[docIdx][wordIdx]:F4}");
                    }
                }
            }
        }

        public static void DemonstrateSentimentAnalysis()
        {
            Console.WriteLine("\n=== Sentiment Analysis with BoW ===\n");

            var model = new BagOfWordsModel();

            // Training data
            model.AddDocument(new Document("1", "excellent product highly recommend", "positive"));
            model.AddDocument(new Document("2", "amazing quality great service", "positive"));
            model.AddDocument(new Document("3", "terrible quality waste money", "negative"));
            model.AddDocument(new Document("4", "poor service very disappointing", "negative"));

            var classifier = new BOWTextClassifier(model);

            var testReviews = new string[]
            {
                "excellent quality and service",
                "terrible waste of money",
                "great product highly satisfied"
            };

            Console.WriteLine("Sentiment Analysis Results:");
            foreach (string review in testReviews)
            {
                var probs = classifier.ClassifyWithProbabilities(review);
                Console.WriteLine($"\nReview: '{review}'");
                Console.WriteLine($"Sentiment: {classifier.Classify(review)}");
                Console.WriteLine($"Confidence: {probs.Values.Max():F4}");
            }
        }

        public static void DemonstrateSpamDetection()
        {
            Console.WriteLine("\n=== Spam Detection with BoW ===\n");

            var model = new BagOfWordsModel();

            // Training data
            model.AddDocument(new Document("1", "click here win free money now", "spam"));
            model.AddDocument(new Document("2", "special offer limited time act", "spam"));
            model.AddDocument(new Document("3", "meeting tomorrow at office", "ham"));
            model.AddDocument(new Document("4", "project update attached document", "ham"));

            var classifier = new BOWTextClassifier(model);

            var testEmails = new string[]
            {
                "click here free offer now",
                "meeting tomorrow office update",
                "win free money special"
            };

            Console.WriteLine("Spam Detection Results:");
            foreach (string email in testEmails)
            {
                string classification = classifier.Classify(email);
                var probs = classifier.ClassifyWithProbabilities(email);
                Console.WriteLine($"\nEmail: '{email}'");
                Console.WriteLine($"Classification: {classification}");
                Console.WriteLine($"Spam probability: {probs["spam"]:F4}");
                Console.WriteLine($"Ham probability: {probs["ham"]:F4}");
            }
        }
    }
}
