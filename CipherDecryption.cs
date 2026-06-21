using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.Cryptanalysis
{
    /// <summary>
    /// Represents frequency analysis results for ciphertext.
    /// Used to identify patterns that help break ciphers.
    /// </summary>
    public class FrequencyAnalysis
    {
        public Dictionary<char, int> CharacterFrequencies { get; set; }
        public Dictionary<string, int> BigramFrequencies { get; set; }
        public Dictionary<string, int> TrigramFrequencies { get; set; }
        public double Entropy { get; set; }

        public FrequencyAnalysis()
        {
            CharacterFrequencies = new Dictionary<char, int>();
            BigramFrequencies = new Dictionary<string, int>();
            TrigramFrequencies = new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// Performs statistical analysis on ciphertext.
    /// </summary>
    public class CiphertextAnalyzer
    {
        private string text;

        public CiphertextAnalyzer(string text)
        {
            this.text = text.ToLower();
        }

        /// <summary>
        /// Analyzes character frequencies in the text.
        /// </summary>
        public FrequencyAnalysis AnalyzeFrequencies()
        {
            var analysis = new FrequencyAnalysis();

            // Character frequencies
            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    if (!analysis.CharacterFrequencies.ContainsKey(c))
                        analysis.CharacterFrequencies[c] = 0;
                    analysis.CharacterFrequencies[c]++;
                }
            }

            // Bigram frequencies
            for (int i = 0; i < text.Length - 1; i++)
            {
                if (char.IsLetter(text[i]) && char.IsLetter(text[i + 1]))
                {
                    string bigram = text.Substring(i, 2);
                    if (!analysis.BigramFrequencies.ContainsKey(bigram))
                        analysis.BigramFrequencies[bigram] = 0;
                    analysis.BigramFrequencies[bigram]++;
                }
            }

            // Trigram frequencies
            for (int i = 0; i < text.Length - 2; i++)
            {
                if (char.IsLetter(text[i]) && char.IsLetter(text[i + 1]) && char.IsLetter(text[i + 2]))
                {
                    string trigram = text.Substring(i, 3);
                    if (!analysis.TrigramFrequencies.ContainsKey(trigram))
                        analysis.TrigramFrequencies[trigram] = 0;
                    analysis.TrigramFrequencies[trigram]++;
                }
            }

            // Calculate entropy
            analysis.Entropy = CalculateEntropy(analysis.CharacterFrequencies);

            return analysis;
        }

        /// <summary>
        /// Calculates Shannon entropy of the text.
        /// Lower entropy suggests more structure (likely plaintext).
        /// </summary>
        private double CalculateEntropy(Dictionary<char, int> frequencies)
        {
            int totalLetters = frequencies.Values.Sum();
            double entropy = 0;

            foreach (var freq in frequencies.Values)
            {
                double probability = freq / (double)totalLetters;
                if (probability > 0)
                {
                    entropy -= probability * Math.Log2(probability);
                }
            }

            return entropy;
        }

        /// <summary>
        /// Calculates chi-squared statistic comparing to expected English frequencies.
        /// Lower values suggest closer match to English.
        /// </summary>
        public double CalculateChiSquaredStatistic(Dictionary<char, double> expectedFrequencies)
        {
            int totalLetters = text.Count(c => char.IsLetter(c));
            double chiSquared = 0;

            foreach (var entry in expectedFrequencies)
            {
                char c = entry.Key;
                double expected = entry.Value * totalLetters;
                double observed = 0;

                if (text.Contains(c))
                {
                    observed = text.Count(x => x == c);
                }

                if (expected > 0)
                {
                    chiSquared += Math.Pow(observed - expected, 2) / expected;
                }
            }

            return chiSquared;
        }

        /// <summary>
        /// Gets the most common characters in descending order.
        /// </summary>
        public List<(char character, int count)> GetMostFrequentCharacters(int topN = 10)
        {
            var analysis = AnalyzeFrequencies();
            return analysis.CharacterFrequencies
                .OrderByDescending(x => x.Value)
                .Take(topN)
                .Select(x => (x.Key, x.Value))
                .ToList();
        }
    }

    /// <summary>
    /// Implements Caesar cipher decryption.
    /// Caesar cipher shifts each letter by a fixed number of positions.
    /// </summary>
    public class CaesarCipherBreaker
    {
        private Dictionary<char, double> englishFrequencies;

        public CaesarCipherBreaker()
        {
            // Standard English letter frequencies
            englishFrequencies = new Dictionary<char, double>
            {
                { 'e', 0.1202 }, { 't', 0.0910 }, { 'a', 0.0812 },
                { 'o', 0.0768 }, { 'i', 0.0731 }, { 'n', 0.0695 },
                { 's', 0.0628 }, { 'h', 0.0609 }, { 'r', 0.0602 },
                { 'd', 0.0432 }, { 'l', 0.0398 }, { 'c', 0.0278 },
                { 'u', 0.0276 }, { 'm', 0.0241 }, { 'w', 0.0236 },
                { 'f', 0.0223 }, { 'g', 0.0202 }, { 'y', 0.0197 },
                { 'p', 0.0193 }, { 'b', 0.0149 }, { 'v', 0.0098 },
                { 'k', 0.0077 }, { 'j', 0.0015 }, { 'x', 0.0015 },
                { 'q', 0.0010 }, { 'z', 0.0007 }
            };
        }

        /// <summary>
        /// Attempts to break Caesar cipher by trying all 26 shifts.
        /// </summary>
        public List<(int shift, string plaintext, double score)> BreakCaesarCipher(string ciphertext)
        {
            var results = new List<(int, string, double)>();

            for (int shift = 0; shift < 26; shift++)
            {
                string decrypted = DecryptWithShift(ciphertext, shift);
                double score = ScorePlaintext(decrypted);
                results.Add((shift, decrypted, score));
            }

            return results.OrderByDescending(r => r.score).ToList();
        }

        /// <summary>
        /// Decrypts text with a specific Caesar shift.
        /// </summary>
        private string DecryptWithShift(string text, int shift)
        {
            string result = "";

            foreach (char c in text)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int charPos = c - baseChar;
                    int newPos = (charPos - shift + 26) % 26;
                    result += (char)(baseChar + newPos);
                }
                else
                {
                    result += c;
                }
            }

            return result;
        }

        /// <summary>
        /// Scores plaintext likelihood using chi-squared test.
        /// </summary>
        private double ScorePlaintext(string text)
        {
            var analyzer = new CiphertextAnalyzer(text);
            double chiSquared = analyzer.CalculateChiSquaredStatistic(englishFrequencies);
            return 1.0 / (1.0 + chiSquared);
        }
    }

    /// <summary>
    /// Implements substitution cipher breaking using frequency analysis and pattern matching.
    /// </summary>
    public class SubstitutionCipherBreaker
    {
        private Dictionary<char, double> englishFrequencies;
        private HashSet<string> dictionary;

        public SubstitutionCipherBreaker(HashSet<string> dictionary = null)
        {
            englishFrequencies = InitializeEnglishFrequencies();
            this.dictionary = dictionary ?? InitializeCommonWords();
        }

        private Dictionary<char, double> InitializeEnglishFrequencies()
        {
            return new Dictionary<char, double>
            {
                { 'e', 0.1202 }, { 't', 0.0910 }, { 'a', 0.0812 },
                { 'o', 0.0768 }, { 'i', 0.0731 }, { 'n', 0.0695 },
                { 's', 0.0628 }, { 'h', 0.0609 }, { 'r', 0.0602 }
            };
        }

        private HashSet<string> InitializeCommonWords()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
                "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                "this", "but", "his", "by", "from", "they", "we", "say", "her", "she"
            };
        }

        /// <summary>
        /// Attempts to break substitution cipher using frequency analysis.
        /// </summary>
        public Dictionary<char, char> BreakSubstitutionCipher(string ciphertext)
        {
            var analyzer = new CiphertextAnalyzer(ciphertext);
            var analysis = analyzer.AnalyzeFrequencies();

            // Get frequency-sorted characters
            var cipherFreq = analysis.CharacterFrequencies
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList();

            var englishFreq = englishFrequencies
                .OrderByDescending(x => x.Value)
                .Select(x => x.Key)
                .ToList();

            // Create initial mapping based on frequency
            var mapping = new Dictionary<char, char>();
            for (int i = 0; i < Math.Min(cipherFreq.Count, englishFreq.Count); i++)
            {
                mapping[cipherFreq[i]] = englishFreq[i];
            }

            return mapping;
        }

        /// <summary>
        /// Decrypts text using a substitution mapping.
        /// </summary>
        public string DecryptWithMapping(string ciphertext, Dictionary<char, char> mapping)
        {
            string result = "";

            foreach (char c in ciphertext)
            {
                if (char.IsLetter(c))
                {
                    char lower = char.ToLower(c);
                    if (mapping.ContainsKey(lower))
                    {
                        char decrypted = mapping[lower];
                        result += char.IsUpper(c) ? char.ToUpper(decrypted) : decrypted;
                    }
                    else
                    {
                        result += c;
                    }
                }
                else
                {
                    result += c;
                }
            }

            return result;
        }

        /// <summary>
        /// Scores decrypted text by matching words to dictionary.
        /// </summary>
        public int ScoreDecryption(string text)
        {
            var words = Regex.Split(text.ToLower(), @"[^a-z]+")
                .Where(w => w.Length > 0)
                .ToList();

            int matches = 0;
            foreach (var word in words)
            {
                if (dictionary.Contains(word))
                    matches++;
            }

            return matches;
        }
    }

    /// <summary>
    /// Detects and breaks transposition ciphers.
    /// </summary>
    public class TranspositionCipherBreaker
    {
        /// <summary>
        /// Attempts to find column transposition key.
        /// </summary>
        public List<(List<int> key, string plaintext, double score)> BreakColumnTransposition(string ciphertext, int keyLength)
        {
            var results = new List<(List<int>, string, double)>();

            // Generate permutations for key length
            var permutations = GeneratePermutations(keyLength);

            foreach (var perm in permutations)
            {
                string plaintext = DecryptColumnTransposition(ciphertext, perm);
                double score = ScorePlaintext(plaintext);
                results.Add((perm, plaintext, score));
            }

            return results.OrderByDescending(r => r.score).Take(10).ToList();
        }

        private string DecryptColumnTransposition(string ciphertext, List<int> key)
        {
            int keyLength = key.Count;
            int rows = (int)Math.Ceiling(ciphertext.Length / (double)keyLength);

            // Create grid
            char[,] grid = new char[rows, keyLength];
            for (int i = 0; i < ciphertext.Length; i++)
            {
                int col = i / rows;
                int row = i % rows;
                grid[row, col] = ciphertext[i];
            }

            // Reorder columns
            string result = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < keyLength; j++)
                {
                    int targetCol = Array.IndexOf(key.ToArray(), j);
                    result += grid[i, targetCol];
                }
            }

            return result;
        }

        private List<List<int>> GeneratePermutations(int length)
        {
            var result = new List<List<int>>();
            var numbers = Enumerable.Range(0, length).ToList();
            GeneratePermutationsHelper(numbers, 0, length - 1, result);
            return result;
        }

        private void GeneratePermutationsHelper(List<int> numbers, int l, int r, List<List<int>> result)
        {
            if (l == r)
            {
                result.Add(new List<int>(numbers));
            }
            else
            {
                for (int i = l; i <= r; i++)
                {
                    Swap(numbers, l, i);
                    GeneratePermutationsHelper(numbers, l + 1, r, result);
                    Swap(numbers, l, i);
                }
            }
        }

        private void Swap(List<int> list, int i, int j)
        {
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        private double ScorePlaintext(string text)
        {
            var words = Regex.Split(text.ToLower(), @"[^a-z]+")
                .Where(w => w.Length > 2)
                .ToList();

            // Simple scoring: more frequent words suggest valid plaintext
            var commonWords = new HashSet<string> { "the", "and", "for", "are", "but", "not", "you", "all", "can", "her" };
            int matches = words.Count(w => commonWords.Contains(w));

            return matches / (double)Math.Max(1, words.Count);
        }
    }

    /// <summary>
    /// Plaintext scorer using NLP techniques.
    /// </summary>
    public class PlaintextScorer
    {
        private Dictionary<string, double> bigramFrequencies;
        private HashSet<string> dictionary;

        public PlaintextScorer()
        {
            InitializeBigramFrequencies();
            InitializeDictionary();
        }

        private void InitializeBigramFrequencies()
        {
            bigramFrequencies = new Dictionary<string, double>
            {
                { "th", 0.0272 }, { "he", 0.0327 }, { "in", 0.0197 },
                { "er", 0.0191 }, { "an", 0.0173 }, { "re", 0.0153 },
                { "ed", 0.0165 }, { "nd", 0.0157 }, { "ha", 0.0160 },
                { "at", 0.0130 }, { "en", 0.0155 }, { "es", 0.0120 }
            };
        }

        private void InitializeDictionary()
        {
            dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "and", "for", "are", "but", "not", "you", "all",
                "can", "her", "was", "one", "our", "out", "day", "get",
                "has", "him", "his", "how", "its", "may", "new", "now",
                "old", "see", "way", "who", "boy", "did", "car", "man"
            };
        }

        /// <summary>
        /// Scores text likelihood as English plaintext.
        /// </summary>
        public double ScorePlaintext(string text)
        {
            text = text.ToLower();
            double score = 0;

            // Dictionary words
            var words = Regex.Split(text, @"[^a-z]+")
                .Where(w => w.Length > 0)
                .ToList();

            int dictMatches = words.Count(w => dictionary.Contains(w));
            score += (dictMatches / (double)Math.Max(1, words.Count)) * 0.5;

            // Bigram frequencies
            int validBigrams = 0;
            int bigramMatches = 0;

            for (int i = 0; i < text.Length - 1; i++)
            {
                if (char.IsLetter(text[i]) && char.IsLetter(text[i + 1]))
                {
                    string bigram = text.Substring(i, 2);
                    validBigrams++;

                    if (bigramFrequencies.ContainsKey(bigram))
                        bigramMatches++;
                }
            }

            if (validBigrams > 0)
            {
                score += (bigramMatches / (double)validBigrams) * 0.5;
            }

            return score;
        }
    }

    /// <summary>
    /// Examples demonstrating cipher decryption techniques.
    /// </summary>
    public static class CipherDecryptionExamples
    {
        public static void DemonstrateCaesarCipherBreaking()
        {
            Console.WriteLine("=== Caesar Cipher Breaking ===\n");

            // Example: "hello world" encrypted with shift 3
            string ciphertext = "khoor zruog";
            Console.WriteLine($"Ciphertext: {ciphertext}\n");

            var breaker = new CaesarCipherBreaker();
            var results = breaker.BreakCaesarCipher(ciphertext);

            Console.WriteLine("Top 5 candidates:");
            for (int i = 0; i < Math.Min(5, results.Count); i++)
            {
                Console.WriteLine($"  Shift {results[i].shift}: '{results[i].plaintext}' (Score: {results[i].score:F4})");
            }
        }

        public static void DemonstrateFrequencyAnalysis()
        {
            Console.WriteLine("\n=== Frequency Analysis ===\n");

            string ciphertext = "uryyb jbeyq guvf vf n grfg bs gur rapelcgvba flcgrz";
            Console.WriteLine($"Ciphertext: {ciphertext}\n");

            var analyzer = new CiphertextAnalyzer(ciphertext);
            var analysis = analyzer.AnalyzeFrequencies();

            Console.WriteLine("Character Frequencies:");
            var topChars = analyzer.GetMostFrequentCharacters(5);
            foreach (var (c, count) in topChars)
            {
                Console.WriteLine($"  '{c}': {count} occurrences");
            }

            Console.WriteLine($"\nEntropy: {analysis.Entropy:F4}");
            Console.WriteLine("(Lower entropy suggests more structure)");

            Console.WriteLine("\nMost Common Bigrams:");
            var topBigrams = analysis.BigramFrequencies
                .OrderByDescending(x => x.Value)
                .Take(5);
            foreach (var bigram in topBigrams)
            {
                Console.WriteLine($"  '{bigram.Key}': {bigram.Value}");
            }
        }

        public static void DemonstrateSubstitutionCipherBreaking()
        {
            Console.WriteLine("\n=== Substitution Cipher Breaking ===\n");

            // Simple substitution example
            string ciphertext = "wbh iye wbh buq wby iy bw wbh nqj";
            Console.WriteLine($"Ciphertext: {ciphertext}\n");

            var dictionary = new HashSet<string> { "the", "and", "for", "are", "you", "but", "not", "our", "out", "day" };
            var breaker = new SubstitutionCipherBreaker(dictionary);

            var mapping = breaker.BreakSubstitutionCipher(ciphertext);

            Console.WriteLine("Frequency-based mapping:");
            var top = mapping.OrderBy(x => x.Key).Take(10);
            foreach (var entry in top)
            {
                Console.WriteLine($"  '{entry.Key}' -> '{entry.Value}'");
            }

            string decrypted = breaker.DecryptWithMapping(ciphertext, mapping);
            Console.WriteLine($"\nDecrypted attempt: {decrypted}");
            Console.WriteLine($"Dictionary match score: {breaker.ScoreDecryption(decrypted)}");
        }

        public static void DemonstratePlaintextScoring()
        {
            Console.WriteLine("\n=== Plaintext Scoring ===\n");

            var scorer = new PlaintextScorer();

            string[] candidates = new string[]
            {
                "the quick brown fox jumps over the lazy dog",
                "zyx wlkcx jdoab rov nkmut oxyd zyx plat rov",
                "abcdefghijklmnopqrstuvwxyz",
                "this is a valid english sentence",
                "xyzzy plugh xyzzy plugh plugh"
            };

            Console.WriteLine("Scoring various texts:");
            foreach (string text in candidates)
            {
                double score = scorer.ScorePlaintext(text);
                Console.WriteLine($"Score: {score:F4} - '{text}'");
            }

            Console.WriteLine("\n(Higher scores indicate more likely English plaintext)");
        }

        public static void DemonstrateCipherAnalysisWorkflow()
        {
            Console.WriteLine("\n=== Complete Cipher Analysis Workflow ===\n");

            string ciphertext = "uryyb jbeyq";
            Console.WriteLine($"Step 1: Receive ciphertext: {ciphertext}\n");

            // Step 1: Frequency analysis
            Console.WriteLine("Step 2: Perform frequency analysis");
            var analyzer = new CiphertextAnalyzer(ciphertext);
            Console.WriteLine($"  Text length: {ciphertext.Length} characters");
            var topChars = analyzer.GetMostFrequentCharacters(3);
            Console.WriteLine($"  Most common: {string.Join(", ", topChars.Select(x => $"'{x.character}'"))}");

            // Step 2: Try Caesar cipher
            Console.WriteLine("\nStep 3: Test Caesar cipher (most common for simple ciphers)");
            var caesarBreaker = new CaesarCipherBreaker();
            var caesarResults = caesarBreaker.BreakCaesarCipher(ciphertext);
            Console.WriteLine($"  Best match: '{caesarResults[0].plaintext}' (Score: {caesarResults[0].score:F4})");

            // Step 3: Score plaintext
            Console.WriteLine("\nStep 4: Validate result using plaintext scoring");
            var scorer = new PlaintextScorer();
            double finalScore = scorer.ScorePlaintext(caesarResults[0].plaintext);
            Console.WriteLine($"  Plaintext score: {finalScore:F4}");

            Console.WriteLine($"\nResult: Likely plaintext is '{caesarResults[0].plaintext}'");
        }

        public static void DemonstrateTranspositionCipherAnalysis()
        {
            Console.WriteLine("\n=== Transposition Cipher Analysis ===\n");

            Console.WriteLine("Transposition ciphers rearrange letter positions.");
            Console.WriteLine("Analysis involves:");
            Console.WriteLine("  1. Estimating key length from text length");
            Console.WriteLine("  2. Testing permutations (key length limited)");
            Console.WriteLine("  3. Scoring results for valid English text");
            Console.WriteLine("  4. Pattern matching with known words");

            Console.WriteLine("\nExample: Column transposition with key length 3");
            var breaker = new TranspositionCipherBreaker();
            string ciphertext = "hlowrdlo";

            Console.WriteLine($"Ciphertext: {ciphertext}");
            Console.WriteLine("Testing possible arrangements...");

            var results = breaker.BreakColumnTransposition(ciphertext, 3);

            Console.WriteLine("\nTop candidates:");
            for (int i = 0; i < Math.Min(3, results.Count); i++)
            {
                Console.WriteLine($"  Key: [{string.Join(",", results[i].key)}] -> '{results[i].plaintext}' (Score: {results[i].score:F4})");
            }
        }

        public static void DemonstrateHybridApproach()
        {
            Console.WriteLine("\n=== Hybrid Cryptanalysis Approach ===\n");

            Console.WriteLine("Hybrid approach combines:");
            Console.WriteLine("  1. Statistical analysis (frequency, entropy)");
            Console.WriteLine("  2. Machine learning scoring (dictionary matching)");
            Console.WriteLine("  3. Pattern recognition (bigrams, trigrams)");
            Console.WriteLine("  4. Contextual validation");

            Console.WriteLine("\nWorkflow:");
            Console.WriteLine("  Step 1: Analyze statistical properties");
            Console.WriteLine("  Step 2: Generate candidate keys/mappings");
            Console.WriteLine("  Step 3: Score each decryption attempt");
            Console.WriteLine("  Step 4: Validate against language model");
            Console.WriteLine("  Step 5: Return ranked results");

            Console.WriteLine("\nAdvantages:");
            Console.WriteLine("  - More robust than single technique");
            Console.WriteLine("  - Handles various cipher types");
            Console.WriteLine("  - Better accuracy with combined scoring");
            Console.WriteLine("  - Can handle partial or damaged ciphertext");
        }

        public static void DemonstrateApplications()
        {
            Console.WriteLine("\n=== Cipher Decryption Applications ===\n");

            Console.WriteLine("1. Security/Cryptanalysis:");
            Console.WriteLine("   - Testing cipher strength");
            Console.WriteLine("   - Authorized security research");
            Console.WriteLine("   - Penetration testing engagement");

            Console.WriteLine("\n2. Intelligence Analysis:");
            Console.WriteLine("   - Decrypting intercepted communications");
            Console.WriteLine("   - Authorized by relevant authorities");
            Console.WriteLine("   - Legal intelligence gathering");

            Console.WriteLine("\n3. Historical Research:");
            Console.WriteLine("   - Decrypting historical documents");
            Console.WriteLine("   - Academic research");
            Console.WriteLine("   - Archaeological/archival work");

            Console.WriteLine("\n4. Educational:");
            Console.WriteLine("   - Cryptography courses");
            Console.WriteLine("   - Capture The Flag (CTF) competitions");
            Console.WriteLine("   - Security training programs");

            Console.WriteLine("\n⚠️  IMPORTANT: Only use these techniques on:");
            Console.WriteLine("   - Your own data");
            Console.WriteLine("   - Authorized penetration tests");
            Console.WriteLine("   - Competitive CTF events");
            Console.WriteLine("   - Educational purposes with permission");
            Console.WriteLine("   - Legally authorized security research");
        }
    }
}
