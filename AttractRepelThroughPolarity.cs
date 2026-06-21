using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AttractRepelThroughPolarity
{
    public class PolarityAxis
    {
        public string AxisName { get; set; }
        public double NegativePolarity { get; set; }
        public double PositivePolarity { get; set; }
        public double Intensity { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Entity
    {
        public string EntityName { get; set; }
        public Dictionary<string, double> PolaritySignature { get; set; }
        public double CoreFrequency { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class PolarityInteraction
    {
        public string Entity1 { get; set; }
        public string Entity2 { get; set; }
        public double AttractionForce { get; set; }
        public double RepulsionForce { get; set; }
        public string InteractionType { get; set; }
        public DateTime InteractionDate { get; set; }
    }

    public class PolarityEngine
    {
        private Dictionary<string, PolarityAxis> axes;
        private Dictionary<string, Entity> entities;
        private List<PolarityInteraction> interactions;
        private List<(string, string, double)> attractedPairs;
        private List<(string, string, double)> repelledPairs;

        public PolarityEngine()
        {
            axes = new Dictionary<string, PolarityAxis>();
            entities = new Dictionary<string, Entity>();
            interactions = new List<PolarityInteraction>();
            attractedPairs = new List<(string, string, double)>();
            repelledPairs = new List<(string, string, double)>();
        }

        public void CreatePolarityAxis(string axisName, double intensity = 1.0)
        {
            var axis = new PolarityAxis
            {
                AxisName = axisName,
                NegativePolarity = 0.0,
                PositivePolarity = 0.0,
                Intensity = intensity,
                CreatedDate = DateTime.Now
            };
            axes[axisName] = axis;
        }

        public void CreateEntity(string entityName, Dictionary<string, double> polaritySignature)
        {
            var entity = new Entity
            {
                EntityName = entityName,
                PolaritySignature = new Dictionary<string, double>(polaritySignature),
                CoreFrequency = CalculateCoreFrequency(polaritySignature),
                CreatedDate = DateTime.Now
            };
            entities[entityName] = entity;
        }

        private double CalculateCoreFrequency(Dictionary<string, double> signature)
        {
            if (signature.Count == 0) return 0.5;
            double positiveSum = signature.Values.Where(v => v > 0.5).Sum();
            double negativeSum = signature.Values.Where(v => v <= 0.5).Sum();
            return (0.5 + (positiveSum - negativeSum) / (signature.Count * 2.0));
        }

        public void SetEntityPolarity(string entityName, string axis, double polarity)
        {
            if (!entities.ContainsKey(entityName)) return;
            entities[entityName].PolaritySignature[axis] = Math.Max(0.0, Math.Min(1.0, polarity));
            entities[entityName].CoreFrequency = CalculateCoreFrequency(entities[entityName].PolaritySignature);
        }

        public double CalculatePolarityAlignment(string entity1, string entity2)
        {
            if (!entities.ContainsKey(entity1) || !entities.ContainsKey(entity2))
                return 0.0;

            var e1 = entities[entity1];
            var e2 = entities[entity2];

            double alignment = 0.0;
            int commonAxes = 0;

            foreach (var axis in e1.PolaritySignature.Keys)
            {
                if (e2.PolaritySignature.ContainsKey(axis))
                {
                    double diff = Math.Abs(e1.PolaritySignature[axis] - e2.PolaritySignature[axis]);
                    alignment += (1.0 - diff);
                    commonAxes++;
                }
            }

            return commonAxes > 0 ? alignment / commonAxes : 0.0;
        }

        public double CalculateAttractionForce(string entity1, string entity2)
        {
            if (!entities.ContainsKey(entity1) || !entities.ContainsKey(entity2))
                return 0.0;

            double alignment = CalculatePolarityAlignment(entity1, entity2);
            double frequencyDifference = Math.Abs(entities[entity1].CoreFrequency - entities[entity2].CoreFrequency);
            double frequencyHarmony = 1.0 - (frequencyDifference * 0.5);

            double attractionBase = alignment * frequencyHarmony;
            return Math.Max(0.0, attractionBase);
        }

        public double CalculateRepulsionForce(string entity1, string entity2)
        {
            if (!entities.ContainsKey(entity1) || !entities.ContainsKey(entity2))
                return 0.0;

            double alignment = CalculatePolarityAlignment(entity1, entity2);
            double repulsion = 1.0 - alignment;

            var e1 = entities[entity1];
            var e2 = entities[entity2];

            double poleMismatch = 0.0;
            int mismatchCount = 0;

            foreach (var axis in e1.PolaritySignature.Keys)
            {
                if (e2.PolaritySignature.ContainsKey(axis))
                {
                    double p1 = e1.PolaritySignature[axis];
                    double p2 = e2.PolaritySignature[axis];

                    if ((p1 > 0.6 && p2 < 0.4) || (p1 < 0.4 && p2 > 0.6))
                    {
                        poleMismatch += 1.0;
                    }
                    mismatchCount++;
                }
            }

            if (mismatchCount > 0)
                poleMismatch /= mismatchCount;

            return (repulsion * 0.7 + poleMismatch * 0.3);
        }

        public void InteractEntities(string entity1, string entity2)
        {
            double attraction = CalculateAttractionForce(entity1, entity2);
            double repulsion = CalculateRepulsionForce(entity1, entity2);

            string interactionType = attraction > repulsion ? "Attraction" : (repulsion > attraction ? "Repulsion" : "Neutral");

            var interaction = new PolarityInteraction
            {
                Entity1 = entity1,
                Entity2 = entity2,
                AttractionForce = attraction,
                RepulsionForce = repulsion,
                InteractionType = interactionType,
                InteractionDate = DateTime.Now
            };
            interactions.Add(interaction);

            if (attraction > 0.6)
                attractedPairs.Add((entity1, entity2, attraction));
            else if (repulsion > 0.6)
                repelledPairs.Add((entity1, entity2, repulsion));
        }

        public void DisplayEntityPolarity(string entityName)
        {
            if (!entities.ContainsKey(entityName)) return;

            var entity = entities[entityName];
            Console.WriteLine($"\n  Entity: {entity.EntityName}");
            Console.WriteLine($"  Core Frequency: {entity.CoreFrequency * 100:F1}%");
            Console.WriteLine($"  Polarity Signature:");
            foreach (var kvp in entity.PolaritySignature.OrderBy(x => x.Key))
            {
                string pole = kvp.Value > 0.5 ? "+" : "-";
                Console.WriteLine($"    {kvp.Key}: {kvp.Value * 100:F0}% {pole}");
            }
        }

        public void DisplayInteraction(string entity1, string entity2)
        {
            var interaction = interactions.FirstOrDefault(i =>
                (i.Entity1 == entity1 && i.Entity2 == entity2) ||
                (i.Entity1 == entity2 && i.Entity2 == entity1));

            if (interaction == null) return;

            Console.WriteLine($"\n  Interaction: {interaction.Entity1} ↔ {interaction.Entity2}");
            Console.WriteLine($"  Type: {interaction.InteractionType}");
            Console.WriteLine($"  Attraction Force: {interaction.AttractionForce * 100:F1}%");
            Console.WriteLine($"  Repulsion Force: {interaction.RepulsionForce * 100:F1}%");
        }

        public List<(string, string, double)> GetAttractionPairs()
        {
            return attractedPairs.OrderByDescending(x => x.Item3).ToList();
        }

        public List<(string, string, double)> GetRepulsionPairs()
        {
            return repelledPairs.OrderByDescending(x => x.Item3).ToList();
        }

        public Dictionary<string, List<string>> GetAttractionGroups()
        {
            var groups = new Dictionary<string, List<string>>();

            foreach (var (e1, e2, force) in attractedPairs)
            {
                if (!groups.ContainsKey(e1))
                    groups[e1] = new List<string>();
                if (!groups[e1].Contains(e2))
                    groups[e1].Add(e2);
            }

            return groups;
        }

        public void DisplayPolarityMatrix()
        {
            var entityNames = entities.Keys.OrderBy(x => x).ToList();
            Console.WriteLine("\n  Polarity Interaction Matrix:");
            Console.Write("    ");
            foreach (var name in entityNames)
            {
                Console.Write($"{name,12}");
            }
            Console.WriteLine();

            foreach (var e1 in entityNames)
            {
                Console.Write($"    {e1,12}");
                foreach (var e2 in entityNames)
                {
                    if (e1 == e2)
                    {
                        Console.Write($"{'*',12}");
                    }
                    else
                    {
                        double attraction = CalculateAttractionForce(e1, e2);
                        string symbol = attraction > 0.6 ? "⇄+" : (attraction < 0.4 ? "⇄-" : "⇄=");
                        Console.Write($"{symbol + attraction:F2}".PadRight(12));
                    }
                }
                Console.WriteLine();
            }
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Attract and Repel Through Polarity and Frequency            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new PolarityEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Polarity Axes]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreatePolarityAxis("Growth", 1.0);
        engine.CreatePolarityAxis("Stability", 1.0);
        engine.CreatePolarityAxis("Innovation", 1.0);
        engine.CreatePolarityAxis("Tradition", 1.0);
        engine.CreatePolarityAxis("Independence", 1.0);

        Console.WriteLine("  ✓ Created 5 polarity axes");
        Console.WriteLine("    Each axis represents a spectrum from negative (0) to positive (1)");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Creating Entities with Polarity Signatures]");
        Console.ResetColor();
        Thread.Sleep(500);

        var entities = new Dictionary<string, Dictionary<string, double>>
        {
            {
                "StartupCompany", new Dictionary<string, double>
                {
                    {"Growth", 0.95}, {"Stability", 0.2}, {"Innovation", 0.9}, {"Tradition", 0.1}, {"Independence", 0.85}
                }
            },
            {
                "EstablishedCorp", new Dictionary<string, double>
                {
                    {"Growth", 0.4}, {"Stability", 0.95}, {"Innovation", 0.3}, {"Tradition", 0.9}, {"Independence", 0.4}
                }
            },
            {
                "CreativeArtist", new Dictionary<string, double>
                {
                    {"Growth", 0.8}, {"Stability", 0.3}, {"Innovation", 0.95}, {"Tradition", 0.2}, {"Independence", 0.9}
                }
            },
            {
                "CommunityOrg", new Dictionary<string, double>
                {
                    {"Growth", 0.6}, {"Stability", 0.7}, {"Innovation", 0.55}, {"Tradition", 0.7}, {"Independence", 0.4}
                }
            },
            {
                "TechInnovator", new Dictionary<string, double>
                {
                    {"Growth", 0.85}, {"Stability", 0.45}, {"Innovation", 0.92}, {"Tradition", 0.15}, {"Independence", 0.88}
                }
            }
        };

        foreach (var kvp in entities)
        {
            engine.CreateEntity(kvp.Key, kvp.Value);
        }

        Console.WriteLine("  ✓ Created 5 entities with distinct polarity signatures");
        engine.DisplayEntityPolarity("StartupCompany");
        engine.DisplayEntityPolarity("EstablishedCorp");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Calculating Polarity Alignment]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Alignment Scores (0-100%, higher = more aligned):");
        var entityList = entities.Keys.ToList();
        for (int i = 0; i < entityList.Count - 1; i++)
        {
            for (int j = i + 1; j < entityList.Count; j++)
            {
                double alignment = engine.CalculatePolarityAlignment(entityList[i], entityList[j]);
                Console.WriteLine($"    {entityList[i]} ↔ {entityList[j]}: {alignment * 100:F1}%");
            }
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Simulating Attractions and Repulsions]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Calculating attraction/repulsion forces:");
        foreach (var e1 in entityList)
        {
            foreach (var e2 in entityList)
            {
                if (e1 != e2)
                {
                    engine.InteractEntities(e1, e2);
                }
            }
        }

        engine.DisplayInteraction("StartupCompany", "TechInnovator");
        engine.DisplayInteraction("StartupCompany", "EstablishedCorp");
        engine.DisplayInteraction("CreativeArtist", "TechInnovator");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Attraction Groups and Synergies]");
        Console.ResetColor();
        Thread.Sleep(500);

        var attractions = engine.GetAttractionPairs();
        Console.WriteLine("  Strongest Attractions (>60% alignment):");
        foreach (var (e1, e2, force) in attractions.Take(5))
        {
            Console.WriteLine($"    {e1} ⇄ {e2}: {force * 100:F1}% attraction force");
        }

        var repulsions = engine.GetRepulsionPairs();
        Console.WriteLine("\n  Strongest Repulsions (>60% misalignment):");
        foreach (var (e1, e2, force) in repulsions.Take(5))
        {
            Console.WriteLine($"    {e1} ⇄ {e2}: {force * 100:F1}% repulsion force");
        }
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Polarity Network Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.DisplayPolarityMatrix();
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Polarity-Based Attraction Model]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Polarity Mechanics:");
        Console.WriteLine("    • Each entity has a signature across multiple polarity axes");
        Console.WriteLine("    • Similar polarities → Attraction (resonance alignment)");
        Console.WriteLine("    • Opposing polarities → Repulsion (frequency discord)");
        Console.WriteLine("    • Mixed polarities → Neutral or complex interactions");
        Console.WriteLine("\n  Attraction Calculation:");
        Console.WriteLine("    Attraction = Alignment × Frequency Harmony");
        Console.WriteLine("    (How similar × How compatible frequencies)");
        Console.WriteLine("\n  Repulsion Calculation:");
        Console.WriteLine("    Repulsion = (1 - Alignment) × Pole Mismatch");
        Console.WriteLine("    (How different × Opposite pole emphasis)");
        Console.WriteLine("\n  Key Insights:");
        Console.WriteLine("    ✓ Like attracts like (similar polarity signatures)");
        Console.WriteLine("    ✓ Opposites repel (complementary polarities)");
        Console.WriteLine("    ✓ Frequency alignment determines resonance strength");
        Console.WriteLine("    ✓ Multiple polarity dimensions create complex dynamics");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Polarity attraction/repulsion system complete");
        Console.ResetColor();
    }
}
