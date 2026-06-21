using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class TheCapabilityToConversateAndOrArgueAndOrDebateWithOrWithoutReasoning
{
    public class Argument
    {
        public string ArgumentId { get; set; }
        public string Claim { get; set; }
        public List<string> Evidence { get; set; }
        public string LogicalStructure { get; set; }
        public double ArgumentStrength { get; set; }
        public bool HasReasoning { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CounterArgument
    {
        public string CounterId { get; set; }
        public string OriginalArgumentId { get; set; }
        public string RebuttalClaim { get; set; }
        public List<string> RefutingEvidence { get; set; }
        public double EffectivenessScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DebateParticipant
    {
        public string ParticipantId { get; set; }
        public string ParticipantName { get; set; }
        public string Position { get; set; }
        public List<string> ArgumentIds { get; set; }
        public double DebateScore { get; set; }
        public double ReasoningCapability { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ConversationTurn
    {
        public string TurnId { get; set; }
        public string ParticipantId { get; set; }
        public string Statement { get; set; }
        public string TurnType { get; set; }
        public bool UsesReasoning { get; set; }
        public List<string> LogicalFallacies { get; set; }
        public double CoherentScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class DebateAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalArguments { get; set; }
        public int TotalCounterArguments { get; set; }
        public int TotalTurns { get; set; }
        public Dictionary<string, int> ArgumentTypes { get; set; }
        public double AverageArgumentStrength { get; set; }
        public double AverageReasoningUsage { get; set; }
        public string DominantParticipant { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class DebateEngine
    {
        private Dictionary<string, Argument> arguments;
        private Dictionary<string, CounterArgument> counterArguments;
        private Dictionary<string, DebateParticipant> participants;
        private Dictionary<string, ConversationTurn> turns;
        private Dictionary<string, DebateAnalysis> analyses;

        public DebateEngine()
        {
            arguments = new Dictionary<string, Argument>();
            counterArguments = new Dictionary<string, CounterArgument>();
            participants = new Dictionary<string, DebateParticipant>();
            turns = new Dictionary<string, ConversationTurn>();
            analyses = new Dictionary<string, DebateAnalysis>();
        }

        public void RegisterParticipant(string participantId, string name, string position, double reasoning)
        {
            var participant = new DebateParticipant
            {
                ParticipantId = participantId,
                ParticipantName = name,
                Position = position,
                ArgumentIds = new List<string>(),
                DebateScore = 0.0,
                ReasoningCapability = reasoning,
                CreatedDate = DateTime.Now
            };
            participants[participantId] = participant;
        }

        public void PresentArgument(string argumentId, string claim, List<string> evidence,
                                   string logicalStructure, bool hasReasoning)
        {
            var argument = new Argument
            {
                ArgumentId = argumentId,
                Claim = claim,
                Evidence = new List<string>(evidence),
                LogicalStructure = logicalStructure,
                ArgumentStrength = CalculateArgumentStrength(evidence, logicalStructure, hasReasoning),
                HasReasoning = hasReasoning,
                CreatedDate = DateTime.Now
            };
            arguments[argumentId] = argument;
        }

        private double CalculateArgumentStrength(List<string> evidence, string structure, bool reasoning)
        {
            double strength = 0.5;
            strength += evidence.Count * 0.1;
            if (structure != "") strength += 0.2;
            if (reasoning) strength += 0.15;
            return Math.Min(strength, 1.0);
        }

        public void PresentCounterArgument(string counterId, string originalArgId, string rebuttal,
                                          List<string> refuting)
        {
            if (!arguments.ContainsKey(originalArgId)) return;

            var counter = new CounterArgument
            {
                CounterId = counterId,
                OriginalArgumentId = originalArgId,
                RebuttalClaim = rebuttal,
                RefutingEvidence = new List<string>(refuting),
                EffectivenessScore = CalculateEffectiveness(refuting, arguments[originalArgId]),
                CreatedDate = DateTime.Now
            };
            counterArguments[counterId] = counter;
        }

        private double CalculateEffectiveness(List<string> refuting, Argument original)
        {
            double effectiveness = 0.5;
            effectiveness += refuting.Count * 0.15;
            if (refuting.Count >= original.Evidence.Count) effectiveness += 0.2;
            return Math.Min(effectiveness, 1.0);
        }

        public void RecordConversationTurn(string turnId, string participantId, string statement,
                                         string turnType, bool usesReasoning)
        {
            if (!participants.ContainsKey(participantId)) return;

            var turn = new ConversationTurn
            {
                TurnId = turnId,
                ParticipantId = participantId,
                Statement = statement,
                TurnType = turnType,
                UsesReasoning = usesReasoning,
                LogicalFallacies = DetectFallacies(statement),
                CoherentScore = CalculateCoherence(statement, usesReasoning),
                CreatedDate = DateTime.Now
            };

            turns[turnId] = turn;

            if (participants[participantId].DebateScore < turn.CoherentScore)
                participants[participantId].DebateScore = turn.CoherentScore;
        }

        private List<string> DetectFallacies(string statement)
        {
            var fallacies = new List<string>();

            if (statement.Contains("everyone") || statement.Contains("always"))
                fallacies.Add("Overgeneralization");

            if (statement.Contains("obviously") || statement.Contains("clearly"))
                fallacies.Add("Begging the question");

            if (statement.Contains("either") && statement.Contains("or"))
                fallacies.Add("False dilemma");

            return fallacies;
        }

        private double CalculateCoherence(string statement, bool reasoning)
        {
            double coherence = 0.6;
            if (statement.Length > 50) coherence += 0.15;
            if (reasoning) coherence += 0.2;
            return Math.Min(coherence, 1.0);
        }

        public void AnalyzeDebate(string analysisId)
        {
            var analysis = new DebateAnalysis
            {
                AnalysisId = analysisId,
                TotalArguments = arguments.Count,
                TotalCounterArguments = counterArguments.Count,
                TotalTurns = turns.Count,
                ArgumentTypes = new Dictionary<string, int>(),
                AverageArgumentStrength = arguments.Count > 0 ? arguments.Values.Average(a => a.ArgumentStrength) : 0.0,
                AverageReasoningUsage = turns.Count > 0 ? (double)turns.Values.Count(t => t.UsesReasoning) / turns.Count : 0.0,
                DominantParticipant = GetDominantParticipant(),
                AnalyzedDate = DateTime.Now
            };

            foreach (var turn in turns.Values)
            {
                if (!analysis.ArgumentTypes.ContainsKey(turn.TurnType))
                    analysis.ArgumentTypes[turn.TurnType] = 0;
                analysis.ArgumentTypes[turn.TurnType]++;
            }

            analyses[analysisId] = analysis;
        }

        private string GetDominantParticipant()
        {
            if (participants.Count == 0) return "None";
            return participants.Values.OrderByDescending(p => p.DebateScore).First().ParticipantName;
        }

        public void DisplayArgument(string argumentId)
        {
            if (!arguments.ContainsKey(argumentId)) return;

            var arg = arguments[argumentId];
            Console.WriteLine($"\n  Argument: {arg.ArgumentId}");
            Console.WriteLine($"  Claim: {arg.Claim}");
            Console.WriteLine($"  Evidence Count: {arg.Evidence.Count}");
            Console.WriteLine($"  Uses Reasoning: {(arg.HasReasoning ? "Yes" : "No")}");
            Console.WriteLine($"  Argument Strength: {arg.ArgumentStrength * 100:F0}%");
        }

        public void DisplayParticipant(string participantId)
        {
            if (!participants.ContainsKey(participantId)) return;

            var participant = participants[participantId];
            Console.WriteLine($"\n  Participant: {participant.ParticipantName}");
            Console.WriteLine($"  Position: {participant.Position}");
            Console.WriteLine($"  Debate Score: {participant.DebateScore * 100:F0}%");
            Console.WriteLine($"  Reasoning Capability: {participant.ReasoningCapability * 100:F0}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Debate Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Arguments: {analysis.TotalArguments}");
            Console.WriteLine($"  Counter Arguments: {analysis.TotalCounterArguments}");
            Console.WriteLine($"  Total Turns: {analysis.TotalTurns}");
            Console.WriteLine($"  Average Argument Strength: {analysis.AverageArgumentStrength * 100:F0}%");
            Console.WriteLine($"  Reasoning Usage: {analysis.AverageReasoningUsage * 100:F0}%");
            Console.WriteLine($"  Dominant Participant: {analysis.DominantParticipant}");
        }

        public int GetTotalArguments()
        {
            return arguments.Count;
        }

        public int GetTotalParticipants()
        {
            return participants.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Conversate, Argue, Debate With or Without Reasoning            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new DebateEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Debate Participants]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterParticipant("PART-001", "Logical Alice", "Pro-Development", 0.95);
        engine.RegisterParticipant("PART-002", "Cautious Bob", "Anti-Development", 0.85);
        engine.RegisterParticipant("PART-003", "Balanced Carol", "Nuanced Position", 0.90);

        Console.WriteLine("  ✓ Registered 3 debate participants");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Presenting Arguments]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.PresentArgument("ARG-001",
            "Technology improves productivity",
            new List<string> { "40% efficiency gain", "Cost reduction", "Better outcomes" },
            "Inductive", true);

        engine.PresentArgument("ARG-002",
            "Rapid development has risks",
            new List<string> { "Quality concerns", "Security issues", "Technical debt" },
            "Deductive", true);

        Console.WriteLine("  ✓ Presented 2 arguments with reasoning");
        engine.DisplayArgument("ARG-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Presenting Counter-Arguments]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.PresentCounterArgument("COUNTER-001", "ARG-001",
            "Productivity gains are offset by maintenance costs",
            new List<string> { "Long-term studies show 30% overhead", "Support costs rise" });

        engine.PresentCounterArgument("COUNTER-002", "ARG-002",
            "Quality control processes mitigate most risks",
            new List<string> { "Automated testing covers 95%", "Security audits prevent issues" });

        Console.WriteLine("  ✓ Presented 2 counter-arguments");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Recording Conversation Turns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordConversationTurn("TURN-001", "PART-001",
            "The evidence clearly demonstrates that technology drives innovation",
            "Assertion", true);

        engine.RecordConversationTurn("TURN-002", "PART-002",
            "However, we must consider the risks involved in rapid deployment",
            "Rebuttal", true);

        engine.RecordConversationTurn("TURN-003", "PART-003",
            "Both perspectives have merit. Balanced approach would include testing phases",
            "Synthesis", true);

        Console.WriteLine("  ✓ Recorded 3 conversation turns");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Displaying Participants]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayParticipant("PART-001");
        engine.DisplayParticipant("PART-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Debate Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeDebate("ANALYSIS-001");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Debate Engine Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Debate Engine Architecture:");
        Console.WriteLine("    Layer 1: Participant Registration (establish speakers)");
        Console.WriteLine("    Layer 2: Argument Presentation (state claims with evidence)");
        Console.WriteLine("    Layer 3: Logical Structure (define argumentation type)");
        Console.WriteLine("    Layer 4: Reasoning Assessment (evaluate logical support)");
        Console.WriteLine("    Layer 5: Counter-Arguments (present rebuttals)");
        Console.WriteLine("    Layer 6: Fallacy Detection (identify logical errors)");
        Console.WriteLine("    Layer 7: Debate Analysis (measure argument quality)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Support multiple debate participants");
        Console.WriteLine("    ✓ Evaluate argument strength with or without reasoning");
        Console.WriteLine("    ✓ Present and analyze counter-arguments");
        Console.WriteLine("    ✓ Detect logical fallacies in statements");
        Console.WriteLine("    ✓ Record and categorize conversation turns");
        Console.WriteLine("    ✓ Measure coherence and debate quality");
        Console.WriteLine("    ✓ Analyze debate patterns and dominant participants");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Debate and conversation system complete");
        Console.ResetColor();
    }
}
