using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.DeepLearning
{
    /// <summary>
    /// Represents a word embedding.
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
    /// Embedding layer that converts words to dense vectors.
    /// </summary>
    public class EmbeddingLayer
    {
        private Dictionary<string, double[]> embeddings;
        private int embeddingDim;
        private Random random;

        public EmbeddingLayer(int embeddingDim)
        {
            this.embeddingDim = embeddingDim;
            this.embeddings = new Dictionary<string, double[]>();
            this.random = new Random();
        }

        /// <summary>
        /// Initializes embeddings for a vocabulary.
        /// </summary>
        public void InitializeEmbeddings(List<string> vocabulary)
        {
            foreach (string word in vocabulary)
            {
                double[] embedding = new double[embeddingDim];
                for (int i = 0; i < embeddingDim; i++)
                {
                    embedding[i] = (random.NextDouble() - 0.5) / Math.Sqrt(embeddingDim);
                }
                embeddings[word.ToLower()] = embedding;
            }
        }

        /// <summary>
        /// Gets embedding for a word.
        /// </summary>
        public double[] GetEmbedding(string word)
        {
            string key = word.ToLower();
            if (embeddings.ContainsKey(key))
            {
                return (double[])embeddings[key].Clone();
            }

            // Return random embedding for unknown words
            double[] unknown = new double[embeddingDim];
            for (int i = 0; i < embeddingDim; i++)
            {
                unknown[i] = (random.NextDouble() - 0.5) / Math.Sqrt(embeddingDim);
            }
            return unknown;
        }

        /// <summary>
        /// Converts text to embedding matrix.
        /// </summary>
        public double[][] TextToEmbeddingMatrix(string text)
        {
            var words = Regex.Split(text.ToLower(), @"[^\w]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToArray();

            double[][] matrix = new double[words.Length][];

            for (int i = 0; i < words.Length; i++)
            {
                matrix[i] = GetEmbedding(words[i]);
            }

            return matrix;
        }
    }

    /// <summary>
    /// Convolutional filter for text processing.
    /// </summary>
    public class ConvolutionalFilter
    {
        public double[,] Weights { get; set; }
        public double Bias { get; set; }
        public int FilterSize { get; set; }
        public int EmbeddingDim { get; set; }

        public ConvolutionalFilter(int filterSize, int embeddingDim)
        {
            FilterSize = filterSize;
            EmbeddingDim = embeddingDim;
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            var random = new Random();
            Weights = new double[FilterSize, EmbeddingDim];
            Bias = (random.NextDouble() - 0.5) * 0.1;

            for (int i = 0; i < FilterSize; i++)
            {
                for (int j = 0; j < EmbeddingDim; j++)
                {
                    Weights[i, j] = (random.NextDouble() - 0.5) / Math.Sqrt(FilterSize * EmbeddingDim);
                }
            }
        }

        /// <summary>
        /// Applies filter to a window of embeddings.
        /// </summary>
        public double ApplyFilter(double[][] window)
        {
            double sum = Bias;

            for (int i = 0; i < window.Length && i < FilterSize; i++)
            {
                for (int j = 0; j < window[i].Length && j < EmbeddingDim; j++)
                {
                    sum += window[i][j] * Weights[i, j];
                }
            }

            return sum;
        }
    }

    /// <summary>
    /// Convolutional layer for text feature extraction.
    /// </summary>
    public class ConvolutionalLayer
    {
        private List<ConvolutionalFilter> filters;
        private int filterSize;
        private int numFilters;

        public ConvolutionalLayer(int filterSize, int numFilters, int embeddingDim)
        {
            this.filterSize = filterSize;
            this.numFilters = numFilters;
            this.filters = new List<ConvolutionalFilter>();

            for (int i = 0; i < numFilters; i++)
            {
                filters.Add(new ConvolutionalFilter(filterSize, embeddingDim));
            }
        }

        /// <summary>
        /// Applies convolution to text embeddings.
        /// </summary>
        public double[] Convolve(double[][] embeddings)
        {
            double[] convolutionOutput = new double[embeddings.Length - filterSize + 1];

            for (int i = 0; i <= embeddings.Length - filterSize; i++)
            {
                double[][] window = new double[filterSize][];
                for (int j = 0; j < filterSize; j++)
                {
                    window[j] = embeddings[i + j];
                }

                convolutionOutput[i] = filters[0].ApplyFilter(window);
            }

            return convolutionOutput;
        }

        /// <summary>
        /// Applies all filters to text embeddings.
        /// </summary>
        public List<double[]> ConvolveAllFilters(double[][] embeddings)
        {
            var results = new List<double[]>();

            foreach (var filter in filters)
            {
                double[] output = new double[Math.Max(1, embeddings.Length - filterSize + 1)];

                for (int i = 0; i <= embeddings.Length - filterSize; i++)
                {
                    double[][] window = new double[filterSize][];
                    for (int j = 0; j < filterSize; j++)
                    {
                        window[j] = embeddings[i + j];
                    }

                    output[i] = ReLU(filter.ApplyFilter(window));
                }

                results.Add(output);
            }

            return results;
        }

        private double ReLU(double x)
        {
            return Math.Max(0, x);
        }

        public int GetNumFilters() => numFilters;
    }

    /// <summary>
    /// Max pooling layer for feature extraction.
    /// </summary>
    public class MaxPoolingLayer
    {
        /// <summary>
        /// Applies max pooling to convolutional output.
        /// </summary>
        public double MaxPooling(double[] convolutionOutput)
        {
            if (convolutionOutput.Length == 0) return 0;
            return convolutionOutput.Max();
        }

        /// <summary>
        /// Applies max pooling to multiple filter outputs.
        /// </summary>
        public double[] PoolAllFilters(List<double[]> filterOutputs)
        {
            double[] pooledOutput = new double[filterOutputs.Count];

            for (int i = 0; i < filterOutputs.Count; i++)
            {
                pooledOutput[i] = MaxPooling(filterOutputs[i]);
            }

            return pooledOutput;
        }
    }

    /// <summary>
    /// Fully connected layer for classification.
    /// </summary>
    public class FullyConnectedLayer
    {
        private double[] weights;
        private double bias;
        private int inputSize;
        private Random random;

        public FullyConnectedLayer(int inputSize)
        {
            this.inputSize = inputSize;
            this.weights = new double[inputSize];
            this.random = new Random();
            InitializeWeights();
        }

        private void InitializeWeights()
        {
            for (int i = 0; i < inputSize; i++)
            {
                weights[i] = (random.NextDouble() - 0.5) / Math.Sqrt(inputSize);
            }
            bias = (random.NextDouble() - 0.5) * 0.1;
        }

        /// <summary>
        /// Forward pass through fully connected layer.
        /// </summary>
        public double Forward(double[] input)
        {
            double sum = bias;
            for (int i = 0; i < input.Length && i < weights.Length; i++)
            {
                sum += input[i] * weights[i];
            }
            return sum;
        }

        /// <summary>
        /// Applies sigmoid activation.
        /// </summary>
        public double Sigmoid(double x)
        {
            if (x < -500) return 0;
            if (x > 500) return 1;
            return 1.0 / (1.0 + Math.Exp(-x));
        }
    }

    /// <summary>
    /// Complete CNN model for text classification.
    /// </summary>
    public class TextCNN
    {
        private EmbeddingLayer embeddingLayer;
        private List<ConvolutionalLayer> convolutionalLayers;
        private MaxPoolingLayer poolingLayer;
        private FullyConnectedLayer classificationLayer;
        private List<string> vocabulary;
        private int embeddingDim;

        public TextCNN(int embeddingDim = 100, List<int> filterSizes = null, int numFiltersPerSize = 100)
        {
            this.embeddingDim = embeddingDim;
            this.embeddingLayer = new EmbeddingLayer(embeddingDim);
            this.convolutionalLayers = new List<ConvolutionalLayer>();
            this.poolingLayer = new MaxPoolingLayer();

            if (filterSizes == null)
                filterSizes = new List<int> { 3, 4, 5 };

            foreach (int filterSize in filterSizes)
            {
                convolutionalLayers.Add(new ConvolutionalLayer(filterSize, numFiltersPerSize, embeddingDim));
            }

            int totalFeatures = filterSizes.Count * numFiltersPerSize;
            classificationLayer = new FullyConnectedLayer(totalFeatures);
        }

        /// <summary>
        /// Initializes model with vocabulary.
        /// </summary>
        public void InitializeWithVocabulary(List<string> vocab)
        {
            this.vocabulary = vocab;
            embeddingLayer.InitializeEmbeddings(vocab);
        }

        /// <summary>
        /// Forward pass through the entire network.
        /// </summary>
        public double Predict(string text)
        {
            // Step 1: Convert text to embeddings
            double[][] embeddings = embeddingLayer.TextToEmbeddingMatrix(text);

            if (embeddings.Length == 0)
                return 0.5;

            // Step 2: Apply convolutional layers and pooling
            double[] pooledFeatures = new double[0];

            foreach (var convLayer in convolutionalLayers)
            {
                var filterOutputs = convLayer.ConvolveAllFilters(embeddings);
                double[] pooledOutput = poolingLayer.PoolAllFilters(filterOutputs);

                // Concatenate pooled features
                double[] combined = new double[pooledFeatures.Length + pooledOutput.Length];
                Array.Copy(pooledFeatures, combined, pooledFeatures.Length);
                Array.Copy(pooledOutput, 0, combined, pooledFeatures.Length, pooledOutput.Length);
                pooledFeatures = combined;
            }

            // Step 3: Fully connected layer with sigmoid
            double output = classificationLayer.Forward(pooledFeatures);
            return classificationLayer.Sigmoid(output);
        }

        /// <summary>
        /// Predicts with confidence scores.
        /// </summary>
        public (string label, double confidence) PredictWithConfidence(string text, string positiveLabel = "positive")
        {
            double score = Predict(text);
            string label = score > 0.5 ? positiveLabel : "negative";
            return (label, Math.Abs(score - 0.5) * 2);
        }
    }

    /// <summary>
    /// Multi-class CNN for text categorization.
    /// </summary>
    public class MultiClassTextCNN
    {
        private EmbeddingLayer embeddingLayer;
        private List<ConvolutionalLayer> convolutionalLayers;
        private MaxPoolingLayer poolingLayer;
        private Dictionary<string, FullyConnectedLayer> classifiers;
        private List<string> classes;
        private int embeddingDim;

        public MultiClassTextCNN(int embeddingDim = 100, List<int> filterSizes = null, int numFiltersPerSize = 100)
        {
            this.embeddingDim = embeddingDim;
            this.embeddingLayer = new EmbeddingLayer(embeddingDim);
            this.convolutionalLayers = new List<ConvolutionalLayer>();
            this.poolingLayer = new MaxPoolingLayer();
            this.classifiers = new Dictionary<string, FullyConnectedLayer>();
            this.classes = new List<string>();

            if (filterSizes == null)
                filterSizes = new List<int> { 3, 4, 5 };

            foreach (int filterSize in filterSizes)
            {
                convolutionalLayers.Add(new ConvolutionalLayer(filterSize, numFiltersPerSize, embeddingDim));
            }
        }

        /// <summary>
        /// Initializes with vocabulary and classes.
        /// </summary>
        public void InitializeWithVocabularyAndClasses(List<string> vocab, List<string> classLabels)
        {
            embeddingLayer.InitializeEmbeddings(vocab);
            this.classes = classLabels;

            int totalFeatures = convolutionalLayers.Count * 100;

            foreach (string classLabel in classLabels)
            {
                classifiers[classLabel] = new FullyConnectedLayer(totalFeatures);
            }
        }

        /// <summary>
        /// Extracts features using CNN layers.
        /// </summary>
        private double[] ExtractFeatures(string text)
        {
            double[][] embeddings = embeddingLayer.TextToEmbeddingMatrix(text);

            if (embeddings.Length == 0)
                return new double[convolutionalLayers.Count * 100];

            double[] pooledFeatures = new double[0];

            foreach (var convLayer in convolutionalLayers)
            {
                var filterOutputs = convLayer.ConvolveAllFilters(embeddings);
                double[] pooledOutput = poolingLayer.PoolAllFilters(filterOutputs);

                double[] combined = new double[pooledFeatures.Length + pooledOutput.Length];
                Array.Copy(pooledFeatures, combined, pooledFeatures.Length);
                Array.Copy(pooledOutput, 0, combined, pooledFeatures.Length, pooledOutput.Length);
                pooledFeatures = combined;
            }

            return pooledFeatures;
        }

        /// <summary>
        /// Predicts class for text.
        /// </summary>
        public string Predict(string text)
        {
            var features = ExtractFeatures(text);
            var scores = new Dictionary<string, double>();

            foreach (string classLabel in classes)
            {
                double rawScore = classifiers[classLabel].Forward(features);
                scores[classLabel] = classifiers[classLabel].Sigmoid(rawScore);
            }

            return scores.OrderByDescending(s => s.Value).First().Key;
        }

        /// <summary>
        /// Predicts with confidence scores for all classes.
        /// </summary>
        public Dictionary<string, double> PredictWithProbabilities(string text)
        {
            var features = ExtractFeatures(text);
            var scores = new Dictionary<string, double>();
            double totalScore = 0;

            foreach (string classLabel in classes)
            {
                double rawScore = classifiers[classLabel].Forward(features);
                double sigmoid = classifiers[classLabel].Sigmoid(rawScore);
                scores[classLabel] = sigmoid;
                totalScore += sigmoid;
            }

            // Normalize to probabilities
            foreach (var key in scores.Keys.ToList())
            {
                scores[key] = scores[key] / totalScore;
            }

            return scores;
        }
    }

    /// <summary>
    /// Examples demonstrating CNN applications in NLP.
    /// </summary>
    public static class TextCNNExamples
    {
        public static void DemonstrateTextEmbedding()
        {
            Console.WriteLine("=== Text Embedding Layer ===\n");

            var vocabulary = new List<string> { "hello", "world", "how", "are", "you", "doing" };
            var embeddingLayer = new EmbeddingLayer(embeddingDim: 4);
            embeddingLayer.InitializeEmbeddings(vocabulary);

            string text = "hello world";
            var embeddings = embeddingLayer.TextToEmbeddingMatrix(text);

            Console.WriteLine($"Text: '{text}'");
            Console.WriteLine($"Embeddings (dimension: 4):");
            for (int i = 0; i < embeddings.Length; i++)
            {
                Console.WriteLine($"  Word {i}: [{string.Join(", ", embeddings[i].Select(v => v.ToString("F3")))}]");
            }
        }

        public static void DemonstrateConvolutionalFilters()
        {
            Console.WriteLine("\n=== Convolutional Filters ===\n");

            var vocabulary = new List<string> { "the", "cat", "sat", "on", "mat" };
            var embeddingLayer = new EmbeddingLayer(embeddingDim: 8);
            embeddingLayer.InitializeEmbeddings(vocabulary);

            string text = "the cat sat on mat";
            var embeddings = embeddingLayer.TextToEmbeddingMatrix(text);

            Console.WriteLine($"Text: '{text}'");
            Console.WriteLine($"Number of words: {embeddings.Length}");
            Console.WriteLine($"Embedding dimension: {embeddings[0].Length}\n");

            var convLayer = new ConvolutionalLayer(filterSize: 3, numFilters: 2, embeddingDim: 8);
            Console.WriteLine("Applying convolutional filters (filter size: 3):");

            var filterOutputs = convLayer.ConvolveAllFilters(embeddings);
            for (int i = 0; i < filterOutputs.Count; i++)
            {
                Console.WriteLine($"  Filter {i + 1} output: [{string.Join(", ", filterOutputs[i].Select(v => v.ToString("F3")))}]");
            }
        }

        public static void DemonstrateMaxPooling()
        {
            Console.WriteLine("\n=== Max Pooling ===\n");

            // Simulated convolutional outputs
            var filterOutputs = new List<double[]>
            {
                new double[] { 0.5, 0.8, 0.3, 0.9, 0.2 },
                new double[] { 0.1, 0.6, 0.7, 0.4, 0.9 },
                new double[] { 0.9, 0.2, 0.5, 0.3, 0.8 }
            };

            var poolingLayer = new MaxPoolingLayer();

            Console.WriteLine("Convolutional outputs from 3 filters:");
            for (int i = 0; i < filterOutputs.Count; i++)
            {
                Console.WriteLine($"  Filter {i + 1}: [{string.Join(", ", filterOutputs[i].Select(v => v.ToString("F2")))}]");
            }

            var pooledOutput = poolingLayer.PoolAllFilters(filterOutputs);
            Console.WriteLine($"\nMax pooling results: [{string.Join(", ", pooledOutput.Select(v => v.ToString("F2")))}]");
            Console.WriteLine("(Each filter contributes its maximum value)");
        }

        public static void DemonstrateSentimentAnalysis()
        {
            Console.WriteLine("\n=== Sentiment Analysis with TextCNN ===\n");

            var vocabulary = new List<string>
            {
                "good", "great", "excellent", "amazing", "wonderful",
                "bad", "terrible", "awful", "poor", "disappointing",
                "movie", "film", "performance", "acting", "is", "was"
            };

            var model = new TextCNN(embeddingDim: 50, filterSizes: new List<int> { 3, 4, 5 }, numFiltersPerSize: 10);
            model.InitializeWithVocabulary(vocabulary);

            var testSentences = new List<string>
            {
                "This movie is excellent and amazing",
                "Terrible film with poor acting",
                "Great performance by the actors",
                "Awful and disappointing movie"
            };

            Console.WriteLine("Sentiment predictions:");
            foreach (var sentence in testSentences)
            {
                var (label, confidence) = model.PredictWithConfidence(sentence, "positive");
                Console.WriteLine($"  Text: '{sentence}'");
                Console.WriteLine($"  Prediction: {label} (confidence: {confidence:F4})\n");
            }
        }

        public static void DemonstrateTextClassification()
        {
            Console.WriteLine("\n=== Multi-class Text Classification ===\n");

            var vocabulary = new List<string>
            {
                "football", "soccer", "basketball", "game", "team", "player",
                "stock", "market", "price", "profit", "trading", "invest",
                "politician", "election", "vote", "government", "policy", "law"
            };

            var classes = new List<string> { "sports", "business", "politics" };

            var model = new MultiClassTextCNN(embeddingDim: 50, filterSizes: new List<int> { 3, 4 }, numFiltersPerSize: 10);
            model.InitializeWithVocabularyAndClasses(vocabulary, classes);

            var testDocuments = new List<string>
            {
                "The football team won the championship game",
                "Stock market price increased significantly today",
                "New government policy passed by election",
                "Basketball player scored amazing points",
                "Trading strategy profit maximization",
                "Politician election vote campaign"
            };

            Console.WriteLine("Document classification:");
            foreach (var doc in testDocuments)
            {
                string predicted = model.Predict(doc);
                var probabilities = model.PredictWithProbabilities(doc);

                Console.WriteLine($"Text: '{doc}'");
                Console.WriteLine($"Predicted class: {predicted}");
                Console.WriteLine("Probabilities:");
                foreach (var prob in probabilities.OrderByDescending(p => p.Value))
                {
                    Console.WriteLine($"  {prob.Key}: {prob.Value:F4}");
                }
                Console.WriteLine();
            }
        }

        public static void DemonstrateCNNArchitecture()
        {
            Console.WriteLine("\n=== CNN Architecture for NLP ===\n");

            Console.WriteLine("Architecture Overview:");
            Console.WriteLine("1. Input Layer");
            Console.WriteLine("   └─ Text sequence\n");

            Console.WriteLine("2. Embedding Layer");
            Console.WriteLine("   └─ Converts words to dense vectors (e.g., 100-dim)\n");

            Console.WriteLine("3. Convolutional Layers (with multiple filter sizes)");
            Console.WriteLine("   ├─ Filter size 3 (trigrams): learns 3-word patterns");
            Console.WriteLine("   ├─ Filter size 4 (4-grams): learns 4-word patterns");
            Console.WriteLine("   └─ Filter size 5 (5-grams): learns 5-word patterns");
            Console.WriteLine("   └─ ReLU activation\n");

            Console.WriteLine("4. Max Pooling Layer");
            Console.WriteLine("   └─ Extracts most important feature from each filter\n");

            Console.WriteLine("5. Fully Connected Layer");
            Console.WriteLine("   └─ Combines features for classification\n");

            Console.WriteLine("6. Output Layer");
            Console.WriteLine("   └─ Sigmoid (binary) or Softmax (multi-class)\n");

            Console.WriteLine("Key advantages:");
            Console.WriteLine("  ✓ Efficient parameter sharing via filters");
            Console.WriteLine("  ✓ Captures n-gram patterns at multiple levels");
            Console.WriteLine("  ✓ Handles variable-length input");
            Console.WriteLine("  ✓ Parallelizable across filters");
        }

        public static void DemonstrateFilterSizes()
        {
            Console.WriteLine("\n=== Impact of Filter Sizes ===\n");

            Console.WriteLine("Different filter sizes capture different patterns:\n");

            Console.WriteLine("Filter size 2 (bigrams):");
            Console.WriteLine("  - Captures 2-word patterns");
            Console.WriteLine("  - Examples: 'very good', 'not bad'");
            Console.WriteLine("  - Good for: phrase-level sentiment\n");

            Console.WriteLine("Filter size 3 (trigrams):");
            Console.WriteLine("  - Captures 3-word patterns");
            Console.WriteLine("  - Examples: 'extremely good movie', 'not very good'");
            Console.WriteLine("  - Good for: context-aware patterns\n");

            Console.WriteLine("Filter size 4-5 (4-grams, 5-grams):");
            Console.WriteLine("  - Captures longer patterns");
            Console.WriteLine("  - Examples: 'This movie is absolutely great'");
            Console.WriteLine("  - Good for: sentence-level patterns\n");

            Console.WriteLine("Best practice: Use multiple filter sizes (3, 4, 5)");
            Console.WriteLine("  - Captures patterns at different granularities");
            Console.WriteLine("  - More robust feature extraction");
        }

        public static void DemonstratePartOfSpeechTagging()
        {
            Console.WriteLine("\n=== Part-of-Speech Tagging (POS) ===\n");

            var vocabulary = new List<string>
            {
                "the", "quick", "brown", "fox", "jumps",
                "over", "lazy", "dog", "cat", "runs"
            };

            var model = new TextCNN(embeddingDim: 50, filterSizes: new List<int> { 2, 3, 4 });
            model.InitializeWithVocabulary(vocabulary);

            Console.WriteLine("CNN for POS tagging:");
            Console.WriteLine("  - Words with similar contexts get similar embeddings");
            Console.WriteLine("  - Convolutional filters learn syntactic patterns");
            Console.WriteLine("  - Common patterns: Articles, Adjectives, Verbs, Nouns\n");

            string sentence = "the quick brown fox";
            Console.WriteLine($"Example: '{sentence}'");
            Console.WriteLine("Learned patterns might identify:");
            Console.WriteLine("  'the [adjective] [adjective] [noun]' pattern");
            Console.WriteLine("  Useful for predicting POS for new sequences");
        }

        public static void DemonstrateNamedEntityRecognition()
        {
            Console.WriteLine("\n=== Named Entity Recognition (NER) ===\n");

            var vocabulary = new List<string>
            {
                "john", "smith", "apple", "google", "paris", "london",
                "president", "works", "at", "lives", "in", "is"
            };

            Console.WriteLine("CNN for Named Entity Recognition:");
            Console.WriteLine("  - Identifies persons, organizations, locations\n");

            Console.WriteLine("Example sentence:");
            Console.WriteLine("  'John Smith works at Apple in California'\n");

            Console.WriteLine("CNN learning:");
            Console.WriteLine("  - Filters learn patterns like:");
            Console.WriteLine("    • Capitalization patterns");
            Console.WriteLine("    • Word contexts (capital word preceded by position)");
            Console.WriteLine("    • Sequence patterns (name + verb + organization)\n");

            Console.WriteLine("Filter operations:");
            Console.WriteLine("  - Filter size 2: Captures name-verb, verb-org patterns");
            Console.WriteLine("  - Filter size 3: Captures 'name verb organization' pattern");
            Console.WriteLine("  - Pooling: Most important contextual clues");
        }

        public static void DemonstrateMachineTranslation()
        {
            Console.WriteLine("\n=== Machine Translation Context ===\n");

            Console.WriteLine("CNN component in translation systems:");
            Console.WriteLine("  - Source language encoder (CNN-based)\n");

            Console.WriteLine("Process:");
            Console.WriteLine("  1. Embedding: Convert source words to vectors");
            Console.WriteLine("  2. Convolution: Learn phrase-level patterns");
            Console.WriteLine("     • Captures idioms: 'kick the bucket'");
            Console.WriteLine("     • Captures grammar: verb conjugations");
            Console.WriteLine("  3. Pooling: Extract important features");
            Console.WriteLine("  4. Generate: Produce target language translation\n");

            Console.WriteLine("Example:");
            Console.WriteLine("  English: 'How are you'");
            Console.WriteLine("  CNN learns patterns for this phrase type");
            Console.WriteLine("  Generates appropriate French: 'Comment allez-vous'");
        }

        public static void DemonstrateAdvantages()
        {
            Console.WriteLine("\n=== CNN Advantages for NLP ===\n");

            Console.WriteLine("1. Learn Complex Patterns");
            Console.WriteLine("   - Multiple filter sizes capture different granularities");
            Console.WriteLine("   - ReLU activation enables non-linearity");
            Console.WriteLine("   - Hierarchical feature learning\n");

            Console.WriteLine("2. Handle Variable Length");
            Console.WriteLine("   - Max pooling adapts to different text lengths");
            Console.WriteLine("   - No need to pad or truncate uniformly\n");

            Console.WriteLine("3. Efficient Computation");
            Console.WriteLine("   - Parameter sharing through filters");
            Console.WriteLine("   - Highly parallelizable");
            Console.WriteLine("   - Faster training than RNNs\n");

            Console.WriteLine("4. Interpretable");
            Console.WriteLine("   - Each filter learns specific patterns");
            Console.WriteLine("   - Can visualize learned features");
            Console.WriteLine("   - Understand model decisions\n");

            Console.WriteLine("5. Practical Benefits");
            Console.WriteLine("   - Simple to implement");
            Console.WriteLine("   - Well-supported by frameworks");
            Console.WriteLine("   - GPU-friendly");
            Console.WriteLine("   - Good baseline for many tasks");
        }

        public static void DemonstrateLimitationsAndBest Practices()
        {
            Console.WriteLine("\n=== CNN Limitations and Best Practices ===\n");

            Console.WriteLine("Limitations:");
            Console.WriteLine("  - Limited long-range dependencies (compare to LSTM)");
            Console.WriteLine("  - Requires large datasets for good performance");
            Console.WriteLine("  - Hyperparameter tuning needed (filters, sizes)\n");

            Console.WriteLine("Best Practices:");
            Console.WriteLine("  1. Use multiple filter sizes (3, 4, 5)");
            Console.WriteLine("  2. Apply dropout for regularization");
            Console.WriteLine("  3. Use pre-trained word embeddings");
            Console.WriteLine("  4. Perform data augmentation for imbalanced data");
            Console.WriteLine("  5. Use cross-validation for evaluation");
            Console.WriteLine("  6. Monitor training/validation loss");
            Console.WriteLine("  7. Experiment with embedding dimensions (100-300)");
            Console.WriteLine("  8. Tune number of filters per size (50-200)\n");

            Console.WriteLine("When to use CNNs:");
            Console.WriteLine("  ✓ Text classification");
            Console.WriteLine("  ✓ Sentiment analysis");
            Console.WriteLine("  ✓ Language detection");
            Console.WriteLine("  ✓ Quick baseline models");
            Console.WriteLine("  ✗ Sequence labeling (use CRF/BiLSTM)");
            Console.WriteLine("  ✗ Machine translation (use Transformer)");
        }
    }
}
