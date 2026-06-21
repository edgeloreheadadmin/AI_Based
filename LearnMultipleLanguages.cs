using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class LearnMultipleLanguages
{
    public class Language
    {
        public string LanguageName { get; set; }
        public string LanguageCode { get; set; }
        public double ProficiencyLevel { get; set; }
        public int VocabularySize { get; set; }
        public List<string> CommonPhrases { get; set; }
        public Dictionary<string, string> Translations { get; set; }
        public DateTime StartedDate { get; set; }
    }

    public class LinguisticConcept
    {
        public string ConceptName { get; set; }
        public Dictionary<string, string> TranslationsAcrossLanguages { get; set; }
        public double UniversalityScore { get; set; }
        public List<string> RelatedConcepts { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class LearningSession
    {
        public string LanguageFocused { get; set; }
        public int WordsLearned { get; set; }
        public double PhrasePracticedCompleteness { get; set; }
        public double RetentionScore { get; set; }
        public List<string> NewVocabulary { get; set; }
        public DateTime SessionDate { get; set; }
    }

    public class PolyglotEngine
    {
        private Dictionary<string, Language> languages;
        private List<LinguisticConcept> concepts;
        private List<LearningSession> sessions;
        private Dictionary<string, int> crossLanguageLinks;

        public PolyglotEngine()
        {
            languages = new Dictionary<string, Language>();
            concepts = new List<LinguisticConcept>();
            sessions = new List<LearningSession>();
            crossLanguageLinks = new Dictionary<string, int>();
        }

        public void InitializeLanguage(string languageName, string languageCode)
        {
            var language = new Language
            {
                LanguageName = languageName,
                LanguageCode = languageCode,
                ProficiencyLevel = 0.1,
                VocabularySize = 0,
                CommonPhrases = new List<string>(),
                Translations = new Dictionary<string, string>(),
                StartedDate = DateTime.Now
            };
            languages[languageCode] = language;
            crossLanguageLinks[languageCode] = 0;
        }

        public void TeachVocabulary(string languageCode, string word, string translation)
        {
            if (!languages.ContainsKey(languageCode)) return;

            var language = languages[languageCode];
            language.Translations[word] = translation;
            language.VocabularySize++;
        }

        public void TeachPhrase(string languageCode, string phrase)
        {
            if (!languages.ContainsKey(languageCode)) return;

            var language = languages[languageCode];
            if (!language.CommonPhrases.Contains(phrase))
            {
                language.CommonPhrases.Add(phrase);
            }
        }

        public void CreateLinguisticConcept(string conceptName, Dictionary<string, string> translations,
                                          List<string> relatedConcepts = null)
        {
            var concept = new LinguisticConcept
            {
                ConceptName = conceptName,
                TranslationsAcrossLanguages = new Dictionary<string, string>(translations),
                UniversalityScore = CalculateUniversality(translations),
                RelatedConcepts = relatedConcepts ?? new List<string>(),
                CreatedDate = DateTime.Now
            };
            concepts.Add(concept);

            foreach (var langCode in translations.Keys)
            {
                if (crossLanguageLinks.ContainsKey(langCode))
                    crossLanguageLinks[langCode]++;
            }
        }

        private double CalculateUniversality(Dictionary<string, string> translations)
        {
            if (translations.Count == 0) return 0.0;
            return Math.Min((double)translations.Count / 10.0, 1.0);
        }

        public void ConductLearningSession(string languageCode, List<string> newWords, double retention)
        {
            if (!languages.ContainsKey(languageCode)) return;

            var session = new LearningSession
            {
                LanguageFocused = languageCode,
                WordsLearned = newWords.Count,
                PhrasePracticedCompleteness = Math.Min(newWords.Count / 10.0, 1.0),
                RetentionScore = Math.Min(retention, 1.0),
                NewVocabulary = new List<string>(newWords),
                SessionDate = DateTime.Now
            };
            sessions.Add(session);

            var language = languages[languageCode];
            foreach (var word in newWords)
            {
                language.CommonPhrases.Add(word);
            }

            double improvement = (newWords.Count * 0.02) + (retention * 0.08);
            language.ProficiencyLevel = Math.Min(language.ProficiencyLevel + improvement, 0.99);
        }

        public void TransferKnowledgeBetweenLanguages(string sourceLangCode, string targetLangCode)
        {
            if (!languages.ContainsKey(sourceLangCode) || !languages.ContainsKey(targetLangCode))
                return;

            var sourceLanguage = languages[sourceLangCode];
            var targetLanguage = languages[targetLangCode];

            foreach (var kvp in sourceLanguage.Translations)
            {
                if (!targetLanguage.Translations.ContainsKey(kvp.Key))
                {
                    targetLanguage.Translations[kvp.Key] = $"[cognate: {kvp.Value}]";
                }
            }

            double transferBoost = sourceLanguage.ProficiencyLevel * 0.15;
            targetLanguage.ProficiencyLevel = Math.Min(targetLanguage.ProficiencyLevel + transferBoost, 0.99);
        }

        public string Translate(string word, string fromLanguage, string toLanguage)
        {
            if (!languages.ContainsKey(fromLanguage) || !languages.ContainsKey(toLanguage))
                return "[Translation unavailable]";

            var sourceLanguage = languages[fromLanguage];
            if (sourceLanguage.Translations.ContainsKey(word))
            {
                return sourceLanguage.Translations[word];
            }

            return $"[{word} - not in vocabulary]";
        }

        public void DisplayLanguageProfile(string languageCode)
        {
            if (!languages.ContainsKey(languageCode)) return;

            var language = languages[languageCode];
            Console.WriteLine($"\n  Language: {language.LanguageName} ({language.LanguageCode})");
            Console.WriteLine($"  Proficiency: {language.ProficiencyLevel * 100:F1}%");
            Console.WriteLine($"  Vocabulary Size: {language.VocabularySize} words");
            Console.WriteLine($"  Common Phrases: {language.CommonPhrases.Count}");
            Console.WriteLine($"  Cross-Language Links: {crossLanguageLinks.GetValueOrDefault(languageCode, 0)}");
        }

        public void DisplayLinguisticConcept(string conceptName)
        {
            var concept = concepts.FirstOrDefault(c => c.ConceptName == conceptName);
            if (concept == null) return;

            Console.WriteLine($"\n  Concept: {concept.ConceptName}");
            Console.WriteLine($"  Universality Score: {concept.UniversalityScore * 100:F1}%");
            Console.WriteLine($"  Translations:");
            foreach (var kvp in concept.TranslationsAcrossLanguages)
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value}");
            }
            if (concept.RelatedConcepts.Count > 0)
            {
                Console.WriteLine($"  Related Concepts: {string.Join(", ", concept.RelatedConcepts)}");
            }
        }

        public Dictionary<string, double> GetLanguageProficiencies()
        {
            return languages.OrderByDescending(l => l.Value.ProficiencyLevel)
                .ToDictionary(l => l.Key, l => l.Value.ProficiencyLevel);
        }

        public List<string> GetMostConnectedLanguages()
        {
            return crossLanguageLinks.OrderByDescending(x => x.Value)
                .Select(x => x.Key).Take(5).ToList();
        }

        public double GetPolyglotCapability()
        {
            if (languages.Count == 0) return 0.0;
            double avgProficiency = languages.Values.Average(l => l.ProficiencyLevel);
            double languageCount = Math.Min(languages.Count / 10.0, 1.0);
            double conceptIntegration = concepts.Count > 0 ? Math.Min(concepts.Average(c => c.UniversalityScore), 1.0) : 0.0;

            return (avgProficiency * 0.5 + languageCount * 0.3 + conceptIntegration * 0.2);
        }

        public int GetTotalSessionCount()
        {
            return sessions.Count;
        }

        public double GetAverageRetention()
        {
            return sessions.Count > 0 ? sessions.Average(s => s.RetentionScore) : 0.0;
        }

        public string GetLanguageProficiencyLevel(double proficiency)
        {
            return proficiency switch
            {
                >= 0.85 => "Fluent - Advanced mastery",
                >= 0.70 => "Advanced - Strong proficiency",
                >= 0.55 => "Intermediate - Developing competence",
                >= 0.40 => "Basic - Functional communication",
                >= 0.25 => "Elementary - Limited vocabulary",
                _ => "Beginner - Starting to learn"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              Learning Multiple Languages                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new PolyglotEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Initializing Language Learning Programs]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.InitializeLanguage("Spanish", "ES");
        engine.InitializeLanguage("French", "FR");
        engine.InitializeLanguage("Mandarin", "ZH");
        engine.InitializeLanguage("Japanese", "JA");
        engine.InitializeLanguage("German", "DE");

        Console.WriteLine("  ✓ Initialized 5 language learning programs");
        engine.DisplayLanguageProfile("ES");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Building Vocabulary Across Languages]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Teaching vocabulary:");
        engine.TeachVocabulary("ES", "hello", "hola");
        engine.TeachVocabulary("ES", "goodbye", "adiós");
        engine.TeachVocabulary("ES", "thank you", "gracias");

        engine.TeachVocabulary("FR", "hello", "bonjour");
        engine.TeachVocabulary("FR", "goodbye", "au revoir");
        engine.TeachVocabulary("FR", "thank you", "merci");

        engine.TeachVocabulary("ZH", "hello", "你好 (nǐ hǎo)");
        engine.TeachVocabulary("ZH", "goodbye", "再见 (zàijiàn)");
        engine.TeachVocabulary("ZH", "thank you", "谢谢 (xièxie)");

        engine.TeachVocabulary("JA", "hello", "こんにちは");
        engine.TeachVocabulary("JA", "thank you", "ありがとう");

        engine.TeachVocabulary("DE", "hello", "hallo");
        engine.TeachVocabulary("DE", "thank you", "danke");

        Console.WriteLine("  ✓ Taught vocabulary across 5 languages");
        engine.DisplayLanguageProfile("FR");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Universal Linguistic Concepts]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateLinguisticConcept("Greeting",
            new Dictionary<string, string>
            {
                {"ES", "saludo"}, {"FR", "salutation"}, {"ZH", "问候"}, {"JA", "挨拶"}, {"DE", "Begrüßung"}
            },
            new List<string> { "Acknowledgment", "Social_Ritual" });

        engine.CreateLinguisticConcept("Gratitude",
            new Dictionary<string, string>
            {
                {"ES", "gratitud"}, {"FR", "gratitude"}, {"ZH", "感谢"}, {"JA", "感謝"}, {"DE", "Dankbarkeit"}
            },
            new List<string> { "Appreciation", "Respect" });

        engine.CreateLinguisticConcept("Farewell",
            new Dictionary<string, string>
            {
                {"ES", "despedida"}, {"FR", "adieu"}, {"ZH", "再见"}, {"JA", "別れ"}, {"DE", "Abschied"}
            },
            new List<string> { "Parting", "Closure" });

        Console.WriteLine("  ✓ Created 3 universal linguistic concepts");
        engine.DisplayLinguisticConcept("Greeting");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Conducting Learning Sessions]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Running language learning sessions:");
        engine.ConductLearningSession("ES", new List<string> { "agua", "comida", "amigo", "feliz" }, 0.85);
        engine.ConductLearningSession("FR", new List<string> { "eau", "nourriture", "ami", "heureux" }, 0.88);
        engine.ConductLearningSession("ZH", new List<string> { "水", "食物", "朋友", "快乐" }, 0.80);
        engine.ConductLearningSession("JA", new List<string> { "水", "食べ物", "友達" }, 0.82);

        Console.WriteLine("  ✓ Conducted 4 focused learning sessions");
        engine.DisplayLanguageProfile("ES");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Cross-Language Knowledge Transfer]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Transferring knowledge between languages:");
        engine.TransferKnowledgeBetweenLanguages("ES", "FR");
        engine.TransferKnowledgeBetweenLanguages("FR", "DE");
        engine.TransferKnowledgeBetweenLanguages("ES", "DE");

        Console.WriteLine("  ✓ Knowledge transferred across language pairs");
        engine.DisplayLanguageProfile("DE");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Polyglot Capability Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var proficiencies = engine.GetLanguageProficiencies();
        Console.WriteLine("  Language Proficiency Rankings:");
        int rank = 1;
        foreach (var kvp in proficiencies)
        {
            string level = engine.GetLanguageProficiencyLevel(kvp.Value);
            Console.WriteLine($"  #{rank}: {kvp.Key} - {kvp.Value * 100:F1}% ({level})");
            rank++;
        }

        var connectedLangs = engine.GetMostConnectedLanguages();
        Console.WriteLine($"\n  Most Connected Languages: {string.Join(", ", connectedLangs)}");
        Console.WriteLine($"  Total Learning Sessions: {engine.GetTotalSessionCount()}");
        Console.WriteLine($"  Average Retention: {engine.GetAverageRetention() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Polyglot Mastery Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        double polyglotCapability = engine.GetPolyglotCapability();
        Console.WriteLine($"  Polyglot Capability Index: {polyglotCapability * 100:F1}%");

        Console.WriteLine("\n  Language Learning Progression:");
        Console.WriteLine("    Stage 1: Language Initialization (0-10%)");
        Console.WriteLine("             Single words, basic awareness");
        Console.WriteLine("    Stage 2: Vocabulary Building (10-30%)");
        Console.WriteLine("             100+ words, simple phrases");
        Console.WriteLine("    Stage 3: Functional Communication (30-55%)");
        Console.WriteLine("             1000+ words, sentence formation");
        Console.WriteLine("    Stage 4: Advanced Proficiency (55-75%)");
        Console.WriteLine("             3000+ words, complex conversation");
        Console.WriteLine("    Stage 5: Fluent Mastery (75%+)");
        Console.WriteLine("             Idioms, nuance, native-like speech");
        Console.WriteLine("\n  Multi-Language Integration:");
        Console.WriteLine("    ✓ Cross-language vocabulary transfer");
        Console.WriteLine("    ✓ Universal concept identification");
        Console.WriteLine("    ✓ Comparative grammar analysis");
        Console.WriteLine("    ✓ Linguistic pattern recognition");
        Console.WriteLine("    ✓ Cumulative knowledge scaling");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Polyglot learning system complete");
        Console.ResetColor();
    }
}
