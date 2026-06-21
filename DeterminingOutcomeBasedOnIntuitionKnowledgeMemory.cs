using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DeterminingOutcomeBasedOnIntuitionKnowledgeMemory
{
    public class IntuitionFactor
    {
        public string FactorId { get; set; }
        public string Description { get; set; }
        public double IntuitionStrength { get; set; }
        public string IntuitionType { get; set; }
        public DateTime AssessedDate { get; set; }
    }

    public class KnowledgeFactor
    {
        public string FactorId { get; set; }
        public string Topic { get; set; }
        public double KnowledgeLevel { get; set; }
        public List<string> RelatedConcepts { get; set; }
        public int YearsOfExperience { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MemoryFactor
    {
        public string MemoryId { get; set; }
        public string Experience { get; set; }
        public double Relevance { get; set; }
        public string MemoryType { get; set; }
        public double RecallAccuracy { get; set; }
        public DateTime MemoryDate { get; set; }
    }

    public class OutcomeCalculation
    {
        public string CalculationId { get; set; }
        public double IntuitionScore { get; set; }
        public double KnowledgeScore { get; set; }
        public double MemoryScore { get; set; }
        public double IntuitionWeight { get; set; }
        public double KnowledgeWeight { get; set; }
        public double MemoryWeight { get; set; }
        public double FinalOutcomeScore { get; set; }
        public DateTime CalculatedDate { get; set; }
    }

    public class PredictionResult
    {
        public string PredictionId { get; set; }
        public string PredictionStatement { get; set; }
        public double PredictionConfidence { get; set; }
        public string RecommendedAction { get; set; }
        public List<string> KeyFactors { get; set; }
        public string OutcomeCategory { get; set; }
        public DateTime PredictedDate { get; set; }
    }

    public class OutcomePredictionEngine
    {
        private Dictionary<string, IntuitionFactor> intuitions;
        private Dictionary<string, KnowledgeFactor> knowledge;
        private Dictionary<string, MemoryFactor> memories;
        private Dictionary<string, OutcomeCalculation> calculations;
        private Dictionary<string, PredictionResult> predictions;

        public OutcomePredictionEngine()
        {
            intuitions = new Dictionary<string, IntuitionFactor>();
            knowledge = new Dictionary<string, KnowledgeFactor>();
            memories = new Dictionary<string, MemoryFactor>();
            calculations = new Dictionary<string, OutcomeCalculation>();
            predictions = new Dictionary<string, PredictionResult>();
        }

        public void RecordIntuition(string intuitionId, string description, double strength, string type)
        {
            var intuition = new IntuitionFactor
            {
                FactorId = intuitionId,
                Description = description,
                IntuitionStrength = strength,
                IntuitionType = type,
                AssessedDate = DateTime.Now
            };
            intuitions[intuitionId] = intuition;
        }

        public void RecordKnowledge(string knowledgeId, string topic, double level, List<string> concepts, int years)
        {
            var knowledge_factor = new KnowledgeFactor
            {
                FactorId = knowledgeId,
                Topic = topic,
                KnowledgeLevel = level,
                RelatedConcepts = new List<string>(concepts),
                YearsOfExperience = years,
                CreatedDate = DateTime.Now
            };
            knowledge[knowledgeId] = knowledge_factor;
        }

        public void RecordMemory(string memoryId, string experience, double relevance, string type, double accuracy)
        {
            var memory = new MemoryFactor
            {
                MemoryId = memoryId,
                Experience = experience,
                Relevance = relevance,
                MemoryType = type,
                RecallAccuracy = accuracy,
                MemoryDate = DateTime.Now
            };
            memories[memoryId] = memory;
        }

        public void CalculateOutcome(string calculationId, double intuWeight = 0.3, double knowWeight = 0.5, double memWeight = 0.2)
        {
            var calculation = new OutcomeCalculation
            {
                CalculationId = calculationId,
                IntuitionScore = intuitions.Count > 0 ? intuitions.Values.Average(i => i.IntuitionStrength) : 0.0,
                KnowledgeScore = knowledge.Count > 0 ? knowledge.Values.Average(k => k.KnowledgeLevel) : 0.0,
                MemoryScore = memories.Count > 0 ? memories.Values.Average(m => m.Relevance * m.RecallAccuracy) : 0.0,
                IntuitionWeight = intuWeight,
                KnowledgeWeight = knowWeight,
                MemoryWeight = memWeight,
                CalculatedDate = DateTime.Now
            };

            calculation.FinalOutcomeScore = (calculation.IntuitionScore * intuWeight) +
                                           (calculation.KnowledgeScore * knowWeight) +
                                           (calculation.MemoryScore * memWeight);

            calculations[calculationId] = calculation;
        }

        public void GeneratePrediction(string predictionId, string statement)
        {
            if (calculations.Count == 0) return;

            var latestCalculation = calculations.Values.Last();
            var prediction = new PredictionResult
            {
                PredictionId = predictionId,
                PredictionStatement = statement,
                PredictionConfidence = latestCalculation.FinalOutcomeScore,
                RecommendedAction = GenerateRecommendation(latestCalculation.FinalOutcomeScore),
                KeyFactors = new List<string>(),
                OutcomeCategory = CategorizeOutcome(latestCalculation.FinalOutcomeScore),
                PredictedDate = DateTime.Now
            };

            if (latestCalculation.IntuitionScore > 0.7)
                prediction.KeyFactors.Add("Strong intuitive guidance");
            if (latestCalculation.KnowledgeScore > 0.8)
                prediction.KeyFactors.Add("Solid knowledge base");
            if (latestCalculation.MemoryScore > 0.7)
                prediction.KeyFactors.Add("Relevant past experiences");

            predictions[predictionId] = prediction;
        }

        private string GenerateRecommendation(double score)
        {
            return score switch
            {
                >= 0.85 => "Proceed with high confidence",
                >= 0.70 => "Proceed with moderate caution",
                >= 0.55 => "Proceed with careful consideration",
                >= 0.40 => "Reconsider or gather more information",
                _ => "Do not proceed - insufficient foundation"
            };
        }

        private string CategorizeOutcome(double score)
        {
            return score switch
            {
                >= 0.85 => "Very Positive",
                >= 0.70 => "Positive",
                >= 0.55 => "Neutral/Mixed",
                >= 0.40 => "Negative",
                _ => "Very Negative"
            };
        }

        public void DisplayCalculation(string calculationId)
        {
            if (!calculations.ContainsKey(calculationId)) return;

            var calc = calculations[calculationId];
            Console.WriteLine($"\n  Outcome Calculation: {calc.CalculationId}");
            Console.WriteLine($"  Intuition Score: {calc.IntuitionScore * 100:F1}% (Weight: {calc.IntuitionWeight * 100:F0}%)");
            Console.WriteLine($"  Knowledge Score: {calc.KnowledgeScore * 100:F1}% (Weight: {calc.KnowledgeWeight * 100:F0}%)");
            Console.WriteLine($"  Memory Score: {calc.MemoryScore * 100:F1}% (Weight: {calc.MemoryWeight * 100:F0}%)");
            Console.WriteLine($"  Final Outcome Score: {calc.FinalOutcomeScore * 100:F1}%");
        }

        public void DisplayPrediction(string predictionId)
        {
            if (!predictions.ContainsKey(predictionId)) return;

            var pred = predictions[predictionId];
            Console.WriteLine($"\n  Prediction: {pred.PredictionId}");
            Console.WriteLine($"  Statement: {pred.PredictionStatement}");
            Console.WriteLine($"  Confidence: {pred.PredictionConfidence * 100:F1}%");
            Console.WriteLine($"  Category: {pred.OutcomeCategory}");
            Console.WriteLine($"  Recommendation: {pred.RecommendedAction}");
            if (pred.KeyFactors.Count > 0)
            {
                Console.WriteLine($"  Key Factors:");
                foreach (var factor in pred.KeyFactors)
                {
                    Console.WriteLine($"    • {factor}");
                }
            }
        }

        public int GetFactorCounts()
        {
            return intuitions.Count + knowledge.Count + memories.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Determining Outcome Based on Intuition, Knowledge, Memory    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new OutcomePredictionEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Recording Intuitive Factors]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordIntuition("INTUIT-001", "Feeling of unease about decision", 0.75, "Gut feeling");
        engine.RecordIntuition("INTUIT-002", "Sense of opportunity", 0.85, "Positive instinct");
        engine.RecordIntuition("INTUIT-003", "Pattern recognition", 0.80, "Subconscious awareness");

        Console.WriteLine("  ✓ Recorded 3 intuitive factors");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Recording Knowledge Factors]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordKnowledge("KNOW-001", "Project Management", 0.85,
            new List<string> { "Estimation", "Risk assessment", "Team coordination" }, 5);

        engine.RecordKnowledge("KNOW-002", "Market Analysis", 0.78,
            new List<string> { "Trend analysis", "Competitor research" }, 3);

        engine.RecordKnowledge("KNOW-003", "Financial Planning", 0.82,
            new List<string> { "Budgeting", "ROI calculation" }, 7);

        Console.WriteLine("  ✓ Recorded 3 knowledge factors");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Recording Memory Factors]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordMemory("MEM-001", "Previous project success with similar scope", 0.90, "Positive outcome", 0.95);
        engine.RecordMemory("MEM-002", "Failed initiative due to poor planning", 0.85, "Negative outcome", 0.90);
        engine.RecordMemory("MEM-003", "Team collaboration on complex project", 0.88, "Positive experience", 0.92);

        Console.WriteLine("  ✓ Recorded 3 memory factors");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Calculating Outcome Scores]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CalculateOutcome("CALC-001", 0.30, 0.50, 0.20);

        Console.WriteLine("  ✓ Calculated weighted outcome");
        engine.DisplayCalculation("CALC-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Generating Predictions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.GeneratePrediction("PRED-001", "Should we proceed with the new product launch?");

        Console.WriteLine("  ✓ Generated prediction");
        engine.DisplayPrediction("PRED-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Multi-Factor Decision Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Decision-Making Architecture:");
        Console.WriteLine("    Layer 1: Intuition Recording (capture gut feelings)");
        Console.WriteLine("    Layer 2: Knowledge Assessment (evaluate expertise)");
        Console.WriteLine("    Layer 3: Memory Integration (recall relevant experiences)");
        Console.WriteLine("    Layer 4: Weight Assignment (prioritize factors)");
        Console.WriteLine("    Layer 5: Score Calculation (compute composite score)");
        Console.WriteLine("    Layer 6: Outcome Categorization (classify result)");
        Console.WriteLine("    Layer 7: Recommendation Generation (suggest action)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Record and measure intuitive guidance");
        Console.WriteLine("    ✓ Assess knowledge depth and relevance");
        Console.WriteLine("    ✓ Integrate relevant past experiences");
        Console.WriteLine("    ✓ Assign customizable weights to factors");
        Console.WriteLine("    ✓ Calculate composite outcome scores");
        Console.WriteLine("    ✓ Generate decision recommendations");
        Console.WriteLine("    ✓ Support multi-factor decision making");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Outcome prediction system complete");
        Console.ResetColor();
    }
}
