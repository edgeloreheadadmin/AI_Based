using System;
using System.Collections.Generic;
using System.Linq;

namespace NLPToolkit
{
    /// <summary>
    /// Chromosome representation for genetic algorithm
    /// </summary>
    public class Chromosome
    {
        public double[] Genes { get; set; }
        public double Fitness { get; set; }

        public Chromosome(int geneCount)
        {
            Genes = new double[geneCount];
            Random rand = new Random();
            for (int i = 0; i < geneCount; i++)
                Genes[i] = rand.NextDouble();
            Fitness = 0;
        }

        public Chromosome(double[] genes)
        {
            Genes = (double[])genes.Clone();
            Fitness = 0;
        }

        public Chromosome Clone()
        {
            return new Chromosome(Genes);
        }

        public void Mutate(double mutationRate)
        {
            Random rand = new Random();
            for (int i = 0; i < Genes.Length; i++)
            {
                if (rand.NextDouble() < mutationRate)
                    Genes[i] = rand.NextDouble();
            }
        }
    }

    /// <summary>
    /// Individual in the population
    /// </summary>
    public class Individual
    {
        public Chromosome Chromosome { get; set; }
        public double Fitness { get; set; }

        public Individual(int geneCount)
        {
            Chromosome = new Chromosome(geneCount);
            Fitness = 0;
        }

        public Individual(Chromosome chromosome)
        {
            Chromosome = chromosome;
            Fitness = 0;
        }

        public void EvaluateFitness(Func<double[], double> fitnessFunction)
        {
            Fitness = fitnessFunction(Chromosome.Genes);
            Chromosome.Fitness = Fitness;
        }

        public Individual Clone()
        {
            return new Individual(Chromosome.Clone());
        }
    }

    /// <summary>
    /// Selection strategies for genetic algorithm
    /// </summary>
    public class SelectionStrategy
    {
        public static List<Individual> TournamentSelection(List<Individual> population, int tournamentSize, int selectCount)
        {
            Random rand = new Random();
            var selected = new List<Individual>();

            for (int i = 0; i < selectCount; i++)
            {
                Individual best = null;
                double bestFitness = -1;

                for (int j = 0; j < tournamentSize; j++)
                {
                    int idx = rand.Next(population.Count);
                    if (population[idx].Fitness > bestFitness)
                    {
                        bestFitness = population[idx].Fitness;
                        best = population[idx];
                    }
                }

                if (best != null)
                    selected.Add(best.Clone());
            }

            return selected;
        }

        public static List<Individual> RouletteWheelSelection(List<Individual> population, int selectCount)
        {
            Random rand = new Random();
            double totalFitness = population.Sum(p => p.Fitness);

            if (totalFitness <= 0)
                return population.Take(selectCount).Select(p => p.Clone()).ToList();

            var selected = new List<Individual>();

            for (int i = 0; i < selectCount; i++)
            {
                double spin = rand.NextDouble() * totalFitness;
                double accumulated = 0;

                foreach (var individual in population)
                {
                    accumulated += individual.Fitness;
                    if (accumulated >= spin)
                    {
                        selected.Add(individual.Clone());
                        break;
                    }
                }
            }

            return selected;
        }

        public static List<Individual> RankSelection(List<Individual> population, int selectCount)
        {
            var sorted = population.OrderByDescending(p => p.Fitness).ToList();
            Random rand = new Random();
            var selected = new List<Individual>();

            for (int i = 0; i < selectCount; i++)
            {
                int rank = (int)(rand.NextDouble() * sorted.Count);
                rank = Math.Min(rank, sorted.Count - 1);
                selected.Add(sorted[rank].Clone());
            }

            return selected;
        }
    }

    /// <summary>
    /// Crossover operations for genetic algorithm
    /// </summary>
    public class CrossoverOperation
    {
        public static (Individual, Individual) SinglePointCrossover(Individual parent1, Individual parent2)
        {
            Random rand = new Random();
            int crossoverPoint = rand.Next(1, parent1.Chromosome.Genes.Length);

            var child1Genes = new double[parent1.Chromosome.Genes.Length];
            var child2Genes = new double[parent2.Chromosome.Genes.Length];

            Array.Copy(parent1.Chromosome.Genes, 0, child1Genes, 0, crossoverPoint);
            Array.Copy(parent2.Chromosome.Genes, crossoverPoint, child1Genes, crossoverPoint,
                parent2.Chromosome.Genes.Length - crossoverPoint);

            Array.Copy(parent2.Chromosome.Genes, 0, child2Genes, 0, crossoverPoint);
            Array.Copy(parent1.Chromosome.Genes, crossoverPoint, child2Genes, crossoverPoint,
                parent1.Chromosome.Genes.Length - crossoverPoint);

            return (new Individual(new Chromosome(child1Genes)), new Individual(new Chromosome(child2Genes)));
        }

        public static (Individual, Individual) UniformCrossover(Individual parent1, Individual parent2, double crossoverRate = 0.5)
        {
            Random rand = new Random();
            var child1Genes = new double[parent1.Chromosome.Genes.Length];
            var child2Genes = new double[parent2.Chromosome.Genes.Length];

            for (int i = 0; i < parent1.Chromosome.Genes.Length; i++)
            {
                if (rand.NextDouble() < crossoverRate)
                {
                    child1Genes[i] = parent1.Chromosome.Genes[i];
                    child2Genes[i] = parent2.Chromosome.Genes[i];
                }
                else
                {
                    child1Genes[i] = parent2.Chromosome.Genes[i];
                    child2Genes[i] = parent1.Chromosome.Genes[i];
                }
            }

            return (new Individual(new Chromosome(child1Genes)), new Individual(new Chromosome(child2Genes)));
        }

        public static (Individual, Individual) MultiPointCrossover(Individual parent1, Individual parent2, int numPoints)
        {
            Random rand = new Random();
            var crossoverPoints = new List<int>();

            for (int i = 0; i < numPoints; i++)
                crossoverPoints.Add(rand.Next(1, parent1.Chromosome.Genes.Length));

            crossoverPoints.Sort();

            var child1Genes = new double[parent1.Chromosome.Genes.Length];
            var child2Genes = new double[parent2.Chromosome.Genes.Length];

            Array.Copy(parent1.Chromosome.Genes, child1Genes, parent1.Chromosome.Genes.Length);
            Array.Copy(parent2.Chromosome.Genes, child2Genes, parent2.Chromosome.Genes.Length);

            bool swap = false;
            int lastPoint = 0;

            foreach (int point in crossoverPoints)
            {
                if (swap)
                {
                    // Swap segment
                    for (int i = lastPoint; i < point; i++)
                    {
                        double temp = child1Genes[i];
                        child1Genes[i] = child2Genes[i];
                        child2Genes[i] = temp;
                    }
                }
                swap = !swap;
                lastPoint = point;
            }

            return (new Individual(new Chromosome(child1Genes)), new Individual(new Chromosome(child2Genes)));
        }
    }

    /// <summary>
    /// Genetic algorithm implementation
    /// </summary>
    public class GeneticAlgorithm
    {
        public List<Individual> Population { get; set; }
        public int PopulationSize { get; set; }
        public int GeneCount { get; set; }
        public double MutationRate { get; set; }
        public double CrossoverRate { get; set; }
        public Func<double[], double> FitnessFunction { get; set; }
        public int Generation { get; set; }
        public Individual BestIndividual { get; set; }
        public List<double> FitnessHistory { get; set; }

        public GeneticAlgorithm(int populationSize, int geneCount, double mutationRate = 0.01,
            double crossoverRate = 0.9, Func<double[], double> fitnessFunction = null)
        {
            PopulationSize = populationSize;
            GeneCount = geneCount;
            MutationRate = mutationRate;
            CrossoverRate = crossoverRate;
            FitnessFunction = fitnessFunction ?? DefaultFitness;
            Generation = 0;
            FitnessHistory = new List<double>();
            InitializePopulation();
        }

        private void InitializePopulation()
        {
            Population = new List<Individual>();
            for (int i = 0; i < PopulationSize; i++)
            {
                var individual = new Individual(GeneCount);
                individual.EvaluateFitness(FitnessFunction);
                Population.Add(individual);
            }
            UpdateBestIndividual();
        }

        private double DefaultFitness(double[] genes)
        {
            // Sphere function for optimization
            double sum = 0;
            foreach (var gene in genes)
                sum += gene * gene;
            return -sum; // Negative because GA maximizes
        }

        private void UpdateBestIndividual()
        {
            var best = Population.OrderByDescending(p => p.Fitness).First();
            if (BestIndividual == null || best.Fitness > BestIndividual.Fitness)
                BestIndividual = best.Clone();
        }

        public void EvolveGeneration()
        {
            // Selection
            var selected = SelectionStrategy.TournamentSelection(Population, tournamentSize: 3,
                selectCount: PopulationSize / 2);

            // Crossover
            var offspring = new List<Individual>();
            Random rand = new Random();

            for (int i = 0; i < selected.Count; i += 2)
            {
                if (i + 1 < selected.Count)
                {
                    if (rand.NextDouble() < CrossoverRate)
                    {
                        var (child1, child2) = CrossoverOperation.SinglePointCrossover(selected[i], selected[i + 1]);
                        offspring.Add(child1);
                        offspring.Add(child2);
                    }
                    else
                    {
                        offspring.Add(selected[i].Clone());
                        offspring.Add(selected[i + 1].Clone());
                    }
                }
                else
                {
                    offspring.Add(selected[i].Clone());
                }
            }

            // Mutation
            foreach (var individual in offspring)
            {
                individual.Chromosome.Mutate(MutationRate);
                individual.EvaluateFitness(FitnessFunction);
            }

            // Replacement - Elitism: Keep best from previous population
            var combined = Population.Concat(offspring).OrderByDescending(p => p.Fitness).Take(PopulationSize).ToList();
            Population = combined;

            UpdateBestIndividual();
            FitnessHistory.Add(BestIndividual.Fitness);
            Generation++;
        }

        public void Evolve(int generations)
        {
            for (int i = 0; i < generations; i++)
                EvolveGeneration();
        }

        public double GetAverageFitness()
        {
            return Population.Average(p => p.Fitness);
        }

        public double GetBestFitness()
        {
            return BestIndividual.Fitness;
        }

        public void PrintStatistics()
        {
            Console.WriteLine($"Generation: {Generation}");
            Console.WriteLine($"Best Fitness: {GetBestFitness():F4}");
            Console.WriteLine($"Average Fitness: {GetAverageFitness():F4}");
            Console.WriteLine($"Best Genes: {string.Join(", ", BestIndividual.Chromosome.Genes.Take(5))}...");
        }
    }

    /// <summary>
    /// GA for text classifier optimization
    /// </summary>
    public class TextClassifierGA
    {
        public GeneticAlgorithm GA { get; set; }
        public List<(double[], int)> TrainingData { get; set; }
        public TextPreprocessor Preprocessor { get; set; }
        public TextEncoder Encoder { get; set; }

        public TextClassifierGA(List<(double[], int)> trainingData, int geneCount = 50)
        {
            TrainingData = trainingData;
            Preprocessor = new TextPreprocessor();
            Encoder = new TextEncoder();

            // Fitness function based on classification accuracy
            GA = new GeneticAlgorithm(
                populationSize: 30,
                geneCount: geneCount,
                mutationRate: 0.05,
                crossoverRate: 0.8,
                fitnessFunction: EvaluateClassifier
            );
        }

        private double EvaluateClassifier(double[] weights)
        {
            if (TrainingData.Count == 0)
                return 0;

            int correct = 0;

            foreach (var (features, label) in TrainingData)
            {
                double score = 0;
                for (int i = 0; i < Math.Min(features.Length, weights.Length); i++)
                    score += features[i] * weights[i];

                int predicted = score > 0.5 ? 1 : 0;
                if (predicted == label)
                    correct++;
            }

            double accuracy = (double)correct / TrainingData.Count;
            return accuracy;
        }

        public void Train(int generations = 50)
        {
            GA.Evolve(generations);
        }

        public int Predict(double[] features)
        {
            double score = 0;
            var weights = GA.BestIndividual.Chromosome.Genes;

            for (int i = 0; i < Math.Min(features.Length, weights.Length); i++)
                score += features[i] * weights[i];

            return score > 0.5 ? 1 : 0;
        }
    }

    /// <summary>
    /// GA for feature selection
    /// </summary>
    public class FeatureSelectionGA
    {
        public GeneticAlgorithm GA { get; set; }
        public List<(double[], int)> TrainingData { get; set; }
        public int TotalFeatures { get; set; }

        public FeatureSelectionGA(List<(double[], int)> trainingData)
        {
            TrainingData = trainingData;
            TotalFeatures = trainingData.Count > 0 ? trainingData[0].Item1.Length : 0;

            GA = new GeneticAlgorithm(
                populationSize: 20,
                geneCount: TotalFeatures,
                mutationRate: 0.1,
                crossoverRate: 0.8,
                fitnessFunction: EvaluateFeatureSet
            );
        }

        private double EvaluateFeatureSet(double[] featureSelection)
        {
            // Binary representation: 0 = feature not selected, 1 = feature selected
            int selectedCount = 0;
            foreach (var gene in featureSelection)
            {
                if (gene > 0.5)
                    selectedCount++;
            }

            if (selectedCount == 0)
                return 0;

            // Simple accuracy with selected features
            int correct = 0;
            foreach (var (features, label) in TrainingData)
            {
                double score = 0;
                int count = 0;

                for (int i = 0; i < features.Length; i++)
                {
                    if (featureSelection[i] > 0.5)
                    {
                        score += features[i];
                        count++;
                    }
                }

                if (count > 0)
                {
                    score /= count;
                    int predicted = score > 0.5 ? 1 : 0;
                    if (predicted == label)
                        correct++;
                }
            }

            // Combine accuracy and feature reduction
            double accuracy = (double)correct / TrainingData.Count;
            double reduction = 1.0 - ((double)selectedCount / TotalFeatures);

            return 0.8 * accuracy + 0.2 * reduction;
        }

        public void Train(int generations = 50)
        {
            GA.Evolve(generations);
        }

        public List<int> GetSelectedFeatures()
        {
            var selected = new List<int>();
            var genes = GA.BestIndividual.Chromosome.Genes;

            for (int i = 0; i < genes.Length; i++)
            {
                if (genes[i] > 0.5)
                    selected.Add(i);
            }

            return selected;
        }
    }

    /// <summary>
    /// GA for hyperparameter optimization
    /// </summary>
    public class HyperparameterOptimizer
    {
        public GeneticAlgorithm GA { get; set; }
        public List<(double[], int)> TrainingData { get; set; }

        public HyperparameterOptimizer(List<(double[], int)> trainingData)
        {
            TrainingData = trainingData;

            // Genes represent: learning rate, regularization, hidden layer size, dropout
            GA = new GeneticAlgorithm(
                populationSize: 20,
                geneCount: 4,
                mutationRate: 0.1,
                crossoverRate: 0.8,
                fitnessFunction: EvaluateHyperparameters
            );
        }

        private double EvaluateHyperparameters(double[] hyperparams)
        {
            double learningRate = hyperparams[0] * 0.1; // 0.0 to 0.1
            double regularization = hyperparams[1] * 0.1; // 0.0 to 0.1
            int hiddenSize = (int)(hyperparams[2] * 256); // 0 to 256
            double dropout = hyperparams[3]; // 0.0 to 1.0

            if (hiddenSize < 8)
                hiddenSize = 8;

            // Simulate model performance
            double baseAccuracy = 0.85;
            double lr_bonus = learningRate > 0.01 && learningRate < 0.05 ? 0.05 : 0;
            double reg_bonus = regularization > 0.01 && regularization < 0.08 ? 0.03 : 0;
            double hidden_bonus = hiddenSize > 32 && hiddenSize < 128 ? 0.05 : 0;
            double dropout_bonus = dropout > 0.3 && dropout < 0.7 ? 0.02 : 0;

            return baseAccuracy + lr_bonus + reg_bonus + hidden_bonus + dropout_bonus;
        }

        public void Optimize(int generations = 50)
        {
            GA.Evolve(generations);
        }

        public (double, double, int, double) GetBestHyperparameters()
        {
            var genes = GA.BestIndividual.Chromosome.Genes;
            double learningRate = genes[0] * 0.1;
            double regularization = genes[1] * 0.1;
            int hiddenSize = (int)(genes[2] * 256);
            double dropout = genes[3];

            return (learningRate, regularization, hiddenSize, dropout);
        }
    }

    /// <summary>
    /// Genetic algorithm examples for NLP
    /// </summary>
    public class GeneticAlgorithmExamples
    {
        public static void RunExamples()
        {
            Console.WriteLine("=== Basic Genetic Algorithm ===");
            var ga = new GeneticAlgorithm(
                populationSize: 30,
                geneCount: 10,
                mutationRate: 0.05,
                crossoverRate: 0.8,
                fitnessFunction: genes =>
                {
                    // Minimize x1^2 + x2^2 + ... (sphere function)
                    double sum = 0;
                    foreach (var gene in genes)
                        sum += gene * gene;
                    return -sum; // Negate because GA maximizes
                }
            );

            Console.WriteLine("Optimizing sphere function (minimize x1^2 + x2^2 + ...)");
            for (int i = 0; i < 10; i++)
            {
                ga.EvolveGeneration();
                Console.WriteLine($"Generation {ga.Generation}: Best Fitness = {ga.GetBestFitness():F4}, Avg = {ga.GetAverageFitness():F4}");
            }

            Console.WriteLine("\n=== Text Classifier Optimization ===");
            var trainingData = new List<(double[], int)>
            {
                (new double[] { 0.9, 0.1, 0.8, 0.2, 0.7 }, 1),
                (new double[] { 0.8, 0.2, 0.7, 0.3, 0.6 }, 1),
                (new double[] { 0.1, 0.9, 0.2, 0.8, 0.3 }, 0),
                (new double[] { 0.2, 0.8, 0.3, 0.7, 0.4 }, 0),
                (new double[] { 0.85, 0.15, 0.75, 0.25, 0.65 }, 1),
                (new double[] { 0.15, 0.85, 0.25, 0.75, 0.35 }, 0),
            };

            var classifierGA = new TextClassifierGA(trainingData, geneCount: 5);
            Console.WriteLine("Training text classifier with GA...");
            classifierGA.Train(generations: 20);
            Console.WriteLine($"Best Accuracy: {classifierGA.GA.BestIndividual.Fitness:F4}");

            // Test prediction
            var testData = new double[] { 0.8, 0.2, 0.7, 0.3, 0.6 };
            int prediction = classifierGA.Predict(testData);
            Console.WriteLine($"Prediction: {prediction}");

            Console.WriteLine("\n=== Feature Selection ===");
            var featureGA = new FeatureSelectionGA(trainingData);
            Console.WriteLine("Selecting best features...");
            featureGA.Train(generations: 20);

            var selectedFeatures = featureGA.GetSelectedFeatures();
            Console.WriteLine($"Selected features: {string.Join(", ", selectedFeatures)}");
            Console.WriteLine($"Best Fitness: {featureGA.GA.BestIndividual.Fitness:F4}");

            Console.WriteLine("\n=== Hyperparameter Optimization ===");
            var hparamOptimizer = new HyperparameterOptimizer(trainingData);
            Console.WriteLine("Optimizing hyperparameters...");
            hparamOptimizer.Optimize(generations: 20);

            var (lr, reg, hidden, dropout) = hparamOptimizer.GetBestHyperparameters();
            Console.WriteLine($"Best Learning Rate: {lr:F4}");
            Console.WriteLine($"Best Regularization: {reg:F4}");
            Console.WriteLine($"Best Hidden Size: {hidden}");
            Console.WriteLine($"Best Dropout: {dropout:F4}");

            Console.WriteLine("\n=== Genetic Algorithm Concepts ===");
            Console.WriteLine("Population: Collection of solutions (individuals)");
            Console.WriteLine("Chromosome: String of genes representing a solution");
            Console.WriteLine("Gene: Feature or parameter in the solution");
            Console.WriteLine("Fitness: Quality measure of a solution");
            Console.WriteLine("Generation: One complete iteration of GA");

            Console.WriteLine("\n=== GA Operations ===");
            Console.WriteLine("1. Selection: Choose fittest individuals for reproduction");
            Console.WriteLine("   - Tournament Selection: Compare random subset");
            Console.WriteLine("   - Roulette Wheel: Probability based on fitness");
            Console.WriteLine("   - Rank Selection: Based on rank ordering");
            Console.WriteLine("\n2. Crossover: Combine two parent chromosomes");
            Console.WriteLine("   - Single-point: One split point");
            Console.WriteLine("   - Multi-point: Multiple split points");
            Console.WriteLine("   - Uniform: Bit-by-bit exchange");
            Console.WriteLine("\n3. Mutation: Random change in genes");
            Console.WriteLine("   - Prevents premature convergence");
            Console.WriteLine("   - Explores solution space");
            Console.WriteLine("   - Rate typically 0.01-0.1");
            Console.WriteLine("\n4. Evaluation: Compute fitness for each individual");
            Console.WriteLine("   - Domain-specific function");
            Console.WriteLine("   - Guides evolution");

            Console.WriteLine("\n=== NLP Applications ===");
            Console.WriteLine("1. Text Classification");
            Console.WriteLine("   - Optimize classifier weights");
            Console.WriteLine("   - Feature weight tuning");
            Console.WriteLine("   - Architecture search");
            Console.WriteLine("\n2. Machine Translation");
            Console.WriteLine("   - Optimize alignment parameters");
            Console.WriteLine("   - Phrase table weights");
            Console.WriteLine("   - Decoder settings");
            Console.WriteLine("\n3. Question Answering");
            Console.WriteLine("   - Retrieval ranking parameters");
            Console.WriteLine("   - Answer selection weights");
            Console.WriteLine("   - Confidence thresholds");
            Console.WriteLine("\n4. Text Summarization");
            Console.WriteLine("   - Extraction weights");
            Console.WriteLine("   - Scoring parameters");
            Console.WriteLine("   - Length constraints");

            Console.WriteLine("\n=== Advantages of GA ===");
            Console.WriteLine("- Global search capability");
            Console.WriteLine("- No gradient requirement");
            Console.WriteLine("- Parallelizable");
            Console.WriteLine("- Flexible problem formulation");
            Console.WriteLine("- Good for discrete/mixed problems");

            Console.WriteLine("\n=== Disadvantages of GA ===");
            Console.WriteLine("- Computationally expensive");
            Console.WriteLine("- Slow convergence");
            Console.WriteLine("- Premature convergence risk");
            Console.WriteLine("- Difficult hyperparameter tuning");
            Console.WriteLine("- No convergence guarantee");

            Console.WriteLine("\n=== GA Hyperparameters ===");
            Console.WriteLine("- Population Size: Larger = more diversity, slower");
            Console.WriteLine("- Mutation Rate: Higher = more exploration, instability");
            Console.WriteLine("- Crossover Rate: Controls recombination");
            Console.WriteLine("- Selection Pressure: Tournament size control");
            Console.WriteLine("- Elite Size: Best solutions to preserve");

            Console.WriteLine("\n=== Convergence Criteria ===");
            Console.WriteLine("- Maximum generations reached");
            Console.WriteLine("- Fitness threshold achieved");
            Console.WriteLine("- Population diversity too low");
            Console.WriteLine("- No improvement for N generations");
            Console.WriteLine("- Time limit exceeded");
        }
    }
}
