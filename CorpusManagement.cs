using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.CorpusTools
{
    /// <summary>
    /// Represents an annotated token with linguistic information.
    /// </summary>
    public class AnnotatedToken
    {
        public string Word { get; set; }
        public string PartOfSpeech { get; set; }
        public string NamedEntity { get; set; }
        public int Position { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public AnnotatedToken(string word, int position)
        {
            Word = word;
            Position = position;
            PartOfSpeech = "UNK";
            NamedEntity = "O";
            Metadata = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"{Word}/{PartOfSpeech}/{NamedEntity}";
        }
    }

    /// <summary>
    /// Represents an annotated sentence with linguistic annotations.
    /// </summary>
    public class AnnotatedSentence
    {
        public string Text { get; set; }
        public List<AnnotatedToken> Tokens { get; set; }
        public string Domain { get; set; }
        public Dictionary<string, string> DocumentMetadata { get; set; }

        public AnnotatedSentence(string text)
        {
            Text = text;
            Tokens = new List<AnnotatedToken>();
            Domain = "general";
            DocumentMetadata = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return string.Join(" ", Tokens.Select(t => t.ToString()));
        }
    }

    /// <summary>
    /// Corpus for storing and managing text data and annotations.
    /// </summary>
    public class Corpus
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AnnotatedSentence> Sentences { get; set; }
        public Dictionary<string, int> Vocabulary { get; set; }
        public Dictionary<string, int> PartOfSpeechCounts { get; set; }
        public Dictionary<string, int> NamedEntityCounts { get; set; }

        public Corpus(string name = "Unnamed Corpus")
        {
            Name = name;
            Description = "";
            Sentences = new List<AnnotatedSentence>();
            Vocabulary = new Dictionary<string, int>();
            PartOfSpeechCounts = new Dictionary<string, int>();
            NamedEntityCounts = new Dictionary<string, int>();
        }

        /// <summary>
        /// Adds a sentence to the corpus.
        /// </summary>
        public void AddSentence(AnnotatedSentence sentence)
        {
            Sentences.Add(sentence);
            UpdateStatistics(sentence);
        }

        /// <summary>
        /// Adds multiple sentences to the corpus.
        /// </summary>
        public void AddSentences(List<AnnotatedSentence> sentences)
        {
            foreach (var sentence in sentences)
            {
                AddSentence(sentence);
            }
        }

        /// <summary>
        /// Updates corpus statistics based on sentence.
        /// </summary>
        private void UpdateStatistics(AnnotatedSentence sentence)
        {
            foreach (var token in sentence.Tokens)
            {
                // Update vocabulary
                string word = token.Word.ToLower();
                if (!Vocabulary.ContainsKey(word))
                    Vocabulary[word] = 0;
                Vocabulary[word]++;

                // Update POS counts
                if (!PartOfSpeechCounts.ContainsKey(token.PartOfSpeech))
                    PartOfSpeechCounts[token.PartOfSpeech] = 0;
                PartOfSpeechCounts[token.PartOfSpeech]++;

                // Update NER counts
                if (!NamedEntityCounts.ContainsKey(token.NamedEntity))
                    NamedEntityCounts[token.NamedEntity] = 0;
                NamedEntityCounts[token.NamedEntity]++;
            }
        }

        /// <summary>
        /// Gets corpus statistics.
        /// </summary>
        public CorpusStatistics GetStatistics()
        {
            int totalTokens = Vocabulary.Values.Sum();
            int uniqueTokens = Vocabulary.Count;
            int totalSentences = Sentences.Count;
            double avgTokensPerSentence = totalSentences > 0 ? totalTokens / (double)totalSentences : 0;

            return new CorpusStatistics
            {
                TotalSentences = totalSentences,
                TotalTokens = totalTokens,
                UniqueTokens = uniqueTokens,
                AverageTokensPerSentence = avgTokensPerSentence,
                VocabularySize = uniqueTokens,
                LanguageComplexity = CalculateComplexity()
            };
        }

        private double CalculateComplexity()
        {
            // Type-token ratio: measure of vocabulary diversity
            return Vocabulary.Count > 0 ? (double)Vocabulary.Count / Vocabulary.Values.Sum() : 0;
        }
    }

    /// <summary>
    /// Statistics about a corpus.
    /// </summary>
    public class CorpusStatistics
    {
        public int TotalSentences { get; set; }
        public int TotalTokens { get; set; }
        public int UniqueTokens { get; set; }
        public double AverageTokensPerSentence { get; set; }
        public int VocabularySize { get; set; }
        public double LanguageComplexity { get; set; }

        public override string ToString()
        {
            return $"Corpus Statistics\n" +
                   $"{'='.ToString().PadRight(40, '=')}\n" +
                   $"Total Sentences: {TotalSentences}\n" +
                   $"Total Tokens: {TotalTokens}\n" +
                   $"Unique Tokens: {UniqueTokens}\n" +
                   $"Vocabulary Size: {VocabularySize}\n" +
                   $"Average Tokens/Sentence: {AverageTokensPerSentence:F2}\n" +
                   $"Language Complexity: {LanguageComplexity:F4}";
        }
    }

    /// <summary>
    /// Tokenizer for breaking text into tokens.
    /// </summary>
    public class Tokenizer
    {
        /// <summary>
        /// Tokenizes text into words.
        /// </summary>
        public static List<string> Tokenize(string text)
        {
            return Regex.Split(text, @"\s+")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }

        /// <summary>
        /// Tokenizes text into sentences.
        /// </summary>
        public static List<string> SentenceTokenize(string text)
        {
            return Regex.Split(text, @"[.!?]+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToList();
        }
    }

    /// <summary>
    /// Part-of-Speech tagger for annotating tokens.
    /// </summary>
    public class POSTagger
    {
        private Dictionary<string, string> wordToPOS;

        public POSTagger()
        {
            InitializeTagDictionary();
        }

        private void InitializeTagDictionary()
        {
            wordToPOS = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Nouns
                { "dog", "NN" }, { "cat", "NN" }, { "person", "NN" },
                { "book", "NN" }, { "house", "NN" }, { "tree", "NN" },

                // Verbs
                { "run", "VB" }, { "walk", "VB" }, { "talk", "VB" },
                { "is", "VBZ" }, { "are", "VBP" }, { "was", "VBD" },
                { "been", "VBN" }, { "running", "VBG" },

                // Adjectives
                { "good", "JJ" }, { "bad", "JJ" }, { "big", "JJ" },
                { "small", "JJ" }, { "happy", "JJ" }, { "sad", "JJ" },

                // Adverbs
                { "quickly", "RB" }, { "slowly", "RB" }, { "very", "RB" },

                // Determiners
                { "the", "DT" }, { "a", "DT" }, { "an", "DT" },

                // Prepositions
                { "in", "IN" }, { "on", "IN" }, { "at", "IN" },
                { "to", "TO" }, { "from", "IN" },

                // Pronouns
                { "i", "PRP" }, { "you", "PRP" }, { "he", "PRP" },
                { "she", "PRP" }, { "it", "PRP" }, { "we", "PRP" },
                { "they", "PRP" }
            };
        }

        /// <summary>
        /// Tags a sentence with POS tags.
        /// </summary>
        public void TagSentence(AnnotatedSentence sentence)
        {
            foreach (var token in sentence.Tokens)
            {
                if (wordToPOS.ContainsKey(token.Word))
                {
                    token.PartOfSpeech = wordToPOS[token.Word];
                }
                else
                {
                    token.PartOfSpeech = PredictPOS(token.Word);
                }
            }
        }

        /// <summary>
        /// Predicts POS tag for unknown word.
        /// </summary>
        private string PredictPOS(string word)
        {
            if (word.EndsWith("ing"))
                return "VBG";
            if (word.EndsWith("ed"))
                return "VBD";
            if (word.EndsWith("ly"))
                return "RB";
            if (word.EndsWith("tion"))
                return "NN";

            return "NN"; // Default to noun
        }
    }

    /// <summary>
    /// Named Entity Recognizer for identifying entities.
    /// </summary>
    public class NamedEntityRecognizer
    {
        private Dictionary<string, string> knownEntities;

        public NamedEntityRecognizer()
        {
            InitializeEntityDictionary();
        }

        private void InitializeEntityDictionary()
        {
            knownEntities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Persons
                { "john", "PERSON" }, { "mary", "PERSON" }, { "smith", "PERSON" },
                { "google", "ORG" }, { "apple", "ORG" }, { "microsoft", "ORG" },

                // Locations
                { "paris", "LOCATION" }, { "london", "LOCATION" }, { "tokyo", "LOCATION" },
                { "france", "LOCATION" }, { "usa", "LOCATION" }, { "new york", "LOCATION" }
            };
        }

        /// <summary>
        /// Tags a sentence with NER labels.
        /// </summary>
        public void TagSentence(AnnotatedSentence sentence)
        {
            for (int i = 0; i < sentence.Tokens.Count; i++)
            {
                string word = sentence.Tokens[i].Word;

                if (knownEntities.ContainsKey(word))
                {
                    sentence.Tokens[i].NamedEntity = "B-" + knownEntities[word];
                }
                else if (IsCapitalized(word) && i > 0 &&
                         sentence.Tokens[i - 1].NamedEntity.StartsWith("B-"))
                {
                    // Continuation of previous entity
                    string entityType = sentence.Tokens[i - 1].NamedEntity.Substring(2);
                    sentence.Tokens[i].NamedEntity = "I-" + entityType;
                }
                else
                {
                    sentence.Tokens[i].NamedEntity = "O"; // Outside
                }
            }
        }

        private bool IsCapitalized(string word)
        {
            return char.IsUpper(word[0]);
        }
    }

    /// <summary>
    /// Corpus splitter for creating train/validation/test sets.
    /// </summary>
    public class CorpusSplitter
    {
        private Random random;

        public CorpusSplitter()
        {
            random = new Random();
        }

        /// <summary>
        /// Splits corpus into train, validation, and test sets.
        /// </summary>
        public (Corpus train, Corpus validation, Corpus test) SplitCorpus(
            Corpus corpus, double trainRatio = 0.7, double valRatio = 0.15)
        {
            double testRatio = 1.0 - trainRatio - valRatio;

            var shuffled = corpus.Sentences.OrderBy(x => random.Next()).ToList();

            int trainSize = (int)(shuffled.Count * trainRatio);
            int valSize = (int)(shuffled.Count * valRatio);

            var trainSentences = shuffled.Take(trainSize).ToList();
            var valSentences = shuffled.Skip(trainSize).Take(valSize).ToList();
            var testSentences = shuffled.Skip(trainSize + valSize).ToList();

            var trainCorpus = new Corpus($"{corpus.Name} - Training");
            var valCorpus = new Corpus($"{corpus.Name} - Validation");
            var testCorpus = new Corpus($"{corpus.Name} - Test");

            trainCorpus.AddSentences(trainSentences);
            valCorpus.AddSentences(valSentences);
            testCorpus.AddSentences(testSentences);

            return (trainCorpus, valCorpus, testCorpus);
        }

        /// <summary>
        /// Splits corpus by domain.
        /// </summary>
        public Dictionary<string, Corpus> SplitByDomain(Corpus corpus)
        {
            var domainCorpora = new Dictionary<string, Corpus>();

            var groupedByDomain = corpus.Sentences.GroupBy(s => s.Domain);

            foreach (var group in groupedByDomain)
            {
                var domainCorpus = new Corpus($"{corpus.Name} - {group.Key} Domain");
                domainCorpus.AddSentences(group.ToList());
                domainCorpora[group.Key] = domainCorpus;
            }

            return domainCorpora;
        }
    }

    /// <summary>
    /// Corpus preprocessor for cleaning and normalizing text.
    /// </summary>
    public class CorpusPreprocessor
    {
        /// <summary>
        /// Cleans text by removing extra whitespace and special characters.
        /// </summary>
        public static string CleanText(string text)
        {
            // Remove extra whitespace
            text = Regex.Replace(text, @"\s+", " ");

            // Keep alphanumeric, spaces, and basic punctuation
            text = Regex.Replace(text, @"[^\w\s.!?-]", "");

            return text.Trim();
        }

        /// <summary>
        /// Normalizes text to lowercase.
        /// </summary>
        public static string NormalizeCase(string text)
        {
            return text.ToLower();
        }

        /// <summary>
        /// Removes stopwords from text.
        /// </summary>
        public static string RemoveStopwords(string text)
        {
            var stopwords = new HashSet<string>
            {
                "the", "a", "an", "and", "or", "but", "in", "on", "at", "to",
                "for", "of", "is", "was", "are", "be", "been", "being", "have",
                "has", "had", "do", "does", "did", "will", "would", "could", "should"
            };

            var words = text.Split(' ');
            var filtered = words.Where(w => !stopwords.Contains(w.ToLower()));

            return string.Join(" ", filtered);
        }

        /// <summary>
        /// Preprocesses an entire corpus.
        /// </summary>
        public static void PreprocessCorpus(Corpus corpus, bool cleanText = true,
            bool lowercase = true, bool removeStopwords = false)
        {
            foreach (var sentence in corpus.Sentences)
            {
                string text = sentence.Text;

                if (cleanText)
                    text = CleanText(text);

                if (lowercase)
                    text = NormalizeCase(text);

                if (removeStopwords)
                    text = RemoveStopwords(text);

                sentence.Text = text;
            }
        }
    }

    /// <summary>
    /// Corpus analyzer for examining linguistic properties.
    /// </summary>
    public class CorpusAnalyzer
    {
        /// <summary>
        /// Analyzes most common words.
        /// </summary>
        public static List<(string word, int count)> GetMostCommonWords(Corpus corpus, int topN = 10)
        {
            return corpus.Vocabulary
                .OrderByDescending(v => v.Value)
                .Take(topN)
                .Select(v => (v.Key, v.Value))
                .ToList();
        }

        /// <summary>
        /// Analyzes POS distribution.
        /// </summary>
        public static Dictionary<string, double> GetPOSDistribution(Corpus corpus)
        {
            int totalPOS = corpus.PartOfSpeechCounts.Values.Sum();
            var distribution = new Dictionary<string, double>();

            foreach (var pos in corpus.PartOfSpeechCounts)
            {
                distribution[pos.Key] = pos.Value / (double)totalPOS;
            }

            return distribution;
        }

        /// <summary>
        /// Analyzes NER distribution.
        /// </summary>
        public static Dictionary<string, double> GetNERDistribution(Corpus corpus)
        {
            int totalNER = corpus.NamedEntityCounts.Values.Sum();
            var distribution = new Dictionary<string, double>();

            foreach (var ner in corpus.NamedEntityCounts)
            {
                distribution[ner.Key] = ner.Value / (double)totalNER;
            }

            return distribution;
        }

        /// <summary>
        /// Calculates sentence length statistics.
        /// </summary>
        public static (double mean, double median, double stdDev) GetSentenceLengthStats(Corpus corpus)
        {
            var lengths = corpus.Sentences.Select(s => (double)s.Tokens.Count).ToList();

            double mean = lengths.Average();
            double median = lengths.OrderBy(x => x).ElementAt(lengths.Count / 2);

            double sumSquaredDifferences = lengths.Sum(x => Math.Pow(x - mean, 2));
            double stdDev = Math.Sqrt(sumSquaredDifferences / lengths.Count);

            return (mean, median, stdDev);
        }
    }

    /// <summary>
    /// Examples demonstrating corpus management.
    /// </summary>
    public static class CorpusExamples
    {
        public static void DemonstrateCorpusCreation()
        {
            Console.WriteLine("=== Corpus Creation and Annotation ===\n");

            var corpus = new Corpus("Movie Reviews");
            corpus.Description = "A corpus of movie reviews for sentiment analysis";

            // Create and add annotated sentences
            var sentences = new List<string>
            {
                "The movie was absolutely amazing and wonderful.",
                "I did not enjoy the film at all.",
                "Great acting performance in this movie."
            };

            var posTagger = new POSTagger();
            var nerTagger = new NamedEntityRecognizer();

            foreach (string text in sentences)
            {
                var sentence = new AnnotatedSentence(text);
                sentence.Domain = "movies";

                // Tokenize
                var words = Tokenizer.Tokenize(text);
                for (int i = 0; i < words.Count; i++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[i], i));
                }

                // Annotate
                posTagger.TagSentence(sentence);
                nerTagger.TagSentence(sentence);

                corpus.AddSentence(sentence);
            }

            Console.WriteLine($"Corpus: {corpus.Name}");
            Console.WriteLine($"Description: {corpus.Description}\n");

            Console.WriteLine("Annotated sentences:");
            foreach (var sentence in corpus.Sentences)
            {
                Console.WriteLine($"  {sentence.Text}");
                Console.WriteLine($"  Annotations: {sentence}\n");
            }

            Console.WriteLine(corpus.GetStatistics());
        }

        public static void DemonstrateCorpusStatistics()
        {
            Console.WriteLine("\n=== Corpus Statistics ===\n");

            var corpus = new Corpus("General Corpus");

            var sampleTexts = new List<string>
            {
                "Natural language processing is a key AI field.",
                "Machine learning models learn from data.",
                "Deep learning uses neural networks.",
                "Text mining extracts information from documents.",
                "Computational linguistics studies language structure."
            };

            var posTagger = new POSTagger();

            foreach (string text in sampleTexts)
            {
                var sentence = new AnnotatedSentence(text);
                var words = Tokenizer.Tokenize(text);

                for (int i = 0; i < words.Count; i++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[i], i));
                }

                posTagger.TagSentence(sentence);
                corpus.AddSentence(sentence);
            }

            var stats = corpus.GetStatistics();
            Console.WriteLine(stats);

            Console.WriteLine("\n\nMost Common Words:");
            var topWords = CorpusAnalyzer.GetMostCommonWords(corpus, 5);
            foreach (var (word, count) in topWords)
            {
                Console.WriteLine($"  {word}: {count}");
            }

            Console.WriteLine("\n\nPOS Distribution:");
            var posDistribution = CorpusAnalyzer.GetPOSDistribution(corpus);
            foreach (var pos in posDistribution.OrderByDescending(p => p.Value))
            {
                Console.WriteLine($"  {pos.Key}: {pos.Value:F4}");
            }
        }

        public static void DemonstrateCorpusSplitting()
        {
            Console.WriteLine("\n=== Corpus Splitting ===\n");

            var corpus = new Corpus("Large Corpus");

            // Create sample corpus
            for (int i = 0; i < 100; i++)
            {
                var sentence = new AnnotatedSentence($"Sample sentence number {i}.");
                sentence.Domain = i % 2 == 0 ? "sports" : "business";

                var words = Tokenizer.Tokenize(sentence.Text);
                for (int j = 0; j < words.Count; j++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[j], j));
                }

                corpus.AddSentence(sentence);
            }

            var splitter = new CorpusSplitter();
            var (train, val, test) = splitter.SplitCorpus(corpus, 0.7, 0.15);

            Console.WriteLine("Corpus Split Results:");
            Console.WriteLine($"  Original corpus: {corpus.Sentences.Count} sentences");
            Console.WriteLine($"  Training set: {train.Sentences.Count} sentences");
            Console.WriteLine($"  Validation set: {val.Sentences.Count} sentences");
            Console.WriteLine($"  Test set: {test.Sentences.Count} sentences");

            Console.WriteLine("\n\nDomain Split:");
            var domainCorpora = splitter.SplitByDomain(corpus);
            foreach (var domain in domainCorpora)
            {
                Console.WriteLine($"  {domain.Key}: {domain.Value.Sentences.Count} sentences");
            }
        }

        public static void DemonstrateAnnotation()
        {
            Console.WriteLine("\n=== POS and NER Annotation ===\n");

            var corpus = new Corpus("Annotated Corpus");

            var texts = new List<string>
            {
                "John Smith works at Google in California.",
                "Mary visited Paris and London last summer.",
                "The quick brown fox jumps over the lazy dog."
            };

            var posTagger = new POSTagger();
            var nerTagger = new NamedEntityRecognizer();

            foreach (string text in texts)
            {
                var sentence = new AnnotatedSentence(text);
                var words = Tokenizer.Tokenize(text);

                for (int i = 0; i < words.Count; i++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[i], i));
                }

                posTagger.TagSentence(sentence);
                nerTagger.TagSentence(sentence);

                corpus.AddSentence(sentence);
            }

            Console.WriteLine("Annotated Examples:\n");
            foreach (var sentence in corpus.Sentences)
            {
                Console.WriteLine($"Text: {sentence.Text}\n");
                Console.WriteLine("Tokens | POS | NER");
                Console.WriteLine("-".PadRight(40, '-'));

                foreach (var token in sentence.Tokens)
                {
                    Console.WriteLine($"{token.Word,-10} | {token.PartOfSpeech,-5} | {token.NamedEntity}");
                }
                Console.WriteLine();
            }
        }

        public static void DemonstratePreprocessing()
        {
            Console.WriteLine("\n=== Corpus Preprocessing ===\n");

            var corpus = new Corpus("Raw Corpus");

            var messyTexts = new List<string>
            {
                "This   is   A   MESSY   text   with   EXTRA   spaces!!!",
                "The quick BROWN fox Jumps over the lazy DOG.",
                "Text with @special #characters and Numbers 123"
            };

            foreach (string text in messyTexts)
            {
                var sentence = new AnnotatedSentence(text);
                var words = Tokenizer.Tokenize(text);

                for (int i = 0; i < words.Count; i++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[i], i));
                }

                corpus.AddSentence(sentence);
            }

            Console.WriteLine("Before Preprocessing:");
            foreach (var sentence in corpus.Sentences)
            {
                Console.WriteLine($"  {sentence.Text}");
            }

            // Preprocess
            CorpusPreprocessor.PreprocessCorpus(corpus, cleanText: true, lowercase: true);

            Console.WriteLine("\nAfter Preprocessing:");
            foreach (var sentence in corpus.Sentences)
            {
                Console.WriteLine($"  {sentence.Text}");
            }
        }

        public static void DemonstrateDomainSpecificCorpora()
        {
            Console.WriteLine("\n=== Domain-Specific Corpora ===\n");

            Console.WriteLine("Types of Domain-Specific Corpora:");
            Console.WriteLine("\n1. Penn Treebank (Parsing)");
            Console.WriteLine("   - Contains English text with syntactic annotations");
            Console.WriteLine("   - Used for training syntactic parsers");
            Console.WriteLine("   - Example tags: S, NP, VP, PP\n");

            Console.WriteLine("2. Biomedical Abstracts (Medical NLP)");
            Console.WriteLine("   - Contains medical research abstracts");
            Console.WriteLine("   - Used for biomedical information extraction");
            Console.WriteLine("   - Common entities: Protein, Gene, Disease\n");

            Console.WriteLine("3. Legal Information Institute (Legal NLP)");
            Console.WriteLine("   - Contains legal documents and case law");
            Console.WriteLine("   - Used for legal text mining and analysis");
            Console.WriteLine("   - Focus on legal entities and relationships\n");

            Console.WriteLine("4. Twitter Corpus (Social Media NLP)");
            Console.WriteLine("   - Contains tweets and social media posts");
            Console.WriteLine("   - Used for sentiment analysis");
            Console.WriteLine("   - Handles informal language and hashtags\n");

            Console.WriteLine("5. Scientific Papers Corpus (Academic NLP)");
            Console.WriteLine("   - Contains research papers");
            Console.WriteLine("   - Used for scholarly information extraction");
            Console.WriteLine("   - Focus on citations and references");
        }

        public static void DemonstrateCorpusEvaluation()
        {
            Console.WriteLine("\n=== Corpus Evaluation ===\n");

            var corpus = new Corpus("Evaluation Corpus");

            var sampleSentences = new List<string>
            {
                "The cat sat on the mat.",
                "Dogs and cats are common pets.",
                "Natural language processing is fascinating.",
                "Machine learning enables many applications.",
                "Deep neural networks learn representations."
            };

            var posTagger = new POSTagger();

            foreach (string text in sampleSentences)
            {
                var sentence = new AnnotatedSentence(text);
                var words = Tokenizer.Tokenize(text);

                for (int i = 0; i < words.Count; i++)
                {
                    sentence.Tokens.Add(new AnnotatedToken(words[i], i));
                }

                posTagger.TagSentence(sentence);
                corpus.AddSentence(sentence);
            }

            Console.WriteLine("Corpus Quality Metrics:\n");

            var (meanLen, medianLen, stdDev) = CorpusAnalyzer.GetSentenceLengthStats(corpus);
            Console.WriteLine("Sentence Length Statistics:");
            Console.WriteLine($"  Mean: {meanLen:F2} tokens");
            Console.WriteLine($"  Median: {medianLen:F2} tokens");
            Console.WriteLine($"  Std Dev: {stdDev:F2}\n");

            var stats = corpus.GetStatistics();
            Console.WriteLine("Annotation Coverage:");
            Console.WriteLine($"  Total tokens: {stats.TotalTokens}");
            Console.WriteLine($"  Unique tokens: {stats.UniqueTokens}");
            Console.WriteLine($"  Type-token ratio: {stats.LanguageComplexity:F4}");
            Console.WriteLine($"  Vocabulary diversity: {'Good' if stats.LanguageComplexity > 0.5 else 'Moderate'}\n");

            Console.WriteLine("POS Annotation Quality:");
            int taggedTokens = corpus.PartOfSpeechCounts.Where(p => p.Key != "UNK").Sum(p => p.Value);
            int totalTokens = corpus.PartOfSpeechCounts.Values.Sum();
            double coverage = (taggedTokens / (double)totalTokens) * 100;
            Console.WriteLine($"  Tagged coverage: {coverage:F2}%");
        }

        public static void DemonstrateCorpusUseCases()
        {
            Console.WriteLine("\n=== Corpus Use Cases in NLP ===\n");

            Console.WriteLine("1. Training NLP Models");
            Console.WriteLine("   - Provide labeled data for supervised learning");
            Console.WriteLine("   - Enable models to learn linguistic patterns");
            Console.WriteLine("   - Required for any machine learning approach\n");

            Console.WriteLine("2. Model Evaluation");
            Console.WriteLine("   - Benchmark model performance");
            Console.WriteLine("   - Compare different algorithms");
            Console.WriteLine("   - Measure generalization capability\n");

            Console.WriteLine("3. Linguistic Research");
            Console.WriteLine("   - Study language patterns and statistics");
            Console.WriteLine("   - Analyze linguistic phenomena");
            Console.WriteLine("   - Validate linguistic theories\n");

            Console.WriteLine("4. Resource Development");
            Console.WriteLine("   - Create lexicons and dictionaries");
            Console.WriteLine("   - Develop language models");
            Console.WriteLine("   - Build annotation guidelines\n");

            Console.WriteLine("5. Quality Assurance");
            Console.WriteLine("   - Test NLP systems");
            Console.WriteLine("   - Identify edge cases");
            Console.WriteLine("   - Validate system behavior\n");

            Console.WriteLine("6. Domain Adaptation");
            Console.WriteLine("   - Adapt models to specific domains");
            Console.WriteLine("   - Train domain-specific systems");
            Console.WriteLine("   - Improve performance on specialized tasks");
        }
    }
}
