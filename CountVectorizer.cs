using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NaturalLanguageProcessing.Vectorization
{
    /// <summary>
    /// Represents a sparse matrix entry (row, column, value).
    /// Used for efficient storage of sparse document-term matrices.
    /// </summary>
    public class SparseEntry
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int Value { get; set; }

        public SparseEntry(int row, int column, int value)
        {
            Row = row;
            Column = column;
            Value = value;
        }
    }

    /// <summary>
    /// Sparse matrix representation for efficient storage.
    /// </summary>
    public class SparseMatrix
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public List<SparseEntry> Entries { get; set; }

        public SparseMatrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Entries = new List<SparseEntry>();
        }

        /// <summary>
        /// Converts to dense array representation.
        /// </summary>
        public int[][] ToDenseArray()
        {
            int[][] dense = new int[Rows][];
            for (int i = 0; i < Rows; i++)
            {
                dense[i] = new int[Columns];
            }

            foreach (var entry in Entries)
            {
                dense[entry.Row][entry.Column] = entry.Value;
            }

            return dense;
        }

        /// <summary>
        /// Gets row as dense array.
        /// </summary>
        public int[] GetRow(int rowIndex)
        {
            int[] row = new int[Columns];
            var rowEntries = Entries.Where(e => e.Row == rowIndex);

            foreach (var entry in rowEntries)
            {
                row[entry.Column] = entry.Value;
            }

            return row;
        }

        /// <summary>
        /// Gets sparsity ratio (percentage of zero entries).
        /// </summary>
        public double GetSparsity()
        {
            int totalElements = Rows * Columns;
            int zeroElements = totalElements - Entries.Count;
            return (zeroElements / (double)totalElements) * 100;
        }
    }

    /// <summary>
    /// CountVectorizer for converting text to numerical vectors.
    /// </summary>
    public class CountVectorizer
    {
        private Dictionary<string, int> vocabulary;
        private List<string> tokens;
        private int minFrequency;
        private int maxFrequency;
        private bool lowercase;
        private int ngramMin;
        private int ngramMax;
        private bool isFitted;

        public CountVectorizer(int minFrequency = 1, int maxFrequency = int.MaxValue,
            bool lowercase = true, int ngramMin = 1, int ngramMax = 1)
        {
            this.vocabulary = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            this.tokens = new List<string>();
            this.minFrequency = minFrequency;
            this.maxFrequency = maxFrequency;
            this.lowercase = lowercase;
            this.ngramMin = ngramMin;
            this.ngramMax = ngramMax;
            this.isFitted = false;
        }

        /// <summary>
        /// Tokenizes text into individual tokens.
        /// </summary>
        private List<string> Tokenize(string text)
        {
            if (lowercase)
                text = text.ToLower();

            // Split by non-word characters
            var tokens = Regex.Split(text, @"[^\w]+")
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();

            return tokens;
        }

        /// <summary>
        /// Generates n-grams from tokens.
        /// </summary>
        private List<string> GenerateNgrams(List<string> tokens)
        {
            var ngrams = new List<string>();

            for (int n = ngramMin; n <= ngramMax; n++)
            {
                for (int i = 0; i <= tokens.Count - n; i++)
                {
                    string ngram = string.Join(" ", tokens.Skip(i).Take(n));
                    ngrams.Add(ngram);
                }
            }

            return ngrams;
        }

        /// <summary>
        /// Fits the vectorizer to documents and builds vocabulary.
        /// </summary>
        public void Fit(List<string> documents)
        {
            var tokenFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            // Count token frequencies
            foreach (string document in documents)
            {
                var docTokens = Tokenize(document);
                var ngrams = GenerateNgrams(docTokens);

                foreach (string token in ngrams)
                {
                    if (!tokenFrequencies.ContainsKey(token))
                        tokenFrequencies[token] = 0;
                    tokenFrequencies[token]++;
                }
            }

            // Build vocabulary with frequency constraints
            vocabulary.Clear();
            int index = 0;

            foreach (var entry in tokenFrequencies
                .Where(e => e.Value >= minFrequency && e.Value <= maxFrequency)
                .OrderBy(e => e.Key))
            {
                vocabulary[entry.Key] = index++;
            }

            isFitted = true;
        }

        /// <summary>
        /// Transforms a single document into a vector.
        /// </summary>
        public int[] TransformSingle(string document)
        {
            if (!isFitted)
                throw new InvalidOperationException("Vectorizer must be fitted first.");

            var vector = new int[vocabulary.Count];
            var docTokens = Tokenize(document);
            var ngrams = GenerateNgrams(docTokens);

            foreach (string ngram in ngrams)
            {
                if (vocabulary.ContainsKey(ngram))
                {
                    vector[vocabulary[ngram]]++;
                }
            }

            return vector;
        }

        /// <summary>
        /// Transforms multiple documents into document-term matrix (dense).
        /// </summary>
        public int[][] Transform(List<string> documents)
        {
            if (!isFitted)
                throw new InvalidOperationException("Vectorizer must be fitted first.");

            int[][] matrix = new int[documents.Count][];

            for (int i = 0; i < documents.Count; i++)
            {
                matrix[i] = TransformSingle(documents[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Transforms documents into sparse matrix representation.
        /// </summary>
        public SparseMatrix TransformSparse(List<string> documents)
        {
            if (!isFitted)
                throw new InvalidOperationException("Vectorizer must be fitted first.");

            var sparseMatrix = new SparseMatrix(documents.Count, vocabulary.Count);

            for (int docIndex = 0; docIndex < documents.Count; docIndex++)
            {
                var vector = TransformSingle(documents[docIndex]);

                for (int colIndex = 0; colIndex < vector.Length; colIndex++)
                {
                    if (vector[colIndex] > 0)
                    {
                        sparseMatrix.Entries.Add(new SparseEntry(docIndex, colIndex, vector[colIndex]));
                    }
                }
            }

            return sparseMatrix;
        }

        /// <summary>
        /// Fit and transform in one step.
        /// </summary>
        public int[][] FitTransform(List<string> documents)
        {
            Fit(documents);
            return Transform(documents);
        }

        /// <summary>
        /// Gets the vocabulary.
        /// </summary>
        public Dictionary<string, int> GetVocabulary()
        {
            return new Dictionary<string, int>(vocabulary);
        }

        /// <summary>
        /// Gets the feature names (vocabulary terms).
        /// </summary>
        public List<string> GetFeatureNames()
        {
            return vocabulary
                .OrderBy(v => v.Value)
                .Select(v => v.Key)
                .ToList();
        }

        /// <summary>
        /// Gets vocabulary size.
        /// </summary>
        public int GetVocabularySize()
        {
            return vocabulary.Count;
        }

        /// <summary>
        /// Checks if vectorizer is fitted.
        /// </summary>
        public bool IsFitted()
        {
            return isFitted;
        }

        /// <summary>
        /// Gets the inverse vocabulary (index to term).
        /// </summary>
        public Dictionary<int, string> GetInverseVocabulary()
        {
            return vocabulary.ToDictionary(v => v.Value, v => v.Key);
        }
    }

    /// <summary>
    /// TF-IDF Vectorizer for weighted term frequencies.
    /// </summary>
    public class TfidfVectorizer
    {
        private CountVectorizer countVectorizer;
        private Dictionary<string, double> idfWeights;
        private int numDocuments;

        public TfidfVectorizer(int minFrequency = 1, int maxFrequency = int.MaxValue,
            bool lowercase = true, int ngramMin = 1, int ngramMax = 1)
        {
            this.countVectorizer = new CountVectorizer(minFrequency, maxFrequency, lowercase, ngramMin, ngramMax);
            this.idfWeights = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Fits the TF-IDF vectorizer.
        /// </summary>
        public void Fit(List<string> documents)
        {
            // First fit the count vectorizer
            countVectorizer.Fit(documents);
            numDocuments = documents.Count;

            // Calculate IDF weights
            var documentFrequencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var vocabulary = countVectorizer.GetVocabulary();

            // Count documents containing each term
            foreach (string doc in documents)
            {
                var vector = countVectorizer.TransformSingle(doc);
                var features = countVectorizer.GetFeatureNames();

                for (int i = 0; i < vector.Length; i++)
                {
                    if (vector[i] > 0)
                    {
                        string term = features[i];
                        if (!documentFrequencies.ContainsKey(term))
                            documentFrequencies[term] = 0;
                        documentFrequencies[term]++;
                    }
                }
            }

            // Calculate IDF for each term
            idfWeights.Clear();
            foreach (string term in vocabulary.Keys)
            {
                int df = documentFrequencies.ContainsKey(term) ? documentFrequencies[term] : 1;
                double idf = Math.Log((double)numDocuments / df);
                idfWeights[term] = idf;
            }
        }

        /// <summary>
        /// Transforms document to TF-IDF vector.
        /// </summary>
        public double[] TransformSingle(string document)
        {
            var countVector = countVectorizer.TransformSingle(document);
            var features = countVectorizer.GetFeatureNames();
            var tfidfVector = new double[countVector.Length];

            // Calculate TF (term frequency)
            int totalTerms = countVector.Sum();

            for (int i = 0; i < countVector.Length; i++)
            {
                if (countVector[i] > 0)
                {
                    double tf = countVector[i] / (double)totalTerms;
                    double idf = idfWeights.ContainsKey(features[i]) ? idfWeights[features[i]] : 0;
                    tfidfVector[i] = tf * idf;
                }
            }

            return tfidfVector;
        }

        /// <summary>
        /// Transforms multiple documents to TF-IDF matrix.
        /// </summary>
        public double[][] Transform(List<string> documents)
        {
            double[][] matrix = new double[documents.Count][];

            for (int i = 0; i < documents.Count; i++)
            {
                matrix[i] = TransformSingle(documents[i]);
            }

            return matrix;
        }

        /// <summary>
        /// Fit and transform in one step.
        /// </summary>
        public double[][] FitTransform(List<string> documents)
        {
            Fit(documents);
            return Transform(documents);
        }

        public CountVectorizer GetCountVectorizer()
        {
            return countVectorizer;
        }

        public Dictionary<string, double> GetIdfWeights()
        {
            return new Dictionary<string, double>(idfWeights);
        }
    }

    /// <summary>
    /// Analyzer for vectorized text data.
    /// </summary>
    public class VectorizerAnalyzer
    {
        /// <summary>
        /// Calculates document similarity using cosine similarity.
        /// </summary>
        public static double CosineSimilarity(int[] vector1, int[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Calculates document similarity for TF-IDF vectors.
        /// </summary>
        public static double CosineSimilarity(double[] vector1, double[] vector2)
        {
            if (vector1.Length != vector2.Length)
                throw new ArgumentException("Vectors must have the same length");

            double dotProduct = 0;
            double magnitude1 = 0;
            double magnitude2 = 0;

            for (int i = 0; i < vector1.Length; i++)
            {
                dotProduct += vector1[i] * vector2[i];
                magnitude1 += vector1[i] * vector1[i];
                magnitude2 += vector2[i] * vector2[i];
            }

            magnitude1 = Math.Sqrt(magnitude1);
            magnitude2 = Math.Sqrt(magnitude2);

            if (magnitude1 == 0 || magnitude2 == 0) return 0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// Gets most important features for a document.
        /// </summary>
        public static List<(string feature, int count)> GetTopFeatures(int[] vector,
            List<string> featureNames, int topN = 10)
        {
            var features = new List<(string, int)>();

            for (int i = 0; i < vector.Length; i++)
            {
                if (vector[i] > 0)
                {
                    features.Add((featureNames[i], vector[i]));
                }
            }

            return features
                .OrderByDescending(f => f.count)
                .Take(topN)
                .ToList();
        }

        /// <summary>
        /// Analyzes feature importance across documents.
        /// </summary>
        public static Dictionary<string, double> GetFeatureImportance(int[][] matrix,
            List<string> featureNames)
        {
            var importance = new Dictionary<string, double>();

            for (int i = 0; i < featureNames.Count; i++)
            {
                int totalCount = 0;
                int documentsContaining = 0;

                foreach (int[] row in matrix)
                {
                    if (row[i] > 0)
                    {
                        totalCount += row[i];
                        documentsContaining++;
                    }
                }

                // Calculate importance as average count and document frequency
                double avgCount = matrix.Length > 0 ? totalCount / (double)matrix.Length : 0;
                double docFreq = matrix.Length > 0 ? documentsContaining / (double)matrix.Length : 0;
                importance[featureNames[i]] = avgCount * docFreq;
            }

            return importance;
        }
    }

    /// <summary>
    /// Examples demonstrating CountVectorizer usage.
    /// </summary>
    public static class CountVectorizerExamples
    {
        public static void DemonstrateBasicCountVectorizer()
        {
            Console.WriteLine("=== Basic CountVectorizer ===\n");

            var documents = new List<string>
            {
                "This is the first document.",
                "This is the second document.",
                "And this is the third one."
            };

            var vectorizer = new CountVectorizer();
            vectorizer.Fit(documents);

            Console.WriteLine($"Documents:");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {documents[i]}");
            }

            Console.WriteLine($"\nVocabulary size: {vectorizer.GetVocabularySize()}");
            Console.WriteLine($"\nVocabulary:");
            var vocab = vectorizer.GetVocabulary();
            foreach (var term in vocab.OrderBy(v => v.Value))
            {
                Console.WriteLine($"  {term.Value}: {term.Key}");
            }

            Console.WriteLine($"\nDocument-Term Matrix:");
            var matrix = vectorizer.Transform(documents);
            var features = vectorizer.GetFeatureNames();

            Console.WriteLine("       | " + string.Join(" | ", features.Select(f => f.PadRight(10))));
            Console.WriteLine("-".PadRight(15 + features.Count * 12, '-'));

            for (int i = 0; i < matrix.Length; i++)
            {
                Console.Write($"Doc {i + 1} | ");
                for (int j = 0; j < matrix[i].Length; j++)
                {
                    Console.Write($"{matrix[i][j],10} | ");
                }
                Console.WriteLine();
            }
        }

        public static void DemonstrateSparseMatrix()
        {
            Console.WriteLine("\n=== Sparse Matrix Representation ===\n");

            var documents = new List<string>
            {
                "machine learning algorithm",
                "deep neural network",
                "natural language processing"
            };

            var vectorizer = new CountVectorizer();
            var sparseMatrix = vectorizer.FitTransform(documents).ToList();

            Console.WriteLine("Sparse vs Dense Comparison:");
            Console.WriteLine($"Vocabulary size: {vectorizer.GetVocabularySize()}");
            Console.WriteLine($"Number of documents: {documents.Count}");

            var sparse = vectorizer.FitTransform(documents);
            vectorizer.Fit(documents);
            var denseSparseMatrix = vectorizer.TransformSparse(documents);

            int totalElements = denseSparseMatrix.Rows * denseSparseMatrix.Columns;
            double sparsity = denseSparseMatrix.GetSparsity();

            Console.WriteLine($"\nMatrix dimensions: {denseSparseMatrix.Rows} × {denseSparseMatrix.Columns}");
            Console.WriteLine($"Total elements: {totalElements}");
            Console.WriteLine($"Non-zero elements: {denseSparseMatrix.Entries.Count}");
            Console.WriteLine($"Sparsity: {sparsity:F2}%");

            Console.WriteLine("\nSparse representation (only non-zero entries):");
            foreach (var entry in denseSparseMatrix.Entries.Take(10))
            {
                var features = vectorizer.GetFeatureNames();
                Console.WriteLine($"  [{entry.Row}, {entry.Column}] = {entry.Value} ({features[entry.Column]})");
            }
        }

        public static void DemonstrateTfidfVectorizer()
        {
            Console.WriteLine("\n=== TF-IDF Vectorizer ===\n");

            var documents = new List<string>
            {
                "the cat sat on the mat",
                "the dog sat on the floor",
                "cats and dogs are animals"
            };

            var vectorizer = new TfidfVectorizer();
            vectorizer.Fit(documents);

            Console.WriteLine("Documents:");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {documents[i]}");
            }

            var tfidfMatrix = vectorizer.Transform(documents);
            var features = vectorizer.GetCountVectorizer().GetFeatureNames();

            Console.WriteLine("\nTF-IDF Matrix:");
            Console.WriteLine("       | " + string.Join(" | ", features.Select(f => f.PadRight(12))));
            Console.WriteLine("-".PadRight(20 + features.Count * 14, '-'));

            for (int i = 0; i < tfidfMatrix.Length; i++)
            {
                Console.Write($"Doc {i + 1} | ");
                for (int j = 0; j < tfidfMatrix[i].Length; j++)
                {
                    Console.Write($"{tfidfMatrix[i][j]:F4}      | ");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nIDF Weights (importance):");
            var idfWeights = vectorizer.GetIdfWeights();
            foreach (var weight in idfWeights.OrderByDescending(w => w.Value).Take(5))
            {
                Console.WriteLine($"  {weight.Key}: {weight.Value:F4}");
            }
        }

        public static void DemonstrateNgrams()
        {
            Console.WriteLine("\n=== N-gram Support ===\n");

            var documents = new List<string>
            {
                "machine learning is important",
                "deep learning networks learn"
            };

            Console.WriteLine("Unigrams (single words):");
            var unigramVectorizer = new CountVectorizer(ngramMin: 1, ngramMax: 1);
            unigramVectorizer.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {unigramVectorizer.GetVocabularySize()}");
            Console.WriteLine($"  Features: {string.Join(", ", unigramVectorizer.GetFeatureNames())}");

            Console.WriteLine("\nBigrams (two-word phrases):");
            var bigramVectorizer = new CountVectorizer(ngramMin: 2, ngramMax: 2);
            bigramVectorizer.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {bigramVectorizer.GetVocabularySize()}");
            Console.WriteLine($"  Features: {string.Join(", ", bigramVectorizer.GetFeatureNames())}");

            Console.WriteLine("\nUnigrams + Bigrams:");
            var ngramVectorizer = new CountVectorizer(ngramMin: 1, ngramMax: 2);
            ngramVectorizer.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {ngramVectorizer.GetVocabularySize()}");
            Console.WriteLine($"  Features: {string.Join(", ", ngramVectorizer.GetFeatureNames())}");
        }

        public static void DemonstrateSimilarity()
        {
            Console.WriteLine("\n=== Document Similarity ===\n");

            var documents = new List<string>
            {
                "the cat sat on the mat",
                "the dog sat on the mat",
                "cats and dogs are animals"
            };

            var vectorizer = new CountVectorizer();
            var matrix = vectorizer.FitTransform(documents);

            Console.WriteLine("Documents:");
            for (int i = 0; i < documents.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {documents[i]}");
            }

            Console.WriteLine("\nDocument Similarity (Cosine):");
            for (int i = 0; i < matrix.Length; i++)
            {
                for (int j = i + 1; j < matrix.Length; j++)
                {
                    double similarity = VectorizerAnalyzer.CosineSimilarity(matrix[i], matrix[j]);
                    Console.WriteLine($"  Doc {i + 1} <-> Doc {j + 1}: {similarity:F4}");
                }
            }
        }

        public static void DemonstrateFeatureAnalysis()
        {
            Console.WriteLine("\n=== Feature Analysis ===\n");

            var documents = new List<string>
            {
                "machine learning models train on data",
                "deep learning neural networks process information",
                "data science analyzes datasets"
            };

            var vectorizer = new CountVectorizer();
            var matrix = vectorizer.FitTransform(documents);
            var features = vectorizer.GetFeatureNames();

            Console.WriteLine("Document-level feature analysis:");
            for (int i = 0; i < matrix.Length; i++)
            {
                Console.WriteLine($"\nDocument {i + 1}: {documents[i]}");
                var topFeatures = VectorizerAnalyzer.GetTopFeatures(matrix[i], features, 5);
                Console.WriteLine("  Top features:");
                foreach (var (feature, count) in topFeatures)
                {
                    Console.WriteLine($"    {feature}: {count}");
                }
            }

            Console.WriteLine("\n\nCorpus-level feature importance:");
            var importance = VectorizerAnalyzer.GetFeatureImportance(matrix, features);
            foreach (var (feature, score) in importance.OrderByDescending(i => i.Value).Take(10))
            {
                Console.WriteLine($"  {feature}: {score:F4}");
            }
        }

        public static void DemonstrateTextClassification()
        {
            Console.WriteLine("\n=== Text Classification Preparation ===\n");

            var positiveExamples = new List<string>
            {
                "this product is great and amazing",
                "excellent quality and good service",
                "wonderful experience highly recommended"
            };

            var negativeExamples = new List<string>
            {
                "terrible product waste of money",
                "poor quality bad customer service",
                "awful experience not recommended"
            };

            var allDocuments = new List<string>(positiveExamples);
            allDocuments.AddRange(negativeExamples);

            var vectorizer = new CountVectorizer();
            var matrix = vectorizer.FitTransform(allDocuments);

            Console.WriteLine("Text Classification Setup:");
            Console.WriteLine($"  Positive examples: {positiveExamples.Count}");
            Console.WriteLine($"  Negative examples: {negativeExamples.Count}");
            Console.WriteLine($"  Vocabulary size: {vectorizer.GetVocabularySize()}\n");

            Console.WriteLine("Feature matrix dimensions:");
            Console.WriteLine($"  Documents: {matrix.Length}");
            Console.WriteLine($"  Features: {matrix[0].Length}");
            Console.WriteLine($"  Total elements: {matrix.Length * matrix[0].Length}");

            Console.WriteLine("\nReady for classification with ML models:");
            Console.WriteLine("  ✓ Input: Document-term matrix");
            Console.WriteLine("  ✓ Output: Binary labels (positive/negative)");
            Console.WriteLine("  ✓ Algorithm: Naive Bayes, SVM, Logistic Regression, etc.");
        }

        public static void DemonstrateFiltering()
        {
            Console.WriteLine("\n=== Frequency Filtering ===\n");

            var documents = new List<string>
            {
                "the the the the cat",
                "the the dog dog",
                "the bird"
            };

            Console.WriteLine("No filtering:");
            var vectorizerNoFilter = new CountVectorizer();
            vectorizerNoFilter.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {vectorizerNoFilter.GetVocabularySize()}");
            Console.WriteLine($"  Terms: {string.Join(", ", vectorizerNoFilter.GetFeatureNames())}");

            Console.WriteLine("\nMin frequency = 2 (term must appear in at least 2 docs):");
            var vectorizerMinFreq = new CountVectorizer(minFrequency: 2);
            vectorizerMinFreq.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {vectorizerMinFreq.GetVocabularySize()}");
            Console.WriteLine($"  Terms: {string.Join(", ", vectorizerMinFreq.GetFeatureNames())}");

            Console.WriteLine("\nMax frequency = 2 (term can appear in at most 2 docs):");
            var vectorizerMaxFreq = new CountVectorizer(maxFrequency: 2);
            vectorizerMaxFreq.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {vectorizerMaxFreq.GetVocabularySize()}");
            Console.WriteLine($"  Terms: {string.Join(", ", vectorizerMaxFreq.GetFeatureNames())}");
        }

        public static void DemonstrateApplications()
        {
            Console.WriteLine("\n=== CountVectorizer Applications ===\n");

            Console.WriteLine("1. Text Classification");
            Console.WriteLine("   Input: Documents");
            Console.WriteLine("   Process: Convert to vectors");
            Console.WriteLine("   Output: Train classifier (Naive Bayes, SVM)");
            Console.WriteLine("   Use: Spam detection, sentiment analysis\n");

            Console.WriteLine("2. Document Similarity");
            Console.WriteLine("   Input: Multiple documents");
            Console.WriteLine("   Process: Create vectors, calculate cosine similarity");
            Console.WriteLine("   Output: Similarity scores");
            Console.WriteLine("   Use: Document clustering, duplicate detection\n");

            Console.WriteLine("3. Topic Modeling");
            Console.WriteLine("   Input: Document corpus");
            Console.WriteLine("   Process: Create document-term matrix");
            Console.WriteLine("   Output: Topic distributions");
            Console.WriteLine("   Use: Latent Dirichlet Allocation (LDA)\n");

            Console.WriteLine("4. Information Retrieval");
            Console.WriteLine("   Input: Query and documents");
            Console.WriteLine("   Process: Vectorize all, compute similarity");
            Console.WriteLine("   Output: Ranked documents");
            Console.WriteLine("   Use: Search engines, information retrieval\n");

            Console.WriteLine("5. Feature Engineering");
            Console.WriteLine("   Input: Raw text");
            Console.WriteLine("   Process: Extract features");
            Console.WriteLine("   Output: Numerical vectors");
            Console.WriteLine("   Use: Preparation for machine learning");
        }

        public static void DemonstrateWorkflow()
        {
            Console.WriteLine("\n=== Complete CountVectorizer Workflow ===\n");

            Console.WriteLine("Step 1: Prepare documents");
            var documents = new List<string>
            {
                "machine learning is powerful",
                "deep learning is effective",
                "machine learning and deep learning"
            };
            Console.WriteLine($"  Documents: {documents.Count}");

            Console.WriteLine("\nStep 2: Create and fit vectorizer");
            var vectorizer = new CountVectorizer();
            vectorizer.Fit(documents);
            Console.WriteLine($"  Vocabulary size: {vectorizer.GetVocabularySize()}");

            Console.WriteLine("\nStep 3: Transform documents");
            var matrix = vectorizer.Transform(documents);
            Console.WriteLine($"  Matrix shape: {matrix.Length} × {matrix[0].Length}");

            Console.WriteLine("\nStep 4: Analyze results");
            var features = vectorizer.GetFeatureNames();
            Console.WriteLine($"  Terms: {string.Join(", ", features)}");

            Console.WriteLine("\nStep 5: Use for ML tasks");
            Console.WriteLine("  ✓ Train classification model");
            Console.WriteLine("  ✓ Calculate document similarity");
            Console.WriteLine("  ✓ Extract important features");
            Console.WriteLine("  ✓ Perform topic modeling");
        }
    }
}
