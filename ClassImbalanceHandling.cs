using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NaturalLanguageProcessing.ClassImbalance
{
    /// <summary>
    /// Represents a labeled text document for classification.
    /// </summary>
    public class LabeledDocument
    {
        public string Text { get; set; }
        public string Label { get; set; }
        public int Id { get; set; }

        public LabeledDocument(int id, string text, string label)
        {
            Id = id;
            Text = text;
            Label = label;
        }
    }

    /// <summary>
    /// Analyzes class distribution and imbalance in datasets.
    /// </summary>
    public class ClassImbalanceAnalyzer
    {
        private List<LabeledDocument> dataset;

        public ClassImbalanceAnalyzer(List<LabeledDocument> dataset)
        {
            this.dataset = dataset;
        }

        /// <summary>
        /// Calculates class distribution statistics.
        /// </summary>
        public Dictionary<string, ClassStatistics> AnalyzeClassDistribution()
        {
            var classStats = new Dictionary<string, ClassStatistics>();
            int totalSamples = dataset.Count;

            var groupedByClass = dataset.GroupBy(d => d.Label);

            foreach (var classGroup in groupedByClass)
            {
                string label = classGroup.Key;
                int count = classGroup.Count();
                double percentage = (count / (double)totalSamples) * 100;

                classStats[label] = new ClassStatistics
                {
                    Label = label,
                    Count = count,
                    Percentage = percentage,
                    SampleIds = classGroup.Select(d => d.Id).ToList()
                };
            }

            return classStats;
        }

        /// <summary>
        /// Calculates imbalance ratio (majority/minority).
        /// </summary>
        public double CalculateImbalanceRatio()
        {
            var stats = AnalyzeClassDistribution();
            if (stats.Count == 0) return 0;

            int maxCount = stats.Values.Max(s => s.Count);
            int minCount = stats.Values.Min(s => s.Count);

            return minCount == 0 ? 0 : maxCount / (double)minCount;
        }

        /// <summary>
        /// Identifies majority and minority classes.
        /// </summary>
        public (string majorityClass, string minorityClass) IdentifyMajorityMinority()
        {
            var stats = AnalyzeClassDistribution();
            var sorted = stats.OrderByDescending(s => s.Value.Count).ToList();

            return (sorted[0].Key, sorted[sorted.Count - 1].Key);
        }

        /// <summary>
        /// Generates imbalance report.
        /// </summary>
        public string GenerateImbalanceReport()
        {
            var stats = AnalyzeClassDistribution();
            var sb = new StringBuilder();

            sb.AppendLine("Class Imbalance Report");
            sb.AppendLine(new string('=', 50));
            sb.AppendLine($"Total samples: {dataset.Count}");
            sb.AppendLine($"Number of classes: {stats.Count}");
            sb.AppendLine($"Imbalance ratio: {CalculateImbalanceRatio():F2}:1");
            sb.AppendLine();

            sb.AppendLine("Class Distribution:");
            foreach (var stat in stats.OrderByDescending(s => s.Value.Count))
            {
                sb.AppendLine($"  {stat.Key}: {stat.Value.Count} ({stat.Value.Percentage:F2}%)");
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Statistics for a single class.
    /// </summary>
    public class ClassStatistics
    {
        public string Label { get; set; }
        public int Count { get; set; }
        public double Percentage { get; set; }
        public List<int> SampleIds { get; set; }
    }

    /// <summary>
    /// Oversampling techniques to increase minority class representation.
    /// </summary>
    public class OversamplingStrategy
    {
        private Random random;

        public OversamplingStrategy()
        {
            random = new Random();
        }

        /// <summary>
        /// Random oversampling: duplicates minority class samples.
        /// </summary>
        public List<LabeledDocument> RandomOversampling(List<LabeledDocument> dataset, string minorityClass)
        {
            var result = new List<LabeledDocument>(dataset);
            var minoritySamples = dataset.Where(d => d.Label == minorityClass).ToList();
            var majoritySamples = dataset.Where(d => d.Label != minorityClass).ToList();

            int targetCount = majoritySamples.Count;
            int samplesToAdd = targetCount - minoritySamples.Count;

            for (int i = 0; i < samplesToAdd; i++)
            {
                var randomSample = minoritySamples[random.Next(minoritySamples.Count)];
                var newDoc = new LabeledDocument(
                    dataset.Count + i,
                    randomSample.Text,
                    randomSample.Label
                );
                result.Add(newDoc);
            }

            return result;
        }

        /// <summary>
        /// SMOTE-inspired oversampling: creates synthetic minority samples.
        /// Simplified version using text interpolation.
        /// </summary>
        public List<LabeledDocument> SMOTEOversampling(List<LabeledDocument> dataset, string minorityClass, int k = 5)
        {
            var result = new List<LabeledDocument>(dataset);
            var minoritySamples = dataset.Where(d => d.Label == minorityClass).ToList();
            var majoritySamples = dataset.Where(d => d.Label != minorityClass).ToList();

            int targetCount = majoritySamples.Count;
            int syntheticSamplesToCreate = targetCount - minoritySamples.Count;

            for (int i = 0; i < syntheticSamplesToCreate; i++)
            {
                var randomSample = minoritySamples[random.Next(minoritySamples.Count)];
                var nearestNeighbor = minoritySamples[random.Next(Math.Min(k, minoritySamples.Count))];

                string syntheticText = CreateSyntheticSample(randomSample.Text, nearestNeighbor.Text);

                var syntheticDoc = new LabeledDocument(
                    dataset.Count + result.Count,
                    syntheticText,
                    minorityClass
                );
                result.Add(syntheticDoc);
            }

            return result;
        }

        /// <summary>
        /// Creates synthetic text sample by interpolating between two texts.
        /// </summary>
        private string CreateSyntheticSample(string text1, string text2)
        {
            var words1 = text1.Split(' ');
            var words2 = text2.Split(' ');

            var syntheticWords = new List<string>();
            int maxLen = Math.Max(words1.Length, words2.Length);

            for (int i = 0; i < maxLen; i++)
            {
                if (i < words1.Length && i < words2.Length)
                {
                    syntheticWords.Add(random.NextDouble() > 0.5 ? words1[i] : words2[i]);
                }
                else if (i < words1.Length)
                {
                    syntheticWords.Add(words1[i]);
                }
                else if (i < words2.Length)
                {
                    syntheticWords.Add(words2[i]);
                }
            }

            return string.Join(" ", syntheticWords);
        }
    }

    /// <summary>
    /// Undersampling techniques to decrease majority class representation.
    /// </summary>
    public class UndersamplingStrategy
    {
        private Random random;

        public UndersamplingStrategy()
        {
            random = new Random();
        }

        /// <summary>
        /// Random undersampling: removes majority class samples.
        /// </summary>
        public List<LabeledDocument> RandomUndersampling(List<LabeledDocument> dataset, string majorityClass)
        {
            var minoritySamples = dataset.Where(d => d.Label != majorityClass).ToList();
            var majoritySamples = dataset.Where(d => d.Label == majorityClass).ToList();

            int targetCount = minoritySamples.Count;
            var undersampledMajority = majoritySamples.OrderBy(x => random.Next()).Take(targetCount).ToList();

            var result = new List<LabeledDocument>(minoritySamples);
            result.AddRange(undersampledMajority);

            return result;
        }

        /// <summary>
        /// Stratified undersampling: maintains class distribution.
        /// </summary>
        public List<LabeledDocument> StratifiedUndersampling(List<LabeledDocument> dataset, double samplingRatio = 0.5)
        {
            var result = new List<LabeledDocument>();
            var groupedByClass = dataset.GroupBy(d => d.Label);

            foreach (var classGroup in groupedByClass)
            {
                int sampleSize = (int)(classGroup.Count() * samplingRatio);
                var samples = classGroup.OrderBy(x => random.Next()).Take(sampleSize);
                result.AddRange(samples);
            }

            return result;
        }

        /// <summary>
        /// Hybrid approach: oversample minority and undersample majority.
        /// </summary>
        public List<LabeledDocument> HybridResampling(List<LabeledDocument> dataset,
            double oversampleRatio = 0.5, double undersampleRatio = 0.5)
        {
            var analyzer = new ClassImbalanceAnalyzer(dataset);
            var (majorityClass, minorityClass) = analyzer.IdentifyMajorityMinority();

            // Oversample minority
            var oversampler = new OversamplingStrategy();
            var oversampled = oversampler.RandomOversampling(dataset, minorityClass);

            // Undersample majority
            var undersampler = new UndersamplingStrategy();
            var result = undersampler.RandomUndersampling(oversampled, majorityClass);

            return result;
        }
    }

    /// <summary>
    /// Cost-sensitive learning weights misclassification costs by class.
    /// </summary>
    public class CostSensitiveLearning
    {
        /// <summary>
        /// Calculates class weights inversely proportional to class frequency.
        /// </summary>
        public Dictionary<string, double> CalculateClassWeights(List<LabeledDocument> dataset)
        {
            var weights = new Dictionary<string, double>();
            int totalSamples = dataset.Count;

            var classFrequencies = dataset.GroupBy(d => d.Label)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var classEntry in classFrequencies)
            {
                double weight = totalSamples / (double)(classFrequencies.Count * classEntry.Value);
                weights[classEntry.Key] = weight;
            }

            return weights;
        }

        /// <summary>
        /// Calculates focal weights for hard-to-classify samples.
        /// </summary>
        public Dictionary<int, double> CalculateFocalWeights(List<LabeledDocument> dataset,
            Dictionary<string, double> predictions, double gamma = 2.0)
        {
            var focalWeights = new Dictionary<int, double>();

            for (int i = 0; i < dataset.Count; i++)
            {
                string predictedLabel = predictions.Keys.FirstOrDefault() ?? dataset[i].Label;
                double confidence = Math.Abs(predictions[predictedLabel]);

                // Focal weight: harder samples get higher weights
                double focalWeight = Math.Pow(1 - confidence, gamma);
                focalWeights[i] = focalWeight;
            }

            return focalWeights;
        }

        /// <summary>
        /// Generates misclassification cost matrix.
        /// </summary>
        public double[,] GenerateCostMatrix(List<LabeledDocument> dataset)
        {
            var classes = dataset.Select(d => d.Label).Distinct().ToList();
            int numClasses = classes.Count;

            double[,] costMatrix = new double[numClasses, numClasses];

            // Higher cost for misclassifying minority classes
            var classFrequencies = dataset.GroupBy(d => d.Label)
                .ToDictionary(g => g.Key, g => g.Count());

            for (int i = 0; i < numClasses; i++)
            {
                for (int j = 0; j < numClasses; j++)
                {
                    if (i == j)
                    {
                        costMatrix[i, j] = 0; // No cost for correct classification
                    }
                    else
                    {
                        // Cost inversely proportional to class size
                        string classJ = classes[j];
                        double cost = 1.0 / classFrequencies[classJ];
                        costMatrix[i, j] = cost;
                    }
                }
            }

            return costMatrix;
        }
    }

    /// <summary>
    /// Ensemble methods for handling class imbalance.
    /// </summary>
    public class EnsembleBalancer
    {
        /// <summary>
        /// Balanced Random Forest approach: creates multiple balanced subsets.
        /// </summary>
        public List<List<LabeledDocument>> CreateBalancedSubsets(List<LabeledDocument> dataset, int numSubsets = 5)
        {
            var subsets = new List<List<LabeledDocument>>();
            var analyzer = new ClassImbalanceAnalyzer(dataset);
            var (majorityClass, minorityClass) = analyzer.IdentifyMajorityMinority();

            var minoritySamples = dataset.Where(d => d.Label == minorityClass).ToList();
            var majoritySamples = dataset.Where(d => d.Label == majorityClass).ToList();

            var random = new Random();

            for (int i = 0; i < numSubsets; i++)
            {
                var subset = new List<LabeledDocument>(minoritySamples);

                // Randomly sample from majority class with replacement
                int samplesToTake = minoritySamples.Count;
                var sampledMajority = new List<LabeledDocument>();

                for (int j = 0; j < samplesToTake; j++)
                {
                    sampledMajority.Add(majoritySamples[random.Next(majoritySamples.Count)]);
                }

                subset.AddRange(sampledMajority);
                subsets.Add(subset);
            }

            return subsets;
        }

        /// <summary>
        /// One-vs-Rest ensemble: trains binary classifiers for each class.
        /// </summary>
        public List<BinaryClassifier> CreateOneVsRestEnsemble(List<LabeledDocument> dataset)
        {
            var classifiers = new List<BinaryClassifier>();
            var classes = dataset.Select(d => d.Label).Distinct().ToList();

            foreach (var positiveClass in classes)
            {
                var binaryDataset = dataset.Select(d => new LabeledDocument(
                    d.Id,
                    d.Text,
                    d.Label == positiveClass ? "positive" : "negative"
                )).ToList();

                var classifier = new BinaryClassifier(positiveClass, binaryDataset);
                classifiers.Add(classifier);
            }

            return classifiers;
        }

        /// <summary>
        /// Boosting-inspired approach: iteratively focuses on hard examples.
        /// </summary>
        public List<List<LabeledDocument>> CreateBoostingWeightedSubsets(List<LabeledDocument> dataset, int numIterations = 5)
        {
            var subsets = new List<List<LabeledDocument>>();
            var weights = dataset.ToDictionary(d => d.Id, d => 1.0 / dataset.Count);
            var random = new Random();

            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                // Sample with probabilities based on weights
                var subset = new List<LabeledDocument>();
                for (int i = 0; i < dataset.Count; i++)
                {
                    double r = random.NextDouble();
                    double cumulative = 0;

                    foreach (var doc in dataset)
                    {
                        cumulative += weights[doc.Id];
                        if (r <= cumulative)
                        {
                            subset.Add(doc);
                            break;
                        }
                    }
                }

                subsets.Add(subset);

                // Update weights (increase for minority class)
                var analyzer = new ClassImbalanceAnalyzer(dataset);
                var (_, minorityClass) = analyzer.IdentifyMajorityMinority();

                foreach (var doc in dataset)
                {
                    if (doc.Label == minorityClass)
                    {
                        weights[doc.Id] *= 1.5;
                    }
                }

                // Normalize weights
                double totalWeight = weights.Values.Sum();
                foreach (var key in weights.Keys.ToList())
                {
                    weights[key] /= totalWeight;
                }
            }

            return subsets;
        }
    }

    /// <summary>
    /// Simple binary classifier for ensemble methods.
    /// </summary>
    public class BinaryClassifier
    {
        public string PositiveClass { get; set; }
        public List<LabeledDocument> TrainingData { get; set; }

        public BinaryClassifier(string positiveClass, List<LabeledDocument> trainingData)
        {
            PositiveClass = positiveClass;
            TrainingData = trainingData;
        }

        public string Predict(string text)
        {
            // Simplified prediction: word overlap
            var textWords = text.Split(' ').ToHashSet();
            int positiveMatches = 0;
            int negativeMatches = 0;

            foreach (var doc in TrainingData)
            {
                var docWords = doc.Text.Split(' ');
                int matches = docWords.Count(w => textWords.Contains(w));

                if (doc.Label == "positive")
                    positiveMatches += matches;
                else
                    negativeMatches += matches;
            }

            return positiveMatches > negativeMatches ? PositiveClass : "other";
        }
    }

    /// <summary>
    /// Metrics specifically designed for imbalanced datasets.
    /// </summary>
    public class ImbalancedMetrics
    {
        /// <summary>
        /// Calculates F1 score (harmonic mean of precision and recall).
        /// </summary>
        public double CalculateF1Score(int truePositives, int falsePositives, int falseNegatives)
        {
            double precision = truePositives / (double)(truePositives + falsePositives);
            double recall = truePositives / (double)(truePositives + falseNegatives);

            if (precision + recall == 0) return 0;
            return 2 * (precision * recall) / (precision + recall);
        }

        /// <summary>
        /// Calculates weighted F1 score across all classes.
        /// </summary>
        public double CalculateWeightedF1Score(Dictionary<string, int> truePositives,
            Dictionary<string, int> falsePositives, Dictionary<string, int> falseNegatives)
        {
            double totalSamples = truePositives.Sum(x => x.Value) +
                                 falsePositives.Sum(x => x.Value);
            double weightedF1 = 0;

            foreach (var classLabel in truePositives.Keys)
            {
                double f1 = CalculateF1Score(
                    truePositives[classLabel],
                    falsePositives.ContainsKey(classLabel) ? falsePositives[classLabel] : 0,
                    falseNegatives.ContainsKey(classLabel) ? falseNegatives[classLabel] : 0
                );

                double weight = truePositives[classLabel] / totalSamples;
                weightedF1 += f1 * weight;
            }

            return weightedF1;
        }

        /// <summary>
        /// Calculates G-Mean (geometric mean) for imbalanced classification.
        /// </summary>
        public double CalculateGMean(int truePositives, int trueNegatives,
            int falsePositives, int falseNegatives)
        {
            double sensitivity = truePositives / (double)(truePositives + falseNegatives);
            double specificity = trueNegatives / (double)(trueNegatives + falsePositives);

            return Math.Sqrt(sensitivity * specificity);
        }

        /// <summary>
        /// Calculates Matthews Correlation Coefficient (good for imbalanced data).
        /// </summary>
        public double CalculateMCC(int truePositives, int trueNegatives,
            int falsePositives, int falseNegatives)
        {
            double numerator = (truePositives * trueNegatives) - (falsePositives * falseNegatives);
            double denominator = Math.Sqrt((double)(truePositives + falsePositives) *
                                         (truePositives + falseNegatives) *
                                         (trueNegatives + falsePositives) *
                                         (trueNegatives + falseNegatives));

            if (denominator == 0) return 0;
            return numerator / denominator;
        }
    }

    /// <summary>
    /// Examples demonstrating class imbalance handling in NLP.
    /// </summary>
    public static class ClassImbalanceExamples
    {
        public static void DemonstrateImbalanceAnalysis()
        {
            Console.WriteLine("=== Class Imbalance Analysis ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "spam email content", "spam"),
                new LabeledDocument(2, "legitimate email content", "ham"),
                new LabeledDocument(3, "another spam email", "spam"),
                new LabeledDocument(4, "real message", "ham"),
                new LabeledDocument(5, "buy now spam", "spam"),
                new LabeledDocument(6, "hello friend", "ham"),
                new LabeledDocument(7, "urgent spam", "spam"),
                new LabeledDocument(8, "normal email", "ham"),
                new LabeledDocument(9, "click here spam", "spam"),
                new LabeledDocument(10, "how are you", "ham"),
                new LabeledDocument(11, "free offer spam", "spam"),
                new LabeledDocument(12, "meeting tomorrow", "ham")
            };

            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            Console.WriteLine($"Imbalance Ratio: {analyzer.CalculateImbalanceRatio():F2}:1");
            var (majority, minority) = analyzer.IdentifyMajorityMinority();
            Console.WriteLine($"Majority class: {majority}");
            Console.WriteLine($"Minority class: {minority}");
        }

        public static void DemonstrateRandomOversampling()
        {
            Console.WriteLine("\n=== Random Oversampling ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "spam email", "spam"),
                new LabeledDocument(2, "ham email", "ham"),
                new LabeledDocument(3, "another spam", "spam"),
                new LabeledDocument(4, "normal email", "ham"),
                new LabeledDocument(5, "spam content", "spam"),
                new LabeledDocument(6, "legitimate", "ham"),
                new LabeledDocument(7, "urgent spam", "spam"),
                new LabeledDocument(8, "real message", "ham")
            };

            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine("Before oversampling:");
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            var oversampler = new OversamplingStrategy();
            var oversampled = oversampler.RandomOversampling(dataset, "ham");

            var newAnalyzer = new ClassImbalanceAnalyzer(oversampled);
            Console.WriteLine("\nAfter random oversampling:");
            Console.WriteLine(newAnalyzer.GenerateImbalanceReport());
        }

        public static void DemonstrateSMOTEOversampling()
        {
            Console.WriteLine("\n=== SMOTE Oversampling ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "positive review great quality", "positive"),
                new LabeledDocument(2, "negative review poor quality", "negative"),
                new LabeledDocument(3, "another positive", "positive"),
                new LabeledDocument(4, "bad service", "negative"),
                new LabeledDocument(5, "excellent product", "positive"),
                new LabeledDocument(6, "terrible experience", "negative"),
                new LabeledDocument(7, "good value", "positive"),
                new LabeledDocument(8, "waste of money", "negative"),
                new LabeledDocument(9, "not recommended", "negative"),
                new LabeledDocument(10, "would buy again", "positive")
            };

            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine("Before SMOTE:");
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            var oversampler = new OversamplingStrategy();
            var smoted = oversampler.SMOTEOversampling(dataset, "positive", k: 3);

            var newAnalyzer = new ClassImbalanceAnalyzer(smoted);
            Console.WriteLine("\nAfter SMOTE oversampling:");
            Console.WriteLine(newAnalyzer.GenerateImbalanceReport());
        }

        public static void DemonstrateUndersampling()
        {
            Console.WriteLine("\n=== Undersampling ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "neutral review", "neutral"),
                new LabeledDocument(2, "neutral text", "neutral"),
                new LabeledDocument(3, "positive review", "positive"),
                new LabeledDocument(4, "neutral content", "neutral"),
                new LabeledDocument(5, "neutral message", "neutral"),
                new LabeledDocument(6, "negative review", "negative"),
                new LabeledDocument(7, "neutral", "neutral"),
                new LabeledDocument(8, "positive", "positive"),
                new LabeledDocument(9, "neutral again", "neutral"),
                new LabeledDocument(10, "negative", "negative")
            };

            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine("Before undersampling:");
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            var undersampler = new UndersamplingStrategy();
            var undersampled = undersampler.RandomUndersampling(dataset, "neutral");

            var newAnalyzer = new ClassImbalanceAnalyzer(undersampled);
            Console.WriteLine("\nAfter random undersampling:");
            Console.WriteLine(newAnalyzer.GenerateImbalanceReport());
        }

        public static void DemonstrateCostSensitiveLearning()
        {
            Console.WriteLine("\n=== Cost-Sensitive Learning ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "fraud transaction", "fraud"),
                new LabeledDocument(2, "normal transaction", "normal"),
                new LabeledDocument(3, "normal", "normal"),
                new LabeledDocument(4, "normal transaction", "normal"),
                new LabeledDocument(5, "fraud", "fraud"),
                new LabeledDocument(6, "normal", "normal"),
                new LabeledDocument(7, "normal", "normal"),
                new LabeledDocument(8, "fraud transaction", "fraud")
            };

            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine("Dataset distribution:");
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            var costLearning = new CostSensitiveLearning();
            var classWeights = costLearning.CalculateClassWeights(dataset);

            Console.WriteLine("\nClass weights (inverse frequency):");
            foreach (var weight in classWeights)
            {
                Console.WriteLine($"  {weight.Key}: {weight.Value:F4}");
            }

            Console.WriteLine("\nInterpretation:");
            Console.WriteLine("  - Higher weight = minority class");
            Console.WriteLine("  - Model focuses on minimizing errors for minority class");
        }

        public static void DemonstrateEnsembleBalancing()
        {
            Console.WriteLine("\n=== Ensemble Balancing ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "disease symptom", "diseased"),
                new LabeledDocument(2, "normal patient", "healthy"),
                new LabeledDocument(3, "healthy", "healthy"),
                new LabeledDocument(4, "disease", "diseased"),
                new LabeledDocument(5, "healthy", "healthy"),
                new LabeledDocument(6, "normal", "healthy"),
                new LabeledDocument(7, "disease symptom", "diseased"),
                new LabeledDocument(8, "normal", "healthy"),
                new LabeledDocument(9, "healthy", "healthy"),
                new LabeledDocument(10, "disease", "diseased")
            };

            var balancer = new EnsembleBalancer();
            var subsets = balancer.CreateBalancedSubsets(dataset, numSubsets: 3);

            Console.WriteLine("Created 3 balanced subsets for ensemble:");
            for (int i = 0; i < subsets.Count; i++)
            {
                var analyzer = new ClassImbalanceAnalyzer(subsets[i]);
                Console.WriteLine($"\nSubset {i + 1}:");
                Console.WriteLine(analyzer.GenerateImbalanceReport());
            }

            Console.WriteLine("\nEnsemble approach:");
            Console.WriteLine("  - Each subset trains a separate model");
            Console.WriteLine("  - Final prediction = majority vote");
            Console.WriteLine("  - Reduces variance and improves minority class prediction");
        }

        public static void DemonstrateOneVsRestEnsemble()
        {
            Console.WriteLine("\n=== One-vs-Rest Ensemble ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "sports news", "sports"),
                new LabeledDocument(2, "business report", "business"),
                new LabeledDocument(3, "political news", "politics"),
                new LabeledDocument(4, "sports update", "sports"),
                new LabeledDocument(5, "business", "business"),
                new LabeledDocument(6, "politics", "politics"),
                new LabeledDocument(7, "sports", "sports"),
                new LabeledDocument(8, "business news", "business"),
                new LabeledDocument(9, "political update", "politics"),
                new LabeledDocument(10, "sports news", "sports")
            };

            var balancer = new EnsembleBalancer();
            var classifiers = balancer.CreateOneVsRestEnsemble(dataset);

            Console.WriteLine($"Created {classifiers.Count} binary classifiers (One-vs-Rest):");
            foreach (var classifier in classifiers)
            {
                Console.WriteLine($"\n  Classifier for '{classifier.PositiveClass}':");
                Console.WriteLine($"    Positive class: {classifier.PositiveClass}");
                Console.WriteLine($"    Negative class: All others");
            }

            Console.WriteLine("\nBenefits of One-vs-Rest:");
            Console.WriteLine("  - Handles each class individually");
            Console.WriteLine("  - Each binary classifier can be balanced independently");
            Console.WriteLine("  - Final prediction = max probability across classifiers");
        }

        public static void DemonstrateImbalancedMetrics()
        {
            Console.WriteLine("\n=== Imbalanced Classification Metrics ===\n");

            var metrics = new ImbalancedMetrics();

            // Example: Class imbalance (90% negative, 10% positive)
            int tp = 8, fp = 2, fn = 2, tn = 88;

            Console.WriteLine("Classification results on imbalanced data:");
            Console.WriteLine($"  True Positives: {tp}");
            Console.WriteLine($"  False Positives: {fp}");
            Console.WriteLine($"  False Negatives: {fn}");
            Console.WriteLine($"  True Negatives: {tn}");
            Console.WriteLine($"  Total: {tp + fp + fn + tn}");

            double accuracy = (tp + tn) / (double)(tp + fp + fn + tn);
            Console.WriteLine($"\nAccuracy: {accuracy:F4} (misleading for imbalanced data!)");

            double f1 = metrics.CalculateF1Score(tp, fp, fn);
            Console.WriteLine($"F1 Score: {f1:F4} (better metric for imbalance)");

            double gmean = metrics.CalculateGMean(tp, tn, fp, fn);
            Console.WriteLine($"G-Mean: {gmean:F4} (balances sensitivity and specificity)");

            double mcc = metrics.CalculateMCC(tp, tn, fp, fn);
            Console.WriteLine($"Matthews Correlation Coefficient: {mcc:F4} (handles imbalance well)");

            Console.WriteLine("\nWhy these metrics matter:");
            Console.WriteLine("  - Accuracy can be high even with poor minority prediction");
            Console.WriteLine("  - F1 Score weights precision and recall equally");
            Console.WriteLine("  - G-Mean ensures both classes are recognized");
            Console.WriteLine("  - MCC is robust to class imbalance");
        }

        public static void DemonstrateCompleteWorkflow()
        {
            Console.WriteLine("\n=== Complete Imbalance Handling Workflow ===\n");

            var dataset = new List<LabeledDocument>
            {
                new LabeledDocument(1, "fraud transaction", "fraud"),
                new LabeledDocument(2, "normal", "normal"),
                new LabeledDocument(3, "normal transaction", "normal"),
                new LabeledDocument(4, "normal", "normal"),
                new LabeledDocument(5, "fraud", "fraud"),
                new LabeledDocument(6, "normal", "normal"),
                new LabeledDocument(7, "normal", "normal"),
                new LabeledDocument(8, "normal", "normal"),
                new LabeledDocument(9, "fraud transaction", "fraud"),
                new LabeledDocument(10, "normal", "normal")
            };

            Console.WriteLine("Step 1: Analyze Class Distribution");
            var analyzer = new ClassImbalanceAnalyzer(dataset);
            Console.WriteLine(analyzer.GenerateImbalanceReport());

            Console.WriteLine("\nStep 2: Choose Strategy");
            Console.WriteLine("Options:");
            Console.WriteLine("  a) Oversampling - more data for minority class");
            Console.WriteLine("  b) Undersampling - less data for majority class");
            Console.WriteLine("  c) Cost-sensitive - adjust misclassification costs");
            Console.WriteLine("  d) Ensemble - combine multiple models");

            Console.WriteLine("\nStep 3: Apply Oversampling");
            var oversampler = new OversamplingStrategy();
            var oversampled = oversampler.RandomOversampling(dataset, "fraud");
            var newAnalyzer = new ClassImbalanceAnalyzer(oversampled);
            Console.WriteLine($"Imbalance ratio after oversampling: {newAnalyzer.CalculateImbalanceRatio():F2}:1");

            Console.WriteLine("\nStep 4: Calculate Class Weights (for cost-sensitive learning)");
            var costLearning = new CostSensitiveLearning();
            var weights = costLearning.CalculateClassWeights(dataset);
            foreach (var w in weights)
            {
                Console.WriteLine($"  {w.Key}: {w.Value:F4}");
            }

            Console.WriteLine("\nStep 5: Evaluate with Appropriate Metrics");
            var metrics = new ImbalancedMetrics();
            double f1 = metrics.CalculateF1Score(5, 1, 3);
            Console.WriteLine($"F1 Score: {f1:F4}");
            double mcc = metrics.CalculateMCC(5, 8, 1, 3);
            Console.WriteLine($"MCC: {mcc:F4}");

            Console.WriteLine("\nStep 6: Monitor Results");
            Console.WriteLine("  - Use F1 score, not accuracy");
            Console.WriteLine("  - Use MCC for overall performance");
            Console.WriteLine("  - Monitor precision and recall separately");
            Console.WriteLine("  - Use stratified cross-validation");
        }
    }
}
