using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Forecasting vector for time-series prediction
    /// Combines NLP features with temporal data
    /// </summary>
    public class ForecastingVector
    {
        public DateTime Timestamp { get; set; }
        public double[] Features { get; set; }
        public double Target { get; set; }

        public ForecastingVector(DateTime timestamp, double[] features, double target = 0)
        {
            Timestamp = timestamp;
            Features = features;
            Target = target;
        }
    }

    /// <summary>
    /// Text-based feature extractor for time-series
    /// </summary>
    public class TemporalFeatureExtractor
    {
        public Dictionary<string, int> SentimentWords { get; set; }
        public List<string> TopicKeywords { get; set; }
        public int WindowSize { get; set; }

        public TemporalFeatureExtractor(int windowSize = 5)
        {
            WindowSize = windowSize;
            SentimentWords = new Dictionary<string, int>
            {
                // Positive sentiment
                { "good", 1 }, { "great", 1 }, { "excellent", 1 }, { "positive", 1 },
                { "growth", 1 }, { "profit", 1 }, { "success", 1 }, { "increase", 1 },
                // Negative sentiment
                { "bad", -1 }, { "poor", -1 }, { "loss", -1 }, { "decline", -1 },
                { "drop", -1 }, { "crash", -1 }, { "fail", -1 }, { "risk", -1 }
            };

            TopicKeywords = new List<string>
            {
                "earnings", "revenue", "stock", "market", "trading", "investor",
                "expansion", "merger", "acquisition", "partnership", "technology"
            };
        }

        public double[] ExtractFeatures(List<string> texts)
        {
            double[] features = new double[5];

            // Feature 0: Average sentiment
            double totalSentiment = 0;
            int sentimentCount = 0;

            // Feature 1: Sentiment variance
            var sentiments = new List<double>();

            // Feature 2: Topic presence
            double topicScore = 0;

            // Feature 3: Text volume
            features[3] = texts.Count;

            // Feature 4: Volatility indicator
            double maxSentiment = -1, minSentiment = 1;

            foreach (var text in texts)
            {
                var words = text.ToLower().Split(' ');
                double textSentiment = 0;
                int wordCount = 0;

                foreach (var word in words)
                {
                    if (SentimentWords.ContainsKey(word))
                    {
                        textSentiment += SentimentWords[word];
                        sentimentCount++;
                        wordCount++;
                    }

                    if (TopicKeywords.Contains(word))
                        topicScore++;
                }

                if (wordCount > 0)
                {
                    textSentiment /= wordCount;
                    sentiments.Add(textSentiment);
                    totalSentiment += textSentiment;
                    maxSentiment = Math.Max(maxSentiment, textSentiment);
                    minSentiment = Math.Min(minSentiment, textSentiment);
                }
            }

            features[0] = sentimentCount > 0 ? totalSentiment / sentimentCount : 0; // Average sentiment
            features[1] = sentiments.Count > 0 ? sentiments.StandardDeviation() : 0; // Sentiment variance
            features[2] = topicScore / (texts.Count * 10.0); // Topic relevance
            features[4] = maxSentiment - minSentiment; // Volatility

            return features;
        }

        public double ExtractSentiment(string text)
        {
            var words = text.ToLower().Split(' ');
            double sentiment = 0;
            int count = 0;

            foreach (var word in words)
            {
                if (SentimentWords.ContainsKey(word))
                {
                    sentiment += SentimentWords[word];
                    count++;
                }
            }

            return count > 0 ? sentiment / count : 0;
        }
    }

    /// <summary>
    /// Time-series forecasting model using regression
    /// </summary>
    public class TimeSeriesForecaster
    {
        public int LookbackWindow { get; set; }
        public double[] Weights { get; set; }
        public double Bias { get; set; }
        public double LearningRate { get; set; }
        public List<ForecastingVector> TrainingData { get; set; }

        public TimeSeriesForecaster(int lookbackWindow = 5, double learningRate = 0.01)
        {
            LookbackWindow = lookbackWindow;
            LearningRate = learningRate;
            Weights = new double[lookbackWindow];
            Bias = 0;
            TrainingData = new List<ForecastingVector>();
        }

        public void AddTrainingData(ForecastingVector vector)
        {
            TrainingData.Add(vector);
        }

        public void Train(int epochs = 50)
        {
            if (TrainingData.Count < LookbackWindow + 1)
                return;

            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalError = 0;

                for (int i = LookbackWindow; i < TrainingData.Count; i++)
                {
                    // Create training example from lookback window
                    double prediction = Bias;
                    for (int j = 0; j < LookbackWindow; j++)
                    {
                        double value = TrainingData[i - LookbackWindow + j].Target;
                        prediction += Weights[j] * value;
                    }

                    double error = prediction - TrainingData[i].Target;
                    totalError += error * error;

                    // Update weights
                    Bias -= LearningRate * error;
                    for (int j = 0; j < LookbackWindow; j++)
                    {
                        double value = TrainingData[i - LookbackWindow + j].Target;
                        Weights[j] -= LearningRate * error * value;
                    }
                }

                if (epoch % 10 == 0)
                    Console.WriteLine($"Epoch {epoch}: MSE = {totalError / (TrainingData.Count - LookbackWindow):F4}");
            }
        }

        public double Forecast(List<double> recentValues)
        {
            if (recentValues.Count != LookbackWindow)
                throw new ArgumentException($"Expected {LookbackWindow} recent values");

            double prediction = Bias;
            for (int i = 0; i < LookbackWindow; i++)
                prediction += Weights[i] * recentValues[i];

            return prediction;
        }
    }

    /// <summary>
    /// Vector autoregression for forecasting
    /// </summary>
    public class VectorAutoregression
    {
        public int Lag { get; set; }
        public double[][] Coefficients { get; set; }
        public double[] Intercept { get; set; }
        public int NumSeries { get; set; }

        public VectorAutoregression(int numSeries, int lag = 1)
        {
            NumSeries = numSeries;
            Lag = lag;
            Coefficients = new double[numSeries][];
            Intercept = new double[numSeries];

            for (int i = 0; i < numSeries; i++)
                Coefficients[i] = new double[numSeries * lag];
        }

        public void Fit(List<double[]> observations, int epochs = 100, double learningRate = 0.01)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalError = 0;

                for (int t = Lag; t < observations.Count; t++)
                {
                    for (int i = 0; i < NumSeries; i++)
                    {
                        double prediction = Intercept[i];

                        // Combine lags
                        for (int lag = 0; lag < Lag; lag++)
                        {
                            for (int j = 0; j < NumSeries; j++)
                            {
                                prediction += Coefficients[i][lag * NumSeries + j] * observations[t - lag - 1][j];
                            }
                        }

                        double error = prediction - observations[t][i];
                        totalError += error * error;

                        // Update intercept
                        Intercept[i] -= learningRate * error;

                        // Update coefficients
                        for (int lag = 0; lag < Lag; lag++)
                        {
                            for (int j = 0; j < NumSeries; j++)
                            {
                                Coefficients[i][lag * NumSeries + j] -= learningRate * error * observations[t - lag - 1][j];
                            }
                        }
                    }
                }

                if (epoch % 20 == 0)
                    Console.WriteLine($"Epoch {epoch}: MSE = {totalError / ((observations.Count - Lag) * NumSeries):F4}");
            }
        }

        public double[] Predict(List<double[]> recentObservations)
        {
            double[] prediction = new double[NumSeries];

            for (int i = 0; i < NumSeries; i++)
            {
                prediction[i] = Intercept[i];

                for (int lag = 0; lag < Lag; lag++)
                {
                    for (int j = 0; j < NumSeries; j++)
                    {
                        prediction[i] += Coefficients[i][lag * NumSeries + j] * recentObservations[lag][j];
                    }
                }
            }

            return prediction;
        }
    }

    /// <summary>
    /// Stock price predictor using forecasting vectors
    /// </summary>
    public class StockPricePredictor
    {
        public TemporalFeatureExtractor FeatureExtractor { get; set; }
        public TimeSeriesForecaster SentimentForecaster { get; set; }
        public TimeSeriesForecaster PriceForecaster { get; set; }
        public List<ForecastingVector> SentimentVectors { get; set; }
        public List<ForecastingVector> PriceVectors { get; set; }

        public StockPricePredictor(int lookbackWindow = 5)
        {
            FeatureExtractor = new TemporalFeatureExtractor(lookbackWindow);
            SentimentForecaster = new TimeSeriesForecaster(lookbackWindow: lookbackWindow);
            PriceForecaster = new TimeSeriesForecaster(lookbackWindow: lookbackWindow);
            SentimentVectors = new List<ForecastingVector>();
            PriceVectors = new List<ForecastingVector>();
        }

        public void AddDayData(DateTime date, List<string> newsArticles, double closePrice)
        {
            // Extract features from news
            double[] features = FeatureExtractor.ExtractFeatures(newsArticles);
            double sentiment = features[0];

            // Create sentiment vector
            var sentimentVec = new ForecastingVector(date, features, sentiment);
            SentimentVectors.Add(sentimentVec);
            SentimentForecaster.AddTrainingData(sentimentVec);

            // Create price vector
            var priceVec = new ForecastingVector(date, new double[] { closePrice }, closePrice);
            PriceVectors.Add(priceVec);
            PriceForecaster.AddTrainingData(priceVec);
        }

        public void Train(int epochs = 50)
        {
            Console.WriteLine("Training sentiment forecaster...");
            SentimentForecaster.Train(epochs: epochs);

            Console.WriteLine("Training price forecaster...");
            PriceForecaster.Train(epochs: epochs);
        }

        public double PredictPrice(List<string> currentNews, double currentPrice)
        {
            // Extract current sentiment
            double[] features = FeatureExtractor.ExtractFeatures(currentNews);
            double sentiment = features[0];

            // Get recent prices
            List<double> recentPrices = PriceVectors
                .Skip(Math.Max(0, PriceVectors.Count - 5))
                .Select(v => v.Target)
                .ToList();

            if (recentPrices.Count == 5)
            {
                return PriceForecaster.Forecast(recentPrices);
            }

            return currentPrice;
        }
    }

    /// <summary>
    /// Customer churn predictor using forecasting vectors
    /// </summary>
    public class CustomerChurnPredictor
    {
        public double[] EngagementWeights { get; set; }
        public double ChurnThreshold { get; set; }

        public CustomerChurnPredictor(double churnThreshold = 0.5)
        {
            EngagementWeights = new double[4];
            ChurnThreshold = churnThreshold;
        }

        public void Train(List<(List<string>, int)> customerData)
        {
            var extractor = new TemporalFeatureExtractor();
            double[] totalWeights = new double[4];
            int churnCount = 0, retainCount = 0;

            foreach (var (messages, hasChurned) in customerData)
            {
                double[] features = extractor.ExtractFeatures(messages);

                if (hasChurned == 1)
                {
                    churnCount++;
                    for (int i = 0; i < 4; i++)
                        totalWeights[i] -= features[i];
                }
                else
                {
                    retainCount++;
                    for (int i = 0; i < 4; i++)
                        totalWeights[i] += features[i];
                }
            }

            for (int i = 0; i < 4; i++)
            {
                EngagementWeights[i] = totalWeights[i] / (churnCount + retainCount);
            }
        }

        public double PredictChurnProbability(List<string> customerMessages)
        {
            var extractor = new TemporalFeatureExtractor();
            double[] features = extractor.ExtractFeatures(customerMessages);

            double score = 0;
            for (int i = 0; i < 4; i++)
                score += features[i] * EngagementWeights[i];

            // Normalize to 0-1
            return 1.0 / (1.0 + Math.Exp(-score));
        }

        public bool PredictChurn(List<string> customerMessages)
        {
            return PredictChurnProbability(customerMessages) > ChurnThreshold;
        }
    }

    /// <summary>
    /// Fraud detector using transaction and text features
    /// </summary>
    public class FraudDetector
    {
        public double[] SuspiciousPatterns { get; set; }
        public double FraudThreshold { get; set; }

        public FraudDetector(double fraudThreshold = 0.5)
        {
            SuspiciousPatterns = new double[6];
            FraudThreshold = fraudThreshold;
        }

        public void Train(List<(string, double, bool)> transactionData)
        {
            var extractor = new TemporalFeatureExtractor();
            double[] fraudFeatures = new double[6];
            double[] legit Features = new double[6];
            int fraudCount = 0, legitCount = 0;

            foreach (var (description, amount, isFraud) in transactionData)
            {
                double sentiment = extractor.ExtractSentiment(description);
                int suspiciousWords = CountSuspiciousWords(description);

                if (isFraud)
                {
                    fraudCount++;
                    fraudFeatures[0] += suspiciousWords;
                    fraudFeatures[1] += Math.Abs(sentiment); // Unusual sentiment
                    fraudFeatures[2] += amount > 1000 ? 1 : 0; // Large amounts
                    fraudFeatures[3] += description.Length;
                    fraudFeatures[4] += CountNumbers(description);
                }
                else
                {
                    legitCount++;
                    legitFeatures[0] += suspiciousWords;
                    legitFeatures[1] += Math.Abs(sentiment);
                    legitFeatures[2] += amount > 1000 ? 1 : 0;
                    legitFeatures[3] += description.Length;
                    legitFeatures[4] += CountNumbers(description);
                }
            }

            // Calculate fraud pattern weights
            for (int i = 0; i < 5; i++)
            {
                double fraudAvg = fraudCount > 0 ? fraudFeatures[i] / fraudCount : 0;
                double legitAvg = legitCount > 0 ? legitFeatures[i] / legitCount : 0;
                SuspiciousPatterns[i] = fraudAvg > legitAvg ? fraudAvg - legitAvg : 0;
            }
        }

        public double PredictFraudScore(string transactionDescription, double amount)
        {
            var extractor = new TemporalFeatureExtractor();
            double score = 0;

            int suspiciousWords = CountSuspiciousWords(transactionDescription);
            score += suspiciousWords * SuspiciousPatterns[0];

            double sentiment = extractor.ExtractSentiment(transactionDescription);
            score += Math.Abs(sentiment) * SuspiciousPatterns[1];

            if (amount > 1000)
                score += SuspiciousPatterns[2];

            score += (transactionDescription.Length / 100.0) * SuspiciousPatterns[3];
            score += CountNumbers(transactionDescription) * SuspiciousPatterns[4];

            return Math.Min(1.0, score / 10.0);
        }

        public bool PredictFraud(string transactionDescription, double amount)
        {
            return PredictFraudScore(transactionDescription, amount) > FraudThreshold;
        }

        private int CountSuspiciousWords(string text)
        {
            var suspicious = new[] { "urgent", "verify", "confirm", "update", "click", "confirm", "error" };
            int count = 0;
            var words = text.ToLower().Split(' ');
            foreach (var word in words)
            {
                if (suspicious.Contains(word))
                    count++;
            }
            return count;
        }

        private int CountNumbers(string text)
        {
            return text.Count(c => char.IsDigit(c));
        }
    }

    /// <summary>
    /// Social media trend predictor
    /// </summary>
    public class TrendPredictor
    {
        public List<(DateTime, double)> TrendHistory { get; set; }
        public TimeSeriesForecaster Forecaster { get; set; }

        public TrendPredictor(int lookbackWindow = 7)
        {
            TrendHistory = new List<(DateTime, double)>();
            Forecaster = new TimeSeriesForecaster(lookbackWindow: lookbackWindow);
        }

        public void AddTrendData(DateTime date, double trendScore)
        {
            TrendHistory.Add((date, trendScore));
            var vector = new ForecastingVector(date, new double[] { trendScore }, trendScore);
            Forecaster.AddTrainingData(vector);
        }

        public void Train(int epochs = 50)
        {
            Forecaster.Train(epochs: epochs);
        }

        public double PredictTrend()
        {
            var recentTrends = TrendHistory
                .Skip(Math.Max(0, TrendHistory.Count - 7))
                .Select(x => x.Item2)
                .ToList();

            if (recentTrends.Count == 7)
            {
                return Forecaster.Forecast(recentTrends);
            }

            return recentTrends.Count > 0 ? recentTrends.Average() : 0;
        }

        public List<double> PredictNextDays(int days)
        {
            var predictions = new List<double>();
            var recentTrends = TrendHistory
                .Skip(Math.Max(0, TrendHistory.Count - 7))
                .Select(x => x.Item2)
                .ToList();

            for (int i = 0; i < days; i++)
            {
                double nextPrediction = Forecaster.Forecast(recentTrends);
                predictions.Add(nextPrediction);
                recentTrends.RemoveAt(0);
                recentTrends.Add(nextPrediction);
            }

            return predictions;
        }
    }

    /// <summary>
    /// Forecasting evaluation metrics
    /// </summary>
    public class ForecastingMetrics
    {
        public static double MeanAbsoluteError(List<double> actual, List<double> predicted)
        {
            return actual.Zip(predicted, (a, p) => Math.Abs(a - p)).Average();
        }

        public static double MeanSquaredError(List<double> actual, List<double> predicted)
        {
            return actual.Zip(predicted, (a, p) => (a - p) * (a - p)).Average();
        }

        public static double RootMeanSquaredError(List<double> actual, List<double> predicted)
        {
            return Math.Sqrt(MeanSquaredError(actual, predicted));
        }

        public static double MeanAbsolutePercentageError(List<double> actual, List<double> predicted)
        {
            double sum = 0;
            for (int i = 0; i < actual.Count; i++)
            {
                if (actual[i] != 0)
                    sum += Math.Abs((actual[i] - predicted[i]) / actual[i]);
            }
            return (sum / actual.Count) * 100;
        }
    }

    /// <summary>
    /// Forecasting vector examples
    /// </summary>
    public class ForecastingVectorExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Stock Price Prediction with Forecasting Vectors ===");
            var stockPredictor = new StockPricePredictor(lookbackWindow: 5);

            var historicalData = new List<(DateTime, List<string>, double)>
            {
                (DateTime.Now.AddDays(-5), new List<string> { "good earnings growth profit" }, 100),
                (DateTime.Now.AddDays(-4), new List<string> { "excellent results expansion success" }, 102),
                (DateTime.Now.AddDays(-3), new List<string> { "positive market investor confidence" }, 104),
                (DateTime.Now.AddDays(-2), new List<string> { "strong performance growth positive" }, 106),
                (DateTime.Now.AddDays(-1), new List<string> { "great quarterly results profit" }, 108),
                (DateTime.Now, new List<string> { "good outlook future positive" }, 110),
            };

            foreach (var (date, news, price) in historicalData)
            {
                stockPredictor.AddDayData(date, news, price);
            }

            Console.WriteLine("Training stock price predictor...");
            stockPredictor.Train(epochs: 50);

            var currentNews = new List<string> { "excellent growth positive outlook" };
            double predictedPrice = stockPredictor.PredictPrice(currentNews, 110);
            Console.WriteLine($"Current Price: 110.00, Predicted Next Price: {predictedPrice:F2}");

            Console.WriteLine("\n=== Customer Churn Prediction ===");
            var churnPredictor = new CustomerChurnPredictor(churnThreshold: 0.5);

            var churnData = new List<(List<string>, int)>
            {
                (new List<string> { "hate this bad poor", "terrible service", "will not return" }, 1),
                (new List<string> { "disappointed poor quality", "bad experience" }, 1),
                (new List<string> { "love this great excellent", "coming back soon" }, 0),
                (new List<string> { "fantastic service wonderful", "very happy" }, 0),
            };

            churnPredictor.Train(churnData);

            var testCustomer = new List<string> { "terrible service bad experience", "disappointed" };
            double churnProb = churnPredictor.PredictChurnProbability(testCustomer);
            bool willChurn = churnPredictor.PredictChurn(testCustomer);

            Console.WriteLine($"Customer Churn Probability: {churnProb:F3}");
            Console.WriteLine($"Will Churn: {willChurn}");

            Console.WriteLine("\n=== Fraud Detection ===");
            var fraudDetector = new FraudDetector(fraudThreshold: 0.5);

            var fraudData = new List<(string, double, bool)>
            {
                ("urgent verify account click link", 500, true),
                ("confirm identity update payment", 1000, true),
                ("normal grocery purchase store", 50, false),
                ("regular gas station fuel", 40, false),
            };

            fraudDetector.Train(fraudData);

            var suspiciousTransaction = "urgent verify click confirm account";
            double fraudScore = fraudDetector.PredictFraudScore(suspiciousTransaction, 800);
            bool isFraud = fraudDetector.PredictFraud(suspiciousTransaction, 800);

            Console.WriteLine($"Fraud Score: {fraudScore:F3}");
            Console.WriteLine($"Is Fraudulent: {isFraud}");

            Console.WriteLine("\n=== Social Media Trend Prediction ===");
            var trendPredictor = new TrendPredictor(lookbackWindow: 7);

            var trends = new List<(DateTime, double)>
            {
                (DateTime.Now.AddDays(-6), 10),
                (DateTime.Now.AddDays(-5), 15),
                (DateTime.Now.AddDays(-4), 20),
                (DateTime.Now.AddDays(-3), 25),
                (DateTime.Now.AddDays(-2), 28),
                (DateTime.Now.AddDays(-1), 30),
                (DateTime.Now, 32),
            };

            foreach (var (date, trend) in trends)
            {
                trendPredictor.AddTrendData(date, trend);
            }

            Console.WriteLine("Training trend predictor...");
            trendPredictor.Train(epochs: 50);

            double nextTrend = trendPredictor.PredictTrend();
            Console.WriteLine($"Predicted Next Trend Score: {nextTrend:F2}");

            var nextDays = trendPredictor.PredictNextDays(3);
            Console.WriteLine("Predictions for next 3 days:");
            for (int i = 0; i < nextDays.Count; i++)
                Console.WriteLine($"  Day {i + 1}: {nextDays[i]:F2}");

            Console.WriteLine("\n=== Vector Autoregression ===");
            var var_model = new VectorAutoregression(numSeries: 2, lag: 2);

            var observations = new List<double[]>
            {
                new double[] { 100, 50 },
                new double[] { 102, 52 },
                new double[] { 105, 54 },
                new double[] { 108, 56 },
                new double[] { 110, 58 },
                new double[] { 112, 60 },
            };

            Console.WriteLine("Training VAR model...");
            var_model.Fit(observations, epochs: 100);

            var lastObs = new List<double[]>
            {
                new double[] { 110, 58 },
                new double[] { 112, 60 }
            };

            var prediction = var_model.Predict(lastObs);
            Console.WriteLine($"VAR Prediction: Stock={prediction[0]:F2}, Volume={prediction[1]:F2}");

            Console.WriteLine("\n=== Forecasting Applications ===");
            Console.WriteLine("1. Stock Price Prediction");
            Console.WriteLine("   - Extract sentiment from financial news");
            Console.WriteLine("   - Combine with historical price data");
            Console.WriteLine("   - Forecast future prices");
            Console.WriteLine("\n2. Customer Churn Prediction");
            Console.WriteLine("   - Analyze customer messages/feedback");
            Console.WriteLine("   - Extract engagement metrics");
            Console.WriteLine("   - Predict likelihood of churn");
            Console.WriteLine("\n3. Fraud Detection");
            Console.WriteLine("   - Analyze transaction descriptions");
            Console.WriteLine("   - Identify suspicious patterns");
            Console.WriteLine("   - Real-time fraud scoring");
            Console.WriteLine("\n4. Social Media Trends");
            Console.WriteLine("   - Track trend evolution over time");
            Console.WriteLine("   - Forecast future trend intensity");
            Console.WriteLine("   - Identify emerging trends");
            Console.WriteLine("\n5. Weather Patterns");
            Console.WriteLine("   - Combine NLP weather reports with data");
            Console.WriteLine("   - Predict future conditions");
            Console.WriteLine("   - Alert on dangerous patterns");
            Console.WriteLine("\n6. Natural Disasters");
            Console.WriteLine("   - Analyze disaster reports and warnings");
            Console.WriteLine("   - Combine with sensor data");
            Console.WriteLine("   - Early warning systems");

            Console.WriteLine("\n=== Advantages of Forecasting Vectors ===");
            Console.WriteLine("- Combines multiple data types (text + numerical)");
            Console.WriteLine("- Captures temporal patterns and trends");
            Console.WriteLine("- Leverages NLP for semantic understanding");
            Console.WriteLine("- Interpretable feature extraction");
            Console.WriteLine("- Flexible for various prediction tasks");
            Console.WriteLine("- Handles streaming data");

            Console.WriteLine("\n=== Model Evaluation Metrics ===");
            Console.WriteLine("- Mean Absolute Error (MAE): Average absolute deviation");
            Console.WriteLine("- Mean Squared Error (MSE): Penalizes large errors");
            Console.WriteLine("- Root Mean Squared Error (RMSE): Same scale as target");
            Console.WriteLine("- MAPE: Percentage error (scale-independent)");
            Console.WriteLine("- Directional Accuracy: Correct trend prediction");

            Console.WriteLine("\n=== Feature Engineering Tips ===");
            Console.WriteLine("- Sentiment: Positive/negative language");
            Console.WriteLine("- Topic Relevance: Domain-specific keywords");
            Console.WriteLine("- Volatility: Measure of sentiment variation");
            Console.WriteLine("- Volume: Amount of text data");
            Console.WriteLine("- Time Features: Hour, day, month patterns");
            Console.WriteLine("- Lagged Features: Previous time period values");
        }
    }
}

// Extension method for standard deviation
public static class ExtensionMethods
{
    public static double StandardDeviation(this List<double> values)
    {
        if (values.Count == 0)
            return 0;

        double mean = values.Average();
        double sumOfSquares = values.Sum(x => (x - mean) * (x - mean));
        return Math.Sqrt(sumOfSquares / values.Count);
    }
}
