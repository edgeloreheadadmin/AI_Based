using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Hidden Markov Model for sequence labeling
    /// States are hidden, observations are visible
    /// </summary>
    public class HiddenMarkovModel
    {
        public List<string> States { get; set; }
        public List<string> Observations { get; set; }
        public double[][] TransitionMatrix { get; set; }  // P(state_t | state_t-1)
        public double[][] EmissionMatrix { get; set; }     // P(observation | state)
        public double[] InitialProbabilities { get; set; } // P(state_0)

        public HiddenMarkovModel(List<string> states, List<string> observations)
        {
            States = states;
            Observations = observations;

            // Initialize matrices with uniform probabilities
            TransitionMatrix = new double[states.Count][];
            EmissionMatrix = new double[states.Count][];
            InitialProbabilities = new double[states.Count];

            for (int i = 0; i < states.Count; i++)
            {
                TransitionMatrix[i] = new double[states.Count];
                EmissionMatrix[i] = new double[observations.Count];

                for (int j = 0; j < states.Count; j++)
                    TransitionMatrix[i][j] = 1.0 / states.Count;

                for (int j = 0; j < observations.Count; j++)
                    EmissionMatrix[i][j] = 1.0 / observations.Count;

                InitialProbabilities[i] = 1.0 / states.Count;
            }
        }

        public void TrainFromLabeled(List<List<string>> sequences, List<List<string>> labels)
        {
            if (sequences.Count == 0)
                return;

            // Count occurrences
            int[][] transitionCounts = new int[States.Count][];
            int[][] emissionCounts = new int[States.Count][];
            int[] initialCounts = new int[States.Count];

            for (int i = 0; i < States.Count; i++)
            {
                transitionCounts[i] = new int[States.Count];
                emissionCounts[i] = new int[Observations.Count];
            }

            // Count from training data
            for (int seq = 0; seq < sequences.Count; seq++)
            {
                var sequence = sequences[seq];
                var label = labels[seq];

                for (int i = 0; i < sequence.Count; i++)
                {
                    string observation = sequence[i];
                    string state = label[i];

                    int stateIdx = States.IndexOf(state);
                    int obsIdx = Observations.IndexOf(observation);

                    if (stateIdx >= 0 && obsIdx >= 0)
                    {
                        emissionCounts[stateIdx][obsIdx]++;

                        // Track initial state
                        if (i == 0)
                            initialCounts[stateIdx]++;

                        // Track transitions
                        if (i > 0)
                        {
                            string prevState = label[i - 1];
                            int prevStateIdx = States.IndexOf(prevState);
                            if (prevStateIdx >= 0)
                                transitionCounts[prevStateIdx][stateIdx]++;
                        }
                    }
                }
            }

            // Normalize counts to probabilities
            for (int i = 0; i < States.Count; i++)
            {
                // Emission probabilities
                double emissionSum = emissionCounts[i].Sum();
                for (int j = 0; j < Observations.Count; j++)
                    EmissionMatrix[i][j] = emissionSum > 0 ? emissionCounts[i][j] / emissionSum : 1.0 / Observations.Count;

                // Transition probabilities
                double transitionSum = transitionCounts[i].Sum();
                for (int j = 0; j < States.Count; j++)
                    TransitionMatrix[i][j] = transitionSum > 0 ? transitionCounts[i][j] / transitionSum : 1.0 / States.Count;
            }

            // Initial probabilities
            double initialSum = initialCounts.Sum();
            for (int i = 0; i < States.Count; i++)
                InitialProbabilities[i] = initialSum > 0 ? initialCounts[i] / initialSum : 1.0 / States.Count;
        }

        public List<string> ViterbiDecode(List<string> observations)
        {
            int n = observations.Count;
            int numStates = States.Count;

            // Viterbi trellis: probability of most likely path
            double[][] viterbi = new double[n][];
            int[][] backpointer = new int[n][];

            for (int i = 0; i < n; i++)
            {
                viterbi[i] = new double[numStates];
                backpointer[i] = new int[numStates];
            }

            // Initialization: first observation
            string firstObs = observations[0];
            int firstObsIdx = Observations.IndexOf(firstObs);

            for (int state = 0; state < numStates; state++)
            {
                double emissionProb = firstObsIdx >= 0 ? EmissionMatrix[state][firstObsIdx] : 1.0 / numStates;
                viterbi[0][state] = InitialProbabilities[state] * emissionProb;
            }

            // Recursion: subsequent observations
            for (int t = 1; t < n; t++)
            {
                string obs = observations[t];
                int obsIdx = Observations.IndexOf(obs);

                for (int state = 0; state < numStates; state++)
                {
                    double maxProb = 0;
                    int bestPrevState = 0;

                    for (int prevState = 0; prevState < numStates; prevState++)
                    {
                        double prob = viterbi[t - 1][prevState] * TransitionMatrix[prevState][state];
                        if (prob > maxProb)
                        {
                            maxProb = prob;
                            bestPrevState = prevState;
                        }
                    }

                    double emissionProb = obsIdx >= 0 ? EmissionMatrix[state][obsIdx] : 1.0 / numStates;
                    viterbi[t][state] = maxProb * emissionProb;
                    backpointer[t][state] = bestPrevState;
                }
            }

            // Backtrack to find best path
            var path = new List<string>();
            int lastState = 0;
            double maxLastProb = viterbi[n - 1][0];

            for (int state = 1; state < numStates; state++)
            {
                if (viterbi[n - 1][state] > maxLastProb)
                {
                    maxLastProb = viterbi[n - 1][state];
                    lastState = state;
                }
            }

            path.Add(States[lastState]);

            for (int t = n - 1; t > 0; t--)
            {
                lastState = backpointer[t][lastState];
                path.Add(States[lastState]);
            }

            path.Reverse();
            return path;
        }

        public List<string> ForwardBackwardDecode(List<string> observations)
        {
            int n = observations.Count;
            int numStates = States.Count;

            // Forward pass
            double[][] forward = new double[n][];
            for (int i = 0; i < n; i++)
                forward[i] = new double[numStates];

            string firstObs = observations[0];
            int firstObsIdx = Observations.IndexOf(firstObs);

            for (int state = 0; state < numStates; state++)
            {
                double emissionProb = firstObsIdx >= 0 ? EmissionMatrix[state][firstObsIdx] : 1.0 / numStates;
                forward[0][state] = InitialProbabilities[state] * emissionProb;
            }

            for (int t = 1; t < n; t++)
            {
                string obs = observations[t];
                int obsIdx = Observations.IndexOf(obs);

                for (int state = 0; state < numStates; state++)
                {
                    double prob = 0;
                    for (int prevState = 0; prevState < numStates; prevState++)
                        prob += forward[t - 1][prevState] * TransitionMatrix[prevState][state];

                    double emissionProb = obsIdx >= 0 ? EmissionMatrix[state][obsIdx] : 1.0 / numStates;
                    forward[t][state] = prob * emissionProb;
                }
            }

            // Backward pass
            double[][] backward = new double[n][];
            for (int i = 0; i < n; i++)
                backward[i] = new double[numStates];

            for (int state = 0; state < numStates; state++)
                backward[n - 1][state] = 1.0;

            for (int t = n - 2; t >= 0; t--)
            {
                string obs = observations[t + 1];
                int obsIdx = Observations.IndexOf(obs);

                for (int state = 0; state < numStates; state++)
                {
                    double prob = 0;
                    for (int nextState = 0; nextState < numStates; nextState++)
                    {
                        double emissionProb = obsIdx >= 0 ? EmissionMatrix[nextState][obsIdx] : 1.0 / numStates;
                        prob += TransitionMatrix[state][nextState] * emissionProb * backward[t + 1][nextState];
                    }
                    backward[t][state] = prob;
                }
            }

            // Find most likely state sequence
            var path = new List<string>();
            for (int t = 0; t < n; t++)
            {
                int bestState = 0;
                double maxProb = forward[t][0] * backward[t][0];

                for (int state = 1; state < numStates; state++)
                {
                    double prob = forward[t][state] * backward[t][state];
                    if (prob > maxProb)
                    {
                        maxProb = prob;
                        bestState = state;
                    }
                }

                path.Add(States[bestState]);
            }

            return path;
        }

        public double GetSequenceProbability(List<string> observations, List<string> states)
        {
            if (observations.Count != states.Count)
                return 0;

            double prob = InitialProbabilities[States.IndexOf(states[0])];

            for (int i = 0; i < observations.Count; i++)
            {
                int stateIdx = States.IndexOf(states[i]);
                int obsIdx = Observations.IndexOf(observations[i]);

                if (stateIdx >= 0 && obsIdx >= 0)
                    prob *= EmissionMatrix[stateIdx][obsIdx];

                if (i > 0)
                {
                    int prevStateIdx = States.IndexOf(states[i - 1]);
                    if (prevStateIdx >= 0)
                        prob *= TransitionMatrix[prevStateIdx][stateIdx];
                }
            }

            return prob;
        }
    }

    /// <summary>
    /// HMM for Part-of-Speech tagging
    /// </summary>
    public class POSTaggingHMM
    {
        public HiddenMarkovModel HMM { get; set; }
        public TextPreprocessor Preprocessor { get; set; }

        public POSTaggingHMM()
        {
            var states = new List<string>
            {
                "NOUN", "VERB", "ADJ", "ADV", "PRON", "DET", "PREP", "CONJ", "PUNCT"
            };

            var observations = new List<string>();
            // Will be expanded during training
            for (int i = 0; i < 100; i++)
                observations.Add($"WORD_{i}");

            HMM = new HiddenMarkovModel(states, observations);
            Preprocessor = new TextPreprocessor();
        }

        public void Train(List<(List<string>, List<string>)> labeledSentences)
        {
            var sequences = new List<List<string>>();
            var labels = new List<List<string>>();

            foreach (var (words, tags) in labeledSentences)
            {
                sequences.Add(words);
                labels.Add(tags);

                // Expand observation vocabulary
                foreach (var word in words)
                {
                    if (!HMM.Observations.Contains(word))
                        HMM.Observations.Add(word);
                }
            }

            HMM.TrainFromLabeled(sequences, labels);
        }

        public List<string> Tag(List<string> words)
        {
            return HMM.ViterbiDecode(words);
        }
    }

    /// <summary>
    /// HMM for Named Entity Recognition
    /// </summary>
    public class NamedEntityHMM
    {
        public HiddenMarkovModel HMM { get; set; }

        public NamedEntityHMM()
        {
            var states = new List<string>
            {
                "B-PER", "I-PER", "B-LOC", "I-LOC", "B-ORG", "I-ORG", "O"
            };

            var observations = new List<string>();
            for (int i = 0; i < 200; i++)
                observations.Add($"TOKEN_{i}");

            HMM = new HiddenMarkovModel(states, observations);
        }

        public void Train(List<(List<string>, List<string>)> labeledSequences)
        {
            var sequences = new List<List<string>>();
            var labels = new List<List<string>>();

            foreach (var (tokens, tags) in labeledSequences)
            {
                sequences.Add(tokens);
                labels.Add(tags);

                foreach (var token in tokens)
                {
                    if (!HMM.Observations.Contains(token))
                        HMM.Observations.Add(token);
                }
            }

            HMM.TrainFromLabeled(sequences, labels);
        }

        public List<string> Recognize(List<string> tokens)
        {
            return HMM.ViterbiDecode(tokens);
        }

        public List<(string, string, string)> ExtractEntities(List<string> tokens)
        {
            var tags = Recognize(tokens);
            var entities = new List<(string, string, string)>();

            string currentEntity = "";
            string currentType = "";

            for (int i = 0; i < tokens.Count; i++)
            {
                string tag = tags[i];

                if (tag == "O")
                {
                    if (!string.IsNullOrEmpty(currentEntity))
                    {
                        entities.Add((currentEntity, currentType, ""));
                        currentEntity = "";
                        currentType = "";
                    }
                }
                else if (tag.StartsWith("B-"))
                {
                    if (!string.IsNullOrEmpty(currentEntity))
                        entities.Add((currentEntity, currentType, ""));

                    currentType = tag.Substring(2);
                    currentEntity = tokens[i];
                }
                else if (tag.StartsWith("I-"))
                {
                    currentEntity += " " + tokens[i];
                }
            }

            if (!string.IsNullOrEmpty(currentEntity))
                entities.Add((currentEntity, currentType, ""));

            return entities;
        }
    }

    /// <summary>
    /// HMM for sequence labeling tasks
    /// </summary>
    public class SequenceLabelingHMM
    {
        public HiddenMarkovModel HMM { get; set; }

        public SequenceLabelingHMM(List<string> states, List<string> observations)
        {
            HMM = new HiddenMarkovModel(states, observations);
        }

        public void Train(List<(List<string>, List<string>)> data)
        {
            var sequences = data.Select(x => x.Item1).ToList();
            var labels = data.Select(x => x.Item2).ToList();
            HMM.TrainFromLabeled(sequences, labels);
        }

        public List<string> Decode(List<string> sequence)
        {
            return HMM.ViterbiDecode(sequence);
        }

        public double GetAccuracy(List<(List<string>, List<string>)> testData)
        {
            int correct = 0;
            int total = 0;

            foreach (var (sequence, trueLabels) in testData)
            {
                var predictedLabels = Decode(sequence);

                for (int i = 0; i < trueLabels.Count; i++)
                {
                    if (i < predictedLabels.Count && trueLabels[i] == predictedLabels[i])
                        correct++;
                    total++;
                }
            }

            return total > 0 ? (double)correct / total : 0;
        }
    }

    /// <summary>
    /// Hidden Markov Model examples
    /// </summary>
    public class HiddenMarkovModelExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Hidden Markov Model (HMM) ===");
            var states = new List<string> { "Sunny", "Rainy" };
            var observations = new List<string> { "happy", "grumpy", "normal" };

            var hmm = new HiddenMarkovModel(states, observations);

            // Simple weather-mood example
            var sequences = new List<List<string>>
            {
                new List<string> { "happy", "happy", "normal" },
                new List<string> { "grumpy", "grumpy", "happy" },
                new List<string> { "normal", "happy", "happy" }
            };

            var labels = new List<List<string>>
            {
                new List<string> { "Sunny", "Sunny", "Rainy" },
                new List<string> { "Rainy", "Rainy", "Sunny" },
                new List<string> { "Rainy", "Sunny", "Sunny" }
            };

            Console.WriteLine("Training HMM from labeled sequences...");
            hmm.TrainFromLabeled(sequences, labels);

            Console.WriteLine("Transition Probabilities (from → to):");
            for (int i = 0; i < states.Count; i++)
            {
                Console.Write($"{states[i]} → ");
                for (int j = 0; j < states.Count; j++)
                    Console.Write($"{states[j]}: {hmm.TransitionMatrix[i][j]:F3} ");
                Console.WriteLine();
            }

            Console.WriteLine("\n=== Viterbi Decoding ===");
            var testSequence = new List<string> { "happy", "normal", "grumpy" };
            var decodedPath = hmm.ViterbiDecode(testSequence);

            Console.WriteLine($"Observations: {string.Join(", ", testSequence)}");
            Console.WriteLine($"Most likely states: {string.Join(", ", decodedPath)}");

            Console.WriteLine("\n=== Part-of-Speech Tagging ===");
            var posHmm = new POSTaggingHMM();

            var trainingData = new List<(List<string>, List<string>)>
            {
                (new List<string> { "the", "cat", "sat" }, new List<string> { "DET", "NOUN", "VERB" }),
                (new List<string> { "a", "dog", "runs" }, new List<string> { "DET", "NOUN", "VERB" }),
                (new List<string> { "the", "fast", "cat" }, new List<string> { "DET", "ADJ", "NOUN" }),
                (new List<string> { "quickly", "ran", "away" }, new List<string> { "ADV", "VERB", "ADV" })
            };

            Console.WriteLine("Training POS tagger...");
            posHmm.Train(trainingData);

            var testSentence = new List<string> { "the", "fast", "dog" };
            var tags = posHmm.Tag(testSentence);

            Console.WriteLine($"Sentence: {string.Join(" ", testSentence)}");
            Console.WriteLine($"Tags: {string.Join(" ", tags)}");

            Console.WriteLine("\n=== Named Entity Recognition ===");
            var nerHmm = new NamedEntityHMM();

            var nerData = new List<(List<string>, List<string>)>
            {
                (new List<string> { "John", "Smith", "works", "at", "Google" },
                 new List<string> { "B-PER", "I-PER", "O", "O", "B-ORG" }),

                (new List<string> { "Paris", "is", "in", "France" },
                 new List<string> { "B-LOC", "O", "O", "B-LOC" }),

                (new List<string> { "Apple", "was", "founded", "by", "Steve", "Jobs" },
                 new List<string> { "B-ORG", "O", "O", "O", "B-PER", "I-PER" })
            };

            Console.WriteLine("Training NER system...");
            nerHmm.Train(nerData);

            var testTokens = new List<string> { "Microsoft", "was", "founded", "by", "Bill", "Gates" };
            var entities = nerHmm.ExtractEntities(testTokens);

            Console.WriteLine($"Text: {string.Join(" ", testTokens)}");
            Console.WriteLine("Extracted Entities:");
            foreach (var (entity, type, _) in entities)
                Console.WriteLine($"  {entity} ({type})");

            Console.WriteLine("\n=== HMM Components ===");
            Console.WriteLine("1. Hidden States: Not directly observable");
            Console.WriteLine("   - Example: Weather (Sunny, Rainy)");
            Console.WriteLine("   - POS tags, entity types, etc.");
            Console.WriteLine("\n2. Observations: Directly observable sequences");
            Console.WriteLine("   - Example: Mood (happy, grumpy)");
            Console.WriteLine("   - Words in text, tokens, etc.");
            Console.WriteLine("\n3. Transition Matrix: P(state_t | state_t-1)");
            Console.WriteLine("   - Probability of moving between states");
            Console.WriteLine("   - First-order Markov assumption");
            Console.WriteLine("\n4. Emission Matrix: P(observation | state)");
            Console.WriteLine("   - Probability of observing a symbol in a state");
            Console.WriteLine("\n5. Initial Probabilities: P(state_0)");
            Console.WriteLine("   - Probability of starting in each state");

            Console.WriteLine("\n=== Key Algorithms ===");
            Console.WriteLine("1. Viterbi Algorithm");
            Console.WriteLine("   - Finds most likely state sequence");
            Console.WriteLine("   - O(T*N^2) complexity where T=time, N=states");
            Console.WriteLine("   - Dynamic programming approach");
            Console.WriteLine("\n2. Forward-Backward Algorithm");
            Console.WriteLine("   - Computes posterior probabilities");
            Console.WriteLine("   - Used for parameter estimation");
            Console.WriteLine("   - More computationally expensive");
            Console.WriteLine("\n3. Baum-Welch Algorithm");
            Console.WriteLine("   - Parameter estimation from unlabeled data");
            Console.WriteLine("   - EM algorithm variant");
            Console.WriteLine("   - Finds locally optimal parameters");

            Console.WriteLine("\n=== NLP Applications ===");
            Console.WriteLine("1. Part-of-Speech Tagging");
            Console.WriteLine("   - Predicting grammatical role of words");
            Console.WriteLine("   - States: POS tags (NOUN, VERB, etc.)");
            Console.WriteLine("   - Observations: Words");
            Console.WriteLine("\n2. Named Entity Recognition");
            Console.WriteLine("   - Identifying named entities (people, places, orgs)");
            Console.WriteLine("   - States: Entity tags (B-PER, I-PER, O)");
            Console.WriteLine("   - Observations: Tokens");
            Console.WriteLine("\n3. Chunking");
            Console.WriteLine("   - Identifying noun/verb phrases");
            Console.WriteLine("   - States: Chunk tags");
            Console.WriteLine("   - Observations: Words with POS tags");
            Console.WriteLine("\n4. Shallow Parsing");
            Console.WriteLine("   - Low-level syntactic analysis");
            Console.WriteLine("   - States: Parse states");
            Console.WriteLine("   - Observations: Tokens");
            Console.WriteLine("\n5. Speech Recognition");
            Console.WriteLine("   - Converting audio to text");
            Console.WriteLine("   - States: Phone/word models");
            Console.WriteLine("   - Observations: Acoustic features");

            Console.WriteLine("\n=== Advantages of HMMs ===");
            Console.WriteLine("- Theoretically sound probabilistic model");
            Console.WriteLine("- Efficient inference algorithms");
            Console.WriteLine("- Can be trained on labeled data");
            Console.WriteLine("- Handle variable-length sequences");
            Console.WriteLine("- Well-established in NLP");
            Console.WriteLine("- Interpretable parameters");

            Console.WriteLine("\n=== Limitations of HMMs ===");
            Console.WriteLine("- Strong independence assumptions");
            Console.WriteLine("- Limited context (first-order Markov)");
            Console.WriteLine("- Fixed state space");
            Console.WriteLine("- Requires labeled training data");
            Console.WriteLine("- Outperformed by deep learning on large data");

            Console.WriteLine("\n=== Extensions ===");
            Console.WriteLine("- Higher-order HMMs (more history)");
            Console.WriteLine("- Factorial HMMs (multiple chains)");
            Console.WriteLine("- Input-Output HMMs (conditional models)");
            Console.WriteLine("- Hierarchical HMMs (multi-level structure)");
            Console.WriteLine("- Conditional Random Fields (CRFs)");

            Console.WriteLine("\n=== Parameter Estimation ===");
            Console.WriteLine("Maximum Likelihood Estimation:");
            Console.WriteLine("- Transition: Count(state_i → state_j) / Count(state_i)");
            Console.WriteLine("- Emission: Count(obs|state) / Count(state)");
            Console.WriteLine("- Smoothing: Add-one or other techniques");
            Console.WriteLine("\nBaum-Welch (EM):");
            Console.WriteLine("- Use forward-backward for soft counts");
            Console.WriteLine("- Re-estimate parameters iteratively");
            Console.WriteLine("- Converges to local maximum");
        }
    }
}
