using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class ClassifyingSomethingByMultipleAttributes
{
    public class Entity
    {
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string PrimaryClass { get; set; }
        public double ClassificationConfidence { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class AttributeDefinition
    {
        public string AttributeId { get; set; }
        public string AttributeName { get; set; }
        public string AttributeType { get; set; }
        public List<string> PossibleValues { get; set; }
        public double WeightInClassification { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassificationProfile
    {
        public string ProfileId { get; set; }
        public string ClassName { get; set; }
        public Dictionary<string, List<string>> AttributePatterns { get; set; }
        public int EntitiesInClass { get; set; }
        public double TypicalityScore { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class ClassificationResult
    {
        public string ResultId { get; set; }
        public string EntityId { get; set; }
        public Dictionary<string, double> ClassScores { get; set; }
        public string PredictedClass { get; set; }
        public double ConfidenceScore { get; set; }
        public List<string> MatchedAttributes { get; set; }
        public DateTime ClassifiedDate { get; set; }
    }

    public class MultiAttributeClassifier
    {
        private Dictionary<string, Entity> entities;
        private Dictionary<string, AttributeDefinition> attributes;
        private Dictionary<string, ClassificationProfile> profiles;
        private Dictionary<string, ClassificationResult> results;

        public MultiAttributeClassifier()
        {
            entities = new Dictionary<string, Entity>();
            attributes = new Dictionary<string, AttributeDefinition>();
            profiles = new Dictionary<string, ClassificationProfile>();
            results = new Dictionary<string, ClassificationResult>();
        }

        public void RegisterAttribute(string attributeId, string attributeName, string type,
                                     List<string> values, double weight)
        {
            var attribute = new AttributeDefinition
            {
                AttributeId = attributeId,
                AttributeName = attributeName,
                AttributeType = type,
                PossibleValues = new List<string>(values),
                WeightInClassification = weight,
                CreatedDate = DateTime.Now
            };
            attributes[attributeId] = attribute;
        }

        public void CreateClassificationProfile(string profileId, string className,
                                               Dictionary<string, List<string>> patterns)
        {
            var profile = new ClassificationProfile
            {
                ProfileId = profileId,
                ClassName = className,
                AttributePatterns = new Dictionary<string, List<string>>(patterns),
                EntitiesInClass = 0,
                TypicalityScore = 0.0,
                CreatedDate = DateTime.Now
            };
            profiles[profileId] = profile;
        }

        public void RegisterEntity(string entityId, string entityName, Dictionary<string, string> attrs)
        {
            var entity = new Entity
            {
                EntityId = entityId,
                EntityName = entityName,
                Attributes = new Dictionary<string, string>(attrs),
                PrimaryClass = "",
                ClassificationConfidence = 0.0,
                CreatedDate = DateTime.Now
            };
            entities[entityId] = entity;
        }

        public void ClassifyEntity(string entityId)
        {
            if (!entities.ContainsKey(entityId)) return;

            var entity = entities[entityId];
            var result = new ClassificationResult
            {
                ResultId = $"Result-{entityId}",
                EntityId = entityId,
                ClassScores = new Dictionary<string, double>(),
                PredictedClass = "",
                ConfidenceScore = 0.0,
                MatchedAttributes = new List<string>(),
                ClassifiedDate = DateTime.Now
            };

            foreach (var profile in profiles.Values)
            {
                double profileScore = 0.0;
                int matchedAttributeCount = 0;

                foreach (var patternKvp in profile.AttributePatterns)
                {
                    if (entity.Attributes.ContainsKey(patternKvp.Key))
                    {
                        string entityValue = entity.Attributes[patternKvp.Key];
                        if (patternKvp.Value.Contains(entityValue))
                        {
                            profileScore += 1.0;
                            matchedAttributeCount++;
                            result.MatchedAttributes.Add($"{patternKvp.Key}:{entityValue}");
                        }
                    }
                }

                if (profile.AttributePatterns.Count > 0)
                {
                    profileScore = profileScore / profile.AttributePatterns.Count;
                }

                result.ClassScores[profile.ClassName] = profileScore;

                if (profileScore > result.ConfidenceScore)
                {
                    result.ConfidenceScore = profileScore;
                    result.PredictedClass = profile.ClassName;
                }
            }

            entity.PrimaryClass = result.PredictedClass;
            entity.ClassificationConfidence = result.ConfidenceScore;

            results[result.ResultId] = result;

            if (!string.IsNullOrEmpty(result.PredictedClass))
            {
                var matchingProfile = profiles.Values.FirstOrDefault(p => p.ClassName == result.PredictedClass);
                if (matchingProfile != null)
                {
                    matchingProfile.EntitiesInClass++;
                }
            }
        }

        public void DisplayEntity(string entityId)
        {
            if (!entities.ContainsKey(entityId)) return;

            var entity = entities[entityId];
            Console.WriteLine($"\n  Entity: {entity.EntityName}");
            Console.WriteLine($"  ID: {entity.EntityId}");
            Console.WriteLine($"  Attributes:");
            foreach (var attr in entity.Attributes)
            {
                Console.WriteLine($"    {attr.Key}: {attr.Value}");
            }
            if (!string.IsNullOrEmpty(entity.PrimaryClass))
            {
                Console.WriteLine($"  Classification: {entity.PrimaryClass} ({entity.ClassificationConfidence * 100:F1}%)");
            }
        }

        public void DisplayProfile(string profileId)
        {
            if (!profiles.ContainsKey(profileId)) return;

            var profile = profiles[profileId];
            Console.WriteLine($"\n  Classification Profile: {profile.ClassName}");
            Console.WriteLine($"  Entities in Class: {profile.EntitiesInClass}");
            Console.WriteLine($"  Attribute Patterns:");
            foreach (var pattern in profile.AttributePatterns)
            {
                Console.WriteLine($"    {pattern.Key}: {string.Join(", ", pattern.Value)}");
            }
        }

        public void DisplayResult(string resultId)
        {
            if (!results.ContainsKey(resultId)) return;

            var result = results[resultId];
            Console.WriteLine($"\n  Classification Result: {result.ResultId}");
            Console.WriteLine($"  Predicted Class: {result.PredictedClass}");
            Console.WriteLine($"  Confidence: {result.ConfidenceScore * 100:F1}%");
            Console.WriteLine($"  Matched Attributes: {result.MatchedAttributes.Count}");
        }

        public int GetTotalEntities()
        {
            return entities.Count;
        }

        public int GetTotalProfiles()
        {
            return profiles.Count;
        }

        public List<(string, int)> GetClassDistribution()
        {
            return profiles.Values
                .Select(p => (p.ClassName, p.EntitiesInClass))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Classifying by Name, Effect, Descriptions, Types and More    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var classifier = new MultiAttributeClassifier();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Defining Classification Attributes]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.RegisterAttribute("ATTR-001", "Color", "Visual",
            new List<string> { "Red", "Blue", "Green", "Yellow" }, 0.25);

        classifier.RegisterAttribute("ATTR-002", "Size", "Dimension",
            new List<string> { "Small", "Medium", "Large" }, 0.20);

        classifier.RegisterAttribute("ATTR-003", "Material", "Composition",
            new List<string> { "Metal", "Plastic", "Wood", "Glass" }, 0.30);

        classifier.RegisterAttribute("ATTR-004", "Purpose", "Function",
            new List<string> { "Utility", "Decorative", "Structural" }, 0.25);

        Console.WriteLine("  ✓ Registered 4 attributes");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Classification Profiles]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.CreateClassificationProfile("PROF-001", "Tool",
            new Dictionary<string, List<string>> {
                { "ATTR-001", new List<string> { "Red", "Blue" } },
                { "ATTR-002", new List<string> { "Medium", "Large" } },
                { "ATTR-003", new List<string> { "Metal" } },
                { "ATTR-004", new List<string> { "Utility" } }
            });

        classifier.CreateClassificationProfile("PROF-002", "Decoration",
            new Dictionary<string, List<string>> {
                { "ATTR-001", new List<string> { "Red", "Green", "Yellow" } },
                { "ATTR-002", new List<string> { "Small", "Medium" } },
                { "ATTR-003", new List<string> { "Glass", "Metal" } },
                { "ATTR-004", new List<string> { "Decorative" } }
            });

        classifier.CreateClassificationProfile("PROF-003", "Structure",
            new Dictionary<string, List<string>> {
                { "ATTR-001", new List<string> { "Blue", "Green" } },
                { "ATTR-002", new List<string> { "Large" } },
                { "ATTR-003", new List<string> { "Wood", "Metal" } },
                { "ATTR-004", new List<string> { "Structural" } }
            });

        Console.WriteLine("  ✓ Created 3 classification profiles");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Registering Entities]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.RegisterEntity("ENT-001", "Hammer",
            new Dictionary<string, string> {
                { "ATTR-001", "Red" },
                { "ATTR-002", "Medium" },
                { "ATTR-003", "Metal" },
                { "ATTR-004", "Utility" }
            });

        classifier.RegisterEntity("ENT-002", "Vase",
            new Dictionary<string, string> {
                { "ATTR-001", "Yellow" },
                { "ATTR-002", "Small" },
                { "ATTR-003", "Glass" },
                { "ATTR-004", "Decorative" }
            });

        classifier.RegisterEntity("ENT-003", "Bridge",
            new Dictionary<string, string> {
                { "ATTR-001", "Blue" },
                { "ATTR-002", "Large" },
                { "ATTR-003", "Metal" },
                { "ATTR-004", "Structural" }
            });

        Console.WriteLine("  ✓ Registered 3 entities");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Classifying Entities]");
        Console.ResetColor();
        Thread.Sleep(500);

        classifier.ClassifyEntity("ENT-001");
        classifier.ClassifyEntity("ENT-002");
        classifier.ClassifyEntity("ENT-003");

        Console.WriteLine("  ✓ Classified all entities");
        classifier.DisplayEntity("ENT-001");
        classifier.DisplayEntity("ENT-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Classification Distribution]");
        Console.ResetColor();
        Thread.Sleep(500);

        var distribution = classifier.GetClassDistribution();
        Console.WriteLine("  Class Distribution:");
        foreach (var (className, count) in distribution)
        {
            Console.WriteLine($"    {className}: {count} entities");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Multi-Attribute Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Classification Architecture:");
        Console.WriteLine("    Layer 1: Attribute Definition (establish features)");
        Console.WriteLine("    Layer 2: Weight Assignment (prioritize attributes)");
        Console.WriteLine("    Layer 3: Profile Creation (define class patterns)");
        Console.WriteLine("    Layer 4: Entity Registration (input item properties)");
        Console.WriteLine("    Layer 5: Pattern Matching (compare to profiles)");
        Console.WriteLine("    Layer 6: Confidence Calculation (measure certainty)");
        Console.WriteLine("    Layer 7: Class Assignment (determine best match)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Define multiple classification attributes");
        Console.WriteLine("    ✓ Create weighted attribute patterns");
        Console.WriteLine("    ✓ Match entities to classification profiles");
        Console.WriteLine("    ✓ Calculate confidence scores");
        Console.WriteLine("    ✓ Track class distributions");
        Console.WriteLine("    ✓ Support multi-dimensional classification");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Multi-attribute classification system complete");
        Console.ResetColor();
    }
}
