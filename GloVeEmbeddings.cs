using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Co-occurrence matrix for GloVe embeddings
    /// Tracks how often words appear together in context
    /// </summary>
    public class CooccurrenceMatrix
    {
        public Dictionary<(string, string), int> CooccurrenceCounts { get; set; }
        public Dictionary<string, int> WordFrequency { get; set; }
        public int ContextWindow { get; set; }
        public int TotalCooccurrences { get; set; }

        public CooccurrenceMatrix(int contextWindow = 5)
        {
            ContextWindow = contextWindow;
            CooccurrenceCounts = new Dictionary<(string, string), int>();
            WordFrequency = new Dictionary<string, int>();
            TotalCooccurrences = 0;
        }

        public void BuildMatrix(List<List<string>> sentences)
        {
            CooccurrenceCounts.Clear();
            WordFrequency.Clear();
            TotalCooccurrences = 0;

            foreach (var sentence in sentences)
            {
                // Track word frequency
                foreach (var word in sentence)
                {
                    if (!WordFrequency.ContainsKey(word))
                        WordFrequency[word] = 0;
                    WordFrequency[word]++;
                }

                // Build co-occurrence matrix
                for (int i = 0; i < sentence.Count; i++)
                {
                    int contextStart = Math.Max(0, i - ContextWindow);
                    int contextEnd = Math.Min(sentence.Count, i + ContextWindow + 1);

                    for (int j = contextStart; j < contextEnd; j++)
                    {
                        if (i != j)
                        {
                            var key = (sentence[i], sentence[j]);
                            if (!CooccurrenceCounts.ContainsKey(key))
                                CooccurrenceCounts[key] = 0;
                            CooccurrenceCounts[key]++;
                            TotalCooccurrences++;
                        }
                    }
                }
            }
        }

        public double GetProbability(string word1, string word2)
        {
            var key = (word1, word2);
            if (CooccurrenceCounts.ContainsKey(key))
                return (double)CooccurrenceCounts[key] / TotalCooccurrences;
            return 0;
        }

        public int GetCount(string word1, string word2)
        {
            var key = (word1, word2);
            return CooccurrenceCounts.ContainsKey(key) ? CooccurrenceCounts[key] : 0;
        }
    }

    /// <summary>
    /// GloVe (Global Vectors for Word Representation) embeddings
    /// Combines global matrix factorization with local context windows
    /// </summary>
    public class GloVeEmbedding
    {
        public int EmbeddingDim { get; set; }
        public int ContextWindow { get; set; }
        public double LearningRate { get; set; }
        public double Xmax { get; set; }
        public double Alpha { get; set; }

        public Dictionary<string, double[]> Embeddings { get; set; }
        public Dictionary<string, double[]> ContextEmbeddings { get; set; }
        public Dictionary<string, double> BiasW { get; set; }
        public Dictionary<string, double> BiasC { get; set; }
        public CooccurrenceMatrix CooccurrenceMatrix { get; set; }
        public Dictionary<string, int> Vocabulary { get; set; }

        public GloVeEmbedding(int embeddingDim = 100, int contextWindow = 5, double learningRate = 0.05,
            double xmax = 100, double alpha = 0.75)
        {
            EmbeddingDim = embeddingDim;
            ContextWindow = contextWindow;
            LearningRate = learningRate;
            Xmax = xmax;
            Alpha = alpha;

            Embeddings = new Dictionary<string, double[]>();
            ContextEmbeddings = new Dictionary<string, double[]>();
            BiasW = new Dictionary<string, double>();
            BiasC = new Dictionary<string, double>();
            CooccurrenceMatrix = new CooccurrenceMatrix(contextWindow);
            Vocabulary = new Dictionary<string, int>();
        }

        private double WeightingFunction(int cooccurrenceCount)
        {
            // f(X) = (X/Xmax)^alpha if X < Xmax, else 1.0
            if (cooccurrenceCount >= Xmax)
                return 1.0;
            return Math.Pow(cooccurrenceCount / Xmax, Alpha);
        }

        public void BuildVocabulary(List<List<string>> sentences)
        {
            Vocabulary.Clear();
            int index = 0;

            foreach (var sentence in sentences)
            {
                foreach (var word in sentence)
                {
                    if (!Vocabulary.ContainsKey(word))
                    {
                        Vocabulary[word] = index++;
                    }
                }
            }

            // Initialize embeddings
            Random rand = new Random(42);
            foreach (var word in Vocabulary.Keys)
            {
                Embeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();

                ContextEmbeddings[word] = Enumerable.Range(0, EmbeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) / EmbeddingDim)
                    .ToArray();

                BiasW[word] = 0;
                BiasC[word] = 0;
            }
        }

        public void Train(List<List<string>> sentences, int epochs = 5)
        {
            BuildVocabulary(sentences);
            CooccurrenceMatrix.BuildMatrix(sentences);

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;
                int updateCount = 0;

                foreach (var pair in CooccurrenceMatrix.CooccurrenceCounts)
                {
                    string word1 = pair.Key.Item1;
                    string word2 = pair.Key.Item2;
                    int cooccurrenceCount = pair.Value;

                    if (!Embeddings.ContainsKey(word1) || !Embeddings.ContainsKey(word2))
                        continue;

                    double[] vec1 = Embeddings[word1];
                    double[] vec2 = ContextEmbeddings[word2];

                    // Compute dot product
                    double dotProduct = 0;
                    for (int k = 0; k < EmbeddingDim; k++)
                        dotProduct += vec1[k] * vec2[k];

                    dotProduct += BiasW[word1] + BiasC[word2];

                    // Log co-occurrence
                    double logCooccurrence = Math.Log(cooccurrenceCount);

                    // Weighting function
                    double weight = WeightingFunction(cooccurrenceCount);

                    // Loss: weight * (dotProduct - log(Xij))^2
                    double diff = dotProduct - logCooccurrence;
                    double loss = weight * diff * diff;
                    totalLoss += loss;

                    // Gradient computation
                    double gradient = 2 * weight * diff;

                    // Update embeddings
                    for (int k = 0; k < EmbeddingDim; k++)
                    {
                        vec1[k] -= LearningRate * gradient * vec2[k];
                        vec2[k] -= LearningRate * gradient * vec1[k];
                    }

                    // Update biases
                    BiasW[word1] -= LearningRate * gradient;
                    BiasC[word2] -= LearningRate * gradient;

                    updateCount++;
                }

                if (epoch % 2 == 0)
                    Console.WriteLine($"Epoch {epoch}: Loss = {totalLoss / updateCount:F4}");
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

        public List<(string, double)> GetSimilarWords(string word, int topK = 5)
        {
            if (!Embeddings.ContainsKey(word))
                return new List<(string, double)>();

            return Embeddings.Keys
                .Where(w => w != word)
                .Select(w => (w, CosineSimilarity(word, w)))
                .OrderByDescending(x => x.Item2)
                .Take(topK)
                .ToList();
        }

        public bool TestAnalogy(string a, string b, string c, string d, double threshold = 0.5)
        {
            // Test if a:b :: c:d (a is to b as c is to d)
            // Vector relationship: b - a ≈ d - c
            if (!Embeddings.ContainsKey(a) || !Embeddings.ContainsKey(b) ||
                !Embeddings.ContainsKey(c) || !Embeddings.ContainsKey(d))
                return false;

            double[] vecA = Embeddings[a];
            double[] vecB = Embeddings[b];
            double[] vecC = Embeddings[c];
            double[] vecD = Embeddings[d];

            // Compute relationship vectors
            double[] relAB = new double[EmbeddingDim];
            double[] relCD = new double[EmbeddingDim];

            for (int i = 0; i < EmbeddingDim; i++)
            {
                relAB[i] = vecB[i] - vecA[i];
                relCD[i] = vecD[i] - vecC[i];
            }

            // Compute similarity between relationships
            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < EmbeddingDim; i++)
            {
                dotProduct += relAB[i] * relCD[i];
                norm1 += relAB[i] * relAB[i];
                norm2 += relCD[i] * relCD[i];
            }

            double similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2) + 1e-10);
            return similarity > threshold;
        }

        public string FindAnalogy(string a, string b, string c)
        {
            // Find d such that a:b :: c:d
            // d_vector = c_vector + (b_vector - a_vector)
            if (!Embeddings.ContainsKey(a) || !Embeddings.ContainsKey(b) || !Embeddings.ContainsKey(c))
                return null;

            double[] vecA = Embeddings[a];
            double[] vecB = Embeddings[b];
            double[] vecC = Embeddings[c];

            double[] targetVec = new double[EmbeddingDim];
            for (int i = 0; i < EmbeddingDim; i++)
                targetVec[i] = vecC[i] + (vecB[i] - vecA[i]);

            // Find closest word
            string bestMatch = null;
            double bestSim = -1;

            foreach (var word in Embeddings.Keys)
            {
                if (word == a || word == b || word == c)
                    continue;

                double[] vec = Embeddings[word];
                double dotProduct = 0, norm1 = 0, norm2 = 0;

                for (int i = 0; i < EmbeddingDim; i++)
                {
                    dotProduct += targetVec[i] * vec[i];
                    norm1 += targetVec[i] * targetVec[i];
                    norm2 += vec[i] * vec[i];
                }

                double similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));

                if (similarity > bestSim)
                {
                    bestSim = similarity;
                    bestMatch = word;
                }
            }

            return bestMatch;
        }

        public double[] GetEmbedding(string word)
        {
            return Embeddings.ContainsKey(word) ? (double[])Embeddings[word].Clone() : null;
        }
    }

    /// <summary>
    /// GloVe-based text classifier
    /// </summary>
    public class GloVeTextClassifier
    {
        public GloVeEmbedding GloVe { get; set; }
        public List<string> Classes { get; set; }
        public Dictionary<string, double[]> ClassCentroids { get; set; }

        public GloVeTextClassifier(int embeddingDim = 100)
        {
            GloVe = new GloVeEmbedding(embeddingDim: embeddingDim);
            Classes = new List<string>();
            ClassCentroids = new Dictionary<string, double[]>();
        }

        public void Train(List<(List<string>, string)> trainingData)
        {
            // Build vocabulary and train GloVe
            var sentences = trainingData.Select(x => x.Item1).ToList();
            GloVe.Train(sentences, epochs: 5);

            // Compute class centroids
            Classes = trainingData.Select(x => x.Item2).Distinct().ToList();

            foreach (var className in Classes)
            {
                var classTexts = trainingData.Where(x => x.Item2 == className).Select(x => x.Item1).ToList();
                var centroid = new double[GloVe.EmbeddingDim];

                foreach (var text in classTexts)
                {
                    double[] textVec = GetSentenceEmbedding(text);
                    for (int i = 0; i < GloVe.EmbeddingDim; i++)
                        centroid[i] += textVec[i];
                }

                for (int i = 0; i < GloVe.EmbeddingDim; i++)
                    centroid[i] /= classTexts.Count;

                ClassCentroids[className] = centroid;
            }
        }

        public double[] GetSentenceEmbedding(List<string> words)
        {
            var embedding = new double[GloVe.EmbeddingDim];
            int count = 0;

            foreach (var word in words)
            {
                if (GloVe.Embeddings.ContainsKey(word))
                {
                    for (int i = 0; i < GloVe.EmbeddingDim; i++)
                        embedding[i] += GloVe.Embeddings[word][i];
                    count++;
                }
            }

            if (count > 0)
            {
                for (int i = 0; i < GloVe.EmbeddingDim; i++)
                    embedding[i] /= count;
            }

            return embedding;
        }

        public string Predict(List<string> text)
        {
            double[] textVec = GetSentenceEmbedding(text);

            string bestClass = null;
            double bestSimilarity = -1;

            foreach (var className in Classes)
            {
                double similarity = CosineSimilarity(textVec, ClassCentroids[className]);
                if (similarity > bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestClass = className;
                }
            }

            return bestClass;
        }

        private double CosineSimilarity(double[] vec1, double[] vec2)
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
    /// GloVe-based machine translation support
    /// </summary>
    public class GloVeTranslationHelper
    {
        public GloVeEmbedding SourceGloVe { get; set; }
        public GloVeEmbedding TargetGloVe { get; set; }
        public double[][] AlignmentMatrix { get; set; }

        public GloVeTranslationHelper(int embeddingDim = 100)
        {
            SourceGloVe = new GloVeEmbedding(embeddingDim: embeddingDim);
            TargetGloVe = new GloVeEmbedding(embeddingDim: embeddingDim);
        }

        public void Train(List<List<string>> sourceTexts, List<List<string>> targetTexts)
        {
            SourceGloVe.Train(sourceTexts, epochs: 5);
            TargetGloVe.Train(targetTexts, epochs: 5);

            // Compute alignment matrix (simplified approach)
            int vocabSize = Math.Max(SourceGloVe.Vocabulary.Count, TargetGloVe.Vocabulary.Count);
            AlignmentMatrix = new double[vocabSize][];
            for (int i = 0; i < vocabSize; i++)
                AlignmentMatrix[i] = new double[vocabSize];
        }

        public List<string> TranslateWords(List<string> sourceWords)
        {
            var translation = new List<string>();

            foreach (var word in sourceWords)
            {
                if (SourceGloVe.Embeddings.ContainsKey(word))
                {
                    double[] sourceVec = SourceGloVe.Embeddings[word];

                    // Find most similar word in target language
                    string bestTarget = null;
                    double bestSim = -1;

                    foreach (var targetWord in TargetGloVe.Embeddings.Keys)
                    {
                        double[] targetVec = TargetGloVe.Embeddings[targetWord];
                        double sim = CosineSimilarity(sourceVec, targetVec);

                        if (sim > bestSim)
                        {
                            bestSim = sim;
                            bestTarget = targetWord;
                        }
                    }

                    if (bestTarget != null)
                        translation.Add(bestTarget);
                }
            }

            return translation;
        }

        private double CosineSimilarity(double[] vec1, double[] vec2)
        {
            double dotProduct = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < Math.Min(vec1.Length, vec2.Length); i++)
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
    /// GloVe embeddings examples
    /// </summary>
    public class GloVeEmbeddingExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== GloVe (Global Vectors) Embeddings ===");
            var glove = new GloVeEmbedding(embeddingDim: 50, contextWindow: 3);

            var sentences = new List<List<string>>
            {
                new List<string> { "the", "dog", "is", "running", "in", "the", "park" },
                new List<string> { "the", "cat", "is", "sleeping", "on", "the", "bed" },
                new List<string> { "dogs", "and", "cats", "are", "common", "pets" },
                new List<string> { "the", "park", "is", "a", "beautiful", "place" },
                new List<string> { "the", "dog", "loves", "to", "play", "fetch" },
                new List<string> { "the", "cat", "enjoys", "playing", "with", "toys" }
            };

            Console.WriteLine("Training GloVe embeddings...");
            glove.Train(sentences, epochs: 5);

            Console.WriteLine("\n=== Word Similarity ===");
            Console.WriteLine("Words similar to 'dog':");
            foreach (var (word, similarity) in glove.GetSimilarWords("dog", topK: 3))
                Console.WriteLine($"  {word}: {similarity:F4}");

            Console.WriteLine("\nWords similar to 'park':");
            foreach (var (word, similarity) in glove.GetSimilarWords("park", topK: 3))
                Console.WriteLine($"  {word}: {similarity:F4}");

            Console.WriteLine("\n=== Word Analogies ===");
            Console.WriteLine("Testing analogies:");
            bool analogy1 = glove.TestAnalogy("dog", "dogs", "cat", "cats");
            Console.WriteLine($"dog:dogs :: cat:cats = {analogy1}");

            string result = glove.FindAnalogy("dog", "dogs", "cat");
            Console.WriteLine($"If dog:dogs :: cat:? then ? = {result}");

            Console.WriteLine("\n=== GloVe Text Classification ===");
            var classifier = new GloVeTextClassifier(embeddingDim: 50);

            var trainingData = new List<(List<string>, string)>
            {
                (new List<string> { "the", "dog", "is", "running" }, "animals"),
                (new List<string> { "the", "cat", "is", "sleeping" }, "animals"),
                (new List<string> { "the", "park", "is", "beautiful" }, "places"),
                (new List<string> { "the", "bed", "is", "comfortable" }, "places"),
            };

            Console.WriteLine("Training GloVe-based classifier...");
            classifier.Train(trainingData);

            var testTexts = new List<(List<string>, string)>
            {
                (new List<string> { "dog", "running" }, "animals"),
                (new List<string> { "park", "beautiful" }, "places"),
                (new List<string> { "cat", "playing" }, "animals"),
            };

            Console.WriteLine("Classification Results:");
            foreach (var (text, actualClass) in testTexts)
            {
                string prediction = classifier.Predict(text);
                Console.WriteLine($"Text: {string.Join(" ", text)}");
                Console.WriteLine($"  Predicted: {prediction}, Actual: {actualClass}");
            }

            Console.WriteLine("\n=== GloVe Machine Translation ===");
            var enSentences = new List<List<string>>
            {
                new List<string> { "hello", "world", "good", "morning" },
                new List<string> { "how", "are", "you", "today" }
            };

            var frSentences = new List<List<string>>
            {
                new List<string> { "bonjour", "monde", "bon", "matin" },
                new List<string> { "comment", "allez", "vous", "aujourd" }
            };

            var translator = new GloVeTranslationHelper(embeddingDim: 50);
            Console.WriteLine("Training translation helper...");
            translator.Train(enSentences, frSentences);

            var englishWords = new List<string> { "hello", "world" };
            var translation = translator.TranslateWords(englishWords);
            Console.WriteLine($"English: {string.Join(" ", englishWords)}");
            Console.WriteLine($"French: {string.Join(" ", translation)}");

            Console.WriteLine("\n=== GloVe vs Word2Vec ===");
            Console.WriteLine("GloVe Advantages:");
            Console.WriteLine("- Uses global co-occurrence statistics");
            Console.WriteLine("- More interpretable vectors");
            Console.WriteLine("- Better on similarity and analogy tasks");
            Console.WriteLine("- No negative sampling required");
            Console.WriteLine("- More scalable to large corpora");
            Console.WriteLine("\nWord2Vec Advantages:");
            Console.WriteLine("- Simpler training procedure");
            Console.WriteLine("- Better at capturing local context");
            Console.WriteLine("- Faster convergence");
            Console.WriteLine("- Smaller memory footprint");

            Console.WriteLine("\n=== Co-occurrence Statistics ===");
            Console.WriteLine("GloVe learns from word-word co-occurrence:");
            Console.WriteLine("- Builds co-occurrence matrix from corpus");
            Console.WriteLine("- Counts how often words appear together");
            Console.WriteLine("- Uses weighted matrix factorization");
            Console.WriteLine("- Weighting function emphasizes important pairs");
            Console.WriteLine("- Combines global and local statistics");

            Console.WriteLine("\n=== Applications in NLP ===");
            Console.WriteLine("1. Machine Translation");
            Console.WriteLine("   - Word alignment");
            Console.WriteLine("   - Context representation");
            Console.WriteLine("   - Translation quality");
            Console.WriteLine("\n2. Text Classification");
            Console.WriteLine("   - Document representation");
            Console.WriteLine("   - Semantic similarity");
            Console.WriteLine("   - Topic classification");
            Console.WriteLine("\n3. Sentiment Analysis");
            Console.WriteLine("   - Sentiment word representation");
            Console.WriteLine("   - Context-aware sentiment");
            Console.WriteLine("   - Aspect-based analysis");
            Console.WriteLine("\n4. Question Answering");
            Console.WriteLine("   - Query representation");
            Console.WriteLine("   - Passage matching");
            Console.WriteLine("   - Answer relevance");
            Console.WriteLine("\n5. Named Entity Recognition");
            Console.WriteLine("   - Entity type representation");
            Console.WriteLine("   - Context modeling");
            Console.WriteLine("   - Entity linking");
            Console.WriteLine("\n6. Part-of-Speech Tagging");
            Console.WriteLine("   - Word class representation");
            Console.WriteLine("   - Context encoding");
            Console.WriteLine("   - Syntactic patterns");

            Console.WriteLine("\n=== GloVe Training Algorithm ===");
            Console.WriteLine("1. Build co-occurrence matrix X");
            Console.WriteLine("2. Initialize word vectors W and context vectors C");
            Console.WriteLine("3. For each (i,j) pair:");
            Console.WriteLine("   - Compute wi · cj + bi + bj");
            Console.WriteLine("   - Compute loss: f(Xij)(prediction - log(Xij))^2");
            Console.WriteLine("4. Update vectors using gradient descent");
            Console.WriteLine("5. Repeat until convergence");

            Console.WriteLine("\n=== Weighting Function ===");
            Console.WriteLine("f(X) = min(1, (X/Xmax)^alpha)");
            Console.WriteLine("- Reduces weight for very frequent pairs");
            Console.WriteLine("- Emphasizes informative co-occurrences");
            Console.WriteLine("- Typical: Xmax=100, alpha=0.75");
            Console.WriteLine("- Improves performance on rare word pairs");

            Console.WriteLine("\n=== Advantages of GloVe ===");
            Console.WriteLine("- Combines global and local information");
            Console.WriteLine("- Better interpretability than Word2Vec");
            Console.WriteLine("- Scalable training process");
            Console.WriteLine("- Strong on analogy tasks");
            Console.WriteLine("- Good performance on similarity tasks");
            Console.WriteLine("- Available pre-trained models");
            Console.WriteLine("- Support for multiple languages");

            Console.WriteLine("\n=== Key Parameters ===");
            Console.WriteLine("- Embedding Dimension: 50-300 typical");
            Console.WriteLine("- Context Window: 3-10 typical");
            Console.WriteLine("- Learning Rate: 0.01-0.1 typical");
            Console.WriteLine("- Xmax: 100 (co-occurrence threshold)");
            Console.WriteLine("- Alpha: 0.75 (weighting exponent)");
            Console.WriteLine("- Epochs: 5-100 typical");
        }
    }
}
