using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class UtilizeMultipleMetricUnits
{
    public class UnitSystem
    {
        public string SystemName { get; set; }
        public string BaseUnit { get; set; }
        public Dictionary<string, double> ConversionFactors { get; set; }
        public List<string> SupportedUnits { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Measurement
    {
        public string MeasurementId { get; set; }
        public double Value { get; set; }
        public string Unit { get; set; }
        public string UnitType { get; set; }
        public double StandardizedValue { get; set; }
        public DateTime RecordedDate { get; set; }
    }

    public class MetricInformation
    {
        public string MetricName { get; set; }
        public List<Measurement> Measurements { get; set; }
        public Dictionary<string, double> UnitsConversion { get; set; }
        public double AverageValue { get; set; }
        public double StandardDeviation { get; set; }
        public string PrimaryUnit { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UniversalMetricsEngine
    {
        private Dictionary<string, UnitSystem> unitSystems;
        private Dictionary<string, Measurement> measurements;
        private Dictionary<string, MetricInformation> metrics;
        private List<(string, string, double)> conversionLog;

        public UniversalMetricsEngine()
        {
            unitSystems = new Dictionary<string, UnitSystem>();
            measurements = new Dictionary<string, Measurement>();
            metrics = new Dictionary<string, MetricInformation>();
            conversionLog = new List<(string, string, double)>();
        }

        public void DefineUnitSystem(string systemName, string baseUnit, Dictionary<string, double> conversions)
        {
            var system = new UnitSystem
            {
                SystemName = systemName,
                BaseUnit = baseUnit,
                ConversionFactors = new Dictionary<string, double>(conversions),
                SupportedUnits = new List<string> { baseUnit },
                CreatedDate = DateTime.Now
            };

            foreach (var unit in conversions.Keys)
            {
                system.SupportedUnits.Add(unit);
            }

            unitSystems[systemName] = system;
        }

        public void RecordMeasurement(string measurementId, double value, string unit, string unitType)
        {
            var measurement = new Measurement
            {
                MeasurementId = measurementId,
                Value = value,
                Unit = unit,
                UnitType = unitType,
                StandardizedValue = value,
                RecordedDate = DateTime.Now
            };
            measurements[measurementId] = measurement;
        }

        public double ConvertUnit(double value, string fromUnit, string toUnit, string unitSystemName)
        {
            if (!unitSystems.ContainsKey(unitSystemName))
                return value;

            var system = unitSystems[unitSystemName];

            if (fromUnit == system.BaseUnit)
            {
                if (system.ConversionFactors.ContainsKey(toUnit))
                {
                    double converted = value * system.ConversionFactors[toUnit];
                    conversionLog.Add((fromUnit, toUnit, converted));
                    return converted;
                }
            }
            else if (toUnit == system.BaseUnit)
            {
                if (system.ConversionFactors.ContainsKey(fromUnit))
                {
                    double converted = value / system.ConversionFactors[fromUnit];
                    conversionLog.Add((fromUnit, toUnit, converted));
                    return converted;
                }
            }
            else if (system.ConversionFactors.ContainsKey(fromUnit) && system.ConversionFactors.ContainsKey(toUnit))
            {
                double toBase = value / system.ConversionFactors[fromUnit];
                double converted = toBase * system.ConversionFactors[toUnit];
                conversionLog.Add((fromUnit, toUnit, converted));
                return converted;
            }

            return value;
        }

        public void CreateMetric(string metricName, string primaryUnit, string unitSystemName)
        {
            var metric = new MetricInformation
            {
                MetricName = metricName,
                Measurements = new List<Measurement>(),
                UnitsConversion = new Dictionary<string, double>(),
                AverageValue = 0.0,
                StandardDeviation = 0.0,
                PrimaryUnit = primaryUnit,
                CreatedDate = DateTime.Now
            };
            metrics[metricName] = metric;

            if (unitSystems.ContainsKey(unitSystemName))
            {
                var system = unitSystems[unitSystemName];
                foreach (var unit in system.SupportedUnits)
                {
                    metric.UnitsConversion[unit] = 1.0;
                }
            }
        }

        public void AddMeasurementToMetric(string metricName, string measurementId)
        {
            if (!metrics.ContainsKey(metricName) || !measurements.ContainsKey(measurementId))
                return;

            var metric = metrics[metricName];
            var measurement = measurements[measurementId];

            metric.Measurements.Add(measurement);
            CalculateMetricStatistics(metricName);
        }

        private void CalculateMetricStatistics(string metricName)
        {
            if (!metrics.ContainsKey(metricName)) return;

            var metric = metrics[metricName];
            if (metric.Measurements.Count == 0)
            {
                metric.AverageValue = 0.0;
                metric.StandardDeviation = 0.0;
                return;
            }

            double sum = 0.0;
            foreach (var measurement in metric.Measurements)
            {
                sum += measurement.Value;
            }
            metric.AverageValue = sum / metric.Measurements.Count;

            double sumSquaredDiff = 0.0;
            foreach (var measurement in metric.Measurements)
            {
                double diff = measurement.Value - metric.AverageValue;
                sumSquaredDiff += diff * diff;
            }
            metric.StandardDeviation = Math.Sqrt(sumSquaredDiff / metric.Measurements.Count);
        }

        public Dictionary<string, double> GetMetricInAllUnits(string metricName, string unitSystemName)
        {
            if (!metrics.ContainsKey(metricName) || !unitSystems.ContainsKey(unitSystemName))
                return new Dictionary<string, double>();

            var metric = metrics[metricName];
            var result = new Dictionary<string, double>();

            var system = unitSystems[unitSystemName];
            foreach (var unit in system.SupportedUnits)
            {
                double converted = ConvertUnit(metric.AverageValue, metric.PrimaryUnit, unit, unitSystemName);
                result[unit] = converted;
            }

            return result;
        }

        public void DisplayUnitSystem(string systemName)
        {
            if (!unitSystems.ContainsKey(systemName)) return;

            var system = unitSystems[systemName];
            Console.WriteLine($"\n  Unit System: {system.SystemName}");
            Console.WriteLine($"  Base Unit: {system.BaseUnit}");
            Console.WriteLine($"  Supported Units: {string.Join(", ", system.SupportedUnits)}");
            Console.WriteLine($"  Conversion Factors:");
            foreach (var kvp in system.ConversionFactors)
            {
                Console.WriteLine($"    1 {system.BaseUnit} = {kvp.Value} {kvp.Key}");
            }
        }

        public void DisplayMeasurement(string measurementId)
        {
            if (!measurements.ContainsKey(measurementId)) return;

            var measurement = measurements[measurementId];
            Console.WriteLine($"\n  Measurement: {measurement.MeasurementId}");
            Console.WriteLine($"  Value: {measurement.Value}");
            Console.WriteLine($"  Unit: {measurement.Unit}");
            Console.WriteLine($"  Type: {measurement.UnitType}");
        }

        public void DisplayMetricInformation(string metricName)
        {
            if (!metrics.ContainsKey(metricName)) return;

            var metric = metrics[metricName];
            Console.WriteLine($"\n  Metric: {metric.MetricName}");
            Console.WriteLine($"  Primary Unit: {metric.PrimaryUnit}");
            Console.WriteLine($"  Measurements Recorded: {metric.Measurements.Count}");
            Console.WriteLine($"  Average Value: {metric.AverageValue:F3} {metric.PrimaryUnit}");
            Console.WriteLine($"  Standard Deviation: {metric.StandardDeviation:F3}");
        }

        public void DisplayMetricConversions(string metricName, string unitSystemName)
        {
            var conversions = GetMetricInAllUnits(metricName, unitSystemName);
            Console.WriteLine($"\n  {metricName} in All Units ({unitSystemName}):");
            foreach (var kvp in conversions)
            {
                Console.WriteLine($"    {kvp.Value:F3} {kvp.Key}");
            }
        }

        public int GetTotalUnitSystems()
        {
            return unitSystems.Count;
        }

        public int GetTotalMeasurements()
        {
            return measurements.Count;
        }

        public int GetTotalConversions()
        {
            return conversionLog.Count;
        }

        public List<string> GetAllSupportedUnits()
        {
            var units = new HashSet<string>();
            foreach (var system in unitSystems.Values)
            {
                foreach (var unit in system.SupportedUnits)
                {
                    units.Add(unit);
                }
            }
            return units.ToList();
        }

        public double GetMetricVariability(string metricName)
        {
            if (!metrics.ContainsKey(metricName)) return 0.0;
            var metric = metrics[metricName];
            if (metric.AverageValue == 0) return 0.0;
            return metric.StandardDeviation / metric.AverageValue;
        }

        public string GetMetricConsistency(double variability)
        {
            return variability switch
            {
                >= 0.5 => "Highly Variable - Significant fluctuations",
                >= 0.3 => "Variable - Notable variations",
                >= 0.15 => "Consistent - Minor variations",
                >= 0.05 => "Very Consistent - Stable measurements",
                _ => "Extremely Consistent - Minimal variation"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Utilize Multiple Metric Information and Units              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new UniversalMetricsEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Multiple Unit Systems]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DefineUnitSystem("Distance",
            "meter",
            new Dictionary<string, double>
            {
                {"kilometer", 0.001}, {"centimeter", 100}, {"mile", 0.000621371},
                {"yard", 1.09361}, {"foot", 3.28084}, {"inch", 39.3701}
            });

        engine.DefineUnitSystem("Mass",
            "kilogram",
            new Dictionary<string, double>
            {
                {"gram", 1000}, {"milligram", 1000000}, {"pound", 2.20462},
                {"ounce", 35.274}, {"ton", 0.001}
            });

        engine.DefineUnitSystem("Temperature",
            "Celsius",
            new Dictionary<string, double>
            {
                {"Kelvin", 273.15}, {"Fahrenheit", 1.8}
            });

        Console.WriteLine("  ✓ Defined 3 unit systems with multiple conversions");
        engine.DisplayUnitSystem("Distance");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Recording Measurements in Various Units]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RecordMeasurement("Distance-1", 1000.0, "meter", "Distance");
        engine.RecordMeasurement("Distance-2", 1.5, "kilometer", "Distance");
        engine.RecordMeasurement("Distance-3", 500.0, "centimeter", "Distance");

        engine.RecordMeasurement("Mass-1", 75.0, "kilogram", "Mass");
        engine.RecordMeasurement("Mass-2", 150.0, "pound", "Mass");
        engine.RecordMeasurement("Mass-3", 2.5, "ton", "Mass");

        Console.WriteLine("  ✓ Recorded measurements across multiple units");
        engine.DisplayMeasurement("Distance-1");
        engine.DisplayMeasurement("Mass-1");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Creating Metrics with Unit Conversions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateMetric("TravelDistance", "kilometer", "Distance");
        engine.CreateMetric("ObjectMass", "kilogram", "Mass");

        engine.AddMeasurementToMetric("TravelDistance", "Distance-1");
        engine.AddMeasurementToMetric("TravelDistance", "Distance-2");
        engine.AddMeasurementToMetric("TravelDistance", "Distance-3");

        engine.AddMeasurementToMetric("ObjectMass", "Mass-1");
        engine.AddMeasurementToMetric("ObjectMass", "Mass-2");
        engine.AddMeasurementToMetric("ObjectMass", "Mass-3");

        Console.WriteLine("  ✓ Created metrics and associated measurements");
        engine.DisplayMetricInformation("TravelDistance");
        engine.DisplayMetricInformation("ObjectMass");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Performing Unit Conversions]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Converting units in Distance system:");
        double meters = 1500.0;
        double km = engine.ConvertUnit(meters, "meter", "kilometer", "Distance");
        double miles = engine.ConvertUnit(meters, "meter", "mile", "Distance");
        double feet = engine.ConvertUnit(meters, "meter", "foot", "Distance");

        Console.WriteLine($"    {meters} meters = {km} kilometers");
        Console.WriteLine($"    {meters} meters = {miles} miles");
        Console.WriteLine($"    {meters} meters = {feet} feet");

        Console.WriteLine("\n  Converting units in Mass system:");
        double kg = 50.0;
        double lbs = engine.ConvertUnit(kg, "kilogram", "pound", "Mass");
        double grams = engine.ConvertUnit(kg, "kilogram", "gram", "Mass");

        Console.WriteLine($"    {kg} kilograms = {lbs} pounds");
        Console.WriteLine($"    {kg} kilograms = {grams} grams");

        Console.WriteLine("  ✓ Unit conversions completed");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Displaying Metrics in All Units]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayMetricConversions("TravelDistance", "Distance");
        engine.DisplayMetricConversions("ObjectMass", "Mass");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Metric Variability Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        double distVariability = engine.GetMetricVariability("TravelDistance");
        double massVariability = engine.GetMetricVariability("ObjectMass");

        string distConsistency = engine.GetMetricConsistency(distVariability);
        string massConsistency = engine.GetMetricConsistency(massVariability);

        Console.WriteLine($"  TravelDistance:");
        Console.WriteLine($"    Variability: {distVariability:F3}");
        Console.WriteLine($"    Consistency: {distConsistency}");

        Console.WriteLine($"\n  ObjectMass:");
        Console.WriteLine($"    Variability: {massVariability:F3}");
        Console.WriteLine($"    Consistency: {massConsistency}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Multi-Metric System Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  System Statistics:");
        Console.WriteLine($"    Unit Systems: {engine.GetTotalUnitSystems()}");
        Console.WriteLine($"    Total Measurements: {engine.GetTotalMeasurements()}");
        Console.WriteLine($"    Unit Conversions Performed: {engine.GetTotalConversions()}");
        Console.WriteLine($"    Total Supported Units: {engine.GetAllSupportedUnits().Count}");

        Console.WriteLine("\n  Multi-Metric Framework:");
        Console.WriteLine("    Layer 1: Unit System Definition (establish conversion hierarchies)");
        Console.WriteLine("    Layer 2: Measurement Recording (capture values in any unit)");
        Console.WriteLine("    Layer 3: Metric Creation (organize measurements by type)");
        Console.WriteLine("    Layer 4: Unit Conversion (translate between units)");
        Console.WriteLine("    Layer 5: Statistical Analysis (calculate properties)");
        Console.WriteLine("    Layer 6: Universal Representation (display in all units)");
        Console.WriteLine("    Layer 7: Consistency Assessment (evaluate measurement quality)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Multiple unit systems with complex conversions");
        Console.WriteLine("    ✓ Measurements recorded in diverse units");
        Console.WriteLine("    ✓ Automatic unit conversion across systems");
        Console.WriteLine("    ✓ Metric statistics independent of unit choice");
        Console.WriteLine("    ✓ Universal representation of measurements");
        Console.WriteLine("    ✓ Variability and consistency analysis");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-metric units system complete");
        Console.ResetColor();
    }
}
