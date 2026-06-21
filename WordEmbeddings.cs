using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NLPToolkit
{
    /// <summary>
    /// Word2Vec Skip-gram model for learning word embeddings
    /// Predicts context words given a target word
    /// </summary>
    public class Word2VecSkipGram
    {
        public int EmbeddingDim { get; set; }
        public int ContextWindow { get; set; }
        public double LearningRate { get; set; }
        public Dictionary<string, double[]> WordEmbeddings { get; set; }
        public Dictionary<string, double[]> ContextEmbeddings { get; set; }
        private Dictionary<string, int> vocabulary;

        public Word2VecSkipGram(int embeddingDim = 100, int contextWindow = 5, double learningRate = 0.025)
        {
            EmbeddingDim = embeddingDim;
            ContextWindow = contextWindow;
            LearningRate = learningRate;
            WordEmbeddings = new Dictionary<string, double[]>();
            ContextEmbeddings = new Dictionary<string, double[]>();
            vocabulary = new Dictionary<string, int>();
        }

        public void BuildVocabulary(List<List<string>> sentences)
        {
            vocabulary.Clear();
            int index = 0;
            foreach (var sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    if (!vocabulary.ContainsKey(word))
                    {
                        vocabulary[word] = index++;
                    }
                }
            }

            // Initialize embeddings
            Random rand = new Random(42);
            foreach (var word in vocabulary.Keys)
            {
                WordEmbeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();

                ContextEmbeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();
            }
        }

        public void Train(List<List<string>> sentences, int epochs = 5)
        {
            BuildVocabulary(sentences);

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                foreach (var sentence in sentences)
                {
                    for (int i = 0; i < sentence.Count; i++)
                    {
                        string targetWord = sentence[i];
                        if (!WordEmbeddings.ContainsKey(targetWord))
                            continue;

                        // Get context words
                        int contextStart = Math.Max(0, i - ContextWindow);
                        int contextEnd = Math.Min(sentence.Count, i + ContextWindow + 1);

                        for (int j = contextStart; j < contextEnd; j++)
                        {
                            if (j == i)
                                continue;

                            string contextWord = sentence[j];
                            if (!WordEmbeddings.ContainsKey(contextWord))
                                continue;

                            // Compute loss and update embeddings
                            double[] targetVec = WordEmbeddings[targetWord];
                            double[] contextVec = ContextEmbeddings[contextWord];

                            double dotProduct = 0;
                            for (int k = 0; k < EmbeddingDim; k++)
                                dotProduct += targetVec[k] * contextVec[k];

                            double sigmoid = 1.0 / (1.0 + Math.Exp(-dotProduct));
                            double error = sigmoid - 1.0; // Target is 1 for positive pairs
                            totalLoss += error * error;

                            // Update embeddings
                            for (int k = 0; k < EmbeddingDim; k++)
                            {
                                targetVec[k] -= LearningRate * error * contextVec[k];
                                contextVec[k] -= LearningRate * error * targetVec[k];
                            }
                        }
                    }
                }
            }
        }

        public double CosineSimilarity(string word1, string word2)
        {
            if (!WordEmbeddings.ContainsKey(word1) || !WordEmbeddings.ContainsKey(word2))
                return 0;

            double[] vec1 = WordEmbeddings[word1];
            double[] vec2 = WordEmbeddings[word2];

            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < EmbeddingDim; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        public List<(string, double)> GetSimilarWords(string word, int topK = 5)
        {
            if (!WordEmbeddings.ContainsKey(word))
                return new List<(string, double)>();

            return WordEmbeddings.Keys
                .Where(w => w != word)
                .Select(w => (w, CosineSimilarity(word, w)))
                .OrderByDescending(x => x.Item2)
                .Take(topK)
                .ToList();
        }
    }

    /// <summary>
    /// Continuous Bag-of-Words (CBOW) model for word embeddings
    /// Predicts target word from context words
    /// </summary>
    public class Word2VecCBOW
    {
        public int EmbeddingDim { get; set; }
        public int ContextWindow { get; set; }
        public double LearningRate { get; set; }
        public Dictionary<string, double[]> WordEmbeddings { get; set; }
        public Dictionary<string, double[]> OutputEmbeddings { get; set; }
        private Dictionary<string, int> vocabulary;

        public Word2VecCBOW(int embeddingDim = 100, int contextWindow = 5, double learningRate = 0.025)
        {
            EmbeddingDim = embeddingDim;
            ContextWindow = contextWindow;
            LearningRate = learningRate;
            WordEmbeddings = new Dictionary<string, double[]>();
            OutputEmbeddings = new Dictionary<string, double[]>();
            vocabulary = new Dictionary<string, int>();
        }

        public void BuildVocabulary(List<List<string>> sentences)
        {
            vocabulary.Clear();
            int index = 0;
            foreach (var sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    if (!vocabulary.ContainsKey(word))
                    {
                        vocabulary[word] = index++;
                    }
                }
            }

            Random rand = new Random(42);
            foreach (var word in vocabulary.Keys)
            {
                WordEmbeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();

                OutputEmbeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();
            }
        }

        public void Train(List<List<string>> sentences, int epochs = 5)
        {
            BuildVocabulary(sentences);

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                foreach (var sentence in sentences)
                {
                    for (int i = 0; i < sentence.Count; i++)
                    {
                        string targetWord = sentence[i];
                        if (!WordEmbeddings.ContainsKey(targetWord))
                            continue;

                        // Get context words
                        int contextStart = Math.Max(0, i - ContextWindow);
                        int contextEnd = Math.Min(sentence.Count, i + ContextWindow + 1);

                        List<string> contextWords = new List<string>();
                        for (int j = contextStart; j < contextEnd; j++)
                        {
                            if (j != i && WordEmbeddings.ContainsKey(sentence[j]))
                                contextWords.Add(sentence[j]);
                        }

                        if (contextWords.Count == 0)
                            continue;

                        // Average context embeddings
                        double[] avgContext = new double[EmbeddingDim];
                        foreach (var contextWord in contextWords)
                        {
                            for (int k = 0; k < EmbeddingDim; k++)
                                avgContext[k] += WordEmbeddings[contextWord][k];
                        }
                        for (int k = 0; k < EmbeddingDim; k++)
                            avgContext[k] /= contextWords.Count;

                        // Compute error
                        double[] targetVec = OutputEmbeddings[targetWord];
                        double dotProduct = 0;
                        for (int k = 0; k < EmbeddingDim; k++)
                            dotProduct += avgContext[k] * targetVec[k];

                        double sigmoid = 1.0 / (1.0 + Math.Exp(-dotProduct));
                        double error = sigmoid - 1.0;

                        // Update embeddings
                        for (int k = 0; k < EmbeddingDim; k++)
                            targetVec[k] -= LearningRate * error * avgContext[k];

                        foreach (var contextWord in contextWords)
                        {
                            for (int k = 0; k < EmbeddingDim; k++)
                                WordEmbeddings[contextWord][k] -= LearningRate * error * targetVec[k];
                        }
                    }
                }
            }
        }

        public double CosineSimilarity(string word1, string word2)
        {
            if (!WordEmbeddings.ContainsKey(word1) || !WordEmbeddings.ContainsKey(word2))
                return 0;

            double[] vec1 = WordEmbeddings[word1];
            double[] vec2 = WordEmbeddings[word2];

            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < EmbeddingDim; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }

    /// <summary>
    /// GloVe (Global Vectors for Word Representation)
    /// Combines global matrix factorization with local context window methods
    /// </summary>
    public class GloVeEmbedding
    {
        public int EmbeddingDim { get; set; }
        public int ContextWindow { get; set; }
        public double LearningRate { get; set; }
        public Dictionary<string, double[]> Embeddings { get; set; }
        public Dictionary<(string, string), int> CooccurrenceMatrix { get; set; }
        private Dictionary<string, int> vocabulary;

        public GloVeEmbedding(int embeddingDim = 100, int contextWindow = 5, double learningRate = 0.05)
        {
            EmbeddingDim = embeddingDim;
            ContextWindow = contextWindow;
            LearningRate = learningRate;
            Embeddings = new Dictionary<string, double[]>();
            CooccurrenceMatrix = new Dictionary<(string, string), int>();
            vocabulary = new Dictionary<string, int>();
        }

        public void BuildVocabulary(List<List<string>> sentences)
        {
            vocabulary.Clear();
            int index = 0;
            foreach (var sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    if (!vocabulary.ContainsKey(word))
                    {
                        vocabulary[word] = index++;
                    }
                }
            }

            Random rand = new Random(42);
            foreach (var word in vocabulary.Keys)
            {
                Embeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();
            }
        }

        public void BuildCooccurrenceMatrix(List<List<string>> sentences)
        {
            CooccurrenceMatrix.Clear();

            foreach (var sentence in sentences)
            {
                for (int i = 0; i < sentence.Count; i++)
                {
                    int contextStart = Math.Max(0, i - ContextWindow);
                    int contextEnd = Math.Min(sentence.Count, i + ContextWindow + 1);

                    for (int j = contextStart; j < contextEnd; j++)
                    {
                        if (i != j)
                        {
                            var key = (sentence[i], sentence[j]);
                            if (!CooccurrenceMatrix.ContainsKey(key))
                                CooccurrenceMatrix[key] = 0;
                            CooccurrenceMatrix[key]++;
                        }
                    }
                }
            }
        }

        public void Train(List<List<string>> sentences, int epochs = 5)
        {
            BuildVocabulary(sentences);
            BuildCooccurrenceMatrix(sentences);

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                foreach (var pair in CooccurrenceMatrix)
                {
                    string word1 = pair.Key.Item1;
                    string word2 = pair.Key.Item2;
                    int count = pair.Value;

                    if (!Embeddings.ContainsKey(word1) || !Embeddings.ContainsKey(word2))
                        continue;

                    double[] vec1 = Embeddings[word1];
                    double[] vec2 = Embeddings[word2];

                    // Compute weighted loss
                    double dotProduct = 0;
                    for (int k = 0; k < EmbeddingDim; k++)
                        dotProduct += vec1[k] * vec2[k];

                    double weightFactor = Math.Min(1.0, Math.Pow(count / 100.0, 0.75));
                    double error = weightFactor * (dotProduct - Math.Log(count + 1));

                    // Update embeddings
                    for (int k = 0; k < EmbeddingDim; k++)
                    {
                        vec1[k] -= LearningRate * error * vec2[k];
                        vec2[k] -= LearningRate * error * vec1[k];
                    }
                }
            }
        }

        public double CosineSimilarity(string word1, string word2)
        {
            if (!Embeddings.ContainsKey(word1) || !Embeddings.ContainsKey(word2))
                return 0;

            double[] vec1 = Embeddings[word1];
            double[] vec2 = Embeddings[word2];

            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < EmbeddingDim; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }

    /// <summary>
    /// Pre-trained embedding wrapper for transfer learning
    /// Loads and uses pre-computed word embeddings
    /// </summary>
    public class PretrainedEmbeddings
    {
        public Dictionary<string, double[]> Embeddings { get; set; }
        public int EmbeddingDim { get; set; }

        public PretrainedEmbeddings(int embeddingDim = 100)
        {
            Embeddings = new Dictionary<string, double[]>();
            EmbeddingDim = embeddingDim;
        }

        public void LoadEmbeddings(Dictionary<string, double[]> embeddingDict)
        {
            Embeddings = embeddingDict;
            if (Embeddings.Count > 0)
                EmbeddingDim = Embeddings.First().Value.Length;
        }

        public double[] GetEmbedding(string word)
        {
            if (Embeddings.ContainsKey(word))
                return Embeddings[word];

            // Return zero vector for unknown words
            return new double[EmbeddingDim];
        }

        public double[] GetSentenceEmbedding(List<string> words)
        {
            double[] sentenceVec = new double[EmbeddingDim];

            foreach (var word in words)
            {
                double[] wordVec = GetEmbedding(word);
                for (int i = 0; i < EmbeddingDim; i++)
                    sentenceVec[i] += wordVec[i];
            }

            // Average pooling
            for (int i = 0; i < EmbeddingDim; i++)
                sentenceVec[i] /= words.Count;

            return sentenceVec;
        }

        public double CosineSimilarity(string word1, string word2)
        {
            double[] vec1 = GetEmbedding(word1);
            double[] vec2 = GetEmbedding(word2);

            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < EmbeddingDim; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        public List<(string, double)> FindMostSimilar(string word, int topK = 5)
        {
            return Embeddings.Keys
                .Where(w => w != word)
                .Select(w => (w, CosineSimilarity(word, w)))
                .OrderByDescending(x => x.Item2)
                .Take(topK)
                .ToList();
        }
    }

    /// <summary>
    /// Text classifier using pre-trained embeddings
    /// Demonstrates transfer learning application
    /// </summary>
    public class EmbeddingBasedClassifier
    {
        public PretrainedEmbeddings Embeddings { get; set; }
        public Dictionary<string, double[]> ClassCentroids { get; set; }

        public EmbeddingBasedClassifier(PretrainedEmbeddings embeddings)
        {
            Embeddings = embeddings;
            ClassCentroids = new Dictionary<string, double[]>();
        }

        public void Train(List<(List<string>, string)> trainingData)
        {
            // Group by class
            var groupedByClass = trainingData.GroupBy(x => x.Item2);

            foreach (var classGroup in groupedByClass)
            {
                string className = classGroup.Key;
                double[] centroid = new double[Embeddings.EmbeddingDim];

                // Compute centroid for each class
                foreach (var (words, _) in classGroup)
                {
                    double[] sentenceVec = Embeddings.GetSentenceEmbedding(words);
                    for (int i = 0; i < Embeddings.EmbeddingDim; i++)
                        centroid[i] += sentenceVec[i];
                }

                // Normalize
                for (int i = 0; i < Embeddings.EmbeddingDim; i++)
                    centroid[i] /= classGroup.Count();

                ClassCentroids[className] = centroid;
            }
        }

        public string Predict(List<string> words)
        {
            double[] sentenceVec = Embeddings.GetSentenceEmbedding(words);

            string bestClass = null;
            double bestSimilarity = double.MinValue;

            foreach (var classEntry in ClassCentroids)
            {
                double similarity = ComputeCosineSimilarity(sentenceVec, classEntry.Value);
                if (similarity > bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestClass = classEntry.Key;
                }
            }

            return bestClass;
        }

        private double ComputeCosineSimilarity(double[] vec1, double[] vec2)
        {
            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                dotProduct += vec1[i] * vec2[i];
                norm1 += vec1[i] * vec1[i];
                norm2 += vec2[i] * vec2[i];
            }

            if (norm1 == 0 || norm2 == 0)
                return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }

    /// <summary>
    /// Example usage of embeddings and Word2Vec models
    /// </summary>
    public class EmbeddingExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Word2Vec Skip-gram Model ===");
            var skipGram = new Word2VecSkipGram(embeddingDim: 50, contextWindow: 3);

            var sentences = new List<List<string>>
            {
                new List<string> { "the", "dog", "is", "running", "in", "the", "park" },
                new List<string> { "the", "cat", "is", "sleeping", "on", "the", "bed" },
                new List<string> { "dogs", "and", "cats", "are", "common", "pets" },
                new List<string> { "the", "park", "is", "a", "beautiful", "place" }
            };

            skipGram.Train(sentences, epochs: 10);

            Console.WriteLine("Skip-gram: Words similar to 'dog':");
            foreach (var (word, similarity) in skipGram.GetSimilarWords("dog", topK: 3))
            {
                Console.WriteLine($"  {word}: {similarity:F4}");
            }

            Console.WriteLine("\nSkip-gram: Cosine similarity between 'dog' and 'cat': "
                + skipGram.CosineSimilarity("dog", "cat"):F4);

            Console.WriteLine("\n=== CBOW Model ===");
            var cbow = new Word2VecCBOW(embeddingDim: 50, contextWindow: 3);
            cbow.Train(sentences, epochs: 10);

            Console.WriteLine("CBOW: Words similar to 'park':");
            foreach (var (word, sim) in cbow.WordEmbeddings.Keys
                .Where(w => w != "park")
                .Select(w => (w, cbow.CosineSimilarity("park", w)))
                .OrderByDescending(x => x.Item2)
                .Take(3))
            {
                Console.WriteLine($"  {word}: {sim:F4}");
            }

            Console.WriteLine("\n=== GloVe Model ===");
            var glove = new GloVeEmbedding(embeddingDim: 50, contextWindow: 3);
            glove.Train(sentences, epochs: 10);

            Console.WriteLine("GloVe: Similarity between 'the' and 'a': "
                + glove.CosineSimilarity("the", "a"):F4);

            Console.WriteLine("\n=== Transfer Learning with Pre-trained Embeddings ===");
            var pretrained = new PretrainedEmbeddings(embeddingDim: 50);
            pretrained.LoadEmbeddings(skipGram.WordEmbeddings);

            var trainingData = new List<(List<string>, string)>
            {
                (new List<string> { "the", "dog", "is", "running" }, "positive"),
                (new List<string> { "the", "cat", "is", "sleeping" }, "positive"),
                (new List<string> { "bad", "movie", "is", "terrible" }, "negative"),
                (new List<string> { "good", "food", "is", "delicious" }, "positive")
            };

            var classifier = new EmbeddingBasedClassifier(pretrained);
            classifier.Train(trainingData);

            var testSentence = new List<string> { "the", "dog", "is", "nice" };
            string prediction = classifier.Predict(testSentence);
            Console.WriteLine($"Text classification: '{string.Join(" ", testSentence)}' -> {prediction}");

            Console.WriteLine("\n=== Word Analogies ===");
            Console.WriteLine("Finding analogies (king - man + woman ≈ queen):");
            var kingVec = skipGram.WordEmbeddings["park"];
            Console.WriteLine("  (Simplified example - full analogies require more sophisticated setup)");

            Console.WriteLine("\n=== Advantages of Embeddings ===");
            Console.WriteLine("- Capture semantic relationships between words");
            Console.WriteLine("- Enable transfer learning across NLP tasks");
            Console.WriteLine("- Reduce dimensionality compared to one-hot encoding");
            Console.WriteLine("- Improve performance on downstream tasks");
            Console.WriteLine("- Enable similarity and analogy computations");
            Console.WriteLine("- Support both supervised and unsupervised learning");

            Console.WriteLine("\n=== Applications ===");
            Console.WriteLine("1. Text Classification: Sentiment analysis, spam detection");
            Console.WriteLine("2. Machine Translation: Better word alignment and context understanding");
            Console.WriteLine("3. Question Answering: Finding relevant passages using semantic similarity");
            Console.WriteLine("4. Word Similarity: Finding synonyms and related words");
            Console.WriteLine("5. Document Clustering: Grouping similar documents");
            Console.WriteLine("6. Recommendation Systems: Finding similar items and users");
            Console.WriteLine("7. Named Entity Recognition: Entity type prediction");
            Console.WriteLine("8. Information Retrieval: Semantic search and ranking");
        }
    }
}
