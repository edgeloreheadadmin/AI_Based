using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ReadingAtDifferentSpeedsAndMeasurements
{
    public class SpeedProfile
    {
        public string ProfileId { get; set; }
        public string SpeedLevel { get; set; }
        public int WordsPerMinute { get; set; }
        public double ComprehensionRate { get; set; }
        public string Description { get; set; }
        public List<string> ApplicableScenarios { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TextContent
    {
        public string ContentId { get; set; }
        public string Text { get; set; }
        public int WordCount { get; set; }
        public string Category { get; set; }
        public double Complexity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ReadingSession
    {
        public string SessionId { get; set; }
        public string ContentId { get; set; }
        public string SpeedProfileId { get; set; }
        public int WordsRead { get; set; }
        public double TimeSpentSeconds { get; set; }
        public double ActualWPM { get; set; }
        public double ComprehensionScore { get; set; }
        public DateTime SessionDate { get; set; }
    }

    public class ReadingAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalSessions { get; set; }
        public double AverageWPM { get; set; }
        public double AverageComprehension { get; set; }
        public Dictionary<string, int> SpeedDistribution { get; set; }
        public double TotalWordsRead { get; set; }
        public string OptimalReadingPattern { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class SpeedMeasurementEngine
    {
        private Dictionary<string, SpeedProfile> profiles;
        private Dictionary<string, TextContent> contents;
        private Dictionary<string, ReadingSession> sessions;
        private Dictionary<string, ReadingAnalysis> analyses;

        public SpeedMeasurementEngine()
        {
            profiles = new Dictionary<string, SpeedProfile>();
            contents = new Dictionary<string, TextContent>();
            sessions = new Dictionary<string, ReadingSession>();
            analyses = new Dictionary<string, ReadingAnalysis>();
        }

        public void RegisterSpeedProfile(string profileId, string speedLevel, int wpm, double comprehension,
                                        string description, List<string> scenarios)
        {
            var profile = new SpeedProfile
            {
                ProfileId = profileId,
                SpeedLevel = speedLevel,
                WordsPerMinute = wpm,
                ComprehensionRate = comprehension,
                Description = description,
                ApplicableScenarios = new List<string>(scenarios),
                CreatedDate = DateTime.Now
            };
            profiles[profileId] = profile;
        }

        public void RegisterContent(string contentId, string text, string category, double complexity)
        {
            var content = new TextContent
            {
                ContentId = contentId,
                Text = text,
                WordCount = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length,
                Category = category,
                Complexity = complexity,
                CreatedDate = DateTime.Now
            };
            contents[contentId] = content;
        }

        public void RecordReadingSession(string sessionId, string contentId, string profileId, double timeSeconds)
        {
            if (!contents.ContainsKey(contentId) || !profiles.ContainsKey(profileId)) return;

            var content = contents[contentId];
            var profile = profiles[profileId];

            var session = new ReadingSession
            {
                SessionId = sessionId,
                ContentId = contentId,
                SpeedProfileId = profileId,
                WordsRead = content.WordCount,
                TimeSpentSeconds = timeSeconds,
                ActualWPM = 0.0,
                ComprehensionScore = 0.0,
                SessionDate = DateTime.Now
            };

            session.ActualWPM = (content.WordCount / timeSeconds) * 60;
            session.ComprehensionScore = CalculateComprehension(profile, session.ActualWPM, content.Complexity);

            sessions[sessionId] = session;
        }

        private double CalculateComprehension(SpeedProfile profile, double actualWPM, double textComplexity)
        {
            double speedFactor = profile.WordsPerMinute > 0 ? Math.Min(actualWPM / profile.WordsPerMinute, 2.0) : 1.0;
            double comprehension = profile.ComprehensionRate / speedFactor;
            comprehension *= (1.0 - textComplexity * 0.3);
            return Math.Min(Math.Max(comprehension, 0.0), 1.0);
        }

        public void AnalyzeReadingPatterns(string analysisId)
        {
            var analysis = new ReadingAnalysis
            {
                AnalysisId = analysisId,
                TotalSessions = sessions.Count,
                AverageWPM = 0.0,
                AverageComprehension = 0.0,
                SpeedDistribution = new Dictionary<string, int>(),
                TotalWordsRead = 0.0,
                OptimalReadingPattern = "",
                AnalyzedDate = DateTime.Now
            };

            if (sessions.Count == 0) return;

            var sessionList = sessions.Values.ToList();
            analysis.AverageWPM = sessionList.Average(s => s.ActualWPM);
            analysis.AverageComprehension = sessionList.Average(s => s.ComprehensionScore);
            analysis.TotalWordsRead = sessionList.Sum(s => s.WordsRead);

            foreach (var session in sessionList)
            {
                string speedCategory = CategorizeSpeed(session.ActualWPM);
                if (!analysis.SpeedDistribution.ContainsKey(speedCategory))
                    analysis.SpeedDistribution[speedCategory] = 0;
                analysis.SpeedDistribution[speedCategory]++;
            }

            analysis.OptimalReadingPattern = DetermineOptimalPattern(analysis.AverageWPM, analysis.AverageComprehension);

            analyses[analysisId] = analysis;
        }

        private string CategorizeSpeed(double wpm)
        {
            if (wpm < 100) return "Slow";
            if (wpm < 200) return "Average";
            if (wpm < 400) return "Fast";
            return "Speed Reading";
        }

        private string DetermineOptimalPattern(double avgWPM, double avgComprehension)
        {
            if (avgComprehension > 0.85) return "Optimal balance at current speed";
            if (avgComprehension > 0.70) return "Good comprehension, can increase speed slightly";
            if (avgComprehension > 0.50) return "Reduce speed to improve comprehension";
            return "Significant comprehension issues - recommend slow reading";
        }

        public void DisplayProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            Console.WriteLine($"\n  Speed Profile: {profile.SpeedLevel}");
            Console.WriteLine($"  Target WPM: {profile.WordsPerMinute}");
            Console.WriteLine($"  Expected Comprehension: {profile.ComprehensionRate * 100:F0}%");
            Console.WriteLine($"  Description: {profile.Description}");
            Console.WriteLine($"  Applicable Scenarios: {string.Join(", ", profile.ApplicableScenarios)}");
        }

        public void DisplaySession(string sessionId)
        {
            if (!sessions.ContainsKey(sessionId)) return;

            var session = sessions[sessionId];
            Console.WriteLine($"\n  Reading Session: {session.SessionId}");
            Console.WriteLine($"  Words Read: {session.WordsRead}");
            Console.WriteLine($"  Time Spent: {session.TimeSpentSeconds:F1} seconds");
            Console.WriteLine($"  Actual WPM: {session.ActualWPM:F1}");
            Console.WriteLine($"  Comprehension Score: {session.ComprehensionScore * 100:F1}%");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Reading Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Sessions: {analysis.TotalSessions}");
            Console.WriteLine($"  Average WPM: {analysis.AverageWPM:F1}");
            Console.WriteLine($"  Average Comprehension: {analysis.AverageComprehension * 100:F1}%");
            Console.WriteLine($"  Total Words Read: {analysis.TotalWordsRead:F0}");
            Console.WriteLine($"  Optimal Pattern: {analysis.OptimalReadingPattern}");
        }

        public int GetTotalProfiles()
        {
            return profiles.Count;
        }

        public int GetTotalSessions()
        {
            return sessions.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║       Reading at Different Speeds and Measurements             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new SpeedMeasurementEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Reading Speed Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterSpeedProfile("PROFILE-001", "Careful Reading",
            100, 0.95,
            "Slow, deliberate reading for complex material",
            new List<string> { "Technical documentation", "Legal contracts", "Research papers" });

        engine.RegisterSpeedProfile("PROFILE-002", "Normal Reading",
            250, 0.80,
            "Standard reading pace for general material",
            new List<string> { "News articles", "Blog posts", "Fiction" });

        engine.RegisterSpeedProfile("PROFILE-003", "Speed Reading",
            600, 0.60,
            "Fast reading for skimming and overview",
            new List<string> { "Email scanning", "Quick reviews", "Overview reading" });

        engine.RegisterSpeedProfile("PROFILE-004", "Skimming",
            1000, 0.40,
            "Very rapid reading for key points only",
            new List<string> { "Headlines", "Table of contents", "Summaries" });

        Console.WriteLine("  ✓ Registered 4 reading speed profiles");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Speed Profile Characteristics]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayProfile("PROFILE-001");
        engine.DisplayProfile("PROFILE-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Registering Text Content]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterContent("CONTENT-001",
            "The quick brown fox jumps over the lazy dog. This sentence contains every letter of the alphabet. " +
            "Reading speed varies based on content complexity and familiarity with the material. " +
            "Different reading strategies apply to different types of content.",
            "General", 0.3);

        engine.RegisterContent("CONTENT-002",
            "Advanced cryptographic algorithms utilize complex mathematical transformations to ensure data " +
            "confidentiality and integrity. Implementation requires understanding of number theory, " +
            "computational complexity, and security vulnerabilities. Modern systems employ asymmetric encryption.",
            "Technical", 0.8);

        Console.WriteLine("  ✓ Registered 2 text contents");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Recording Reading Sessions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordReadingSession("SESSION-001", "CONTENT-001", "PROFILE-001", 30.0);
        engine.RecordReadingSession("SESSION-002", "CONTENT-001", "PROFILE-002", 15.0);
        engine.RecordReadingSession("SESSION-003", "CONTENT-002", "PROFILE-001", 60.0);
        engine.RecordReadingSession("SESSION-004", "CONTENT-002", "PROFILE-003", 20.0);

        Console.WriteLine("  ✓ Recorded 4 reading sessions");
        engine.DisplaySession("SESSION-001");
        engine.DisplaySession("SESSION-004");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Reading Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeReadingPatterns("ANALYSIS-001");

        Console.WriteLine("  ✓ Analyzed reading patterns");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Speed Measurement Summary:");
        Console.WriteLine($"    Registered Profiles: {engine.GetTotalProfiles()}");
        Console.WriteLine($"    Recorded Sessions: {engine.GetTotalSessions()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Reading Speed and Comprehension Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Speed Measurement Architecture:");
        Console.WriteLine("    Layer 1: Speed Profile Definition (establish reading targets)");
        Console.WriteLine("    Layer 2: Content Registration (encode material properties)");
        Console.WriteLine("    Layer 3: Session Recording (capture reading activity)");
        Console.WriteLine("    Layer 4: WPM Calculation (measure actual reading speed)");
        Console.WriteLine("    Layer 5: Comprehension Scoring (assess understanding)");
        Console.WriteLine("    Layer 6: Speed Categorization (classify reading pace)");
        Console.WriteLine("    Layer 7: Pattern Analysis (identify optimal strategies)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define multiple reading speed profiles");
        Console.WriteLine("    ✓ Register text content with complexity metrics");
        Console.WriteLine("    ✓ Record and analyze reading sessions");
        Console.WriteLine("    ✓ Calculate words per minute (WPM)");
        Console.WriteLine("    ✓ Measure comprehension scores");
        Console.WriteLine("    ✓ Track speed distribution patterns");
        Console.WriteLine("    ✓ Recommend optimal reading strategies");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Speed measurement and reading analysis system complete");
        Console.ResetColor();
    }
}
