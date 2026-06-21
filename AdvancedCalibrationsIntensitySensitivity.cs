using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AdvancedCalibrationsIntensitySensitivity
{
    public class Calibration
    {
        public string CalibrationName { get; set; }
        public double BaselineValue { get; set; }
        public double CurrentValue { get; set; }
        public double Sensitivity { get; set; }
        public double Intensity { get; set; }
        public double AccuracyTolerance { get; set; }
        public List<double> CalibratedValues { get; set; }
        public DateTime LastCalibrated { get; set; }
    }

    public class SensoryChannel
    {
        public string ChannelName { get; set; }
        public double Threshold { get; set; }
        public double MaximumCapacity { get; set; }
        public double CurrentSignal { get; set; }
        public double NoiseLevel { get; set; }
        public double SignalToNoiseRatio { get; set; }
        public List<double> SignalHistory { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class NeuralResponseProfile
    {
        public string ProfileName { get; set; }
        public Dictionary<string, double> SensitivityMap { get; set; }
        public Dictionary<string, double> IntensityMap { get; set; }
        public double OverallResponsiveness { get; set; }
        public double AdaptationRate { get; set; }
        public List<string> ActiveChannels { get; set; }
        public DateTime ProfileDate { get; set; }
    }

    public class CalibrationEngine
    {
        private Dictionary<string, Calibration> calibrations;
        private Dictionary<string, SensoryChannel> channels;
        private Dictionary<string, NeuralResponseProfile> profiles;
        private List<(string, double, double)> calibrationLog;

        public CalibrationEngine()
        {
            calibrations = new Dictionary<string, Calibration>();
            channels = new Dictionary<string, SensoryChannel>();
            profiles = new Dictionary<string, NeuralResponseProfile>();
            calibrationLog = new List<(string, double, double)>();
        }

        public void InitializeCalibration(string calibrationName, double baseline)
        {
            var calibration = new Calibration
            {
                CalibrationName = calibrationName,
                BaselineValue = baseline,
                CurrentValue = baseline,
                Sensitivity = 0.5,
                Intensity = 0.5,
                AccuracyTolerance = 0.05,
                CalibratedValues = new List<double> { baseline },
                LastCalibrated = DateTime.Now
            };
            calibrations[calibrationName] = calibration;
        }

        public void IncreaseSensitivity(string calibrationName, double sensitivityBoost)
        {
            if (!calibrations.ContainsKey(calibrationName)) return;

            var calibration = calibrations[calibrationName];
            calibration.Sensitivity = Math.Min(calibration.Sensitivity + sensitivityBoost, 1.0);
            calibration.AccuracyTolerance = Math.Max(calibration.AccuracyTolerance - (sensitivityBoost * 0.01), 0.001);
        }

        public void AdjustIntensity(string calibrationName, double intensityAdjustment)
        {
            if (!calibrations.ContainsKey(calibrationName)) return;

            var calibration = calibrations[calibrationName];
            calibration.Intensity = Math.Max(0.01, Math.Min(calibration.Intensity + intensityAdjustment, 1.0));
        }

        public void PerformCalibration(string calibrationName, double measuredValue)
        {
            if (!calibrations.ContainsKey(calibrationName)) return;

            var calibration = calibrations[calibrationName];
            double difference = Math.Abs(measuredValue - calibration.CurrentValue);

            double correction = (measuredValue - calibration.CurrentValue) * calibration.Sensitivity;
            calibration.CurrentValue = calibration.CurrentValue + correction;

            double intensityFactor = 1.0 + (calibration.Intensity * 0.5);
            double accuracy = 1.0 / (1.0 + difference * intensityFactor);

            calibration.CalibratedValues.Add(calibration.CurrentValue);
            calibration.LastCalibrated = DateTime.Now;

            calibrationLog.Add((calibrationName, calibration.Sensitivity, accuracy));
        }

        public void CreateSensoryChannel(string channelName, double threshold, double maxCapacity)
        {
            var channel = new SensoryChannel
            {
                ChannelName = channelName,
                Threshold = threshold,
                MaximumCapacity = maxCapacity,
                CurrentSignal = 0.0,
                NoiseLevel = 0.1,
                SignalToNoiseRatio = 0.0,
                SignalHistory = new List<double>(),
                CreatedDate = DateTime.Now
            };
            channels[channelName] = channel;
        }

        public void TransmitSignalThroughChannel(string channelName, double signalStrength)
        {
            if (!channels.ContainsKey(channelName)) return;

            var channel = channels[channelName];
            double effectiveSignal = Math.Min(signalStrength, channel.MaximumCapacity);

            if (effectiveSignal >= channel.Threshold)
            {
                channel.CurrentSignal = effectiveSignal;
                channel.SignalHistory.Add(effectiveSignal);

                double snr = effectiveSignal / (channel.NoiseLevel + 0.001);
                channel.SignalToNoiseRatio = Math.Min(snr, 100.0);
            }
        }

        public void CreateNeuralProfile(string profileName, List<string> channelNames)
        {
            var profile = new NeuralResponseProfile
            {
                ProfileName = profileName,
                SensitivityMap = new Dictionary<string, double>(),
                IntensityMap = new Dictionary<string, double>(),
                OverallResponsiveness = 0.5,
                AdaptationRate = 0.1,
                ActiveChannels = new List<string>(channelNames),
                ProfileDate = DateTime.Now
            };

            foreach (var channelName in channelNames)
            {
                profile.SensitivityMap[channelName] = 0.5;
                profile.IntensityMap[channelName] = 0.5;
            }

            profiles[profileName] = profile;
        }

        public void OptimizeNeuralResponse(string profileName, string channelName, double sensitivityAdjustment)
        {
            if (!profiles.ContainsKey(profileName)) return;

            var profile = profiles[profileName];
            if (profile.SensitivityMap.ContainsKey(channelName))
            {
                profile.SensitivityMap[channelName] =
                    Math.Min(profile.SensitivityMap[channelName] + sensitivityAdjustment, 1.0);

                double avgSensitivity = profile.SensitivityMap.Values.Average();
                double avgIntensity = profile.IntensityMap.Values.Average();
                profile.OverallResponsiveness = (avgSensitivity * 0.6 + avgIntensity * 0.4);

                profile.AdaptationRate = Math.Min(profile.AdaptationRate + 0.02, 0.99);
            }
        }

        public void DisplayCalibrationStatus(string calibrationName)
        {
            if (!calibrations.ContainsKey(calibrationName)) return;

            var calibration = calibrations[calibrationName];
            Console.WriteLine($"\n  Calibration: {calibration.CalibrationName}");
            Console.WriteLine($"  Baseline: {calibration.BaselineValue:F3}");
            Console.WriteLine($"  Current Value: {calibration.CurrentValue:F3}");
            Console.WriteLine($"  Sensitivity: {calibration.Sensitivity * 100:F1}%");
            Console.WriteLine($"  Intensity: {calibration.Intensity * 100:F1}%");
            Console.WriteLine($"  Accuracy Tolerance: {calibration.AccuracyTolerance:F4}");
            Console.WriteLine($"  Calibration Points: {calibration.CalibratedValues.Count}");
        }

        public void DisplayChannelStatus(string channelName)
        {
            if (!channels.ContainsKey(channelName)) return;

            var channel = channels[channelName];
            Console.WriteLine($"\n  Sensory Channel: {channel.ChannelName}");
            Console.WriteLine($"  Threshold: {channel.Threshold:F3}");
            Console.WriteLine($"  Current Signal: {channel.CurrentSignal:F3}");
            Console.WriteLine($"  Noise Level: {channel.NoiseLevel:F3}");
            Console.WriteLine($"  Signal-to-Noise Ratio: {channel.SignalToNoiseRatio:F2}:1");
            Console.WriteLine($"  Signal History Length: {channel.SignalHistory.Count}");
        }

        public void DisplayNeuralProfile(string profileName)
        {
            if (!profiles.ContainsKey(profileName)) return;

            var profile = profiles[profileName];
            Console.WriteLine($"\n  Neural Profile: {profile.ProfileName}");
            Console.WriteLine($"  Overall Responsiveness: {profile.OverallResponsiveness * 100:F1}%");
            Console.WriteLine($"  Adaptation Rate: {profile.AdaptationRate * 100:F1}%");
            Console.WriteLine($"  Active Channels: {profile.ActiveChannels.Count}");
            Console.WriteLine($"  Channel Sensitivities:");
            foreach (var kvp in profile.SensitivityMap)
            {
                Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F1}%");
            }
        }

        public Dictionary<string, double> GetCalibrationAccuracies()
        {
            var accuracies = new Dictionary<string, double>();
            var groupedLogs = calibrationLog.GroupBy(x => x.Item1);

            foreach (var group in groupedLogs)
            {
                double avgAccuracy = group.Average(x => x.Item3);
                accuracies[group.Key] = avgAccuracy;
            }

            return accuracies.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public double GetSystemResponsiveness()
        {
            if (profiles.Count == 0) return 0.0;
            return profiles.Values.Average(p => p.OverallResponsiveness);
        }

        public int GetTotalCalibrationEvents()
        {
            return calibrationLog.Count;
        }

        public double GetAverageSensitivity()
        {
            if (calibrations.Count == 0) return 0.0;
            return calibrations.Values.Average(c => c.Sensitivity);
        }

        public string GetCalibrationQuality(double accuracy)
        {
            return accuracy switch
            {
                >= 0.95 => "Exceptional - Precision calibration achieved",
                >= 0.85 => "Excellent - High accuracy calibration",
                >= 0.75 => "Good - Reliable calibration",
                >= 0.65 => "Acceptable - Working calibration",
                _ => "Needs Adjustment - Recalibration required"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Advanced Calibrations: Intensity, Sensitivity & Precision    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new CalibrationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Initializing Calibration Systems]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.InitializeCalibration("VisualAcuity", 1.0);
        engine.InitializeCalibration("AuditoryThreshold", 0.5);
        engine.InitializeCalibration("TactileResponsivity", 0.6);
        engine.InitializeCalibration("TemperatureSensitivity", 0.4);

        Console.WriteLine("  ✓ Initialized 4 calibration systems");
        engine.DisplayCalibrationStatus("VisualAcuity");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Enhancing Sensitivity and Intensity]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Increasing sensitivity to detect subtle variations:");
        for (int i = 0; i < 5; i++)
        {
            engine.IncreaseSensitivity("VisualAcuity", 0.12);
            engine.IncreaseSensitivity("AuditoryThreshold", 0.10);
            engine.AdjustIntensity("VisualAcuity", 0.08);
            engine.AdjustIntensity("AuditoryThreshold", 0.06);
        }

        Console.WriteLine("  ✓ Sensitivity and intensity enhanced");
        engine.DisplayCalibrationStatus("VisualAcuity");
        engine.DisplayCalibrationStatus("AuditoryThreshold");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Sensory Input Channels]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateSensoryChannel("VisualInput", 0.1, 1.0);
        engine.CreateSensoryChannel("AuditoryInput", 0.2, 0.8);
        engine.CreateSensoryChannel("TactileInput", 0.15, 0.9);
        engine.CreateSensoryChannel("VestibularInput", 0.05, 1.0);

        Console.WriteLine("  ✓ Created 4 sensory input channels");
        engine.DisplayChannelStatus("VisualInput");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Transmitting Calibrated Signals]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Transmitting signals through calibrated channels:");
        engine.TransmitSignalThroughChannel("VisualInput", 0.85);
        engine.TransmitSignalThroughChannel("AuditoryInput", 0.65);
        engine.TransmitSignalThroughChannel("TactileInput", 0.75);
        engine.TransmitSignalThroughChannel("VestibularInput", 0.55);

        Console.WriteLine("  ✓ Signals transmitted with high signal-to-noise ratios");
        engine.DisplayChannelStatus("VisualInput");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Performing Precision Calibrations]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Performing multiple calibration adjustments:");
        for (int i = 0; i < 8; i++)
        {
            double variance = (i % 2 == 0 ? 1.05 : 0.95);
            engine.PerformCalibration("VisualAcuity", 1.0 * variance);
            engine.PerformCalibration("AuditoryThreshold", 0.5 * variance);
            engine.PerformCalibration("TactileResponsivity", 0.6 * variance);
            Console.WriteLine($"  Calibration cycle {i + 1} completed");
        }

        Console.WriteLine("  ✓ Precision calibrations achieved");
        engine.DisplayCalibrationStatus("VisualAcuity");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Creating Neural Response Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateNeuralProfile("AlertProfile", new List<string> { "VisualInput", "AuditoryInput" });
        engine.CreateNeuralProfile("RestProfile", new List<string> { "TactileInput", "VestibularInput" });

        Console.WriteLine("  ✓ Created 2 neural response profiles");

        for (int i = 0; i < 5; i++)
        {
            engine.OptimizeNeuralResponse("AlertProfile", "VisualInput", 0.12);
            engine.OptimizeNeuralResponse("AlertProfile", "AuditoryInput", 0.10);
            engine.OptimizeNeuralResponse("RestProfile", "TactileInput", 0.08);
        }

        engine.DisplayNeuralProfile("AlertProfile");
        engine.DisplayNeuralProfile("RestProfile");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Advanced Calibration Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var accuracies = engine.GetCalibrationAccuracies();
        Console.WriteLine("  Calibration Accuracy Rankings:");
        int rank = 1;
        foreach (var kvp in accuracies)
        {
            string quality = engine.GetCalibrationQuality(kvp.Value);
            Console.WriteLine($"  #{rank}: {kvp.Key}");
            Console.WriteLine($"       Accuracy: {kvp.Value * 100:F1}%");
            Console.WriteLine($"       Quality: {quality}");
            rank++;
        }

        Console.WriteLine($"\n  System Metrics:");
        Console.WriteLine($"    Overall Responsiveness: {engine.GetSystemResponsiveness() * 100:F1}%");
        Console.WriteLine($"    Average Sensitivity: {engine.GetAverageSensitivity() * 100:F1}%");
        Console.WriteLine($"    Total Calibration Events: {engine.GetTotalCalibrationEvents()}");

        Console.WriteLine("\n  Calibration Framework:");
        Console.WriteLine("    Sensitivity: Ability to detect minute changes (0-100%)");
        Console.WriteLine("    Intensity: Strength of response to stimulus (0-100%)");
        Console.WriteLine("    Accuracy: Precision of measurement within tolerance");
        Console.WriteLine("    Signal-to-Noise: Ratio of signal clarity to interference");
        Console.WriteLine("\n  Key Mechanisms:");
        Console.WriteLine("    ✓ Multi-point calibration for accuracy verification");
        Console.WriteLine("    ✓ Sensitivity enhancement for subtle perception");
        Console.WriteLine("    ✓ Intensity modulation for response magnitude");
        Console.WriteLine("    ✓ Neural profile adaptation for optimal performance");
        Console.WriteLine("    ✓ Continuous recalibration for system tuning");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Advanced calibration system complete");
        Console.ResetColor();
    }
}
