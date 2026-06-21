using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class MultipleWordDefinitions
{
    public class Definition
    {
        public string DefinitionText { get; set; }
        public string PartOfSpeech { get; set; }
        public string Context { get; set; }
        public int UsageFrequency { get; set; }
        public List<string> RelatedWords { get; set; }
        public DateTime AddedDate { get; set; }
        public string Etymology { get; set; }
    }

    public class SemanticNode
    {
        public string Word { get; set; }
        public List<Definition> Definitions { get; set; }
        public Dictionary<string, double> ConnectionStrengths { get; set; }
        public string PrimaryMeaning { get; set; }
        public double Ambiguity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SemanticLink
    {
        public string SourceWord { get; set; }
        public string TargetWord { get; set; }
        public string LinkType { get; set; }
        public double Strength { get; set; }
        public string Explanation { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class SemanticNetwork
    {
        private Dictionary<string, SemanticNode> nodes;
        private List<SemanticLink> links;
        private Dictionary<string, List<string>> contextGroups;

        public SemanticNetwork()
        {
            nodes = new Dictionary<string, SemanticNode>();
            links = new List<SemanticLink>();
            contextGroups = new Dictionary<string, List<string>>();
        }

        public void CreateWordNode(string word)
        {
            if (!nodes.ContainsKey(word.ToLower()))
            {
                var node = new SemanticNode
                {
                    Word = word,
                    Definitions = new List<Definition>(),
                    ConnectionStrengths = new Dictionary<string, double>(),
                    PrimaryMeaning = "",
                    Ambiguity = 0.0,
                    CreatedDate = DateTime.Now
                };
                nodes[word.ToLower()] = node;
            }
        }

        public void AddDefinition(string word, string definitionText, string partOfSpeech,
                                 string context, string etymology = "")
        {
            string wordKey = word.ToLower();
            if (!nodes.ContainsKey(wordKey))
                CreateWordNode(word);

            var definition = new Definition
            {
                DefinitionText = definitionText,
                PartOfSpeech = partOfSpeech,
                Context = context,
                UsageFrequency = 1,
                RelatedWords = new List<string>(),
                AddedDate = DateTime.Now,
                Etymology = etymology
            };

            nodes[wordKey].Definitions.Add(definition);
            UpdateAmbiguity(wordKey);

            if (!nodes[wordKey].PrimaryMeaning.Contains(definitionText))
            {
                if (nodes[wordKey].Definitions.Count == 1)
                    nodes[wordKey].PrimaryMeaning = definitionText;
            }
        }

        public void AddRelatedWord(string word, int definitionIndex, string relatedWord)
        {
            string wordKey = word.ToLower();
            if (!nodes.ContainsKey(wordKey)) return;

            if (definitionIndex >= 0 && definitionIndex < nodes[wordKey].Definitions.Count)
            {
                if (!nodes[wordKey].Definitions[definitionIndex].RelatedWords.Contains(relatedWord))
                {
                    nodes[wordKey].Definitions[definitionIndex].RelatedWords.Add(relatedWord);
                }
            }
        }

        public void CreateSemanticLink(string sourceWord, string targetWord, string linkType,
                                      double strength, string explanation)
        {
            string sourceKey = sourceWord.ToLower();
            string targetKey = targetWord.ToLower();

            if (!nodes.ContainsKey(sourceKey))
                CreateWordNode(sourceWord);
            if (!nodes.ContainsKey(targetKey))
                CreateWordNode(targetWord);

            var link = new SemanticLink
            {
                SourceWord = sourceKey,
                TargetWord = targetKey,
                LinkType = linkType,
                Strength = Math.Min(strength, 1.0),
                Explanation = explanation,
                CreatedDate = DateTime.Now
            };
            links.Add(link);

            nodes[sourceKey].ConnectionStrengths[targetKey] = Math.Min(
                nodes[sourceKey].ConnectionStrengths.GetValueOrDefault(targetKey, 0.0) + strength,
                1.0
            );
        }

        private void UpdateAmbiguity(string wordKey)
        {
            if (!nodes.ContainsKey(wordKey)) return;

            int defCount = nodes[wordKey].Definitions.Count;
            if (defCount <= 1)
            {
                nodes[wordKey].Ambiguity = 0.0;
            }
            else
            {
                nodes[wordKey].Ambiguity = Math.Min(1.0, (defCount - 1) / 10.0);
            }
        }

        public void DisplayWordDefinitions(string word)
        {
            string wordKey = word.ToLower();
            if (!nodes.ContainsKey(wordKey)) return;

            var node = nodes[wordKey];
            Console.WriteLine($"\n  Word: {node.Word}");
            Console.WriteLine($"  Ambiguity: {node.Ambiguity * 100:F1}% ({node.Definitions.Count} definitions)");
            Console.WriteLine($"  Definitions:");

            int index = 1;
            foreach (var def in node.Definitions)
            {
                Console.WriteLine($"    {index}. ({def.PartOfSpeech}) {def.DefinitionText}");
                Console.WriteLine($"       Context: {def.Context}");
                if (!string.IsNullOrEmpty(def.Etymology))
                    Console.WriteLine($"       Etymology: {def.Etymology}");
                if (def.RelatedWords.Count > 0)
                    Console.WriteLine($"       Related: {string.Join(", ", def.RelatedWords)}");
                index++;
            }
        }

        public void DisplayWordConnections(string word)
        {
            string wordKey = word.ToLower();
            if (!nodes.ContainsKey(wordKey)) return;

            var node = nodes[wordKey];
            Console.WriteLine($"\n  Semantic Connections for '{word}':");

            var relevantLinks = links.Where(l => l.SourceWord == wordKey || l.TargetWord == wordKey).ToList();

            if (relevantLinks.Count == 0)
            {
                Console.WriteLine("    (No connections established)");
                return;
            }

            foreach (var link in relevantLinks.OrderByDescending(l => l.Strength))
            {
                string direction = link.SourceWord == wordKey ? "→" : "←";
                string otherWord = link.SourceWord == wordKey ? link.TargetWord : link.SourceWord;
                Console.WriteLine($"    {word} {direction} {otherWord}");
                Console.WriteLine($"      Type: {link.LinkType} | Strength: {link.Strength * 100:F0}%");
                Console.WriteLine($"      Explanation: {link.Explanation}");
            }
        }

        public void DisplaySemanticNetwork(int depth = 2)
        {
            Console.WriteLine("\n  Semantic Network Overview:");
            Console.WriteLine($"  Total Words: {nodes.Count}");
            Console.WriteLine($"  Total Connections: {links.Count}");

            Console.WriteLine("\n  Word Nodes with Multiple Definitions:");
            var multiDef = nodes.Where(n => n.Value.Definitions.Count > 1)
                .OrderByDescending(n => n.Value.Definitions.Count).ToList();

            foreach (var kvp in multiDef.Take(5))
            {
                Console.WriteLine($"    '{kvp.Key}': {kvp.Value.Definitions.Count} definitions, " +
                                $"Ambiguity: {kvp.Value.Ambiguity * 100:F0}%");
            }

            Console.WriteLine("\n  Strongest Semantic Links:");
            var strongLinks = links.OrderByDescending(l => l.Strength).Take(5).ToList();
            foreach (var link in strongLinks)
            {
                Console.WriteLine($"    {link.SourceWord} --[{link.LinkType}]--> {link.TargetWord}");
                Console.WriteLine($"      Strength: {link.Strength * 100:F0}%");
            }
        }

        public Dictionary<string, int> GetConnectionDegree()
        {
            var degrees = new Dictionary<string, int>();

            foreach (var word in nodes.Keys)
            {
                int degree = links.Count(l => l.SourceWord == word || l.TargetWord == word);
                degrees[word] = degree;
            }

            return degrees;
        }

        public List<string> FindSynonyms(string word)
        {
            string wordKey = word.ToLower();
            var synonymLinks = links.Where(l =>
                (l.SourceWord == wordKey && l.LinkType == "synonym") ||
                (l.TargetWord == wordKey && l.LinkType == "synonym")).ToList();

            return synonymLinks.Select(l => l.SourceWord == wordKey ? l.TargetWord : l.SourceWord).ToList();
        }

        public List<string> FindAntonyms(string word)
        {
            string wordKey = word.ToLower();
            var antonymLinks = links.Where(l =>
                (l.SourceWord == wordKey && l.LinkType == "antonym") ||
                (l.TargetWord == wordKey && l.LinkType == "antonym")).ToList();

            return antonymLinks.Select(l => l.SourceWord == wordKey ? l.TargetWord : l.SourceWord).ToList();
        }

        public List<string> TraceConceptPath(string startWord, string endWord, int maxDepth = 4)
        {
            var queue = new Queue<(string, List<string>)>();
            var visited = new HashSet<string>();

            queue.Enqueue((startWord.ToLower(), new List<string> { startWord.ToLower() }));
            visited.Add(startWord.ToLower());

            while (queue.Count > 0)
            {
                var (current, path) = queue.Dequeue();

                if (current == endWord.ToLower())
                    return path;

                if (path.Count >= maxDepth) continue;

                var nextWords = links.Where(l =>
                    (l.SourceWord == current && !visited.Contains(l.TargetWord)) ||
                    (l.TargetWord == current && !visited.Contains(l.SourceWord)))
                    .Select(l => l.SourceWord == current ? l.TargetWord : l.SourceWord)
                    .ToList();

                foreach (var next in nextWords)
                {
                    if (!visited.Contains(next))
                    {
                        visited.Add(next);
                        var newPath = new List<string>(path) { next };
                        queue.Enqueue((next, newPath));
                    }
                }
            }

            return new List<string>();
        }

        public string GetDefinitionContext(string word, int defIndex)
        {
            string wordKey = word.ToLower();
            if (!nodes.ContainsKey(wordKey) || defIndex < 0 || defIndex >= nodes[wordKey].Definitions.Count)
                return "";

            return nodes[wordKey].Definitions[defIndex].Context;
        }

        public int GetTotalDefinitions()
        {
            return nodes.Values.Sum(n => n.Definitions.Count);
        }

        public double GetAverageAmbiguity()
        {
            return nodes.Count > 0 ? nodes.Values.Average(n => n.Ambiguity) : 0.0;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Multiple Definitions and Semantic Network Connections        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var network = new SemanticNetwork();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Words with Multiple Definitions]");
        Console.ResetColor();
        Thread.Sleep(500);

        network.AddDefinition("light",
            "Natural radiation made visible to the eye",
            "noun",
            "Physics and perception",
            "Old English lēoht");

        network.AddDefinition("light",
            "Of little weight; not heavy",
            "adjective",
            "Physical properties");

        network.AddDefinition("light",
            "Not heavy in pressure or amount",
            "adjective",
            "Quantity and intensity");

        network.AddDefinition("light",
            "To set something on fire; to illuminate",
            "verb",
            "Action and causation");

        network.AddDefinition("light",
            "To descend and settle on something",
            "verb",
            "Movement and positioning");

        Console.WriteLine("  ✓ 'Light' defined with 5 distinct meanings");

        network.AddDefinition("bright",
            "Giving out much light; luminous",
            "adjective",
            "Light and vision");

        network.AddDefinition("bright",
            "Intelligent and quick to learn",
            "adjective",
            "Mental capability");

        network.AddDefinition("bright",
            "Cheerful and optimistic in manner",
            "adjective",
            "Emotional state");

        Console.WriteLine("  ✓ 'Bright' defined with 3 meanings");

        network.AddDefinition("run",
            "To move quickly on foot",
            "verb",
            "Physical movement");

        network.AddDefinition("run",
            "A continuous series of similar events",
            "noun",
            "Sequence and pattern");

        network.AddDefinition("run",
            "To manage or operate something",
            "verb",
            "Control and management");

        Console.WriteLine("  ✓ Total of 10 definitions across 3 key words");
        network.DisplayWordDefinitions("light");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Establishing Semantic Relationships]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Creating semantic links between concepts:");

        network.CreateSemanticLink("light", "bright", "synonym", 0.85,
            "Both describe illumination and luminosity");

        network.CreateSemanticLink("light", "heavy", "antonym", 0.90,
            "Direct opposites in physical weight and substance");

        network.CreateSemanticLink("bright", "intelligent", "semantic", 0.75,
            "Brightness often metaphorically represents intellectual capacity");

        network.CreateSemanticLink("light", "dark", "antonym", 0.95,
            "Fundamental opposites in visual perception");

        network.CreateSemanticLink("bright", "cheerful", "synonym", 0.70,
            "Both convey positive emotional tone");

        network.CreateSemanticLink("run", "move", "synonym", 0.80,
            "Running is a specific form of rapid movement");

        Console.WriteLine("  ✓ Established 6 semantic relationships");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Adding Related Words to Definitions]");
        Console.ResetColor();
        Thread.Sleep(500);

        network.AddRelatedWord("light", 0, "illumination");
        network.AddRelatedWord("light", 0, "radiance");
        network.AddRelatedWord("light", 0, "photon");

        network.AddRelatedWord("light", 1, "weight");
        network.AddRelatedWord("light", 1, "feather");
        network.AddRelatedWord("light", 1, "buoyant");

        network.AddRelatedWord("bright", 0, "luminous");
        network.AddRelatedWord("bright", 0, "shining");
        network.AddRelatedWord("bright", 1, "clever");
        network.AddRelatedWord("bright", 1, "genius");

        Console.WriteLine("  ✓ Connected definitions to related conceptual words");
        network.DisplayWordDefinitions("bright");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Exploring Semantic Connections]");
        Console.ResetColor();
        Thread.Sleep(500);

        network.DisplayWordConnections("light");
        network.DisplayWordConnections("bright");

        var synonyms = network.FindSynonyms("light");
        Console.WriteLine($"\n  Synonyms of 'light': {string.Join(", ", synonyms)}");

        var antonyms = network.FindAntonyms("light");
        Console.WriteLine($"  Antonyms of 'light': {string.Join(", ", antonyms)}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Tracing Conceptual Paths]");
        Console.ResetColor();
        Thread.Sleep(500);

        var path1 = network.TraceConceptPath("light", "intelligent");
        Console.WriteLine("  Conceptual path from 'light' to 'intelligent':");
        if (path1.Count > 0)
        {
            Console.WriteLine($"    {string.Join(" → ", path1)}");
        }
        else
        {
            Console.WriteLine("    (Path not found with current connections)");
        }

        var path2 = network.TraceConceptPath("light", "dark");
        Console.WriteLine("\n  Conceptual path from 'light' to 'dark':");
        if (path2.Count > 0)
        {
            Console.WriteLine($"    {string.Join(" → ", path2)}");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Network Statistics and Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var degrees = network.GetConnectionDegree();
        Console.WriteLine("  Word Connection Degrees (how connected each word is):");
        foreach (var kvp in degrees.OrderByDescending(x => x.Value))
        {
            Console.WriteLine($"    '{kvp.Key}': {kvp.Value} connections");
        }

        Console.WriteLine($"\n  Network Statistics:");
        Console.WriteLine($"    Total Definitions: {network.GetTotalDefinitions()}");
        Console.WriteLine($"    Average Word Ambiguity: {network.GetAverageAmbiguity() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Semantic Network Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        network.DisplaySemanticNetwork();

        Console.WriteLine("\n  Multi-Definition Framework:");
        Console.WriteLine("    Layer 1: Primary Definitions (most common usage)");
        Console.WriteLine("    Layer 2: Alternative Meanings (different contexts)");
        Console.WriteLine("    Layer 3: Etymology (linguistic origins)");
        Console.WriteLine("    Layer 4: Related Words (conceptual associations)");
        Console.WriteLine("    Layer 5: Semantic Links (relationships to other words)");
        Console.WriteLine("    Layer 6: Conceptual Paths (how meanings interconnect)");
        Console.WriteLine("\n  Key Principles:");
        Console.WriteLine("    ✓ Words contain multiple meanings simultaneously");
        Console.WriteLine("    ✓ Meanings connect through semantic relationships");
        Console.WriteLine("    ✓ Context determines which definition applies");
        Console.WriteLine("    ✓ Synonyms and antonyms map conceptual space");
        Console.WriteLine("    ✓ Paths between concepts reveal linguistic structure");
        Console.WriteLine("    ✓ Network effects enable metaphor and analogy");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Semantic network system complete");
        Console.ResetColor();
    }
}
