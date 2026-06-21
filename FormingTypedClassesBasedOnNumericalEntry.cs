using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class FormingTypedClassesBasedOnNumericalEntry
{
    public class NumericSpecification
    {
        public string SpecId { get; set; }
        public double Value { get; set; }
        public string Category { get; set; }
        public List<double> RangeThresholds { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TypedClass
    {
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public string ClassType { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public double NumericalBasis { get; set; }
        public List<string> ComputedCharacteristics { get; set; }
        public DateTime FormedDate { get; set; }
    }

    public class ClassFormationRule
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public double MinThreshold { get; set; }
        public double MaxThreshold { get; set; }
        public string ResultingClassName { get; set; }
        public List<string> ApplicableProperties { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class FormationAnalysis
    {
        public string AnalysisId { get; set; }
        public int TotalClassesFormed { get; set; }
        public Dictionary<string, int> ClassDistribution { get; set; }
        public List<(string, double)> ClassBoundaries { get; set; }
        public double AverageNumericalValue { get; set; }
        public string PrevalentClassType { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class DynamicClassFormationEngine
    {
        private Dictionary<string, NumericSpecification> specifications;
        private Dictionary<string, TypedClass> classes;
        private Dictionary<string, ClassFormationRule> rules;
        private Dictionary<string, FormationAnalysis> analyses;

        public DynamicClassFormationEngine()
        {
            specifications = new Dictionary<string, NumericSpecification>();
            classes = new Dictionary<string, TypedClass>();
            rules = new Dictionary<string, ClassFormationRule>();
            analyses = new Dictionary<string, FormationAnalysis>();
        }

        public void RegisterFormationRule(string ruleId, string ruleName, double minThreshold,
                                         double maxThreshold, string className, List<string> properties)
        {
            var rule = new ClassFormationRule
            {
                RuleId = ruleId,
                RuleName = ruleName,
                MinThreshold = minThreshold,
                MaxThreshold = maxThreshold,
                ResultingClassName = className,
                ApplicableProperties = new List<string>(properties),
                CreatedDate = DateTime.Now
            };
            rules[ruleId] = rule;
        }

        public void RegisterNumericSpecification(string specId, double value, string category, List<double> thresholds)
        {
            var spec = new NumericSpecification
            {
                SpecId = specId,
                Value = value,
                Category = category,
                RangeThresholds = new List<double>(thresholds),
                CreatedDate = DateTime.Now
            };
            specifications[specId] = spec;
        }

        public void FormTypeFromNumeric(string classId, string specId)
        {
            if (!specifications.ContainsKey(specId)) return;

            var spec = specifications[specId];
            var matchedRule = FindMatchingRule(spec.Value);

            var newClass = new TypedClass
            {
                ClassId = classId,
                ClassName = matchedRule != null ? matchedRule.ResultingClassName : "Unclassified",
                ClassType = spec.Category,
                Properties = new Dictionary<string, object>(),
                NumericalBasis = spec.Value,
                ComputedCharacteristics = new List<string>(),
                FormedDate = DateTime.Now
            };

            if (matchedRule != null)
            {
                foreach (var prop in matchedRule.ApplicableProperties)
                {
                    newClass.Properties[prop] = GeneratePropertyValue(prop, spec.Value);
                }

                newClass.ComputedCharacteristics = GenerateCharacteristics(spec.Value, matchedRule);
            }

            classes[classId] = newClass;
        }

        private ClassFormationRule FindMatchingRule(double value)
        {
            foreach (var rule in rules.Values)
            {
                if (value >= rule.MinThreshold && value <= rule.MaxThreshold)
                {
                    return rule;
                }
            }
            return null;
        }

        private object GeneratePropertyValue(string propertyName, double basis)
        {
            if (propertyName.Contains("Level"))
                return Math.Round(basis * 100, 2);
            if (propertyName.Contains("Status"))
                return basis > 50 ? "Active" : "Inactive";
            if (propertyName.Contains("Score"))
                return Math.Round(basis, 2);
            return basis;
        }

        private List<string> GenerateCharacteristics(double value, ClassFormationRule rule)
        {
            var characteristics = new List<string>();

            if (value < rule.MinThreshold + (rule.MaxThreshold - rule.MinThreshold) * 0.25)
                characteristics.Add("Low intensity");
            else if (value > rule.MinThreshold + (rule.MaxThreshold - rule.MinThreshold) * 0.75)
                characteristics.Add("High intensity");
            else
                characteristics.Add("Medium intensity");

            characteristics.Add($"Within {rule.ResultingClassName} range");
            characteristics.Add($"Numerical basis: {value:F2}");

            if (value > 50)
                characteristics.Add("Above median");
            else
                characteristics.Add("Below median");

            return characteristics;
        }

        public void AnalyzeFormations(string analysisId)
        {
            var analysis = new FormationAnalysis
            {
                AnalysisId = analysisId,
                TotalClassesFormed = classes.Count,
                ClassDistribution = new Dictionary<string, int>(),
                ClassBoundaries = new List<(string, double)>(),
                AverageNumericalValue = 0.0,
                PrevalentClassType = "",
                AnalyzedDate = DateTime.Now
            };

            if (classes.Count == 0) return;

            foreach (var typedClass in classes.Values)
            {
                if (!analysis.ClassDistribution.ContainsKey(typedClass.ClassName))
                    analysis.ClassDistribution[typedClass.ClassName] = 0;
                analysis.ClassDistribution[typedClass.ClassName]++;
            }

            var values = classes.Values.Select(c => c.NumericalBasis).OrderBy(v => v).ToList();
            analysis.AverageNumericalValue = values.Average();

            foreach (var rule in rules.Values)
            {
                analysis.ClassBoundaries.Add((rule.ResultingClassName, rule.MaxThreshold));
            }

            if (analysis.ClassDistribution.Count > 0)
            {
                analysis.PrevalentClassType = analysis.ClassDistribution.OrderByDescending(x => x.Value).First().Key;
            }

            analyses[analysisId] = analysis;
        }

        public void DisplayRule(string ruleId)
        {
            if (!rules.ContainsKey(ruleId)) return;

            var rule = rules[ruleId];
            Console.WriteLine($"\n  Formation Rule: {rule.RuleName}");
            Console.WriteLine($"  Range: {rule.MinThreshold:F2} - {rule.MaxThreshold:F2}");
            Console.WriteLine($"  Resulting Class: {rule.ResultingClassName}");
            Console.WriteLine($"  Properties: {string.Join(", ", rule.ApplicableProperties)}");
        }

        public void DisplayFormedClass(string classId)
        {
            if (!classes.ContainsKey(classId)) return;

            var typedClass = classes[classId];
            Console.WriteLine($"\n  Formed Class: {typedClass.ClassName}");
            Console.WriteLine($"  Type: {typedClass.ClassType}");
            Console.WriteLine($"  Numerical Basis: {typedClass.NumericalBasis:F2}");
            Console.WriteLine($"  Properties:");
            foreach (var prop in typedClass.Properties)
            {
                Console.WriteLine($"    {prop.Key}: {prop.Value}");
            }
            Console.WriteLine($"  Characteristics: {string.Join(", ", typedClass.ComputedCharacteristics)}");
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Formation Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Total Classes Formed: {analysis.TotalClassesFormed}");
            Console.WriteLine($"  Average Numerical Value: {analysis.AverageNumericalValue:F2}");
            Console.WriteLine($"  Prevalent Class Type: {analysis.PrevalentClassType}");
            Console.WriteLine($"  Class Distribution: {string.Join(", ", analysis.ClassDistribution.Select(x => $"{x.Key}:{x.Value}"))}");
        }

        public int GetTotalRules()
        {
            return rules.Count;
        }

        public int GetTotalFormedClasses()
        {
            return classes.Count;
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Forming Typed Classes Based on Numerical Entry            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new DynamicClassFormationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Class Formation Rules]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterFormationRule("RULE-001", "Low Value Class",
            0, 25, "LowClass",
            new List<string> { "Level", "Status", "Priority" });

        engine.RegisterFormationRule("RULE-002", "Medium Value Class",
            25, 75, "MediumClass",
            new List<string> { "Level", "Score", "Status", "Intensity" });

        engine.RegisterFormationRule("RULE-003", "High Value Class",
            75, 100, "HighClass",
            new List<string> { "Level", "Score", "Status", "Priority", "Impact" });

        Console.WriteLine("  ✓ Registered 3 class formation rules");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Displaying Formation Rules]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayRule("RULE-001");
        engine.DisplayRule("RULE-002");
        engine.DisplayRule("RULE-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Registering Numeric Specifications]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterNumericSpecification("SPEC-001", 15.5, "Performance", new List<double> { 0, 100 });
        engine.RegisterNumericSpecification("SPEC-002", 52.3, "Quality", new List<double> { 0, 100 });
        engine.RegisterNumericSpecification("SPEC-003", 87.9, "Efficiency", new List<double> { 0, 100 });
        engine.RegisterNumericSpecification("SPEC-004", 43.2, "Reliability", new List<double> { 0, 100 });
        engine.RegisterNumericSpecification("SPEC-005", 91.5, "Effectiveness", new List<double> { 0, 100 });

        Console.WriteLine("  ✓ Registered 5 numeric specifications");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Forming Classes from Numeric Values]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.FormTypeFromNumeric("CLASS-001", "SPEC-001");
        engine.FormTypeFromNumeric("CLASS-002", "SPEC-002");
        engine.FormTypeFromNumeric("CLASS-003", "SPEC-003");
        engine.FormTypeFromNumeric("CLASS-004", "SPEC-004");
        engine.FormTypeFromNumeric("CLASS-005", "SPEC-005");

        Console.WriteLine("  ✓ Formed 5 typed classes from numeric input");
        engine.DisplayFormedClass("CLASS-001");
        engine.DisplayFormedClass("CLASS-003");
        engine.DisplayFormedClass("CLASS-005");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Analyzing Class Formation Patterns]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.AnalyzeFormations("ANALYSIS-001");

        Console.WriteLine("  ✓ Analyzed class formation patterns");
        engine.DisplayAnalysis("ANALYSIS-001");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Class Formation Summary:");
        Console.WriteLine($"    Registered Rules: {engine.GetTotalRules()}");
        Console.WriteLine($"    Formed Classes: {engine.GetTotalFormedClasses()}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Dynamic Class Formation Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Class Formation Architecture:");
        Console.WriteLine("    Layer 1: Rule Registration (define formation criteria)");
        Console.WriteLine("    Layer 2: Range Threshold Setting (establish boundaries)");
        Console.WriteLine("    Layer 3: Numeric Specification (input numeric values)");
        Console.WriteLine("    Layer 4: Rule Matching (find applicable rules)");
        Console.WriteLine("    Layer 5: Class Creation (instantiate typed class)");
        Console.WriteLine("    Layer 6: Property Generation (compute class properties)");
        Console.WriteLine("    Layer 7: Characteristic Derivation (determine attributes)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define class formation rules with numeric ranges");
        Console.WriteLine("    ✓ Register numeric specifications");
        Console.WriteLine("    ✓ Dynamically form typed classes from numeric input");
        Console.WriteLine("    ✓ Generate properties based on numerical values");
        Console.WriteLine("    ✓ Derive characteristics from numeric basis");
        Console.WriteLine("    ✓ Analyze class formation distributions");
        Console.WriteLine("    ✓ Support runtime class instantiation");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Dynamic class formation system complete");
        Console.ResetColor();
    }
}
