using System;
using System.Collections.Generic;
using System.Linq;
using BiorhythmDecisionSystem;

class Program
{
    static void Main()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Biorhythmic Decision-Making Engine v1.0                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

        // User's birth date - modify as needed
        var birthDate = new DateTime(1990, 5, 15);
        Console.WriteLine($"Birth Date: {birthDate:MMMM dd, yyyy}\n");

        // Initialize systems
        var engine = new BiorhythmicDecisionEngine(birthDate);
        var learner = new BiorhythmDecisionLearner(engine);
        var analytics = new BiorhythmAnalytics(birthDate);

        while (true)
        {
            Console.WriteLine("\n╔══ Main Menu ══════════════════════╗");
            Console.WriteLine("║ 1. Get Decision Recommendation    ║");
            Console.WriteLine("║ 2. Get Adaptive Recommendation    ║");
            Console.WriteLine("║ 3. Record Decision Outcome        ║");
            Console.WriteLine("║ 4. View Performance Stats         ║");
            Console.WriteLine("║ 5. Find Optimal Days              ║");
            Console.WriteLine("║ 6. Find Critical Days             ║");
            Console.WriteLine("║ 7. Detailed Phase Analysis        ║");
            Console.WriteLine("║ 8. Today's Full Analysis          ║");
            Console.WriteLine("║ 0. Exit                           ║");
            Console.WriteLine("╚═══════════════════════════════════╝");

            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    GetDecisionRecommendation(engine);
                    break;
                case "2":
                    GetAdaptiveRecommendation(learner);
                    break;
                case "3":
                    RecordDecisionOutcome(learner);
                    break;
                case "4":
                    ViewPerformanceStats(learner);
                    break;
                case "5":
                    FindOptimalDays(analytics);
                    break;
                case "6":
                    FindCriticalDays(analytics);
                    break;
                case "7":
                    DetailedPhaseAnalysis(analytics);
                    break;
                case "8":
                    TodaysFullAnalysis(engine, analytics);
                    break;
                case "0":
                    Console.WriteLine("\nGoodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void GetDecisionRecommendation(BiorhythmicDecisionEngine engine)
    {
        Console.WriteLine("\n╔═══ Decision Recommendation ═══╗");
        Console.WriteLine("║ Decision Types:               ║");
        Console.WriteLine("║ - athletic                    ║");
        Console.WriteLine("║ - creative                    ║");
        Console.WriteLine("║ - analytical                  ║");
        Console.WriteLine("║ - business                    ║");
        Console.WriteLine("║ - interpersonal               ║");
        Console.WriteLine("║ - health                      ║");
        Console.WriteLine("╚═══════════════════════════════╝");

        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        var recommendation = engine.GetDecisionRecommendation(decisionType, DateTime.Now);
        PrintRecommendation(recommendation);
    }

    static void GetAdaptiveRecommendation(BiorhythmDecisionLearner learner)
    {
        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        var recommendation = learner.GetAdaptiveRecommendation(decisionType, DateTime.Now);
        Console.WriteLine("\n╔═══ Adaptive Recommendation ═══╗");
        PrintRecommendation(recommendation);
        Console.WriteLine("(Adjusted based on your historical data)");
    }

    static void RecordDecisionOutcome(BiorhythmDecisionLearner learner)
    {
        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        Console.Write("Enter decision date (YYYY-MM-DD): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var date))
        {
            Console.WriteLine("Invalid date format.");
            return;
        }

        Console.WriteLine("\nOutcome (success/failure/neutral): ");
        var outcome = Console.ReadLine();

        Console.Write("Rating (1-10): ");
        if (!int.TryParse(Console.ReadLine(), out var rating))
        {
            Console.WriteLine("Invalid rating.");
            return;
        }

        Console.Write("Additional notes (optional): ");
        var notes = Console.ReadLine();

        learner.RecordDecision(decisionType, date, outcome, rating, notes);
        Console.WriteLine("\n✓ Decision recorded successfully!");
    }

    static void ViewPerformanceStats(BiorhythmDecisionLearner learner)
    {
        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        var stats = learner.GetPerformanceStats(decisionType);

        Console.WriteLine($"\n╔═══ Performance Stats: {decisionType.ToUpper()} ═══╗");
        foreach (var stat in stats)
        {
            if (stat.Key == "success_rate")
                Console.WriteLine($"║ {stat.Key}: {(double)stat.Value:P0}");
            else if (stat.Key == "average_rating")
                Console.WriteLine($"║ {stat.Key}: {(double)stat.Value:F1}/10");
            else
                Console.WriteLine($"║ {stat.Key}: {stat.Value}");
        }
        Console.WriteLine("╚══════════════════════════════════════╝");
    }

    static void FindOptimalDays(BiorhythmAnalytics analytics)
    {
        Console.Write("\nHow many days to analyze? (default 30): ");
        if (!int.TryParse(Console.ReadLine() ?? "30", out var days) || days <= 0)
            days = 30;

        var optimalDays = analytics.FindOptimalDays("any", days);

        Console.WriteLine($"\n╔═══ Top 10 Optimal Days (Next {days} days) ═══╗");
        foreach (var (date, score) in optimalDays)
        {
            Console.WriteLine($"║ {date:ddd MMM dd, yyyy} - Score: {score:F1}/100");
        }
        Console.WriteLine("╚════════════════════════════════════╝");
    }

    static void FindCriticalDays(BiorhythmAnalytics analytics)
    {
        Console.Write("\nHow many days to analyze? (default 30): ");
        if (!int.TryParse(Console.ReadLine() ?? "30", out var days) || days <= 0)
            days = 30;

        var criticalDays = analytics.FindCriticalDays(days);

        if (!criticalDays.Any())
        {
            Console.WriteLine($"\n✓ No critical days found in the next {days} days!");
        }
        else
        {
            Console.WriteLine($"\n╔═══ Critical Days (Next {days} days) ═══╗");
            Console.WriteLine("⚠️  Avoid major decisions on these dates:");
            foreach (var date in criticalDays)
            {
                Console.WriteLine($"║ {date:ddd MMM dd, yyyy}");
            }
            Console.WriteLine("╚═════════════════════════════════╝");
        }
    }

    static void DetailedPhaseAnalysis(BiorhythmAnalytics analytics)
    {
        Console.Write("\nEnter date (YYYY-MM-DD, or Enter for today): ");
        var input = Console.ReadLine();
        var date = string.IsNullOrWhiteSpace(input) ? DateTime.Now : DateTime.Parse(input);

        Console.WriteLine("\n" + analytics.GetPhaseAnalysis(date));
    }

    static void TodaysFullAnalysis(BiorhythmicDecisionEngine engine, BiorhythmAnalytics analytics)
    {
        var today = DateTime.Now;
        Console.WriteLine($"\n╔═══ Full Biorhythm Analysis for Today ({today:MMMM dd, yyyy}) ═══╗\n");

        // Show all decision types
        var decisionTypes = new[] { "athletic", "creative", "analytical", "business", "interpersonal", "health" };

        foreach (var type in decisionTypes)
        {
            var recommendation = engine.GetDecisionRecommendation(type, today);
            Console.WriteLine($"\n📋 {type.ToUpper()}");
            Console.WriteLine($"   Score: {recommendation.RecommendationScore:F1}/100");
            Console.WriteLine($"   {recommendation.Recommendation}");

            if (recommendation.Warnings.Any())
            {
                foreach (var warning in recommendation.Warnings.Take(2))
                    Console.WriteLine($"   {warning}");
            }
        }

        Console.WriteLine("\n" + analytics.GetPhaseAnalysis(today));
    }

    static void PrintRecommendation(DecisionRecommendation rec)
    {
        Console.WriteLine($"\n╔═══ {rec.DecisionType.ToUpper()} ═══╗");
        Console.WriteLine($"║ Recommendation Score: {rec.RecommendationScore:F1}/100");
        Console.WriteLine($"║ {rec.Recommendation}");

        if (rec.Warnings.Any())
        {
            Console.WriteLine("║");
            Console.WriteLine("║ ⚠️  Warnings:");
            foreach (var warning in rec.Warnings)
                Console.WriteLine($"║ {warning}");
        }

        if (rec.OptimalTiming.Any())
        {
            Console.WriteLine("║");
            Console.WriteLine("║ 📅 Timing:");
            foreach (var timing in rec.OptimalTiming)
                Console.WriteLine($"║ {timing}");
        }

        Console.WriteLine("╚═══════════════════════════╝");
    }
}
