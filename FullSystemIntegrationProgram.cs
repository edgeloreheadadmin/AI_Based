using System;
using System.Collections.Generic;
using System.Linq;
using BiorhythmDecisionSystem;

class FullSystemIntegrationProgram
{
    static void Main()
    {
        Console.WriteLine("╔═════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Automatic Thought Process Alignment & Decision-Making System   ║");
        Console.WriteLine("║         Biorhythm → Mindset → Cognition → Decision             ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════════════╝\n");

        var birthDate = new DateTime(1990, 5, 15);
        Console.WriteLine($"Birth Date: {birthDate:MMMM dd, yyyy}\n");

        var thoughtMaker = new ThoughtProcessAlignmentMaker(birthDate);
        var cognitiveTracker = new CognitivePatternTracker(birthDate);
        var cognitiveAnalyzer = new CognitiveStateAnalyzer(birthDate);

        while (true)
        {
            Console.WriteLine("\n╔══════════════ Main Menu ══════════════╗");
            Console.WriteLine("║ COGNITIVE ANALYSIS                    ║");
            Console.WriteLine("║ 1. Today's Cognitive State             ║");
            Console.WriteLine("║ 2. Cognitive Health Assessment         ║");
            Console.WriteLine("║ 3. Optimal Tasks by Thinking Style     ║");
            Console.WriteLine("║ 4. Flow State Analysis                 ║");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("║ THOUGHT-ALIGNED DECISIONS              ║");
            Console.WriteLine("║ 5. Decision Aligned to Thought Process ║");
            Console.WriteLine("║ 6. Thinking Style Guide                ║");
            Console.WriteLine("║ 7. Compare Decision Types by Cognition ║");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("║ COGNITIVE TRACKING                     ║");
            Console.WriteLine("║ 8. Record Cognitive State              ║");
            Console.WriteLine("║ 9. Thinking Styles by Day              ║");
            Console.WriteLine("║ 10. Cognitive Quality Trends           ║");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("║ SYSTEM INTEGRATION                     ║");
            Console.WriteLine("║ 11. Full 3-Layer Analysis              ║");
            Console.WriteLine("║ 12. Predict Optimal Conditions         ║");
            Console.WriteLine("║ 13. Weekly Cognitive Schedule          ║");
            Console.WriteLine("║ 0. Exit                                ║");
            Console.WriteLine("╚════════════════════════════════════════╝");

            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowTodaysCognitiveState(cognitiveAnalyzer);
                    break;
                case "2":
                    ShowCognitiveHealthAssessment(thoughtMaker);
                    break;
                case "3":
                    ShowOptimalTasksByThinkingStyle(thoughtMaker);
                    break;
                case "4":
                    ShowFlowStateAnalysis(thoughtMaker);
                    break;
                case "5":
                    GetThoughtAlignedDecision(thoughtMaker);
                    break;
                case "6":
                    ShowThinkingStyleGuide(cognitiveAnalyzer);
                    break;
                case "7":
                    CompareDecisionsByThinking(cognitiveAnalyzer, thoughtMaker);
                    break;
                case "8":
                    RecordCognitiveState(cognitiveTracker);
                    break;
                case "9":
                    ShowThinkingStylePatterns(cognitiveTracker);
                    break;
                case "10":
                    ShowCognitiveQualityTrends(cognitiveTracker);
                    break;
                case "11":
                    ShowFullThreeLayerAnalysis(cognitiveAnalyzer, thoughtMaker);
                    break;
                case "12":
                    PredictOptimalConditions(cognitiveTracker);
                    break;
                case "13":
                    ShowWeeklyCognitiveSchedule(cognitiveAnalyzer);
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

    static void ShowTodaysCognitiveState(CognitiveStateAnalyzer analyzer)
    {
        var cognitive = analyzer.AnalyzeCognitiveState(DateTime.Now);

        Console.WriteLine($"\n╔═══ Cognitive State for {DateTime.Now:MMMM dd, yyyy} ═══╗\n");
        PrintCognitiveState(cognitive, analyzer);
    }

    static void ShowCognitiveHealthAssessment(ThoughtProcessAlignmentMaker maker)
    {
        Console.WriteLine(maker.GetCognitiveHealthAssessment(DateTime.Now));
    }

    static void ShowOptimalTasksByThinkingStyle(ThoughtProcessAlignmentMaker maker)
    {
        var tasks = maker.GetCognitiveOptimalTasks(DateTime.Now, 12);

        Console.WriteLine("\n╔═══ Optimal Tasks for Your Current Thinking Style ═══╗\n");

        for (int i = 0; i < Math.Min(12, tasks.Count); i++)
        {
            var (task, suitability, style) = tasks[i];
            var bar = new string('█', (int)(suitability * 20));
            Console.WriteLine($"{i + 1:2}. {task,-30} {bar,-20} {suitability:P0}");
        }
    }

    static void ShowFlowStateAnalysis(ThoughtProcessAlignmentMaker maker)
    {
        var (inFlow, activities, advice) = maker.GetFlowStateAnalysis(DateTime.Now);

        Console.WriteLine("\n╔═══ Flow State Analysis ═══╗\n");

        if (inFlow)
        {
            Console.WriteLine(advice);
            Console.WriteLine("\n🎯 Flow Activities:");
            foreach (var activity in activities)
                Console.WriteLine($"  ✓ {activity}");
        }
        else
        {
            Console.WriteLine(advice);
            Console.WriteLine("\n💡 To enhance flow state:");
            Console.WriteLine("  • Reduce distractions");
            Console.WriteLine("  • Extend focus periods gradually");
            Console.WriteLine("  • Match tasks to your thinking style");
        }
    }

    static void GetThoughtAlignedDecision(ThoughtProcessAlignmentMaker maker)
    {
        Console.WriteLine("\n╔═══ Decision Types ═══╗");
        Console.WriteLine("║ - analytical        ║");
        Console.WriteLine("║ - creative          ║");
        Console.WriteLine("║ - athletic          ║");
        Console.WriteLine("║ - business          ║");
        Console.WriteLine("║ - interpersonal     ║");
        Console.WriteLine("║ - health            ║");
        Console.WriteLine("╚═══════════════════════╝");

        Console.Write("\nEnter decision type: ");
        var decisionType = Console.ReadLine();

        var (decision, mindset, cognitive) = maker.GetThoughtAlignedRecommendation(decisionType, DateTime.Now);

        Console.WriteLine($"\n╔═══ Three-Layer Analysis ═══╗\n");

        Console.WriteLine($"Layer 1: BIORHYTHM");
        Console.WriteLine($"  (Physical, Emotional, Intellectual cycles)\n");

        Console.WriteLine($"Layer 2: MINDSET");
        Console.WriteLine($"  Current: {mindset.Type}");
        Console.WriteLine($"  Intensity: {mindset.Intensity:P0} | Stability: {mindset.Stability:P0}\n");

        Console.WriteLine($"Layer 3: COGNITIVE");
        Console.WriteLine($"  Primary Thinking: {cognitive.PrimaryStyle}");
        Console.WriteLine($"  Mental Clarity: {cognitive.MentalClarity:P0}");
        Console.WriteLine($"  Quality Level: {cognitive.CognitiveQualityLevel}");
        Console.WriteLine($"  Flow State: {(cognitive.InFlowState ? "YES 🔥" : "No")}\n");

        Console.WriteLine($"DECISION RECOMMENDATION");
        Console.WriteLine($"  Score: {decision.RecommendationScore:F1}/100");
        Console.WriteLine($"  {decision.Recommendation}\n");

        if (decision.Warnings.Any())
        {
            Console.WriteLine("Insights:");
            foreach (var warning in decision.Warnings.Take(4))
                Console.WriteLine($"  {warning}");
        }
    }

    static void ShowThinkingStyleGuide(CognitiveStateAnalyzer analyzer)
    {
        Console.WriteLine("\n╔═══ Thinking Style Guide ═══╗\n");

        var styles = new[]
        {
            ThinkingStyle.AnalyticalSequential,
            ThinkingStyle.HolisticSynthetic,
            ThinkingStyle.IntuitiveFast,
            ThinkingStyle.DetailOrientedPrecise,
            ThinkingStyle.CreativeAssociative,
            ThinkingStyle.SystemicStructured,
            ThinkingStyle.AdaptiveFluid,
            ThinkingStyle.AbstractConceptual,
            ThinkingStyle.ConcreteExperiential,
            ThinkingStyle.IntegrativeSynthesis
        };

        foreach (var style in styles)
        {
            var profile = analyzer.GetThinkingStyleInfo(style);
            Console.WriteLine($"\n{GetThinkingStyleEmoji(style)} {style}");
            Console.WriteLine($"  Description: {profile.Description}");
            Console.WriteLine($"  Strengths: {string.Join(", ", profile.Strengths)}");
            Console.WriteLine($"  Best for: {string.Join(", ", profile.OptimalFor.Take(3))}");
        }
    }

    static void CompareDecisionsByThinking(CognitiveStateAnalyzer analyzer, ThoughtProcessAlignmentMaker maker)
    {
        var cognitive = analyzer.AnalyzeCognitiveState(DateTime.Now);
        var decisionTypes = new[] { "analytical", "creative", "business", "interpersonal", "health", "athletic" };

        Console.WriteLine($"\n╔═══ Decision Readiness by Current Thinking ═══╗\n");
        Console.WriteLine($"Your Thinking: {cognitive.PrimaryStyle} (Primary) + {cognitive.SecondaryStyle} (Secondary)\n");

        foreach (var dt in decisionTypes)
        {
            var (decision, _, _) = maker.GetThoughtAlignedRecommendation(dt, DateTime.Now);
            var emoji = decision.RecommendationScore >= 80 ? "✓✓" : decision.RecommendationScore >= 60 ? "✓" : "✗";
            Console.WriteLine($"{emoji} {dt,-15} {decision.RecommendationScore:F0}/100");
        }
    }

    static void RecordCognitiveState(CognitivePatternTracker tracker)
    {
        Console.Write("\nEnter date (YYYY-MM-DD, or Enter for today): ");
        var input = Console.ReadLine();
        var date = string.IsNullOrWhiteSpace(input) ? DateTime.Now : DateTime.Parse(input);

        tracker.RecordCognitiveState(date);
        Console.WriteLine("✓ Cognitive state recorded successfully!");
    }

    static void ShowThinkingStylePatterns(CognitivePatternTracker tracker)
    {
        var patterns = tracker.GetThinkingStylesByDayOfWeek();

        Console.WriteLine("\n╔═══ Dominant Thinking Styles by Day ═══╗\n");

        var daysOrder = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                               DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };

        foreach (var day in daysOrder)
        {
            if (patterns.TryGetValue(day, out var style))
                Console.WriteLine($"{day,-10} → {GetThinkingStyleEmoji(style)} {style}");
            else
                Console.WriteLine($"{day,-10} → (No data)");
        }
    }

    static void ShowCognitiveQualityTrends(CognitivePatternTracker tracker)
    {
        Console.Write("\nAnalyze last how many days? (default 14): ");
        if (!int.TryParse(Console.ReadLine() ?? "14", out var days) || days <= 0)
            days = 14;

        Console.WriteLine("\n" + tracker.GetCognitiveQualityTrends(days));
    }

    static void ShowFullThreeLayerAnalysis(CognitiveStateAnalyzer analyzer, ThoughtProcessAlignmentMaker maker)
    {
        var today = DateTime.Now;

        Console.WriteLine($"\n╔═══ Complete Three-Layer Analysis ═══╗\n");

        // Layer 1: Biorhythm
        var calculator = new BiorhythmCalculator(new DateTime(1990, 5, 15));
        var profile = calculator.GetProfile(today);

        Console.WriteLine("LAYER 1: BIORHYTHM CYCLES");
        Console.WriteLine("─────────────────────────");
        Console.WriteLine($"Physical:     {profile.Physical.Value:+0.00;-0.00} ({profile.Physical.Phase})");
        Console.WriteLine($"Emotional:    {profile.Emotional.Value:+0.00;-0.00} ({profile.Emotional.Phase})");
        Console.WriteLine($"Intellectual: {profile.Intellectual.Value:+0.00;-0.00} ({profile.Intellectual.Phase})");
        Console.WriteLine($"Harmonic:     {profile.HarmonicIndex:P0}\n");

        // Layer 2: Mindset
        var mindsetAnalyzer = new MindsetAlignmentAnalyzer(new DateTime(1990, 5, 15));
        var mindset = mindsetAnalyzer.AnalyzeMindset(today);

        Console.WriteLine("LAYER 2: PSYCHOLOGICAL MINDSET");
        Console.WriteLine("──────────────────────────────");
        Console.WriteLine($"Type:         {mindset.Type}");
        Console.WriteLine($"Intensity:    {mindset.Intensity:P0}");
        Console.WriteLine($"Stability:    {mindset.Stability:P0}\n");

        // Layer 3: Cognition
        var cognitive = analyzer.AnalyzeCognitiveState(today);

        Console.WriteLine("LAYER 3: COGNITIVE THINKING");
        Console.WriteLine("───────────────────────────");
        Console.WriteLine($"Primary:      {cognitive.PrimaryStyle}");
        Console.WriteLine($"Secondary:    {cognitive.SecondaryStyle}");
        Console.WriteLine($"Clarity:      {cognitive.MentalClarity:P0}");
        Console.WriteLine($"Focus:        {cognitive.FocusIntensity:P0}");
        Console.WriteLine($"Quality:      {cognitive.CognitiveQualityLevel}");
        Console.WriteLine($"Flow:         {(cognitive.InFlowState ? "YES 🔥" : "No")}\n");

        // Integrated Assessment
        Console.WriteLine("INTEGRATED ASSESSMENT");
        Console.WriteLine("─────────────────────");
        double alignment = (profile.HarmonicIndex + mindset.Stability + cognitive.MentalClarity) / 3;
        Console.WriteLine($"Overall Alignment: {alignment:P0}");
        Console.WriteLine($"Status: {GetAlignmentStatus(alignment)}\n");

        Console.WriteLine("RECOMMENDATIONS");
        Console.WriteLine("───────────────");
        if (cognitive.InFlowState)
            Console.WriteLine("🔥 SEIZE THIS WINDOW - Optimal state for deep work");
        else if (alignment > 0.75)
            Console.WriteLine("✓ Excellent conditions - good time for important decisions");
        else if (alignment > 0.55)
            Console.WriteLine("◐ Acceptable conditions - proceed with standard caution");
        else
            Console.WriteLine("⚠️  Suboptimal conditions - defer non-urgent decisions");
    }

    static void PredictOptimalConditions(CognitivePatternTracker tracker)
    {
        Console.WriteLine("\n╔═══ Activities ═══╗");
        Console.WriteLine("║ - coding         ║");
        Console.WriteLine("║ - design         ║");
        Console.WriteLine("║ - strategy       ║");
        Console.WriteLine("║ - negotiation    ║");
        Console.WriteLine("║ - editing        ║");
        Console.WriteLine("║ - brainstorm     ║");
        Console.WriteLine("║ - analysis       ║");
        Console.WriteLine("║ - planning       ║");
        Console.WriteLine("║ - learning       ║");
        Console.WriteLine("║ - execution      ║");
        Console.WriteLine("╚══════════════════╝");

        Console.Write("\nEnter activity: ");
        var activity = Console.ReadLine();

        var (idealStyle, timing, tips) = tracker.PredictOptimalConditionsForActivity(activity);

        Console.WriteLine($"\n╔═══ Optimal Conditions for '{activity}' ═══╗\n");
        Console.WriteLine($"Ideal Thinking Style: {GetThinkingStyleEmoji(idealStyle)} {idealStyle}");
        Console.WriteLine($"Timing: {timing}\n");
        Console.WriteLine("Optimization Tips:");
        foreach (var tip in tips)
            Console.WriteLine($"  • {tip}");
    }

    static void ShowWeeklyCognitiveSchedule(CognitiveStateAnalyzer analyzer)
    {
        Console.WriteLine("\n╔═══ 7-Day Cognitive Schedule ═══╗\n");

        for (int i = 0; i < 7; i++)
        {
            var date = DateTime.Now.AddDays(i);
            var cognitive = analyzer.AnalyzeCognitiveState(date);

            Console.WriteLine($"{date:ddd MMM dd}");
            Console.WriteLine($"  Primary:   {cognitive.PrimaryStyle}");
            Console.WriteLine($"  Clarity:   {cognitive.MentalClarity:P0}");
            Console.WriteLine($"  Quality:   {cognitive.CognitiveQualityLevel}");
            Console.WriteLine($"  Best For:  {string.Join(", ", cognitive.OptimalTasks.Take(2))}");
            Console.WriteLine();
        }
    }

    static void PrintCognitiveState(CognitiveState cognitive, CognitiveStateAnalyzer analyzer)
    {
        Console.WriteLine($"Thinking Styles:");
        Console.WriteLine($"  Primary:   {GetThinkingStyleEmoji(cognitive.PrimaryStyle)} {cognitive.PrimaryStyle}");
        Console.WriteLine($"  Secondary: {GetThinkingStyleEmoji(cognitive.SecondaryStyle)} {cognitive.SecondaryStyle}\n");

        Console.WriteLine($"Mental Capacities:");
        Console.WriteLine($"  Mental Clarity:     {cognitive.MentalClarity:P0}");
        Console.WriteLine($"  Focus Intensity:    {cognitive.FocusIntensity:P0}");
        Console.WriteLine($"  Creative Capacity:  {cognitive.CreativeCapacity:P0}");
        Console.WriteLine($"  Logical Capacity:   {cognitive.LogicalCapacity:P0}");
        Console.WriteLine($"  Processing Speed:   {cognitive.ProcessingSpeed:P0}");
        Console.WriteLine($"  Intuitive Power:    {cognitive.IntuitivePower:P0}\n");

        Console.WriteLine($"State:");
        Console.WriteLine($"  Quality Level:      {cognitive.CognitiveQualityLevel}");
        Console.WriteLine($"  Flow State:         {(cognitive.InFlowState ? "YES 🔥" : "No")}");
        Console.WriteLine($"  Cognitive Shift In: ~{cognitive.MinutesUntilCognitiveShift} minutes\n");

        Console.WriteLine($"Optimal Activities:");
        foreach (var task in cognitive.OptimalTasks.Take(5))
            Console.WriteLine($"  ✓ {task}");

        Console.WriteLine($"\nActivities to Avoid:");
        foreach (var task in cognitive.SuboptimalTasks.Take(3))
            Console.WriteLine($"  ✗ {task}");

        var profile1 = analyzer.GetThinkingStyleInfo(cognitive.PrimaryStyle);
        var profile2 = analyzer.GetThinkingStyleInfo(cognitive.SecondaryStyle);

        Console.WriteLine($"\nPrimary Style Capabilities:");
        foreach (var (capability, value) in profile1.ProcessCapabilities)
            Console.WriteLine($"  {capability}: {value:P0}");

        Console.WriteLine();
    }

    static string GetThinkingStyleEmoji(ThinkingStyle style)
    {
        return style switch
        {
            ThinkingStyle.AnalyticalSequential => "🔍",
            ThinkingStyle.HolisticSynthetic => "🌐",
            ThinkingStyle.IntuitiveFast => "⚡",
            ThinkingStyle.DetailOrientedPrecise => "🎯",
            ThinkingStyle.CreativeAssociative => "✨",
            ThinkingStyle.SystemicStructured => "📐",
            ThinkingStyle.AdaptiveFluid => "🌊",
            ThinkingStyle.AbstractConceptual => "☁️",
            ThinkingStyle.ConcreteExperiential => "🏔️",
            ThinkingStyle.IntegrativeSynthesis => "🧩",
            _ => "🧠"
        };
    }

    static string GetAlignmentStatus(double alignment)
    {
        return alignment switch
        {
            > 0.85 => "🔥 PEAK - All systems optimal",
            > 0.75 => "✓ EXCELLENT - Great alignment",
            > 0.65 => "◐ GOOD - Acceptable conditions",
            > 0.50 => "○ FAIR - Proceed with caution",
            _ => "✗ POOR - Defer if possible"
        };
    }
}
