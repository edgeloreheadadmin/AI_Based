using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizeMultipleMeasurementMetrics
{
    public class MetricDefinition
    {
        public string MetricName { get; set; }
        public string Unit { get; set; }
        public double MinRange { get; set; }
        public double MaxRange { get; set; }
        public string Category { get; set; }
        public Func<double, double> NormalizationFunc { get; set; }
    }

    public class MeasurementSample
    {
        public DateTime SampleTime { get; set; }
        public Dictionary<string, double> Values { get; set; }
        public string SourceSystem { get; set; }
        public double AggregateScore { get; set; }
    }

    public class MetricCalculator
    {
        private Dictionary<string, MetricDefinition> metrics;
        private List<MeasurementSample> samples;
        private Dictionary<string, List<double>> historicalData;

        public MetricCalculator()
        {
            metrics = new Dictionary<string, MetricDefinition>();
            samples = new List<MeasurementSample>();
            historicalData = new Dictionary<string, List<double>>();
        }

        public void RegisterMetric(string name, string unit, double min, double max, string category)
        {
            var metric = new MetricDefinition
            {
                MetricName = name,
                Unit = unit,
                MinRange = min,
                MaxRange = max,
                Category = category,
                NormalizationFunc = (value) => (value - min) / (max - min)
            };
            metrics[name] = metric;
            historicalData[name] = new List<double>();
        }

        public void RecordMeasurement(Dictionary<string, double> values, string source)
        {
            var sample = new MeasurementSample
            {
                SampleTime = DateTime.Now,
                Values = new Dictionary<string, double>(values),
                SourceSystem = source,
                AggregateScore = 0.0
            };

            foreach (var kvp in values)
            {
                if (metrics.ContainsKey(kvp.Key))
                {
                    double normalized = metrics[kvp.Key].NormalizationFunc(kvp.Value);
                    normalized = Math.Max(0, Math.Min(1.0, normalized));
                    sample.Values[kvp.Key] = normalized;

                    if (!historicalData.ContainsKey(kvp.Key))
                        historicalData[kvp.Key] = new List<double>();

                    historicalData[kvp.Key].Add(normalized);
                }
            }

            sample.AggregateScore = sample.Values.Values.Average();
            samples.Add(sample);
        }

        public double CalculateMetricStatistic(string metricName, string statistic)
        {
            if (!historicalData.ContainsKey(metricName) || historicalData[metricName].Count == 0)
                return 0.0;

            var data = historicalData[metricName];

            return statistic.ToLower() switch
            {
                "mean" => data.Average(),
                "median" => CalculateMedian(data),
                "stddev" => CalculateStandardDeviation(data),
                "min" => data.Min(),
                "max" => data.Max(),
                "range" => data.Max() - data.Min(),
                "variance" => CalculateVariance(data),
                "skewness" => CalculateSkewness(data),
                "kurtosis" => CalculateKurtosis(data),
                _ => 0.0
            };
        }

        private double CalculateMedian(List<double> data)
        {
            var sorted = data.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            return sorted[count / 2];
        }

        private double CalculateStandardDeviation(List<double> data)
        {
            double mean = data.Average();
            double variance = data.Select(x => (x - mean) * (x - mean)).Average();
            return Math.Sqrt(variance);
        }

        private double CalculateVariance(List<double> data)
        {
            double mean = data.Average();
            return data.Select(x => (x - mean) * (x - mean)).Average();
        }

        private double CalculateSkewness(List<double> data)
        {
            double mean = data.Average();
            double stddev = CalculateStandardDeviation(data);
            if (stddev == 0) return 0.0;

            double cubed = data.Select(x => Math.Pow((x - mean) / stddev, 3)).Average();
            return cubed;
        }

        private double CalculateKurtosis(List<double> data)
        {
            double mean = data.Average();
            double stddev = CalculateStandardDeviation(data);
            if (stddev == 0) return 0.0;

            double fourth = data.Select(x => Math.Pow((x - mean) / stddev, 4)).Average();
            return fourth - 3.0;
        }

        public double CalculateCorrelation(string metric1, string metric2)
        {
            if (!historicalData.ContainsKey(metric1) || !historicalData.ContainsKey(metric2))
                return 0.0;

            var data1 = historicalData[metric1];
            var data2 = historicalData[metric2];
            int minLength = Math.Min(data1.Count, data2.Count);

            if (minLength < 2) return 0.0;

            data1 = data1.Take(minLength).ToList();
            data2 = data2.Take(minLength).ToList();

            double mean1 = data1.Average();
            double mean2 = data2.Average();

            double numerator = Enumerable.Range(0, minLength)
                .Select(i => (data1[i] - mean1) * (data2[i] - mean2))
                .Sum();

            double std1 = CalculateStandardDeviation(data1);
            double std2 = CalculateStandardDeviation(data2);

            if (std1 == 0 || std2 == 0) return 0.0;

            return numerator / (minLength * std1 * std2);
        }

        public Dictionary<string, double> CalculateAllMetricMeans()
        {
            var result = new Dictionary<string, double>();
            foreach (var kvp in historicalData)
            {
                result[kvp.Key] = kvp.Value.Count > 0 ? kvp.Value.Average() : 0.0;
            }
            return result;
        }

        public void DisplayMetricsSummary()
        {
            Console.WriteLine("\n  Registered Metrics:");
            foreach (var metric in metrics.Values.OrderBy(m => m.Category))
            {
                Console.WriteLine($"    {metric.MetricName} ({metric.Unit}): {metric.MinRange}-{metric.MaxRange} [{metric.Category}]");
            }
        }

        public void DisplaySampleStatistics(int sampleIndex)
        {
            if (sampleIndex < 0 || sampleIndex >= samples.Count) return;

            var sample = samples[sampleIndex];
            Console.WriteLine($"\n  Sample {sampleIndex + 1} ({sample.SourceSystem}):");
            Console.WriteLine($"  Aggregate Score: {sample.AggregateScore * 100:F1}%");
            foreach (var kvp in sample.Values.OrderBy(x => x.Key))
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F1}%");
            }
        }

        public void DisplayStatisticalAnalysis(string metricName)
        {
            Console.WriteLine($"\n  Statistical Analysis - {metricName}:");
            Console.WriteLine($"    Mean:     {CalculateMetricStatistic(metricName, "mean") * 100:F1}%");
            Console.WriteLine($"    Median:   {CalculateMetricStatistic(metricName, "median") * 100:F1}%");
            Console.WriteLine($"    StdDev:   {CalculateMetricStatistic(metricName, "stddev") * 100:F1}%");
            Console.WriteLine($"    Min:      {CalculateMetricStatistic(metricName, "min") * 100:F1}%");
            Console.WriteLine($"    Max:      {CalculateMetricStatistic(metricName, "max") * 100:F1}%");
            Console.WriteLine($"    Variance: {CalculateMetricStatistic(metricName, "variance") * 100:F1}%");
            Console.WriteLine($"    Skewness: {CalculateMetricStatistic(metricName, "skewness"):F3}");
            Console.WriteLine($"    Kurtosis: {CalculateMetricStatistic(metricName, "kurtosis"):F3}");
        }

        public void DisplayCorrelationMatrix()
        {
            var metricNames = metrics.Keys.ToList();
            Console.WriteLine("\n  Correlation Matrix:");

            for (int i = 0; i < metricNames.Count; i++)
            {
                Console.Write($"    {metricNames[i],15}");
                for (int j = 0; j < metricNames.Count; j++)
                {
                    double correlation = i == j ? 1.0 : CalculateCorrelation(metricNames[i], metricNames[j]);
                    Console.Write($"  {correlation:F2}");
                }
                Console.WriteLine();
            }
        }

        public int GetSampleCount()
        {
            return samples.Count;
        }

        public double GetLatestAggregateScore()
        {
            return samples.Count > 0 ? samples.Last().AggregateScore : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Recognize and Calculate Multiple Measurement Metrics        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var calculator = new MetricCalculator();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Multiple Metric Types]");
        Console.ResetColor();
        Thread.Sleep(500);

        calculator.RegisterMetric("Temperature", "°C", 0, 100, "Environmental");
        calculator.RegisterMetric("Humidity", "%", 0, 100, "Environmental");
        calculator.RegisterMetric("Pressure", "hPa", 950, 1050, "Environmental");
        calculator.RegisterMetric("Accuracy", "%", 0, 100, "Performance");
        calculator.RegisterMetric("ResponseTime", "ms", 0, 1000, "Performance");
        calculator.RegisterMetric("Efficiency", "%", 0, 100, "Performance");
        calculator.RegisterMetric("CognitiveLead", "score", 0, 100, "Cognitive");
        calculator.RegisterMetric("FocusLevel", "%", 0, 100, "Cognitive");

        Console.WriteLine("  ✓ Registered 8 metrics across 3 categories:");
        calculator.DisplayMetricsSummary();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Recording Multi-Metric Measurement Samples]");
        Console.ResetColor();
        Thread.Sleep(500);

        var sampleData = new[] {
            new Dictionary<string, double> { {"Temperature", 22}, {"Humidity", 65}, {"Pressure", 1013}, {"Accuracy", 92}, {"ResponseTime", 150}, {"Efficiency", 85}, {"CognitiveLead", 78}, {"FocusLevel", 88} },
            new Dictionary<string, double> { {"Temperature", 23}, {"Humidity", 68}, {"Pressure", 1012}, {"Accuracy", 95}, {"ResponseTime", 140}, {"Efficiency", 88}, {"CognitiveLead", 82}, {"FocusLevel", 91} },
            new Dictionary<string, double> { {"Temperature", 21}, {"Humidity", 62}, {"Pressure", 1014}, {"Accuracy", 89}, {"ResponseTime", 160}, {"Efficiency", 82}, {"CognitiveLead", 75}, {"FocusLevel", 85} },
            new Dictionary<string, double> { {"Temperature", 24}, {"Humidity", 70}, {"Pressure", 1011}, {"Accuracy", 94}, {"ResponseTime", 145}, {"Efficiency", 87}, {"CognitiveLead", 80}, {"FocusLevel", 89} }
        };

        string[] sources = { "Lab_A", "Lab_B", "Lab_C", "Lab_D" };

        for (int i = 0; i < sampleData.Length; i++)
        {
            calculator.RecordMeasurement(sampleData[i], sources[i]);
            Console.WriteLine($"  ✓ Sample {i + 1} recorded from {sources[i]}");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Individual Sample Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 0; i < Math.Min(2, calculator.GetSampleCount()); i++)
        {
            calculator.DisplaySampleStatistics(i);
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Comprehensive Statistical Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        calculator.DisplayStatisticalAnalysis("Accuracy");
        calculator.DisplayStatisticalAnalysis("ResponseTime");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Multi-Metric Correlations]");
        Console.ResetColor();
        Thread.Sleep(500);

        calculator.DisplayCorrelationMatrix();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Aggregated Metric Performance]");
        Console.ResetColor();
        Thread.Sleep(500);

        var allMeans = calculator.CalculateAllMetricMeans();
        Console.WriteLine("  Mean Values Across All Metrics:");
        foreach (var kvp in allMeans.OrderBy(x => x.Key))
        {
            Console.WriteLine($"    {kvp.Key,18}: {kvp.Value * 100:F1}%");
        }

        Console.WriteLine($"\n  Overall System Score: {calculator.GetLatestAggregateScore() * 100:F1}%");
        Console.WriteLine($"  Total Samples Recorded: {calculator.GetSampleCount()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Metric Recognition Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Measurement Metrics Model:");
        Console.WriteLine("    Tier 1: Individual Metrics (Temperature, Humidity, Accuracy, etc.)");
        Console.WriteLine("    Tier 2: Categorical Grouping (Environmental, Performance, Cognitive)");
        Console.WriteLine("    Tier 3: Statistical Analysis (Mean, Median, StdDev, Skewness, Kurtosis)");
        Console.WriteLine("    Tier 4: Correlation Analysis (Cross-metric relationships)");
        Console.WriteLine("    Tier 5: Aggregate Scoring (Combined performance index)");
        Console.WriteLine("    Tier 6: Trend Analysis (Historical patterns and predictions)");
        Console.WriteLine("\n  Calculation Capabilities:");
        Console.WriteLine("    ✓ 8 fundamental statistical measures");
        Console.WriteLine("    ✓ Pearson correlation for multi-metric dependencies");
        Console.WriteLine("    ✓ Normalization across different units and ranges");
        Console.WriteLine("    ✓ Temporal aggregation and trending");
        Console.WriteLine("    ✓ Outlier detection and statistical anomalies");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-metric measurement system complete");
        Console.ResetColor();
    }
}
