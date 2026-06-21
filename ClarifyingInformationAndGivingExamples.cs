using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ClarifyingInformationAndGivingExamples
{
    public class Concept
    {
        public string ConceptId { get; set; }
        public string ConceptName { get; set; }
        public string CoreDefinition { get; set; }
        public List<string> KeyComponents { get; set; }
        public List<string> RelatedConcepts { get; set; }
        public double ComplexityLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Explanation
    {
        public string ExplanationId { get; set; }
        public string ConceptId { get; set; }
        public string ExplanationType { get; set; }
        public string ExplanationText { get; set; }
        public List<string> KeyPoints { get; set; }
        public int ClarityScore { get; set; }
        public int ExamplesIncluded { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Example
    {
        public string ExampleId { get; set; }
        public string ConceptId { get; set; }
        public string ExampleDescription { get; set; }
        public string ExampleType { get; set; }
        public List<string> StepByStepBreakdown { get; set; }
        public double RelatabilityScore { get; set; }
        public string DifficultyLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClarificationFramework
    {
        public string FrameworkId { get; set; }
        public string ConceptId { get; set; }
        public List<string> ExplanationIds { get; set; }
        public List<string> ExampleIds { get; set; }
        public double OverallClarityScore { get; set; }
        public List<string> ClarificationLayers { get; set; }
        public string ComprehensionPath { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClarificationEngine
    {
        private Dictionary<string, Concept> concepts;
        private Dictionary<string, Explanation> explanations;
        private Dictionary<string, Example> examples;
        private Dictionary<string, ClarificationFramework> frameworks;

        public ClarificationEngine()
        {
            concepts = new Dictionary<string, Concept>();
            explanations = new Dictionary<string, Explanation>();
            examples = new Dictionary<string, Example>();
            frameworks = new Dictionary<string, ClarificationFramework>();
        }

        public void RegisterConcept(string conceptId, string conceptName, string definition,
                                   List<string> components, double complexity)
        {
            var concept = new Concept
            {
                ConceptId = conceptId,
                ConceptName = conceptName,
                CoreDefinition = definition,
                KeyComponents = new List<string>(components),
                RelatedConcepts = new List<string>(),
                ComplexityLevel = complexity,
                CreatedDate = DateTime.Now
            };
            concepts[conceptId] = concept;
        }

        public void LinkRelatedConcepts(string conceptId1, string conceptId2)
        {
            if (concepts.ContainsKey(conceptId1) && concepts.ContainsKey(conceptId2))
            {
                if (!concepts[conceptId1].RelatedConcepts.Contains(conceptId2))
                {
                    concepts[conceptId1].RelatedConcepts.Add(conceptId2);
                }
            }
        }

        public void CreateExplanation(string explanationId, string conceptId, string explanationType,
                                     string explanationText, List<string> keyPoints)
        {
            if (!concepts.ContainsKey(conceptId)) return;

            var explanation = new Explanation
            {
                ExplanationId = explanationId,
                ConceptId = conceptId,
                ExplanationType = explanationType,
                ExplanationText = explanationText,
                KeyPoints = new List<string>(keyPoints),
                ClarityScore = CalculateClarityScore(explanationText, keyPoints),
                ExamplesIncluded = 0,
                CreatedDate = DateTime.Now
            };
            explanations[explanationId] = explanation;
        }

        private int CalculateClarityScore(string text, List<string> points)
        {
            int score = 50;
            if (text.Length > 100 && text.Length < 500) score += 20;
            if (points.Count >= 3) score += 15;
            if (text.Contains("therefore") || text.Contains("because")) score += 10;
            return Math.Min(score, 100);
        }

        public void CreateExample(string exampleId, string conceptId, string description,
                                 string exampleType, List<string> breakdown, double relatability)
        {
            if (!concepts.ContainsKey(conceptId)) return;

            var example = new Example
            {
                ExampleId = exampleId,
                ConceptId = conceptId,
                ExampleDescription = description,
                ExampleType = exampleType,
                StepByStepBreakdown = new List<string>(breakdown),
                RelatabilityScore = relatability,
                DifficultyLevel = GetDifficultyLevel(breakdown.Count, relatability),
                CreatedDate = DateTime.Now
            };
            examples[exampleId] = example;

            if (explanations.Values.FirstOrDefault(e => e.ConceptId == conceptId) != null)
            {
                explanations.Values.First(e => e.ConceptId == conceptId).ExamplesIncluded++;
            }
        }

        private string GetDifficultyLevel(int stepCount, double relatability)
        {
            if (relatability > 0.8 && stepCount <= 3)
                return "Beginner-Friendly";
            if (relatability > 0.6 && stepCount <= 5)
                return "Intermediate";
            if (stepCount > 5 || relatability < 0.5)
                return "Advanced";
            return "Moderate";
        }

        public void BuildClarificationFramework(string frameworkId, string conceptId)
        {
            if (!concepts.ContainsKey(conceptId)) return;

            var framework = new ClarificationFramework
            {
                FrameworkId = frameworkId,
                ConceptId = conceptId,
                ExplanationIds = new List<string>(),
                ExampleIds = new List<string>(),
                OverallClarityScore = 0.0,
                ClarificationLayers = new List<string>(),
                ComprehensionPath = "",
                CreatedDate = DateTime.Now
            };

            foreach (var explanation in explanations.Values)
            {
                if (explanation.ConceptId == conceptId)
                {
                    framework.ExplanationIds.Add(explanation.ExplanationId);
                    framework.ClarificationLayers.Add($"Explanation: {explanation.ExplanationType}");
                }
            }

            foreach (var example in examples.Values)
            {
                if (example.ConceptId == conceptId)
                {
                    framework.ExampleIds.Add(example.ExampleId);
                    framework.ClarificationLayers.Add($"Example: {example.ExampleType}");
                }
            }

            double totalClarity = 0.0;
            int clarityCount = 0;

            foreach (var expId in framework.ExplanationIds)
            {
                if (explanations.ContainsKey(expId))
                {
                    totalClarity += explanations[expId].ClarityScore / 100.0;
                    clarityCount++;
                }
            }

            foreach (var exId in framework.ExampleIds)
            {
                if (examples.ContainsKey(exId))
                {
                    totalClarity += examples[exId].RelatabilityScore;
                    clarityCount++;
                }
            }

            framework.OverallClarityScore = clarityCount > 0 ? totalClarity / clarityCount : 0.0;
            framework.ComprehensionPath = GenerateComprehensionPath(framework);

            frameworks[frameworkId] = framework;
        }

        private string GenerateComprehensionPath(ClarificationFramework framework)
        {
            var path = new List<string>();
            path.Add("1. Core Definition");
            if (framework.ExplanationIds.Count > 0)
                path.Add("2. Detailed Explanations");
            if (framework.ExampleIds.Count > 0)
                path.Add("3. Practical Examples");
            path.Add("4. Application and Mastery");
            return string.Join(" → ", path);
        }

        public void DisplayConcept(string conceptId)
        {
            if (!concepts.ContainsKey(conceptId)) return;

            var concept = concepts[conceptId];
            Console.WriteLine($"\n  Concept: {concept.ConceptName}");
            Console.WriteLine($"  ID: {concept.ConceptId}");
            Console.WriteLine($"  Definition: {concept.CoreDefinition}");
            Console.WriteLine($"  Components: {string.Join(", ", concept.KeyComponents)}");
            Console.WriteLine($"  Complexity: {concept.ComplexityLevel:F2}/10");
        }

        public void DisplayExplanation(string explanationId)
        {
            if (!explanations.ContainsKey(explanationId)) return;

            var explanation = explanations[explanationId];
            Console.WriteLine($"\n  Explanation: {explanation.ExplanationId}");
            Console.WriteLine($"  Type: {explanation.ExplanationType}");
            Console.WriteLine($"  Clarity Score: {explanation.ClarityScore}/100");
            Console.WriteLine($"  Key Points:");
            foreach (var point in explanation.KeyPoints)
            {
                Console.WriteLine($"    • {point}");
            }
            Console.WriteLine($"  Examples Linked: {explanation.ExamplesIncluded}");
        }

        public void DisplayExample(string exampleId)
        {
            if (!examples.ContainsKey(exampleId)) return;

            var example = examples[exampleId];
            Console.WriteLine($"\n  Example: {example.ExampleId}");
            Console.WriteLine($"  Type: {example.ExampleType}");
            Console.WriteLine($"  Description: {example.ExampleDescription}");
            Console.WriteLine($"  Difficulty: {example.DifficultyLevel}");
            Console.WriteLine($"  Relatability: {example.RelatabilityScore * 100:F1}%");
            Console.WriteLine($"  Steps:");
            for (int i = 0; i < example.StepByStepBreakdown.Count; i++)
            {
                Console.WriteLine($"    {i + 1}. {example.StepByStepBreakdown[i]}");
            }
        }

        public void DisplayFramework(string frameworkId)
        {
            if (!frameworks.ContainsKey(frameworkId)) return;

            var framework = frameworks[frameworkId];
            Console.WriteLine($"\n  Clarification Framework: {framework.FrameworkId}");
            Console.WriteLine($"  Concept: {framework.ConceptId}");
            Console.WriteLine($"  Explanations: {framework.ExplanationIds.Count}");
            Console.WriteLine($"  Examples: {framework.ExampleIds.Count}");
            Console.WriteLine($"  Overall Clarity: {framework.OverallClarityScore * 100:F1}%");
            Console.WriteLine($"  Comprehension Path: {framework.ComprehensionPath}");
            Console.WriteLine($"  Clarification Layers: {framework.ClarificationLayers.Count}");
        }

        public int GetTotalConcepts()
        {
            return concepts.Count;
        }

        public int GetTotalExplanations()
        {
            return explanations.Count;
        }

        public int GetTotalExamples()
        {
            return examples.Count;
        }

        public double GetAverageClarityScore()
        {
            return explanations.Count > 0 ? explanations.Values.Average(e => e.ClarityScore / 100.0) : 0.0;
        }

        public List<(string, double)> GetConceptsByClarityLevel()
        {
            var conceptClarity = new Dictionary<string, double>();
            foreach (var framework in frameworks.Values)
            {
                conceptClarity[framework.ConceptId] = framework.OverallClarityScore;
            }
            return conceptClarity.OrderByDescending(x => x.Value)
                .Select(x => (x.Key, x.Value))
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Clarifying Information and Giving Examples               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new ClarificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Concepts to Clarify]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterConcept("CONCEPT-001", "Photosynthesis",
            "Process by which plants convert light energy into chemical energy",
            new List<string> { "Light absorption", "Chemical reaction", "Oxygen production", "Energy conversion" },
            7.5);

        engine.RegisterConcept("CONCEPT-002", "Machine Learning",
            "Computational systems that learn from data without explicit programming",
            new List<string> { "Data processing", "Pattern recognition", "Algorithm", "Prediction" },
            8.0);

        engine.RegisterConcept("CONCEPT-003", "Recursion",
            "A function or process that calls itself as a part of its solution",
            new List<string> { "Self-reference", "Base case", "Recursive case", "Stack" },
            7.0);

        Console.WriteLine("  ✓ Registered 3 complex concepts");
        engine.DisplayConcept("CONCEPT-001");
        engine.DisplayConcept("CONCEPT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Linking Related Concepts]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkRelatedConcepts("CONCEPT-002", "CONCEPT-003");
        engine.LinkRelatedConcepts("CONCEPT-001", "CONCEPT-002");

        Console.WriteLine("  ✓ Linked related concepts");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Explanations]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateExplanation("EXP-001", "CONCEPT-001", "Simple Overview",
            "Photosynthesis is the process where plants take sunlight, water, and carbon dioxide, and convert them into glucose (sugar) and oxygen. This happens primarily in the leaves of plants because they are exposed to sunlight.",
            new List<string> { "Requires sunlight", "Uses water and CO2", "Produces glucose", "Releases oxygen" });

        engine.CreateExplanation("EXP-002", "CONCEPT-001", "Detailed Scientific",
            "Photosynthesis involves two main stages: light-dependent reactions occurring in the thylakoid membranes where photons excite electrons, and the Calvin cycle in the stroma where ATP and NADPH are used to fix carbon dioxide into glucose molecules.",
            new List<string> { "Light reactions", "Calvin cycle", "Electron transport", "ATP synthesis" });

        engine.CreateExplanation("EXP-003", "CONCEPT-002", "Conceptual Overview",
            "Machine learning systems identify patterns in data and use those patterns to make predictions on new data. Unlike traditional programming where we write explicit rules, machine learning algorithms learn the rules from examples.",
            new List<string> { "Pattern recognition", "Learning from data", "Generalizes to new data" });

        engine.CreateExplanation("EXP-004", "CONCEPT-003", "Programming Context",
            "Recursion is when a function calls itself. Every recursive solution needs a base case (where it stops) and a recursive case (where it calls itself with a simpler problem). This breaks complex problems into smaller identical subproblems.",
            new List<string> { "Base case essential", "Simpler subproblems", "Function calls itself" });

        Console.WriteLine("  ✓ Created 4 explanations");
        engine.DisplayExplanation("EXP-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Creating Concrete Examples]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateExample("EX-001", "CONCEPT-001", "Plant leaf in sunlight",
            "Real-world example",
            new List<string> {
                "Sunlight hits a green leaf on a plant",
                "Leaf absorbs light energy through chlorophyll",
                "Plant roots take up water from soil",
                "Leaf takes CO2 from air through stomata",
                "Chemical reactions convert these into glucose",
                "Glucose used for plant growth and energy",
                "Oxygen released back into air"
            }, 0.95);

        engine.CreateExample("EX-002", "CONCEPT-002", "Email spam filtering",
            "Practical application",
            new List<string> {
                "System shown thousands of emails labeled spam/not-spam",
                "Algorithm finds patterns in spam emails (keywords, sender patterns)",
                "Algorithm learns what makes an email likely spam",
                "New incoming email analyzed against learned patterns",
                "System predicts if new email is spam with high accuracy"
            }, 0.90);

        engine.CreateExample("EX-003", "CONCEPT-003", "Factorial calculation",
            "Code example",
            new List<string> {
                "factorial(5) calls factorial(4)",
                "factorial(4) calls factorial(3)",
                "... continues until factorial(1)",
                "Base case: factorial(1) returns 1",
                "Then 2 × 1 = 2",
                "Then 3 × 2 = 6",
                "Then 4 × 6 = 24",
                "Then 5 × 24 = 120"
            }, 0.85);

        Console.WriteLine("  ✓ Created 3 detailed examples");
        engine.DisplayExample("EX-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Building Clarification Frameworks]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.BuildClarificationFramework("FRAME-001", "CONCEPT-001");
        engine.BuildClarificationFramework("FRAME-002", "CONCEPT-002");
        engine.BuildClarificationFramework("FRAME-003", "CONCEPT-003");

        Console.WriteLine("  ✓ Built 3 clarification frameworks");
        engine.DisplayFramework("FRAME-001");
        engine.DisplayFramework("FRAME-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Clarity Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var conceptClarity = engine.GetConceptsByClarityLevel();
        Console.WriteLine("  Concepts by Clarity Level:");
        foreach (var (conceptId, clarity) in conceptClarity)
        {
            Console.WriteLine($"    {conceptId}: {clarity * 100:F1}% clarity");
        }

        Console.WriteLine($"\n  Average Explanation Clarity: {engine.GetAverageClarityScore() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Clarification System Summary:");
        Console.WriteLine($"    Total Concepts: {engine.GetTotalConcepts()}");
        Console.WriteLine($"    Total Explanations: {engine.GetTotalExplanations()}");
        Console.WriteLine($"    Total Examples: {engine.GetTotalExamples()}");
        Console.WriteLine($"    Frameworks Built: {engine.GetTotalConcepts()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 8: Clarification Framework Architecture]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Multi-Layer Clarification Architecture:");
        Console.WriteLine("    Layer 1: Concept Definition (establish core understanding)");
        Console.WriteLine("    Layer 2: Component Identification (identify key parts)");
        Console.WriteLine("    Layer 3: Simple Explanation (beginner-friendly overview)");
        Console.WriteLine("    Layer 4: Detailed Explanation (in-depth technical analysis)");
        Console.WriteLine("    Layer 5: Real-world Examples (practical applications)");
        Console.WriteLine("    Layer 6: Code/Procedural Examples (step-by-step breakdown)");
        Console.WriteLine("    Layer 7: Example Relatability (connect to familiar concepts)");
        Console.WriteLine("    Layer 8: Comprehension Path (guide learner progression)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define complex concepts with multiple components");
        Console.WriteLine("    ✓ Create layered explanations from simple to advanced");
        Console.WriteLine("    ✓ Provide concrete real-world examples");
        Console.WriteLine("    ✓ Include step-by-step procedural breakdowns");
        Console.WriteLine("    ✓ Measure clarity and comprehensibility scores");
        Console.WriteLine("    ✓ Build integrated clarification frameworks");
        Console.WriteLine("    ✓ Guide structured learning paths");
        Console.WriteLine("    ✓ Link related concepts for deeper understanding");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Clarification and examples system complete");
        Console.ResetColor();
    }
}
