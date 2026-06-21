using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Text preprocessing for neural network input
    /// </summary>
    public class TextPreprocessor
    {
        public HashSet<string> StopWords { get; set; }
        public Dictionary<string, string> StemMap { get; set; }

        public TextPreprocessor()
        {
            // Common English stop words
            StopWords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to", "for",
                "of", "is", "was", "are", "been", "be", "have", "has", "had", "do",
                "does", "did", "will", "would", "should", "could", "may", "might"
            };

            // Simple stemming map
            StemMap = new Dictionary<string, string>
            {
                { "running", "run" }, { "runs", "run" }, { "ran", "run" },
                { "walking", "walk" }, { "walks", "walk" }, { "walked", "walk" },
                { "talking", "talk" }, { "talks", "talk" }, { "talked", "talk" },
                { "playing", "play" }, { "plays", "play" }, { "played", "play" },
                { "flying", "fly" }, { "flies", "fly" }, { "flew", "fly" },
                { "studies", "study" }, { "studying", "study" }, { "studied", "study" }
            };
        }

        public string CleanText(string text)
        {
            // Convert to lowercase
            text = text.ToLower();
            // Remove punctuation
            text = System.Text.RegularExpressions.Regex.Replace(text, "[^a-z0-9\\s]", "");
            // Remove extra whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, "\\s+", " ");
            return text.Trim();
        }

        public List<string> Tokenize(string text)
        {
            return text.Split(' ').ToList();
        }

        public List<string> RemoveStopWords(List<string> tokens)
        {
            return tokens.Where(t => !StopWords.Contains(t)).ToList();
        }

        public List<string> Stem(List<string> tokens)
        {
            return tokens.Select(t => StemMap.ContainsKey(t) ? StemMap[t] : t).ToList();
        }

        public List<string> Preprocess(string text, bool removeStopWords = true, bool applyStemming = true)
        {
            text = CleanText(text);
            var tokens = Tokenize(text);

            if (removeStopWords)
                tokens = RemoveStopWords(tokens);

            if (applyStemming)
                tokens = Stem(tokens);

            return tokens;
        }
    }

    /// <summary>
    /// Text encoding schemes for neural network input
    /// </summary>
    public class TextEncoder
    {
        public Dictionary<string, int> WordToIndex { get; set; }
        public Dictionary<int, string> IndexToWord { get; set; }
        public int VocabularySize { get; set; }

        public TextEncoder()
        {
            WordToIndex = new Dictionary<string, int>();
            IndexToWord = new Dictionary<int, string>();
            VocabularySize = 0;
        }

        public void BuildVocabulary(List<List<string>> documents)
        {
            WordToIndex.Clear();
            IndexToWord.Clear();
            int index = 0;

            foreach (var doc in documents)
            {
                foreach (var word in doc)
                {
                    if (!WordToIndex.ContainsKey(word))
                    {
                        WordToIndex[word] = index;
                        IndexToWord[index] = word;
                        index++;
                    }
                }
            }

            VocabularySize = index;
        }

        public double[] OneHotEncode(string word)
        {
            double[] encoding = new double[VocabularySize];
            if (WordToIndex.ContainsKey(word))
                encoding[WordToIndex[word]] = 1.0;
            return encoding;
        }

        public double[] BagOfWordsEncode(List<string> words)
        {
            double[] encoding = new double[VocabularySize];
            foreach (var word in words)
            {
                if (WordToIndex.ContainsKey(word))
                    encoding[WordToIndex[word]]++;
            }
            return encoding;
        }

        public double[] TfidfEncode(List<string> words, int docFrequency)
        {
            double[] encoding = new double[VocabularySize];
            foreach (var word in words)
            {
                if (WordToIndex.ContainsKey(word))
                {
                    double tf = 1.0 + Math.Log(words.Count(w => w == word));
                    double idf = Math.Log(VocabularySize / (double)docFrequency);
                    encoding[WordToIndex[word]] = tf * idf;
                }
            }
            return encoding;
        }

        public List<int> SequenceEncode(List<string> words)
        {
            var sequence = new List<int>();
            foreach (var word in words)
            {
                if (WordToIndex.ContainsKey(word))
                    sequence.Add(WordToIndex[word]);
            }
            return sequence;
        }
    }

    /// <summary>
    /// Neural network layer for forward propagation
    /// </summary>
    public class NetworkLayer
    {
        public double[][] Weights { get; set; }
        public double[] Biases { get; set; }
        public string ActivationType { get; set; }
        public double[] LastInput { get; set; }
        public double[] LastPreActivation { get; set; }
        public double[] LastOutput { get; set; }

        public NetworkLayer(int inputSize, int outputSize, string activation = "relu")
        {
            ActivationType = activation;
            Weights = new double[inputSize][];
            Biases = new double[outputSize];

            // Initialize weights
            Random rand = new Random(42);
            double scale = Math.Sqrt(2.0 / inputSize);

            for (int i = 0; i < inputSize; i++)
            {
                Weights[i] = new double[outputSize];
                for (int j = 0; j < outputSize; j++)
                    Weights[i][j] = (rand.NextDouble() - 0.5) * scale;
            }

            for (int j = 0; j < outputSize; j++)
                Biases[j] = 0;
        }

        public double[] Forward(double[] input)
        {
            LastInput = input;

            // Linear transformation
            double[] preActivation = new double[Biases.Length];
            for (int j = 0; j < Biases.Length; j++)
            {
                preActivation[j] = Biases[j];
                for (int i = 0; i < input.Length; i++)
                    preActivation[j] += input[i] * Weights[i][j];
            }

            LastPreActivation = preActivation;

            // Apply activation function
            double[] output = ApplyActivation(preActivation);
            LastOutput = output;

            return output;
        }

        private double[] ApplyActivation(double[] input)
        {
            double[] output = new double[input.Length];

            switch (ActivationType.ToLower())
            {
                case "relu":
                    for (int i = 0; i < input.Length; i++)
                        output[i] = Math.Max(0, input[i]);
                    break;

                case "sigmoid":
                    for (int i = 0; i < input.Length; i++)
                        output[i] = 1.0 / (1.0 + Math.Exp(-input[i]));
                    break;

                case "tanh":
                    for (int i = 0; i < input.Length; i++)
                        output[i] = Math.Tanh(input[i]);
                    break;

                case "softmax":
                    double maxVal = input.Max();
                    double sum = 0;
                    for (int i = 0; i < input.Length; i++)
                    {
                        output[i] = Math.Exp(input[i] - maxVal);
                        sum += output[i];
                    }
                    for (int i = 0; i < output.Length; i++)
                        output[i] /= sum;
                    break;

                case "linear":
                default:
                    Array.Copy(input, output, input.Length);
                    break;
            }

            return output;
        }
    }

    /// <summary>
    /// Forward propagation through neural network
    /// </summary>
    public class ForwardPropagationNetwork
    {
        public List<NetworkLayer> Layers { get; set; }
        public List<double[]> LayerOutputs { get; set; }

        public ForwardPropagationNetwork()
        {
            Layers = new List<NetworkLayer>();
            LayerOutputs = new List<double[]>();
        }

        public void AddLayer(int inputSize, int outputSize, string activation = "relu")
        {
            Layers.Add(new NetworkLayer(inputSize, outputSize, activation));
        }

        public double[] Forward(double[] input)
        {
            LayerOutputs.Clear();
            LayerOutputs.Add(input);

            double[] current = input;

            foreach (var layer in Layers)
            {
                current = layer.Forward(current);
                LayerOutputs.Add(current);
            }

            return current;
        }

        public void PrintArchitecture()
        {
            Console.WriteLine("Network Architecture:");
            Console.WriteLine($"Input Layer: Variable size");

            for (int i = 0; i < Layers.Count; i++)
            {
                Console.WriteLine($"Layer {i + 1}: {Layers[i].Weights[0].Length} neurons, Activation: {Layers[i].ActivationType}");
            }
        }

        public void PrintForwardPass(double[] input)
        {
            Console.WriteLine("Forward Propagation Process:");
            double[] current = input;

            Console.WriteLine($"Input: {string.Join(", ", input.Take(5))}... (size: {input.Length})");

            for (int i = 0; i < Layers.Count; i++)
            {
                current = Layers[i].Forward(current);
                Console.WriteLine($"Layer {i + 1} Output: {string.Join(", ", current.Take(5))}... (size: {current.Length})");
            }
        }
    }

    /// <summary>
    /// Text classifier using forward propagation
    /// </summary>
    public class ForwardPropagationClassifier
    {
        public TextPreprocessor Preprocessor { get; set; }
        public TextEncoder Encoder { get; set; }
        public ForwardPropagationNetwork Network { get; set; }
        public List<string> Classes { get; set; }

        public ForwardPropagationClassifier(int vocabSize, int hiddenSize, List<string> classes)
        {
            Preprocessor = new TextPreprocessor();
            Encoder = new TextEncoder();
            Classes = classes;

            // Build network: vocab -> hidden -> classes
            Network = new ForwardPropagationNetwork();
            Network.AddLayer(vocabSize, hiddenSize, "relu");
            Network.AddLayer(hiddenSize, hiddenSize / 2, "relu");
            Network.AddLayer(hiddenSize / 2, classes.Count, "softmax");
        }

        public void Train(List<(string, string)> trainingData)
        {
            // Preprocess and build vocabulary
            var processedDocs = new List<List<string>>();
            foreach (var (text, _) in trainingData)
            {
                var processed = Preprocessor.Preprocess(text);
                processedDocs.Add(processed);
            }

            Encoder.BuildVocabulary(processedDocs);
            Console.WriteLine($"Vocabulary size: {Encoder.VocabularySize}");
        }

        public string Predict(string text)
        {
            var processed = Preprocessor.Preprocess(text);
            double[] encoded = Encoder.BagOfWordsEncode(processed);
            double[] output = Network.Forward(encoded);

            int maxIndex = 0;
            for (int i = 1; i < output.Length; i++)
            {
                if (output[i] > output[maxIndex])
                    maxIndex = i;
            }

            return Classes[maxIndex];
        }

        public Dictionary<string, double> PredictWithProbabilities(string text)
        {
            var processed = Preprocessor.Preprocess(text);
            double[] encoded = Encoder.BagOfWordsEncode(processed);
            double[] output = Network.Forward(encoded);

            var result = new Dictionary<string, double>();
            for (int i = 0; i < Classes.Count; i++)
                result[Classes[i]] = output[i];

            return result;
        }
    }

    /// <summary>
    /// Machine translation using forward propagation
    /// </summary>
    public class ForwardPropagationTranslator
    {
        public TextEncoder SourceEncoder { get; set; }
        public TextEncoder TargetEncoder { get; set; }
        public ForwardPropagationNetwork EncoderNetwork { get; set; }
        public ForwardPropagationNetwork DecoderNetwork { get; set; }

        public ForwardPropagationTranslator(int sourceVocabSize, int targetVocabSize, int embeddingDim = 128)
        {
            SourceEncoder = new TextEncoder();
            TargetEncoder = new TextEncoder();

            // Encoder: source text -> embedding
            EncoderNetwork = new ForwardPropagationNetwork();
            EncoderNetwork.AddLayer(sourceVocabSize, embeddingDim, "relu");
            EncoderNetwork.AddLayer(embeddingDim, embeddingDim / 2, "relu");

            // Decoder: embedding -> target text
            DecoderNetwork = new ForwardPropagationNetwork();
            DecoderNetwork.AddLayer(embeddingDim / 2, embeddingDim, "relu");
            DecoderNetwork.AddLayer(embeddingDim, targetVocabSize, "softmax");
        }

        public void BuildVocabularies(List<List<string>> sourceDocs, List<List<string>> targetDocs)
        {
            SourceEncoder.BuildVocabulary(sourceDocs);
            TargetEncoder.BuildVocabulary(targetDocs);
        }

        public List<string> Translate(List<string> sourceWords)
        {
            // Encode source
            double[] sourceEncoded = SourceEncoder.BagOfWordsEncode(sourceWords);
            double[] sourceEmbedding = EncoderNetwork.Forward(sourceEncoded);

            // Decode to target
            double[] targetOutput = DecoderNetwork.Forward(sourceEmbedding);

            // Get top words
            var translation = new List<string>();
            for (int i = 0; i < Math.Min(sourceWords.Count, 10); i++)
            {
                int maxIdx = 0;
                double maxVal = -1;

                for (int j = 0; j < targetOutput.Length; j++)
                {
                    if (targetOutput[j] > maxVal)
                    {
                        maxVal = targetOutput[j];
                        maxIdx = j;
                    }
                }

                if (TargetEncoder.IndexToWord.ContainsKey(maxIdx))
                    translation.Add(TargetEncoder.IndexToWord[maxIdx]);

                // Zero out the max for next iteration
                targetOutput[maxIdx] = -1;
            }

            return translation;
        }
    }

    /// <summary>
    /// Question answering using forward propagation
    /// </summary>
    public class ForwardPropagationQA
    {
        public TextPreprocessor Preprocessor { get; set; }
        public TextEncoder Encoder { get; set; }
        public ForwardPropagationNetwork Network { get; set; }
        public Dictionary<int, string> KnowledgeBase { get; set; }

        public ForwardPropagationQA(int vocabSize)
        {
            Preprocessor = new TextPreprocessor();
            Encoder = new TextEncoder();
            KnowledgeBase = new Dictionary<int, string>();

            // Network: question -> relevance scores
            Network = new ForwardPropagationNetwork();
            Network.AddLayer(vocabSize, 64, "relu");
            Network.AddLayer(64, 32, "relu");
            Network.AddLayer(32, 1, "sigmoid");
        }

        public void AddKnowledgeBase(List<string> passages)
        {
            var docs = new List<List<string>>();
            for (int i = 0; i < passages.Count; i++)
            {
                var processed = Preprocessor.Preprocess(passages[i]);
                docs.Add(processed);
                KnowledgeBase[i] = passages[i];
            }

            Encoder.BuildVocabulary(docs);
        }

        public string AnswerQuestion(string question)
        {
            var processedQ = Preprocessor.Preprocess(question);
            double[] questionEncoded = Encoder.BagOfWordsEncode(processedQ);

            double maxScore = -1;
            int bestPassage = -1;

            foreach (var (idx, passage) in KnowledgeBase)
            {
                double[] output = Network.Forward(questionEncoded);
                double score = output[0];

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPassage = idx;
                }
            }

            return bestPassage >= 0 ? KnowledgeBase[bestPassage] : "No answer found.";
        }
    }

    /// <summary>
    /// Forward propagation examples for NLP
    /// </summary>
    public class ForwardPropagationExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Text Preprocessing ===");
            var preprocessor = new TextPreprocessor();

            string sampleText = "The quick brown fox is jumping over the lazy dog!";
            Console.WriteLine($"Original: {sampleText}");

            string cleaned = preprocessor.CleanText(sampleText);
            Console.WriteLine($"Cleaned: {cleaned}");

            var tokens = preprocessor.Tokenize(cleaned);
            Console.WriteLine($"Tokenized: {string.Join(", ", tokens)}");

            var filtered = preprocessor.RemoveStopWords(tokens);
            Console.WriteLine($"After stop words removal: {string.Join(", ", filtered)}");

            var stemmed = preprocessor.Stem(filtered);
            Console.WriteLine($"After stemming: {string.Join(", ", stemmed)}");

            Console.WriteLine("\n=== Text Encoding ===");
            var encoder = new TextEncoder();

            var documents = new List<List<string>>
            {
                new List<string> { "good", "great", "excellent" },
                new List<string> { "bad", "poor", "terrible" },
                new List<string> { "happy", "joy", "pleased" }
            };

            encoder.BuildVocabulary(documents);
            Console.WriteLine($"Vocabulary size: {encoder.VocabularySize}");

            Console.WriteLine("\nOne-hot encoding of 'good': " + string.Join(", ", encoder.OneHotEncode("good")));

            var testWords = new List<string> { "good", "great" };
            Console.WriteLine("Bag-of-words encoding: " + string.Join(", ", encoder.BagOfWordsEncode(testWords)));

            Console.WriteLine("\n=== Forward Propagation Network ===");
            var network = new ForwardPropagationNetwork();
            network.AddLayer(10, 5, "relu");
            network.AddLayer(5, 3, "relu");
            network.AddLayer(3, 2, "softmax");

            network.PrintArchitecture();

            var testInput = new double[10];
            for (int i = 0; i < 10; i++)
                testInput[i] = i / 10.0;

            Console.WriteLine("\nForward pass through network:");
            network.PrintForwardPass(testInput);

            Console.WriteLine("\n=== Text Classification with Forward Propagation ===");
            var classifier = new ForwardPropagationClassifier(
                vocabSize: 100,
                hiddenSize: 64,
                classes: new List<string> { "Positive", "Negative" }
            );

            var trainingData = new List<(string, string)>
            {
                ("great movie excellent acting", "Positive"),
                ("amazing wonderful fantastic", "Positive"),
                ("terrible bad awful", "Negative"),
                ("horrible poor disappointing", "Negative"),
            };

            Console.WriteLine("Training classifier...");
            classifier.Train(trainingData);

            var testTexts = new List<string>
            {
                "great movie very good",
                "terrible awful film",
                "excellent wonderful performance"
            };

            Console.WriteLine("Classification Predictions:");
            foreach (var text in testTexts)
            {
                string prediction = classifier.Predict(text);
                var probs = classifier.PredictWithProbabilities(text);
                Console.WriteLine($"Text: \"{text}\"");
                Console.WriteLine($"  Prediction: {prediction}");
                foreach (var p in probs)
                    Console.WriteLine($"    {p.Key}: {p.Value:F3}");
            }

            Console.WriteLine("\n=== Machine Translation ===");
            var translator = new ForwardPropagationTranslator(
                sourceVocabSize: 50,
                targetVocabSize: 50,
                embeddingDim: 32
            );

            var englishDocs = new List<List<string>>
            {
                new List<string> { "hello", "world" },
                new List<string> { "good", "morning" }
            };

            var frenchDocs = new List<List<string>>
            {
                new List<string> { "bonjour", "monde" },
                new List<string> { "bon", "matin" }
            };

            translator.BuildVocabularies(englishDocs, frenchDocs);
            Console.WriteLine("Translator built and ready for translation");

            var sourceText = new List<string> { "hello", "world" };
            var translation = translator.Translate(sourceText);
            Console.WriteLine($"English: {string.Join(" ", sourceText)}");
            Console.WriteLine($"French: {string.Join(" ", translation)}");

            Console.WriteLine("\n=== Question Answering ===");
            var qa = new ForwardPropagationQA(vocabSize: 100);

            var passages = new List<string>
            {
                "Paris is the capital of France",
                "London is the capital of England",
                "Berlin is the capital of Germany",
                "Madrid is the capital of Spain"
            };

            qa.AddKnowledgeBase(passages);

            var questions = new List<string>
            {
                "What is the capital of France",
                "Where is Paris located",
                "Tell me about England capital"
            };

            Console.WriteLine("Question Answering:");
            foreach (var q in questions)
            {
                string answer = qa.AnswerQuestion(q);
                Console.WriteLine($"Q: {q}");
                Console.WriteLine($"A: {answer}");
            }

            Console.WriteLine("\n=== Forward Propagation Process Steps ===");
            Console.WriteLine("1. Text Preprocessing");
            Console.WriteLine("   - Clean text (lowercase, remove punctuation)");
            Console.WriteLine("   - Tokenization (split into words)");
            Console.WriteLine("   - Stop word removal");
            Console.WriteLine("   - Stemming/Lemmatization");
            Console.WriteLine("\n2. Text Encoding");
            Console.WriteLine("   - One-hot encoding (unique vector per word)");
            Console.WriteLine("   - Bag-of-Words (word frequency)");
            Console.WriteLine("   - TF-IDF (weighted frequencies)");
            Console.WriteLine("   - Embeddings (dense vectors)");
            Console.WriteLine("\n3. Feed to Neural Network");
            Console.WriteLine("   - Input layer receives encoded text");
            Console.WriteLine("   - Hidden layers compute intermediate representations");
            Console.WriteLine("   - Each layer: linear transformation + activation");
            Console.WriteLine("\n4. Output Computation");
            Console.WriteLine("   - Final layer applies task-specific activation");
            Console.WriteLine("   - Classification: Softmax for probabilities");
            Console.WriteLine("   - Regression: Linear for continuous values");

            Console.WriteLine("\n=== Activation Functions in Forward Pass ===");
            Console.WriteLine("- ReLU: Hidden layers (non-linearity, sparse)");
            Console.WriteLine("- Sigmoid: Binary classification (0-1 output)");
            Console.WriteLine("- Tanh: Hidden layers (normalized output)");
            Console.WriteLine("- Softmax: Multi-class (probability distribution)");
            Console.WriteLine("- Linear: Regression output");

            Console.WriteLine("\n=== Advantages of Forward Propagation ===");
            Console.WriteLine("- Simple computation from input to output");
            Console.WriteLine("- Well-understood and widely implemented");
            Console.WriteLine("- Efficient for inference");
            Console.WriteLine("- Foundation for backpropagation");
            Console.WriteLine("- Scalable to large networks");
            Console.WriteLine("- Supports various architectures");

            Console.WriteLine("\n=== NLP Applications ===");
            Console.WriteLine("1. Text Classification: Sentiment, spam, topic");
            Console.WriteLine("2. Machine Translation: Language-to-language");
            Console.WriteLine("3. Text Summarization: Extract key information");
            Console.WriteLine("4. Question Answering: Find relevant passages");
            Console.WriteLine("5. Named Entity Recognition: Identify entities");
            Console.WriteLine("6. Part-of-Speech Tagging: Grammar analysis");

            Console.WriteLine("\n=== Complexity and Performance ===");
            Console.WriteLine("Time: O(input_size × layer_width × num_layers)");
            Console.WriteLine("Space: O(weights + biases + activations)");
            Console.WriteLine("GPU Acceleration: Highly parallelizable");
            Console.WriteLine("Batch Processing: Process multiple samples");

            Console.WriteLine("\n=== Best Practices ===");
            Console.WriteLine("- Normalize input features to [0, 1] or [-1, 1]");
            Console.WriteLine("- Use appropriate activation functions");
            Console.WriteLine("- Monitor intermediate layer outputs");
            Console.WriteLine("- Batch processing for efficiency");
            Console.WriteLine("- Gradient checking for correctness");
            Console.WriteLine("- Profile computational bottlenecks");
        }
    }
}
