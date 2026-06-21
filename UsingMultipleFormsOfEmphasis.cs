using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class UsingMultipleFormsOfEmphasis
{
    public class EmphasisTechnique
    {
        public string TechniqueId { get; set; }
        public string TechniqueName { get; set; }
        public string Description { get; set; }
        public double IntensityRange { get; set; }
        public string PhysicalMarker { get; set; }
        public string AcousticMarker { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class StressPattern
    {
        public string PatternId { get; set; }
        public string Content { get; set; }
        public List<int> StressedSyllables { get; set; }
        public double StressIntensity { get; set; }
        public string StressType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class EmphasisApplication
    {
        public string ApplicationId { get; set; }
        public string OriginalText { get; set; }
        public Dictionary<string, string> EmphasisVariations { get; set; }
        public string PrimaryEmphasis { get; set; }
        public double EmphasisStrength { get; set; }
        public List<string> AppliedTechniques { get; set; }
        public DateTime AppliedDate { get; set; }
    }

    public class EmphasisProfile
    {
        public string ProfileId { get; set; }
        public string SpeakerId { get; set; }
        public Dictionary<string, double> TechniquePreferences { get; set; }
        public double AverageEmphasisIntensity { get; set; }
        public int FrequentlyUsedTechniques { get; set; }
        public string PreferredEmphasisStyle { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class EmphasisEngine
    {
        private Dictionary<string, EmphasisTechnique> techniques;
        private Dictionary<string, StressPattern> patterns;
        private Dictionary<string, EmphasisApplication> applications;
        private Dictionary<string, EmphasisProfile> profiles;

        public EmphasisEngine()
        {
            techniques = new Dictionary<string, EmphasisTechnique>();
            patterns = new Dictionary<string, StressPattern>();
            applications = new Dictionary<string, EmphasisApplication>();
            profiles = new Dictionary<string, EmphasisProfile>();
        }

        public void RegisterTechnique(string techniqueId, string techniqueName, string description,
                                     double intensityRange, string physical, string acoustic)
        {
            var technique = new EmphasisTechnique
            {
                TechniqueId = techniqueId,
                TechniqueName = techniqueName,
                Description = description,
                IntensityRange = intensityRange,
                PhysicalMarker = physical,
                AcousticMarker = acoustic,
                CreatedDate = DateTime.Now
            };
            techniques[techniqueId] = technique;
        }

        public void RegisterStressPattern(string patternId, string content, List<int> syllables, double intensity, string type)
        {
            var pattern = new StressPattern
            {
                PatternId = patternId,
                Content = content,
                StressedSyllables = new List<int>(syllables),
                StressIntensity = intensity,
                StressType = type,
                CreatedDate = DateTime.Now
            };
            patterns[patternId] = pattern;
        }

        public void ApplyEmphasis(string applicationId, string text, List<string> techniqueIds)
        {
            var application = new EmphasisApplication
            {
                ApplicationId = applicationId,
                OriginalText = text,
                EmphasisVariations = new Dictionary<string, string>(),
                PrimaryEmphasis = "",
                EmphasisStrength = 0.0,
                AppliedTechniques = new List<string>(),
                AppliedDate = DateTime.Now
            };

            double totalIntensity = 0.0;
            int count = 0;

            foreach (var techniqueId in techniqueIds)
            {
                if (techniques.ContainsKey(techniqueId))
                {
                    var technique = techniques[techniqueId];
                    string emphasized = ApplyTechnique(text, techniqueId);
                    application.EmphasisVariations[techniqueId] = emphasized;
                    application.AppliedTechniques.Add(technique.TechniqueName);
                    totalIntensity += technique.IntensityRange;
                    count++;
                }
            }

            if (count > 0)
            {
                application.PrimaryEmphasis = application.AppliedTechniques.First();
                application.EmphasisStrength = totalIntensity / count;
            }

            applications[applicationId] = application;
        }

        private string ApplyTechnique(string text, string techniqueId)
        {
            if (!techniques.ContainsKey(techniqueId)) return text;

            var technique = techniques[techniqueId];

            if (techniqueId == "TECH-001")
                return text.ToUpper();
            else if (techniqueId == "TECH-002")
                return $"**{text}**";
            else if (techniqueId == "TECH-003")
                return $"*{text}*";
            else if (techniqueId == "TECH-004")
                return $"...{text}...";
            else
                return text;
        }

        public void CreateEmphasisProfile(string profileId, string speakerId, Dictionary<string, double> prefs)
        {
            var profile = new EmphasisProfile
            {
                ProfileId = profileId,
                SpeakerId = speakerId,
                TechniquePreferences = new Dictionary<string, double>(prefs),
                AverageEmphasisIntensity = prefs.Values.Average(),
                FrequentlyUsedTechniques = prefs.Count(x => x.Value > 0.6),
                PreferredEmphasisStyle = prefs.OrderByDescending(x => x.Value).First().Key,
                CreatedDate = DateTime.Now
            };
            profiles[profileId] = profile;
        }

        public void DisplayTechnique(string techniqueId)
        {
            if (!techniques.ContainsKey(techniqueId)) return;

            var technique = techniques[techniqueId];
            Console.WriteLine($"\n  Emphasis Technique: {technique.TechniqueName}");
            Console.WriteLine($"  Description: {technique.Description}");
            Console.WriteLine($"  Intensity Range: {technique.IntensityRange * 100:F0}%");
            Console.WriteLine($"  Physical Marker: {technique.PhysicalMarker}");
            Console.WriteLine($"  Acoustic Marker: {technique.AcousticMarker}");
        }

        public void DisplayApplication(string applicationId)
        {
            if (!applications.ContainsKey(applicationId)) return;

            var application = applications[applicationId];
            Console.WriteLine($"\n  Emphasis Application: {application.ApplicationId}");
            Console.WriteLine($"  Original: {application.OriginalText}");
            Console.WriteLine($"  Techniques Applied: {string.Join(", ", application.AppliedTechniques)}");
            Console.WriteLine($"  Overall Emphasis Strength: {application.EmphasisStrength * 100:F1}%");
        }

        public void DisplayProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            Console.WriteLine($"\n  Emphasis Profile: {profile.SpeakerId}");
            Console.WriteLine($"  Preferred Style: {profile.PreferredEmphasisStyle}");
            Console.WriteLine($"  Average Intensity: {profile.AverageEmphasisIntensity * 100:F1}%");
            Console.WriteLine($"  Frequently Used Techniques: {profile.FrequentlyUsedTechniques}");
        }

        public int GetTotalTechniques()
        {
            return techniques.Count;
        }

        public int GetTotalApplications()
        {
            return applications.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           Using Multiple Forms of Emphasis                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new EmphasisEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Emphasis Techniques]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterTechnique("TECH-001", "Volume Emphasis",
            "Increased vocal volume for impact",
            0.90, "Raised shoulders", "Elevated pitch");

        engine.RegisterTechnique("TECH-002", "Stress Emphasis",
            "Emphasis through syllable stress",
            0.75, "Jaw tension", "Lengthened vowels");

        engine.RegisterTechnique("TECH-003", "Speed Emphasis",
            "Rapid or slowed speech for effect",
            0.80, "Quick gestures or stillness", "Rapid or slow delivery");

        engine.RegisterTechnique("TECH-004", "Pause Emphasis",
            "Strategic silence for impact",
            0.85, "Stillness", "Temporal gap");

        engine.RegisterTechnique("TECH-005", "Pitch Variation",
            "Rising or falling pitch patterns",
            0.70, "Head movement", "Tonal variation");

        Console.WriteLine("  ✓ Registered 5 emphasis techniques");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Technique Characteristics]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayTechnique("TECH-001");
        engine.DisplayTechnique("TECH-004");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Registering Stress Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterStressPattern("STRESS-001", "IMPORTANT information",
            new List<int> { 0, 2 }, 0.85, "Vocal stress");

        engine.RegisterStressPattern("STRESS-002", "Please listen carefully",
            new List<int> { 1, 3 }, 0.75, "Conversational stress");

        Console.WriteLine("  ✓ Registered 2 stress patterns");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Applying Emphasis to Text]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ApplyEmphasis("APP-001", "This is really significant",
            new List<string> { "TECH-001", "TECH-004" });

        engine.ApplyEmphasis("APP-002", "Pay close attention",
            new List<string> { "TECH-002", "TECH-005" });

        engine.ApplyEmphasis("APP-003", "The moment is now",
            new List<string> { "TECH-001", "TECH-002", "TECH-004" });

        Console.WriteLine("  ✓ Applied emphasis techniques to 3 utterances");
        engine.DisplayApplication("APP-001");
        engine.DisplayApplication("APP-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Creating Emphasis Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateEmphasisProfile("PROF-001", "Passionate Speaker",
            new Dictionary<string, double> {
                { "TECH-001", 0.90 },
                { "TECH-004", 0.85 },
                { "TECH-005", 0.80 },
                { "TECH-002", 0.70 },
                { "TECH-003", 0.60 }
            });

        engine.CreateEmphasisProfile("PROF-002", "Measured Speaker",
            new Dictionary<string, double> {
                { "TECH-002", 0.75 },
                { "TECH-005", 0.70 },
                { "TECH-004", 0.65 },
                { "TECH-001", 0.50 },
                { "TECH-003", 0.55 }
            });

        Console.WriteLine("  ✓ Created 2 emphasis profiles");
        engine.DisplayProfile("PROF-001");
        engine.DisplayProfile("PROF-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Emphasis System Summary:");
        Console.WriteLine($"    Registered Techniques: {engine.GetTotalTechniques()}");
        Console.WriteLine($"    Applied Emphasis Cases: {engine.GetTotalApplications()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Emphasis Communication Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Emphasis Application Architecture:");
        Console.WriteLine("    Layer 1: Technique Registration (define emphasis methods)");
        Console.WriteLine("    Layer 2: Physical Markers (establish visual cues)");
        Console.WriteLine("    Layer 3: Acoustic Markers (establish vocal cues)");
        Console.WriteLine("    Layer 4: Stress Pattern (define syllable emphasis)");
        Console.WriteLine("    Layer 5: Intensity Measurement (gauge emphasis strength)");
        Console.WriteLine("    Layer 6: Application Rules (apply techniques to text)");
        Console.WriteLine("    Layer 7: Profile Creation (establish speaker preferences)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple emphasis techniques");
        Console.WriteLine("    ✓ Track physical and acoustic markers");
        Console.WriteLine("    ✓ Apply stress patterns to content");
        Console.WriteLine("    ✓ Combine multiple emphasis techniques");
        Console.WriteLine("    ✓ Measure overall emphasis strength");
        Console.WriteLine("    ✓ Create speaker emphasis profiles");
        Console.WriteLine("    ✓ Support multi-technique emphasis");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-emphasis communication system complete");
        Console.ResetColor();
    }
}
