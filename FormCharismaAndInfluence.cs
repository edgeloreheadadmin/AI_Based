using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class FormCharismaAndInfluence
{
    public class CharismaProfile
    {
        public string PersonName { get; set; }
        public double PresenceStrength { get; set; }
        public double AuthenticityScore { get; set; }
        public double EmotionalIntelligence { get; set; }
        public double VisionClarity { get; set; }
        public double CharismaIndex { get; set; }
        public List<string> CoreValues { get; set; }
        public DateTime ProfileDate { get; set; }
    }

    public class InfluenceMetric
    {
        public string MetricName { get; set; }
        public double CurrentLevel { get; set; }
        public double MaximumCapacity { get; set; }
        public int PracticeCount { get; set; }
        public List<double> GrowthHistory { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class AudienceResponse
    {
        public string AudienceSegment { get; set; }
        public int PeopleInfluenced { get; set; }
        public double ResonanceScore { get; set; }
        public List<string> KeyMessages { get; set; }
        public double ActionConversion { get; set; }
        public DateTime ResponseDate { get; set; }
    }

    public class CharismaEngine
    {
        private Dictionary<string, CharismaProfile> profiles;
        private Dictionary<string, List<InfluenceMetric>> influenceMetrics;
        private List<AudienceResponse> responses;

        public CharismaEngine()
        {
            profiles = new Dictionary<string, CharismaProfile>();
            influenceMetrics = new Dictionary<string, List<InfluenceMetric>>();
            responses = new List<AudienceResponse>();
        }

        public void CreateCharismaProfile(string name, List<string> coreValues)
        {
            var profile = new CharismaProfile
            {
                PersonName = name,
                PresenceStrength = 0.3,
                AuthenticityScore = 0.5,
                EmotionalIntelligence = 0.4,
                VisionClarity = 0.35,
                CharismaIndex = 0.0,
                CoreValues = coreValues,
                ProfileDate = DateTime.Now
            };
            profiles[name] = profile;
            influenceMetrics[name] = new List<InfluenceMetric>();
            CalculateCharismaIndex(name);
        }

        public void TrainCharismaElement(string personName, string element, double improvement)
        {
            if (!profiles.ContainsKey(personName)) return;

            var profile = profiles[personName];
            double boost = improvement * 0.15;

            switch (element.ToLower())
            {
                case "presence":
                    profile.PresenceStrength = Math.Min(profile.PresenceStrength + boost, 1.0);
                    break;
                case "authenticity":
                    profile.AuthenticityScore = Math.Min(profile.AuthenticityScore + boost, 1.0);
                    break;
                case "emotional_intelligence":
                    profile.EmotionalIntelligence = Math.Min(profile.EmotionalIntelligence + boost, 1.0);
                    break;
                case "vision":
                    profile.VisionClarity = Math.Min(profile.VisionClarity + boost, 1.0);
                    break;
            }

            CalculateCharismaIndex(personName);
        }

        private void CalculateCharismaIndex(string personName)
        {
            if (!profiles.ContainsKey(personName)) return;

            var profile = profiles[personName];
            double averageScore = (profile.PresenceStrength + profile.AuthenticityScore +
                                  profile.EmotionalIntelligence + profile.VisionClarity) / 4.0;

            double synergy = profile.PresenceStrength * profile.AuthenticityScore;
            double impact = profile.EmotionalIntelligence * profile.VisionClarity;

            profile.CharismaIndex = (averageScore * 0.5) + (synergy * 0.25) + (impact * 0.25);
        }

        public void InfluenceAudience(string personName, string segment, int peopleCount, double resonance)
        {
            if (!profiles.ContainsKey(personName)) return;

            var profile = profiles[personName];
            double amplificationFactor = 1.0 + (profile.CharismaIndex * 0.8);
            double effectiveResonance = Math.Min(resonance * amplificationFactor, 1.0);
            double conversionRate = profile.CharismaIndex * effectiveResonance;

            var response = new AudienceResponse
            {
                AudienceSegment = segment,
                PeopleInfluenced = (int)(peopleCount * effectiveResonance),
                ResonanceScore = effectiveResonance,
                KeyMessages = profile.CoreValues,
                ActionConversion = conversionRate,
                ResponseDate = DateTime.Now
            };
            responses.Add(response);
        }

        public void AddInfluenceMetric(string personName, string metricName)
        {
            if (!influenceMetrics.ContainsKey(personName))
                influenceMetrics[personName] = new List<InfluenceMetric>();

            var metric = new InfluenceMetric
            {
                MetricName = metricName,
                CurrentLevel = 0.2,
                MaximumCapacity = 1.0,
                PracticeCount = 0,
                GrowthHistory = new List<double> { 0.2 },
                LastUpdated = DateTime.Now
            };
            influenceMetrics[personName].Add(metric);
        }

        public void DevelopInfluence(string personName, string metricName, double practiceIntensity)
        {
            if (!influenceMetrics.ContainsKey(personName)) return;

            var metric = influenceMetrics[personName].FirstOrDefault(m => m.MetricName == metricName);
            if (metric == null) return;

            double growth = practiceIntensity * 0.1;
            metric.CurrentLevel = Math.Min(metric.CurrentLevel + growth, metric.MaximumCapacity);
            metric.PracticeCount++;
            metric.GrowthHistory.Add(metric.CurrentLevel);
            metric.LastUpdated = DateTime.Now;
        }

        public void DisplayCharismaProfile(string personName)
        {
            if (!profiles.ContainsKey(personName)) return;

            var profile = profiles[personName];
            Console.WriteLine($"\n  Person: {profile.PersonName}");
            Console.WriteLine($"  Charisma Index: {profile.CharismaIndex * 100:F1}%");
            Console.WriteLine($"  Presence Strength: {profile.PresenceStrength * 100:F1}%");
            Console.WriteLine($"  Authenticity: {profile.AuthenticityScore * 100:F1}%");
            Console.WriteLine($"  Emotional Intelligence: {profile.EmotionalIntelligence * 100:F1}%");
            Console.WriteLine($"  Vision Clarity: {profile.VisionClarity * 100:F1}%");
            Console.WriteLine($"  Core Values: {string.Join(", ", profile.CoreValues)}");
        }

        public void DisplayInfluenceMetrics(string personName)
        {
            if (!influenceMetrics.ContainsKey(personName)) return;

            Console.WriteLine($"\n  Influence Metrics for {personName}:");
            foreach (var metric in influenceMetrics[personName])
            {
                Console.WriteLine($"    {metric.MetricName}: {metric.CurrentLevel * 100:F1}% (Practices: {metric.PracticeCount})");
            }
        }

        public void DisplayAudienceImpact(string personName)
        {
            var personResponses = responses.Where(r => profiles.ContainsKey(personName)).ToList();
            Console.WriteLine($"\n  Audience Impact for {personName}:");
            foreach (var response in personResponses.TakeLast(5))
            {
                Console.WriteLine($"    {response.AudienceSegment}: {response.PeopleInfluenced} influenced (Resonance: {response.ResonanceScore * 100:F0}%)");
            }
        }

        public int GetTotalInfluenced(string personName)
        {
            return responses.Sum(r => r.PeopleInfluenced);
        }

        public double GetAverageResonance(string personName)
        {
            var personResponses = responses.Where(r => profiles.ContainsKey(personName)).ToList();
            return personResponses.Count > 0 ? personResponses.Average(r => r.ResonanceScore) : 0.0;
        }

        public Dictionary<string, double> GetCharismaRankings()
        {
            return profiles.OrderByDescending(x => x.Value.CharismaIndex)
                .ToDictionary(x => x.Key, x => x.Value.CharismaIndex);
        }

        public string GetInfluenceLevel(double index)
        {
            return index switch
            {
                >= 0.8 => "Legendary - Transforms societies and movements",
                >= 0.6 => "High - Influences thousands, creates change",
                >= 0.4 => "Moderate - Influences hundreds, respected leader",
                >= 0.2 => "Growing - Influences dozens, developing presence",
                _ => "Emerging - Building foundation for influence"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Forming Charisma and Influence at Greater Scale             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CharismaEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Charisma Profiles with Core Values]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateCharismaProfile("Alexandra", new List<string> { "Authenticity", "Vision", "Empathy", "Courage" });
        engine.CreateCharismaProfile("Marcus", new List<string> { "Excellence", "Trust", "Innovation", "Impact" });
        engine.CreateCharismaProfile("Elena", new List<string> { "Wisdom", "Compassion", "Integrity", "Growth" });

        Console.WriteLine("  ✓ Created 3 charisma profiles with distinct core values");
        engine.DisplayCharismaProfile("Alexandra");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Developing Core Charisma Elements]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Training Alexandra's presence and authenticity:");
        for (int i = 0; i < 5; i++)
        {
            engine.TrainCharismaElement("Alexandra", "presence", 0.8);
            engine.TrainCharismaElement("Alexandra", "authenticity", 0.9);
        }

        Console.WriteLine("  Training Marcus's emotional intelligence and vision:");
        for (int i = 0; i < 5; i++)
        {
            engine.TrainCharismaElement("Marcus", "emotional_intelligence", 0.85);
            engine.TrainCharismaElement("Marcus", "vision", 0.88);
        }

        Console.WriteLine("  Training Elena's all elements equally:");
        for (int i = 0; i < 5; i++)
        {
            engine.TrainCharismaElement("Elena", "presence", 0.8);
            engine.TrainCharismaElement("Elena", "authenticity", 0.85);
            engine.TrainCharismaElement("Elena", "emotional_intelligence", 0.82);
            engine.TrainCharismaElement("Elena", "vision", 0.87);
        }

        engine.DisplayCharismaProfile("Alexandra");
        engine.DisplayCharismaProfile("Marcus");
        engine.DisplayCharismaProfile("Elena");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Building Specific Influence Metrics]");
        Console.ResetColor();
        Thread.Sleep(500);

        foreach (var person in new[] { "Alexandra", "Marcus", "Elena" })
        {
            engine.AddInfluenceMetric(person, "Public Speaking");
            engine.AddInfluenceMetric(person, "Persuasion");
            engine.AddInfluenceMetric(person, "Networking");
            engine.AddInfluenceMetric(person, "Presence in Room");
        }

        Console.WriteLine("  ✓ Added 4 influence metrics for each person");
        engine.DisplayInfluenceMetrics("Alexandra");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Developing Influence Through Deliberate Practice]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Developing influences through practice:");
        for (int round = 0; round < 8; round++)
        {
            engine.DevelopInfluence("Alexandra", "Public Speaking", 0.9);
            engine.DevelopInfluence("Marcus", "Persuasion", 0.95);
            engine.DevelopInfluence("Elena", "Presence in Room", 0.92);
        }

        engine.DisplayInfluenceMetrics("Alexandra");
        engine.DisplayInfluenceMetrics("Marcus");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Influencing Audiences at Scale]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Influencing different audience segments:");
        engine.InfluenceAudience("Alexandra", "Tech Entrepreneurs", 500, 0.85);
        engine.InfluenceAudience("Alexandra", "Business Leaders", 300, 0.90);

        engine.InfluenceAudience("Marcus", "Corporate Teams", 400, 0.88);
        engine.InfluenceAudience("Marcus", "Startup Community", 250, 0.92);

        engine.InfluenceAudience("Elena", "Educational Institution", 600, 0.87);
        engine.InfluenceAudience("Elena", "Non-profit Leaders", 350, 0.91);

        engine.DisplayAudienceImpact("Alexandra");
        engine.DisplayAudienceImpact("Marcus");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Comparative Charisma Rankings]");
        Console.ResetColor();
        Thread.Sleep(500);

        var rankings = engine.GetCharismaRankings();
        int rank = 1;
        foreach (var kvp in rankings)
        {
            string level = engine.GetInfluenceLevel(kvp.Value);
            double avgResonance = engine.GetAverageResonance(kvp.Key);
            Console.WriteLine($"  #{rank}: {kvp.Key}");
            Console.WriteLine($"       Index: {kvp.Value * 100:F1}% | Avg Resonance: {avgResonance * 100:F1}%");
            Console.WriteLine($"       Level: {level}");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Charisma Development Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  The Four Pillars of Charisma:");
        Console.WriteLine("    1. Presence Strength (0-100%): Magnetic personal presence");
        Console.WriteLine("    2. Authenticity (0-100%): Genuine alignment with values");
        Console.WriteLine("    3. Emotional Intelligence (0-100%): Understanding others' emotions");
        Console.WriteLine("    4. Vision Clarity (0-100%): Clear compelling purpose");
        Console.WriteLine("\n  Influence Scaling Mechanism:");
        Console.WriteLine("    • Synergy Effect: Presence × Authenticity multiplier");
        Console.WriteLine("    • Impact Effect: Emotional Intelligence × Vision multiplier");
        Console.WriteLine("    • Amplification: Charisma Index amplifies audience resonance 1.0-1.8x");
        Console.WriteLine("    • Conversion: Charisma × Resonance = Action taken");
        Console.WriteLine("\n  Influence Levels:");
        Console.WriteLine("    Emerging (0-20%) → Growing (20-40%) → Moderate (40-60%)");
        Console.WriteLine("    → High (60-80%) → Legendary (80%+)");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Charisma and influence system complete");
        Console.ResetColor();
    }
}
