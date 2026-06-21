using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Linear Vectorization
/// Process of converting a data matrix into a vector
/// Fundamental operation in machine learning and quantum computing
/// </summary>
public class LinearVectorization
{
    private double[,] dataMatrix;
    private double[] vectorizedData;
    private int rows;
    private int columns;

    public LinearVectorization(double[,] matrix)
    {
        this.dataMatrix = matrix;
        this.rows = matrix.GetLength(0);
        this.columns = matrix.GetLength(1);
        this.vectorizedData = new double[rows * columns];
    }

    public double[] VectorizeRowMajor()
    {
        int index = 0;
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                vectorizedData[index++] = dataMatrix[i, j];
            }
        }
        return vectorizedData;
    }

    public double[] VectorizeColumnMajor()
    {
        int index = 0;
        for (int j = 0; j < columns; j++)
        {
            for (int i = 0; i < rows; i++)
            {
                vectorizedData[index++] = dataMatrix[i, j];
            }
        }
        return vectorizedData;
    }

    public double[] VectorizeByDiagonal()
    {
        int index = 0;
        int diagonals = rows + columns - 1;

        for (int d = 0; d < diagonals; d++)
        {
            for (int i = 0; i < rows; i++)
            {
                int j = d - i;
                if (j >= 0 && j < columns)
                {
                    vectorizedData[index++] = dataMatrix[i, j];
                }
            }
        }
        return vectorizedData;
    }

    public double[] NormalizeVector()
    {
        double norm = Math.Sqrt(vectorizedData.Sum(x => x * x));

        if (norm > 0)
        {
            for (int i = 0; i < vectorizedData.Length; i++)
                vectorizedData[i] /= norm;
        }

        return vectorizedData;
    }

    public double[] StandardizeVector()
    {
        double mean = vectorizedData.Average();
        double variance = vectorizedData.Average(x => Math.Pow(x - mean, 2));
        double stdDev = Math.Sqrt(variance);

        if (stdDev > 0)
        {
            for (int i = 0; i < vectorizedData.Length; i++)
                vectorizedData[i] = (vectorizedData[i] - mean) / stdDev;
        }

        return vectorizedData;
    }

    public double CalculateMagnitude()
    {
        return Math.Sqrt(vectorizedData.Sum(x => x * x));
    }

    public double DotProduct(double[] otherVector)
    {
        if (otherVector.Length != vectorizedData.Length)
            throw new ArgumentException("Vector dimensions must match");

        double product = 0;
        for (int i = 0; i < vectorizedData.Length; i++)
            product += vectorizedData[i] * otherVector[i];

        return product;
    }

    public void PrintVector()
    {
        Console.WriteLine("Vectorized Data: [" + string.Join(", ", vectorizedData.Select(x => x.ToString("F3"))) + "]");
    }

    public void PrintMatrix()
    {
        Console.WriteLine("Data Matrix:");
        for (int i = 0; i < rows; i++)
        {
            Console.Write("  [");
            for (int j = 0; j < columns; j++)
            {
                Console.Write(dataMatrix[i, j].ToString("F3"));
                if (j < columns - 1)
                    Console.Write(", ");
            }
            Console.WriteLine("]");
        }
    }

    public static void Main()
    {
        Console.WriteLine("=== Linear Vectorization ===\n");

        double[,] matrix = new double[,]
        {
            { 1.0, 2.0, 3.0 },
            { 4.0, 5.0, 6.0 },
            { 7.0, 8.0, 9.0 }
        };

        var vectorizer = new LinearVectorization(matrix);

        Console.WriteLine("Original Matrix (3x3):");
        vectorizer.PrintMatrix();

        Console.WriteLine("\n1. Row-Major Vectorization:");
        vectorizer.VectorizeRowMajor();
        vectorizer.PrintVector();

        Console.WriteLine("\n2. Column-Major Vectorization:");
        vectorizer.VectorizeColumnMajor();
        vectorizer.PrintVector();

        Console.WriteLine("\n3. Diagonal Vectorization:");
        vectorizer.VectorizeByDiagonal();
        vectorizer.PrintVector();

        Console.WriteLine("\n4. Vector Normalization:");
        vectorizer.VectorizeRowMajor();
        vectorizer.NormalizeVector();
        Console.WriteLine($"Magnitude after normalization: {vectorizer.CalculateMagnitude():F6}");
        vectorizer.PrintVector();

        Console.WriteLine("\n5. Vector Standardization:");
        vectorizer.VectorizeRowMajor();
        vectorizer.StandardizeVector();
        Console.WriteLine($"Mean: {vectorizer.vectorizedData.Average():F6}");
        Console.WriteLine($"StdDev: {Math.Sqrt(vectorizer.vectorizedData.Average(x => x * x)):F6}");
        vectorizer.PrintVector();

        Console.WriteLine("\n6. Dot Product:");
        double[] vec1 = vectorizer.VectorizeRowMajor();
        double[] vec2 = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        double dotProd = vectorizer.DotProduct(vec2);
        Console.WriteLine($"Vector 1: {string.Join(", ", vec1.Select(x => x.ToString("F2")))}");
        Console.WriteLine($"Vector 2: {string.Join(", ", vec2.Select(x => x.ToString("F2")))}");
        Console.WriteLine($"Dot Product: {dotProd:F2}");

        Console.WriteLine("\nLinear Vectorization converts multi-dimensional data into 1D format for processing.");
    }
}
