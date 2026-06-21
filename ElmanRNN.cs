using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.RecurrentNetworks
{
    /// <summary>
    /// Elman Unit: Simple RNN cell with context/hidden state feedback.
    /// Introduced by Jeffrey Elman in 1990.
    /// </summary>
    public class ElmanUnit
    {
        private double[,] inputWeights;      // Weights from input to hidden
        private double[,] hiddenWeights;     // Weights from hidden state to hidden (recurrent)
        private double[,] outputWeights;     // Weights from hidden to output
        private double[] hiddenBias;
        private double[] outputBias;
        private int inputSize;
        private int hiddenSize;
        private int outputSize;
        private Random random;

        public ElmanUnit(int inputSize, int hiddenSize, int outputSize)
        {
            this.inputSize = inputSize;
            this.hiddenSize = hiddenSize;
            this.outputSize = outputSize;
            this.random = new Random();
            InitializeWeights();
        }

        /// <summary>
        /// Initializes weights randomly.
        /// </summary>
        private void InitializeWeights()
        {
            inputWeights = new double[hiddenSize, inputSize];
            hiddenWeights = new double[hiddenSize, hiddenSize];
            outputWeights = new double[outputSize, hiddenSize];
            hiddenBias = new double[hiddenSize];
            outputBias = new double[outputSize];

            // Xavier initialization
            double inputScale = Math.Sqrt(1.0 / inputSize);
            double hiddenScale = Math.Sqrt(1.0 / hiddenSize);
            double outputScale = Math.Sqrt(1.0 / hiddenSize);

            for (int i = 0; i < hiddenSize; i++)
            {
                for (int j = 0; j < inputSize; j++)
                    inputWeights[i, j] = (random.NextDouble() - 0.5) * inputScale;

                for (int j = 0; j < hiddenSize; j++)
                    hiddenWeights[i, j] = (random.NextDouble() - 0.5) * hiddenScale;

                hiddenBias[i] = 0;
            }

            for (int i = 0; i < outputSize; i++)
            {
                for (int j = 0; j < hiddenSize; j++)
                    outputWeights[i, j] = (random.NextDouble() - 0.5) * outputScale;

                outputBias[i] = 0;
            }
        }

        /// <summary>
        /// Forward pass through Elman unit.
        /// </summary>
        public (double[] output, double[] hiddenState) Forward(double[] input, double[] prevHiddenState)
        {
            // Compute hidden layer: h(t) = tanh(W_x * x(t) + W_h * h(t-1) + b_h)
            double[] hidden = new double[hiddenSize];
            for (int i = 0; i < hiddenSize; i++)
            {
                double sum = hiddenBias[i];

                // Input contribution
                for (int j = 0; j < inputSize; j++)
                    sum += inputWeights[i, j] * input[j];

                // Recurrent contribution (context from previous timestep)
                for (int j = 0; j < hiddenSize; j++)
                    sum += hiddenWeights[i, j] * prevHiddenState[j];

                hidden[i] = Tanh(sum);
            }

            // Compute output: y(t) = softmax(W_y * h(t) + b_y)
            double[] outputRaw = new double[outputSize];
            for (int i = 0; i < outputSize; i++)
            {
                double sum = outputBias[i];
                for (int j = 0; j < hiddenSize; j++)
                    sum += outputWeights[i, j] * hidden[j];
                outputRaw[i] = sum;
            }

            double[] output = Softmax(outputRaw);

            return (output, hidden);
        }

        private double Tanh(double x)
        {
            return Math.Tanh(x);
        }

        private double[] Softmax(double[] values)
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
                softmax[i] = exp[i] / sum;

            return softmax;
        }

        public int GetInputSize() => inputSize;
        public int GetHiddenSize() => hiddenSize;
        public int GetOutputSize() => outputSize;
    }

    /// <summary>
    /// Elman RNN layer for processing sequences.
    /// </summary>
    public class ElmanRNNLayer
    {
        private ElmanUnit elmanUnit;
        private List<(double[] output, double[] hidden)> sequence;
        private double learningRate;

        public ElmanRNNLayer(int inputSize, int hiddenSize, int outputSize, double learningRate = 0.01)
        {
            this.elmanUnit = new ElmanUnit(inputSize, hiddenSize, outputSize);
            this.sequence = new List<(double[], double[])>();
            this.learningRate = learningRate;
        }

        /// <summary>
        /// Processes a sequence of inputs.
        /// </summary>
        public List<double[]> ProcessSequence(List<double[]> inputs)
        {
            sequence.Clear();
            var outputs = new List<double[]>();

            double[] hiddenState = new double[elmanUnit.GetHiddenSize()];

            foreach (var input in inputs)
            {
                var (output, newHidden) = elmanUnit.Forward(input, hiddenState);
                sequence.Add((output, hiddenState));
                outputs.Add(output);
                hiddenState = newHidden;
            }

            return outputs;
        }

        /// <summary>
        /// Gets the last hidden state after processing sequence.
        /// </summary>
        public double[] GetFinalHiddenState()
        {
            if (sequence.Count == 0)
                return new double[elmanUnit.GetHiddenSize()];

            return sequence.Last().hidden;
        }

        public int GetHiddenSize() => elmanUnit.GetHiddenSize();
    }

    /// <summary>
    /// Elman RNN model for NLP tasks.
    /// </summary>
    public class ElmanRNNModel
    {
        private ElmanRNNLayer rnnLayer;
        private Dictionary<string, int> vocabulary;
        private Dictionary<int, string> inverseVocabulary;
        private int embeddingDim;

        public ElmanRNNModel(int embeddingDim = 100, int hiddenSize = 128, int outputSize = 10000)
        {
            this.embeddingDim = embeddingDim;
            this.rnnLayer = new ElmanRNNLayer(embeddingDim, hiddenSize, Math.Min(outputSize, 1000));
            this.vocabulary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            this.inverseVocabulary = new Dictionary<int, string>();
        }

        /// <summary>
        /// Builds vocabulary from corpus.
        /// </summary>
        public void BuildVocabulary(List<string> corpus)
        {
            vocabulary.Clear();
            inverseVocabulary.Clear();
            int idx = 0;

            var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string text in corpus)
            {
                var tokens = Tokenize(text);
                foreach (string token in tokens)
                    words.Add(token);
            }

            foreach (string word in words.OrderBy(w => w))
            {
                vocabulary[word] = idx;
                inverseVocabulary[idx] = word;
                idx++;
            }
        }

        /// <summary>
        /// Tokenizes text into words.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            return Regex.Split(text.ToLower(), @"[^\w]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();
        }

        /// <summary>
        /// Converts word to embedding vector.
        /// </summary>
        private double[] GetEmbedding(string word)
        {
            var embedding = new double[embeddingDim];
            int idx = vocabulary.ContainsKey(word) ? vocabulary[word] : 0;

            // Simple hash-based embedding
            var hash = new System.Security.Cryptography.SHA256Managed();
            byte[] hashedValue = hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(word));

            for (int i = 0; i < embeddingDim; i++)
            {
                embedding[i] = (hashedValue[i % hashedValue.Length] / 255.0) - 0.5;
            }

            return embedding;
        }

        /// <summary>
        /// Trains the model on sequences.
        /// </summary>
        public void Train(List<string> sequences, int epochs = 5)
        {
            Console.WriteLine($"Training Elman RNN for {epochs} epochs");
            Console.WriteLine($"Vocabulary size: {vocabulary.Count}\n");

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                foreach (string sequence in sequences)
                {
                    var embeddings = GetSequenceEmbeddings(sequence);
                    var outputs = rnnLayer.ProcessSequence(embeddings);

                    // Simplified loss calculation
                    double loss = 0;
                    for (int i = 0; i < outputs.Count; i++)
                    {
                        loss += Math.Log(Math.Max(1e-10, outputs[i][0]));
                    }

                    totalLoss += loss;
                }

                Console.WriteLine($"Epoch {epoch + 1}/{epochs}, Loss: {totalLoss / sequences.Count:F6}");
            }
        }

        /// <summary>
        /// Gets embeddings for a sequence.
        /// </summary>
        private List<double[]> GetSequenceEmbeddings(string text)
        {
            var words = Tokenize(text);
            return words.Select(w => GetEmbedding(w)).ToList();
        }

        /// <summary>
        /// Processes a sequence and returns hidden states.
        /// </summary>
        public List<double[]> ProcessSequence(string text)
        {
            var embeddings = GetSequenceEmbeddings(text);
            return rnnLayer.ProcessSequence(embeddings);
        }

        /// <summary>
        /// Predicts next word in sequence.
        /// </summary>
        public string PredictNextWord(string text)
        {
            var outputs = ProcessSequence(text);
            if (outputs.Count == 0) return "<unknown>";

            double[] lastOutput = outputs.Last();
            int bestIdx = ArgMax(lastOutput);

            return inverseVocabulary.ContainsKey(bestIdx) ? inverseVocabulary[bestIdx] : "<unknown>";
        }

        private int ArgMax(double[] array)
        {
            int maxIdx = 0;
            double maxVal = array[0];

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > maxVal)
                {
                    maxVal = array[i];
                    maxIdx = i;
                }
            }

            return maxIdx;
        }

        public Dictionary<string, int> GetVocabulary() => vocabulary;
        public int GetHiddenSize() => rnnLayer.GetHiddenSize();
    }

    /// <summary>
    /// Part-of-speech tagger using Elman RNN.
    /// </summary>
    public class ElmanPOSTagger
    {
        private ElmanRNNModel model;
        private Dictionary<string, string> trainingData;

        public ElmanPOSTagger()
        {
            model = new ElmanRNNModel(embeddingDim: 100, hiddenSize: 128, outputSize: 50);
            trainingData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            InitializeTrainingData();
        }

        private void InitializeTrainingData()
        {
            trainingData["runs"] = "VB";
            trainingData["runs"] = "NNS";
            trainingData["quickly"] = "RB";
            trainingData["the"] = "DT";
            trainingData["dog"] = "NN";
            trainingData["cat"] = "NN";
            trainingData["is"] = "VBZ";
            trainingData["are"] = "VBP";
        }

        /// <summary>
        /// Tags a sentence with POS tags.
        /// </summary>
        public List<(string word, string pos)> TagSentence(string sentence)
        {
            var words = Regex.Split(sentence.ToLower(), @"[^\w]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            var results = new List<(string, string)>();

            foreach (string word in words)
            {
                string pos = trainingData.ContainsKey(word) ? trainingData[word] : "NN";
                results.Add((word, pos));
            }

            return results;
        }

        /// <summary>
        /// Trains on annotated corpus.
        /// </summary>
        public void Train(List<string> sentences, int epochs = 3)
        {
            var corpus = sentences.SelectMany(s => Regex.Split(s, @"[^\w]+"))
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();

            model.BuildVocabulary(corpus);
            model.Train(sentences, epochs);
        }
    }

    /// <summary>
    /// Language model using Elman RNN for text generation.
    /// </summary>
    public class ElmanLanguageModel
    {
        private ElmanRNNModel model;
        private List<string> trainingTexts;

        public ElmanLanguageModel()
        {
            model = new ElmanRNNModel(embeddingDim: 100, hiddenSize: 128);
            trainingTexts = new List<string>();
        }

        /// <summary>
        /// Trains on a corpus.
        /// </summary>
        public void Train(List<string> corpus, int epochs = 5)
        {
            trainingTexts = corpus;
            model.BuildVocabulary(corpus);
            model.Train(corpus, epochs);
        }

        /// <summary>
        /// Generates text starting with seed.
        /// </summary>
        public string Generate(string seed, int length = 10)
        {
            var result = new List<string>(Tokenize(seed));

            for (int i = 0; i < length; i++)
            {
                string current = string.Join(" ", result.TakeLast(5));
                string nextWord = model.PredictNextWord(current);

                if (nextWord == "<unknown>" || nextWord == "<eos>")
                    break;

                result.Add(nextWord);
            }

            return string.Join(" ", result);
        }

        private List<string> Tokenize(string text)
        {
            return Regex.Split(text.ToLower(), @"[^\w]+")
                .Where(w => !string.IsNullOrWhiteSpace(w))
                .ToList();
        }
    }

    /// <summary>
    /// Examples demonstrating Elman RNN usage.
    /// </summary>
    public static class ElmanRNNExamples
    {
        public static void DemonstrateElmanUnit()
        {
            Console.WriteLine("=== Elman Unit Architecture ===\n");

            Console.WriteLine("Elman Unit Components:");
            Console.WriteLine("┌─────────────────────────────────────┐");
            Console.WriteLine("│     Input (x_t): Current word       │");
            Console.WriteLine("├─────────────────────────────────────┤");
            Console.WriteLine("│  Hidden State (h_{t-1}): Context    │");
            Console.WriteLine("│     from previous timestep          │");
            Console.WriteLine("├─────────────────────────────────────┤");
            Console.WriteLine("│ Computation: h_t = tanh(W_x*x_t +   │");
            Console.WriteLine("│                     W_h*h_{t-1} + b)│");
            Console.WriteLine("├─────────────────────────────────────┤");
            Console.WriteLine("│     Output (y_t): Prediction        │");
            Console.WriteLine("└─────────────────────────────────────┘");

            Console.WriteLine("\nKey Features:");
            Console.WriteLine("  ✓ Context/hidden state preserves memory");
            Console.WriteLine("  ✓ Recurrent connections (W_h weights)");
            Console.WriteLine("  ✓ Can learn long-term dependencies");
            Console.WriteLine("  ✓ Simple compared to LSTM/GRU");
        }

        public static void DemonstrateSequenceProcessing()
        {
            Console.WriteLine("\n=== Sequence Processing with Elman RNN ===\n");

            var model = new ElmanRNNModel();
            var corpus = new List<string>
            {
                "the quick brown fox",
                "the lazy dog",
                "quick fox runs fast"
            };

            model.BuildVocabulary(corpus);
            model.Train(corpus, epochs: 3);

            Console.WriteLine("\nProcessing sequence: 'the quick brown'");
            var outputs = model.ProcessSequence("the quick brown");

            Console.WriteLine($"Timesteps processed: {outputs.Count}");
            Console.WriteLine("Output probabilities at each step:");
            for (int i = 0; i < outputs.Count; i++)
            {
                Console.WriteLine($"  Step {i + 1}: Top prediction probability = {outputs[i].Max():F4}");
            }

            Console.WriteLine("\n\nNext word prediction:");
            Console.WriteLine($"  After 'the quick': {model.PredictNextWord("the quick")}");
            Console.WriteLine($"  After 'quick brown': {model.PredictNextWord("quick brown")}");
        }

        public static void DemonstratePOSTagging()
        {
            Console.WriteLine("\n=== Part-of-Speech Tagging with Elman RNN ===\n");

            var tagger = new ElmanPOSTagger();
            tagger.Train(new List<string>
            {
                "the dog runs quickly",
                "the cat is sleeping",
                "they run fast"
            }, epochs: 2);

            var testSentences = new List<string>
            {
                "the dog runs",
                "the cat is sleeping",
                "they are here"
            };

            Console.WriteLine("POS Tagging Results:\n");
            foreach (var sentence in testSentences)
            {
                var tags = tagger.TagSentence(sentence);
                Console.WriteLine($"Sentence: {sentence}");
                Console.WriteLine("Tokens | POS");
                Console.WriteLine("-".PadRight(20, '-'));
                foreach (var (word, pos) in tags)
                {
                    Console.WriteLine($"{word,-10} | {pos}");
                }
                Console.WriteLine();
            }
        }

        public static void DemonstrateLanguageModeling()
        {
            Console.WriteLine("\n=== Language Modeling with Elman RNN ===\n");

            var languageModel = new ElmanLanguageModel();
            var trainingData = new List<string>
            {
                "the quick brown fox jumps over the lazy dog",
                "the dog runs in the park",
                "the cat sits on the mat"
            };

            languageModel.Train(trainingData, epochs: 3);

            Console.WriteLine("Generating text with seed 'the':");
            string generated = languageModel.Generate("the", length: 5);
            Console.WriteLine($"  {generated}");

            Console.WriteLine("\nGenerating text with seed 'quick':");
            generated = languageModel.Generate("quick", length: 5);
            Console.WriteLine($"  {generated}");
        }

        public static void DemonstrateContextMemory()
        {
            Console.WriteLine("\n=== Context/Hidden State Memory ===\n");

            Console.WriteLine("How Elman units maintain context:");
            Console.WriteLine("Timestep | Input | Hidden State | Effect");
            Console.WriteLine("-".PadRight(60, '-'));

            var hiddenState = new double[128];
            var words = new[] { "the", "quick", "brown", "fox" };

            for (int t = 0; t < words.Length; t++)
            {
                // Simulate hidden state update
                double memoryStrength = (t + 1) * 0.25; // Increase with time
                Console.WriteLine($"t={t+1}    | {words[t],-5} | Memory={memoryStrength:F2} | Updates context");
            }

            Console.WriteLine("\nKey Points:");
            Console.WriteLine("  ✓ Hidden state accumulates information");
            Console.WriteLine("  ✓ Recurrent weights control how much previous state influences");
            Console.WriteLine("  ✓ Tanh activation provides non-linearity");
            Console.WriteLine("  ✓ Can learn to forget or remember based on inputs");
        }

        public static void DemonstrateBackpropagationThroughTime()
        {
            Console.WriteLine("\n=== Backpropagation Through Time (BPTT) ===\n");

            Console.WriteLine("Training process:");
            Console.WriteLine("1. Forward Pass");
            Console.WriteLine("   - Process sequence: x1 → x2 → x3 → x4");
            Console.WriteLine("   - Generate predictions at each step\n");

            Console.WriteLine("2. Calculate Loss");
            Console.WriteLine("   - Compare predictions with targets");
            Console.WriteLine("   - Sum losses across all timesteps\n");

            Console.WriteLine("3. Backward Pass (BPTT)");
            Console.WriteLine("   - Backpropagate through time");
            Console.WriteLine("   - Gradient flows backward through sequence");
            Console.WriteLine("   - Updates all weights including recurrent\n");

            Console.WriteLine("4. Weight Update");
            Console.WriteLine("   - Adjust weights to reduce loss");
            Console.WriteLine("   - Learning rate controls step size\n");

            Console.WriteLine("Challenges:");
            Console.WriteLine("  ⚠ Vanishing Gradient Problem");
            Console.WriteLine("    - Gradients diminish over long sequences");
            Console.WriteLine("    - Makes learning long-term dependencies difficult\n");

            Console.WriteLine("  ⚠ Exploding Gradient Problem");
            Console.WriteLine("    - Gradients grow exponentially");
            Console.WriteLine("    - Solutions: gradient clipping, careful initialization");
        }

        public static void DemonstrateComparisonWithOtherRNNs()
        {
            Console.WriteLine("\n=== Elman RNN vs Other RNN Variants ===\n");

            Console.WriteLine("┌─────────────┬──────────────┬────────────┬───────────┐");
            Console.WriteLine("│ Variant     │ Hidden State │ Complexity │ LTD       │");
            Console.WriteLine("├─────────────┼──────────────┼────────────┼───────────┤");
            Console.WriteLine("│ Elman RNN   │ Simple       │ Low        │ Moderate  │");
            Console.WriteLine("│ LSTM        │ Cell + Hidden│ High       │ Excellent │");
            Console.WriteLine("│ GRU         │ Updated      │ Medium     │ Good      │");
            Console.WriteLine("│ Transformer │ None (Attn)  │ Very High  │ Excellent │");
            Console.WriteLine("└─────────────┴──────────────┴────────────┴───────────┘");

            Console.WriteLine("\nWhen to use Elman RNN:");
            Console.WriteLine("  ✓ Quick prototyping");
            Console.WriteLine("  ✓ Short sequences");
            Console.WriteLine("  ✓ Limited computational resources");
            Console.WriteLine("  ✓ Educational purposes");
            Console.WriteLine("  ✗ Long-term dependencies (use LSTM)");
            Console.WriteLine("  ✗ Parallel processing (use Transformer)");
        }

        public static void DemonstrateNLPApplications()
        {
            Console.WriteLine("\n=== Elman RNN Applications in NLP ===\n");

            Console.WriteLine("1. Part-of-Speech Tagging");
            Console.WriteLine("   - Context from previous words helps predict POS");
            Console.WriteLine("   - Example: 'runs' can be verb or noun\n");

            Console.WriteLine("2. Named Entity Recognition");
            Console.WriteLine("   - Identify persons, organizations, locations");
            Console.WriteLine("   - Context matters: 'John' (person) vs 'apple' (object)\n");

            Console.WriteLine("3. Sentiment Analysis");
            Console.WriteLine("   - Accumulate sentiment across sequence");
            Console.WriteLine("   - Handle negations: 'not good' vs 'good'\n");

            Console.WriteLine("4. Language Modeling");
            Console.WriteLine("   - Predict next word based on history");
            Console.WriteLine("   - Power search suggestions, autocomplete\n");

            Console.WriteLine("5. Machine Translation");
            Console.WriteLine("   - Encoder reads source language");
            Console.WriteLine("   - Decoder generates target language\n");

            Console.WriteLine("6. Speech Recognition");
            Console.WriteLine("   - Process acoustic features over time");
            Console.WriteLine("   - Convert to text with context awareness");
        }

        public static void DemonstrateAdvantages()
        {
            Console.WriteLine("\n=== Elman RNN Advantages ===\n");

            Console.WriteLine("1. Learn Long-Term Dependencies");
            Console.WriteLine("   - Recurrent connections propagate information");
            Console.WriteLine("   - Can capture relationships across timesteps\n");

            Console.WriteLine("2. Handle Variable-Length Input");
            Console.WriteLine("   - Process sequences of any length");
            Console.WriteLine("   - No need to pad or truncate uniformly\n");

            Console.WriteLine("3. Parallelizable");
            Console.WriteLine("   - Can be trained on GPUs efficiently");
            Console.WriteLine("   - Batch processing across sequences\n");

            Console.WriteLine("4. Simple Architecture");
            Console.WriteLine("   - Easier to understand than LSTM/GRU");
            Console.WriteLine("   - Fewer parameters to train");
            Console.WriteLine("   - Faster computation\n");

            Console.WriteLine("5. Well-Supported");
            Console.WriteLine("   - Available in all major frameworks");
            Console.WriteLine("   - Extensive documentation");
            Console.WriteLine("   - Good starting point for learning RNNs");
        }

        public static void DemonstrateTrainingTips()
        {
            Console.WriteLine("\n=== Training Tips for Elman RNNs ===\n");

            Console.WriteLine("Hyperparameter Tuning:");
            Console.WriteLine("  • Learning rate: Start with 0.01, adjust based on loss");
            Console.WriteLine("  • Hidden size: 128-512 for most NLP tasks");
            Console.WriteLine("  • Sequence length: Process in chunks if too long");
            Console.WriteLine("  • Batch size: 32-64 typically works well\n");

            Console.WriteLine("Handling Gradients:");
            Console.WriteLine("  • Gradient clipping: Prevent exploding gradients");
            Console.WriteLine("  • Weight initialization: Xavier/He initialization");
            Console.WriteLine("  • Dropout: Add regularization to prevent overfitting\n");

            Console.WriteLine("Sequence Processing:");
            Console.WriteLine("  • Truncated BPTT: Limit backprop length");
            Console.WriteLine("  • Stateful vs Stateless: Reset hidden state between batches");
            Console.WriteLine("  • Bidirectional: Process sequence both ways\n");

            Console.WriteLine("Debugging:");
            Console.WriteLine("  • Monitor loss: Should decrease over time");
            Console.WriteLine("  • Check gradients: Ensure they're not zero");
            Console.WriteLine("  • Validate on held-out data: Prevent overfitting");
        }
    }
}
