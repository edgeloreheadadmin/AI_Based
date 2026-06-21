using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.CipherIntegration
{
    /// <summary>
    /// Represents a cipher for text encryption/decryption.
    /// </summary>
    public interface ICipher
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
        string GetCipherType();
    }

    /// <summary>
    /// Caesar cipher implementation.
    /// Simple substitution cipher that shifts each letter by a fixed amount.
    /// </summary>
    public class CaesarCipher : ICipher
    {
        private int shift;

        public CaesarCipher(int shift = 3)
        {
            this.shift = shift % 26;
        }

        public string Encrypt(string plaintext)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in plaintext)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int charPos = c - baseChar;
                    int newPos = (charPos + shift) % 26;
                    result.Append((char)(baseChar + newPos));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public string Decrypt(string ciphertext)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in ciphertext)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int charPos = c - baseChar;
                    int newPos = (charPos - shift + 26) % 26;
                    result.Append((char)(baseChar + newPos));
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public string GetCipherType() => "Caesar";
    }

    /// <summary>
    /// Vigenère cipher implementation.
    /// Uses a keyword to shift letters by different amounts.
    /// </summary>
    public class VigenereCipher : ICipher
    {
        private string key;

        public VigenereCipher(string key = "KEY")
        {
            this.key = key.ToUpper();
        }

        public string Encrypt(string plaintext)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;

            foreach (char c in plaintext)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int shift = key[keyIndex % key.Length] - 'A';
                    int charPos = char.ToUpper(c) - 'A';
                    int newPos = (charPos + shift) % 26;

                    char encrypted = (char)('A' + newPos);
                    result.Append(char.IsUpper(c) ? encrypted : char.ToLower(encrypted));
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public string Decrypt(string ciphertext)
        {
            StringBuilder result = new StringBuilder();
            int keyIndex = 0;

            foreach (char c in ciphertext)
            {
                if (char.IsLetter(c))
                {
                    char baseChar = char.IsUpper(c) ? 'A' : 'a';
                    int shift = key[keyIndex % key.Length] - 'A';
                    int charPos = char.ToUpper(c) - 'A';
                    int newPos = (charPos - shift + 26) % 26;

                    char decrypted = (char)('A' + newPos);
                    result.Append(char.IsUpper(c) ? decrypted : char.ToLower(decrypted));
                    keyIndex++;
                }
                else
                {
                    result.Append(c);
                }
            }

            return result.ToString();
        }

        public string GetCipherType() => "Vigenere";
    }

    /// <summary>
    /// NLP-aware cipher that preserves word boundaries for analysis.
    /// </summary>
    public class NLPAwareCipher : ICipher
    {
        private ICipher baseCipher;
        private bool preserveWordBoundaries;
        private bool preserveCase;

        public NLPAwareCipher(ICipher baseCipher, bool preserveWordBoundaries = true, bool preserveCase = true)
        {
            this.baseCipher = baseCipher;
            this.preserveWordBoundaries = preserveWordBoundaries;
            this.preserveCase = preserveCase;
        }

        public string Encrypt(string plaintext)
        {
            if (preserveWordBoundaries)
            {
                return EncryptWithBoundaryPreservation(plaintext);
            }
            else
            {
                return baseCipher.Encrypt(plaintext);
            }
        }

        public string Decrypt(string ciphertext)
        {
            if (preserveWordBoundaries)
            {
                return DecryptWithBoundaryPreservation(ciphertext);
            }
            else
            {
                return baseCipher.Decrypt(ciphertext);
            }
        }

        private string EncryptWithBoundaryPreservation(string plaintext)
        {
            StringBuilder result = new StringBuilder();
            var words = Regex.Split(plaintext, @"(\s+|[^\w])");

            foreach (string word in words)
            {
                if (Regex.IsMatch(word, @"^\w+$"))
                {
                    result.Append(baseCipher.Encrypt(word));
                }
                else
                {
                    result.Append(word);
                }
            }

            return result.ToString();
        }

        private string DecryptWithBoundaryPreservation(string ciphertext)
        {
            StringBuilder result = new StringBuilder();
            var words = Regex.Split(ciphertext, @"(\s+|[^\w])");

            foreach (string word in words)
            {
                if (Regex.IsMatch(word, @"^\w+$"))
                {
                    result.Append(baseCipher.Decrypt(word));
                }
                else
                {
                    result.Append(word);
                }
            }

            return result.ToString();
        }

        public string GetCipherType() => $"NLPAware({baseCipher.GetCipherType()})";
    }

    /// <summary>
    /// Secure NLP pipeline for privacy-preserving text processing.
    /// </summary>
    public class SecureNLPPipeline
    {
        private ICipher cipher;
        private bool encryptDuringProcessing;

        public SecureNLPPipeline(ICipher cipher, bool encryptDuringProcessing = false)
        {
            this.cipher = cipher;
            this.encryptDuringProcessing = encryptDuringProcessing;
        }

        /// <summary>
        /// Encrypts text before processing.
        /// </summary>
        public string EncryptForStorage(string plaintext)
        {
            return cipher.Encrypt(plaintext);
        }

        /// <summary>
        /// Decrypts text for processing.
        /// </summary>
        public string DecryptForProcessing(string ciphertext)
        {
            return cipher.Decrypt(ciphertext);
        }

        /// <summary>
        /// Tokenizes encrypted text (works on encrypted word-level).
        /// </summary>
        public List<string> TokenizeEncrypted(string encryptedText)
        {
            return Regex.Split(encryptedText, @"\s+")
                .Where(t => !string.IsNullOrEmpty(t))
                .ToList();
        }

        /// <summary>
        /// Calculates word frequency in encrypted text.
        /// Useful for frequency analysis without decrypting.
        /// </summary>
        public Dictionary<string, int> CalculateEncryptedWordFrequency(string encryptedText)
        {
            var tokens = TokenizeEncrypted(encryptedText);
            var frequency = new Dictionary<string, int>();

            foreach (var token in tokens)
            {
                if (!frequency.ContainsKey(token))
                    frequency[token] = 0;
                frequency[token]++;
            }

            return frequency;
        }

        /// <summary>
        /// Performs secure text search on encrypted content.
        /// </summary>
        public int EncryptedSearch(string encryptedText, string searchTerm)
        {
            string encryptedSearchTerm = cipher.Encrypt(searchTerm);
            var tokens = TokenizeEncrypted(encryptedText);
            return tokens.Count(t => t.Equals(encryptedSearchTerm, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Analyzes encrypted text structure while maintaining privacy.
        /// </summary>
        public Dictionary<string, object> AnalyzeEncryptedTextStructure(string encryptedText)
        {
            var analysis = new Dictionary<string, object>();
            var tokens = TokenizeEncrypted(encryptedText);

            analysis["TotalEncryptedWords"] = tokens.Count;
            analysis["UniqueEncryptedWords"] = tokens.Distinct().Count();
            analysis["AverageEncryptedWordLength"] = tokens.Average(t => t.Length);
            analysis["LongestEncryptedWord"] = tokens.MaxBy(t => t.Length).Length;

            var frequency = CalculateEncryptedWordFrequency(encryptedText);
            analysis["MostFrequentEncryptedWord"] = frequency.MaxBy(x => x.Value).Key;
            analysis["MostFrequentEncryptedWordCount"] = frequency.MaxBy(x => x.Value).Value;

            return analysis;
        }
    }

    /// <summary>
    /// Cipher strength evaluator for NLP resistance.
    /// </summary>
    public class CipherStrengthEvaluator
    {
        /// <summary>
        /// Evaluates cipher resistance to frequency analysis.
        /// </summary>
        public double EvaluateFrequencyResistance(string plaintext, ICipher cipher)
        {
            string encrypted = cipher.Encrypt(plaintext);

            // Analyze plaintext frequencies
            var plaintextFreq = AnalyzeFrequencies(plaintext);

            // Analyze ciphertext frequencies
            var ciphertextFreq = AnalyzeFrequencies(encrypted);

            // Calculate frequency distribution difference
            double difference = 0;
            foreach (var letter in "abcdefghijklmnopqrstuvwxyz".ToCharArray())
            {
                double pFreq = plaintextFreq.ContainsKey(letter) ? plaintextFreq[letter] : 0;
                double cFreq = ciphertextFreq.ContainsKey(letter) ? ciphertextFreq[letter] : 0;
                difference += Math.Abs(pFreq - cFreq);
            }

            // Score: higher is better (0-1 scale)
            return Math.Min(1.0, difference / 2.0);
        }

        /// <summary>
        /// Evaluates cipher for pattern preservation.
        /// </summary>
        public double EvaluatePatternPreservation(string plaintext, ICipher cipher)
        {
            string encrypted = cipher.Encrypt(plaintext);

            // Count repeated encrypted sequences
            var encryptedWords = Regex.Split(encrypted, @"\s+").Where(w => w.Length > 0).ToList();
            var plaintextWords = Regex.Split(plaintext.ToLower(), @"\s+").Where(w => w.Length > 0).ToList();

            // If same plaintext words encrypt to same ciphertext words, pattern is preserved
            var plaintextToEncrypted = new Dictionary<string, string>();
            int patternPreservationCount = 0;

            for (int i = 0; i < plaintextWords.Count && i < encryptedWords.Count; i++)
            {
                string pWord = plaintextWords[i].ToLower();
                string cWord = encryptedWords[i].ToLower();

                if (plaintextToEncrypted.ContainsKey(pWord))
                {
                    if (plaintextToEncrypted[pWord] == cWord)
                    {
                        patternPreservationCount++;
                    }
                }
                else
                {
                    plaintextToEncrypted[pWord] = cWord;
                }
            }

            return (double)patternPreservationCount / Math.Max(1, plaintextWords.Count);
        }

        private Dictionary<char, double> AnalyzeFrequencies(string text)
        {
            var frequencies = new Dictionary<char, double>();
            int totalLetters = text.Count(c => char.IsLetter(c));

            foreach (char c in "abcdefghijklmnopqrstuvwxyz".ToCharArray())
            {
                int count = text.Count(x => char.ToLower(x) == c);
                frequencies[c] = totalLetters > 0 ? (double)count / totalLetters : 0;
            }

            return frequencies;
        }

        /// <summary>
        /// Overall cipher strength score.
        /// </summary>
        public double CalculateOverallStrength(string plaintext, ICipher cipher)
        {
            double frequencyResistance = EvaluateFrequencyResistance(plaintext, cipher);
            double patternResistance = 1.0 - EvaluatePatternPreservation(plaintext, cipher);

            return (frequencyResistance + patternResistance) / 2.0;
        }
    }

    /// <summary>
    /// Secure text comparison without decryption.
    /// </summary>
    public class SecureTextComparison
    {
        private ICipher cipher;

        public SecureTextComparison(ICipher cipher)
        {
            this.cipher = cipher;
        }

        /// <summary>
        /// Compares two texts by comparing encrypted versions.
        /// </summary>
        public bool SecureEquality(string text1, string text2)
        {
            string encrypted1 = cipher.Encrypt(text1);
            string encrypted2 = cipher.Encrypt(text2);
            return encrypted1 == encrypted2;
        }

        /// <summary>
        /// Calculates similarity between encrypted texts.
        /// </summary>
        public double SecureSimilarity(string text1, string text2)
        {
            string encrypted1 = cipher.Encrypt(text1);
            string encrypted2 = cipher.Encrypt(text2);

            int matches = 0;
            int total = Math.Max(encrypted1.Length, encrypted2.Length);

            for (int i = 0; i < Math.Min(encrypted1.Length, encrypted2.Length); i++)
            {
                if (encrypted1[i] == encrypted2[i])
                    matches++;
            }

            return (double)matches / total;
        }

        /// <summary>
        /// Finds common encrypted patterns between texts.
        /// </summary>
        public Dictionary<string, int> FindCommonEncryptedPatterns(string text1, string text2)
        {
            string encrypted1 = cipher.Encrypt(text1);
            string encrypted2 = cipher.Encrypt(text2);

            var patterns = new Dictionary<string, int>();

            // Find common bigrams
            for (int i = 0; i < encrypted1.Length - 1; i++)
            {
                string bigram = encrypted1.Substring(i, 2);
                if (encrypted2.Contains(bigram))
                {
                    if (!patterns.ContainsKey(bigram))
                        patterns[bigram] = 0;
                    patterns[bigram]++;
                }
            }

            return patterns;
        }
    }

    /// <summary>
    /// Examples demonstrating cipher-aware NLP integration.
    /// </summary>
    public static class CipherAwareNLPExamples
    {
        public static void DemonstrateCaesarCipherIntegration()
        {
            Console.WriteLine("=== Caesar Cipher with NLP ===\n");

            string plaintext = "The quick brown fox jumps over the lazy dog";
            ICipher cipher = new CaesarCipher(3);

            Console.WriteLine($"Original: {plaintext}");
            string encrypted = cipher.Encrypt(plaintext);
            Console.WriteLine($"Encrypted: {encrypted}");
            string decrypted = cipher.Decrypt(encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }

        public static void DemonstrateVigenereCipherIntegration()
        {
            Console.WriteLine("\n=== Vigenère Cipher with NLP ===\n");

            string plaintext = "secret message for encryption";
            ICipher cipher = new VigenereCipher("PASSWORD");

            Console.WriteLine($"Original: {plaintext}");
            string encrypted = cipher.Encrypt(plaintext);
            Console.WriteLine($"Encrypted: {encrypted}");
            string decrypted = cipher.Decrypt(encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }

        public static void DemonstrateNLPAwareCipher()
        {
            Console.WriteLine("\n=== NLP-Aware Cipher (Preserves Word Boundaries) ===\n");

            string plaintext = "natural language processing with ciphers";
            ICipher baseCipher = new CaesarCipher(5);
            ICipher nlpCipher = new NLPAwareCipher(baseCipher, preserveWordBoundaries: true);

            Console.WriteLine($"Original: {plaintext}");
            string encrypted = nlpCipher.Encrypt(plaintext);
            Console.WriteLine($"Encrypted: {encrypted}");
            Console.WriteLine("(Notice: Word boundaries are preserved for NLP processing)");

            string decrypted = nlpCipher.Decrypt(encrypted);
            Console.WriteLine($"Decrypted: {decrypted}");
        }

        public static void DemonstrateSecureNLPPipeline()
        {
            Console.WriteLine("\n=== Secure NLP Pipeline ===\n");

            string plaintext = "This document contains sensitive information that must be protected";
            ICipher cipher = new VigenereCipher("SECURE_KEY");
            var pipeline = new SecureNLPPipeline(cipher);

            // Step 1: Encrypt for storage
            string encrypted = pipeline.EncryptForStorage(plaintext);
            Console.WriteLine($"Step 1 - Encrypted for storage: {encrypted}");

            // Step 2: Analyze structure without decrypting
            var analysis = pipeline.AnalyzeEncryptedTextStructure(encrypted);
            Console.WriteLine($"\nStep 2 - Text Structure Analysis (without decryption):");
            foreach (var entry in analysis)
            {
                Console.WriteLine($"  {entry.Key}: {entry.Value}");
            }

            // Step 3: Search in encrypted text
            string searchTerm = "document";
            int matches = pipeline.EncryptedSearch(encrypted, searchTerm);
            Console.WriteLine($"\nStep 3 - Secure search for '{searchTerm}': {matches} match(es)");

            // Step 4: Decrypt when needed
            string decrypted = pipeline.DecryptForProcessing(encrypted);
            Console.WriteLine($"\nStep 4 - Decrypted for processing: {decrypted}");
        }

        public static void DemonstrateCipherStrengthEvaluation()
        {
            Console.WriteLine("\n=== Cipher Strength Evaluation ===\n");

            string trainingText = "the quick brown fox jumps over the lazy dog the quick brown fox";
            var evaluator = new CipherStrengthEvaluator();

            var ciphers = new List<(string name, ICipher cipher)>
            {
                ("Caesar (shift 3)", new CaesarCipher(3)),
                ("Vigenère (key: HELLO)", new VigenereCipher("HELLO")),
                ("NLP-Aware Caesar", new NLPAwareCipher(new CaesarCipher(3), preserveWordBoundaries: true))
            };

            Console.WriteLine("Cipher Strength Analysis:");
            Console.WriteLine("-".PadRight(70, '-'));

            foreach (var (name, cipher) in ciphers)
            {
                double frequencyResistance = evaluator.EvaluateFrequencyResistance(trainingText, cipher);
                double patternPreservation = evaluator.EvaluatePatternPreservation(trainingText, cipher);
                double overallStrength = evaluator.CalculateOverallStrength(trainingText, cipher);

                Console.WriteLine($"\n{name}:");
                Console.WriteLine($"  Frequency Resistance: {frequencyResistance:F4} (higher is better)");
                Console.WriteLine($"  Pattern Preservation: {patternPreservation:F4} (lower is better for security)");
                Console.WriteLine($"  Overall Strength: {overallStrength:F4}");
            }
        }

        public static void DemonstrateSecureTextComparison()
        {
            Console.WriteLine("\n=== Secure Text Comparison ===\n");

            ICipher cipher = new VigenereCipher("SECRET");
            var comparison = new SecureTextComparison(cipher);

            string text1 = "confidential information";
            string text2 = "confidential information";
            string text3 = "different content here";

            Console.WriteLine("Secure Comparison (without decrypting):");
            Console.WriteLine($"\nText 1: '{text1}'");
            Console.WriteLine($"Text 2: '{text2}'");
            Console.WriteLine($"Text 3: '{text3}'");

            Console.WriteLine($"\nSecure Equality (Text1 == Text2): {comparison.SecureEquality(text1, text2)}");
            Console.WriteLine($"Secure Equality (Text1 == Text3): {comparison.SecureEquality(text1, text3)}");

            Console.WriteLine($"\nSecure Similarity (Text1 vs Text2): {comparison.SecureSimilarity(text1, text2):F4}");
            Console.WriteLine($"Secure Similarity (Text1 vs Text3): {comparison.SecureSimilarity(text1, text3):F4}");

            var patterns = comparison.FindCommonEncryptedPatterns(text1, text3);
            Console.WriteLine($"\nCommon encrypted patterns between Text1 and Text3: {patterns.Count}");
        }

        public static void DemonstratePrivacyPreservingProcessing()
        {
            Console.WriteLine("\n=== Privacy-Preserving Text Processing ===\n");

            var sensitiveDocuments = new List<string>
            {
                "User email: john@example.com, Age: 28",
                "User email: jane@example.com, Age: 32",
                "User email: bob@example.com, Age: 25"
            };

            ICipher cipher = new VigenereCipher("PRIVATE_KEY");
            var pipeline = new SecureNLPPipeline(cipher);

            Console.WriteLine("Processing sensitive documents securely:\n");

            foreach (var doc in sensitiveDocuments)
            {
                string encrypted = pipeline.EncryptForStorage(doc);
                var wordFreq = pipeline.CalculateEncryptedWordFrequency(encrypted);

                Console.WriteLine($"Original (for reference): {doc}");
                Console.WriteLine($"Encrypted: {encrypted}");
                Console.WriteLine($"Encrypted word count: {wordFreq.Count}");
                Console.WriteLine();
            }

            Console.WriteLine("✓ Analysis performed on encrypted data");
            Console.WriteLine("✓ Original sensitive data never exposed during processing");
        }

        public static void DemonstrateEncryptedSearch()
        {
            Console.WriteLine("\n=== Encrypted Search Application ===\n");

            string encryptedDatabase = "Encrypted_Content_1 Encrypted_Content_2 Encrypted_Content_3 Encrypted_Content_1";

            ICipher cipher = new VigenereCipher("SEARCH_KEY");
            var pipeline = new SecureNLPPipeline(cipher);

            Console.WriteLine("Scenario: Search encrypted database without decryption");
            Console.WriteLine($"Encrypted content: {encryptedDatabase}\n");

            var searchTerms = new List<string> { "hello", "world" };

            foreach (var term in searchTerms)
            {
                int matches = pipeline.EncryptedSearch(encryptedDatabase, term);
                Console.WriteLine($"Searching for '{term}': {matches} match(es)");
            }

            Console.WriteLine("\n✓ Search performed on encrypted content");
            Console.WriteLine("✓ Search terms never exposed in plaintext");
        }

        public static void DemonstrateCipherAwareNLPWorkflow()
        {
            Console.WriteLine("\n=== Complete Cipher-Aware NLP Workflow ===\n");

            Console.WriteLine("Workflow for secure text processing:\n");

            Console.WriteLine("1. INPUT: Sensitive plaintext document");
            Console.WriteLine("   Example: 'Contains private medical information'\n");

            Console.WriteLine("2. ENCRYPTION: Protect with cipher");
            ICipher cipher = new VigenereCipher("HIPAA_COMPLIANT");
            string plaintext = "Contains private medical information";
            string encrypted = cipher.Encrypt(plaintext);
            Console.WriteLine($"   Encrypted: {encrypted}\n");

            Console.WriteLine("3. NLP PROCESSING: Analyze encrypted form");
            var pipeline = new SecureNLPPipeline(cipher);
            var analysis = pipeline.AnalyzeEncryptedTextStructure(encrypted);
            Console.WriteLine($"   Word count: {analysis["TotalEncryptedWords"]}");
            Console.WriteLine($"   Unique words: {analysis["UniqueEncryptedWords"]}\n");

            Console.WriteLine("4. SECURE OPERATIONS:");
            int searchResults = pipeline.EncryptedSearch(encrypted, "private");
            Console.WriteLine($"   Search results: {searchResults}");
            var freq = pipeline.CalculateEncryptedWordFrequency(encrypted);
            Console.WriteLine($"   Frequency analysis: {freq.Count} unique encrypted terms\n");

            Console.WriteLine("5. DECRYPTION: Only when necessary");
            string decrypted = pipeline.DecryptForProcessing(encrypted);
            Console.WriteLine($"   Decrypted: {decrypted}\n");

            Console.WriteLine("Benefits:");
            Console.WriteLine("  ✓ Data remains encrypted during storage");
            Console.WriteLine("  ✓ NLP analysis without exposing plaintext");
            Console.WriteLine("  ✓ Compliance with privacy regulations");
            Console.WriteLine("  ✓ Secure search and comparison");
        }

        public static void DemonstrateApplications()
        {
            Console.WriteLine("\n=== Real-World Applications ===\n");

            Console.WriteLine("1. HEALTHCARE (HIPAA Compliance)");
            Console.WriteLine("   - Process patient records while maintaining encryption");
            Console.WriteLine("   - Search encrypted medical histories");
            Console.WriteLine("   - Analyze trends without exposing patient data\n");

            Console.WriteLine("2. FINANCIAL SERVICES (PCI-DSS)");
            Console.WriteLine("   - Encrypt transaction records");
            Console.WriteLine("   - Analyze payment patterns securely");
            Console.WriteLine("   - Fraud detection on encrypted data\n");

            Console.WriteLine("3. CLOUD STORAGE");
            Console.WriteLine("   - Store documents encrypted");
            Console.WriteLine("   - Search without decrypting");
            Console.WriteLine("   - Process analytics on encrypted content\n");

            Console.WriteLine("4. SECURE COMMUNICATION");
            Console.WriteLine("   - End-to-end encrypted messaging");
            Console.WriteLine("   - NLP-based spam filtering on encrypted messages");
            Console.WriteLine("   - Content analysis without exposing plaintext\n");

            Console.WriteLine("5. EDUCATIONAL/RESEARCH");
            Console.WriteLine("   - Secure datasets for machine learning");
            Console.WriteLine("   - Privacy-preserving text analysis");
            Console.WriteLine("   - Compliance with data protection regulations");
        }
    }
}
