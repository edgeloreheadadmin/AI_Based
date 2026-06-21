using System;
using System.Collections.Generic;

/// <summary>
/// Quantum TGate - Deutsch Algorithm Implementation
/// Determines if a Boolean function is balanced
/// Balanced: equal number of outputs of 0 and 1
/// Uses superposition and entanglement concepts
/// </summary>
public class QuantumTGate
{
    private Func<int, int> booleanFunction;
    private int phaseKickback;

    public QuantumTGate(Func<int, int> function)
    {
        this.booleanFunction = function;
        this.phaseKickback = 0;
    }

    public bool IsBalanced()
    {
        var superpositionResult = ApplySuperposition();
        return DetectEntanglement(superpositionResult);
    }

    private List<int> ApplySuperposition()
    {
        List<int> superposition = new List<int> { 0, 1 };
        List<int> results = new List<int>();

        foreach (int input in superposition)
        {
            results.Add(booleanFunction(input));
        }

        return results;
    }

    private bool DetectEntanglement(List<int> results)
    {
        if (results.Count != 2)
            return false;

        int result0 = results[0];
        int result1 = results[1];

        return result0 != result1;
    }

    public int GetPhaseKickback()
    {
        var results = ApplySuperposition();
        foreach (int result in results)
        {
            if (result == 1)
                phaseKickback = (phaseKickback + 1) % 2;
        }
        return phaseKickback;
    }

    public Dictionary<string, object> Analyze()
    {
        Dictionary<string, object> analysis = new Dictionary<string, object>();

        var results = ApplySuperposition();
        bool balanced = DetectEntanglement(results);

        analysis["IsBalanced"] = balanced;
        analysis["Output_for_0"] = results[0];
        analysis["Output_for_1"] = results[1];
        analysis["PhaseKickback"] = GetPhaseKickback();
        analysis["Measurement"] = balanced ? "Balanced (Entangled)" : "Not Balanced (Separable)";

        return analysis;
    }

    public static void Main()
    {
        Console.WriteLine("=== Balanced Function Test (XOR) ===");
        Func<int, int> xorFunction = (x) => x;
        var tgateXor = new QuantumTGate(xorFunction);
        PrintAnalysis(tgateXor.Analyze());

        Console.WriteLine("\n=== NOT Balanced Function (Constant 0) ===");
        Func<int, int> constant0 = (x) => 0;
        var tgateConstant0 = new QuantumTGate(constant0);
        PrintAnalysis(tgateConstant0.Analyze());

        Console.WriteLine("\n=== NOT Balanced Function (Constant 1) ===");
        Func<int, int> constant1 = (x) => 1;
        var tgateConstant1 = new QuantumTGate(constant1);
        PrintAnalysis(tgateConstant1.Analyze());

        Console.WriteLine("\n=== Balanced Function Test (Negated Input) ===");
        Func<int, int> negatedInput = (x) => x == 0 ? 1 : 0;
        var tgateNegated = new QuantumTGate(negatedInput);
        PrintAnalysis(tgateNegated.Analyze());
    }

    private static void PrintAnalysis(Dictionary<string, object> analysis)
    {
        foreach (var kvp in analysis)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }
}
