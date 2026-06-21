using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// F1 Score and related evaluation metrics for binary and multi-class classification
    /// F1 = 2 * (Precision * Recall) / (Precision + Recall)
    /// </summary>
    public class F1ScoreCalculator
    {
        public int TruePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalsePositives { get; set; }
        public int FalseNegatives { get; set; }

        public F1ScoreCalculator()
        {
            ResetMetrics();
        }

        public void ResetMetrics()
        {
            TruePositives = 0;
            TrueNegatives = 0;
            FalsePositives = 0;
            FalseNegatives = 0;
        }

        public void AddPrediction(bool actual, bool predicted)
        {
            if (actual && predicted)
                TruePositives++;
            else if (!actual && !predicted)
                TrueNegatives++;
            else if (!actual && predicted)
                FalsePositives++;
            else if (actual && !predicted)
                FalseNegatives++;
        }

        public double GetPrecision()
        {
            int positivesPredicted = TruePositives + FalsePositives;
            if (positivesPredicted == 0)
                return 0;
            return (double)TruePositives / positivesPredicted;
        }

        public double GetRecall()
        {
            int positivesActual = TruePositives + FalseNegatives;
            if (positivesActual == 0)
                return 0;
            return (double)TruePositives / positivesActual;
        }

        public double GetF1Score()
        {
            double precision = GetPrecision();
            double recall = GetRecall();

            if (precision + recall == 0)
                return 0;

            return 2.0 * (precision * recall) / (precision + recall);
        }

        public double GetAccuracy()
        {
            int total = TruePositives + TrueNegatives + FalsePositives + FalseNegatives;
            if (total == 0)
                return 0;
            return (double)(TruePositives + TrueNegatives) / total;
        }

        public double GetSpecificity()
        {
            int negativesActual = TrueNegatives + FalsePositives;
            if (negativesActual == 0)
                return 0;
            return (double)TrueNegatives / negativesActual;
        }

        public double GetFalsePositiveRate()
        {
            return 1.0 - GetSpecificity();
        }

        public double GetFalseNegativeRate()
        {
            return 1.0 - GetRecall();
        }

        public double GetMatthewsCorrelationCoefficient()
        {
            int tp = TruePositives;
            int tn = TrueNegatives;
            int fp = FalsePositives;
            int fn = FalseNegatives;

            double numerator = (tp * tn) - (fp * fn);
            double denominator = Math.Sqrt((double)(tp + fp) * (tp + fn) * (tn + fp) * (tn + fn));

            if (denominator == 0)
                return 0;

            return numerator / denominator;
        }

        public void PrintMetrics()
        {
            Console.WriteLine($"True Positives: {TruePositives}");
            Console.WriteLine($"True Negatives: {TrueNegatives}");
            Console.WriteLine($"False Positives: {FalsePositives}");
            Console.WriteLine($"False Negatives: {FalseNegatives}");
            Console.WriteLine($"Precision: {GetPrecision():F4}");
            Console.WriteLine($"Recall: {GetRecall():F4}");
            Console.WriteLine($"F1 Score: {GetF1Score():F4}");
            Console.WriteLine($"Accuracy: {GetAccuracy():F4}");
            Console.WriteLine($"Specificity: {GetSpecificity():F4}");
            Console.WriteLine($"MCC: {GetMatthewsCorrelationCoefficient():F4}");
        }
    }

    /// <summary>
    /// Multi-class F1 score calculator with macro, micro, and weighted averaging
    /// </summary>
    public class MultiClassF1ScoreCalculator
    {
        public Dictionary<string, int> TruePositives { get; set; }
        public Dictionary<string, int> FalsePositives { get; set; }
        public Dictionary<string, int> FalseNegatives { get; set; }
        public Dictionary<string, int> SupportPerClass { get; set; }

        public MultiClassF1ScoreCalculator()
        {
            TruePositives = new Dictionary<string, int>();
            FalsePositives = new Dictionary<string, int>();
            FalseNegatives = new Dictionary<string, int>();
            SupportPerClass = new Dictionary<string, int>();
        }

        public void AddPrediction(string actualClass, string predictedClass, List<string> allClasses)
        {
            // Initialize if needed
            foreach (var c in allClasses)
            {
                if (!TruePositives.ContainsKey(c))
                {
                    TruePositives[c] = 0;
                    FalsePositives[c] = 0;
                    FalseNegatives[c] = 0;
                    SupportPerClass[c] = 0;
                }
            }

            // Update support count
            SupportPerClass[actualClass]++;

            // Update TP, FP, FN for each class
            foreach (var c in allClasses)
            {
                if (actualClass == c && predictedClass == c)
                {
                    TruePositives[c]++;
                }
                else if (actualClass != c && predictedClass == c)
                {
                    FalsePositives[c]++;
                }
                else if (actualClass == c && predictedClass != c)
                {
                    FalseNegatives[c]++;
                }
            }
        }

        public double GetPrecisionPerClass(string className)
        {
            int positivesPredicted = TruePositives[className] + FalsePositives[className];
            if (positivesPredicted == 0)
                return 0;
            return (double)TruePositives[className] / positivesPredicted;
        }

        public double GetRecallPerClass(string className)
        {
            int positivesActual = TruePositives[className] + FalseNegatives[className];
            if (positivesActual == 0)
                return 0;
            return (double)TruePositives[className] / positivesActual;
        }

        public double GetF1ScorePerClass(string className)
        {
            double precision = GetPrecisionPerClass(className);
            double recall = GetRecallPerClass(className);

            if (precision + recall == 0)
                return 0;

            return 2.0 * (precision * recall) / (precision + recall);
        }

        public double GetMacroF1Score()
        {
            if (TruePositives.Count == 0)
                return 0;

            double totalF1 = TruePositives.Keys.Sum(c => GetF1ScorePerClass(c));
            return totalF1 / TruePositives.Count;
        }

        public double GetMicroF1Score()
        {
            int totalTP = TruePositives.Values.Sum();
            int totalFP = FalsePositives.Values.Sum();
            int totalFN = FalseNegatives.Values.Sum();

            double microPrecision = (totalTP + totalFP == 0) ? 0 : (double)totalTP / (totalTP + totalFP);
            double microRecall = (totalTP + totalFN == 0) ? 0 : (double)totalTP / (totalTP + totalFN);

            if (microPrecision + microRecall == 0)
                return 0;

            return 2.0 * (microPrecision * microRecall) / (microPrecision + microRecall);
        }

        public double GetWeightedF1Score()
        {
            int totalSupport = SupportPerClass.Values.Sum();
            if (totalSupport == 0)
                return 0;

            double weightedF1 = 0;
            foreach (var className in TruePositives.Keys)
            {
                double f1 = GetF1ScorePerClass(className);
                double weight = (double)SupportPerClass[className] / totalSupport;
                weightedF1 += f1 * weight;
            }

            return weightedF1;
        }

        public void PrintMetrics()
        {
            Console.WriteLine("\n=== Per-Class Metrics ===");
            foreach (var className in TruePositives.Keys.OrderBy(x => x))
            {
                Console.WriteLine($"\nClass: {className}");
                Console.WriteLine($"  TP: {TruePositives[className]}, FP: {FalsePositives[className]}, FN: {FalseNegatives[className]}");
                Console.WriteLine($"  Precision: {GetPrecisionPerClass(className):F4}");
                Console.WriteLine($"  Recall: {GetRecallPerClass(className):F4}");
                Console.WriteLine($"  F1 Score: {GetF1ScorePerClass(className):F4}");
                Console.WriteLine($"  Support: {SupportPerClass[className]}");
            }

            Console.WriteLine($"\n=== Aggregate Metrics ===");
            Console.WriteLine($"Macro F1 Score: {GetMacroF1Score():F4}");
            Console.WriteLine($"Micro F1 Score: {GetMicroF1Score():F4}");
            Console.WriteLine($"Weighted F1 Score: {GetWeightedF1Score():F4}");
        }
    }

    /// <summary>
    /// Confusion matrix for visualizing and analyzing classification results
    /// </summary>
    public class ConfusionMatrix
    {
        public Dictionary<string, Dictionary<string, int>> Matrix { get; set; }
        public List<string> Classes { get; set; }

        public ConfusionMatrix(List<string> classes)
        {
            Classes = classes;
            Matrix = new Dictionary<string, Dictionary<string, int>>();

            foreach (var actualClass in classes)
            {
                Matrix[actualClass] = new Dictionary<string, int>();
                foreach (var predictedClass in classes)
                {
                    Matrix[actualClass][predictedClass] = 0;
                }
            }
        }

        public void AddPrediction(string actualClass, string predictedClass)
        {
            if (Matrix.ContainsKey(actualClass) && Matrix[actualClass].ContainsKey(predictedClass))
            {
                Matrix[actualClass][predictedClass]++;
            }
        }

        public int GetTP(string className)
        {
            return Matrix[className][className];
        }

        public int GetFP(string className)
        {
            int fp = 0;
            foreach (var actualClass in Classes)
            {
                if (actualClass != className)
                {
                    fp += Matrix[actualClass][className];
                }
            }
            return fp;
        }

        public int GetFN(string className)
        {
            int fn = 0;
            foreach (var predictedClass in Classes)
            {
                if (predictedClass != className)
                {
                    fn += Matrix[className][predictedClass];
                }
            }
            return fn;
        }

        public void PrintMatrix()
        {
            Console.WriteLine("\nConfusion Matrix:");
            Console.Write("Predicted →\n           ");
            foreach (var c in Classes)
            {
                Console.Write($"{c,12}");
            }
            Console.WriteLine();

            foreach (var actualClass in Classes)
            {
                Console.Write($"Actual {actualClass,-6}");
                foreach (var predictedClass in Classes)
                {
                    Console.Write($"{Matrix[actualClass][predictedClass],12}");
                }
                Console.WriteLine();
            }
        }

        public void PrintNormalizedMatrix()
        {
            Console.WriteLine("\nNormalized Confusion Matrix (by actual):");
            Console.Write("Predicted →\n           ");
            foreach (var c in Classes)
            {
                Console.Write($"{c,12}");
            }
            Console.WriteLine();

            foreach (var actualClass in Classes)
            {
                int totalActual = Classes.Sum(c => Matrix[actualClass][c]);
                Console.Write($"Actual {actualClass,-6}");
                foreach (var predictedClass in Classes)
                {
                    double normalized = totalActual > 0 ? (double)Matrix[actualClass][predictedClass] / totalActual : 0;
                    Console.Write($"{normalized,12:F3}");
                }
                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// ROC Curve generator for threshold analysis
    /// </summary>
    public class ROCCurveAnalyzer
    {
        public List<(double, double)> ROCPoints { get; set; } // (FPR, TPR) pairs
        public double AUC { get; set; }

        public ROCCurveAnalyzer()
        {
            ROCPoints = new List<(double, double)>();
            AUC = 0;
        }

        public void GenerateROCCurve(List<(double, bool)> predictions)
        {
            // Sort by score descending
            var sorted = predictions.OrderByDescending(x => x.Item1).ToList();

            int totalPositive = predictions.Count(x => x.Item2);
            int totalNegative = predictions.Count(x => !x.Item2);

            ROCPoints.Clear();
            ROCPoints.Add((0, 0)); // Start at origin

            int tp = 0, fp = 0;

            foreach (var (score, isPositive) in sorted)
            {
                if (isPositive)
                    tp++;
                else
                    fp++;

                double tpr = totalPositive > 0 ? (double)tp / totalPositive : 0;
                double fpr = totalNegative > 0 ? (double)fp / totalNegative : 0;

                ROCPoints.Add((fpr, tpr));
            }

            // Calculate AUC using trapezoidal rule
            AUC = 0;
            for (int i = 1; i < ROCPoints.Count; i++)
            {
                double x1 = ROCPoints[i - 1].Item1;
                double x2 = ROCPoints[i].Item1;
                double y1 = ROCPoints[i - 1].Item2;
                double y2 = ROCPoints[i].Item2;

                AUC += (x2 - x1) * (y1 + y2) / 2.0;
            }
        }

        public void PrintROCCurve()
        {
            Console.WriteLine($"\nROC Curve AUC: {AUC:F4}");
            Console.WriteLine("FPR\t\tTPR");
            foreach (var (fpr, tpr) in ROCPoints)
            {
                Console.WriteLine($"{fpr:F3}\t\t{tpr:F3}");
            }
        }
    }

    /// <summary>
    /// Threshold optimizer to find best decision threshold
    /// </summary>
    public class ThresholdOptimizer
    {
        public double OptimalThreshold { get; set; }
        public double OptimalF1Score { get; set; }

        public void FindOptimalThreshold(List<(double, bool)> predictions)
        {
            OptimalF1Score = 0;
            OptimalThreshold = 0.5;

            var sortedScores = predictions.Select(x => x.Item1).Distinct().OrderBy(x => x).ToList();

            foreach (var threshold in sortedScores)
            {
                var calculator = new F1ScoreCalculator();

                foreach (var (score, actual) in predictions)
                {
                    bool predicted = score >= threshold;
                    calculator.AddPrediction(actual, predicted);
                }

                double f1 = calculator.GetF1Score();
                if (f1 > OptimalF1Score)
                {
                    OptimalF1Score = f1;
                    OptimalThreshold = threshold;
                }
            }
        }

        public void PrintOptimalThreshold()
        {
            Console.WriteLine($"Optimal Threshold: {OptimalThreshold:F4}");
            Console.WriteLine($"F1 Score at Optimal Threshold: {OptimalF1Score:F4}");
        }
    }

    /// <summary>
    /// F1 Score evaluation for NLP tasks
    /// </summary>
    public class NLPTaskEvaluator
    {
        public string TaskName { get; set; }
        public MultiClassF1ScoreCalculator Evaluator { get; set; }

        public NLPTaskEvaluator(string taskName, List<string> classes)
        {
            TaskName = taskName;
            Evaluator = new MultiClassF1ScoreCalculator();
        }

        public void EvaluatePredictions(List<(string, string)> predictions, List<string> classes)
        {
            foreach (var (actual, predicted) in predictions)
            {
                Evaluator.AddPrediction(actual, predicted, classes);
            }
        }

        public void GenerateReport()
        {
            Console.WriteLine($"\n{'='} {TaskName} Evaluation Report {'='}");
            Evaluator.PrintMetrics();
        }
    }

    /// <summary>
    /// Example usage of F1 Score metrics
    /// </summary>
    public class F1ScoreExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Binary Classification F1 Score ===");
            var calculator = new F1ScoreCalculator();

            // Sentiment analysis example: Positive vs Negative
            var sentimentPredictions = new List<(bool, bool)>
            {
                (true, true),   // TP: Actual positive, predicted positive
                (true, true),   // TP
                (true, false),  // FN: Actual positive, predicted negative
                (false, false), // TN: Actual negative, predicted negative
                (false, true),  // FP: Actual negative, predicted positive
                (true, true),   // TP
                (false, false), // TN
                (true, true),   // TP
            };

            foreach (var (actual, predicted) in sentimentPredictions)
            {
                calculator.AddPrediction(actual, predicted);
            }

            Console.WriteLine("Sentiment Analysis Results:");
            calculator.PrintMetrics();

            Console.WriteLine("\n=== Multi-Class F1 Score ===");
            var multiClassCalculator = new MultiClassF1ScoreCalculator();
            var classes = new List<string> { "Sports", "Politics", "Technology", "Entertainment" };

            // Text classification predictions
            var classificationPredictions = new List<(string, string)>
            {
                ("Sports", "Sports"),
                ("Sports", "Sports"),
                ("Sports", "Technology"),
                ("Politics", "Politics"),
                ("Politics", "Politics"),
                ("Politics", "Sports"),
                ("Technology", "Technology"),
                ("Technology", "Technology"),
                ("Technology", "Politics"),
                ("Entertainment", "Entertainment"),
                ("Entertainment", "Entertainment"),
                ("Entertainment", "Sports"),
            };

            foreach (var (actual, predicted) in classificationPredictions)
            {
                multiClassCalculator.AddPrediction(actual, predicted, classes);
            }

            multiClassCalculator.PrintMetrics();

            Console.WriteLine("\n=== Confusion Matrix ===");
            var confusionMatrix = new ConfusionMatrix(classes);
            foreach (var (actual, predicted) in classificationPredictions)
            {
                confusionMatrix.AddPrediction(actual, predicted);
            }

            confusionMatrix.PrintMatrix();
            confusionMatrix.PrintNormalizedMatrix();

            Console.WriteLine("\n=== ROC Curve Analysis ===");
            var rocAnalyzer = new ROCCurveAnalyzer();

            // Probability predictions for binary classification
            var probabilityPredictions = new List<(double, bool)>
            {
                (0.9, true),
                (0.8, true),
                (0.7, true),
                (0.6, true),
                (0.55, true),
                (0.5, false),
                (0.4, false),
                (0.3, false),
                (0.2, false),
                (0.1, false),
            };

            rocAnalyzer.GenerateROCCurve(probabilityPredictions);
            rocAnalyzer.PrintROCCurve();

            Console.WriteLine("\n=== Threshold Optimization ===");
            var thresholdOptimizer = new ThresholdOptimizer();
            thresholdOptimizer.FindOptimalThreshold(probabilityPredictions);
            thresholdOptimizer.PrintOptimalThreshold();

            Console.WriteLine("\n=== NLP Task Evaluation ===");
            Console.WriteLine("\n--- Question Answering Evaluation ---");
            var qaEvaluator = new NLPTaskEvaluator("Question Answering", new List<string> { "Correct", "Incorrect", "Partial" });
            var qaPredictions = new List<(string, string)>
            {
                ("Correct", "Correct"),
                ("Correct", "Correct"),
                ("Correct", "Partial"),
                ("Incorrect", "Incorrect"),
                ("Incorrect", "Incorrect"),
                ("Partial", "Partial"),
                ("Partial", "Correct"),
            };
            qaEvaluator.EvaluatePredictions(qaPredictions, new List<string> { "Correct", "Incorrect", "Partial" });
            qaEvaluator.GenerateReport();

            Console.WriteLine("\n--- Named Entity Recognition Evaluation ---");
            var nerEvaluator = new NLPTaskEvaluator("Named Entity Recognition",
                new List<string> { "Person", "Organization", "Location", "Other" });
            var nerPredictions = new List<(string, string)>
            {
                ("Person", "Person"),
                ("Person", "Person"),
                ("Person", "Organization"),
                ("Organization", "Organization"),
                ("Location", "Location"),
                ("Location", "Location"),
                ("Other", "Other"),
            };
            nerEvaluator.EvaluatePredictions(nerPredictions, new List<string> { "Person", "Organization", "Location", "Other" });
            nerEvaluator.GenerateReport();

            Console.WriteLine("\n=== F1 Score Applications in NLP ===");
            Console.WriteLine("1. Text Classification");
            Console.WriteLine("   - Balances precision (avoiding false positives) with recall (finding all positives)");
            Console.WriteLine("   - Useful for imbalanced datasets");
            Console.WriteLine("   - Example: Spam detection, sentiment analysis");

            Console.WriteLine("\n2. Machine Translation");
            Console.WriteLine("   - BLEU score combined with F1 for evaluation");
            Console.WriteLine("   - Measures accuracy of translated words");
            Console.WriteLine("   - Handles synonyms and paraphrases");

            Console.WriteLine("\n3. Question Answering");
            Console.WriteLine("   - Evaluates exact match and partial match correctness");
            Console.WriteLine("   - Measures precision of answer extraction");
            Console.WriteLine("   - F1 score balances type I and type II errors");

            Console.WriteLine("\n4. Named Entity Recognition");
            Console.WriteLine("   - Per-entity-type F1 scores");
            Console.WriteLine("   - Macro F1 for overall performance");
            Console.WriteLine("   - Micro F1 for handling class imbalance");

            Console.WriteLine("\n5. Information Retrieval");
            Console.WriteLine("   - Evaluates ranking quality");
            Console.WriteLine("   - Measures both precision and recall at different cutoff points");
            Console.WriteLine("   - Mean Average Precision (MAP) variants");

            Console.WriteLine("\n=== Advantages of F1 Score ===");
            Console.WriteLine("- Balances precision and recall");
            Console.WriteLine("- Better than accuracy for imbalanced datasets");
            Console.WriteLine("- Single metric for model comparison");
            Console.WriteLine("- Handles both false positives and false negatives");
            Console.WriteLine("- Widely adopted in NLP community");
            Console.WriteLine("- Comparable across different models and datasets");

            Console.WriteLine("\n=== When to Use Different Metrics ===");
            Console.WriteLine("- Accuracy: Balanced datasets, cost-insensitive tasks");
            Console.WriteLine("- Precision: When false positives are costly (medical diagnosis)");
            Console.WriteLine("- Recall: When false negatives are costly (finding all relevant documents)");
            Console.WriteLine("- F1 Score: When precision and recall are equally important");
            Console.WriteLine("- Macro F1: When all classes are equally important");
            Console.WriteLine("- Micro F1: When large classes should have more weight");
            Console.WriteLine("- Weighted F1: When class distribution should be considered");
        }
    }
}
