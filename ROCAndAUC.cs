using System;
using System.Collections.Generic;
using System.Linq;

namespace MachineLearning.Evaluation
{
    /// <summary>
    /// Represents a prediction with its true label and predicted probability.
    /// Used for calculating ROC curves and AUC.
    /// </summary>
    public class Prediction
    {
        public bool TrueLabel { get; set; }
        public double PredictedProbability { get; set; }

        public Prediction(bool trueLabel, double predictedProbability)
        {
            TrueLabel = trueLabel;
            PredictedProbability = Math.Max(0, Math.Min(1, predictedProbability));
        }
    }

    /// <summary>
    /// Represents a point on the ROC curve with a specific threshold.
    /// </summary>
    public class ROCPoint
    {
        public double Threshold { get; set; }
        public double FPR { get; set; }
        public double TPR { get; set; }
        public int TruePositives { get; set; }
        public int FalsePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalseNegatives { get; set; }

        public override string ToString()
        {
            return $"Threshold: {Threshold:F3}, TPR: {TPR:F4}, FPR: {FPR:F4}";
        }
    }

    /// <summary>
    /// Calculates confusion matrix metrics for binary classification.
    /// </summary>
    public class ConfusionMatrix
    {
        public int TruePositives { get; set; }
        public int TrueNegatives { get; set; }
        public int FalsePositives { get; set; }
        public int FalseNegatives { get; set; }

        public double Accuracy => (TruePositives + TrueNegatives) / (double)(TruePositives + TrueNegatives + FalsePositives + FalseNegatives);
        public double Precision => TruePositives / (double)(TruePositives + FalsePositives);
        public double Recall => TruePositives / (double)(TruePositives + FalseNegatives);
        public double F1Score => 2 * (Precision * Recall) / (Precision + Recall);
        public double Specificity => TrueNegatives / (double)(TrueNegatives + FalsePositives);
        public double FalsePositiveRate => FalsePositives / (double)(FalsePositives + TrueNegatives);
        public double TruePositiveRate => TruePositives / (double)(TruePositives + FalseNegatives);

        public override string ToString()
        {
            return $"TP: {TruePositives}, TN: {TrueNegatives}, FP: {FalsePositives}, FN: {FalseNegatives}\n" +
                   $"Accuracy: {Accuracy:F4}, Precision: {Precision:F4}, Recall: {Recall:F4}, F1: {F1Score:F4}";
        }
    }

    /// <summary>
    /// Calculates ROC curve points and AUC for binary classification models.
    /// </summary>
    public class ROCCalculator
    {
        private List<Prediction> predictions;
        private List<ROCPoint> rocCurve;
        private double aucValue;

        public ROCCalculator(List<Prediction> predictions)
        {
            this.predictions = predictions.OrderByDescending(p => p.PredictedProbability).ToList();
            this.rocCurve = new List<ROCPoint>();
            CalculateROCCurve();
        }

        /// <summary>
        /// Calculates the ROC curve points for different thresholds.
        /// </summary>
        private void CalculateROCCurve()
        {
            int totalPositives = predictions.Count(p => p.TrueLabel);
            int totalNegatives = predictions.Count(p => !p.TrueLabel);

            // Add point at threshold 1.0 (no positive predictions)
            rocCurve.Add(new ROCPoint
            {
                Threshold = 1.0,
                TPR = 0,
                FPR = 0,
                TruePositives = 0,
                FalsePositives = 0,
                TrueNegatives = totalNegatives,
                FalseNegatives = totalPositives
            });

            // Calculate points for each unique probability threshold
            var uniqueProbabilities = predictions
                .Select(p => p.PredictedProbability)
                .Distinct()
                .OrderByDescending(p => p)
                .ToList();

            foreach (double threshold in uniqueProbabilities)
            {
                int tp = 0, fp = 0, tn = 0, fn = 0;

                foreach (var prediction in predictions)
                {
                    bool predictedPositive = prediction.PredictedProbability >= threshold;
                    bool actualPositive = prediction.TrueLabel;

                    if (predictedPositive && actualPositive) tp++;
                    else if (predictedPositive && !actualPositive) fp++;
                    else if (!predictedPositive && actualPositive) fn++;
                    else tn++;
                }

                double tpr = totalPositives > 0 ? tp / (double)totalPositives : 0;
                double fpr = totalNegatives > 0 ? fp / (double)totalNegatives : 0;

                rocCurve.Add(new ROCPoint
                {
                    Threshold = threshold,
                    TPR = tpr,
                    FPR = fpr,
                    TruePositives = tp,
                    FalsePositives = fp,
                    TrueNegatives = tn,
                    FalseNegatives = fn
                });
            }

            // Add point at threshold 0.0 (all positive predictions)
            rocCurve.Add(new ROCPoint
            {
                Threshold = 0.0,
                TPR = 1,
                FPR = 1,
                TruePositives = totalPositives,
                FalsePositives = totalNegatives,
                TrueNegatives = 0,
                FalseNegatives = 0
            });

            CalculateAUC();
        }

        /// <summary>
        /// Calculates AUC by integrating the area under the ROC curve using the trapezoidal rule.
        /// </summary>
        private void CalculateAUC()
        {
            rocCurve = rocCurve.OrderBy(p => p.FPR).ToList();
            aucValue = 0;

            for (int i = 1; i < rocCurve.Count; i++)
            {
                double width = rocCurve[i].FPR - rocCurve[i - 1].FPR;
                double height = (rocCurve[i].TPR + rocCurve[i - 1].TPR) / 2.0;
                aucValue += width * height;
            }
        }

        public double GetAUC() => aucValue;

        public List<ROCPoint> GetROCCurve() => rocCurve;

        /// <summary>
        /// Gets the optimal threshold that maximizes the sum of TPR and specificity.
        /// </summary>
        public double GetOptimalThreshold()
        {
            return rocCurve
                .OrderByDescending(p => p.TPR + (1 - p.FPR))
                .First()
                .Threshold;
        }

        /// <summary>
        /// Gets confusion matrix at a specific threshold.
        /// </summary>
        public ConfusionMatrix GetConfusionMatrixAtThreshold(double threshold)
        {
            var point = rocCurve.FirstOrDefault(p => Math.Abs(p.Threshold - threshold) < 0.0001);
            if (point == null) return null;

            return new ConfusionMatrix
            {
                TruePositives = point.TruePositives,
                FalsePositives = point.FalsePositives,
                TrueNegatives = point.TrueNegatives,
                FalseNegatives = point.FalseNegatives
            };
        }

        /// <summary>
        /// Prints ROC curve points in a formatted table.
        /// </summary>
        public void PrintROCCurve()
        {
            Console.WriteLine("ROC Curve Points:");
            Console.WriteLine("Threshold | TPR      | FPR      | TP | FP | TN | FN");
            Console.WriteLine("-".PadRight(60, '-'));

            foreach (var point in rocCurve.OrderByDescending(p => p.Threshold))
            {
                Console.WriteLine($"{point.Threshold:F3}     | {point.TPR:F4}    | {point.FPR:F4}    | {point.TruePositives:D3} | {point.FalsePositives:D3} | {point.TrueNegatives:D3} | {point.FalseNegatives:D3}");
            }
        }
    }

    /// <summary>
    /// Evaluates NLP model performance using multiple metrics.
    /// </summary>
    public class NLPModelEvaluator
    {
        private List<Prediction> predictions;
        private ROCCalculator rocCalculator;

        public NLPModelEvaluator(List<Prediction> predictions)
        {
            this.predictions = predictions;
            this.rocCalculator = new ROCCalculator(predictions);
        }

        /// <summary>
        /// Gets comprehensive evaluation report.
        /// </summary>
        public string GetEvaluationReport()
        {
            double auc = rocCalculator.GetAUC();
            double optimalThreshold = rocCalculator.GetOptimalThreshold();
            var confusionMatrix = rocCalculator.GetConfusionMatrixAtThreshold(optimalThreshold);

            return $"Model Evaluation Report\n" +
                   $"{'='.ToString().PadRight(40, '=')}\n" +
                   $"AUC: {auc:F4}\n" +
                   $"Optimal Threshold: {optimalThreshold:F4}\n" +
                   $"\nConfusion Matrix at Optimal Threshold:\n{confusionMatrix}\n" +
                   $"ROC Curve Points: {rocCalculator.GetROCCurve().Count}";
        }

        public double GetAUC() => rocCalculator.GetAUC();
        public List<ROCPoint> GetROCCurve() => rocCalculator.GetROCCurve();
    }

    /// <summary>
    /// Compares performance of multiple NLP models using AUC.
    /// </summary>
    public class ModelComparator
    {
        private Dictionary<string, double> modelAUCs;
        private Dictionary<string, NLPModelEvaluator> evaluators;

        public ModelComparator()
        {
            modelAUCs = new Dictionary<string, double>();
            evaluators = new Dictionary<string, NLPModelEvaluator>();
        }

        /// <summary>
        /// Adds a model to the comparison.
        /// </summary>
        public void AddModel(string modelName, List<Prediction> predictions)
        {
            var evaluator = new NLPModelEvaluator(predictions);
            modelAUCs[modelName] = evaluator.GetAUC();
            evaluators[modelName] = evaluator;
        }

        /// <summary>
        /// Prints model comparison sorted by AUC.
        /// </summary>
        public void PrintComparison()
        {
            Console.WriteLine("Model Comparison (sorted by AUC):");
            Console.WriteLine("-".PadRight(40, '-'));

            foreach (var model in modelAUCs.OrderByDescending(m => m.Value))
            {
                Console.WriteLine($"{model.Key.PadRight(25)} | AUC: {model.Value:F4}");
            }
        }

        /// <summary>
        /// Gets the best performing model.
        /// </summary>
        public string GetBestModel()
        {
            return modelAUCs.OrderByDescending(m => m.Value).First().Key;
        }

        /// <summary>
        /// Calculates the performance difference between two models.
        /// </summary>
        public double CalculateDifference(string model1, string model2)
        {
            if (!modelAUCs.ContainsKey(model1) || !modelAUCs.ContainsKey(model2))
                return 0;

            return Math.Abs(modelAUCs[model1] - modelAUCs[model2]);
        }
    }

    /// <summary>
    /// Handles class imbalance metrics and statistics.
    /// </summary>
    public class ClassImbalanceAnalyzer
    {
        private List<Prediction> predictions;

        public ClassImbalanceAnalyzer(List<Prediction> predictions)
        {
            this.predictions = predictions;
        }

        public double GetClassImbalanceRatio()
        {
            int positives = predictions.Count(p => p.TrueLabel);
            int negatives = predictions.Count(p => !p.TrueLabel);

            if (positives == 0 || negatives == 0) return 0;

            return Math.Max(positives, negatives) / (double)Math.Min(positives, negatives);
        }

        public string GetImbalanceReport()
        {
            int positives = predictions.Count(p => p.TrueLabel);
            int negatives = predictions.Count(p => !p.TrueLabel);
            int total = predictions.Count;
            double ratio = GetClassImbalanceRatio();

            return $"Class Imbalance Analysis\n" +
                   $"Positive samples: {positives} ({positives * 100.0 / total:F2}%)\n" +
                   $"Negative samples: {negatives} ({negatives * 100.0 / total:F2}%)\n" +
                   $"Imbalance ratio: {ratio:F2}:1";
        }
    }

    /// <summary>
    /// Examples demonstrating AUC and ROC curve usage in NLP tasks.
    /// </summary>
    public static class ROCAndAUCExamples
    {
        public static void DemonstrateSpamDetection()
        {
            Console.WriteLine("=== Spam Detection Model Evaluation ===\n");

            var predictions = new List<Prediction>
            {
                new Prediction(true, 0.95),
                new Prediction(true, 0.87),
                new Prediction(false, 0.15),
                new Prediction(true, 0.92),
                new Prediction(false, 0.22),
                new Prediction(true, 0.88),
                new Prediction(false, 0.05),
                new Prediction(false, 0.18),
                new Prediction(true, 0.91),
                new Prediction(false, 0.12)
            };

            var evaluator = new NLPModelEvaluator(predictions);
            Console.WriteLine(evaluator.GetEvaluationReport());
        }

        public static void DemonstrateTextClassification()
        {
            Console.WriteLine("\n=== Text Classification Model Evaluation ===\n");

            var predictions = new List<Prediction>
            {
                new Prediction(true, 0.92),
                new Prediction(true, 0.85),
                new Prediction(false, 0.20),
                new Prediction(true, 0.88),
                new Prediction(false, 0.25),
                new Prediction(true, 0.90),
                new Prediction(false, 0.10),
                new Prediction(false, 0.15),
                new Prediction(true, 0.89),
                new Prediction(false, 0.30),
                new Prediction(true, 0.91),
                new Prediction(false, 0.05)
            };

            var evaluator = new NLPModelEvaluator(predictions);
            var rocCalculator = new ROCCalculator(predictions);

            Console.WriteLine($"AUC Score: {rocCalculator.GetAUC():F4}");
            Console.WriteLine($"Optimal Threshold: {rocCalculator.GetOptimalThreshold():F4}\n");

            rocCalculator.PrintROCCurve();
        }

        public static void DemonstrateModelComparison()
        {
            Console.WriteLine("\n=== Comparing Multiple Models ===\n");

            var comparator = new ModelComparator();

            // Model 1: Good performance
            var model1Predictions = new List<Prediction>
            {
                new Prediction(true, 0.95), new Prediction(true, 0.87),
                new Prediction(false, 0.15), new Prediction(true, 0.92),
                new Prediction(false, 0.22), new Prediction(true, 0.88),
                new Prediction(false, 0.05), new Prediction(false, 0.18),
                new Prediction(true, 0.91), new Prediction(false, 0.12)
            };

            // Model 2: Moderate performance
            var model2Predictions = new List<Prediction>
            {
                new Prediction(true, 0.80), new Prediction(true, 0.75),
                new Prediction(false, 0.30), new Prediction(true, 0.78),
                new Prediction(false, 0.35), new Prediction(true, 0.72),
                new Prediction(false, 0.25), new Prediction(false, 0.40),
                new Prediction(true, 0.76), new Prediction(false, 0.28)
            };

            // Model 3: Poor performance
            var model3Predictions = new List<Prediction>
            {
                new Prediction(true, 0.55), new Prediction(true, 0.50),
                new Prediction(false, 0.45), new Prediction(true, 0.60),
                new Prediction(false, 0.55), new Prediction(true, 0.48),
                new Prediction(false, 0.52), new Prediction(false, 0.58),
                new Prediction(true, 0.51), new Prediction(false, 0.49)
            };

            comparator.AddModel("Advanced NLP Model", model1Predictions);
            comparator.AddModel("Standard LSTM Model", model2Predictions);
            comparator.AddModel("Baseline Model", model3Predictions);

            comparator.PrintComparison();
            Console.WriteLine($"\nBest Model: {comparator.GetBestModel()}");
        }

        public static void DemonstrateClassImbalanceHandling()
        {
            Console.WriteLine("\n=== Class Imbalance Analysis ===\n");

            // Imbalanced dataset: mostly negative samples
            var predictions = new List<Prediction>
            {
                new Prediction(true, 0.92),
                new Prediction(true, 0.87),
                new Prediction(false, 0.15),
                new Prediction(false, 0.20),
                new Prediction(false, 0.10),
                new Prediction(false, 0.25),
                new Prediction(false, 0.05),
                new Prediction(false, 0.18),
                new Prediction(false, 0.12),
                new Prediction(false, 0.30),
                new Prediction(false, 0.08),
                new Prediction(false, 0.22),
                new Prediction(false, 0.14),
                new Prediction(false, 0.28),
                new Prediction(false, 0.06)
            };

            var analyzer = new ClassImbalanceAnalyzer(predictions);
            Console.WriteLine(analyzer.GetImbalanceReport());

            Console.WriteLine("\n" + "-".PadRight(40, '-'));
            var evaluator = new NLPModelEvaluator(predictions);
            Console.WriteLine($"\nDespite class imbalance, AUC is still informative:");
            Console.WriteLine($"AUC: {evaluator.GetAUC():F4}");
            Console.WriteLine("(AUC = 1.0 would indicate perfect classification)");
        }

        public static void DemonstrateThresholdSelection()
        {
            Console.WriteLine("\n=== Threshold Selection Impact ===\n");

            var predictions = new List<Prediction>
            {
                new Prediction(true, 0.92), new Prediction(true, 0.85),
                new Prediction(false, 0.20), new Prediction(true, 0.88),
                new Prediction(false, 0.25), new Prediction(true, 0.90),
                new Prediction(false, 0.10), new Prediction(false, 0.15),
                new Prediction(true, 0.89), new Prediction(false, 0.30)
            };

            var rocCalculator = new ROCCalculator(predictions);
            var optimalThreshold = rocCalculator.GetOptimalThreshold();

            Console.WriteLine($"Optimal Threshold: {optimalThreshold:F4}\n");

            double[] thresholds = { 0.3, optimalThreshold, 0.7 };

            Console.WriteLine("Performance at Different Thresholds:");
            Console.WriteLine("-".PadRight(60, '-'));

            foreach (double threshold in thresholds)
            {
                var cm = rocCalculator.GetConfusionMatrixAtThreshold(threshold);
                if (cm != null)
                {
                    Console.WriteLine($"\nThreshold: {threshold:F3}");
                    Console.WriteLine($"Accuracy: {cm.Accuracy:F4}, Precision: {cm.Precision:F4}, Recall: {cm.Recall:F4}");
                }
            }
        }

        public static void DemonstrateMachinTranslationEvaluation()
        {
            Console.WriteLine("\n=== Machine Translation Quality Evaluation ===\n");

            var predictions = new List<Prediction>
            {
                new Prediction(true, 0.89),  // Good translation
                new Prediction(true, 0.84),  // Good translation
                new Prediction(false, 0.28), // Poor translation
                new Prediction(true, 0.91),  // Good translation
                new Prediction(false, 0.19), // Poor translation
                new Prediction(true, 0.87),  // Good translation
                new Prediction(false, 0.08), // Poor translation
                new Prediction(false, 0.32), // Poor translation
                new Prediction(true, 0.90),  // Good translation
                new Prediction(false, 0.15), // Poor translation
                new Prediction(true, 0.86),  // Good translation
                new Prediction(false, 0.25)  // Poor translation
            };

            var evaluator = new NLPModelEvaluator(predictions);
            Console.WriteLine($"Machine Translation Model Performance:");
            Console.WriteLine($"AUC: {evaluator.GetAUC():F4}");
            Console.WriteLine("\nInterpretation:");
            Console.WriteLine("- AUC > 0.9: Excellent at distinguishing good vs poor translations");
            Console.WriteLine("- AUC > 0.8: Good performance");
            Console.WriteLine("- AUC > 0.7: Fair performance");
            Console.WriteLine("- AUC = 0.5: Random performance");
        }
    }
}
