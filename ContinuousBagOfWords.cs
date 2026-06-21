using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.WordEmbeddings
{
    /// <summary>
    /// Represents a context window for CBOW training.
    /// Contains surrounding words and the target word.
    /// </summary>
    public class ContextWindow
    {
        public List<string> ContextWords { get; set; }
        public string TargetWord { get; set; }

        public ContextWindow(List<string> contextWords, string targetWord)
        {
            ContextWords = contextWords;
            TargetWord = targetWord;
        }
    }

    /// <summary>
    /// Word embeddings vector with metadata.
    /// </summary>
    public class WordEmbedding
    {
        public string Word { get; set; }
        public double[] Vector { get; set; }

        public WordEmbedding(string word, double[] vector)
        {
            Word = word;
            Vector = vector;
        }
    }

    /// <summary>
    /// Tokenizes text and generates context windows for CBOW training.
    /// </summary>
    public class ContextWindowGenerator
    {
        private int windowSize;
        private HashSet<string> stopWords;
        private bool removeStopWords;

        public ContextWindowGenerator(int windowSize = 2, bool removeStopWords = false)
        {
            this.windowSize = windowSize;
            this.removeStopWords = removeStopWords;
            this.stopWords = InitializeStopWords();
        }

        private HashSet<string> InitializeStopWords()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "with", "by", "from", "is", "was", "are", "be", "been", "being"
            };
        }

        /// <summary>
        /// Tokenizes text into words.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            text = text.ToLower();
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
        /// Generates context windows from text.
        /// </summary>
        public List<ContextWindow> GenerateContextWindows(string text)
        {
            List<string> tokens = Tokenize(text);
            List<ContextWindow> windows = new List<ContextWindow>();

            for (int i = windowSize; i < tokens.Count - windowSize; i++)
            {
                List<string> context = new List<string>();

                for (int j = i - windowSize; j < i; j++)
                {
                    context.Add(tokens[j]);
                }

                for (int j = i + 1; j <= i + windowSize; j++)
                {
                    if (j < tokens.Count)
                    {
                        context.Add(tokens[j]);
                    }
                }

                windows.Add(new ContextWindow(context, tokens[i]));
            }

            return windows;
        }

        /// <summary>
        /// Generates context windows from multiple documents.
        /// </summary>
        public List<ContextWindow> GenerateContextWindowsFromMultipleDocs(List<string> documents)
        {
            List<ContextWindow> allWindows = new List<ContextWindow>();

            foreach (string doc in documents)
            {
                allWindows.AddRange(GenerateContextWindows(doc));
            }

            return allWindows;
        }
    }

    /// <summary>
    /// Vocabulary manager for CBOW model.
    /// </summary>
    public class CBOWVocabulary
    {
        private Dictionary<string, int> wordToIndex;
        private Dictionary<int, string> indexToWord;
        private int nextIndex;

        public CBOWVocabulary()
        {
            wordToIndex = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            indexToWord = new Dictionary<int, string>();
            nextIndex = 0;
        }

        public int AddWord(string word)
        {
            if (wordToIndex.TryGetValue(word, out int index))
                return index;

            index = nextIndex++;
            wordToIndex[word] = index;
            indexToWord[index] = word;
            return index;
        }

        public int GetIndex(string word)
        {
            return wordToIndex.TryGetValue(word, out int index) ? index : -1;
        }

        public string GetWord(int index)
        {
            return indexToWord.TryGetValue(index, out string word) ? word : null;
        }

        public int Size => wordToIndex.Count;
        public IEnumerable<string> GetAllWords() => wordToIndex.Keys;
    }

    /// <summary>
    /// Continuous Bag-of-Words (CBOW) neural network model.
    /// Predicts target word given context words.
    /// </summary>
    public class CBOWModel
    {
        private CBOWVocabulary vocabulary;
        private double[][] inputWeights;
        private double[][] outputWeights;
        private int embeddingDimension;
        private double learningRate;
        private Random random;

        public CBOWModel(int embeddingDimension = 100, double learningRate = 0.01)
        {
            this.vocabulary = new CBOWVocabulary();
            this.embeddingDimension = embeddingDimension;
            this.learningRate = learningRate;
            this.random = new Random();
        }

        /// <summary>
        /// Initializes the neural network weights from a corpus.
        /// </summary>
        public void InitializeFromContextWindows(List<ContextWindow> windows)
        {
            // Build vocabulary
            foreach (var window in windows)
            {
                foreach (var contextWord in window.ContextWords)
                {
                    vocabulary.AddWord(contextWord);
                }
                vocabulary.AddWord(window.TargetWord);
            }

            int vocabSize = vocabulary.Size;

            // Initialize weight matrices with random values
            inputWeights = new double[vocabSize][];
            outputWeights = new double[vocabSize][];

            for (int i = 0; i < vocabSize; i++)
            {
                inputWeights[i] = new double[embeddingDimension];
                outputWeights[i] = new double[embeddingDimension];

                for (int j = 0; j < embeddingDimension; j++)
                {
                    inputWeights[i][j] = (random.NextDouble() - 0.5) / embeddingDimension;
                    outputWeights[i][j] = (random.NextDouble() - 0.5) / embeddingDimension;
                }
            }
        }

        /// <summary>
        /// Trains the CBOW model on context windows.
        /// </summary>
        public void Train(List<ContextWindow> windows, int epochs = 5)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                foreach (var window in windows)
                {
                    totalLoss += TrainOnWindow(window);
                }

                Console.WriteLine($"Epoch {epoch + 1}/{epochs}, Loss: {totalLoss:F6}");
            }
        }

        /// <summary>
        /// Trains the model on a single context window.
        /// </summary>
        private double TrainOnWindow(ContextWindow window)
        {
            int targetIndex = vocabulary.GetIndex(window.TargetWord);
            if (targetIndex < 0) return 0;

            // Get context word indices
            List<int> contextIndices = new List<int>();
            foreach (var contextWord in window.ContextWords)
            {
                int idx = vocabulary.GetIndex(contextWord);
                if (idx >= 0)
                    contextIndices.Add(idx);
            }

            if (contextIndices.Count == 0) return 0;

            // Forward pass: compute average context embedding
            double[] contextEmbedding = ComputeAverageEmbedding(contextIndices);

            // Compute output layer
            double[] output = new double[vocabulary.Size];
            for (int i = 0; i < vocabulary.Size; i++)
            {
                output[i] = ComputeDotProduct(contextEmbedding, outputWeights[i]);
            }

            // Apply softmax
            double[] softmaxOutput = ApplySoftmax(output);

            // Compute loss (cross-entropy)
            double loss = -Math.Log(Math.Max(1e-10, softmaxOutput[targetIndex]));

            // Backpropagation
            double[] outputGradient = new double[vocabulary.Size];
            for (int i = 0; i < vocabulary.Size; i++)
            {
                outputGradient[i] = softmaxOutput[i];
            }
            outputGradient[targetIndex] -= 1;

            // Update output weights
            for (int i = 0; i < vocabulary.Size; i++)
            {
                for (int j = 0; j < embeddingDimension; j++)
                {
                    outputWeights[i][j] -= learningRate * outputGradient[i] * contextEmbedding[j];
                }
            }

            // Compute gradient for context embeddings
            double[] contextGradient = new double[embeddingDimension];
            for (int j = 0; j < embeddingDimension; j++)
            {
                for (int i = 0; i < vocabulary.Size; i++)
                {
                    contextGradient[j] += outputGradient[i] * outputWeights[i][j];
                }
            }

            // Update input weights for context words
            foreach (int contextIdx in contextIndices)
            {
                for (int j = 0; j < embeddingDimension; j++)
                {
                    inputWeights[contextIdx][j] -= learningRate * contextGradient[j] / contextIndices.Count;
                }
            }

            return loss;
        }

        private double[] ComputeAverageEmbedding(List<int> indices)
        {
            double[] average = new double[embeddingDimension];

            foreach (int idx in indices)
            {
                for (int i = 0; i < embeddingDimension; i++)
                {
                    average[i] += inputWeights[idx][i];
                }
            }

            for (int i = 0; i < embeddingDimension; i++)
            {
                average[i] /= indices.Count;
            }

            return average;
        }

        private double ComputeDotProduct(double[] vector1, double[] vector2)
        {
            double result = 0;
            for (int i = 0; i < vector1.Length; i++)
            {
                result += vector1[i] * vector2[i];
            }
            return result;
        }

        private double[] ApplySoftmax(double[] values)
        {
            double max = values.Max();
            double[] exp = new double[values.Length];
            double sum = 0;

            for (int i = 0; i < values.Length; i++)
            {
                exp[i] = Math.Exp(values[i] - max);
                sum += exp[i];
            }

            double[] softmax = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                softmax[i] = exp[i] / sum;
            }

            return softmax;
        }

        /// <summary>
        /// Predicts the target word given context words.
        /// </summary>
        public string PredictTargetWord(List<string> contextWords)
        {
            List<int> contextIndices = new List<int>();
            foreach (var contextWord in contextWords)
            {
                int idx = vocabulary.GetIndex(contextWord);
                if (idx >= 0)
                    contextIndices.Add(idx);
            }

            if (contextIndices.Count == 0) return null;

            double[] contextEmbedding = ComputeAverageEmbedding(contextIndices);

            double[] output = new double[vocabulary.Size];
            for (int i = 0; i < vocabulary.Size; i++)
            {
                output[i] = ComputeDotProduct(contextEmbedding, outputWeights[i]);
            }

            double[] softmax = ApplySoftmax(output);

            int maxIndex = 0;
            for (int i = 1; i < softmax.Length; i++)
            {
                if (softmax[i] > softmax[maxIndex])
                    maxIndex = i;
            }

            return vocabulary.GetWord(maxIndex);
        }

        /// <summary>
        /// Predicts target word with probabilities.
        /// </summary>
        public Dictionary<string, double> PredictTargetWordWithProbabilities(List<string> contextWords, int topN = 5)
        {
            List<int> contextIndices = new List<int>();
            foreach (var contextWord in contextWords)
            {
                int idx = vocabulary.GetIndex(contextWord);
                if (idx >= 0)
                    contextIndices.Add(idx);
            }

            if (contextIndices.Count == 0) return new Dictionary<string, double>();

            double[] contextEmbedding = ComputeAverageEmbedding(contextIndices);

            double[] output = new double[vocabulary.Size];
            for (int i = 0; i < vocabulary.Size; i++)
            {
                output[i] = ComputeDotProduct(contextEmbedding, outputWeights[i]);
            }

            double[] softmax = ApplySoftmax(output);

            var predictions = new Dictionary<string, double>();
            for (int i = 0; i < vocabulary.Size; i++)
            {
                predictions[vocabulary.GetWord(i)] = softmax[i];
            }

            return predictions
                .OrderByDescending(p => p.Value)
                .Take(topN)
                .ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        /// Gets the word embedding vector.
        /// </summary>
        public double[] GetWordEmbedding(string word)
        {
            int index = vocabulary.GetIndex(word);
            if (index < 0) return null;

            return (double[])inputWeights[index].Clone();
        }

        /// <summary>
        /// Gets all word embeddings.
        /// </summary>
        public List<WordEmbedding> GetAllEmbeddings()
        {
            List<WordEmbedding> embeddings = new List<WordEmbedding>();

            foreach (string word in vocabulary.GetAllWords())
            {
                int index = vocabulary.GetIndex(word);
                embeddings.Add(new WordEmbedding(word, (double[])inputWeights[index].Clone()));
            }

            return embeddings;
        }

        public CBOWVocabulary GetVocabulary() => vocabulary;
    }

    /// <summary>
    /// Calculates semantic similarity between word embeddings.
    /// </summary>
    public class WordSimilarityCalculator
    {
        private CBOWModel model;

        public WordSimilarityCalculator(CBOWModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Calculates cosine similarity between two words.
        /// </summary>
        public double CosineSimilarity(string word1, string word2)
        {
            double[] vector1 = model.GetWordEmbedding(word1);
            double[] vector2 = model.GetWordEmbedding(word2);

            if (vector1 == null || vector2 == null) return 0;

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

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Finds most similar words to a given word.
        /// </summary>
        public List<(string word, double similarity)> FindMostSimilarWords(string word, int topN = 5)
        {
            double[] targetVector = model.GetWordEmbedding(word);
            if (targetVector == null) return new List<(string, double)>();

            var similarities = new List<(string, double)>();

            foreach (var embedding in model.GetAllEmbeddings())
            {
                if (embedding.Word != word)
                {
                    double sim = CalculateCosineSimilarity(targetVector, embedding.Vector);
                    similarities.Add((embedding.Word, sim));
                }
            }

            return similarities
                .OrderByDescending(s => s.similarity)
                .Take(topN)
                .ToList();
        }

        private double CalculateCosineSimilarity(double[] vector1, double[] vector2)
        {
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

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }
    }

    /// <summary>
    /// Examples demonstrating CBOW usage for various NLP tasks.
    /// </summary>
    public static class CBOWExamples
    {
        public static void DemonstrateBasicCBOWTraining()
        {
            Console.WriteLine("=== Basic CBOW Training ===\n");

            string trainingText = "The cat sat on the mat. The mat was very comfortable. " +
                                "The cat loved to sit on the mat.";

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindows(trainingText);

            Console.WriteLine($"Training text: '{trainingText}'");
            Console.WriteLine($"Generated context windows: {windows.Count}\n");

            // Show some example windows
            Console.WriteLine("Example context windows:");
            for (int i = 0; i < Math.Min(3, windows.Count); i++)
            {
                Console.WriteLine($"  Context: [{string.Join(", ", windows[i].ContextWords)}] -> Target: '{windows[i].TargetWord}'");
            }

            Console.WriteLine("\n--- Training CBOW Model ---");
            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 3);
        }

        public static void DemonatratePredictingTargetWord()
        {
            Console.WriteLine("\n=== Predicting Target Words ===\n");

            string trainingText = "the quick brown fox jumps over the lazy dog. " +
                                "the brown dog played in the park. " +
                                "the quick fox ran through the forest.";

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindows(trainingText);

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 5);

            Console.WriteLine("Predicting target words given context:\n");

            var testContexts = new List<List<string>>
            {
                new List<string> { "the", "brown" },
                new List<string> { "quick", "jumps" },
                new List<string> { "lazy", "dog" }
            };

            foreach (var context in testContexts)
            {
                string predicted = model.PredictTargetWord(context);
                var probabilities = model.PredictTargetWordWithProbabilities(context, topN: 3);

                Console.WriteLine($"Context: [{string.Join(", ", context)}]");
                Console.WriteLine($"Predicted: {predicted}");
                Console.WriteLine("Top predictions:");
                foreach (var prob in probabilities)
                {
                    Console.WriteLine($"  {prob.Key}: {prob.Value:F4}");
                }
                Console.WriteLine();
            }
        }

        public static void DemonstrateWordSimilarity()
        {
            Console.WriteLine("\n=== Word Similarity from Embeddings ===\n");

            string trainingText = "cat and dog are animals. cat is fluffy. dog is loyal. " +
                                "the fluffy cat sleeps. the loyal dog plays. " +
                                "animals love food.";

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindows(trainingText);

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 5);

            var calculator = new WordSimilarityCalculator(model);

            Console.WriteLine("Word Similarity Scores (Cosine):");
            Console.WriteLine("-".PadRight(40, '-'));

            var wordPairs = new List<(string, string)>
            {
                ("cat", "dog"),
                ("cat", "animal"),
                ("dog", "animal"),
                ("cat", "food"),
                ("fluffy", "loyal")
            };

            foreach (var (word1, word2) in wordPairs)
            {
                double similarity = calculator.CosineSimilarity(word1, word2);
                Console.WriteLine($"{word1} <-> {word2}: {similarity:F4}");
            }

            Console.WriteLine("\n\nMost Similar Words:");
            Console.WriteLine("-".PadRight(40, '-'));

            var targetWords = new List<string> { "cat", "dog", "animal" };
            foreach (string word in targetWords)
            {
                var similar = calculator.FindMostSimilarWords(word, topN: 3);
                Console.WriteLine($"\nSimilar to '{word}':");
                foreach (var (simWord, sim) in similar)
                {
                    Console.WriteLine($"  {simWord}: {sim:F4}");
                }
            }
        }

        public static void DemonstrateLanguageTranslationContext()
        {
            Console.WriteLine("\n=== Language Understanding via Context (Simplified) ===\n");

            string[] trainingDocs = new string[]
            {
                "the restaurant serves delicious food.",
                "the food at the restaurant was amazing.",
                "the waiter brought the food quickly.",
                "good food makes people happy.",
                "the restaurant has good service."
            };

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindowsFromMultipleDocs(trainingDocs.ToList());

            Console.WriteLine($"Training documents: {trainingDocs.Length}");
            Console.WriteLine($"Generated context windows: {windows.Count}\n");

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 3);

            Console.WriteLine("Context-based predictions show language understanding:");
            Console.WriteLine("-".PadRight(50, '-'));

            var predictions = new List<List<string>>
            {
                new List<string> { "the", "serves" },
                new List<string> { "delicious", "was" },
                new List<string> { "brought", "quickly" }
            };

            foreach (var context in predictions)
            {
                var topPredictions = model.PredictTargetWordWithProbabilities(context, topN: 2);
                Console.WriteLine($"\nContext: [{string.Join(", ", context)}]");
                foreach (var pred in topPredictions)
                {
                    Console.WriteLine($"  {pred.Key}: {pred.Value:F4}");
                }
            }
        }

        public static void DemonstratePartOfSpeechContext()
        {
            Console.WriteLine("\n=== Part-of-Speech Tagging Context ===\n");

            string[] sentences = new string[]
            {
                "the quick brown fox jumps.",
                "the lazy dog sleeps.",
                "quick foxes run fast.",
                "brown dogs bark loud.",
                "the fast runner jumps high."
            };

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindowsFromMultipleDocs(sentences.ToList());

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 3);

            var calculator = new WordSimilarityCalculator(model);

            Console.WriteLine("Words with similar contexts should have similar embeddings:");
            Console.WriteLine("(Useful for POS tagging)\n");

            Console.WriteLine("Adjectives (similar context - before nouns):");
            Console.WriteLine($"  'quick' <-> 'brown': {calculator.CosineSimilarity("quick", "brown"):F4}");
            Console.WriteLine($"  'quick' <-> 'lazy': {calculator.CosineSimilarity("quick", "lazy"):F4}");

            Console.WriteLine("\nNouns (similar context - after adjectives):");
            Console.WriteLine($"  'fox' <-> 'dog': {calculator.CosineSimilarity("fox", "dog"):F4}");

            Console.WriteLine("\nVerbs (similar context - ending sentences):");
            Console.WriteLine($"  'jumps' <-> 'sleeps': {calculator.CosineSimilarity("jumps", "sleeps"):F4}");
        }

        public static void DemonstrateNamedEntityContext()
        {
            Console.WriteLine("\n=== Named Entity Recognition Context ===\n");

            string[] documents = new string[]
            {
                "John lives in New York.",
                "Mary works in New York.",
                "New York is a city.",
                "John and Mary are friends.",
                "Paris is in France.",
                "London is in England."
            };

            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindowsFromMultipleDocs(documents.ToList());

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 3);

            var calculator = new WordSimilarityCalculator(model);

            Console.WriteLine("Entities in similar contexts have similar embeddings:");
            Console.WriteLine("(Useful for Named Entity Recognition)\n");

            Console.WriteLine("People (names in similar contexts):");
            Console.WriteLine($"  'john' <-> 'mary': {calculator.CosineSimilarity("john", "mary"):F4}");

            Console.WriteLine("\nLocations (place names in similar contexts):");
            Console.WriteLine($"  'york' <-> 'paris': {calculator.CosineSimilarity("york", "paris"):F4}");
            Console.WriteLine($"  'new' <-> 'france': {calculator.CosineSimilarity("new", "france"):F4}");

            Console.WriteLine("\nMost similar to 'john' (other people):");
            var similar = calculator.FindMostSimilarWords("john", topN: 3);
            foreach (var (word, sim) in similar)
            {
                Console.WriteLine($"  {word}: {sim:F4}");
            }
        }

        public static void DemonstrateTextClassificationContext()
        {
            Console.WriteLine("\n=== Text Classification via Context ===\n");

            string[] sportsDocuments = new string[]
            {
                "the team won the championship game.",
                "the player scored an amazing goal.",
                "the coach led the team to victory.",
                "the championship was exciting."
            };

            string[] businessDocuments = new string[]
            {
                "the company announced record profits.",
                "the executive made a strategic decision.",
                "the market responded positively to the news.",
                "the investment was profitable."
            };

            var allDocs = sportsDocuments.Concat(businessDocuments).ToList();
            var windowGenerator = new ContextWindowGenerator(windowSize: 2);
            var windows = windowGenerator.GenerateContextWindowsFromMultipleDocs(allDocs);

            var model = new CBOWModel(embeddingDimension: 50, learningRate: 0.01);
            model.InitializeFromContextWindows(windows);
            model.Train(windows, epochs: 3);

            var calculator = new WordSimilarityCalculator(model);

            Console.WriteLine("Domain-specific words cluster based on context:");
            Console.WriteLine("-".PadRight(50, '-'));

            Console.WriteLine("\nSports domain words:");
            Console.WriteLine($"  'team' <-> 'player': {calculator.CosineSimilarity("team", "player"):F4}");
            Console.WriteLine($"  'won' <-> 'victory': {calculator.CosineSimilarity("won", "victory"):F4}");

            Console.WriteLine("\nBusiness domain words:");
            Console.WriteLine($"  'company' <-> 'executive': {calculator.CosineSimilarity("company", "executive"):F4}");
            Console.WriteLine($"  'profitable' <-> 'profits': {calculator.CosineSimilarity("profitable", "profits"):F4}");

            Console.WriteLine("\nCross-domain (should be less similar):");
            Console.WriteLine($"  'team' <-> 'company': {calculator.CosineSimilarity("team", "company"):F4}");
        }
    }
}
