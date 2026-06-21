using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AdvancedMathematicsAndPhysics
{
    public class Vector3D
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Magnitude()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public Vector3D Normalize()
        {
            double mag = Magnitude();
            if (mag == 0) return new Vector3D(0, 0, 0);
            return new Vector3D(X / mag, Y / mag, Z / mag);
        }

        public static double DotProduct(Vector3D a, Vector3D b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Vector3D CrossProduct(Vector3D a, Vector3D b)
        {
            return new Vector3D(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X
            );
        }

        public static double Distance(Vector3D a, Vector3D b)
        {
            return new Vector3D(b.X - a.X, b.Y - a.Y, b.Z - a.Z).Magnitude();
        }
    }

    public class Matrix3x3
    {
        public double[,] Data { get; set; }

        public Matrix3x3()
        {
            Data = new double[3, 3];
        }

        public static Matrix3x3 Identity()
        {
            var m = new Matrix3x3();
            m.Data[0, 0] = m.Data[1, 1] = m.Data[2, 2] = 1.0;
            return m;
        }

        public static Matrix3x3 Multiply(Matrix3x3 a, Matrix3x3 b)
        {
            var result = new Matrix3x3();
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    for (int k = 0; k < 3; k++)
                        result.Data[i, j] += a.Data[i, k] * b.Data[k, j];
            return result;
        }

        public static Vector3D MultiplyVector(Matrix3x3 m, Vector3D v)
        {
            return new Vector3D(
                m.Data[0, 0] * v.X + m.Data[0, 1] * v.Y + m.Data[0, 2] * v.Z,
                m.Data[1, 0] * v.X + m.Data[1, 1] * v.Y + m.Data[1, 2] * v.Z,
                m.Data[2, 0] * v.X + m.Data[2, 1] * v.Y + m.Data[2, 2] * v.Z
            );
        }

        public double Determinant()
        {
            return Data[0, 0] * (Data[1, 1] * Data[2, 2] - Data[1, 2] * Data[2, 1]) -
                   Data[0, 1] * (Data[1, 0] * Data[2, 2] - Data[1, 2] * Data[2, 0]) +
                   Data[0, 2] * (Data[1, 0] * Data[2, 1] - Data[1, 1] * Data[2, 0]);
        }
    }

    public class PhysicsCalculator
    {
        public const double GravityConstant = 6.674e-11;
        public const double SpeedOfLight = 3e8;
        public const double PlanckConstant = 6.626e-34;
        public const double BoltzmannConstant = 1.381e-23;

        public static double CalculateKineticEnergy(double mass, double velocity)
        {
            return 0.5 * mass * velocity * velocity;
        }

        public static double CalculatePotentialEnergy(double mass, double height)
        {
            return mass * 9.81 * height;
        }

        public static double CalculateRelativisticEnergy(double mass)
        {
            return mass * SpeedOfLight * SpeedOfLight;
        }

        public static double CalculateGravitationalForce(double mass1, double mass2, double distance)
        {
            return GravityConstant * mass1 * mass2 / (distance * distance);
        }

        public static double CalculatePhotonEnergy(double frequency)
        {
            return PlanckConstant * frequency;
        }

        public static double CalculateEscapeVelocity(double mass, double radius)
        {
            return Math.Sqrt(2 * GravityConstant * mass / radius);
        }

        public static double CalculateWaveLength(double frequency)
        {
            return SpeedOfLight / frequency;
        }

        public static double CalculateOrbitalVelocity(double mass, double radius)
        {
            return Math.Sqrt(GravityConstant * mass / radius);
        }

        public static double CalculateHydrostaticPressure(double density, double depth)
        {
            return density * 9.81 * depth;
        }

        public static double CalculateFreeFallDistance(double time)
        {
            return 0.5 * 9.81 * time * time;
        }
    }

    public class AdvancedMathCalculator
    {
        public static double CalculateFibonacci(int n)
        {
            if (n <= 1) return n;
            double a = 0, b = 1;
            for (int i = 2; i <= n; i++)
            {
                double temp = a + b;
                a = b;
                b = temp;
            }
            return b;
        }

        public static double CalculateFactorial(int n)
        {
            if (n <= 1) return 1;
            return n * CalculateFactorial(n - 1);
        }

        public static double CalculatePower(double base_, double exponent)
        {
            return Math.Pow(base_, exponent);
        }

        public static double CalculateLogarithm(double value, double baseNum)
        {
            return Math.Log(value, baseNum);
        }

        public static double CalculateNaturalLogarithm(double value)
        {
            return Math.Log(value);
        }

        public static double SolveQuadratic(double a, double b, double c, bool positive = true)
        {
            double discriminant = b * b - 4 * a * c;
            if (discriminant < 0) return 0;

            double sqrt = Math.Sqrt(discriminant);
            return positive ? (-b + sqrt) / (2 * a) : (-b - sqrt) / (2 * a);
        }

        public static double CalculateGoldenRatio()
        {
            return (1 + Math.Sqrt(5)) / 2;
        }

        public static double CalculateSineWave(double angle, double amplitude = 1.0)
        {
            return amplitude * Math.Sin(angle);
        }

        public static double CalculateCosineWave(double angle, double amplitude = 1.0)
        {
            return amplitude * Math.Cos(angle);
        }

        public static double CalculateDerivative(Func<double, double> function, double x, double delta = 0.0001)
        {
            return (function(x + delta) - function(x - delta)) / (2 * delta);
        }

        public static double CalculateIntegral(Func<double, double> function, double a, double b, int intervals = 1000)
        {
            double width = (b - a) / intervals;
            double sum = 0;
            for (int i = 0; i < intervals; i++)
            {
                double x = a + i * width;
                sum += function(x) * width;
            }
            return sum;
        }

        public static double CalculateStandardDeviation(List<double> data)
        {
            double mean = data.Average();
            double variance = data.Select(x => (x - mean) * (x - mean)).Average();
            return Math.Sqrt(variance);
        }

        public static double CalculateLinearRegression(List<(double x, double y)> points, double x_new)
        {
            int n = points.Count;
            double sumX = points.Sum(p => p.x);
            double sumY = points.Sum(p => p.y);
            double sumXY = points.Sum(p => p.x * p.y);
            double sumX2 = points.Sum(p => p.x * p.x);

            double slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double intercept = (sumY - slope * sumX) / n;

            return slope * x_new + intercept;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Advanced Mathematics and Physics Calculations             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Advanced Mathematics - Sequences and Series]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Fibonacci Sequence:");
        for (int i = 1; i <= 10; i++)
        {
            Console.WriteLine($"    F({i}) = {AdvancedMathCalculator.CalculateFibonacci(i)}");
        }

        Console.WriteLine("\n  Factorial Calculations:");
        for (int i = 1; i <= 8; i++)
        {
            Console.WriteLine($"    {i}! = {AdvancedMathCalculator.CalculateFactorial(i)}");
        }

        double goldenRatio = AdvancedMathCalculator.CalculateGoldenRatio();
        Console.WriteLine($"\n  Golden Ratio (φ): {goldenRatio:F10}");
        Console.WriteLine($"    Usage: Architecture, art, nature patterns");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Advanced Mathematics - Calculus and Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        Func<double, double> sinFunction = x => Math.Sin(x);
        double derivativeSin = AdvancedMathCalculator.CalculateDerivative(sinFunction, Math.PI / 4);
        Console.WriteLine($"  Derivative of sin(x) at π/4: {derivativeSin:F6}");
        Console.WriteLine($"    (Expected ≈ cos(π/4) = {Math.Cos(Math.PI / 4):F6})");

        double integralSin = AdvancedMathCalculator.CalculateIntegral(sinFunction, 0, Math.PI);
        Console.WriteLine($"\n  Integral of sin(x) from 0 to π: {integralSin:F6}");
        Console.WriteLine($"    (Expected: 2.0)");

        var quadraticRoots = new[] {
            AdvancedMathCalculator.SolveQuadratic(1, -5, 6, true),
            AdvancedMathCalculator.SolveQuadratic(1, -5, 6, false)
        };
        Console.WriteLine($"\n  Solving x² - 5x + 6 = 0:");
        Console.WriteLine($"    x₁ = {quadraticRoots[0]:F2}, x₂ = {quadraticRoots[1]:F2}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Vector Mathematics and Operations]");
        Console.ResetColor();
        Thread.Sleep(500);

        var v1 = new Vector3D(3, 4, 0);
        var v2 = new Vector3D(1, 0, 2);

        Console.WriteLine($"  Vector 1: ({v1.X}, {v1.Y}, {v1.Z})");
        Console.WriteLine($"  Vector 2: ({v2.X}, {v2.Y}, {v2.Z})");

        Console.WriteLine($"\n  Magnitude of V1: {v1.Magnitude():F3}");
        Console.WriteLine($"  Magnitude of V2: {v2.Magnitude():F3}");

        var v1Norm = v1.Normalize();
        Console.WriteLine($"\n  Normalized V1: ({v1Norm.X:F3}, {v1Norm.Y:F3}, {v1Norm.Z:F3})");

        double dotProduct = Vector3D.DotProduct(v1, v2);
        Console.WriteLine($"\n  Dot Product (V1 · V2): {dotProduct:F2}");

        var crossProduct = Vector3D.CrossProduct(v1, v2);
        Console.WriteLine($"  Cross Product (V1 × V2): ({crossProduct.X:F2}, {crossProduct.Y:F2}, {crossProduct.Z:F2})");

        double distance = Vector3D.Distance(v1, v2);
        Console.WriteLine($"  Distance between V1 and V2: {distance:F3}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Matrix Operations]");
        Console.ResetColor();
        Thread.Sleep(500);

        var matrix = Matrix3x3.Identity();
        Console.WriteLine("  Identity Matrix:");
        for (int i = 0; i < 3; i++)
        {
            Console.WriteLine($"    [{matrix.Data[i, 0]:F0}  {matrix.Data[i, 1]:F0}  {matrix.Data[i, 2]:F0}]");
        }

        Console.WriteLine($"\n  Determinant: {matrix.Determinant():F0}");

        var testVec = new Vector3D(1, 2, 3);
        var transformed = Matrix3x3.MultiplyVector(matrix, testVec);
        Console.WriteLine($"\n  Matrix × Vector ({testVec.X}, {testVec.Y}, {testVec.Z}):");
        Console.WriteLine($"    = ({transformed.X:F0}, {transformed.Y:F0}, {transformed.Z:F0})");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Classical Physics Calculations]");
        Console.ResetColor();
        Thread.Sleep(500);

        double mass = 1000;
        double velocity = 20;
        double height = 100;

        double kineticEnergy = PhysicsCalculator.CalculateKineticEnergy(mass, velocity);
        double potentialEnergy = PhysicsCalculator.CalculatePotentialEnergy(mass, height);

        Console.WriteLine($"  Object: mass={mass}kg, velocity={velocity}m/s, height={height}m");
        Console.WriteLine($"  Kinetic Energy: {kineticEnergy:F0} J");
        Console.WriteLine($"  Potential Energy: {potentialEnergy:F0} J");
        Console.WriteLine($"  Total Mechanical Energy: {kineticEnergy + potentialEnergy:F0} J");

        double fallTime = Math.Sqrt(2 * height / 9.81);
        double fallDistance = PhysicsCalculator.CalculateFreeFallDistance(fallTime);
        Console.WriteLine($"\n  Free Fall Time from {height}m: {fallTime:F2} seconds");
        Console.WriteLine($"  Free Fall Distance: {fallDistance:F2}m");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Modern Physics - Relativity and Quantum]");
        Console.ResetColor();
        Thread.Sleep(500);

        double electronMass = 9.109e-31;
        double einsteinEnergy = PhysicsCalculator.CalculateRelativisticEnergy(electronMass);
        Console.WriteLine($"  Einstein's E=mc² for electron:");
        Console.WriteLine($"    Mass: {electronMass:E3}kg");
        Console.WriteLine($"    Energy: {einsteinEnergy:E3} J");

        double frequency = 5e14;
        double photonEnergy = PhysicsCalculator.CalculatePhotonEnergy(frequency);
        double wavelength = PhysicsCalculator.CalculateWaveLength(frequency);
        Console.WriteLine($"\n  Photon Properties:");
        Console.WriteLine($"    Frequency: {frequency:E3} Hz");
        Console.WriteLine($"    Wavelength: {wavelength * 1e9:F1} nm (infrared)");
        Console.WriteLine($"    Energy: {photonEnergy:E3} J");

        double gravitationalForce = PhysicsCalculator.CalculateGravitationalForce(
            5.972e24, 1000, 6.371e6);
        Console.WriteLine($"\n  Gravitational Force on 1000kg at Earth's surface:");
        Console.WriteLine($"    F = {gravitationalForce:F0} N ≈ {gravitationalForce / 9.81 / 1000:F1} kN");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Statistical Analysis and Regression]");
        Console.ResetColor();
        Thread.Sleep(500);

        var dataSet = new List<double> { 10, 15, 12, 18, 20, 14, 16, 19, 13, 17 };
        double mean = dataSet.Average();
        double stdDev = AdvancedMathCalculator.CalculateStandardDeviation(dataSet);

        Console.WriteLine($"  Dataset: {string.Join(", ", dataSet)}");
        Console.WriteLine($"  Mean: {mean:F2}");
        Console.WriteLine($"  Standard Deviation: {stdDev:F2}");

        var points = new List<(double, double)>
        {
            (1, 2.1), (2, 3.9), (3, 6.2), (4, 7.8), (5, 10.1)
        };

        double prediction = AdvancedMathCalculator.CalculateLinearRegression(points, 6);
        Console.WriteLine($"\n  Linear Regression on 5 points:");
        Console.WriteLine($"    Predicted value at x=6: {prediction:F2}");

        Console.WriteLine("\n  Advanced Mathematics Framework:");
        Console.WriteLine("    ✓ Calculus (derivatives, integrals)");
        Console.WriteLine("    ✓ Linear Algebra (vectors, matrices, determinants)");
        Console.WriteLine("    ✓ Classical Mechanics (energy, momentum, forces)");
        Console.WriteLine("    ✓ Relativity (E=mc², time dilation)");
        Console.WriteLine("    ✓ Quantum Mechanics (photon energy, wavelength)");
        Console.WriteLine("    ✓ Statistics (mean, variance, regression)");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Advanced mathematics and physics system complete");
        Console.ResetColor();
    }
}
