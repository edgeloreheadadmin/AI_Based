using System;
using System.Collections.Generic;
using System.Linq;
using BiorhythmDecisionSystem;

class MindsetIntegratedProgram
{
    static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Automatic Mindset Alignment & Decision-Making System        ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        var birthDate = new DateTime(1990, 5, 15);
        Console.WriteLine($"Birth Date: {birthDate:MMMM dd, yyyy}\n");

        var decisionEngine = new BiorhythmicDecisionEngine(birthDate);
        var mindsetMaker = new MindsetAlignedDecisionMaker(birthDate);
        var mindsetTracker = new MindsetTracker(birthDate);
        var mindsetAnalyzer = new MindsetAlignmentAnalyzer(birthDate);

        while (true)
        {
            Console.WriteLine("\n╔════════════ Main Menu ════════════════╗");
            Console.WriteLine("║ MINDSET ANALYSIS                      ║");
            Console.WriteLine("║ 1. Today's Mindset Profile            ║");
            Console.WriteLine("║ 2. Mindset Timeline (Next 7 Days)     ║");
            Console.WriteLine("║ 3. Optimal Activities for Today       ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ MINDSET-ALIGNED DECISIONS             ║");
            Console.WriteLine("║ 4. Get Decision Aligned to Mindset    ║");
            Console.WriteLine("║ 5. Compare Decision Types by Mindset  ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ MINDSET TRACKING                      ║");
            Console.WriteLine("║ 6. Record Current Mindset             ║");
            Console.WriteLine("║ 7. Mindset Frequency Analysis         ║");
            Console.WriteLine("║ 8. Patterns by Day of Week            ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ PLANNING                              ║");
            Console.WriteLine("║ 9. Optimized Weekly Schedule          ║");
            Console.WriteLine("║ 10. Full System Analysis              ║");
            Console.WriteLine("║ 0. Exit                               ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");

            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowTodaysMindset(mindsetAnalyzer);
                    break;
                case "2":
                    ShowMindsetTimeline(mindsetAnalyzer);
                    break;
                case "3":
                    ShowOptimalActivities(mindsetMaker);
                    break;
                case "4":
                    GetMindsetAlignedDecision(mindsetMaker);
                    break;
                case "5":
                    CompareDecisionsByMindset(mindsetAnalyzer, decisionEngine);
                    break;
                case "6":
                    RecordMindset(mindsetTracker);
                    break;
                case "7":
                    ShowMindsetFrequency(mindsetTracker);
                    break;
                case "8":
                    ShowMindsetPatterns(mindsetTracker);
                    break;
                case "9":
                    ShowOptimizedSchedule(mindsetTracker);
                    break;
                case "10":
                    ShowFullAnalysis(mindsetAnalyzer, mindsetMaker, decisionEngine);
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

    static void ShowTodaysMindset(MindsetAlignmentAnalyzer analyzer)
    {
        var today = DateTime.Now;
        var mindset = analyzer.AnalyzeMindset(today);

        Console.WriteLine($"\n╔═══ Mindset Analysis for {today:MMMM dd, yyyy} ═══╗\n");
        PrintMindsetState(mindset);
    }

    static void ShowMindsetTimeline(MindsetAlignmentAnalyzer analyzer)
    {
        Console.WriteLine("\n╔═══ 7-Day Mindset Timeline ═══╗\n");

        for (int i = 0; i < 7; i++)
        {
            var date = DateTime.Now.AddDays(i);
            var mindset = analyzer.AnalyzeMindset(date);

            Console.WriteLine($"{date:ddd MMM dd} - {GetMindsetEmoji(mindset.Type)} {mindset.Type}");
            Console.WriteLine($"  Intensity: {mindset.Intensity:P0} | Stability: {mindset.Stability:P0}");
            Console.WriteLine($"  Peak in: {mindset.TransitionDaysUntilNext} days");
            Console.WriteLine();
        }
    }

    static void ShowOptimalActivities(MindsetAlignedDecisionMaker maker)
    {
        var activities = maker.GetMindsetOptimalActivities(DateTime.Now);

        Console.WriteLine("\n╔═══ Activities Optimized for Your Current Mindset ═══╗\n");

        for (int i = 0; i < Math.Min(8, activities.Count); i++)
        {
            var (activity, compat) = activities[i];
            var bar = new string('█', (int)(compat * 20));
            Console.WriteLine($"{i + 1}. {activity,-25} {bar,-20} {compat:P0}");
        }
    }

    static void GetMindsetAlignedDecision(MindsetAlignedDecisionMaker maker)
    {
        Console.WriteLine("\n╔═══ Decision Types ═══╗");
        Console.WriteLine("║ - athletic            ║");
        Console.WriteLine("║ - creative            ║");
        Console.WriteLine("║ - analytical          ║");
        Console.WriteLine("║ - business            ║");
        Console.WriteLine("║ - interpersonal       ║");
        Console.WriteLine("║ - health              ║");
        Console.WriteLine("╚═══════════════════════╝");

        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        var (decision, mindset) = maker.GetMindsetAlignedRecommendation(decisionType, DateTime.Now);

        Console.WriteLine($"\n╔═══ Mindset-Aligned Recommendation ═══╗\n");
        Console.WriteLine($"Current Mindset: {GetMindsetEmoji(mindset.Type)} {mindset.Type}");
        Console.WriteLine($"Intensity: {mindset.Intensity:P0} | Stability: {mindset.Stability:P0}\n");

        Console.WriteLine($"Decision Score: {decision.RecommendationScore:F1}/100");
        Console.WriteLine($"Recommendation: {decision.Recommendation}\n");

        Console.WriteLine($"✓ Your mindset characteristics:");
        foreach (var char in mindset.Characteristics.Take(3))
            Console.WriteLine($"  • {char}");

        Console.WriteLine($"\n💪 Your strengths right now:");
        foreach (var strength in mindset.StrengthAreas.Take(3))
            Console.WriteLine($"  • {strength}");

        Console.WriteLine($"\n📋 Things to be mindful of:");
        foreach (var weakness in mindset.WeakAreas.Take(3))
            Console.WriteLine($"  • {weakness}");
    }

    static void CompareDecisionsByMindset(MindsetAlignmentAnalyzer analyzer, BiorhythmicDecisionEngine engine)
    {
        Console.WriteLine("\n╔═══ Decision Scores by Current Mindset ═══╗\n");

        var mindset = analyzer.AnalyzeMindset(DateTime.Now);
        var decisionTypes = new[] { "athletic", "creative", "analytical", "business", "interpersonal", "health" };

        var scores = decisionTypes.Select(dt =>
        {
            var rec = engine.GetDecisionRecommendation(dt, DateTime.Now);
            return (type: dt, score: rec.RecommendationScore);
        }).OrderByDescending(x => x.score).ToList();

        Console.WriteLine($"Current Mindset: {mindset.Type}\n");

        foreach (var (type, score) in scores)
        {
            var bar = new string('█', (int)(score / 5));
            var status = score >= 80 ? "✓" : score >= 60 ? "◐" : score >= 40 ? "○" : "✗";
            Console.WriteLine($"{status} {type,-15} {bar,-20} {score:F1}/100");
        }
    }

    static void RecordMindset(MindsetTracker tracker)
    {
        Console.Write("\nEnter date (YYYY-MM-DD, or Enter for today): ");
        var input = Console.ReadLine();
        var date = string.IsNullOrWhiteSpace(input) ? DateTime.Now : DateTime.Parse(input);

        tracker.RecordMindset(date);
        Console.WriteLine("✓ Mindset recorded successfully!");
    }

    static void ShowMindsetFrequency(MindsetTracker tracker)
    {
        var frequencies = tracker.GetMindsetFrequency(60);

        if (!frequencies.Any())
        {
            Console.WriteLine("\nNo recorded mindsets. Start by recording your mindset with option 6.");
            return;
        }

        Console.WriteLine("\n╔═══ Mindset Frequency (Last 60 Days) ═══╗\n");

        foreach (var (mindset, count) in frequencies.OrderByDescending(f => f.Value))
        {
            var bar = new string('█', count);
            Console.WriteLine($"{mindset,-25} {bar,-20} {count}x");
        }
    }

    static void ShowMindsetPatterns(MindsetTracker tracker)
    {
        var patterns = tracker.GetMindsetsByDayOfWeek();

        Console.WriteLine("\n╔═══ Dominant Mindsets by Day of Week ═══╗\n");

        var daysOrder = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                               DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

        foreach (var day in daysOrder)
        {
            if (patterns.TryGetValue(day, out var mindset))
                Console.WriteLine($"{day,-10} → {GetMindsetEmoji(mindset)} {mindset}");
            else
                Console.WriteLine($"{day,-10} → (No data)");
        }
    }

    static void ShowOptimizedSchedule(MindsetTracker tracker)
    {
        var schedule = tracker.GetOptimizedScheduleRecommendation(DateTime.Now, 7);
        Console.WriteLine("\n" + schedule);
    }

    static void ShowFullAnalysis(MindsetAlignmentAnalyzer analyzer, MindsetAlignedDecisionMaker maker,
        BiorhythmicDecisionEngine engine)
    {
        var today = DateTime.Now;
        var mindset = analyzer.AnalyzeMindset(today);
        var profile = new BiorhythmCalculator(today).GetProfile(today);

        Console.WriteLine($"\n╔═══ Complete System Analysis for {today:MMMM dd, yyyy} ═══╗\n");

        // Biorhythm Status
        Console.WriteLine("BIORHYTHM STATUS");
        Console.WriteLine("─────────────────");
        Console.WriteLine($"Physical:     {profile.Physical.Value:+0.00;-0.00} - {profile.Physical.Phase}");
        Console.WriteLine($"Emotional:    {profile.Emotional.Value:+0.00;-0.00} - {profile.Emotional.Phase}");
        Console.WriteLine($"Intellectual: {profile.Intellectual.Value:+0.00;-0.00} - {profile.Intellectual.Phase}");
        Console.WriteLine($"Harmonic:     {profile.HarmonicIndex:P0}\n");

        // Mindset Status
        Console.WriteLine("MINDSET STATUS");
        Console.WriteLine("──────────────");
        Console.WriteLine($"Type:         {GetMindsetEmoji(mindset.Type)} {mindset.Type}");
        Console.WriteLine($"Intensity:    {mindset.Intensity:P0}");
        Console.WriteLine($"Stability:    {mindset.Stability:P0}");
        Console.WriteLine($"Duration:     {mindset.TransitionDaysUntilNext} days\n");

        // Characteristics
        Console.WriteLine("YOUR STATE");
        Console.WriteLine("──────────");
        foreach (var char in mindset.Characteristics)
            Console.WriteLine($"• {char}");
        Console.WriteLine();

        // Best Activities
        Console.WriteLine("BEST ACTIVITIES TODAY");
        Console.WriteLine("────────────────────");
        foreach (var activity in mindset.RecommendedActivities.Take(5))
            Console.WriteLine($"✓ {activity}");
        Console.WriteLine();

        // Activities to Avoid
        Console.WriteLine("AVOID TODAY");
        Console.WriteLine("───────────");
        foreach (var activity in mindset.ActivitiesToAvoid.Take(5))
            Console.WriteLine($"✗ {activity}");
        Console.WriteLine();

        // Decision Readiness
        Console.WriteLine("DECISION READINESS");
        Console.WriteLine("──────────────────");
        var decisionTypes = new[] { "athletic", "creative", "analytical", "business", "interpersonal", "health" };
        foreach (var dt in decisionTypes)
        {
            var rec = engine.GetDecisionRecommendation(dt, today);
            var emoji = rec.RecommendationScore >= 80 ? "✓" : rec.RecommendationScore >= 60 ? "◐" : "✗";
            Console.WriteLine($"{emoji} {dt,-15} {rec.RecommendationScore:F0}/100");
        }

        Console.WriteLine("\n" + new string('─', 50));
    }

    static void PrintMindsetState(MindsetState mindset)
    {
        Console.WriteLine($"Mindset Type:     {GetMindsetEmoji(mindset.Type)} {mindset.Type}");
        Console.WriteLine($"Intensity:        {mindset.Intensity:P0} (how pronounced)");
        Console.WriteLine($"Stability:        {mindset.Stability:P0} (how consistent)");
        Console.WriteLine($"Transition:       {mindset.TransitionDaysUntilNext} days until shift\n");

        Console.WriteLine("Characteristics:");
        foreach (var char in mindset.Characteristics)
            Console.WriteLine($"  • {char}");

        Console.WriteLine("\nStrengths:");
        foreach (var strength in mindset.StrengthAreas)
            Console.WriteLine($"  ✓ {strength}");

        Console.WriteLine("\nAreas to Watch:");
        foreach (var weakness in mindset.WeakAreas)
            Console.WriteLine($"  ⚠ {weakness}");

        Console.WriteLine("\nRecommended Activities:");
        foreach (var activity in mindset.RecommendedActivities.Take(5))
            Console.WriteLine($"  ✓ {activity}");

        Console.WriteLine("\nActivities to Avoid:");
        foreach (var activity in mindset.ActivitiesToAvoid.Take(5))
            Console.WriteLine($"  ✗ {activity}");

        Console.WriteLine();
    }

    static string GetMindsetEmoji(MindsetType type)
    {
        return type switch
        {
            MindsetType.AnalyticalLogical => "🧠",
            MindsetType.CreativeExpressive => "🎨",
            MindsetType.ActionOriented => "⚡",
            MindsetType.EmotionalIntuitive => "❤️",
            MindsetType.BalancedHarmonious => "☮️",
            MindsetType.LowEnergyReflective => "🧘",
            MindsetType.StressedFractured => "⚠️",
            MindsetType.FocusedDetermined => "🎯",
            MindsetType.PlayfulSpontaneous => "🎭",
            MindsetType.CautiousConservative => "🛡️",
            _ => "🔮"
        };
    }
}
