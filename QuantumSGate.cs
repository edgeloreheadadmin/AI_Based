using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Quantum SGate - Deutsch-Jozsa Algorithm Implementation
/// Determines if a Boolean function is constant or balanced
/// Constant: always returns the same output
/// Balanced: returns 0 for half inputs and 1 for the other half
/// </summary>
public class QuantumSGate
{
    private Func<int, int> booleanFunction;
    private int inputSize;

    public QuantumSGate(Func<int, int> function, int inputSize)
    {
        this.booleanFunction = function;
        this.inputSize = inputSize;
    }

    public string Analyze()
    {
        List<int> outputs = new List<int>();

        int maxInput = (int)Math.Pow(2, inputSize);

        for (int i = 0; i < maxInput; i++)
        {
            outputs.Add(booleanFunction(i));
        }

        return IsConstant(outputs) ? "Constant" : "Balanced";
    }

    private bool IsConstant(List<int> outputs)
    {
        if (outputs.Count == 0)
            return false;

        int firstValue = outputs[0];
        return outputs.All(o => o == firstValue);
    }

    public Dictionary<string, int> GetAnalysisDetails()
    {
        Dictionary<string, int> details = new Dictionary<string, int>();

        int maxInput = (int)Math.Pow(2, inputSize);
        int countZeros = 0, countOnes = 0;

        for (int i = 0; i < maxInput; i++)
        {
            int output = booleanFunction(i);
            if (output == 0)
                countZeros++;
            else
                countOnes++;
        }

        details["ZeroCount"] = countZeros;
        details["OneCount"] = countOnes;
        details["TotalOutputs"] = maxInput;

        return details;
    }

    public static void Main()
    {
        Console.WriteLine("=== Constant Function Test ===");
        Func<int, int> constantFunction = (x) => 1;
        var sgateConstant = new QuantumSGate(constantFunction, 3);
        Console.WriteLine($"Result: {sgateConstant.Analyze()}");
        PrintDetails(sgateConstant.GetAnalysisDetails());

        Console.WriteLine("\n=== Balanced Function Test ===");
        Func<int, int> balancedFunction = (x) => x % 2;
        var sgateBalanced = new QuantumSGate(balancedFunction, 3);
        Console.WriteLine($"Result: {sgateBalanced.Analyze()}");
        PrintDetails(sgateBalanced.GetAnalysisDetails());

        Console.WriteLine("\n=== XOR Function (Balanced) ===");
        Func<int, int> xorFunction = (x) => (x ^ (x >> 1)) & 1;
        var sgateXor = new QuantumSGate(xorFunction, 3);
        Console.WriteLine($"Result: {sgateXor.Analyze()}");
        PrintDetails(sgateXor.GetAnalysisDetails());
    }

    private static void PrintDetails(Dictionary<string, int> details)
    {
        Console.WriteLine($"Zeros: {details["ZeroCount"]}, Ones: {details["OneCount"]}, Total: {details["TotalOutputs"]}");
    }
}
