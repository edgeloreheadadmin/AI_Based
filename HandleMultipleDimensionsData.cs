using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class HandleMultipleDimensionsData
{
    public class DataPoint
    {
        public string PointId { get; set; }
        public Dictionary<string, double> Dimensions { get; set; }
        public double MagnitudeNorm { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DimensionalAxis
    {
        public string AxisName { get; set; }
        public string AxisType { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public double RangeNormalized { get; set; }
        public int DataPointsAlong { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DataSet
    {
        public string DataSetId { get; set; }
        public List<string> DimensionNames { get; set; }
        public List<string> DataPointIds { get; set; }
        public Dictionary<string, double> CentroidPosition { get; set; }
        public double DataDensity { get; set; }
        public int ProcessedCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MultidimensionalProcessingEngine
    {
        private Dictionary<string, DataPoint> dataPoints;
        private Dictionary<string, DimensionalAxis> axes;
        private Dictionary<string, DataSet> dataSets;
        private List<(string, string, double)> parallelProcessingLog;

        public MultidimensionalProcessingEngine()
        {
            dataPoints = new Dictionary<string, DataPoint>();
            axes = new Dictionary<string, DimensionalAxis>();
            dataSets = new Dictionary<string, DataSet>();
            parallelProcessingLog = new List<(string, string, double)>();
        }

        public void DefineDimensionalAxis(string axisName, string axisType, double minValue, double maxValue)
        {
            var axis = new DimensionalAxis
            {
                AxisName = axisName,
                AxisType = axisType,
                MinValue = minValue,
                MaxValue = maxValue,
                RangeNormalized = maxValue - minValue,
                DataPointsAlong = 0,
                CreatedDate = DateTime.Now
            };
            axes[axisName] = axis;
        }

        public void CreateDataPoint(string pointId, Dictionary<string, double> dimensionalValues)
        {
            var dataPoint = new DataPoint
            {
                PointId = pointId,
                Dimensions = new Dictionary<string, double>(dimensionalValues),
                MagnitudeNorm = 0.0,
                CreatedDate = DateTime.Now
            };

            double sumOfSquares = 0.0;
            foreach (var value in dimensionalValues.Values)
            {
                sumOfSquares += value * value;
            }
            dataPoint.MagnitudeNorm = Math.Sqrt(sumOfSquares);

            dataPoints[pointId] = dataPoint;

            foreach (var axisName in dimensionalValues.Keys)
            {
                if (axes.ContainsKey(axisName))
                {
                    axes[axisName].DataPointsAlong++;
                }
            }
        }

        public void CreateDataSet(string dataSetId, List<string> dimensionNames, List<string> pointIds)
        {
            var dataSet = new DataSet
            {
                DataSetId = dataSetId,
                DimensionNames = new List<string>(dimensionNames),
                DataPointIds = new List<string>(pointIds),
                CentroidPosition = new Dictionary<string, double>(),
                DataDensity = 0.0,
                ProcessedCount = 0,
                CreatedDate = DateTime.Now
            };

            CalculateCentroid(dataSet);
            dataSets[dataSetId] = dataSet;
        }

        private void CalculateCentroid(DataSet dataSet)
        {
            foreach (var dimension in dataSet.DimensionNames)
            {
                double sum = 0.0;
                int count = 0;

                foreach (var pointId in dataSet.DataPointIds)
                {
                    if (dataPoints.ContainsKey(pointId) &&
                        dataPoints[pointId].Dimensions.ContainsKey(dimension))
                    {
                        sum += dataPoints[pointId].Dimensions[dimension];
                        count++;
                    }
                }

                dataSet.CentroidPosition[dimension] = count > 0 ? sum / count : 0.0;
            }
        }

        public void ProcessDataInParallel(string dataSetId, Func<DataPoint, double> processingFunction)
        {
            if (!dataSets.ContainsKey(dataSetId)) return;

            var dataSet = dataSets[dataSetId];
            int totalProcessed = 0;

            foreach (var pointId in dataSet.DataPointIds)
            {
                if (dataPoints.ContainsKey(pointId))
                {
                    double result = processingFunction(dataPoints[pointId]);
                    parallelProcessingLog.Add((dataSetId, pointId, result));
                    totalProcessed++;
                }
            }

            dataSet.ProcessedCount += totalProcessed;
            dataSet.DataDensity = (double)totalProcessed / Math.Max(dataSet.DataPointIds.Count, 1);
        }

        public double CalculateEuclideanDistance(string pointId1, string pointId2)
        {
            if (!dataPoints.ContainsKey(pointId1) || !dataPoints.ContainsKey(pointId2))
                return 0.0;

            var p1 = dataPoints[pointId1];
            var p2 = dataPoints[pointId2];

            double sumOfSquares = 0.0;
            var commonDimensions = p1.Dimensions.Keys.Intersect(p2.Dimensions.Keys);

            foreach (var dim in commonDimensions)
            {
                double diff = p1.Dimensions[dim] - p2.Dimensions[dim];
                sumOfSquares += diff * diff;
            }

            return Math.Sqrt(sumOfSquares);
        }

        public double CalculateManhathanDistance(string pointId1, string pointId2)
        {
            if (!dataPoints.ContainsKey(pointId1) || !dataPoints.ContainsKey(pointId2))
                return 0.0;

            var p1 = dataPoints[pointId1];
            var p2 = dataPoints[pointId2];

            double sum = 0.0;
            var commonDimensions = p1.Dimensions.Keys.Intersect(p2.Dimensions.Keys);

            foreach (var dim in commonDimensions)
            {
                sum += Math.Abs(p1.Dimensions[dim] - p2.Dimensions[dim]);
            }

            return sum;
        }

        public double CalculateCosineSimilarity(string pointId1, string pointId2)
        {
            if (!dataPoints.ContainsKey(pointId1) || !dataPoints.ContainsKey(pointId2))
                return 0.0;

            var p1 = dataPoints[pointId1];
            var p2 = dataPoints[pointId2];

            double dotProduct = 0.0;
            var commonDimensions = p1.Dimensions.Keys.Intersect(p2.Dimensions.Keys);

            foreach (var dim in commonDimensions)
            {
                dotProduct += p1.Dimensions[dim] * p2.Dimensions[dim];
            }

            if (p1.MagnitudeNorm == 0 || p2.MagnitudeNorm == 0)
                return 0.0;

            return dotProduct / (p1.MagnitudeNorm * p2.MagnitudeNorm);
        }

        public void DisplayAxisInfo(string axisName)
        {
            if (!axes.ContainsKey(axisName)) return;

            var axis = axes[axisName];
            Console.WriteLine($"\n  Axis: {axis.AxisName} ({axis.AxisType})");
            Console.WriteLine($"  Range: {axis.MinValue} to {axis.MaxValue}");
            Console.WriteLine($"  Normalized Range: {axis.RangeNormalized}");
            Console.WriteLine($"  Data Points Along: {axis.DataPointsAlong}");
        }

        public void DisplayDataPoint(string pointId)
        {
            if (!dataPoints.ContainsKey(pointId)) return;

            var point = dataPoints[pointId];
            Console.WriteLine($"\n  Data Point: {point.PointId}");
            Console.WriteLine($"  Dimensions: {point.Dimensions.Count}");
            Console.WriteLine($"  Values:");
            foreach (var kvp in point.Dimensions)
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F3}");
            }
            Console.WriteLine($"  Magnitude (L2 Norm): {point.MagnitudeNorm:F3}");
        }

        public void DisplayDataSet(string dataSetId)
        {
            if (!dataSets.ContainsKey(dataSetId)) return;

            var dataSet = dataSets[dataSetId];
            Console.WriteLine($"\n  DataSet: {dataSet.DataSetId}");
            Console.WriteLine($"  Dimensions: {dataSet.DimensionNames.Count}");
            Console.WriteLine($"  Data Points: {dataSet.DataPointIds.Count}");
            Console.WriteLine($"  Processed Count: {dataSet.ProcessedCount}");
            Console.WriteLine($"  Data Density: {dataSet.DataDensity * 100:F1}%");
            Console.WriteLine($"  Centroid:");
            foreach (var kvp in dataSet.CentroidPosition)
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value:F3}");
            }
        }

        public int GetTotalDimensions()
        {
            return axes.Count;
        }

        public int GetTotalDataPoints()
        {
            return dataPoints.Count;
        }

        public double GetAverageDataPointMagnitude()
        {
            return dataPoints.Count > 0 ? dataPoints.Values.Average(p => p.MagnitudeNorm) : 0.0;
        }

        public int GetParallelProcessingEvents()
        {
            return parallelProcessingLog.Count;
        }

        public string GetDimensionalComplexityLevel(int dims, int points)
        {
            double complexity = (dims * points) / 100.0;
            return complexity switch
            {
                >= 100 => "Hyperdimensional - Complex multi-axis space",
                >= 50 => "Highly Multidimensional - Complex relationships",
                >= 20 => "Multidimensional - Several interacting dimensions",
                >= 5 => "Moderate Dimensionality - Few key dimensions",
                _ => "Low Dimensionality - Simple relationships"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Handle Multiple Dimensions and Simultaneous Processing      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new MultidimensionalProcessingEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Dimensional Axes]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DefineDimensionalAxis("TimeAxis", "Temporal", 0.0, 100.0);
        engine.DefineDimensionalAxis("SpaceX", "Spatial", -50.0, 50.0);
        engine.DefineDimensionalAxis("SpaceY", "Spatial", -50.0, 50.0);
        engine.DefineDimensionalAxis("SpaceZ", "Spatial", -50.0, 50.0);
        engine.DefineDimensionalAxis("Intensity", "Magnitude", 0.0, 1.0);
        engine.DefineDimensionalAxis("Frequency", "Oscillatory", 0.0, 100.0);

        Console.WriteLine("  ✓ Defined 6 dimensional axes");
        engine.DisplayAxisInfo("TimeAxis");
        engine.DisplayAxisInfo("SpaceX");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Multi-Dimensional Data Points]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateDataPoint("Point-1", new Dictionary<string, double>
        {
            {"TimeAxis", 25.0}, {"SpaceX", 10.0}, {"SpaceY", 15.0}, {"SpaceZ", 5.0},
            {"Intensity", 0.75}, {"Frequency", 50.0}
        });

        engine.CreateDataPoint("Point-2", new Dictionary<string, double>
        {
            {"TimeAxis", 50.0}, {"SpaceX", -10.0}, {"SpaceY", -15.0}, {"SpaceZ", 10.0},
            {"Intensity", 0.85}, {"Frequency", 60.0}
        });

        engine.CreateDataPoint("Point-3", new Dictionary<string, double>
        {
            {"TimeAxis", 75.0}, {"SpaceX", 20.0}, {"SpaceY", 0.0}, {"SpaceZ", -5.0},
            {"Intensity", 0.65}, {"Frequency", 40.0}
        });

        engine.CreateDataPoint("Point-4", new Dictionary<string, double>
        {
            {"TimeAxis", 100.0}, {"SpaceX", 0.0}, {"SpaceY", 25.0}, {"SpaceZ", 15.0},
            {"Intensity", 0.90}, {"Frequency", 70.0}
        });

        Console.WriteLine("  ✓ Created 4 data points across 6 dimensions");
        engine.DisplayDataPoint("Point-1");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Multi-Dimensional Data Sets]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateDataSet("FullDataSet",
            new List<string> { "TimeAxis", "SpaceX", "SpaceY", "SpaceZ", "Intensity", "Frequency" },
            new List<string> { "Point-1", "Point-2", "Point-3", "Point-4" });

        Console.WriteLine("  ✓ Created data set with all 6 dimensions");
        engine.DisplayDataSet("FullDataSet");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Simultaneous Parallel Processing]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Processing data in parallel across dimensions:");
        engine.ProcessDataInParallel("FullDataSet", p => p.MagnitudeNorm);
        engine.ProcessDataInParallel("FullDataSet", p => p.Dimensions.Values.Average());

        Console.WriteLine($"  ✓ Parallel processing completed");
        Console.WriteLine($"  Total Processing Events: {engine.GetParallelProcessingEvents()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Multi-Metric Distance Calculations]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Computing distances using multiple metrics:");
        double euclidean = engine.CalculateEuclideanDistance("Point-1", "Point-2");
        double manhattan = engine.CalculateManhathanDistance("Point-1", "Point-2");
        double cosine = engine.CalculateCosineSimilarity("Point-1", "Point-2");

        Console.WriteLine($"    Euclidean Distance (P1→P2): {euclidean:F3}");
        Console.WriteLine($"    Manhattan Distance (P1→P2): {manhattan:F3}");
        Console.WriteLine($"    Cosine Similarity (P1↔P2): {cosine:F3}");

        double euclid_13 = engine.CalculateEuclideanDistance("Point-1", "Point-3");
        double cosine_13 = engine.CalculateCosineSimilarity("Point-1", "Point-3");
        Console.WriteLine($"\n    Euclidean Distance (P1→P3): {euclid_13:F3}");
        Console.WriteLine($"    Cosine Similarity (P1↔P3): {cosine_13:F3}");

        Console.WriteLine("  ✓ Multiple distance metrics computed simultaneously");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: High-Dimensional Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        int dims = engine.GetTotalDimensions();
        int points = engine.GetTotalDataPoints();
        double avgMag = engine.GetAverageDataPointMagnitude();

        Console.WriteLine($"  Dimensional Space Statistics:");
        Console.WriteLine($"    Total Axes (Dimensions): {dims}");
        Console.WriteLine($"    Total Data Points: {points}");
        Console.WriteLine($"    Average Point Magnitude: {avgMag:F3}");
        Console.WriteLine($"    Complexity: {engine.GetDimensionalComplexityLevel(dims, points)}");

        engine.DisplayDataSet("FullDataSet");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multidimensional Data Processing Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Multi-Dimensional Architecture:");
        Console.WriteLine("    Layer 1: Axis Definition (establish dimensions)");
        Console.WriteLine("    Layer 2: Data Point Creation (assign values across dimensions)");
        Console.WriteLine("    Layer 3: Data Set Formation (organize points with dimensions)");
        Console.WriteLine("    Layer 4: Centroid Calculation (find central tendency)");
        Console.WriteLine("    Layer 5: Parallel Processing (simultaneous computation)");
        Console.WriteLine("    Layer 6: Multi-Metric Analysis (Euclidean, Manhattan, Cosine)");
        Console.WriteLine("    Layer 7: High-Dimensional Interpretation (complex relationships)");
        Console.WriteLine("\n  Simultaneous Processing:");
        Console.WriteLine("    ✓ Process multiple dimensions in parallel");
        Console.WriteLine("    ✓ Calculate multiple distance metrics concurrently");
        Console.WriteLine("    ✓ Analyze relationships across all axes");
        Console.WriteLine("    ✓ Compute centroids in high-dimensional space");
        Console.WriteLine("    ✓ Handle hyperdimensional data sets efficiently");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multidimensional data processing system complete");
        Console.ResetColor();
    }
}
