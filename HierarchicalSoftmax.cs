using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Binary tree node for hierarchical softmax
    /// </summary>
    public class HierarchicalNode
    {
        public int NodeId { get; set; }
        public int? WordIndex { get; set; } // null for internal nodes
        public string Word { get; set; }
        public HierarchicalNode Left { get; set; }
        public HierarchicalNode Right { get; set; }
        public HierarchicalNode Parent { get; set; }
        public double[] Weights { get; set; }

        public HierarchicalNode(int nodeId, string word = null, int? wordIndex = null)
        {
            NodeId = nodeId;
            Word = word;
            WordIndex = wordIndex;
            Left = null;
            Right = null;
            Parent = null;
            Weights = null; // Will be initialized later
        }

        public bool IsLeaf()
        {
            return WordIndex != null;
        }

        public List<(HierarchicalNode, int)> GetPathToRoot()
        {
            var path = new List<(HierarchicalNode, int)>();
            HierarchicalNode current = this;
            int direction = -1; // 0 = left, 1 = right

            while (current.Parent != null)
            {
                if (current.Parent.Left == current)
                    direction = 0;
                else if (current.Parent.Right == current)
                    direction = 1;

                path.Add((current.Parent, direction));
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }
    }

    /// <summary>
    /// Huffman tree builder for hierarchical softmax
    /// More frequent words have shorter paths
    /// </summary>
    public class HuffmanTreeBuilder
    {
        public static HierarchicalNode BuildHuffmanTree(List<(string, int)> wordFrequencies)
        {
            // Create leaf nodes for each word
            var nodes = new List<HierarchicalNode>();
            int nodeId = 0;

            foreach (var (word, freq) in wordFrequencies)
            {
                var node = new HierarchicalNode(nodeId++, word, nodes.Count)
                {
                    Weights = new double[100] // Embedding dimension
                };
                nodes.Add(node);
            }

            // Build tree bottom-up
            int nextNodeId = nodeId;
            var heap = new PriorityQueue<(HierarchicalNode, int), int>();

            foreach (var (i, (_, freq)) in wordFrequencies.Select((x, i) => (i, x)))
                heap.Enqueue((nodes[i], freq), freq);

            while (heap.Count > 1)
            {
                var (node1, freq1) = heap.Dequeue();
                var (node2, freq2) = heap.Dequeue();

                var parent = new HierarchicalNode(nextNodeId++)
                {
                    Left = node1,
                    Right = node2,
                    Weights = new double[100]
                };

                node1.Parent = parent;
                node2.Parent = parent;

                heap.Enqueue((parent, freq1 + freq2), freq1 + freq2);
            }

            var (root, _) = heap.Dequeue();
            return root;
        }

        public static HierarchicalNode BuildBalancedTree(List<string> words)
        {
            // Build balanced binary tree (not frequency-based)
            int nodeId = 0;

            var leaves = words.Select((w, i) => new HierarchicalNode(nodeId++, w, i)
            {
                Weights = new double[100]
            }).ToList();

            return BuildBalancedTreeRecursive(leaves, ref nodeId);
        }

        private static HierarchicalNode BuildBalancedTreeRecursive(List<HierarchicalNode> nodes, ref int nodeId)
        {
            if (nodes.Count == 1)
                return nodes[0];

            if (nodes.Count == 2)
            {
                var parent = new HierarchicalNode(nodeId++)
                {
                    Left = nodes[0],
                    Right = nodes[1],
                    Weights = new double[100]
                };
                nodes[0].Parent = parent;
                nodes[1].Parent = parent;
                return parent;
            }

            int mid = nodes.Count / 2;
            var leftNodes = nodes.Take(mid).ToList();
            var rightNodes = nodes.Skip(mid).ToList();

            var left = BuildBalancedTreeRecursive(leftNodes, ref nodeId);
            var right = BuildBalancedTreeRecursive(rightNodes, ref nodeId);

            var parent_node = new HierarchicalNode(nodeId++)
            {
                Left = left,
                Right = right,
                Weights = new double[100]
            };

            left.Parent = parent_node;
            right.Parent = parent_node;

            return parent_node;
        }
    }

    /// <summary>
    /// Hierarchical softmax implementation
    /// </summary>
    public class HierarchicalSoftmax
    {
        public HierarchicalNode Root { get; set; }
        public Dictionary<string, HierarchicalNode> WordToNode { get; set; }
        public int EmbeddingDim { get; set; }
        public double LearningRate { get; set; }
        public List<string> Vocabulary { get; set; }

        public HierarchicalSoftmax(List<string> vocabulary, int embeddingDim = 100, double learningRate = 0.01)
        {
            EmbeddingDim = embeddingDim;
            LearningRate = learningRate;
            Vocabulary = vocabulary;
            WordToNode = new Dictionary<string, HierarchicalNode>();

            // Build balanced tree for simplicity
            Root = HuffmanTreeBuilder.BuildBalancedTree(vocabulary);
            IndexAllNodes(Root);
        }

        private void IndexAllNodes(HierarchicalNode node)
        {
            if (node == null)
                return;

            if (node.IsLeaf() && node.Word != null)
                WordToNode[node.Word] = node;

            IndexAllNodes(node.Left);
            IndexAllNodes(node.Right);
        }

        public double ComputeProbability(string word, double[] contextVector)
        {
            if (!WordToNode.ContainsKey(word))
                return 0;

            var node = WordToNode[word];
            var path = node.GetPathToRoot();

            double probability = 1.0;

            foreach (var (parentNode, direction) in path)
            {
                // Sigmoid of dot product
                double dotProduct = 0;
                for (int i = 0; i < Math.Min(contextVector.Length, parentNode.Weights.Length); i++)
                    dotProduct += contextVector[i] * parentNode.Weights[i];

                double sigmoid = 1.0 / (1.0 + Math.Exp(-dotProduct));

                // Probability depends on direction
                if (direction == 0) // left child
                    probability *= sigmoid;
                else // right child
                    probability *= (1.0 - sigmoid);
            }

            return probability;
        }

        public string PredictWord(double[] contextVector)
        {
            // Traverse tree to find most likely word
            HierarchicalNode current = Root;

            while (!current.IsLeaf())
            {
                double dotProduct = 0;
                for (int i = 0; i < Math.Min(contextVector.Length, current.Weights.Length); i++)
                    dotProduct += contextVector[i] * current.Weights[i];

                double sigmoid = 1.0 / (1.0 + Math.Exp(-dotProduct));

                // Go left if sigmoid < 0.5, right otherwise
                current = sigmoid < 0.5 ? current.Left : current.Right;
            }

            return current.Word;
        }

        public List<(string, double)> GetTopKPredictions(double[] contextVector, int topK = 5)
        {
            var predictions = new List<(string, double)>();

            foreach (var word in Vocabulary)
            {
                double prob = ComputeProbability(word, contextVector);
                predictions.Add((word, prob));
            }

            return predictions.OrderByDescending(x => x.Item2).Take(topK).ToList();
        }

        public void UpdateWeights(double[] contextVector, string targetWord, double learningRate)
        {
            if (!WordToNode.ContainsKey(targetWord))
                return;

            var node = WordToNode[targetWord];
            var path = node.GetPathToRoot();

            foreach (var (parentNode, direction) in path)
            {
                double dotProduct = 0;
                for (int i = 0; i < Math.Min(contextVector.Length, parentNode.Weights.Length); i++)
                    dotProduct += contextVector[i] * parentNode.Weights[i];

                double sigmoid = 1.0 / (1.0 + Math.Exp(-dotProduct));
                double target = direction == 0 ? 1.0 : 0.0;
                double error = sigmoid - target;

                // Update weights
                for (int i = 0; i < Math.Min(contextVector.Length, parentNode.Weights.Length); i++)
                    parentNode.Weights[i] -= learningRate * error * contextVector[i];
            }
        }

        public int GetPathLength(string word)
        {
            if (!WordToNode.ContainsKey(word))
                return 0;

            var node = WordToNode[word];
            var path = node.GetPathToRoot();
            return path.Count;
        }

        public double GetComputationReduction()
        {
            // Compute average path length vs full vocabulary
            double avgPathLength = Vocabulary.Average(w => GetPathLength(w));
            return 1.0 - (avgPathLength / Math.Log2(Vocabulary.Count));
        }
    }

    /// <summary>
    /// Language model using hierarchical softmax
    /// </summary>
    public class HierarchicalLanguageModel
    {
        public HierarchicalSoftmax HierarchicalSoftmax { get; set; }
        public Dictionary<string, double[]> WordEmbeddings { get; set; }
        public int EmbeddingDim { get; set; }
        public double LearningRate { get; set; }

        public HierarchicalLanguageModel(List<string> vocabulary, int embeddingDim = 100, double learningRate = 0.01)
        {
            EmbeddingDim = embeddingDim;
            LearningRate = learningRate;
            HierarchicalSoftmax = new HierarchicalSoftmax(vocabulary, embeddingDim, learningRate);
            WordEmbeddings = new Dictionary<string, double[]>();

            // Initialize word embeddings
            Random rand = new Random(42);
            foreach (var word in vocabulary)
            {
                WordEmbeddings[word] = Enumerable.Range(0, embeddingDim)
                    .Select(_ => (rand.NextDouble() - 0.5) * 2)
                    .ToArray();
            }
        }

        public void Train(List<List<string>> sequences, int epochs = 5)
        {
            for (int epoch = 0; epoch < epochs; epoch++)
            {
                double totalLoss = 0;

                foreach (var sequence in sequences)
                {
                    for (int i = 1; i < sequence.Count; i++)
                    {
                        string targetWord = sequence[i];

                        // Context: average of previous words
                        double[] contextVec = new double[EmbeddingDim];
                        for (int j = Math.Max(0, i - 3); j < i; j++)
                        {
                            if (WordEmbeddings.ContainsKey(sequence[j]))
                            {
                                for (int k = 0; k < EmbeddingDim; k++)
                                    contextVec[k] += WordEmbeddings[sequence[j]][k];
                            }
                        }

                        // Normalize context
                        int contextCount = Math.Min(i, 3);
                        for (int k = 0; k < EmbeddingDim; k++)
                            contextVec[k] /= contextCount;

                        // Compute loss
                        double prob = HierarchicalSoftmax.ComputeProbability(targetWord, contextVec);
                        double loss = -Math.Log(Math.Max(prob, 1e-10));
                        totalLoss += loss;

                        // Update weights
                        HierarchicalSoftmax.UpdateWeights(contextVec, targetWord, LearningRate);
                    }
                }

                Console.WriteLine($"Epoch {epoch}: Loss = {totalLoss / sequences.Sum(s => s.Count):F4}");
            }
        }

        public string PredictNextWord(List<string> context)
        {
            // Compute context vector
            double[] contextVec = new double[EmbeddingDim];
            int count = 0;

            for (int i = Math.Max(0, context.Count - 3); i < context.Count; i++)
            {
                if (WordEmbeddings.ContainsKey(context[i]))
                {
                    for (int k = 0; k < EmbeddingDim; k++)
                        contextVec[k] += WordEmbeddings[context[i]][k];
                    count++;
                }
            }

            if (count > 0)
            {
                for (int k = 0; k < EmbeddingDim; k++)
                    contextVec[k] /= count;
            }

            return HierarchicalSoftmax.PredictWord(contextVec);
        }

        public List<(string, double)> GetNextWordCandidates(List<string> context, int topK = 5)
        {
            double[] contextVec = new double[EmbeddingDim];
            int count = 0;

            for (int i = Math.Max(0, context.Count - 3); i < context.Count; i++)
            {
                if (WordEmbeddings.ContainsKey(context[i]))
                {
                    for (int k = 0; k < EmbeddingDim; k++)
                        contextVec[k] += WordEmbeddings[context[i]][k];
                    count++;
                }
            }

            if (count > 0)
            {
                for (int k = 0; k < EmbeddingDim; k++)
                    contextVec[k] /= count;
            }

            return HierarchicalSoftmax.GetTopKPredictions(contextVec, topK);
        }
    }

    /// <summary>
    /// Comparison: Hierarchical vs Standard Softmax
    /// </summary>
    public class SoftmaxComparison
    {
        public static void ComputeComplexity(int vocabularySize)
        {
            Console.WriteLine($"\nVocabulary Size: {vocabularySize}");

            // Standard softmax: O(V) where V = vocabulary size
            double standardComplexity = vocabularySize;

            // Hierarchical softmax: O(log V) average case with balanced tree
            double hierarchicalComplexity = Math.Log2(vocabularySize);

            double reduction = (1.0 - (hierarchicalComplexity / standardComplexity)) * 100;

            Console.WriteLine($"Standard Softmax Complexity: O({standardComplexity:F0})");
            Console.WriteLine($"Hierarchical Softmax Complexity: O({hierarchicalComplexity:F2})");
            Console.WriteLine($"Computation Reduction: {reduction:F1}%");
        }
    }

    /// <summary>
    /// Hierarchical softmax examples
    /// </summary>
    public class HierarchicalSoftmaxExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Hierarchical Softmax ===");

            var vocabulary = new List<string>
            {
                "the", "cat", "sat", "on", "mat", "dog", "ran", "quickly",
                "jumped", "over", "fence", "and", "ate", "food", "played"
            };

            Console.WriteLine($"Vocabulary Size: {vocabulary.Count}");

            // Create hierarchical softmax
            var hsoftmax = new HierarchicalSoftmax(vocabulary, embeddingDim: 50);

            Console.WriteLine("\n=== Path Lengths (Lower = More Frequent Expected) ===");
            foreach (var word in vocabulary.Take(8))
            {
                int pathLen = hsoftmax.GetPathLength(word);
                Console.WriteLine($"{word}: path length = {pathLen}");
            }

            Console.WriteLine($"\nComputation Reduction: {hsoftmax.GetComputationReduction():F2}%");

            Console.WriteLine("\n=== Language Model with Hierarchical Softmax ===");
            var sequences = new List<List<string>>
            {
                new List<string> { "the", "cat", "sat", "on", "the", "mat" },
                new List<string> { "the", "dog", "ran", "quickly", "over", "the", "fence" },
                new List<string> { "the", "cat", "and", "dog", "played", "together" },
                new List<string> { "the", "dog", "ate", "the", "food", "quickly" }
            };

            var lm = new HierarchicalLanguageModel(vocabulary, embeddingDim: 50, learningRate: 0.01);
            Console.WriteLine("Training language model...");
            lm.Train(sequences, epochs: 3);

            Console.WriteLine("\n=== Next Word Prediction ===");
            var contexts = new List<List<string>>
            {
                new List<string> { "the", "cat" },
                new List<string> { "the", "dog", "ran" },
                new List<string> { "sat", "on", "the" }
            };

            foreach (var context in contexts)
            {
                string prediction = lm.PredictNextWord(context);
                var candidates = lm.GetNextWordCandidates(context, topK: 3);

                Console.WriteLine($"\nContext: {string.Join(" ", context)}");
                Console.WriteLine($"Predicted: {prediction}");
                Console.WriteLine("Top candidates:");
                foreach (var (word, prob) in candidates)
                    Console.WriteLine($"  {word}: {prob:F4}");
            }

            Console.WriteLine("\n=== Complexity Analysis ===");
            SoftmaxComparison.ComputeComplexity(1000);
            SoftmaxComparison.ComputeComplexity(100000);
            SoftmaxComparison.ComputeComplexity(1000000);

            Console.WriteLine("\n=== Hierarchical Softmax Structure ===");
            Console.WriteLine("Binary Tree Properties:");
            Console.WriteLine("- Each internal node has sigmoid classifier");
            Console.WriteLine("- Each leaf node represents a word");
            Console.WriteLine("- Path from root to leaf gives word probability");
            Console.WriteLine("- More frequent words have shorter paths");

            Console.WriteLine("\n=== Computation at Each Node ===");
            Console.WriteLine("For each internal node:");
            Console.WriteLine("1. Compute dot product: context · weights");
            Console.WriteLine("2. Apply sigmoid: σ(dot_product)");
            Console.WriteLine("3. Branch left if sigmoid < 0.5, else right");
            Console.WriteLine("4. Repeat until reaching leaf node");

            Console.WriteLine("\n=== Advantages of Hierarchical Softmax ===");
            Console.WriteLine("1. Reduced Computation");
            Console.WriteLine("   - O(log V) instead of O(V)");
            Console.WriteLine("   - Significant speedup for large vocabularies");
            Console.WriteLine("   - Balanced trees: predictable performance");

            Console.WriteLine("\n2. Memory Efficiency");
            Console.WriteLine("   - Fewer parameters to learn");
            Console.WriteLine("   - Each node only stores d-dimensional weights");
            Console.WriteLine("   - Scales to very large vocabularies");

            Console.WriteLine("\n3. Grouping Similar Words");
            Console.WriteLine("   - Internal nodes group related words");
            Console.WriteLine("   - Provides regularization");
            Console.WriteLine("   - Better generalization");

            Console.WriteLine("\n4. Frequency-Based Optimization");
            Console.WriteLine("   - Huffman trees put common words near root");
            Console.WriteLine("   - Rare words take longer paths");
            Console.WriteLine("   - Matches natural word distribution");

            Console.WriteLine("\n=== Disadvantages ===");
            Console.WriteLine("1. Tree Construction Dependency");
            Console.WriteLine("   - Performance depends on tree quality");
            Console.WriteLine("   - Huffman trees require frequency data");
            Console.WriteLine("   - Different trees for different datasets");

            Console.WriteLine("\n2. Limited Word Context");
            Console.WriteLine("   - Each path decision independent");
            Console.WriteLine("   - May miss complex relationships");
            Console.WriteLine("   - CRF or other methods might be better");

            Console.WriteLine("\n3. Implementation Complexity");
            Console.WriteLine("   - More complex than standard softmax");
            Console.WriteLine("   - Tree construction overhead");
            Console.WriteLine("   - Gradient computation more involved");

            Console.WriteLine("\n=== Applications in NLP ===");
            Console.WriteLine("1. Language Modeling");
            Console.WriteLine("   - Next word prediction");
            Console.WriteLine("   - Efficient perplexity computation");
            Console.WriteLine("   - Large vocabulary support");

            Console.WriteLine("\n2. Machine Translation");
            Console.WriteLine("   - Output layer acceleration");
            Console.WriteLine("   - Reduces decoding time");
            Console.WriteLine("   - Enables beam search feasibility");

            Console.WriteLine("\n3. Text Summarization");
            Console.WriteLine("   - Word selection efficiency");
            Console.WriteLine("   - Abstract summary generation");
            Console.WriteLine("   - Reduced inference time");

            Console.WriteLine("\n4. Question Answering");
            Console.WriteLine("   - Answer word ranking");
            Console.WriteLine("   - Candidate scoring");
            Console.WriteLine("   - Fast inference");

            Console.WriteLine("\n5. Speech Recognition");
            Console.WriteLine("   - Language model integration");
            Console.WriteLine("   - Real-time decoding");
            Console.WriteLine("   - Mobile deployment");

            Console.WriteLine("\n=== Alternatives ===");
            Console.WriteLine("- Negative Sampling: Sample subset of vocabulary");
            Console.WriteLine("- Importance Sampling: Weighted sampling approach");
            Console.WriteLine("- Differentiated Softmax: Partition words by frequency");
            Console.WriteLine("- Noise Contrastive Estimation: NCE approximation");
            Console.WriteLine("- Static Softmax: Subsampling high-frequency words");

            Console.WriteLine("\n=== When to Use Hierarchical Softmax ===");
            Console.WriteLine("✓ Very large vocabularies (>100K words)");
            Console.WriteLine("✓ Deployment on resource-constrained devices");
            Console.WriteLine("✓ Need for consistent inference speed");
            Console.WriteLine("✓ Word frequency data available");
            Console.WriteLine("✗ Small vocabularies (<10K words)");
            Console.WriteLine("✗ When exact softmax probabilities critical");
            Console.WriteLine("✗ Limited implementation support");
        }
    }
}
