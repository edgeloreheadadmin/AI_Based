using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class SpeakingInMoreThanOneStyle
{
    public class CommunicationStyle
    {
        public string StyleId { get; set; }
        public string StyleName { get; set; }
        public string Description { get; set; }
        public List<string> CharacteristicPhrases { get; set; }
        public double FormalityLevel { get; set; }
        public double ComplexityLevel { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Utterance
    {
        public string UtteranceId { get; set; }
        public string OriginalText { get; set; }
        public Dictionary<string, string> StyledVariations { get; set; }
        public string PrimaryStyle { get; set; }
        public double StyleMatchConfidence { get; set; }
        public DateTime GeneratedDate { get; set; }
    }

    public class StyleAdaptation
    {
        public string AdaptationId { get; set; }
        public string SourceStyle { get; set; }
        public string TargetStyle { get; set; }
        public string OriginalMessage { get; set; }
        public string AdaptedMessage { get; set; }
        public List<string> TransformationRules { get; set; }
        public DateTime AdaptedDate { get; set; }
    }

    public class StyleProfile
    {
        public string ProfileId { get; set; }
        public string PersonName { get; set; }
        public Dictionary<string, double> StylePreferences { get; set; }
        public string DominantStyle { get; set; }
        public List<string> AvailableStyles { get; set; }
        public double StyleFlexibility { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SpeakingStyleEngine
    {
        private Dictionary<string, CommunicationStyle> styles;
        private Dictionary<string, Utterance> utterances;
        private Dictionary<string, StyleAdaptation> adaptations;
        private Dictionary<string, StyleProfile> profiles;

        public SpeakingStyleEngine()
        {
            styles = new Dictionary<string, CommunicationStyle>();
            utterances = new Dictionary<string, Utterance>();
            adaptations = new Dictionary<string, StyleAdaptation>();
            profiles = new Dictionary<string, StyleProfile>();
        }

        public void RegisterStyle(string styleId, string styleName, string description,
                                 List<string> phrases, double formality, double complexity)
        {
            var style = new CommunicationStyle
            {
                StyleId = styleId,
                StyleName = styleName,
                Description = description,
                CharacteristicPhrases = new List<string>(phrases),
                FormalityLevel = formality,
                ComplexityLevel = complexity,
                CreatedDate = DateTime.Now
            };
            styles[styleId] = style;
        }

        public void GenerateStyledUtterance(string utteranceId, string originalText, List<string> styleIds)
        {
            var utterance = new Utterance
            {
                UtteranceId = utteranceId,
                OriginalText = originalText,
                StyledVariations = new Dictionary<string, string>(),
                PrimaryStyle = "",
                StyleMatchConfidence = 0.0,
                GeneratedDate = DateTime.Now
            };

            foreach (var styleId in styleIds)
            {
                if (styles.ContainsKey(styleId))
                {
                    string styledText = TransformToStyle(originalText, styleId);
                    utterance.StyledVariations[styleId] = styledText;
                }
            }

            if (utterance.StyledVariations.Count > 0)
            {
                utterance.PrimaryStyle = utterance.StyledVariations.Keys.First();
                utterance.StyleMatchConfidence = 0.85;
            }

            utterances[utteranceId] = utterance;
        }

        public void AdaptMessage(string adaptationId, string sourceStyleId, string targetStyleId, string message)
        {
            if (!styles.ContainsKey(sourceStyleId) || !styles.ContainsKey(targetStyleId)) return;

            var sourceStyle = styles[sourceStyleId];
            var targetStyle = styles[targetStyleId];

            var adaptation = new StyleAdaptation
            {
                AdaptationId = adaptationId,
                SourceStyle = sourceStyleId,
                TargetStyle = targetStyleId,
                OriginalMessage = message,
                AdaptedMessage = "",
                TransformationRules = new List<string>(),
                AdaptedDate = DateTime.Now
            };

            adaptation.TransformationRules.Add($"Adjust formality from {sourceStyle.FormalityLevel} to {targetStyle.FormalityLevel}");
            adaptation.TransformationRules.Add($"Adjust complexity from {sourceStyle.ComplexityLevel} to {targetStyle.ComplexityLevel}");

            adaptation.AdaptedMessage = TransformToStyle(message, targetStyleId);

            adaptations[adaptationId] = adaptation;
        }

        public void CreateStyleProfile(string profileId, string personName, Dictionary<string, double> stylePrefs)
        {
            var profile = new StyleProfile
            {
                ProfileId = profileId,
                PersonName = personName,
                StylePreferences = new Dictionary<string, double>(stylePrefs),
                DominantStyle = stylePrefs.OrderByDescending(x => x.Value).First().Key,
                AvailableStyles = new List<string>(stylePrefs.Keys),
                StyleFlexibility = CalculateFlexibility(stylePrefs),
                CreatedDate = DateTime.Now
            };
            profiles[profileId] = profile;
        }

        private string TransformToStyle(string text, string styleId)
        {
            if (!styles.ContainsKey(styleId)) return text;

            var style = styles[styleId];
            string transformed = text;

            if (style.FormalityLevel > 0.8)
            {
                transformed = transformed.Replace("gonna", "going to")
                                        .Replace("wanna", "want to")
                                        .Replace("can't", "cannot");
            }

            if (style.ComplexityLevel > 0.7)
            {
                transformed = $"[{style.StyleName} adaptation] {transformed}";
            }

            return transformed;
        }

        private double CalculateFlexibility(Dictionary<string, double> preferences)
        {
            if (preferences.Count == 0) return 0.0;
            var values = preferences.Values.ToList();
            double average = values.Average();
            double variance = values.Sum(v => Math.Pow(v - average, 2)) / values.Count;
            return Math.Sqrt(variance);
        }

        public void DisplayStyle(string styleId)
        {
            if (!styles.ContainsKey(styleId)) return;

            var style = styles[styleId];
            Console.WriteLine($"\n  Communication Style: {style.StyleName}");
            Console.WriteLine($"  Description: {style.Description}");
            Console.WriteLine($"  Formality Level: {style.FormalityLevel * 100:F0}%");
            Console.WriteLine($"  Complexity Level: {style.ComplexityLevel * 100:F0}%");
            Console.WriteLine($"  Example Phrases: {string.Join(", ", style.CharacteristicPhrases.Take(3))}");
        }

        public void DisplayUtterance(string utteranceId)
        {
            if (!utterances.ContainsKey(utteranceId)) return;

            var utterance = utterances[utteranceId];
            Console.WriteLine($"\n  Utterance: {utterance.UtteranceId}");
            Console.WriteLine($"  Original: {utterance.OriginalText}");
            Console.WriteLine($"  Styled Variations: {utterance.StyledVariations.Count}");
            foreach (var variation in utterance.StyledVariations.Take(2))
            {
                Console.WriteLine($"    [{variation.Key}]: {variation.Value}");
            }
        }

        public void DisplayProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            Console.WriteLine($"\n  Style Profile: {profile.PersonName}");
            Console.WriteLine($"  Dominant Style: {profile.DominantStyle}");
            Console.WriteLine($"  Available Styles: {profile.AvailableStyles.Count}");
            Console.WriteLine($"  Style Flexibility: {profile.StyleFlexibility:F2}");
        }

        public int GetTotalStyles()
        {
            return styles.Count;
        }

        public int GetTotalUtterances()
        {
            return utterances.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║            Speaking in More Than One Style                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new SpeakingStyleEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Communication Styles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterStyle("STYLE-001", "Formal Academic",
            "Precise, scholarly communication with technical terminology",
            new List<string> { "Furthermore", "In conclusion", "It is evident that" }, 0.95, 0.85);

        engine.RegisterStyle("STYLE-002", "Casual Conversational",
            "Relaxed, friendly communication with colloquialisms",
            new List<string> { "Hey", "You know what", "Actually" }, 0.30, 0.40);

        engine.RegisterStyle("STYLE-003", "Technical Professional",
            "Precise technical communication for specialists",
            new List<string> { "Algorithm", "Implementation", "Protocol" }, 0.75, 0.90);

        engine.RegisterStyle("STYLE-004", "Poetic Expressive",
            "Artistic, imaginative communication with metaphors",
            new List<string> { "Echoes", "Whispers", "Illuminates" }, 0.60, 0.70);

        Console.WriteLine("  ✓ Registered 4 communication styles");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Style Characteristics]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayStyle("STYLE-001");
        engine.DisplayStyle("STYLE-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Generating Styled Utterances]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.GenerateStyledUtterance("UTT-001", "The system works really well",
            new List<string> { "STYLE-001", "STYLE-002", "STYLE-003" });

        engine.GenerateStyledUtterance("UTT-002", "It's going to be awesome",
            new List<string> { "STYLE-001", "STYLE-004" });

        Console.WriteLine("  ✓ Generated 2 utterances with multiple style variations");
        engine.DisplayUtterance("UTT-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Adapting Messages Between Styles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AdaptMessage("ADAPT-001", "STYLE-002", "STYLE-001",
            "Hey, this thing is gonna be really cool");

        engine.AdaptMessage("ADAPT-002", "STYLE-001", "STYLE-004",
            "The implementation demonstrates significant efficiency improvements");

        Console.WriteLine("  ✓ Adapted messages across style boundaries");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Creating Style Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateStyleProfile("PROF-001", "Technical Speaker",
            new Dictionary<string, double> {
                { "STYLE-001", 0.75 },
                { "STYLE-003", 0.90 },
                { "STYLE-002", 0.40 },
                { "STYLE-004", 0.30 }
            });

        engine.CreateStyleProfile("PROF-002", "Versatile Communicator",
            new Dictionary<string, double> {
                { "STYLE-001", 0.70 },
                { "STYLE-002", 0.75 },
                { "STYLE-003", 0.65 },
                { "STYLE-004", 0.72 }
            });

        Console.WriteLine("  ✓ Created 2 style profiles");
        engine.DisplayProfile("PROF-001");
        engine.DisplayProfile("PROF-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Communication System Summary:");
        Console.WriteLine($"    Registered Styles: {engine.GetTotalStyles()}");
        Console.WriteLine($"    Generated Utterances: {engine.GetTotalUtterances()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Style Communication Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Style Adaptation Architecture:");
        Console.WriteLine("    Layer 1: Style Registration (define communication patterns)");
        Console.WriteLine("    Layer 2: Characteristic Phrases (establish style markers)");
        Console.WriteLine("    Layer 3: Formality Classification (measure formality level)");
        Console.WriteLine("    Layer 4: Complexity Assignment (measure complexity level)");
        Console.WriteLine("    Layer 5: Message Generation (produce styled utterances)");
        Console.WriteLine("    Layer 6: Style Adaptation (transform between styles)");
        Console.WriteLine("    Layer 7: Profile Creation (establish user preferences)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Register multiple communication styles");
        Console.WriteLine("    ✓ Generate styled variations of messages");
        Console.WriteLine("    ✓ Adapt messages between style boundaries");
        Console.WriteLine("    ✓ Track formality and complexity levels");
        Console.WriteLine("    ✓ Create user style preferences");
        Console.WriteLine("    ✓ Measure style flexibility");
        Console.WriteLine("    ✓ Support style-based communication");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-style communication system complete");
        Console.ResetColor();
    }
}
