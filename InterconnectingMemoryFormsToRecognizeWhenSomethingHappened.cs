using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class InterconnectingMemoryFormsToRecognizeWhenSomethingHappened
{
    public class EpisodicMemory
    {
        public string MemoryId { get; set; }
        public string Event { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public List<string> Participants { get; set; }
        public double VividnessLevel { get; set; }
        public DateTime EncodedDate { get; set; }
    }

    public class SemanticMemory
    {
        public string KnowledgeId { get; set; }
        public string Concept { get; set; }
        public string Definition { get; set; }
        public List<string> RelatedConcepts { get; set; }
        public double RelevanceToEvent { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ProceduralMemory
    {
        public string ProcedureId { get; set; }
        public string SkillName { get; set; }
        public List<string> Steps { get; set; }
        public double ProficiencyLevel { get; set; }
        public DateTime LastPerformedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TemporalConnection
    {
        public string ConnectionId { get; set; }
        public string EpisodicMemoryId { get; set; }
        public List<string> ConnectedSemanticIds { get; set; }
        public List<string> ConnectedProceduralIds { get; set; }
        public DateTime EventTime { get; set; }
        public string TimelinePosition { get; set; }
        public double ConnectionStrength { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TemporalMemoryAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalEvents { get; set; }
        public List<(DateTime, string)> EventTimeline { get; set; }
        public Dictionary<string, int> ConceptFrequency { get; set; }
        public double AverageConnectionStrength { get; set; }
        public string TimelinePattern { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class TemporalMemoryEngine
    {
        private Dictionary<string, EpisodicMemory> episodic;
        private Dictionary<string, SemanticMemory> semantic;
        private Dictionary<string, ProceduralMemory> procedural;
        private Dictionary<string, TemporalConnection> connections;
        private Dictionary<string, TemporalMemoryAnalysis> analyses;

        public TemporalMemoryEngine()
        {
            episodic = new Dictionary<string, EpisodicMemory>();
            semantic = new Dictionary<string, SemanticMemory>();
            procedural = new Dictionary<string, ProceduralMemory>();
            connections = new Dictionary<string, TemporalConnection>();
            analyses = new Dictionary<string, TemporalMemoryAnalysis>();
        }

        public void StoreEpisodicMemory(string memoryId, string eventDescription, DateTime eventDate,
                                      string location, List<string> participants, double vividness)
        {
            var memory = new EpisodicMemory
            {
                MemoryId = memoryId,
                Event = eventDescription,
                EventDate = eventDate,
                Location = location,
                Participants = new List<string>(participants),
                VividnessLevel = vividness,
                EncodedDate = DateTime.Now
            };
            episodic[memoryId] = memory;
        }

        public void StoreSemanticMemory(string knowledgeId, string concept, string definition,
                                       List<string> relatedConcepts, double relevance)
        {
            var memory = new SemanticMemory
            {
                KnowledgeId = knowledgeId,
                Concept = concept,
                Definition = definition,
                RelatedConcepts = new List<string>(relatedConcepts),
                RelevanceToEvent = relevance,
                CreatedDate = DateTime.Now
            };
            semantic[knowledgeId] = memory;
        }

        public void StoreProceduralMemory(string procedureId, string skillName, List<string> steps, double proficiency)
        {
            var memory = new ProceduralMemory
            {
                ProcedureId = procedureId,
                SkillName = skillName,
                Steps = new List<string>(steps),
                ProficiencyLevel = proficiency,
                LastPerformedDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            procedural[procedureId] = memory;
        }

        public void ConnectMemories(string connectionId, string episodicId, List<string> semanticIds,
                                   List<string> proceduralIds)
        {
            if (!episodic.ContainsKey(episodicId)) return;

            var episodicEvent = episodic[episodicId];
            var connection = new TemporalConnection
            {
                ConnectionId = connectionId,
                EpisodicMemoryId = episodicId,
                ConnectedSemanticIds = new List<string>(semanticIds),
                ConnectedProceduralIds = new List<string>(proceduralIds),
                EventTime = episodicEvent.EventDate,
                TimelinePosition = CalculateTimelinePosition(episodicEvent.EventDate),
                ConnectionStrength = 0.0,
                CreatedDate = DateTime.Now
            };

            double semanticRelevance = semanticIds.Count > 0 ?
                semantic.Where(s => semanticIds.Contains(s.Key))
                       .Average(s => s.Value.RelevanceToEvent) : 0.0;

            double proceduralRelevance = proceduralIds.Count > 0 ?
                procedural.Where(p => proceduralIds.Contains(p.Key))
                         .Average(p => p.Value.ProficiencyLevel) : 0.0;

            connection.ConnectionStrength = (semanticRelevance + proceduralRelevance) / 2;

            connections[connectionId] = connection;
        }

        private string CalculateTimelinePosition(DateTime eventDate)
        {
            TimeSpan span = DateTime.Now - eventDate;
            if (span.TotalSeconds < 60) return "Just now";
            if (span.TotalMinutes < 60) return "Within the hour";
            if (span.TotalHours < 24) return "Today";
            if (span.TotalDays < 7) return "This week";
            if (span.TotalDays < 30) return "This month";
            if (span.TotalDays < 365) return "This year";
            return "Earlier";
        }

        public void AnalyzeTemporalPatterns(string analysisId)
        {
            var analysis = new TemporalMemoryAnalysis
            {
                AnalysisId = analysisId,
                TotalEvents = episodic.Count,
                EventTimeline = new List<(DateTime, string)>(),
                ConceptFrequency = new Dictionary<string, int>(),
                AverageConnectionStrength = 0.0,
                TimelinePattern = "",
                AnalyzedDate = DateTime.Now
            };

            foreach (var episodicEvent in episodic.Values.OrderBy(e => e.EventDate))
            {
                analysis.EventTimeline.Add((episodicEvent.EventDate, episodicEvent.Event));
            }

            foreach (var semanticItem in semantic.Values)
            {
                if (!analysis.ConceptFrequency.ContainsKey(semanticItem.Concept))
                    analysis.ConceptFrequency[semanticItem.Concept] = 0;
                analysis.ConceptFrequency[semanticItem.Concept]++;
            }

            analysis.AverageConnectionStrength = connections.Count > 0 ?
                connections.Values.Average(c => c.ConnectionStrength) : 0.0;

            analysis.TimelinePattern = DetermineTimelinePattern(episodic.Values.Select(e => e.EventDate).ToList());

            analyses[analysisId] = analysis;
        }

        private string DetermineTimelinePattern(List<DateTime> dates)
        {
            if (dates.Count < 2) return "Insufficient data";

            var sortedDates = dates.OrderBy(d => d).ToList();
            TimeSpan avgSpan = TimeSpan.Zero;

            for (int i = 1; i < sortedDates.Count; i++)
            {
                avgSpan += sortedDates[i] - sortedDates[i - 1];
            }

            avgSpan = TimeSpan.FromSeconds(avgSpan.TotalSeconds / (sortedDates.Count - 1));

            if (avgSpan.TotalDays < 1) return "Frequent events (less than 1 day apart)";
            if (avgSpan.TotalDays < 7) return "Regular weekly events";
            if (avgSpan.TotalDays < 30) return "Monthly pattern";
            return "Sparse events";
        }

        public void DisplayEpisodicMemory(string memoryId)
        {
            if (!episodic.ContainsKey(memoryId)) return;

            var memory = episodic[memoryId];
            Console.WriteLine($"\n  Episodic Memory: {memory.MemoryId}");
            Console.WriteLine($"  Event: {memory.Event}");
            Console.WriteLine($"  Date: {memory.EventDate:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Location: {memory.Location}");
            Console.WriteLine($"  Vividness: {memory.VividnessLevel * 100:F0}%");
            Console.WriteLine($"  Participants: {string.Join(", ", memory.Participants)}");
        }

        public void DisplayConnection(string connectionId)
        {
            if (!connections.ContainsKey(connectionId)) return;

            var connection = connections[connectionId];
            Console.WriteLine($"\n  Temporal Connection: {connection.ConnectionId}");
            Console.WriteLine($"  Event Time: {connection.EventTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  Timeline Position: {connection.TimelinePosition}");
            Console.WriteLine($"  Connected Semantic Concepts: {connection.ConnectedSemanticIds.Count}");
            Console.WriteLine($"  Connected Procedures: {connection.ConnectedProceduralIds.Count}");
            Console.WriteLine($"  Connection Strength: {connection.ConnectionStrength * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Temporal Memory Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Events: {analysis.TotalEvents}");
            Console.WriteLine($"  Average Connection Strength: {analysis.AverageConnectionStrength * 100:F1}%");
            Console.WriteLine($"  Timeline Pattern: {analysis.TimelinePattern}");
            Console.WriteLine($"  Distinct Concepts: {analysis.ConceptFrequency.Count}");
        }

        public int GetTotalEpisodic()
        {
            return episodic.Count;
        }

        public int GetTotalConnections()
        {
            return connections.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║ Interconnecting Memory Forms to Recognize When Something Happened║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new TemporalMemoryEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Storing Episodic Memories]");
        Console.ResetColor();
        Thread.Sleep(500);

        var threeDaysAgo = DateTime.Now.AddDays(-3);
        var oneWeekAgo = DateTime.Now.AddDays(-7);

        engine.StoreEpisodicMemory("EP-001", "Team meeting about project launch",
            threeDaysAgo, "Conference Room A", new List<string> { "Alice", "Bob", "Charlie" }, 0.9);

        engine.StoreEpisodicMemory("EP-002", "Completed first code review",
            oneWeekAgo, "Office", new List<string> { "Developer", "Reviewer" }, 0.85);

        Console.WriteLine("  ✓ Stored 2 episodic memories");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Storing Semantic Memories]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.StoreSemanticMemory("SEM-001", "Project Management",
            "Process of planning and executing project tasks",
            new List<string> { "Scheduling", "Resource allocation", "Risk management" }, 0.85);

        engine.StoreSemanticMemory("SEM-002", "Code Review Process",
            "Quality assurance through peer examination",
            new List<string> { "Standards", "Best practices", "Quality gates" }, 0.80);

        engine.StoreSemanticMemory("SEM-003", "Team Collaboration",
            "Working together toward common goals",
            new List<string> { "Communication", "Coordination", "Shared vision" }, 0.75);

        Console.WriteLine("  ✓ Stored 3 semantic memories");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Storing Procedural Memories]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.StoreProceduralMemory("PROC-001", "Code Review",
            new List<string> { "Read code", "Check standards", "Provide feedback", "Approve" }, 0.90);

        engine.StoreProceduralMemory("PROC-002", "Meeting Facilitation",
            new List<string> { "Set agenda", "Introduce topic", "Facilitate discussion", "Summarize" }, 0.75);

        Console.WriteLine("  ✓ Stored 2 procedural memories");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Connecting Memory Forms Temporally]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ConnectMemories("CONN-001", "EP-001",
            new List<string> { "SEM-001", "SEM-003" },
            new List<string> { "PROC-002" });

        engine.ConnectMemories("CONN-002", "EP-002",
            new List<string> { "SEM-002" },
            new List<string> { "PROC-001" });

        Console.WriteLine("  ✓ Connected episodic, semantic, and procedural memories");
        engine.DisplayConnection("CONN-001");
        engine.DisplayConnection("CONN-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Temporal Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeTemporalPatterns("ANALYSIS-001");

        Console.WriteLine("  ✓ Analyzed temporal memory patterns");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Temporal Memory Summary:");
        Console.WriteLine($"    Episodic Events: {engine.GetTotalEpisodic()}");
        Console.WriteLine($"    Memory Connections: {engine.GetTotalConnections()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Temporal Memory Integration Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Memory Integration Architecture:");
        Console.WriteLine("    Layer 1: Episodic Storage (encode personal events)");
        Console.WriteLine("    Layer 2: Semantic Storage (store knowledge)");
        Console.WriteLine("    Layer 3: Procedural Storage (record skills)");
        Console.WriteLine("    Layer 4: Temporal Indexing (assign timeline position)");
        Console.WriteLine("    Layer 5: Cross-Memory Connection (link memory forms)");
        Console.WriteLine("    Layer 6: Relevance Calculation (measure connection strength)");
        Console.WriteLine("    Layer 7: Pattern Analysis (identify temporal patterns)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Store and organize episodic memories");
        Console.WriteLine("    ✓ Maintain semantic knowledge base");
        Console.WriteLine("    ✓ Track procedural skill memories");
        Console.WriteLine("    ✓ Connect memory forms temporally");
        Console.WriteLine("    ✓ Calculate connection strength");
        Console.WriteLine("    ✓ Analyze temporal patterns");
        Console.WriteLine("    ✓ Recognize event sequences");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Temporal memory integration system complete");
        Console.ResetColor();
    }
}
