using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Simple Linear Regression for single variable prediction
    /// y = mx + b
    /// </summary>
    public class SimpleLinearRegression
    {
        public double Slope { get; set; }
        public double Intercept { get; set; }
        public double RSquared { get; set; }

        public SimpleLinearRegression()
        {
            Slope = 0;
            Intercept = 0;
            RSquared = 0;
        }

        public void Fit(List<double> x, List<double> y)
        {
            if (x.Count != y.Count || x.Count == 0)
                throw new ArgumentException("X and Y must have same length and not be empty");

            double meanX = x.Average();
            double meanY = y.Average();

            double numerator = 0;
            double denominator = 0;
            double ssRes = 0;
            double ssTot = 0;

            for (int i = 0; i < x.Count; i++)
            {
                numerator += (x[i] - meanX) * (y[i] - meanY);
                denominator += (x[i] - meanX) * (x[i] - meanX);
                ssTot += (y[i] - meanY) * (y[i] - meanY);
            }

            Slope = denominator != 0 ? numerator / denominator : 0;
            Intercept = meanY - Slope * meanX;

            // Calculate R-squared
            for (int i = 0; i < x.Count; i++)
            {
                double predicted = Predict(x[i]);
                ssRes += (y[i] - predicted) * (y[i] - predicted);
            }

            RSquared = ssTot != 0 ? 1.0 - (ssRes / ssTot) : 0;
        }

        public double Predict(double x)
        {
            return Slope * x + Intercept;
        }

        public void PrintModel()
        {
            Console.WriteLine($"Linear Regression Model: y = {Slope:F4}x + {Intercept:F4}");
            Console.WriteLine($"R-squared: {RSquared:F4}");
        }
    }

    /// <summary>
    /// Multiple Linear Regression for multiple features
    /// y = b0 + b1*x1 + b2*x2 + ... + bn*xn
    /// </summary>
    public class MultipleLinearRegression
    {
        public double[] Coefficients { get; set; }
        public double Intercept { get; set; }
        public double LearningRate { get; set; }
        public double L2Penalty { get; set; }

        public MultipleLinearRegression(int numFeatures, double learningRate = 0.01, double l2Penalty = 0.0)
        {
            Coefficients = new double[numFeatures];
            Intercept = 0;
            LearningRate = learningRate;
            L2Penalty = l2Penalty;
        }

        public void Fit(List<double[]> X, List<double> y, int epochs = 100)
        {
            if (X.Count == 0 || X[0].Length != Coefficients.Length)
                throw new ArgumentException("Feature dimension mismatch");

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalError = 0;

                for (int i = 0; i < X.Count; i++)
                {
                    double predicted = Predict(X[i]);
                    double error = predicted - y[i];
                    totalError += error * error;

                    // Update intercept
                    Intercept -= LearningRate * error;

                    // Update coefficients
                    for (int j = 0; j < Coefficients.Length; j++)
                    {
                        double gradient = error * X[i][j];
                        if (L2Penalty > 0)
                            gradient += L2Penalty * Coefficients[j];

                        Coefficients[j] -= LearningRate * gradient;
                    }
                }

                if (epoch % 20 == 0)
                    Console.WriteLine($"Epoch {epoch}: MSE = {totalError / X.Count:F4}");
            }
        }

        public double Predict(double[] features)
        {
            double result = Intercept;
            for (int i = 0; i < features.Length; i++)
                result += Coefficients[i] * features[i];
            return result;
        }

        public void PrintModel()
        {
            Console.WriteLine("Multiple Linear Regression Model:");
            Console.WriteLine($"y = {Intercept:F4}");
            for (int i = 0; i < Coefficients.Length; i++)
                Console.WriteLine($"  + {Coefficients[i]:F4} * x{i}");
        }
    }

    /// <summary>
    /// Logistic Regression for binary classification
    /// Uses sigmoid function for probability output
    /// </summary>
    public class LogisticRegression
    {
        public double[] Coefficients { get; set; }
        public double Intercept { get; set; }
        public double LearningRate { get; set; }

        public LogisticRegression(int numFeatures, double learningRate = 0.01)
        {
            Coefficients = new double[numFeatures];
            Intercept = 0;
            LearningRate = learningRate;
        }

        private double Sigmoid(double x)
        {
            if (x < -500)
                return 0;
            if (x > 500)
                return 1;
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public void Fit(List<double[]> X, List<int> y, int epochs = 100)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                for (int i = 0; i < X.Count; i++)
                {
                    double z = Intercept;
                    for (int j = 0; j < Coefficients.Length; j++)
                        z += Coefficients[j] * X[i][j];

                    double prediction = Sigmoid(z);
                    double error = prediction - y[i];

                    // Binary cross-entropy loss
                    double loss = -y[i] * Math.Log(prediction + 1e-10) - (1 - y[i]) * Math.Log(1 - prediction + 1e-10);
                    totalLoss += loss;

                    // Update intercept
                    Intercept -= LearningRate * error;

                    // Update coefficients
                    for (int j = 0; j < Coefficients.Length; j++)
                        Coefficients[j] -= LearningRate * error * X[i][j];
                }

                if (epoch % 20 == 0)
                    Console.WriteLine($"Epoch {epoch}: Loss = {totalLoss / X.Count:F4}");
            }
        }

        public double Predict(double[] features)
        {
            double z = Intercept;
            for (int i = 0; i < features.Length; i++)
                z += Coefficients[i] * features[i];
            return Sigmoid(z);
        }

        public int PredictClass(double[] features, double threshold = 0.5)
        {
            return Predict(features) >= threshold ? 1 : 0;
        }
    }

    /// <summary>
    /// Support Vector Machine for binary classification
    /// Simplified SVM using soft-margin optimization
    /// </summary>
    public class SupportVectorMachine
    {
        public double[] Weights { get; set; }
        public double Bias { get; set; }
        public double LearningRate { get; set; }
        public double Lambda { get; set; }

        public SupportVectorMachine(int numFeatures, double learningRate = 0.01, double lambda = 0.01)
        {
            Weights = new double[numFeatures];
            Bias = 0;
            LearningRate = learningRate;
            Lambda = lambda;
        }

        public void Fit(List<double[]> X, List<int> y, int epochs = 100)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                for (int i = 0; i < X.Count; i++)
                {
                    double prediction = Predict(X[i]);
                    int label = 2 * y[i] - 1; // Convert 0,1 to -1,1

                    // Hinge loss
                    double loss = Math.Max(0, 1 - label * prediction);
                    totalLoss += loss;

                    if (loss > 0)
                    {
                        // Update weights
                        for (int j = 0; j < Weights.Length; j++)
                            Weights[j] -= LearningRate * (Lambda * Weights[j] - label * X[i][j]);

                        // Update bias
                        Bias -= LearningRate * (-label);
                    }
                    else
                    {
                        // Only L2 regularization update
                        for (int j = 0; j < Weights.Length; j++)
                            Weights[j] -= LearningRate * Lambda * Weights[j];
                    }
                }

                if (epoch % 20 == 0)
                    Console.WriteLine($"Epoch {epoch}: Hinge Loss = {totalLoss / X.Count:F4}");
            }
        }

        public double Predict(double[] features)
        {
            double result = Bias;
            for (int i = 0; i < features.Length; i++)
                result += Weights[i] * features[i];
            return result;
        }

        public int PredictClass(double[] features)
        {
            return Predict(features) >= 0 ? 1 : 0;
        }
    }

    /// <summary>
    /// Feature extractor for NLP texts
    /// Converts text to numerical feature vectors
    /// </summary>
    public class NLPFeatureExtractor
    {
        public Dictionary<string, int> WordToIndex { get; set; }
        public int VocabularySize { get; set; }

        public NLPFeatureExtractor()
        {
            WordToIndex = new Dictionary<string, int>();
            VocabularySize = 0;
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

            VocabularySize = WordToIndex.Count;
        }

        public double[] ExtractBagOfWords(string text)
        {
            double[] features = new double[VocabularySize];
            var words = text.Split(' ');

            foreach (var word in words)
            {
                if (WordToIndex.ContainsKey(word))
                    features[WordToIndex[word]]++;
            }

            return features;
        }

        public double[] ExtractNormalized(string text)
        {
            double[] features = ExtractBagOfWords(text);
            double sum = features.Sum();

            if (sum > 0)
            {
                for (int i = 0; i < features.Length; i++)
                    features[i] /= sum;
            }

            return features;
        }

        public double[] ExtractCustomFeatures(string text)
        {
            var words = text.Split(' ');
            double[] features = new double[VocabularySize + 5];

            // Bag of words features
            for (int i = 0; i < VocabularySize; i++)
            {
                foreach (var word in words)
                {
                    if (WordToIndex.ContainsKey(word) && WordToIndex[word] == i)
                        features[i]++;
                }
            }

            // Additional features
            features[VocabularySize] = text.Length; // Text length
            features[VocabularySize + 1] = words.Length; // Word count
            features[VocabularySize + 2] = CountPunctuation(text); // Punctuation count
            features[VocabularySize + 3] = CountCapitalLetters(text); // Capital letters
            features[VocabularySize + 4] = CountNumbers(text); // Numbers

            return features;
        }

        private int CountPunctuation(string text)
        {
            return text.Count(c => ".,!?;:".Contains(c));
        }

        private int CountCapitalLetters(string text)
        {
            return text.Count(c => char.IsUpper(c));
        }

        private int CountNumbers(string text)
        {
            return text.Count(c => char.IsDigit(c));
        }
    }

    /// <summary>
    /// Sentiment analyzer using linear regression
    /// </summary>
    public class LinearRegressionSentimentAnalyzer
    {
        public LogisticRegression Model { get; set; }
        public NLPFeatureExtractor FeatureExtractor { get; set; }

        public LinearRegressionSentimentAnalyzer(int vocabSize)
        {
            Model = new LogisticRegression(numFeatures: vocabSize, learningRate: 0.01);
            FeatureExtractor = new NLPFeatureExtractor();
        }

        public void Train(List<(string, int)> trainingData)
        {
            var texts = trainingData.Select(x => x.Item1).ToList();
            FeatureExtractor.BuildVocabulary(texts);

            var X = new List<double[]>();
            var y = new List<int>();

            foreach (var (text, label) in trainingData)
            {
                X.Add(FeatureExtractor.ExtractNormalized(text));
                y.Add(label);
            }

            Model.Fit(X, y, epochs: 50);
        }

        public double PredictSentiment(string text)
        {
            double[] features = FeatureExtractor.ExtractNormalized(text);
            return Model.Predict(features);
        }

        public int PredictSentimentClass(string text)
        {
            return Model.PredictClass(FeatureExtractor.ExtractNormalized(text));
        }
    }

    /// <summary>
    /// Topic classifier using SVM
    /// </summary>
    public class SVMTopicClassifier
    {
        public SupportVectorMachine Model { get; set; }
        public NLPFeatureExtractor FeatureExtractor { get; set; }
        public string Topic { get; set; }

        public SVMTopicClassifier(string topic, int vocabSize)
        {
            Topic = topic;
            Model = new SupportVectorMachine(numFeatures: vocabSize, learningRate: 0.01, lambda: 0.01);
            FeatureExtractor = new NLPFeatureExtractor();
        }

        public void Train(List<(string, bool)> trainingData)
        {
            var texts = trainingData.Select(x => x.Item1).ToList();
            FeatureExtractor.BuildVocabulary(texts);

            var X = new List<double[]>();
            var y = new List<int>();

            foreach (var (text, isTopic) in trainingData)
            {
                X.Add(FeatureExtractor.ExtractNormalized(text));
                y.Add(isTopic ? 1 : 0);
            }

            Model.Fit(X, y, epochs: 50);
        }

        public bool Predict(string text)
        {
            double[] features = FeatureExtractor.ExtractNormalized(text);
            return Model.PredictClass(features) == 1;
        }

        public double Score(string text)
        {
            double[] features = FeatureExtractor.ExtractNormalized(text);
            return Model.Predict(features);
        }
    }

    /// <summary>
    /// Linear Regression and SVM examples
    /// </summary>
    public class LinearRegressionExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Simple Linear Regression ===");
            var simpleRegression = new SimpleLinearRegression();

            var x = new List<double> { 1, 2, 3, 4, 5 };
            var y = new List<double> { 2, 4, 5, 4, 5 };

            simpleRegression.Fit(x, y);
            simpleRegression.PrintModel();

            Console.WriteLine("\nPredictions:");
            foreach (var xVal in x)
            {
                Console.WriteLine($"x = {xVal}: y = {simpleRegression.Predict(xVal):F2}");
            }

            Console.WriteLine("\n=== Multiple Linear Regression ===");
            var multiRegression = new MultipleLinearRegression(numFeatures: 3, learningRate: 0.01);

            var X = new List<double[]>
            {
                new double[] { 1, 2, 3 },
                new double[] { 2, 3, 4 },
                new double[] { 3, 4, 5 },
                new double[] { 4, 5, 6 },
                new double[] { 5, 6, 7 }
            };

            var yMulti = new List<double> { 14, 20, 26, 32, 38 };

            multiRegression.Fit(X, yMulti, epochs: 100);
            multiRegression.PrintModel();

            Console.WriteLine("\n=== Logistic Regression for Sentiment ===");
            var logisticRegression = new LogisticRegression(numFeatures: 3, learningRate: 0.1);

            var Xlog = new List<double[]>
            {
                new double[] { 1, 1, 1 }, // Positive features
                new double[] { 1, 0, 1 },
                new double[] { 0, 1, 1 },
                new double[] { 0, 0, 0 }, // Negative features
                new double[] { 0, 0, 1 },
                new double[] { 1, 0, 0 }
            };

            var yLog = new List<int> { 1, 1, 1, 0, 0, 0 };

            logisticRegression.Fit(Xlog, yLog, epochs: 100);

            Console.WriteLine("Logistic Regression Predictions:");
            for (int i = 0; i < Xlog.Count; i++)
            {
                double prob = logisticRegression.Predict(Xlog[i]);
                int pred = logisticRegression.PredictClass(Xlog[i]);
                Console.WriteLine($"Input: [{Xlog[i][0]}, {Xlog[i][1]}, {Xlog[i][2]}] -> Probability: {prob:F3}, Class: {pred}");
            }

            Console.WriteLine("\n=== Support Vector Machine ===");
            var svm = new SupportVectorMachine(numFeatures: 3, learningRate: 0.01, lambda: 0.01);

            var Xsvm = new List<double[]>
            {
                new double[] { 2, 3, 4 },
                new double[] { 3, 4, 5 },
                new double[] { -1, -2, -1 },
                new double[] { -2, -3, -2 }
            };

            var ysvm = new List<int> { 1, 1, 0, 0 };

            svm.Fit(Xsvm, ysvm, epochs: 100);

            Console.WriteLine("SVM Predictions:");
            for (int i = 0; i < Xsvm.Count; i++)
            {
                double score = svm.Predict(Xsvm[i]);
                int pred = svm.PredictClass(Xsvm[i]);
                Console.WriteLine($"Input: [{Xsvm[i][0]}, {Xsvm[i][1]}, {Xsvm[i][2]}] -> Score: {score:F3}, Class: {pred}");
            }

            Console.WriteLine("\n=== NLP Sentiment Analysis with Linear Regression ===");
            var sentimentAnalyzer = new LinearRegressionSentimentAnalyzer(vocabSize: 20);

            var sentimentData = new List<(string, int)>
            {
                ("good great excellent", 1),
                ("amazing wonderful fantastic", 1),
                ("bad terrible awful", 0),
                ("horrible poor disappointing", 0),
                ("love enjoy like", 1),
                ("hate dislike ugly", 0)
            };

            Console.WriteLine("Training sentiment analyzer...");
            sentimentAnalyzer.Train(sentimentData);

            var testTexts = new List<string>
            {
                "good wonderful",
                "terrible awful",
                "excellent amazing"
            };

            Console.WriteLine("\nSentiment Predictions:");
            foreach (var text in testTexts)
            {
                double sentiment = sentimentAnalyzer.PredictSentiment(text);
                int label = sentimentAnalyzer.PredictSentimentClass(text);
                Console.WriteLine($"Text: \"{text}\" -> Probability: {sentiment:F3}, Class: {(label == 1 ? "Positive" : "Negative")}");
            }

            Console.WriteLine("\n=== SVM Topic Classification ===");
            var topicClassifier = new SVMTopicClassifier("Sports", vocabSize: 30);

            var topicData = new List<(string, bool)>
            {
                ("football game team player", true),
                ("basketball championship score", true),
                ("computer science programming", false),
                ("machine learning algorithm", false),
                ("soccer match goal", true),
                ("election vote government", false)
            };

            Console.WriteLine("Training topic classifier...");
            topicClassifier.Train(topicData);

            var testTopics = new List<string>
            {
                "football team game",
                "programming code software",
                "basketball player"
            };

            Console.WriteLine("\nTopic Classification:");
            foreach (var text in testTopics)
            {
                bool isSports = topicClassifier.Predict(text);
                double score = topicClassifier.Score(text);
                Console.WriteLine($"Text: \"{text}\" -> Is Sports: {isSports}, Score: {score:F3}");
            }

            Console.WriteLine("\n=== Applications of Linear Regression in NLP ===");
            Console.WriteLine("1. Sentiment Analysis");
            Console.WriteLine("   - Predict sentiment score from word features");
            Console.WriteLine("   - Logistic regression for binary classification");
            Console.WriteLine("   - Handle class imbalance with weighted loss");
            Console.WriteLine("\n2. Topic Modeling");
            Console.WriteLine("   - Predict document topic from word frequencies");
            Console.WriteLine("   - Multiple independent classifiers per topic");
            Console.WriteLine("   - SVM for better separation");
            Console.WriteLine("\n3. Machine Translation Quality");
            Console.WriteLine("   - Predict translation quality score");
            Console.WriteLine("   - Regression on feature vectors");
            Console.WriteLine("   - Combine with language model scores");
            Console.WriteLine("\n4. Document Regression");
            Console.WriteLine("   - Predict document properties (length, complexity)");
            Console.WriteLine("   - Linear relationship modeling");
            Console.WriteLine("   - Feature importance analysis");

            Console.WriteLine("\n=== Advantages of Linear Methods ===");
            Console.WriteLine("- Simple to understand and implement");
            Console.WriteLine("- Fast training and inference");
            Console.WriteLine("- Interpretable coefficients (feature weights)");
            Console.WriteLine("- Good baseline for comparison");
            Console.WriteLine("- Scalable to large datasets");
            Console.WriteLine("- Well-understood mathematically");
            Console.WriteLine("- Minimal hyperparameter tuning");

            Console.WriteLine("\n=== Linear Regression vs SVM ===");
            Console.WriteLine("Linear Regression:");
            Console.WriteLine("  - Minimizes squared error");
            Console.WriteLine("  - Probabilistic interpretation");
            Console.WriteLine("  - Sensitive to outliers");
            Console.WriteLine("  - Provides confidence intervals");
            Console.WriteLine("\nSVM:");
            Console.WriteLine("  - Maximizes margin");
            Console.WriteLine("  - Robust to outliers");
            Console.WriteLine("  - Better for high-dimensional data");
            Console.WriteLine("  - Non-probabilistic scores");

            Console.WriteLine("\n=== Feature Engineering for NLP ===");
            Console.WriteLine("1. Bag of Words: Word frequency counts");
            Console.WriteLine("2. TF-IDF: Term frequency - inverse document frequency");
            Console.WriteLine("3. Word Embeddings: Distributed representations");
            Console.WriteLine("4. Text Statistics: Length, punctuation, capitals");
            Console.WriteLine("5. N-grams: Word and character n-grams");
            Console.WriteLine("6. Domain-Specific: POS tags, named entities");

            Console.WriteLine("\n=== When to Use Linear Methods ===");
            Console.WriteLine("- Baseline model establishment");
            Console.WriteLine("- High-dimensional sparse data (NLP typically)");
            Console.WriteLine("- Need for interpretability");
            Console.WriteLine("- Real-time predictions required");
            Console.WriteLine("- Limited computational resources");
            Console.WriteLine("- Small to medium datasets");
        }
    }
}
