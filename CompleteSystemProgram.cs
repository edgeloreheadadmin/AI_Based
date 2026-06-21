using System;
using System.Collections.Generic;
using System.Linq;
using BiorhythmDecisionSystem;

class CompleteSystemProgram
{
    static void Main()
    {
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     COMPLETE FOUR-LAYER DECISION-MAKING SYSTEM                ║");
        Console.WriteLine("║  Biorhythm → Mindset → Cognition → Body Timing → Decision     ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝\n");

        var birthDate = new DateTime(1990, 5, 15);
        Console.WriteLine($"Birth Date: {birthDate:MMMM dd, yyyy}");
        Console.Write("Select your chronotype (1=ExtremeEarlyBird, 2=Lark, 3=Intermediate, 4=Owl, 5=ExtremeNightOwl) [default=3]: ");

        var chronotypeInput = Console.ReadLine();
        var chronotype = (chronotypeInput) switch
        {
            "1" => Chronotype.ExtremeEarlyBird,
            "2" => Chronotype.Lark,
            "4" => Chronotype.Owl,
            "5" => Chronotype.ExtremeNightOwl,
            _ => Chronotype.Intermediate
        };

        Console.WriteLine();

        var fourLayerMaker = new FourLayerDecisionMaker(birthDate, chronotype);
        var bodyTimingAnalyzer = new BodyTimingAnalyzer(birthDate, chronotype);
        var scheduleOptimizer = new ScheduleOptimizer(birthDate, chronotype);

        while (true)
        {
            Console.WriteLine("\n╔══════════════ Main Menu ══════════════╗");
            Console.WriteLine("║ FOUR-LAYER SYSTEM                     ║");
            Console.WriteLine("║ 1. Complete Alignment Report (Now)    ║");
            Console.WriteLine("║ 2. Fully Aligned Decision             ║");
            Console.WriteLine("║ 3. Body Timing Report                 ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ SCHEDULING & OPTIMIZATION             ║");
            Console.WriteLine("║ 4. Daily Schedule Visualization       ║");
            Console.WriteLine("║ 5. Find Optimal Time for Activity     ║");
            Console.WriteLine("║ 6. Hourly Schedule with Details       ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ ANALYSIS                              ║");
            Console.WriteLine("║ 7. Best Times This Week               ║");
            Console.WriteLine("║ 8. Compare Times (Today)              ║");
            Console.WriteLine("║ 9. Activity-Specific Timing           ║");
            Console.WriteLine("║                                       ║");
            Console.WriteLine("║ 10. Exit                              ║");
            Console.WriteLine("╚═══════════════════════════════════════╝");

            Console.Write("\nSelect option: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ShowCompleteAlignmentReport(fourLayerMaker);
                    break;
                case "2":
                    GetFullyAlignedDecision(fourLayerMaker);
                    break;
                case "3":
                    ShowBodyTimingReport(bodyTimingAnalyzer);
                    break;
                case "4":
                    ShowDailyScheduleVisualization(scheduleOptimizer);
                    break;
                case "5":
                    FindOptimalTimeForActivity(scheduleOptimizer);
                    break;
                case "6":
                    ShowHourlyScheduleDetails(scheduleOptimizer);
                    break;
                case "7":
                    ShowBestTimesThisWeek(bodyTimingAnalyzer);
                    break;
                case "8":
                    CompareTimesInDay(bodyTimingAnalyzer);
                    break;
                case "9":
                    ShowActivitySpecificTiming(scheduleOptimizer);
                    break;
                case "10":
                    Console.WriteLine("\nGoodbye!");
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    static void ShowCompleteAlignmentReport(FourLayerDecisionMaker maker)
    {
        Console.Write("\nAnalyze what time? (YYYY-MM-DD HH:mm, or Enter for now): ");
        var input = Console.ReadLine();
        var targetTime = string.IsNullOrWhiteSpace(input) ? DateTime.Now : DateTime.Parse(input);

        Console.WriteLine("\n" + maker.GetCompleteAlignmentReport(targetTime));
    }

    static void GetFullyAlignedDecision(FourLayerDecisionMaker maker)
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

        Console.Write("Analyze what time? (YYYY-MM-DD HH:mm, or Enter for now): ");
        var timeInput = Console.ReadLine();
        var targetTime = string.IsNullOrWhiteSpace(timeInput) ? DateTime.Now : DateTime.Parse(timeInput);

        var (decision, mindset, cognitive, bodyTiming) =
            maker.GetFullyAlignedRecommendation(decisionType, targetTime);

        Console.WriteLine($"\n╔═══ Four-Layer Aligned Decision Recommendation ═══╗\n");

        PrintFourLayers(mindset, cognitive, bodyTiming);

        Console.WriteLine($"\nFINAL DECISION SCORE: {decision.RecommendationScore:F1}/100");
        Console.WriteLine($"Recommendation: {decision.Recommendation}\n");

        if (decision.Warnings.Any())
        {
            Console.WriteLine("Insights:");
            foreach (var warning in decision.Warnings.Take(5))
                Console.WriteLine($"  {warning}");
        }
    }

    static void ShowBodyTimingReport(BodyTimingAnalyzer analyzer)
    {
        Console.Write("\nWhat time? (HH:mm, or Enter for now): ");
        var timeInput = Console.ReadLine();
        var time = string.IsNullOrWhiteSpace(timeInput)
            ? DateTime.Now
            : DateTime.Now.Date.Add(TimeSpan.Parse(timeInput));

        Console.WriteLine("\n" + analyzer.GetDetailedTimingReport(time));
    }

    static void ShowDailyScheduleVisualization(ScheduleOptimizer optimizer)
    {
        Console.Write("\nWork start time (HH:mm) [default 08:00]: ");
        var startInput = Console.ReadLine() ?? "08:00";
        var startTime = TimeSpan.Parse(startInput);

        Console.Write("Work end time (HH:mm) [default 18:00]: ");
        var endInput = Console.ReadLine() ?? "18:00";
        var endTime = TimeSpan.Parse(endInput);

        Console.WriteLine("\n" + optimizer.VisualizeDailySchedule(DateTime.Now, startTime, endTime));
    }

    static void FindOptimalTimeForActivity(ScheduleOptimizer optimizer)
    {
        Console.WriteLine("\n╔═══ Activity Types ═══╗");
        Console.WriteLine("║ - focus             ║");
        Console.WriteLine("║ - creative          ║");
        Console.WriteLine("║ - physical          ║");
        Console.WriteLine("║ - social            ║");
        Console.WriteLine("║ - rest              ║");
        Console.WriteLine("╚═════════════════════╝");

        Console.Write("\nEnter activity type: ");
        var activityType = Console.ReadLine();

        Console.Write("Search start time (HH:mm) [default 08:00]: ");
        var startInput = Console.ReadLine() ?? "08:00";
        var searchStart = TimeSpan.Parse(startInput);

        Console.Write("Search end time (HH:mm) [default 18:00]: ");
        var endInput = Console.ReadLine() ?? "18:00";
        var searchEnd = TimeSpan.Parse(endInput);

        var optimalTime = optimizer.FindOptimalTimeForActivity(DateTime.Now, activityType, searchStart, searchEnd);

        Console.WriteLine($"\n╔═══ Optimal Time for '{activityType}' ═══╗");
        Console.WriteLine($"║ Recommended: {optimalTime:hh\\:mm}\n");
        Console.WriteLine("This is when your body/mind is best suited for this activity type.");
        Console.WriteLine("╚════════════════════════════════════════╝");
    }

    static void ShowHourlyScheduleDetails(ScheduleOptimizer optimizer)
    {
        Console.Write("\nStart time (HH:mm) [default 06:00]: ");
        var startInput = Console.ReadLine() ?? "06:00";
        var startTime = TimeSpan.Parse(startInput);

        Console.Write("End time (HH:mm) [default 22:00]: ");
        var endInput = Console.ReadLine() ?? "22:00";
        var endTime = TimeSpan.Parse(endInput);

        var schedule = optimizer.GenerateOptimalSchedule(DateTime.Now, startTime, endTime);

        Console.WriteLine("\n╔═══ Detailed Hourly Schedule ═══╗\n");
        foreach (var block in schedule)
        {
            var startHour = block.StartTime.Hours;
            var startMin = block.StartTime.Minutes;
            var endHour = block.EndTime.Hours;
            var endMin = block.EndTime.Minutes;

            Console.WriteLine($"{startHour:D2}:{startMin:D2}-{endHour:D2}:{endMin:D2} [{block.BodyState}]");
            Console.WriteLine($"  Energy:    {block.EnergyLevel:P0}");
            Console.WriteLine($"  Focus:     {block.FocusQuality:P0}");
            Console.WriteLine($"  Creativity: {block.CreativityLevel:P0}");
            Console.WriteLine($"  Best for:  {string.Join(", ", block.OptimalActivities)}");
            Console.WriteLine();
        }
    }

    static void ShowBestTimesThisWeek(BodyTimingAnalyzer analyzer)
    {
        Console.WriteLine("\n╔═══ Best Times This Week (Next 7 Days) ═══╗\n");

        var dayScores = new List<(DateTime date, double avgEnergy, string topPeriod)>();

        for (int i = 0; i < 7; i++)
        {
            var date = DateTime.Now.AddDays(i);
            double totalEnergy = 0;
            TimeSpan topTime = TimeSpan.Zero;
            double topEnergy = 0;

            // Sample every 2 hours
            for (int hour = 8; hour < 22; hour += 2)
            {
                var checkTime = date.Date.Add(new TimeSpan(hour, 0, 0));
                var state = analyzer.AnalyzeBodyTiming(checkTime);
                totalEnergy += state.EnergyLevel;

                if (state.EnergyLevel > topEnergy)
                {
                    topEnergy = state.EnergyLevel;
                    topTime = checkTime.TimeOfDay;
                }
            }

            double avgEnergy = totalEnergy / 7;
            dayScores.Add((date, avgEnergy, topTime.ToString(@"hh\:mm")));
        }

        foreach (var (date, energy, topTime) in dayScores)
        {
            var bar = new string('█', (int)(energy * 20));
            Console.WriteLine($"{date:ddd MMM dd}  {bar,-20} Peak: {topTime}");
        }
    }

    static void CompareTimesInDay(BodyTimingAnalyzer analyzer)
    {
        Console.WriteLine("\n╔═══ Energy Levels Throughout Today ═══╗\n");

        var times = new[] { 6, 8, 10, 12, 14, 16, 18, 20, 22 };

        foreach (var hour in times)
        {
            var checkTime = DateTime.Now.Date.Add(new TimeSpan(hour, 0, 0));
            var state = analyzer.AnalyzeBodyTiming(checkTime);
            var bar = new string('█', (int)(state.EnergyLevel * 25));
            var period = state.CurrentEnergyPhase;

            Console.WriteLine($"{hour:D2}:00  {bar,-25} {state.EnergyLevel:P0}  [{period}]");
        }
    }

    static void ShowActivitySpecificTiming(ScheduleOptimizer optimizer)
    {
        var activities = new[] { "focus", "creative", "physical", "social", "rest" };

        Console.WriteLine("\n╔═══ Optimal Times for Each Activity Type ═══╗\n");

        foreach (var activity in activities)
        {
            var optimalTime = optimizer.FindOptimalTimeForActivity(
                DateTime.Now, activity,
                new TimeSpan(6, 0, 0),
                new TimeSpan(22, 0, 0)
            );

            Console.WriteLine($"{activity,-12} → {optimalTime:hh\\:mm}");
        }
    }

    static void PrintFourLayers(MindsetState mindset, CognitiveState cognitive, BodyTimingState bodyTiming)
    {
        Console.WriteLine("LAYER 2: MINDSET");
        Console.WriteLine($"  Type: {mindset.Type} (Intensity: {mindset.Intensity:P0}, Stability: {mindset.Stability:P0})");

        Console.WriteLine("\nLAYER 3: COGNITION");
        Console.WriteLine($"  Thinking: {cognitive.PrimaryStyle}");
        Console.WriteLine($"  Clarity: {cognitive.MentalClarity:P0}, Quality: {cognitive.CognitiveQualityLevel}");

        Console.WriteLine("\nLAYER 4: BODY TIMING");
        Console.WriteLine($"  Energy: {bodyTiming.CurrentEnergyPhase} ({bodyTiming.EnergyLevel:P0})");
        Console.WriteLine($"  Alertness: {bodyTiming.AlertnessLevel:P0}");
        Console.WriteLine($"  Optimal for: {bodyTiming.OptimalActivityNow}");
    }
}
