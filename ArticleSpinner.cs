using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.TextGeneration
{
    /// <summary>
    /// Interface for article spinning strategies.
    /// Article spinning creates variations of text while maintaining semantic meaning.
    /// </summary>
    public interface IArticleSpinner
    {
        /// <summary>
        /// Spins an entire article, returning a rewritten version.
        /// </summary>
        /// <param name="article">Original article text</param>
        /// <returns>Spun article text</returns>
        string SpinArticle(string article);

        /// <summary>
        /// Spins a single sentence.
        /// </summary>
        /// <param name="sentence">Original sentence</param>
        /// <returns>Spun sentence</returns>
        string SpinSentence(string sentence);

        /// <summary>
        /// Generates multiple variations of the same article.
        /// </summary>
        /// <param name="article">Original article text</param>
        /// <param name="variationCount">Number of variations to generate</param>
        /// <returns>List of article variations</returns>
        List<string> GenerateVariations(string article, int variationCount);
    }

    /// <summary>
    /// Manages synonym mappings for word replacement during spinning.
    /// </summary>
    public class SynonymDictionary
    {
        private Dictionary<string, List<string>> synonyms;
        private Random random;

        public SynonymDictionary()
        {
            random = new Random();
            synonyms = InitializeDefaultSynonyms();
        }

        private Dictionary<string, List<string>> InitializeDefaultSynonyms()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                { "cat", new List<string> { "feline", "kitty", "tom", "mouser" } },
                { "dog", new List<string> { "canine", "puppy", "hound", "pooch" } },
                { "sat", new List<string> { "perched", "rested", "positioned", "reclined" } },
                { "mat", new List<string> { "rug", "carpet", "floor covering", "pad" } },
                { "animal", new List<string> { "creature", "beast", "organism", "fauna" } },
                { "important", new List<string> { "crucial", "vital", "significant", "essential" } },
                { "good", new List<string> { "excellent", "fine", "outstanding", "superior" } },
                { "bad", new List<string> { "poor", "inadequate", "inferior", "subpar" } },
                { "big", new List<string> { "large", "huge", "substantial", "considerable" } },
                { "small", new List<string> { "tiny", "little", "compact", "miniature" } },
                { "fast", new List<string> { "rapid", "swift", "speedy", "quick" } },
                { "slow", new List<string> { "sluggish", "gradual", "unhurried", "leisurely" } },
                { "happy", new List<string> { "joyful", "cheerful", "content", "pleased" } },
                { "sad", new List<string> { "unhappy", "sorrowful", "melancholy", "dejected" } },
                { "create", new List<string> { "generate", "produce", "make", "construct" } },
                { "article", new List<string> { "piece", "write-up", "composition", "text" } },
                { "content", new List<string> { "material", "information", "substance", "text" } },
                { "algorithm", new List<string> { "procedure", "method", "technique", "process" } },
                { "useful", new List<string> { "beneficial", "helpful", "practical", "valuable" } },
                { "quality", new List<string> { "standard", "caliber", "grade", "level" } }
            };
        }

        /// <summary>
        /// Adds a custom synonym mapping.
        /// </summary>
        public void AddSynonym(string word, string synonym)
        {
            if (!synonyms.ContainsKey(word))
            {
                synonyms[word] = new List<string>();
            }
            synonyms[word].Add(synonym);
        }

        /// <summary>
        /// Gets a random synonym for a word, or returns the original word if no synonyms exist.
        /// </summary>
        public string GetRandomSynonym(string word)
        {
            if (synonyms.TryGetValue(word.ToLower(), out var synList) && synList.Count > 0)
            {
                return synList[random.Next(synList.Count)];
            }
            return word;
        }

        /// <summary>
        /// Gets all synonyms for a word.
        /// </summary>
        public List<string> GetSynonyms(string word)
        {
            if (synonyms.TryGetValue(word.ToLower(), out var synList))
            {
                return new List<string>(synList);
            }
            return new List<string>();
        }
    }

    /// <summary>
    /// Basic article spinner that uses synonym replacement.
    /// Replaces words with synonyms from a dictionary.
    /// </summary>
    public class BasicArticleSpinner : IArticleSpinner
    {
        private SynonymDictionary synonymDictionary;
        private Random random;
        private const double ReplacementProbability = 0.4;

        public BasicArticleSpinner(SynonymDictionary synonymDictionary = null)
        {
            this.synonymDictionary = synonymDictionary ?? new SynonymDictionary();
            this.random = new Random();
        }

        public string SpinArticle(string article)
        {
            string[] sentences = SplitIntoSentences(article);
            string[] spunSentences = new string[sentences.Length];

            for (int i = 0; i < sentences.Length; i++)
            {
                spunSentences[i] = SpinSentence(sentences[i]);
            }

            return string.Join(" ", spunSentences);
        }

        public string SpinSentence(string sentence)
        {
            string[] words = sentence.Split(new[] { ' ' }, StringSplitOptions.None);
            string[] spunWords = new string[words.Length];

            for (int i = 0; i < words.Length; i++)
            {
                if (random.NextDouble() < ReplacementProbability)
                {
                    string cleanWord = CleanWord(words[i]);
                    string synonym = synonymDictionary.GetRandomSynonym(cleanWord);
                    spunWords[i] = PreserveCase(words[i], synonym);
                }
                else
                {
                    spunWords[i] = words[i];
                }
            }

            return string.Join(" ", spunWords);
        }

        public List<string> GenerateVariations(string article, int variationCount)
        {
            List<string> variations = new List<string>();
            for (int i = 0; i < variationCount; i++)
            {
                variations.Add(SpinArticle(article));
            }
            return variations;
        }

        private string CleanWord(string word)
        {
            return Regex.Replace(word, @"[^\w]", "");
        }

        private string PreserveCase(string original, string replacement)
        {
            if (string.IsNullOrEmpty(replacement)) return original;

            string punctuation = "";
            string cleanOriginal = original;

            if (!char.IsLetterOrDigit(original[original.Length - 1]))
            {
                punctuation = original[original.Length - 1].ToString();
                cleanOriginal = original.Substring(0, original.Length - 1);
            }

            if (char.IsUpper(cleanOriginal[0]))
            {
                replacement = char.ToUpper(replacement[0]) + replacement.Substring(1);
            }

            return replacement + punctuation;
        }

        private string[] SplitIntoSentences(string text)
        {
            return Regex.Split(text, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }
    }

    /// <summary>
    /// Advanced spinner that combines synonym replacement with sentence restructuring.
    /// </summary>
    public class AdvancedArticleSpinner : IArticleSpinner
    {
        private SynonymDictionary synonymDictionary;
        private Random random;

        public AdvancedArticleSpinner(SynonymDictionary synonymDictionary = null)
        {
            this.synonymDictionary = synonymDictionary ?? new SynonymDictionary();
            this.random = new Random();
        }

        public string SpinArticle(string article)
        {
            string[] sentences = SplitIntoSentences(article);
            List<string> spunSentences = new List<string>();

            foreach (var sentence in sentences)
            {
                if (random.NextDouble() > 0.3)
                {
                    spunSentences.Add(RestructureSentence(sentence));
                }
                else
                {
                    spunSentences.Add(sentence);
                }
            }

            return string.Join(" ", spunSentences);
        }

        public string SpinSentence(string sentence)
        {
            return RestructureSentence(sentence);
        }

        public List<string> GenerateVariations(string article, int variationCount)
        {
            List<string> variations = new List<string>();
            for (int i = 0; i < variationCount; i++)
            {
                variations.Add(SpinArticle(article));
            }
            return variations;
        }

        private string RestructureSentence(string sentence)
        {
            string[] words = sentence.Split(new[] { ' ' }, StringSplitOptions.None);

            if (words.Length < 3) return sentence;

            switch (random.Next(3))
            {
                case 0:
                    return ReverseSentenceStructure(words);
                case 1:
                    return ReplaceWithSynonyms(sentence);
                case 2:
                    return AddIntensifierOrQualifier(sentence);
                default:
                    return sentence;
            }
        }

        private string ReverseSentenceStructure(string[] words)
        {
            List<string> reversed = new List<string>(words);
            reversed.Reverse();
            return string.Join(" ", reversed);
        }

        private string ReplaceWithSynonyms(string sentence)
        {
            string[] words = sentence.Split(new[] { ' ' }, StringSplitOptions.None);
            for (int i = 0; i < words.Length; i++)
            {
                string cleanWord = Regex.Replace(words[i], @"[^\w]", "");
                string synonym = synonymDictionary.GetRandomSynonym(cleanWord);
                if (synonym != cleanWord)
                {
                    words[i] = synonym;
                }
            }
            return string.Join(" ", words);
        }

        private string AddIntensifierOrQualifier(string sentence)
        {
            string[] intensifiers = { "definitely", "certainly", "undoubtedly", "truly", "remarkably" };
            string[] qualifiers = { "arguably", "perhaps", "conceivably", "potentially", "possibly" };

            if (random.NextDouble() > 0.5)
            {
                return intensifiers[random.Next(intensifiers.Length)] + " " + sentence;
            }
            else
            {
                return sentence + " " + qualifiers[random.Next(qualifiers.Length)] + ".";
            }
        }

        private string[] SplitIntoSentences(string text)
        {
            return Regex.Split(text, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();
        }
    }

    /// <summary>
    /// Generates content variations with different levels of adaptation.
    /// Useful for creating social media posts or alternate versions.
    /// </summary>
    public class ContentVariationGenerator
    {
        private SynonymDictionary synonymDictionary;
        private Random random;

        public ContentVariationGenerator(SynonymDictionary synonymDictionary = null)
        {
            this.synonymDictionary = synonymDictionary ?? new SynonymDictionary();
            this.random = new Random();
        }

        /// <summary>
        /// Creates a condensed version suitable for social media.
        /// </summary>
        public string CreateSocialMediaVersion(string article, int maxLength = 280)
        {
            string[] sentences = Regex.Split(article, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            string summary = "";
            foreach (var sentence in sentences)
            {
                if ((summary + " " + sentence).Length <= maxLength)
                {
                    summary += " " + sentence;
                }
                else
                {
                    break;
                }
            }

            return summary.Trim();
        }

        /// <summary>
        /// Creates an SEO-optimized version focused on specific keywords.
        /// </summary>
        public string CreateSEOOptimizedVersion(string article, string keyword)
        {
            string result = article;

            if (!result.ToLower().Contains(keyword.ToLower()))
            {
                string[] sentences = Regex.Split(result, @"(?<=[.!?])\s+")
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();

                if (sentences.Length > 0)
                {
                    sentences[0] = $"In the context of {keyword}, {sentences[0].ToLower()}";
                    result = string.Join(" ", sentences);
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a version with improved readability.
        /// </summary>
        public string CreateReadableVersion(string article)
        {
            string[] sentences = Regex.Split(article, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            List<string> improvedSentences = new List<string>();
            foreach (var sentence in sentences)
            {
                if (sentence.Length > 150)
                {
                    improvedSentences.AddRange(SplitLongSentence(sentence));
                }
                else
                {
                    improvedSentences.Add(sentence);
                }
            }

            return string.Join(" ", improvedSentences);
        }

        private List<string> SplitLongSentence(string sentence)
        {
            List<string> parts = new List<string>();
            string[] words = sentence.Split(' ');
            string current = "";

            foreach (var word in words)
            {
                if ((current + " " + word).Length > 75 && !string.IsNullOrEmpty(current))
                {
                    parts.Add(current.Trim() + ".");
                    current = word;
                }
                else
                {
                    current += " " + word;
                }
            }

            if (!string.IsNullOrEmpty(current))
            {
                parts.Add(current.Trim());
            }

            return parts;
        }
    }

    /// <summary>
    /// Quality assessment for spun content to ensure it maintains coherence and readability.
    /// </summary>
    public class SpunContentQualityAssessor
    {
        /// <summary>
        /// Calculates a quality score (0-100) for spun content.
        /// </summary>
        public int AssessQuality(string originalText, string spunText)
        {
            int score = 100;

            if (string.IsNullOrWhiteSpace(spunText))
                return 0;

            if (spunText.Length < originalText.Length * 0.5)
                score -= 20;

            if (spunText.Length > originalText.Length * 1.5)
                score -= 15;

            if (!HasProperSentenceStructure(spunText))
                score -= 25;

            if (HasGrammaricalIssues(spunText))
                score -= 20;

            if (CalculateSimilarity(originalText, spunText) < 0.4)
                score -= 15;

            return Math.Max(0, score);
        }

        private bool HasProperSentenceStructure(string text)
        {
            return Regex.IsMatch(text, @"[A-Z][^.!?]*[.!?]");
        }

        private bool HasGrammaricalIssues(string text)
        {
            return text.Contains("  ") || text.Contains("\n\n");
        }

        private double CalculateSimilarity(string text1, string text2)
        {
            string[] words1 = text1.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] words2 = text2.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int commonWords = words1.Intersect(words2).Count();
            int totalWords = words1.Union(words2).Count();

            return totalWords == 0 ? 0 : (double)commonWords / totalWords;
        }
    }

    /// <summary>
    /// Examples demonstrating article spinning techniques and use cases.
    /// </summary>
    public static class ArticleSpinningExamples
    {
        public static void DemonstrateSynonymReplacement()
        {
            Console.WriteLine("=== Basic Synonym Replacement ===\n");

            string original = "The cat sat on the mat.";
            var spinner = new BasicArticleSpinner();

            Console.WriteLine($"Original: {original}");
            Console.WriteLine("\nSpun variations:");
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"  {i + 1}. {spinner.SpinSentence(original)}");
            }
        }

        public static void DemonstrateArticleSpinning()
        {
            Console.WriteLine("\n=== Full Article Spinning ===\n");

            string article = "Article spinning is a technique in natural language processing. " +
                           "It uses algorithms to rewrite articles. " +
                           "This makes content appear unique to search engines.";

            var spinner = new BasicArticleSpinner();
            string spun = spinner.SpinArticle(article);

            Console.WriteLine($"Original:\n{article}\n");
            Console.WriteLine($"Spun:\n{spun}");
        }

        public static void DemonstrateMultipleVariations()
        {
            Console.WriteLine("\n=== Generating Multiple Content Variations ===\n");

            string article = "Good content is important for websites. " +
                           "Quality material helps readers understand topics.";

            var spinner = new BasicArticleSpinner();
            var variations = spinner.GenerateVariations(article, 3);

            Console.WriteLine("Original:");
            Console.WriteLine(article);
            Console.WriteLine("\nVariations:");
            for (int i = 0; i < variations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {variations[i]}");
            }
        }

        public static void DemonstrateSocialMediaAdaptation()
        {
            Console.WriteLine("\n=== Creating Social Media Version ===\n");

            string article = "Article spinning is an important technique used in natural language processing " +
                           "and content management. It helps create variations of content for different platforms. " +
                           "This technique can be useful for creating social media posts, adjusted content for different audiences, " +
                           "and optimized versions for search engines.";

            var generator = new ContentVariationGenerator();
            string socialVersion = generator.CreateSocialMediaVersion(article, 280);

            Console.WriteLine($"Original ({article.Length} chars):\n{article}\n");
            Console.WriteLine($"Social Media Version ({socialVersion.Length} chars):\n{socialVersion}");
        }

        public static void DemonstrateSEOOptimization()
        {
            Console.WriteLine("\n=== Creating SEO-Optimized Version ===\n");

            string article = "Natural language processing algorithms are useful for text analysis. " +
                           "They help in various applications.";

            var generator = new ContentVariationGenerator();
            string seoVersion = generator.CreateSEOOptimizedVersion(article, "NLP techniques");

            Console.WriteLine($"Original:\n{article}\n");
            Console.WriteLine($"SEO Optimized:\n{seoVersion}");
        }

        public static void DemonstrateQualityAssessment()
        {
            Console.WriteLine("\n=== Quality Assessment of Spun Content ===\n");

            string original = "Article spinning creates different versions of content.";
            var spinner = new BasicArticleSpinner();
            var assessor = new SpunContentQualityAssessor();

            Console.WriteLine($"Original:\n{original}\n");
            Console.WriteLine("Spun versions with quality scores:");

            for (int i = 0; i < 5; i++)
            {
                string spun = spinner.SpinSentence(original);
                int quality = assessor.AssessQuality(original, spun);
                Console.WriteLine($"  Score: {quality:D3}/100 - {spun}");
            }
        }

        public static void DemonstrateResponsibleSpinning()
        {
            Console.WriteLine("\n=== Responsible Article Spinning for Different Purposes ===\n");

            string article = "Natural language processing is an important field of artificial intelligence. " +
                           "It focuses on enabling computers to understand and process human language effectively.";

            Console.WriteLine("Original Article:");
            Console.WriteLine(article);
            Console.WriteLine();

            var basicSpinner = new BasicArticleSpinner();
            var generator = new ContentVariationGenerator();
            var assessor = new SpunContentQualityAssessor();

            string variation1 = basicSpinner.SpinArticle(article);
            Console.WriteLine($"Variation for different audience:\n{variation1}");
            Console.WriteLine($"Quality Score: {assessor.AssessQuality(article, variation1)}/100\n");

            string socialVersion = generator.CreateSocialMediaVersion(article, 150);
            Console.WriteLine($"Adapted for social media:\n{socialVersion}");
            Console.WriteLine($"Length: {socialVersion.Length} characters\n");

            string readableVersion = generator.CreateReadableVersion(article);
            Console.WriteLine($"Improved readability version:\n{readableVersion}");
        }
    }
}
