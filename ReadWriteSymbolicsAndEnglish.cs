using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ReadWriteSymbolicsAndEnglish
{
    public class Symbol
    {
        public string SymbolCharacter { get; set; }
        public string MeaningEnglish { get; set; }
        public List<string> AlternativeMeanings { get; set; }
        public string Category { get; set; }
        public double Clarity { get; set; }
        public DateTime DefinedDate { get; set; }
    }

    public class SymbolicStatement
    {
        public string SymbolicForm { get; set; }
        public string EnglishTranslation { get; set; }
        public List<string> ComponentSymbols { get; set; }
        public double ComplexityLevel { get; set; }
        public List<string> InterpretationNotes { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BilingualConcept
    {
        public string ConceptName { get; set; }
        public string EnglishDescription { get; set; }
        public string SymbolicRepresentation { get; set; }
        public double UniversalityScore { get; set; }
        public List<string> RelatedConcepts { get; set; }
        public DateTime EstablishedDate { get; set; }
    }

    public class SymboticLanguageEngine
    {
        private Dictionary<string, Symbol> symbolDictionary;
        private List<SymbolicStatement> statements;
        private List<BilingualConcept> concepts;
        private Dictionary<string, int> usageFrequency;

        public SymboticLanguageEngine()
        {
            symbolDictionary = new Dictionary<string, Symbol>();
            statements = new List<SymbolicStatement>();
            concepts = new List<BilingualConcept>();
            usageFrequency = new Dictionary<string, int>();
        }

        public void DefineSymbol(string symbolChar, string englishMeaning, string category,
                                List<string> alternativeMeanings = null)
        {
            var symbol = new Symbol
            {
                SymbolCharacter = symbolChar,
                MeaningEnglish = englishMeaning,
                AlternativeMeanings = alternativeMeanings ?? new List<string>(),
                Category = category,
                Clarity = 0.8,
                DefinedDate = DateTime.Now
            };
            symbolDictionary[symbolChar] = symbol;
            usageFrequency[symbolChar] = 0;
        }

        public void CreateBilingualConcept(string conceptName, string englishDesc,
                                         string symbolicForm, List<string> relatedConcepts = null)
        {
            var concept = new BilingualConcept
            {
                ConceptName = conceptName,
                EnglishDescription = englishDesc,
                SymbolicRepresentation = symbolicForm,
                UniversalityScore = 0.7,
                RelatedConcepts = relatedConcepts ?? new List<string>(),
                EstablishedDate = DateTime.Now
            };
            concepts.Add(concept);
        }

        public void RecordSymbolicStatement(string symbolicForm, string englishTranslation,
                                          List<string> componentSymbols, List<string> notes = null)
        {
            var statement = new SymbolicStatement
            {
                SymbolicForm = symbolicForm,
                EnglishTranslation = englishTranslation,
                ComponentSymbols = new List<string>(componentSymbols),
                ComplexityLevel = CalculateComplexity(componentSymbols),
                InterpretationNotes = notes ?? new List<string>(),
                CreatedDate = DateTime.Now
            };
            statements.Add(statement);

            foreach (var symbol in componentSymbols)
            {
                if (usageFrequency.ContainsKey(symbol))
                    usageFrequency[symbol]++;
            }
        }

        private double CalculateComplexity(List<string> components)
        {
            if (components.Count == 0) return 0.1;
            return Math.Min((components.Count * 0.2), 1.0);
        }

        public string TranslateSymbolicToEnglish(string symbolicForm)
        {
            var statement = statements.FirstOrDefault(s => s.SymbolicForm == symbolicForm);
            if (statement != null)
                return statement.EnglishTranslation;

            string translation = "(";
            foreach (var component in symbolicForm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (symbolDictionary.ContainsKey(component))
                {
                    translation += symbolDictionary[component].MeaningEnglish + " ";
                }
                else
                {
                    translation += component + " ";
                }
            }
            return translation.Trim() + ")";
        }

        public string TranslateEnglishToSymbolic(string englishPhrase)
        {
            var statement = statements.FirstOrDefault(s => s.EnglishTranslation == englishPhrase);
            if (statement != null)
                return statement.SymbolicForm;

            var matchingConcept = concepts.FirstOrDefault(c =>
                c.EnglishDescription.Contains(englishPhrase, StringComparison.OrdinalIgnoreCase));

            if (matchingConcept != null)
                return matchingConcept.SymbolicRepresentation;

            return $"[{englishPhrase}]";
        }

        public void AnalyzeSymbolicStatement(string symbolicForm)
        {
            var statement = statements.FirstOrDefault(s => s.SymbolicForm == symbolicForm);
            if (statement == null) return;

            Console.WriteLine($"\n  Symbolic Statement: {statement.SymbolicForm}");
            Console.WriteLine($"  English Translation: {statement.EnglishTranslation}");
            Console.WriteLine($"  Complexity Level: {statement.ComplexityLevel * 100:F0}%");
            Console.WriteLine($"  Component Symbols: {string.Join(", ", statement.ComponentSymbols)}");
            if (statement.InterpretationNotes.Count > 0)
            {
                Console.WriteLine($"  Notes:");
                foreach (var note in statement.InterpretationNotes)
                {
                    Console.WriteLine($"    • {note}");
                }
            }
        }

        public void DisplaySymbol(string symbolChar)
        {
            if (!symbolDictionary.ContainsKey(symbolChar)) return;

            var symbol = symbolDictionary[symbolChar];
            Console.WriteLine($"\n  Symbol: {symbol.SymbolCharacter}");
            Console.WriteLine($"  Meaning: {symbol.MeaningEnglish}");
            Console.WriteLine($"  Category: {symbol.Category}");
            Console.WriteLine($"  Clarity: {symbol.Clarity * 100:F1}%");
            Console.WriteLine($"  Usage Frequency: {usageFrequency.GetValueOrDefault(symbolChar, 0)}");
            if (symbol.AlternativeMeanings.Count > 0)
            {
                Console.WriteLine($"  Alternative Meanings: {string.Join(", ", symbol.AlternativeMeanings)}");
            }
        }

        public void DisplayBilingualConcept(string conceptName)
        {
            var concept = concepts.FirstOrDefault(c => c.ConceptName == conceptName);
            if (concept == null) return;

            Console.WriteLine($"\n  Concept: {concept.ConceptName}");
            Console.WriteLine($"  English: {concept.EnglishDescription}");
            Console.WriteLine($"  Symbolic: {concept.SymbolicRepresentation}");
            Console.WriteLine($"  Universality: {concept.UniversalityScore * 100:F1}%");
            if (concept.RelatedConcepts.Count > 0)
            {
                Console.WriteLine($"  Related: {string.Join(", ", concept.RelatedConcepts)}");
            }
        }

        public Dictionary<string, int> GetMostUsedSymbols()
        {
            return usageFrequency.OrderByDescending(x => x.Value)
                .Take(10).ToDictionary(x => x.Key, x => x.Value);
        }

        public List<SymbolicStatement> GetStatementsByComplexity()
        {
            return statements.OrderByDescending(s => s.ComplexityLevel).ToList();
        }

        public double CalculateSymbolicCompression(int englishWords, int symbolicLength)
        {
            if (englishWords == 0) return 0.0;
            return 1.0 - ((double)symbolicLength / englishWords);
        }

        public void DisplayBilingualLexicon()
        {
            Console.WriteLine("\n  Bilingual Symbol Dictionary:");
            var topSymbols = GetMostUsedSymbols();
            foreach (var kvp in topSymbols.Take(5))
            {
                Console.WriteLine($"    {kvp.Key}: {symbolDictionary[kvp.Key].MeaningEnglish} (used {kvp.Value} times)");
            }
        }

        public void DisplayStatementComplexity()
        {
            Console.WriteLine("\n  Statement Complexity Ranking:");
            var byComplexity = GetStatementsByComplexity().Take(5);
            int rank = 1;
            foreach (var stmt in byComplexity)
            {
                Console.WriteLine($"    {rank}. Symbolic: {stmt.SymbolicForm}");
                Console.WriteLine($"       English: {stmt.EnglishTranslation}");
                Console.WriteLine($"       Complexity: {stmt.ComplexityLevel * 100:F0}%");
                rank++;
            }
        }

        public double GetBilingualLiteracyLevel()
        {
            if (statements.Count == 0) return 0.0;
            double avgUniversality = concepts.Count > 0 ? concepts.Average(c => c.UniversalityScore) : 0.5;
            double symbolCoverage = Math.Min((double)symbolDictionary.Count / 30.0, 1.0);
            return (avgUniversality * 0.5 + symbolCoverage * 0.5);
        }

        public string GetLiteracyLevel(double literacy)
        {
            return literacy switch
            {
                >= 0.85 => "Fluent - Complete bilingual expression",
                >= 0.70 => "Advanced - Strong symbolic vocabulary",
                >= 0.55 => "Intermediate - Developing symbolic literacy",
                >= 0.40 => "Basic - Foundational understanding",
                _ => "Emerging - Beginning symbolic learning"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Reading and Writing Symbolics and English               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new SymboticLanguageEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Core Symbolic Alphabet]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DefineSymbol("∅", "empty, void, nothing", "Fundamental");
        engine.DefineSymbol("⊙", "unity, wholeness, integration", "Fundamental");
        engine.DefineSymbol("⇄", "relationship, connection, exchange", "Relational");
        engine.DefineSymbol("↑", "increase, ascend, elevation", "Directional");
        engine.DefineSymbol("↓", "decrease, descend, reduction", "Directional");
        engine.DefineSymbol("∆", "change, transformation, difference", "Process");
        engine.DefineSymbol("≈", "similarity, approximation, resonance", "Comparative");
        engine.DefineSymbol("◇", "potential, possibility, expansion", "Potentiality");
        engine.DefineSymbol("§", "section, division, structure", "Organizational");
        engine.DefineSymbol("Ω", "completion, omega, endpoint", "Terminal");

        Console.WriteLine("  ✓ Defined 10 core symbolic characters");
        engine.DisplaySymbol("⊙");
        engine.DisplaySymbol("∆");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Bilingual Concepts]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateBilingualConcept("Growth",
            "A process of increasing in size, capacity, or understanding",
            "↑ ∆ ⊙",
            new List<string> { "Development", "Expansion", "Evolution" });

        engine.CreateBilingualConcept("Harmony",
            "The state of different elements working together in agreement",
            "⊙ ≈ ⇄",
            new List<string> { "Balance", "Coherence", "Alignment" });

        engine.CreateBilingualConcept("Transformation",
            "A complete change in form or function through intentional effort",
            "∆ ◇ ⊙",
            new List<string> { "Change", "Metamorphosis", "Conversion" });

        Console.WriteLine("  ✓ Created 3 bilingual concepts");
        engine.DisplayBilingualConcept("Growth");
        engine.DisplayBilingualConcept("Harmony");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Recording Symbolic Statements]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordSymbolicStatement("↑ ∆ ⊙",
            "Growth increases through transformation to unity",
            new List<string> { "↑", "∆", "⊙" },
            new List<string> { "Represents progressive development", "Emphasizes integration of change" });

        engine.RecordSymbolicStatement("⊙ ≈ ⇄",
            "Wholeness creates similarity and enables connection",
            new List<string> { "⊙", "≈", "⇄" },
            new List<string> { "Foundation for harmony", "Shows relationship basis" });

        engine.RecordSymbolicStatement("∆ ◇ § ↑",
            "Structured change creates expansion and upward movement",
            new List<string> { "∆", "◇", "§", "↑" },
            new List<string> { "Four-symbol statement", "Combines process and direction" });

        Console.WriteLine("  ✓ Recorded 3 symbolic statements");
        engine.AnalyzeSymbolicStatement("↑ ∆ ⊙");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Symbolic to English Translation]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Translating symbolic forms to English:");
        string sym1 = "↑ ∆ ⊙";
        string eng1 = engine.TranslateSymbolicToEnglish(sym1);
        Console.WriteLine($"    {sym1} → {eng1}");

        string sym2 = "⊙ ≈ ⇄";
        string eng2 = engine.TranslateSymbolicToEnglish(sym2);
        Console.WriteLine($"    {sym2} → {eng2}");

        string sym3 = "∆ ◇ § ↑";
        string eng3 = engine.TranslateSymbolicToEnglish(sym3);
        Console.WriteLine($"    {sym3} → {eng3}");

        Console.WriteLine("  ✓ Successful translation from symbolic to English");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: English to Symbolic Translation]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Translating English concepts to symbolic form:");
        string eng4 = "Growth increases through transformation to unity";
        string sym4 = engine.TranslateEnglishToSymbolic(eng4);
        Console.WriteLine($"    \"{eng4}\"");
        Console.WriteLine($"    → {sym4}");

        string eng5 = "Wholeness creates similarity and enables connection";
        string sym5 = engine.TranslateEnglishToSymbolic(eng5);
        Console.WriteLine($"    \"{eng5}\"");
        Console.WriteLine($"    → {sym5}");

        Console.WriteLine("  ✓ Successful translation from English to symbolic");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Bilingual Literacy Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayBilingualLexicon();

        Console.WriteLine("\n  Statement Complexity Analysis:");
        engine.DisplayStatementComplexity();

        double literacy = engine.GetBilingualLiteracyLevel();
        string literacyLevel = engine.GetLiteracyLevel(literacy);
        Console.WriteLine($"\n  Bilingual Literacy Level: {literacy * 100:F1}%");
        Console.WriteLine($"  Assessment: {literacyLevel}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Bilingual Communication Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Symbolic Language Framework:");
        Console.WriteLine("    Tier 1: Fundamental Symbols (empty, unity, connection)");
        Console.WriteLine("    Tier 2: Directional Symbols (increase, decrease, direction)");
        Console.WriteLine("    Tier 3: Process Symbols (change, transformation, dynamics)");
        Console.WriteLine("    Tier 4: Comparative Symbols (similarity, difference, resonance)");
        Console.WriteLine("    Tier 5: Composite Concepts (multiple symbols combined)");
        Console.WriteLine("\n  Translation Mechanics:");
        Console.WriteLine("    • One-to-one: Symbol ↔ English word");
        Console.WriteLine("    • Compositional: Symbol phrases ↔ Concept descriptions");
        Console.WriteLine("    • Semantic: Meaning preservation across languages");
        Console.WriteLine("    • Context: Interpretation based on surrounding symbols");
        Console.WriteLine("\n  Key Advantages of Symbolic Language:");
        Console.WriteLine("    ✓ Compression: Multiple words compressed to single symbols");
        Console.WriteLine("    ✓ Universality: Symbols transcend linguistic boundaries");
        Console.WriteLine("    ✓ Precision: Exact conceptual representation");
        Console.WriteLine("    ✓ Integration: Combines logic, emotion, and meaning");
        Console.WriteLine("    ✓ Conciseness: Efficient information transfer");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Bilingual symbolic/English system complete");
        Console.ResetColor();
    }
}
