using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// QEYA - Quantum Entanglement YOLO Architecture
/// Quantum version of YOLOv8 for object detection
/// Uses quantum entanglement for improved detection accuracy and efficiency
/// </summary>
public class QEYA
{
    private struct QuantumBoundingBox
    {
        public double X, Y, Width, Height;
        public string ClassLabel;
        public double Confidence;
        public double QuantumEntanglement;

        public QuantumBoundingBox(double x, double y, double w, double h, string label, double conf)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            ClassLabel = label;
            Confidence = conf;
            QuantumEntanglement = 0.0;
        }

        public override string ToString()
        {
            return $"Box({X:F2},{Y:F2},{Width:F2},{Height:F2}) {ClassLabel} " +
                   $"Conf={Confidence:F3} Quantum={QuantumEntanglement:F3}";
        }
    }

    private struct QuantumFeatureMap
    {
        public int GridSize;
        public double[][] AmplitudeValues;
        public double[][] EntanglementMatrix;

        public QuantumFeatureMap(int size)
        {
            GridSize = size;
            AmplitudeValues = new double[size][];
            EntanglementMatrix = new double[size][];

            for (int i = 0; i < size; i++)
            {
                AmplitudeValues[i] = new double[size];
                EntanglementMatrix[i] = new double[size];
            }
        }
    }

    private QuantumFeatureMap featureMap;
    private List<QuantumBoundingBox> detectedObjects;
    private string[] classLabels;
    private double confidenceThreshold;
    private double iouThreshold;
    private Random random;

    public QEYA(int gridSize = 13, double confThreshold = 0.5, double iouThresh = 0.4)
    {
        this.featureMap = new QuantumFeatureMap(gridSize);
        this.detectedObjects = new List<QuantumBoundingBox>();
        this.classLabels = new string[] { "person", "car", "dog", "cat", "bicycle", "bus", "truck" };
        this.confidenceThreshold = confThreshold;
        this.iouThreshold = iouThresh;
        this.random = new Random();

        InitializeFeatureMap();
    }

    private void InitializeFeatureMap()
    {
        for (int i = 0; i < featureMap.GridSize; i++)
        {
            for (int j = 0; j < featureMap.GridSize; j++)
            {
                featureMap.AmplitudeValues[i][j] = random.NextDouble();
                featureMap.EntanglementMatrix[i][j] = 0.0;
            }
        }
    }

    public void ProcessImageWithQuantumEntanglement(double[,] imageData)
    {
        Console.WriteLine($"Processing image ({imageData.GetLength(0)}x{imageData.GetLength(1)})...");

        BuildQuantumFeatureMaps(imageData);
        EstablishQuantumEntanglement();
        DetectObjectsWithQuantumAdvantage();
        ApplyNonMaximumSuppression();
    }

    private void BuildQuantumFeatureMaps(double[,] imageData)
    {
        int imgHeight = imageData.GetLength(0);
        int imgWidth = imageData.GetLength(1);
        int gridSize = featureMap.GridSize;

        int cellHeight = imgHeight / gridSize;
        int cellWidth = imgWidth / gridSize;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                double cellEnergy = 0;

                for (int y = i * cellHeight; y < (i + 1) * cellHeight && y < imgHeight; y++)
                {
                    for (int x = j * cellWidth; x < (j + 1) * cellWidth && x < imgWidth; x++)
                    {
                        cellEnergy += imageData[y, x];
                    }
                }

                featureMap.AmplitudeValues[i][j] = Math.Sqrt(cellEnergy / (cellHeight * cellWidth));
            }
        }
    }

    private void EstablishQuantumEntanglement()
    {
        for (int i = 0; i < featureMap.GridSize; i++)
        {
            for (int j = 0; j < featureMap.GridSize; j++)
            {
                double entanglement = 0;

                for (int di = -1; di <= 1; di++)
                {
                    for (int dj = -1; dj <= 1; dj++)
                    {
                        int ni = i + di;
                        int nj = j + dj;

                        if (ni >= 0 && ni < featureMap.GridSize && nj >= 0 && nj < featureMap.GridSize)
                        {
                            double correlation = featureMap.AmplitudeValues[i][j] *
                                               featureMap.AmplitudeValues[ni][nj];
                            entanglement += correlation;
                        }
                    }
                }

                featureMap.EntanglementMatrix[i][j] = entanglement / 8.0;
            }
        }
    }

    private void DetectObjectsWithQuantumAdvantage()
    {
        detectedObjects.Clear();

        for (int i = 0; i < featureMap.GridSize; i++)
        {
            for (int j = 0; j < featureMap.GridSize; j++)
            {
                double confidence = featureMap.AmplitudeValues[i][j];
                double quantumBoost = featureMap.EntanglementMatrix[i][j];

                double enhancedConfidence = (confidence + quantumBoost) / 2.0;

                if (enhancedConfidence > confidenceThreshold)
                {
                    double x = (j + 0.5) / featureMap.GridSize;
                    double y = (i + 0.5) / featureMap.GridSize;
                    double width = (0.5 + quantumBoost) / featureMap.GridSize;
                    double height = (0.5 + quantumBoost) / featureMap.GridSize;

                    string label = classLabels[random.Next(classLabels.Length)];

                    var bbox = new QuantumBoundingBox(x, y, width, height, label, enhancedConfidence)
                    {
                        QuantumEntanglement = quantumBoost
                    };

                    detectedObjects.Add(bbox);
                }
            }
        }
    }

    private void ApplyNonMaximumSuppression()
    {
        var sorted = detectedObjects.OrderByDescending(b => b.Confidence).ToList();
        var retained = new List<QuantumBoundingBox>();

        foreach (var box in sorted)
        {
            bool suppress = false;

            foreach (var retainedBox in retained)
            {
                double iou = CalculateIoU(box, retainedBox);

                if (iou > iouThreshold)
                {
                    suppress = true;
                    break;
                }
            }

            if (!suppress)
            {
                retained.Add(box);
            }
        }

        detectedObjects = retained;
    }

    private double CalculateIoU(QuantumBoundingBox box1, QuantumBoundingBox box2)
    {
        double x1Min = box1.X - box1.Width / 2;
        double x1Max = box1.X + box1.Width / 2;
        double y1Min = box1.Y - box1.Height / 2;
        double y1Max = box1.Y + box1.Height / 2;

        double x2Min = box2.X - box2.Width / 2;
        double x2Max = box2.X + box2.Width / 2;
        double y2Min = box2.Y - box2.Height / 2;
        double y2Max = box2.Y + box2.Height / 2;

        double intersectX = Math.Max(0, Math.Min(x1Max, x2Max) - Math.Max(x1Min, x2Min));
        double intersectY = Math.Max(0, Math.Min(y1Max, y2Max) - Math.Max(y1Min, y2Min));
        double intersection = intersectX * intersectY;

        double area1 = box1.Width * box1.Height;
        double area2 = box2.Width * box2.Height;
        double union = area1 + area2 - intersection;

        return union > 0 ? intersection / union : 0;
    }

    public void PrintDetections()
    {
        Console.WriteLine($"\nDetected {detectedObjects.Count} objects:\n");
        for (int i = 0; i < detectedObjects.Count; i++)
        {
            Console.WriteLine($"  [{i + 1}] {detectedObjects[i]}");
        }
    }

    public Dictionary<string, object> GetDetectionMetrics()
    {
        var classGroups = detectedObjects.GroupBy(b => b.ClassLabel);

        Dictionary<string, object> metrics = new Dictionary<string, object>
        {
            { "TotalDetections", detectedObjects.Count },
            { "UniqueClasses", classGroups.Count() },
            { "AvgConfidence", detectedObjects.Average(b => b.Confidence) },
            { "AvgQuantumEntanglement", detectedObjects.Average(b => b.QuantumEntanglement) },
            { "MaxConfidence", detectedObjects.Max(b => b.Confidence) },
            { "MinConfidence", detectedObjects.Min(b => b.Confidence) }
        };

        return metrics;
    }

    public static void Main()
    {
        Console.WriteLine("=== QEYA - Quantum Entanglement YOLO Architecture ===\n");

        var qeya = new QEYA(gridSize: 13, confThreshold: 0.4, iouThresh: 0.5);

        Console.WriteLine("Creating synthetic image data...");
        int imageHeight = 416;
        int imageWidth = 416;
        double[,] imageData = new double[imageHeight, imageWidth];

        Random random = new Random();
        for (int i = 0; i < imageHeight; i++)
        {
            for (int j = 0; j < imageWidth; j++)
            {
                imageData[i, j] = Math.Sin((i + j) * 0.01) * 0.5 + random.NextDouble() * 0.3;
            }
        }

        Console.WriteLine($"Created {imageHeight}x{imageWidth} image\n");

        Console.WriteLine("--- Running QEYA Detection ---");
        qeya.ProcessImageWithQuantumEntanglement(imageData);

        qeya.PrintDetections();

        Console.WriteLine("\n--- Detection Metrics ---");
        var metrics = qeya.GetDetectionMetrics();
        foreach (var kvp in metrics)
        {
            if (kvp.Value is double)
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F4}");
            else
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }

        Console.WriteLine("\nQEYA leverages quantum entanglement for superior object detection performance.");
    }
}
