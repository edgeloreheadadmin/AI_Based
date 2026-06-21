using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class RecognizingMultipleFormsOfBeliefsAndTypes
{
    public class Belief
    {
        public string BeliefId { get; set; }
        public string BeliefStatement { get; set; }
        public string BeliefType { get; set; }
        public string BeliefCategory { get; set; }
        public double ConvictionStrength { get; set; }
        public List<string> SupportingEvidence { get; set; }
        public List<string> OriginSources { get; set; }
        public DateTime BeliefAdoptedDate { get; set; }
    }

    public class BeliefType
    {
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public string TypeDescription { get; set; }
        public List<string> CharacteristicFeatures { get; set; }
        public string VerifiabilityLevel { get; set; }
        public int BeliefsOfThisType { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BeliefCategory
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryDomain { get; set; }
        public List<string> BeliefIds { get; set; }
        public List<string> RelatedCategories { get; set; }
        public double CategoryCohesion { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class BeliefAnalysis
    {
        public string AnalysisId { get; set; }
        public string BeliefId { get; set; }
        public string ClassifiedType { get; set; }
        public string ClassifiedCategory { get; set; }
        public double TypeMatchConfidence { get; set; }
        public double CategoryMatchConfidence { get; set; }
        public List<string> RelatedBeliefs { get; set; }
        public string AnalysisConclusion { get; set; }
        public DateTime AnalyzedDate { get; set; }
    }

    public class BeliefClassificationEngine
    {
        private Dictionary<string, Belief> beliefs;
        private Dictionary<string, BeliefType> beliefTypes;
        private Dictionary<string, BeliefCategory> categories;
        private Dictionary<string, BeliefAnalysis> analyses;

        public BeliefClassificationEngine()
        {
            beliefs = new Dictionary<string, Belief>();
            beliefTypes = new Dictionary<string, BeliefType>();
            categories = new Dictionary<string, BeliefCategory>();
            analyses = new Dictionary<string, BeliefAnalysis>();
        }

        public void RegisterBeliefType(string typeId, string typeName, string description,
                                      List<string> features, string verifiability)
        {
            var beliefType = new BeliefType
            {
                TypeId = typeId,
                TypeName = typeName,
                TypeDescription = description,
                CharacteristicFeatures = new List<string>(features),
                VerifiabilityLevel = verifiability,
                BeliefsOfThisType = 0,
                CreatedDate = DateTime.Now
            };
            beliefTypes[typeId] = beliefType;
        }

        public void RegisterBeliefCategory(string categoryId, string categoryName, string domain)
        {
            var category = new BeliefCategory
            {
                CategoryId = categoryId,
                CategoryName = categoryName,
                CategoryDomain = domain,
                BeliefIds = new List<string>(),
                RelatedCategories = new List<string>(),
                CategoryCohesion = 0.0,
                CreatedDate = DateTime.Now
            };
            categories[categoryId] = category;
        }

        public void RegisterBelief(string beliefId, string statement, double conviction,
                                  List<string> evidence, List<string> sources)
        {
            var belief = new Belief
            {
                BeliefId = beliefId,
                BeliefStatement = statement,
                BeliefType = "",
                BeliefCategory = "",
                ConvictionStrength = conviction,
                SupportingEvidence = new List<string>(evidence),
                OriginSources = new List<string>(sources),
                BeliefAdoptedDate = DateTime.Now
            };
            beliefs[beliefId] = belief;
        }

        public void ClassifyBelief(string beliefId)
        {
            if (!beliefs.ContainsKey(beliefId)) return;

            var belief = beliefs[beliefId];
            string bestTypeId = "";
            string bestCategoryId = "";
            double bestTypeConfidence = 0.0;
            double bestCategoryConfidence = 0.0;

            foreach (var beliefType in beliefTypes.Values)
            {
                double typeConfidence = CalculateTypeMatch(belief, beliefType);
                if (typeConfidence > bestTypeConfidence)
                {
                    bestTypeConfidence = typeConfidence;
                    bestTypeId = beliefType.TypeId;
                }
            }

            foreach (var category in categories.Values)
            {
                double categoryConfidence = CalculateCategoryMatch(belief, category);
                if (categoryConfidence > bestCategoryConfidence)
                {
                    bestCategoryConfidence = categoryConfidence;
                    bestCategoryId = category.CategoryId;
                }
            }

            if (bestTypeConfidence > 0)
            {
                belief.BeliefType = bestTypeId;
                beliefTypes[bestTypeId].BeliefsOfThisType++;
            }

            if (bestCategoryConfidence > 0)
            {
                belief.BeliefCategory = bestCategoryId;
                categories[bestCategoryId].BeliefIds.Add(beliefId);
            }

            AnalyzeBelief(beliefId, bestTypeId, bestCategoryId, bestTypeConfidence, bestCategoryConfidence);
        }

        private double CalculateTypeMatch(Belief belief, BeliefType beliefType)
        {
            double score = 0.0;

            if (beliefType.TypeName == "Empirical" && belief.SupportingEvidence.Count > 0)
            {
                score = Math.Min((double)belief.SupportingEvidence.Count / 5, 1.0) * 0.9;
            }

            if (beliefType.TypeName == "Religious" && belief.OriginSources.Any(s => s.Contains("Faith")))
            {
                score = 0.95;
            }

            if (beliefType.TypeName == "Scientific" && belief.OriginSources.Any(s => s.Contains("Research")))
            {
                score = 0.95;
            }

            if (beliefType.TypeName == "Personal" && belief.OriginSources.Contains("Experience"))
            {
                score = 0.9;
            }

            if (beliefType.TypeName == "Cultural" && belief.OriginSources.Any(s => s.Contains("Tradition")))
            {
                score = 0.85;
            }

            return score;
        }

        private double CalculateCategoryMatch(Belief belief, BeliefCategory category)
        {
            double score = 0.5;

            if (belief.SupportingEvidence.Count > 0)
            {
                score += 0.2;
            }

            if (belief.ConvictionStrength > 0.7)
            {
                score += 0.15;
            }

            return Math.Min(score, 1.0);
        }

        private void AnalyzeBelief(string beliefId, string typeId, string categoryId,
                                  double typeConfidence, double categoryConfidence)
        {
            var belief = beliefs[beliefId];
            var analysis = new BeliefAnalysis
            {
                AnalysisId = $"Analysis-{beliefId}",
                BeliefId = beliefId,
                ClassifiedType = typeId,
                ClassifiedCategory = categoryId,
                TypeMatchConfidence = typeConfidence,
                CategoryMatchConfidence = categoryConfidence,
                RelatedBeliefs = new List<string>(),
                AnalysisConclusion = "",
                AnalyzedDate = DateTime.Now
            };

            foreach (var otherBelief in beliefs.Values)
            {
                if (otherBelief.BeliefId != beliefId && otherBelief.BeliefType == typeId)
                {
                    analysis.RelatedBeliefs.Add(otherBelief.BeliefId);
                }
            }

            analysis.AnalysisConclusion = GenerateConclusion(belief, typeConfidence, categoryConfidence);

            analyses[analysis.AnalysisId] = analysis;
        }

        private string GenerateConclusion(Belief belief, double typeConf, double catConf)
        {
            if (typeConf > 0.85 && belief.ConvictionStrength > 0.8)
                return "Well-defined belief with strong classification and high conviction";
            if (typeConf > 0.7)
                return "Moderately classified belief with reasonable confidence";
            return "Uncertain belief classification requiring further analysis";
        }

        public void LinkCategoryRelationship(string categoryId1, string categoryId2)
        {
            if (categories.ContainsKey(categoryId1) && categories.ContainsKey(categoryId2))
            {
                if (!categories[categoryId1].RelatedCategories.Contains(categoryId2))
                {
                    categories[categoryId1].RelatedCategories.Add(categoryId2);
                }
            }
        }

        public void DisplayBeliefType(string typeId)
        {
            if (!beliefTypes.ContainsKey(typeId)) return;

            var beliefType = beliefTypes[typeId];
            Console.WriteLine($"\n  Belief Type: {beliefType.TypeName}");
            Console.WriteLine($"  Description: {beliefType.TypeDescription}");
            Console.WriteLine($"  Features: {string.Join(", ", beliefType.CharacteristicFeatures)}");
            Console.WriteLine($"  Verifiability: {beliefType.VerifiabilityLevel}");
            Console.WriteLine($"  Beliefs of This Type: {beliefType.BeliefsOfThisType}");
        }

        public void DisplayBelief(string beliefId)
        {
            if (!beliefs.ContainsKey(beliefId)) return;

            var belief = beliefs[beliefId];
            Console.WriteLine($"\n  Belief: {belief.BeliefStatement}");
            Console.WriteLine($"  ID: {belief.BeliefId}");
            Console.WriteLine($"  Type: {belief.BeliefType}");
            Console.WriteLine($"  Category: {belief.BeliefCategory}");
            Console.WriteLine($"  Conviction Strength: {belief.ConvictionStrength * 100:F1}%");
            if (belief.SupportingEvidence.Count > 0)
            {
                Console.WriteLine($"  Evidence: {string.Join(", ", belief.SupportingEvidence)}");
            }
        }

        public void DisplayAnalysis(string analysisId)
        {
            if (!analyses.ContainsKey(analysisId)) return;

            var analysis = analyses[analysisId];
            Console.WriteLine($"\n  Belief Analysis: {analysis.AnalysisId}");
            Console.WriteLine($"  Type Match: {analysis.TypeMatchConfidence * 100:F1}%");
            Console.WriteLine($"  Category Match: {analysis.CategoryMatchConfidence * 100:F1}%");
            Console.WriteLine($"  Related Beliefs: {analysis.RelatedBeliefs.Count}");
            Console.WriteLine($"  Conclusion: {analysis.AnalysisConclusion}");
        }

        public int GetTotalBeliefs()
        {
            return beliefs.Count;
        }

        public int GetTotalBeliefTypes()
        {
            return beliefTypes.Count;
        }

        public int GetTotalCategories()
        {
            return categories.Count;
        }

        public List<(string, int)> GetBeliefTypeDistribution()
        {
            return beliefTypes.Values
                .Where(bt => bt.BeliefsOfThisType > 0)
                .Select(bt => (bt.TypeName, bt.BeliefsOfThisType))
                .OrderByDescending(x => x.Item2)
                .ToList();
        }

        public double GetAverageConvictionStrength()
        {
            return beliefs.Count > 0 ? beliefs.Values.Average(b => b.ConvictionStrength) : 0.0;
        }

        public List<string> GetHighConvictionBeliefs(double threshold)
        {
            return beliefs.Values
                .Where(b => b.ConvictionStrength >= threshold)
                .Select(b => b.BeliefId)
                .ToList();
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  Recognizing Multiple Forms of Beliefs and Their Types        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new BeliefClassificationEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Registering Belief Types]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterBeliefType("TYPE-001", "Empirical",
            "Beliefs based on observation and measurable evidence",
            new List<string> { "Evidence-based", "Testable", "Observable" },
            "Highly Verifiable");

        engine.RegisterBeliefType("TYPE-002", "Religious",
            "Beliefs rooted in faith and spiritual traditions",
            new List<string> { "Faith-based", "Transcendent", "Metaphysical" },
            "Subjectively Verifiable");

        engine.RegisterBeliefType("TYPE-003", "Scientific",
            "Beliefs derived from systematic research and experimentation",
            new List<string> { "Research-based", "Peer-reviewed", "Reproducible" },
            "Objectively Verifiable");

        engine.RegisterBeliefType("TYPE-004", "Personal",
            "Beliefs formed from individual experience and reflection",
            new List<string> { "Experience-based", "Individual", "Introspective" },
            "Subjectively Verifiable");

        engine.RegisterBeliefType("TYPE-005", "Cultural",
            "Beliefs shared within communities and traditions",
            new List<string> { "Tradition-based", "Collective", "Inherited" },
            "Socially Verifiable");

        Console.WriteLine("  ✓ Registered 5 belief types");
        engine.DisplayBeliefType("TYPE-001");
        engine.DisplayBeliefType("TYPE-003");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Registering Belief Categories]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterBeliefCategory("CAT-001", "Science & Nature", "Natural World");
        engine.RegisterBeliefCategory("CAT-002", "Religion & Spirituality", "Metaphysical");
        engine.RegisterBeliefCategory("CAT-003", "Society & Culture", "Human Interaction");
        engine.RegisterBeliefCategory("CAT-004", "Personal Values", "Individual");
        engine.RegisterBeliefCategory("CAT-005", "Morality & Ethics", "Behavioral");

        Console.WriteLine("  ✓ Registered 5 belief categories");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Linking Category Relationships]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.LinkCategoryRelationship("CAT-001", "CAT-005");
        engine.LinkCategoryRelationship("CAT-002", "CAT-005");
        engine.LinkCategoryRelationship("CAT-003", "CAT-004");
        engine.LinkCategoryRelationship("CAT-004", "CAT-005");

        Console.WriteLine("  ✓ Created 4 category relationships");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Registering Individual Beliefs]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.RegisterBelief("BELIEF-001", "The Earth orbits the Sun",
            0.98, new List<string> { "Astronomical observations", "Physics laws" },
            new List<string> { "Research", "Observation" });

        engine.RegisterBelief("BELIEF-002", "God exists and guides the universe",
            0.95, new List<string> { "Spiritual experience" },
            new List<string> { "Faith", "Religious texts" });

        engine.RegisterBelief("BELIEF-003", "Honesty is the best policy",
            0.85, new List<string> { "Personal experience", "Cultural wisdom" },
            new List<string> { "Tradition", "Experience" });

        engine.RegisterBelief("BELIEF-004", "Water freezes at 0 degrees Celsius",
            0.99, new List<string> { "Experimental verification", "Physics equations" },
            new List<string> { "Research", "Experimentation" });

        engine.RegisterBelief("BELIEF-005", "Family bonds are sacred",
            0.90, new List<string> { "Emotional connection", "Cultural tradition" },
            new List<string> { "Tradition", "Experience" });

        Console.WriteLine("  ✓ Registered 5 individual beliefs");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Classifying Beliefs]");
        Console.ResetColor();
        Thread.Sleep(500);

        for (int i = 1; i <= 5; i++)
        {
            engine.ClassifyBelief($"BELIEF-00{i}");
        }

        Console.WriteLine("  ✓ Classified all 5 beliefs into types and categories");
        engine.DisplayBelief("BELIEF-001");
        engine.DisplayBelief("BELIEF-002");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Analyzing Belief Distribution]");
        Console.ResetColor();
        Thread.Sleep(500);

        var typeDistribution = engine.GetBeliefTypeDistribution();
        Console.WriteLine("  Belief Type Distribution:");
        foreach (var (typeName, count) in typeDistribution)
        {
            Console.WriteLine($"    {typeName}: {count} belief(s)");
        }

        var highConviction = engine.GetHighConvictionBeliefs(0.9);
        Console.WriteLine($"\n  High Conviction Beliefs (>90%): {highConviction.Count}");
        Console.WriteLine($"  Average Conviction Strength: {engine.GetAverageConvictionStrength() * 100:F1}%");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: System Statistics]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine($"  Belief System Summary:");
        Console.WriteLine($"    Total Beliefs: {engine.GetTotalBeliefs()}");
        Console.WriteLine($"    Belief Types: {engine.GetTotalBeliefTypes()}");
        Console.WriteLine($"    Belief Categories: {engine.GetTotalCategories()}");
        Console.WriteLine($"    Types with Beliefs: {typeDistribution.Count}");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 8: Belief Classification Framework]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Belief Recognition Architecture:");
        Console.WriteLine("    Layer 1: Belief Type Definition (establish classification types)");
        Console.WriteLine("    Layer 2: Category Registration (define domain categories)");
        Console.WriteLine("    Layer 3: Belief Statement (formulate individual beliefs)");
        Console.WriteLine("    Layer 4: Evidence Collection (gather supporting information)");
        Console.WriteLine("    Layer 5: Source Attribution (identify belief origins)");
        Console.WriteLine("    Layer 6: Type Matching (classify belief type with confidence)");
        Console.WriteLine("    Layer 7: Category Assignment (place in categorical framework)");
        Console.WriteLine("    Layer 8: Conviction Analysis (measure belief strength and relationships)");
        Console.WriteLine("\n  Key Capabilities:");
        Console.WriteLine("    ✓ Recognize empirical, religious, scientific beliefs");
        Console.WriteLine("    ✓ Classify personal and cultural beliefs");
        Console.WriteLine("    ✓ Measure conviction strength and evidence support");
        Console.WriteLine("    ✓ Track belief origins and supporting evidence");
        Console.WriteLine("    ✓ Identify belief type distributions");
        Console.WriteLine("    ✓ Analyze cross-category belief relationships");
        Console.WriteLine("    ✓ Generate belief classification reports");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Belief classification and recognition system complete");
        Console.ResetColor();
    }
}
