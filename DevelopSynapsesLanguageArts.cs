using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DevelopSynapsesLanguageArts
{
    public class Synapse
    {
        public string SourceNeuron { get; set; }
        public string TargetNeuron { get; set; }
        public double Strength { get; set; }
        public int FireCount { get; set; }
        public List<string> LinguisticTriggers { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CognitiveFeature
    {
        public string FeatureName { get; set; }
        public string Category { get; set; }
        public double Activation { get; set; }
        public List<string> SupportingWords { get; set; }
        public List<Synapse> ConnectedSynapses { get; set; }
        public int DevelopmentStage { get; set; }
        public DateTime DevelopedDate { get; set; }
    }

    public class LanguageExposure
    {
        public string Text { get; set; }
        public List<string> Concepts { get; set; }
        public double Complexity { get; set; }
        public double LinguisticDiversity { get; set; }
        public DateTime ExposureDate { get; set; }
    }

    public class NeurocognitiveEngine
    {
        private Dictionary<string, Synapse> synapses;
        private Dictionary<string, CognitiveFeature> features;
        private List<LanguageExposure> exposures;
        private Dictionary<string, int> wordFrequency;

        public NeurocognitiveEngine()
        {
            synapses = new Dictionary<string, Synapse>();
            features = new Dictionary<string, CognitiveFeature>();
            exposures = new List<LanguageExposure>();
            wordFrequency = new Dictionary<string, int>();
        }

        public void ExposeTolanguage(string text, double complexity)
        {
            var words = text.ToLower().Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
            var concepts = ExtractConcepts(text);

            var exposure = new LanguageExposure
            {
                Text = text,
                Concepts = concepts,
                Complexity = Math.Min(complexity, 1.0),
                LinguisticDiversity = CalculateLinguisticDiversity(words),
                ExposureDate = DateTime.Now
            };
            exposures.Add(exposure);

            foreach (var word in words)
            {
                if (!wordFrequency.ContainsKey(word))
                    wordFrequency[word] = 0;
                wordFrequency[word]++;
            }

            ActivateCognitivePaths(concepts, complexity);
        }

        private List<string> ExtractConcepts(string text)
        {
            var conceptMap = new Dictionary<string, List<string>>
            {
                { "language", new List<string> { "word", "sentence", "grammar", "syntax", "meaning" } },
                { "learning", new List<string> { "knowledge", "skill", "understanding", "mastery", "practice" } },
                { "mind", new List<string> { "thought", "idea", "consciousness", "cognition", "awareness" } },
                { "connection", new List<string> { "link", "bond", "relationship", "network", "integration" } },
                { "growth", new List<string> { "development", "improvement", "advancement", "expansion", "potential" } }
            };

            var concepts = new List<string>();
            foreach (var kvp in conceptMap)
            {
                if (text.ToLower().Contains(kvp.Key))
                {
                    concepts.AddRange(kvp.Value);
                }
            }

            return concepts.Distinct().ToList();
        }

        private double CalculateLinguisticDiversity(string[] words)
        {
            if (words.Length == 0) return 0.0;
            int uniqueWords = words.Distinct().Count();
            return Math.Min((double)uniqueWords / words.Length, 1.0);
        }

        private void ActivateCognitivePaths(List<string> concepts, double complexity)
        {
            foreach (var concept in concepts)
            {
                if (!features.ContainsKey(concept))
                {
                    features[concept] = new CognitiveFeature
                    {
                        FeatureName = concept,
                        Category = "Language-Activated",
                        Activation = 0.3,
                        SupportingWords = new List<string>(),
                        ConnectedSynapses = new List<Synapse>(),
                        DevelopmentStage = 1,
                        DevelopedDate = DateTime.Now
                    };
                }
                else
                {
                    var feature = features[concept];
                    feature.Activation = Math.Min(feature.Activation + (complexity * 0.2), 0.99);
                    feature.DevelopmentStage = (int)(feature.Activation * 10) + 1;
                }
            }

            CreateSynapticConnections(concepts);
        }

        private void CreateSynapticConnections(List<string> concepts)
        {
            for (int i = 0; i < concepts.Count - 1; i++)
            {
                for (int j = i + 1; j < concepts.Count; j++)
                {
                    string synapseKey = $"{concepts[i]}->{concepts[j]}";
                    if (!synapses.ContainsKey(synapseKey))
                    {
                        var synapse = new Synapse
                        {
                            SourceNeuron = concepts[i],
                            TargetNeuron = concepts[j],
                            Strength = 0.4,
                            FireCount = 1,
                            LinguisticTriggers = new List<string>(),
                            CreatedDate = DateTime.Now
                        };
                        synapses[synapseKey] = synapse;
                    }
                    else
                    {
                        var synapse = synapses[synapseKey];
                        synapse.Strength = Math.Min(synapse.Strength + 0.15, 0.99);
                        synapse.FireCount++;
                    }
                }
            }
        }

        public void StudyLanguageArts(string artType, double intensity)
        {
            string feature = $"{artType}-Mastery";
            if (!features.ContainsKey(feature))
            {
                features[feature] = new CognitiveFeature
                {
                    FeatureName = feature,
                    Category = "Language Arts",
                    Activation = 0.2,
                    SupportingWords = new List<string>(),
                    ConnectedSynapses = new List<Synapse>(),
                    DevelopmentStage = 1,
                    DevelopedDate = DateTime.Now
                };
            }

            var feat = features[feature];
            feat.Activation = Math.Min(feat.Activation + (intensity * 0.25), 0.99);
            feat.DevelopmentStage = (int)(feat.Activation * 10) + 1;

            switch (artType.ToLower())
            {
                case "poetry":
                    feat.SupportingWords.AddRange(new[] { "rhythm", "metaphor", "imagery", "emotion", "flow" });
                    break;
                case "rhetoric":
                    feat.SupportingWords.AddRange(new[] { "persuasion", "argument", "style", "voice", "impact" });
                    break;
                case "writing":
                    feat.SupportingWords.AddRange(new[] { "clarity", "structure", "narrative", "expression", "craft" });
                    break;
                case "reading":
                    feat.SupportingWords.AddRange(new[] { "comprehension", "analysis", "interpretation", "context", "nuance" });
                    break;
            }
        }

        public void DisplaySynapticNetwork()
        {
            Console.WriteLine("\n  Synaptic Network Overview:");
            Console.WriteLine($"  Total Synapses: {synapses.Count}");
            Console.WriteLine($"  Total Cognitive Features: {features.Count}");
            Console.WriteLine($"  Unique Words Encountered: {wordFrequency.Count}");

            Console.WriteLine("\n  Strongest Synapses (by connection strength):");
            var strongSynapses = synapses.Values.OrderByDescending(s => s.Strength).Take(5);
            foreach (var syn in strongSynapses)
            {
                Console.WriteLine($"    {syn.SourceNeuron} ↔ {syn.TargetNeuron}: {syn.Strength * 100:F1}% strength");
            }
        }

        public void DisplayCognitiveFeature(string featureName)
        {
            if (!features.ContainsKey(featureName)) return;

            var feature = features[featureName];
            Console.WriteLine($"\n  Cognitive Feature: {feature.FeatureName}");
            Console.WriteLine($"  Category: {feature.Category}");
            Console.WriteLine($"  Activation Level: {feature.Activation * 100:F1}%");
            Console.WriteLine($"  Development Stage: {feature.DevelopmentStage}/10");
            if (feature.SupportingWords.Count > 0)
            {
                Console.WriteLine($"  Supporting Words: {string.Join(", ", feature.SupportingWords.Take(5))}");
            }
        }

        public void DisplayLanguageExposureProgress()
        {
            Console.WriteLine("\n  Language Exposure Progress:");
            Console.WriteLine($"  Total Exposures: {exposures.Count}");

            double avgComplexity = exposures.Count > 0 ? exposures.Average(e => e.Complexity) : 0.0;
            double avgDiversity = exposures.Count > 0 ? exposures.Average(e => e.LinguisticDiversity) : 0.0;

            Console.WriteLine($"  Average Complexity: {avgComplexity * 100:F1}%");
            Console.WriteLine($"  Average Linguistic Diversity: {avgDiversity * 100:F1}%");

            Console.WriteLine("\n  Most Frequent Words:");
            var topWords = wordFrequency.OrderByDescending(w => w.Value).Take(10);
            foreach (var kvp in topWords)
            {
                Console.WriteLine($"    '{kvp.Key}': {kvp.Value} occurrences");
            }
        }

        public Dictionary<string, double> GetFeatureActivations()
        {
            return features.OrderByDescending(f => f.Value.Activation)
                .ToDictionary(f => f.Key, f => f.Value.Activation);
        }

        public int GetTotalSynapticConnections()
        {
            return synapses.Values.Sum(s => s.FireCount);
        }

        public double GetNetworkComplexity()
        {
            if (features.Count == 0) return 0.0;
            double avgActivation = features.Values.Average(f => f.Activation);
            double synapseIntensity = synapses.Count > 0 ? synapses.Values.Average(s => s.Strength) : 0.0;
            return (avgActivation * 0.6 + synapseIntensity * 0.4);
        }

        public string GetDevelopmentLevel(double complexity)
        {
            return complexity switch
            {
                >= 0.8 => "Highly Developed - Advanced neural integration",
                >= 0.6 => "Well Developed - Strong synaptic pathways",
                >= 0.4 => "Developing - Growing cognitive networks",
                >= 0.2 => "Emerging - Initial neural connections",
                _ => "Nascent - Beginning language integration"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Developing Synapses Through Language and Language Arts       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new NeurocognitiveEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Linguistic Exposure and Concept Activation]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ExposeTolanguage("Language shapes thought and creates new neural pathways in the mind", 0.75);
        engine.ExposeTolanguage("Learning through reading activates multiple cognitive networks simultaneously", 0.80);
        engine.ExposeTolanguage("The connection between words and thoughts develops synaptic strength over time", 0.85);

        Console.WriteLine("  ✓ Exposed to 3 language samples of increasing complexity");
        Console.WriteLine("  ✓ Concepts extracted and neural activation triggered");
        engine.DisplayLanguageExposureProgress();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Building Synaptic Connections]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.ExposeTolanguage("Cognitive development requires sustained language engagement and practice", 0.78);
        engine.ExposeTolanguage("Each new word strengthens neural pathways and expands mental models", 0.82);
        engine.ExposeTolanguage("Language arts cultivate both analytical thinking and creative expression", 0.88);

        Console.WriteLine("  ✓ Continued language exposure strengthens synaptic pathways");
        Console.WriteLine("  ✓ Repeated concepts create deeper neural integration");
        engine.DisplaySynapticNetwork();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Language Arts Mastery Development]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Developing mastery through specialized language arts:");
        engine.StudyLanguageArts("Poetry", 0.9);
        engine.StudyLanguageArts("Rhetoric", 0.85);
        engine.StudyLanguageArts("Writing", 0.88);
        engine.StudyLanguageArts("Reading", 0.92);

        engine.DisplayCognitiveFeature("Poetry-Mastery");
        engine.DisplayCognitiveFeature("Rhetoric-Mastery");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Advanced Cognitive Feature Activation]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Advanced language processing activates complex features:");
        engine.ExposeTolanguage("Metaphorical language engages abstract thinking and creative problem-solving abilities", 0.90);
        engine.ExposeTolanguage("Narrative structure develops sequential processing and pattern recognition skills", 0.87);
        engine.ExposeTolanguage("Linguistic analysis strengthens critical thinking and logical reasoning capacities", 0.89);

        var activations = engine.GetFeatureActivations();
        Console.WriteLine("\n  Top Activated Cognitive Features:");
        int rank = 1;
        foreach (var kvp in activations.Take(5))
        {
            Console.WriteLine($"    {rank}. {kvp.Key}: {kvp.Value * 100:F1}% activation");
            rank++;
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Synaptic Connection Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Total Synaptic Connections Formed: {engine.GetTotalSynapticConnections()}");
        Console.WriteLine($"  Network Complexity Index: {engine.GetNetworkComplexity() * 100:F1}%");

        double complexity = engine.GetNetworkComplexity();
        string level = engine.GetDevelopmentLevel(complexity);
        Console.WriteLine($"  Overall Development: {level}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Continued Language Exposure Impact]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 0; i < 5; i++)
        {
            engine.ExposeTolanguage("Continued engagement with rich language strengthens neural networks exponentially", 0.85 + (i * 0.02));
            engine.StudyLanguageArts("Writing", 0.85);
        }

        Console.WriteLine("  ✓ Reinforced learning through repeated exposure");
        Console.WriteLine("  ✓ Complexity and synaptic strength continue to develop");
        engine.DisplayLanguageExposureProgress();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Neurological Development Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Language-Based Synaptic Development:");
        Console.WriteLine("    Stage 1: Word Exposure (0-20% activation)");
        Console.WriteLine("            Single word recognition, basic concepts");
        Console.WriteLine("    Stage 2: Concept Formation (20-40% activation)");
        Console.WriteLine("            Clustering related ideas, semantic networks");
        Console.WriteLine("    Stage 3: Synaptic Integration (40-60% activation)");
        Console.WriteLine("            Multiple concepts connecting, pattern emergence");
        Console.WriteLine("    Stage 4: Neural Mastery (60-80% activation)");
        Console.WriteLine("            Automatic processing, complex understanding");
        Console.WriteLine("    Stage 5: Cognitive Excellence (80%+ activation)");
        Console.WriteLine("            Integrated systems, creative application");
        Console.WriteLine("\n  Key Mechanisms:");
        Console.WriteLine("    ✓ Linguistic exposure triggers neural activation");
        Console.WriteLine("    ✓ Repeated concepts strengthen synaptic pathways");
        Console.WriteLine("    ✓ Language arts develop specialized cognitive features");
        Console.WriteLine("    ✓ Complexity creates richer neural networks");
        Console.WriteLine("    ✓ Diversity in language exposure broadens understanding");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Synaptic development through language system complete");
        Console.ResetColor();
    }
}
