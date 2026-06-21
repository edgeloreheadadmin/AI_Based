using System;
using System.Collections.Generic;
using System.Linq;

namespace BiorhythmDecisionSystem
{
    /// <summary>
    /// Chronotype determines natural sleep/wake preferences
    /// </summary>
    public enum Chronotype
    {
        ExtremeEarlyBird,    // Sleep 8pm-4am, peak 6-10am
        Lark,                // Sleep 10pm-6am, peak 7-11am
        Intermediate,        // Sleep 11pm-7am, balanced
        Owl,                 // Sleep 1am-8am, peak 10pm-2am
        ExtremeNightOwl      // Sleep 3am-10am, peak midnight-4am
    }

    /// <summary>
    /// Energy levels throughout the day (ultradian rhythms)
    /// </summary>
    public enum EnergyPhase
    {
        Peak,           // Maximum energy available
        High,           // Above average energy
        Moderate,       // Standard baseline
        Low,            // Below average, declining
        Trough,         // Minimum energy, recovery needed
        Rising          // Energy increasing after trough
    }

    /// <summary>
    /// Represents the body's circadian and ultradian state
    /// </summary>
    public class BodyTimingState
    {
        public Chronotype UserChronotype { get; set; }
        public double CircadianPhase { get; set; } // 0-1, where 0=midnight, 0.5=noon
        public EnergyPhase CurrentEnergyPhase { get; set; }
        public double EnergyLevel { get; set; } // 0-1
        public double BodyTemperature { get; set; } // Relative to baseline
        public double AlertnessLevel { get; set; } // 0-1
        public double CortisolLevel { get; set; } // Relative (high in morning, low evening)
        public double MelatoninLevel { get; set; } // Relative (low in day, high at night)
        public bool IsOptimalForFocusedWork { get; set; }
        public bool IsOptimalForCreativeWork { get; set; }
        public bool IsOptimalForPhysicalActivity { get; set; }
        public bool IsOptimalForSocialInteraction { get; set; }
        public bool IsOptimalForRestRecovery { get; set; }
        public int MinutesUntilNextPeak { get; set; }
        public int MinutesUntilNextTrough { get; set; }
        public string OptimalActivityNow { get; set; }
    }

    /// <summary>
    /// Time block recommendation for scheduling
    /// </summary>
    public class TimeBlockRecommendation
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<string> OptimalActivities { get; set; }
        public double EnergyLevel { get; set; }
        public double FocusQuality { get; set; }
        public double CreativityLevel { get; set; }
        public string BodyState { get; set; } // Peak, High, Moderate, Low, Trough
    }

    /// <summary>
    /// Analyzes circadian rhythms and body timing patterns
    /// </summary>
    public class BodyTimingAnalyzer
    {
        private readonly Chronotype _userChronotype;
        private readonly DateTime _birthDate;

        public BodyTimingAnalyzer(DateTime birthDate, Chronotype chronotype = Chronotype.Intermediate)
        {
            _birthDate = birthDate;
            _userChronotype = chronotype;
        }

        /// <summary>
        /// Analyze body timing state at a specific time
        /// </summary>
        public BodyTimingState AnalyzeBodyTiming(DateTime targetTime)
        {
            var circadianPhase = CalculateCircadianPhase(targetTime);
            var energyPhase = DetermineEnergyPhase(circadianPhase);
            var energyLevel = CalculateEnergyLevel(circadianPhase);

            var bodyState = new BodyTimingState
            {
                UserChronotype = _userChronotype,
                CircadianPhase = circadianPhase,
                CurrentEnergyPhase = energyPhase,
                EnergyLevel = energyLevel,
                BodyTemperature = CalculateBodyTemperature(circadianPhase),
                AlertnessLevel = CalculateAlertness(circadianPhase),
                CortisolLevel = CalculateCortisol(circadianPhase),
                MelatoninLevel = CalculateMelatonin(circadianPhase),
                IsOptimalForFocusedWork = energyPhase == EnergyPhase.Peak || energyPhase == EnergyPhase.High,
                IsOptimalForCreativeWork = energyPhase == EnergyPhase.High || energyPhase == EnergyPhase.Moderate,
                IsOptimalForPhysicalActivity = energyPhase == EnergyPhase.Peak || energyPhase == EnergyPhase.High,
                IsOptimalForSocialInteraction = energyLevel > 0.5,
                IsOptimalForRestRecovery = energyPhase == EnergyPhase.Trough || energyPhase == EnergyPhase.Low,
                MinutesUntilNextPeak = CalculateMinutesUntilNextPeak(circadianPhase),
                MinutesUntilNextTrough = CalculateMinutesUntilNextTrough(circadianPhase),
                OptimalActivityNow = GetOptimalActivityForTimeOfDay(targetTime, energyPhase)
            };

            return bodyState;
        }

        private double CalculateCircadianPhase(DateTime targetTime)
        {
            // 0 = midnight, 0.5 = noon, 1 = next midnight
            var hoursSinceMidnight = targetTime.Hour + (targetTime.Minute / 60.0);
            return hoursSinceMidnight / 24.0;
        }

        private EnergyPhase DetermineEnergyPhase(double circadianPhase)
        {
            // Normalize based on chronotype
            var adjustedPhase = AdjustPhaseForChronotype(circadianPhase);

            // Energy naturally follows circadian curve
            // Peak around 10-12 hours after wake, low before sleep
            return adjustedPhase switch
            {
                // Morning (wake to +4h) - rising cortisol, increasing energy
                >= 0 and < 0.17 => EnergyPhase.Rising,  // 0-4am
                >= 0.17 and < 0.33 => EnergyPhase.Peak,  // 4-8am
                >= 0.33 and < 0.42 => EnergyPhase.High,  // 8-10am

                // Late morning to early afternoon - peak performance
                >= 0.42 and < 0.58 => EnergyPhase.Peak,  // 10am-2pm (PRIMARY PEAK)

                // Afternoon - post-lunch dip
                >= 0.58 and < 0.67 => EnergyPhase.Moderate,  // 2-4pm

                // Late afternoon - recovery
                >= 0.67 and < 0.75 => EnergyPhase.Rising,  // 4-6pm (SECOND PEAK INCOMING)

                // Evening - secondary peak
                >= 0.75 and < 0.83 => EnergyPhase.High,   // 6-8pm

                // Night - declining
                >= 0.83 and < 0.92 => EnergyPhase.Moderate,  // 8-10pm
                >= 0.92 => EnergyPhase.Low,  // 10pm-midnight

                < 0 => EnergyPhase.Trough,
                _ => EnergyPhase.Moderate
            };
        }

        private double AdjustPhaseForChronotype(double circadianPhase)
        {
            // Shift phase based on chronotype
            return _userChronotype switch
            {
                Chronotype.ExtremeEarlyBird => (circadianPhase + 0.5) % 1.0,  // Shift 12 hours earlier
                Chronotype.Lark => (circadianPhase + 0.33) % 1.0,             // Shift 8 hours earlier
                Chronotype.Intermediate => circadianPhase,                     // No shift
                Chronotype.Owl => (circadianPhase - 0.33 + 1.0) % 1.0,        // Shift 8 hours later
                Chronotype.ExtremeNightOwl => (circadianPhase - 0.5 + 1.0) % 1.0, // Shift 12 hours later
                _ => circadianPhase
            };
        }

        private double CalculateEnergyLevel(double circadianPhase)
        {
            // Cosine function approximates circadian energy rhythm
            // Peak around 10-12 hours after wake, trough at wake time
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);
            double sine = Math.Sin(2 * Math.PI * (adjustedPhase - 0.25)); // Shift so peak is at 0.4
            return (sine + 1) / 2; // Normalize to 0-1
        }

        private double CalculateBodyTemperature(double circadianPhase)
        {
            // Body temperature peaks in late afternoon, lowest at 4-5am
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);
            return Math.Sin(2 * Math.PI * (adjustedPhase - 0.25));
        }

        private double CalculateAlertness(double circadianPhase)
        {
            // Peaks mid-morning, dips after lunch, rises evening, crashes at night
            return CalculateEnergyLevel(circadianPhase);
        }

        private double CalculateCortisol(double circadianPhase)
        {
            // High in early morning (peak 30-45 min after waking), declining through day
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);
            double cortisol = Math.Cos(2 * Math.PI * adjustedPhase) * 0.7 + 0.3;
            return Math.Max(0, cortisol); // Cortisol doesn't go negative
        }

        private double CalculateMelatonin(double circadianPhase)
        {
            // Low during day, rises evening, peaks at night, drops at morning
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);
            double melatonin = Math.Cos(2 * Math.PI * (adjustedPhase + 0.5)); // Opposite of cortisol
            return (melatonin + 1) / 2; // Normalize to 0-1
        }

        private int CalculateMinutesUntilNextPeak(double circadianPhase)
        {
            // Primary peak typically 10am-2pm, secondary peak 6-8pm
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);

            // If before primary peak
            if (adjustedPhase < 0.42)
                return (int)((0.42 - adjustedPhase) * 24 * 60);
            // If in primary peak
            if (adjustedPhase < 0.58)
                return 0;
            // If between peaks
            if (adjustedPhase < 0.75)
                return (int)((0.75 - adjustedPhase) * 24 * 60);
            // If after secondary peak
            return (int)((1.42 - adjustedPhase) * 24 * 60);
        }

        private int CalculateMinutesUntilNextTrough(double circadianPhase)
        {
            // Primary trough typically 3-5am, secondary trough 2-4pm
            double adjustedPhase = AdjustPhaseForChronotype(circadianPhase);

            // Approximate next trough
            if (adjustedPhase < 0.25)
                return (int)((0.25 - adjustedPhase) * 24 * 60);
            if (adjustedPhase < 0.58)
                return (int)((0.58 - adjustedPhase) * 24 * 60);

            return (int)((1.25 - adjustedPhase) * 24 * 60);
        }

        private string GetOptimalActivityForTimeOfDay(DateTime targetTime, EnergyPhase phase)
        {
            var hour = targetTime.Hour;

            return (hour, phase) switch
            {
                // Early morning (4am-8am): Wake up, high cortisol
                (>= 4 and < 8, _) => "Morning routine, light exercise, planning",

                // Mid-morning (8am-12pm): Primary peak
                (>= 8 and < 12, EnergyPhase.Peak or EnergyPhase.High) => "Focused work, difficult problems, deep learning",

                // Noon-2pm: Sustained peak
                (>= 12 and < 14, EnergyPhase.Peak) => "Strategic decisions, complex analysis, important meetings",

                // Afternoon dip (2pm-4pm): Post-lunch energy crash
                (>= 14 and < 16, EnergyPhase.Low or EnergyPhase.Moderate) => "Light tasks, exercise, outdoor time",

                // Late afternoon (4pm-6pm): Secondary peak rising
                (>= 16 and < 18, EnergyPhase.Rising or EnergyPhase.High) => "Creative work, collaboration, team activities",

                // Evening (6pm-8pm): Secondary peak, high creativity
                (>= 18 and < 20, EnergyPhase.High) => "Creative projects, social activities, hobbies",

                // Late evening (8pm-10pm): Declining
                (>= 20 and < 22, EnergyPhase.Moderate) => "Leisure, light reading, relationships",

                // Night (10pm-midnight): Melatonin rising
                (>= 22 or < 4, EnergyPhase.Low or EnergyPhase.Trough) => "Wind down, prepare for sleep, reflection",

                _ => "Flexible time, based on energy level"
            };
        }

        public string GetDetailedTimingReport(DateTime targetTime)
        {
            var state = AnalyzeBodyTiming(targetTime);
            var report = new System.Text.StringBuilder();

            report.AppendLine($"╔═══ Body Timing Report for {targetTime:h:mm tt} ═══╗\n");

            report.AppendLine($"Chronotype:       {state.UserChronotype}");
            report.AppendLine($"Circadian Phase:  {state.CircadianPhase:P0} through 24-hour cycle");
            report.AppendLine($"Energy Phase:     {state.CurrentEnergyPhase}");
            report.AppendLine($"Energy Level:     {state.EnergyLevel:P0}\n");

            report.AppendLine($"Biological Markers:");
            report.AppendLine($"  Body Temp:      {state.BodyTemperature:+0.00;-0.00} (relative)");
            report.AppendLine($"  Alertness:      {state.AlertnessLevel:P0}");
            report.AppendLine($"  Cortisol:       {state.CortisolLevel:P0} (peak morning)");
            report.AppendLine($"  Melatonin:      {state.MelatoninLevel:P0} (peak evening)\n");

            report.AppendLine($"Activity Suitability:");
            report.AppendLine($"  Focused Work:   {(state.IsOptimalForFocusedWork ? "✓ OPTIMAL" : "✗ Not ideal")}");
            report.AppendLine($"  Creative Work:  {(state.IsOptimalForCreativeWork ? "✓ OPTIMAL" : "✗ Not ideal")}");
            report.AppendLine($"  Physical:       {(state.IsOptimalForPhysicalActivity ? "✓ OPTIMAL" : "✗ Not ideal")}");
            report.AppendLine($"  Social:         {(state.IsOptimalForSocialInteraction ? "✓ OPTIMAL" : "✗ Not ideal")}");
            report.AppendLine($"  Rest/Recovery:  {(state.IsOptimalForRestRecovery ? "✓ OPTIMAL" : "✗ Not ideal")}\n");

            report.AppendLine($"Timing Information:");
            report.AppendLine($"  Next Peak:      ~{state.MinutesUntilNextPeak} minutes");
            report.AppendLine($"  Next Trough:    ~{state.MinutesUntilNextTrough} minutes");
            report.AppendLine($"  Recommended:    {state.OptimalActivityNow}");

            return report.ToString();
        }
    }

    /// <summary>
    /// Optimizes daily schedule based on body timing and activities
    /// </summary>
    public class ScheduleOptimizer
    {
        private readonly BodyTimingAnalyzer _timingAnalyzer;
        private readonly List<(string activity, TimeSpan duration, string type)> _activities;

        public ScheduleOptimizer(DateTime birthDate, Chronotype chronotype = Chronotype.Intermediate)
        {
            _timingAnalyzer = new BodyTimingAnalyzer(birthDate, chronotype);
            _activities = new List<(string, TimeSpan, string)>();
        }

        /// <summary>
        /// Add activity to schedule
        /// </summary>
        public void AddActivity(string activityName, TimeSpan duration, string activityType)
        {
            // Types: "focus", "creative", "physical", "social", "admin", "rest"
            _activities.Add((activityName, duration, activityType));
        }

        /// <summary>
        /// Generate optimized daily schedule
        /// </summary>
        public List<TimeBlockRecommendation> GenerateOptimalSchedule(DateTime targetDate, TimeSpan workStartTime, TimeSpan workEndTime)
        {
            var schedule = new List<TimeBlockRecommendation>();
            var currentTime = targetDate.Date.Add(workStartTime);
            var endTime = targetDate.Date.Add(workEndTime);

            while (currentTime < endTime)
            {
                var bodyState = _timingAnalyzer.AnalyzeBodyTiming(currentTime);

                // Categorize block quality
                string blockQuality = bodyState.CurrentEnergyPhase switch
                {
                    EnergyPhase.Peak => "Peak",
                    EnergyPhase.High => "High",
                    EnergyPhase.Moderate => "Moderate",
                    EnergyPhase.Low => "Low",
                    EnergyPhase.Trough => "Trough",
                    EnergyPhase.Rising => "High",
                    _ => "Moderate"
                };

                var focusQuality = bodyState.IsOptimalForFocusedWork ? bodyState.EnergyLevel : bodyState.EnergyLevel * 0.6;
                var creativityLevel = bodyState.IsOptimalForCreativeWork ? bodyState.EnergyLevel * 0.9 : bodyState.EnergyLevel * 0.5;

                var block = new TimeBlockRecommendation
                {
                    StartTime = currentTime.TimeOfDay,
                    EndTime = currentTime.AddHours(1).TimeOfDay,
                    EnergyLevel = bodyState.EnergyLevel,
                    FocusQuality = focusQuality,
                    CreativityLevel = creativityLevel,
                    BodyState = blockQuality,
                    OptimalActivities = GetOptimalActivitiesForBlock(bodyState)
                };

                schedule.Add(block);
                currentTime = currentTime.AddHours(1);
            }

            return schedule;
        }

        private List<string> GetOptimalActivitiesForBlock(BodyTimingState state)
        {
            var activities = new List<string>();

            if (state.IsOptimalForFocusedWork)
                activities.Add("Deep focused work");
            if (state.IsOptimalForCreativeWork)
                activities.Add("Creative projects");
            if (state.IsOptimalForPhysicalActivity)
                activities.Add("Exercise/physical tasks");
            if (state.IsOptimalForSocialInteraction)
                activities.Add("Meetings/collaboration");
            if (state.IsOptimalForRestRecovery)
                activities.Add("Rest/recovery/light tasks");

            return activities.Count > 0 ? activities : new List<string> { "Flexible/admin work" };
        }

        /// <summary>
        /// Get ideal time to schedule specific activity
        /// </summary>
        public TimeSpan FindOptimalTimeForActivity(DateTime targetDate, string activityType, TimeSpan searchStart, TimeSpan searchEnd)
        {
            // activityType: "focus", "creative", "physical", "social", "rest"
            var currentTime = targetDate.Date.Add(searchStart);
            var endTime = targetDate.Date.Add(searchEnd);
            var bestTime = currentTime;
            double bestScore = 0;

            while (currentTime < endTime)
            {
                var state = _timingAnalyzer.AnalyzeBodyTiming(currentTime);
                double score = 0;

                switch (activityType.ToLower())
                {
                    case "focus":
                        score = state.IsOptimalForFocusedWork ? state.EnergyLevel : state.EnergyLevel * 0.5;
                        break;
                    case "creative":
                        score = state.IsOptimalForCreativeWork ? state.EnergyLevel : state.EnergyLevel * 0.4;
                        break;
                    case "physical":
                        score = state.IsOptimalForPhysicalActivity ? state.EnergyLevel : state.EnergyLevel * 0.3;
                        break;
                    case "social":
                        score = state.IsOptimalForSocialInteraction ? state.EnergyLevel : state.EnergyLevel * 0.5;
                        break;
                    case "rest":
                        score = state.IsOptimalForRestRecovery ? 1.0 - state.EnergyLevel : state.EnergyLevel * 0.2;
                        break;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTime = currentTime;
                }

                currentTime = currentTime.AddHours(1);
            }

            return bestTime.TimeOfDay;
        }

        /// <summary>
        /// Get full day schedule visualization
        /// </summary>
        public string VisualizeDailySchedule(DateTime targetDate, TimeSpan workStart, TimeSpan workEnd)
        {
            var schedule = GenerateOptimalSchedule(targetDate, workStart, workEnd);
            var visualization = new System.Text.StringBuilder();

            visualization.AppendLine("╔═══ Optimized Daily Schedule ═══╗\n");

            foreach (var block in schedule)
            {
                var startHour = block.StartTime.Hours;
                var startMin = block.StartTime.Minutes;
                var endHour = block.EndTime.Hours;
                var endMin = block.EndTime.Minutes;

                var energyBar = new string('█', (int)(block.EnergyLevel * 20));
                var focusBar = new string('▓', (int)(block.FocusQuality * 15));
                var creativityBar = new string('░', (int)(block.CreativityLevel * 15));

                visualization.AppendLine($"{startHour:D2}:{startMin:D2}-{endHour:D2}:{endMin:D2} [{block.BodyState,-8}]");
                visualization.AppendLine($"  Energy:    {energyBar,-20} {block.EnergyLevel:P0}");
                visualization.AppendLine($"  Focus:     {focusBar,-15} {block.FocusQuality:P0}");
                visualization.AppendLine($"  Creativity: {creativityBar,-15} {block.CreativityLevel:P0}");
                visualization.AppendLine($"  Best for:  {string.Join(", ", block.OptimalActivities)}");
                visualization.AppendLine();
            }

            return visualization.ToString();
        }
    }

    /// <summary>
    /// Complete four-layer decision system with body timing alignment
    /// </summary>
    public class FourLayerDecisionMaker
    {
        private readonly BiorhythmCalculator _biorhythm;
        private readonly MindsetAlignmentAnalyzer _mindset;
        private readonly CognitiveStateAnalyzer _cognitive;
        private readonly BodyTimingAnalyzer _bodyTiming;

        public FourLayerDecisionMaker(DateTime birthDate, Chronotype chronotype = Chronotype.Intermediate)
        {
            _biorhythm = new BiorhythmCalculator(birthDate);
            _mindset = new MindsetAlignmentAnalyzer(birthDate);
            _cognitive = new CognitiveStateAnalyzer(birthDate);
            _bodyTiming = new BodyTimingAnalyzer(birthDate, chronotype);
        }

        /// <summary>
        /// Get complete four-layer aligned decision recommendation
        /// </summary>
        public (DecisionRecommendation decision, MindsetState mindset, CognitiveState cognitive, BodyTimingState bodyTiming)
            GetFullyAlignedRecommendation(string decisionType, DateTime targetTime)
        {
            var profile = _biorhythm.GetProfile(targetTime);
            var mindsetState = _mindset.AnalyzeMindset(targetTime);
            var cognitiveState = _cognitive.AnalyzeCognitiveState(targetTime);
            var bodyState = _bodyTiming.AnalyzeBodyTiming(targetTime);

            // Start with base biorhythm decision
            var baseEngine = new BiorhythmicDecisionEngine(_biorhythm._birthDate);
            var baseDecision = baseEngine.GetDecisionRecommendation(decisionType, targetTime);

            // Apply layer 2 adjustment (mindset)
            double mindsetAdjustment = 0.7 + (0.3 * CalculateMindsetAlignment(decisionType, mindsetState));
            baseDecision.RecommendationScore *= mindsetAdjustment;

            // Apply layer 3 adjustment (cognition)
            double cognitiveAdjustment = 0.8 + (0.2 * CalculateCognitiveAlignment(decisionType, cognitiveState));
            baseDecision.RecommendationScore *= cognitiveAdjustment;

            // Apply layer 4 adjustment (body timing)
            double bodyTimingAdjustment = CalculateBodyTimingAlignment(decisionType, bodyState);
            baseDecision.RecommendationScore *= bodyTimingAdjustment;

            // Cap at 100
            baseDecision.RecommendationScore = Math.Min(100, baseDecision.RecommendationScore);

            // Add four-layer insights
            var fourLayerInsights = GenerateFourLayerInsights(decisionType, mindsetState, cognitiveState, bodyState);
            baseDecision.Warnings.InsertRange(0, fourLayerInsights);

            return (baseDecision, mindsetState, cognitiveState, bodyState);
        }

        private double CalculateMindsetAlignment(string decisionType, MindsetState mindset)
        {
            return mindset.Stability > 0.6 ? 0.8 : 0.4;
        }

        private double CalculateCognitiveAlignment(string decisionType, CognitiveState cognitive)
        {
            return cognitive.MentalClarity > 0.7 ? 0.85 : 0.5;
        }

        private double CalculateBodyTimingAlignment(string decisionType, BodyTimingState bodyState)
        {
            return decisionType.ToLower() switch
            {
                "analytical" => bodyState.IsOptimalForFocusedWork ? 1.15 : 0.85,
                "creative" => bodyState.IsOptimalForCreativeWork ? 1.15 : 0.8,
                "athletic" => bodyState.IsOptimalForPhysicalActivity ? 1.2 : 0.7,
                "business" => bodyState.EnergyLevel > 0.6 ? 1.1 : 0.85,
                "interpersonal" => bodyState.IsOptimalForSocialInteraction ? 1.1 : 0.85,
                "health" => bodyState.IsOptimalForPhysicalActivity ? 1.05 : 0.95,
                _ => 1.0
            };
        }

        private List<string> GenerateFourLayerInsights(string decisionType, MindsetState mindset,
            CognitiveState cognitive, BodyTimingState bodyTiming)
        {
            var insights = new List<string>();

            // Body timing insights
            if (bodyTiming.CurrentEnergyPhase == EnergyPhase.Peak)
                insights.Add("🔋 BODY PEAK - Physical energy at maximum");
            else if (bodyTiming.CurrentEnergyPhase == EnergyPhase.Trough)
                insights.Add("🔋 BODY TROUGH - Physical recovery needed");

            if (bodyTiming.IsOptimalForFocusedWork && cognitive.MentalClarity > 0.75)
                insights.Add("⚡ OPTIMAL FOR FOCUS - Body and mind perfectly aligned");

            if (!bodyTiming.IsOptimalForPhysicalActivity && decisionType.ToLower() == "athletic")
                insights.Add("⚠️ BODY TIMING: Not optimal for athletic activity - consider rescheduling");

            if (bodyTiming.MinutesUntilNextPeak < 60 && bodyTiming.MinutesUntilNextPeak > 0)
                insights.Add($"📊 Peak window incoming in {bodyTiming.MinutesUntilNextPeak} minutes");

            return insights;
        }

        /// <summary>
        /// Get complete system alignment report
        /// </summary>
        public string GetCompleteAlignmentReport(DateTime targetTime)
        {
            var report = new System.Text.StringBuilder();

            var profile = _biorhythm.GetProfile(targetTime);
            var mindset = _mindset.AnalyzeMindset(targetTime);
            var cognitive = _cognitive.AnalyzeCognitiveState(targetTime);
            var bodyTiming = _bodyTiming.AnalyzeBodyTiming(targetTime);

            report.AppendLine("╔════════ FOUR-LAYER ALIGNMENT REPORT ════════╗\n");
            report.AppendLine($"Time: {targetTime:h:mm tt}\n");

            report.AppendLine("LAYER 1: BIORHYTHM CYCLES");
            report.AppendLine($"  Physical:     {profile.Physical.Value:+0.00;-0.00} ({profile.Physical.Phase})");
            report.AppendLine($"  Emotional:    {profile.Emotional.Value:+0.00;-0.00} ({profile.Emotional.Phase})");
            report.AppendLine($"  Intellectual: {profile.Intellectual.Value:+0.00;-0.00} ({profile.Intellectual.Phase})");
            report.AppendLine($"  Harmonic:     {profile.HarmonicIndex:P0}\n");

            report.AppendLine("LAYER 2: PSYCHOLOGICAL MINDSET");
            report.AppendLine($"  Type:      {mindset.Type}");
            report.AppendLine($"  Intensity: {mindset.Intensity:P0}");
            report.AppendLine($"  Stability: {mindset.Stability:P0}\n");

            report.AppendLine("LAYER 3: COGNITIVE THINKING");
            report.AppendLine($"  Primary:   {cognitive.PrimaryStyle}");
            report.AppendLine($"  Clarity:   {cognitive.MentalClarity:P0}");
            report.AppendLine($"  Flow:      {(cognitive.InFlowState ? "YES 🔥" : "No")}\n");

            report.AppendLine("LAYER 4: BODY TIMING");
            report.AppendLine($"  Chronotype:    {bodyTiming.UserChronotype}");
            report.AppendLine($"  Energy Phase:  {bodyTiming.CurrentEnergyPhase}");
            report.AppendLine($"  Energy Level:  {bodyTiming.EnergyLevel:P0}");
            report.AppendLine($"  Alertness:     {bodyTiming.AlertnessLevel:P0}");
            report.AppendLine($"  Cortisol:      {bodyTiming.CortisolLevel:P0} (high=morning)");
            report.AppendLine($"  Melatonin:     {bodyTiming.MelatoninLevel:P0} (high=night)\n");

            report.AppendLine("FOUR-LAYER ALIGNMENT SCORE");
            double totalAlignment = (profile.HarmonicIndex + mindset.Stability + cognitive.MentalClarity + bodyTiming.EnergyLevel) / 4;
            report.AppendLine($"  Average:   {totalAlignment:P0}");
            report.AppendLine($"  Status:    {GetAlignmentStatus(totalAlignment)}\n");

            report.AppendLine("RECOMMENDATIONS");
            if (totalAlignment > 0.85)
                report.AppendLine("  ✓✓ PEAK ALIGNMENT - Optimal for any decision");
            else if (totalAlignment > 0.75)
                report.AppendLine("  ✓ EXCELLENT - Great conditions for decisions");
            else if (totalAlignment > 0.65)
                report.AppendLine("  ◐ GOOD - Acceptable conditions");
            else if (totalAlignment > 0.5)
                report.AppendLine("  ○ FAIR - Proceed with caution");
            else
                report.AppendLine("  ✗ POOR - Defer non-urgent decisions");

            report.AppendLine($"\n  Best activities now: {bodyTiming.OptimalActivityNow}");

            return report.ToString();
        }

        private string GetAlignmentStatus(double alignment)
        {
            return alignment switch
            {
                > 0.85 => "🔥 PEAK - All systems optimal",
                > 0.75 => "✓ EXCELLENT - Strong alignment",
                > 0.65 => "◐ GOOD - Acceptable alignment",
                > 0.50 => "○ FAIR - Suboptimal alignment",
                _ => "✗ POOR - Misaligned conditions"
            };
        }
    }
}
