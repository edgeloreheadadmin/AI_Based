using System;

namespace NeuralNetwork.ActivationFunctions
{
    /// <summary>
    /// Interface for activation functions used in neural networks.
    /// Activation functions introduce non-linearity into the network.
    /// </summary>
    public interface IActivationFunction
    {
        /// <summary>
        /// Applies the activation function to the input value.
        /// </summary>
        /// <param name="x">Input value</param>
        /// <returns>Activated output value</returns>
        double Activate(double x);

        /// <summary>
        /// Computes the derivative of the activation function.
        /// Used during backpropagation in neural network training.
        /// </summary>
        /// <param name="x">Input value (or activated output)</param>
        /// <returns>Derivative value</returns>
        double Derivative(double x);

        /// <summary>
        /// Applies the activation function to an array of values.
        /// </summary>
        /// <param name="values">Array of input values</param>
        /// <returns>Array of activated values</returns>
        double[] ActivateArray(double[] values);

        /// <summary>
        /// Computes derivatives for an array of values.
        /// </summary>
        /// <param name="values">Array of values</param>
        /// <returns>Array of derivative values</returns>
        double[] DerivativeArray(double[] values);
    }

    /// <summary>
    /// Sigmoid Activation Function: f(x) = 1 / (1 + e^-x)
    /// Output range: [0, 1]
    /// Common use cases: Binary classification, sentiment analysis, text classification
    /// </summary>
    public class SigmoidActivation : IActivationFunction
    {
        public double Activate(double x)
        {
            if (x < -500) return 0;
            if (x > 500) return 1;
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public double Derivative(double x)
        {
            double sigmoid = Activate(x);
            return sigmoid * (1.0 - sigmoid);
        }

        public double[] ActivateArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Activate(values[i]);
            }
            return result;
        }

        public double[] DerivativeArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Derivative(values[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Hyperbolic Tangent (Tanh) Activation Function: f(x) = (e^x - e^-x) / (e^x + e^-x)
    /// Output range: [-1, 1]
    /// Common use cases: Machine translation, text summarization, recurrent networks
    /// Advantage: Zero-centered output, often converges faster than sigmoid
    /// </summary>
    public class TanhActivation : IActivationFunction
    {
        public double Activate(double x)
        {
            if (x < -20) return -1;
            if (x > 20) return 1;
            return Math.Tanh(x);
        }

        public double Derivative(double x)
        {
            double tanh = Activate(x);
            return 1.0 - (tanh * tanh);
        }

        public double[] ActivateArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Activate(values[i]);
            }
            return result;
        }

        public double[] DerivativeArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Derivative(values[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Rectified Linear Unit (ReLU) Activation Function: f(x) = max(0, x)
    /// Output range: [0, ∞)
    /// Common use cases: Question answering, image captioning, deep networks
    /// Advantages: Computationally efficient, helps with vanishing gradient problem
    /// </summary>
    public class ReLUActivation : IActivationFunction
    {
        public double Activate(double x)
        {
            return Math.Max(0, x);
        }

        public double Derivative(double x)
        {
            return x > 0 ? 1.0 : 0.0;
        }

        public double[] ActivateArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Activate(values[i]);
            }
            return result;
        }

        public double[] DerivativeArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Derivative(values[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Leaky ReLU Activation Function: f(x) = x if x > 0 else alpha * x
    /// Output range: (-∞, ∞) with negative slope for negative inputs
    /// Advantage: Allows small gradients for negative inputs, prevents dead neurons
    /// </summary>
    public class LeakyReLUActivation : IActivationFunction
    {
        private readonly double alpha;

        public LeakyReLUActivation(double alpha = 0.01)
        {
            this.alpha = alpha;
        }

        public double Activate(double x)
        {
            return x > 0 ? x : alpha * x;
        }

        public double Derivative(double x)
        {
            return x > 0 ? 1.0 : alpha;
        }

        public double[] ActivateArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Activate(values[i]);
            }
            return result;
        }

        public double[] DerivativeArray(double[] values)
        {
            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = Derivative(values[i]);
            }
            return result;
        }
    }

    /// <summary>
    /// Softmax Activation Function: Often used in output layer for multi-class classification
    /// Converts output to probability distribution where sum of outputs = 1
    /// </summary>
    public class SoftmaxActivation : IActivationFunction
    {
        public double Activate(double x)
        {
            throw new NotImplementedException("Use ActivateArray for softmax");
        }

        public double Derivative(double x)
        {
            throw new NotImplementedException("Use DerivativeArray for softmax");
        }

        public double[] ActivateArray(double[] values)
        {
            double max = double.MinValue;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > max) max = values[i];
            }

            double[] exponentials = new double[values.Length];
            double sum = 0;

            for (int i = 0; i < values.Length; i++)
            {
                exponentials[i] = Math.Exp(values[i] - max);
                sum += exponentials[i];
            }

            double[] result = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                result[i] = exponentials[i] / sum;
            }

            return result;
        }

        public double[] DerivativeArray(double[] values)
        {
            double[] softmax = ActivateArray(values);
            double[] result = new double[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                result[i] = softmax[i] * (1.0 - softmax[i]);
            }

            return result;
        }
    }

    /// <summary>
    /// Utility class demonstrating activation function usage in NLP tasks
    /// </summary>
    public static class ActivationFunctionExamples
    {
        public static void DemonstrateSentimentAnalysis()
        {
            Console.WriteLine("=== Sentiment Analysis (Sigmoid) ===");
            IActivationFunction sigmoid = new SigmoidActivation();

            double[] logits = { -2.0, -0.5, 0.5, 2.0 };
            double[] predictions = sigmoid.ActivateArray(logits);

            for (int i = 0; i < logits.Length; i++)
            {
                Console.WriteLine($"Logit: {logits[i]:F2} -> Probability: {predictions[i]:F4}");
            }
        }

        public static void DemonstrateTextClassification()
        {
            Console.WriteLine("\n=== Multi-class Text Classification (Softmax) ===");
            IActivationFunction softmax = new SoftmaxActivation();

            double[] logits = { 2.0, 1.0, 0.1 };
            double[] probabilities = softmax.ActivateArray(logits);

            string[] categories = { "Sports", "Business", "Politics" };

            for (int i = 0; i < logits.Length; i++)
            {
                Console.WriteLine($"{categories[i]}: {probabilities[i]:F4}");
            }
        }

        public static void DemonstrateMachineTranslation()
        {
            Console.WriteLine("\n=== Machine Translation (Tanh) ===");
            IActivationFunction tanh = new TanhActivation();

            double[] hiddenLayerOutput = { -2.0, -1.0, 0.0, 1.0, 2.0 };
            double[] activated = tanh.ActivateArray(hiddenLayerOutput);

            for (int i = 0; i < hiddenLayerOutput.Length; i++)
            {
                Console.WriteLine($"Hidden: {hiddenLayerOutput[i]:F2} -> Tanh: {activated[i]:F4}");
            }
        }

        public static void DemonstrateQuestionAnswering()
        {
            Console.WriteLine("\n=== Question Answering (ReLU) ===");
            IActivationFunction relu = new ReLUActivation();

            double[] logits = { -3.0, -1.0, 0.0, 1.0, 3.0 };
            double[] activated = relu.ActivateArray(logits);

            for (int i = 0; i < logits.Length; i++)
            {
                Console.WriteLine($"Logit: {logits[i]:F2} -> ReLU: {activated[i]:F2}");
            }
        }

        public static void DemonstrateLearningComparisonWithDerivatives()
        {
            Console.WriteLine("\n=== Activation Function Derivatives (for Backpropagation) ===");

            double[] testValues = { -2.0, -1.0, 0.0, 1.0, 2.0 };

            IActivationFunction sigmoid = new SigmoidActivation();
            IActivationFunction tanh = new TanhActivation();
            IActivationFunction relu = new ReLUActivation();

            Console.WriteLine("Value\t| Sigmoid'\t| Tanh'\t\t| ReLU'");
            Console.WriteLine("--------|-------------|-----------|--------");

            for (int i = 0; i < testValues.Length; i++)
            {
                double sigmoidDeriv = sigmoid.Derivative(testValues[i]);
                double tanhDeriv = tanh.Derivative(testValues[i]);
                double reluDeriv = relu.Derivative(testValues[i]);

                Console.WriteLine($"{testValues[i]:F2}\t| {sigmoidDeriv:F4}\t\t| {tanhDeriv:F4}\t\t| {reluDeriv:F4}");
            }
        }
    }
}
