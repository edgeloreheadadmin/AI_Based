using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Feedforward Neural Network Layer
    /// </summary>
    public class DenseLayer
    {
        public int InputSize { get; set; }
        public int OutputSize { get; set; }
        public double[][] Weights { get; set; }
        public double[] Biases { get; set; }
        public double[][] WeightGradients { get; set; }
        public double[] BiasGradients { get; set; }
        public double[] LastInput { get; set; }
        public double[] LastOutput { get; set; }

        public DenseLayer(int inputSize, int outputSize)
        {
            InputSize = inputSize;
            OutputSize = outputSize;
            Weights = new double[inputSize][];
            Biases = new double[outputSize];
            WeightGradients = new double[inputSize][];
            BiasGradients = new double[outputSize];

            // Initialize weights and gradients
            Random rand = new Random(42);
            double scale = Math.Sqrt(2.0 / inputSize); // Xavier initialization

            for (int i = 0; i < inputSize; i++)
            {
                Weights[i] = new double[outputSize];
                WeightGradients[i] = new double[outputSize];

                for (int j = 0; j < outputSize; j++)
                {
                    Weights[i][j] = (rand.NextDouble() - 0.5) * scale;
                }
            }

            for (int j = 0; j < outputSize; j++)
            {
                Biases[j] = 0;
            }
        }

        public double[] Forward(double[] input)
        {
            LastInput = input;
            double[] output = new double[OutputSize];

            for (int j = 0; j < OutputSize; j++)
            {
                double sum = Biases[j];
                for (int i = 0; i < InputSize; i++)
                {
                    sum += input[i] * Weights[i][j];
                }
                output[j] = sum;
            }

            LastOutput = output;
            return output;
        }

        public double[] Backward(double[] outputGradient, double learningRate, double l2Penalty = 0)
        {
            // Compute input gradients
            double[] inputGradient = new double[InputSize];
            for (int i = 0; i < InputSize; i++)
            {
                double sum = 0;
                for (int j = 0; j < OutputSize; j++)
                {
                    sum += outputGradient[j] * Weights[i][j];
                }
                inputGradient[i] = sum;
            }

            // Compute weight and bias gradients
            for (int i = 0; i < InputSize; i++)
            {
                for (int j = 0; j < OutputSize; j++)
                {
                    WeightGradients[i][j] = outputGradient[j] * LastInput[i];
                    // Add L2 regularization gradient
                    if (l2Penalty > 0)
                        WeightGradients[i][j] += l2Penalty * Weights[i][j];
                }
            }

            for (int j = 0; j < OutputSize; j++)
            {
                BiasGradients[j] = outputGradient[j];
            }

            // Update weights and biases
            for (int i = 0; i < InputSize; i++)
            {
                for (int j = 0; j < OutputSize; j++)
                {
                    Weights[i][j] -= learningRate * WeightGradients[i][j];
                }
            }

            for (int j = 0; j < OutputSize; j++)
            {
                Biases[j] -= learningRate * BiasGradients[j];
            }

            return inputGradient;
        }
    }

    /// <summary>
    /// Activation function wrapper
    /// </summary>
    public class ActivationLayer
    {
        public string ActivationType { get; set; }
        public double[] LastInput { get; set; }
        public double[] LastOutput { get; set; }

        public ActivationLayer(string activationType = "relu")
        {
            ActivationType = activationType;
        }

        public double[] Forward(double[] input)
        {
            LastInput = input;
            LastOutput = new double[input.Length];

            switch (ActivationType.ToLower())
            {
                case "relu":
                    for (int i = 0; i < input.Length; i++)
                        LastOutput[i] = Math.Max(0, input[i]);
                    break;
                case "sigmoid":
                    for (int i = 0; i < input.Length; i++)
                        LastOutput[i] = 1.0 / (1.0 + Math.Exp(-input[i]));
                    break;
                case "tanh":
                    for (int i = 0; i < input.Length; i++)
                        LastOutput[i] = Math.Tanh(input[i]);
                    break;
                case "softmax":
                    LastOutput = Softmax(input);
                    break;
                case "linear":
                    Array.Copy(input, LastOutput, input.Length);
                    break;
            }

            return LastOutput;
        }

        public double[] Backward(double[] outputGradient)
        {
            double[] inputGradient = new double[outputGradient.Length];

            switch (ActivationType.ToLower())
            {
                case "relu":
                    for (int i = 0; i < outputGradient.Length; i++)
                        inputGradient[i] = LastInput[i] > 0 ? outputGradient[i] : 0;
                    break;
                case "sigmoid":
                    for (int i = 0; i < outputGradient.Length; i++)
                    {
                        double sig = 1.0 / (1.0 + Math.Exp(-LastInput[i]));
                        inputGradient[i] = outputGradient[i] * sig * (1 - sig);
                    }
                    break;
                case "tanh":
                    for (int i = 0; i < outputGradient.Length; i++)
                    {
                        double t = Math.Tanh(LastInput[i]);
                        inputGradient[i] = outputGradient[i] * (1 - t * t);
                    }
                    break;
                case "softmax":
                    inputGradient = outputGradient;
                    break;
                case "linear":
                    Array.Copy(outputGradient, inputGradient, outputGradient.Length);
                    break;
            }

            return inputGradient;
        }

        private double[] Softmax(double[] input)
        {
            double[] output = new double[input.Length];
            double maxInput = input.Max();
            double sum = 0;

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Math.Exp(input[i] - maxInput);
                sum += output[i];
            }

            for (int i = 0; i < output.Length; i++)
                output[i] /= sum;

            return output;
        }
    }

    /// <summary>
    /// Complete feedforward neural network
    /// </summary>
    public class FeedforwardNeuralNetwork
    {
        public List<DenseLayer> DenseLayers { get; set; }
        public List<ActivationLayer> ActivationLayers { get; set; }
        public double LearningRate { get; set; }
        public double L2Penalty { get; set; }
        public string LossFunction { get; set; }

        public FeedforwardNeuralNetwork(double learningRate = 0.01, string lossFunction = "mse", double l2Penalty = 0)
        {
            DenseLayers = new List<DenseLayer>();
            ActivationLayers = new List<ActivationLayer>();
            LearningRate = learningRate;
            LossFunction = lossFunction;
            L2Penalty = l2Penalty;
        }

        public void AddLayer(int inputSize, int outputSize, string activation = "relu")
        {
            DenseLayers.Add(new DenseLayer(inputSize, outputSize));
            ActivationLayers.Add(new ActivationLayer(activation));
        }

        public double[] Forward(double[] input)
        {
            double[] current = input;

            for (int i = 0; i < DenseLayers.Count; i++)
            {
                current = DenseLayers[i].Forward(current);
                current = ActivationLayers[i].Forward(current);
            }

            return current;
        }

        public void Backward(double[] outputGradient)
        {
            double[] gradient = outputGradient;

            for (int i = DenseLayers.Count - 1; i >= 0; i--)
            {
                gradient = ActivationLayers[i].Backward(gradient);
                gradient = DenseLayers[i].Backward(gradient, LearningRate, L2Penalty);
            }
        }

        public double ComputeLoss(double[] output, double[] target)
        {
            switch (LossFunction.ToLower())
            {
                case "mse":
                    double mseLoss = 0;
                    for (int i = 0; i < output.Length; i++)
                    {
                        double error = output[i] - target[i];
                        mseLoss += error * error;
                    }
                    return mseLoss / output.Length;

                case "crossentropy":
                    double ceLoss = 0;
                    for (int i = 0; i < output.Length; i++)
                    {
                        if (output[i] > 0)
                            ceLoss -= target[i] * Math.Log(output[i]);
                    }
                    return ceLoss;

                default:
                    return 0;
            }
        }

        public double[] GetLossGradient(double[] output, double[] target)
        {
            double[] gradient = new double[output.Length];

            switch (LossFunction.ToLower())
            {
                case "mse":
                    for (int i = 0; i < output.Length; i++)
                        gradient[i] = 2.0 * (output[i] - target[i]) / output.Length;
                    break;

                case "crossentropy":
                    for (int i = 0; i < output.Length; i++)
                        gradient[i] = -target[i] / (output[i] + 1e-8);
                    break;
            }

            return gradient;
        }

        public void Train(List<(double[], double[])> trainingData, int epochs = 100, int batchSize = 32)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                // Shuffle data
                var shuffled = trainingData.OrderBy(_ => Guid.NewGuid()).ToList();

                // Mini-batch training
                for (int batch = 0; batch < shuffled.Count; batch += batchSize)
                {
                    int batchEnd = Math.Min(batch + batchSize, shuffled.Count);

                    for (int i = batch; i < batchEnd; i++)
                    {
                        var (input, target) = shuffled[i];

                        // Forward pass
                        double[] output = Forward(input);

                        // Compute loss
                        double loss = ComputeLoss(output, target);
                        totalLoss += loss;

                        // Backward pass
                        double[] gradient = GetLossGradient(output, target);
                        Backward(gradient);
                    }
                }

                if (epoch % 10 == 0)
                    Console.WriteLine($"Epoch {epoch}: Loss = {totalLoss / shuffled.Count:F4}");
            }
        }

        public double[] Predict(double[] input)
        {
            return Forward(input);
        }
    }

    /// <summary>
    /// Text classifier using feedforward neural network
    /// </summary>
    public class NNTextClassifier
    {
        public FeedforwardNeuralNetwork Network { get; set; }
        public Dictionary<string, int> WordToIndex { get; set; }
        public List<string> Classes { get; set; }

        public NNTextClassifier(int vocabSize, int hiddenSize, List<string> classes)
        {
            Classes = classes;
            WordToIndex = new Dictionary<string, int>();

            // Build network: vocab -> hidden -> classes
            Network = new FeedforwardNeuralNetwork(learningRate: 0.01, lossFunction: "crossentropy");
            Network.AddLayer(vocabSize, hiddenSize, "relu");
            Network.AddLayer(hiddenSize, hiddenSize / 2, "relu");
            Network.AddLayer(hiddenSize / 2, classes.Count, "softmax");
        }

        public void BuildVocabulary(List<List<string>> texts)
        {
            WordToIndex.Clear();
            int index = 0;

            foreach (var text in texts)
            {
                foreach (var word in text)
                {
                    if (!WordToIndex.ContainsKey(word))
                        WordToIndex[word] = index++;
                }
            }
        }

        public double[] TextToVector(List<string> text)
        {
            double[] vector = new double[WordToIndex.Count];
            foreach (var word in text)
            {
                if (WordToIndex.ContainsKey(word))
                    vector[WordToIndex[word]]++;
            }
            return vector;
        }

        public void Train(List<(List<string>, string)> trainingData, int epochs = 50)
        {
            BuildVocabulary(trainingData.Select(x => x.Item1).ToList());

            var trainingExamples = new List<(double[], double[])>();

            foreach (var (text, className) in trainingData)
            {
                double[] input = TextToVector(text);
                double[] target = new double[Classes.Count];
                target[Classes.IndexOf(className)] = 1.0;
                trainingExamples.Add((input, target));
            }

            Network.Train(trainingExamples, epochs: epochs);
        }

        public string Predict(List<string> text)
        {
            double[] input = TextToVector(text);
            double[] output = Network.Predict(input);

            int maxIndex = 0;
            for (int i = 1; i < output.Length; i++)
            {
                if (output[i] > output[maxIndex])
                    maxIndex = i;
            }

            return Classes[maxIndex];
        }

        public Dictionary<string, double> PredictWithProbabilities(List<string> text)
        {
            double[] input = TextToVector(text);
            double[] output = Network.Predict(input);

            var result = new Dictionary<string, double>();
            for (int i = 0; i < Classes.Count; i++)
            {
                result[Classes[i]] = output[i];
            }

            return result;
        }
    }

    /// <summary>
    /// Sentiment analyzer using feedforward neural network
    /// </summary>
    public class NNSentimentAnalyzer
    {
        public FeedforwardNeuralNetwork Network { get; set; }
        public Dictionary<string, int> WordToIndex { get; set; }
        public int EmbeddingDim { get; set; }

        public NNSentimentAnalyzer(int vocabSize = 1000, int embeddingDim = 50, int hiddenSize = 128)
        {
            EmbeddingDim = embeddingDim;
            WordToIndex = new Dictionary<string, int>();

            // Network: vocab -> embedding -> hidden -> binary output
            Network = new FeedforwardNeuralNetwork(learningRate: 0.01, lossFunction: "mse");
            Network.AddLayer(vocabSize, hiddenSize, "relu");
            Network.AddLayer(hiddenSize, hiddenSize / 2, "relu");
            Network.AddLayer(hiddenSize / 2, 1, "sigmoid");
        }

        public void BuildVocabulary(List<string> texts)
        {
            WordToIndex.Clear();
            int index = 0;

            foreach (var text in texts)
            {
                var words = text.Split(' ');
                foreach (var word in words)
                {
                    if (!WordToIndex.ContainsKey(word))
                        WordToIndex[word] = index++;
                }
            }
        }

        public double[] TextToVector(string text)
        {
            double[] vector = new double[WordToIndex.Count];
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (WordToIndex.ContainsKey(word))
                    vector[WordToIndex[word]]++;
            }

            // Normalize
            double sum = vector.Sum();
            if (sum > 0)
            {
                for (int i = 0; i < vector.Length; i++)
                    vector[i] /= sum;
            }

            return vector;
        }

        public void Train(List<(string, double)> trainingData, int epochs = 50)
        {
            BuildVocabulary(trainingData.Select(x => x.Item1).ToList());

            var trainingExamples = new List<(double[], double[])>();

            foreach (var (text, sentiment) in trainingData)
            {
                double[] input = TextToVector(text);
                double[] target = new double[] { sentiment };
                trainingExamples.Add((input, target));
            }

            Network.Train(trainingExamples, epochs: epochs);
        }

        public double PredictSentiment(string text)
        {
            double[] input = TextToVector(text);
            double[] output = Network.Predict(input);
            return output[0];
        }

        public string PredictSentimentLabel(string text)
        {
            double sentiment = PredictSentiment(text);
            if (sentiment > 0.5)
                return "Positive";
            else if (sentiment < 0.5)
                return "Negative";
            else
                return "Neutral";
        }
    }

    /// <summary>
    /// Feedforward Neural Network examples and demonstrations
    /// </summary>
    public class FeedforwardNNExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Basic Feedforward Neural Network ===");
            var network = new FeedforwardNeuralNetwork(learningRate: 0.1, lossFunction: "mse");
            network.AddLayer(2, 4, "relu");    // Input -> Hidden1
            network.AddLayer(4, 3, "relu");    // Hidden1 -> Hidden2
            network.AddLayer(3, 1, "sigmoid"); // Hidden2 -> Output

            // XOR problem training data
            var trainingData = new List<(double[], double[])>
            {
                (new double[] { 0, 0 }, new double[] { 0 }),
                (new double[] { 0, 1 }, new double[] { 1 }),
                (new double[] { 1, 0 }, new double[] { 1 }),
                (new double[] { 1, 1 }, new double[] { 0 }),
            };

            Console.WriteLine("Training XOR problem...");
            network.Train(trainingData, epochs: 200);

            Console.WriteLine("\nXOR Predictions:");
            foreach (var (input, target) in trainingData)
            {
                double[] output = network.Predict(input);
                Console.WriteLine($"Input: [{input[0]}, {input[1]}] -> Output: {output[0]:F4} (Target: {target[0]})");
            }

            Console.WriteLine("\n=== Text Classification with Feedforward Network ===");
            var classifier = new NNTextClassifier(
                vocabSize: 100,
                hiddenSize: 64,
                classes: new List<string> { "Sports", "Politics", "Technology" }
            );

            var trainingTexts = new List<(List<string>, string)>
            {
                (new List<string> { "football", "game", "team", "player" }, "Sports"),
                (new List<string> { "basketball", "championship", "score" }, "Sports"),
                (new List<string> { "election", "vote", "government", "law" }, "Politics"),
                (new List<string> { "political", "debate", "senate" }, "Politics"),
                (new List<string> { "computer", "software", "algorithm", "code" }, "Technology"),
                (new List<string> { "artificial", "intelligence", "machine", "learning" }, "Technology"),
            };

            Console.WriteLine("Training text classifier...");
            classifier.Train(trainingTexts, epochs: 100);

            var testTexts = new List<List<string>>
            {
                new List<string> { "football", "game", "player" },
                new List<string> { "election", "government" },
                new List<string> { "software", "code" },
            };

            Console.WriteLine("\nText Classification Predictions:");
            foreach (var text in testTexts)
            {
                string prediction = classifier.Predict(text);
                var probs = classifier.PredictWithProbabilities(text);
                Console.WriteLine($"Text: {string.Join(", ", text)}");
                Console.WriteLine($"  Predicted: {prediction}");
                Console.WriteLine($"  Probabilities: {string.Join(", ", probs.Select(x => $"{x.Key}:{x.Value:F3}"))}");
            }

            Console.WriteLine("\n=== Sentiment Analysis ===");
            var sentimentAnalyzer = new NNSentimentAnalyzer();

            var sentimentData = new List<(string, double)>
            {
                ("this movie is amazing wonderful great", 1.0),
                ("i love this wonderful film", 1.0),
                ("terrible horrible bad movie", 0.0),
                ("worst film ever made", 0.0),
                ("good movie interesting plot", 1.0),
                ("not good bad acting", 0.0),
            };

            Console.WriteLine("Training sentiment analyzer...");
            sentimentAnalyzer.Train(sentimentData, epochs: 100);

            var testSentences = new List<string>
            {
                "amazing wonderful movie",
                "terrible bad film",
                "interesting good plot",
            };

            Console.WriteLine("\nSentiment Analysis Results:");
            foreach (var sentence in testSentences)
            {
                double sentiment = sentimentAnalyzer.PredictSentiment(sentence);
                string label = sentimentAnalyzer.PredictSentimentLabel(sentence);
                Console.WriteLine($"Text: \"{sentence}\"");
                Console.WriteLine($"  Sentiment Score: {sentiment:F4}");
                Console.WriteLine($"  Label: {label}");
            }

            Console.WriteLine("\n=== Feedforward Network Architecture ===");
            Console.WriteLine("Input Layer -> Hidden Layer(s) -> Output Layer");
            Console.WriteLine("Information flows in one direction (no cycles)");
            Console.WriteLine("Each layer learns hierarchical representations");
            Console.WriteLine("Deep networks can learn complex patterns");

            Console.WriteLine("\n=== Activation Functions Used ===");
            Console.WriteLine("- ReLU: Hidden layers (introduces non-linearity)");
            Console.WriteLine("- Sigmoid: Binary classification output (0-1 range)");
            Console.WriteLine("- Softmax: Multi-class classification output (probabilities)");
            Console.WriteLine("- Linear: Regression output");

            Console.WriteLine("\n=== Training Techniques ===");
            Console.WriteLine("- Forward Propagation: Compute output");
            Console.WriteLine("- Backward Propagation: Compute gradients");
            Console.WriteLine("- Gradient Descent: Update weights");
            Console.WriteLine("- Mini-batch Training: Faster convergence");
            Console.WriteLine("- L2 Regularization: Prevent overfitting");

            Console.WriteLine("\n=== Advantages of Feedforward Networks ===");
            Console.WriteLine("- Simple to understand and implement");
            Console.WriteLine("- Can learn complex non-linear patterns");
            Console.WriteLine("- Well-supported by ML libraries");
            Console.WriteLine("- Scalable to large datasets");
            Console.WriteLine("- Can be trained on GPUs");
            Console.WriteLine("- Good baseline for many NLP tasks");

            Console.WriteLine("\n=== NLP Applications ===");
            Console.WriteLine("1. Text Classification");
            Console.WriteLine("   - Document categorization");
            Console.WriteLine("   - Sentiment analysis");
            Console.WriteLine("   - Spam detection");
            Console.WriteLine("2. Machine Translation");
            Console.WriteLine("   - Word alignment");
            Console.WriteLine("   - Scoring translation candidates");
            Console.WriteLine("3. Question Answering");
            Console.WriteLine("   - Answer selection");
            Console.WriteLine("   - Relevance ranking");
            Console.WriteLine("4. Named Entity Recognition");
            Console.WriteLine("   - Entity type classification");
            Console.WriteLine("   - Token-level classification");

            Console.WriteLine("\n=== Comparison with Other Architectures ===");
            Console.WriteLine("Feedforward vs RNN:");
            Console.WriteLine("  - Feedforward: Processes fixed inputs, no state");
            Console.WriteLine("  - RNN: Processes sequences, maintains state");
            Console.WriteLine("Feedforward vs CNN:");
            Console.WriteLine("  - Feedforward: Dense connections, fully connected");
            Console.WriteLine("  - CNN: Local connections, weight sharing");
            Console.WriteLine("Feedforward vs Transformer:");
            Console.WriteLine("  - Feedforward: Simple, fast");
            Console.WriteLine("  - Transformer: Attention-based, more powerful");

            Console.WriteLine("\n=== Hyperparameter Tuning ===");
            Console.WriteLine("- Learning Rate: Controls update magnitude");
            Console.WriteLine("- Hidden Layers: Affects model capacity");
            Console.WriteLine("- Hidden Units: Width of each layer");
            Console.WriteLine("- Activation Function: Non-linearity type");
            Console.WriteLine("- Regularization: Prevents overfitting");
            Console.WriteLine("- Batch Size: Training efficiency");
            Console.WriteLine("- Epochs: Training iterations");
        }
    }
}
