using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class AttachBeliefToObjects
{
    public class Object
    {
        public string ObjectName { get; set; }
        public string PhysicalDescription { get; set; }
        public double BaseMeaning { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Category { get; set; }
    }

    public class AttachedBelief
    {
        public string BeliefStatement { get; set; }
        public double BeliefStrength { get; set; }
        public List<string> Justifications { get; set; }
        public string BeliefType { get; set; }
        public int ReinforceCount { get; set; }
        public DateTime AttachedDate { get; set; }
    }

    public class MeaningfulObject
    {
        public Object BaseObject { get; set; }
        public List<AttachedBelief> Beliefs { get; set; }
        public double OverallMeaning { get; set; }
        public List<string> Descriptions { get; set; }
        public List<string> AssociatedMemories { get; set; }
        public double EmotionalResonance { get; set; }
        public DateTime LastModified { get; set; }
    }

    public class ObjectMeaningEngine
    {
        private Dictionary<string, MeaningfulObject> meaningfulObjects;
        private Dictionary<string, List<string>> objectCategories;
        private List<(string, string, double)> meaningTransfers;

        public ObjectMeaningEngine()
        {
            meaningfulObjects = new Dictionary<string, MeaningfulObject>();
            objectCategories = new Dictionary<string, List<string>>();
            meaningTransfers = new List<(string, string, double)>();
        }

        public void CreateObject(string objectName, string description, string category)
        {
            var baseObject = new Object
            {
                ObjectName = objectName,
                PhysicalDescription = description,
                BaseMeaning = 0.2,
                CreatedDate = DateTime.Now,
                Category = category
            };

            var meaningfulObject = new MeaningfulObject
            {
                BaseObject = baseObject,
                Beliefs = new List<AttachedBelief>(),
                OverallMeaning = 0.2,
                Descriptions = new List<string> { description },
                AssociatedMemories = new List<string>(),
                EmotionalResonance = 0.0,
                LastModified = DateTime.Now
            };

            meaningfulObjects[objectName] = meaningfulObject;

            if (!objectCategories.ContainsKey(category))
                objectCategories[category] = new List<string>();
            objectCategories[category].Add(objectName);
        }

        public void AttachBelief(string objectName, string beliefStatement, double strength, string beliefType)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var belief = new AttachedBelief
            {
                BeliefStatement = beliefStatement,
                BeliefStrength = Math.Min(strength, 1.0),
                Justifications = new List<string>(),
                BeliefType = beliefType,
                ReinforceCount = 0,
                AttachedDate = DateTime.Now
            };

            meaningfulObjects[objectName].Beliefs.Add(belief);
            UpdateObjectMeaning(objectName);
        }

        public void AddJustificationToBeliefs(string objectName, string justification)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            if (meaningObj.Beliefs.Count > 0)
            {
                meaningObj.Beliefs.Last().Justifications.Add(justification);
                meaningObj.Beliefs.Last().BeliefStrength =
                    Math.Min(meaningObj.Beliefs.Last().BeliefStrength + 0.1, 1.0);
            }
        }

        public void ReinforceObjectBelief(string objectName, int beliefIndex)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            if (beliefIndex >= 0 && beliefIndex < meaningObj.Beliefs.Count)
            {
                meaningObj.Beliefs[beliefIndex].ReinforceCount++;
                meaningObj.Beliefs[beliefIndex].BeliefStrength =
                    Math.Min(meaningObj.Beliefs[beliefIndex].BeliefStrength + 0.08, 0.99);
                UpdateObjectMeaning(objectName);
            }
        }

        public void AttachMemory(string objectName, string memory)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            meaningObj.AssociatedMemories.Add(memory);
            meaningObj.EmotionalResonance = Math.Min(meaningObj.EmotionalResonance + 0.15, 1.0);
            UpdateObjectMeaning(objectName);
        }

        private void UpdateObjectMeaning(string objectName)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];

            double beliefMeaning = meaningObj.Beliefs.Count > 0 ?
                meaningObj.Beliefs.Average(b => b.BeliefStrength) : 0.0;

            double memoryMeaning = meaningObj.AssociatedMemories.Count > 0 ?
                Math.Min(meaningObj.AssociatedMemories.Count * 0.15, 1.0) : 0.0;

            meaningObj.OverallMeaning = (meaningObj.BaseObject.BaseMeaning * 0.2) +
                                       (beliefMeaning * 0.4) +
                                       (memoryMeaning * 0.2) +
                                       (meaningObj.EmotionalResonance * 0.2);

            meaningObj.LastModified = DateTime.Now;
        }

        public void AddDescription(string objectName, string description)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            if (!meaningObj.Descriptions.Contains(description))
            {
                meaningObj.Descriptions.Add(description);
            }
        }

        public void TransferMeaning(string sourceObject, string targetObject)
        {
            if (!meaningfulObjects.ContainsKey(sourceObject) ||
                !meaningfulObjects.ContainsKey(targetObject)) return;

            double sourceMeaning = meaningfulObjects[sourceObject].OverallMeaning;

            foreach (var belief in meaningfulObjects[sourceObject].Beliefs)
            {
                AttachBelief(targetObject, belief.BeliefStatement,
                    belief.BeliefStrength * 0.8, belief.BeliefType);
            }

            meaningTransfers.Add((sourceObject, targetObject, sourceMeaning));
            UpdateObjectMeaning(targetObject);
        }

        public void DisplayObjectProfile(string objectName)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            Console.WriteLine($"\n  Object: {meaningObj.BaseObject.ObjectName}");
            Console.WriteLine($"  Category: {meaningObj.BaseObject.Category}");
            Console.WriteLine($"  Description: {meaningObj.BaseObject.PhysicalDescription}");
            Console.WriteLine($"  Overall Meaning: {meaningObj.OverallMeaning * 100:F1}%");
            Console.WriteLine($"  Emotional Resonance: {meaningObj.EmotionalResonance * 100:F1}%");
        }

        public void DisplayObjectBeliefs(string objectName)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            Console.WriteLine($"\n  Beliefs Attached to {objectName}:");

            if (meaningObj.Beliefs.Count == 0)
            {
                Console.WriteLine("    (No beliefs attached)");
                return;
            }

            int index = 1;
            foreach (var belief in meaningObj.Beliefs)
            {
                Console.WriteLine($"    Belief {index}: {belief.BeliefStatement}");
                Console.WriteLine($"      Type: {belief.BeliefType}");
                Console.WriteLine($"      Strength: {belief.BeliefStrength * 100:F1}%");
                Console.WriteLine($"      Reinforcements: {belief.ReinforceCount}");
                if (belief.Justifications.Count > 0)
                {
                    Console.WriteLine($"      Justifications:");
                    foreach (var just in belief.Justifications)
                    {
                        Console.WriteLine($"        • {just}");
                    }
                }
                index++;
            }
        }

        public void DisplayObjectDescriptions(string objectName)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            Console.WriteLine($"\n  Multiple Descriptions of {objectName}:");
            int index = 1;
            foreach (var desc in meaningObj.Descriptions)
            {
                Console.WriteLine($"    {index}. {desc}");
                index++;
            }
        }

        public void DisplayObjectMemories(string objectName)
        {
            if (!meaningfulObjects.ContainsKey(objectName)) return;

            var meaningObj = meaningfulObjects[objectName];
            Console.WriteLine($"\n  Associated Memories - {objectName}:");
            if (meaningObj.AssociatedMemories.Count == 0)
            {
                Console.WriteLine("    (No memories attached)");
                return;
            }

            foreach (var memory in meaningObj.AssociatedMemories)
            {
                Console.WriteLine($"    • {memory}");
            }
        }

        public Dictionary<string, double> GetMeaningRankings()
        {
            return meaningfulObjects.OrderByDescending(x => x.Value.OverallMeaning)
                .ToDictionary(x => x.Key, x => x.Value.OverallMeaning);
        }

        public int GetBeliefCount(string objectName)
        {
            return meaningfulObjects.ContainsKey(objectName) ?
                meaningfulObjects[objectName].Beliefs.Count : 0;
        }

        public List<string> GetObjectsInCategory(string category)
        {
            return objectCategories.ContainsKey(category) ? objectCategories[category] : new List<string>();
        }

        public string GetMeaningLevel(double meaning)
        {
            return meaning switch
            {
                >= 0.8 => "Sacred - Profound spiritual/emotional significance",
                >= 0.6 => "Highly Meaningful - Strong symbolic power",
                >= 0.4 => "Meaningful - Clear personal significance",
                >= 0.2 => "Moderately Meaningful - Some personal connection",
                _ => "Minimal Meaning - Low significance"
            };
        }
    }

    static void Main()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   Attach Beliefs to Objects and Give Meaning                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var engine = new ObjectMeaningEngine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 1: Creating Objects with Base Descriptions]");
        Console.ResetColor();
        Thread.Sleep(500);

        engine.CreateObject("AncientLocket", "Golden locket with intricate engravings, holds two small photographs", "Heirloom");
        engine.CreateObject("JournalBook", "Leather-bound notebook with handwritten entries spanning 5 years", "Personal");
        engine.CreateObject("MountainStone", "Smooth river stone collected from mountain peak, palm-sized", "Talisman");
        engine.CreateObject("FamilyQuilt", "Patchwork quilt made from grandmother's fabrics and loved ones' clothing", "Heritage");

        Console.WriteLine("  ✓ Created 4 objects with meaningful physical descriptions");
        engine.DisplayObjectProfile("AncientLocket");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 2: Attaching Beliefs to Objects]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Attaching beliefs to AncientLocket:");
        engine.AttachBelief("AncientLocket", "This locket carries the love of generations", 0.85, "Symbolic");
        engine.AttachBelief("AncientLocket", "Holding it connects me to my ancestors", 0.80, "Spiritual");
        engine.AttachBelief("AncientLocket", "It protects those who wear it with intention", 0.75, "Protective");

        Console.WriteLine("  Attaching beliefs to JournalBook:");
        engine.AttachBelief("JournalBook", "Writing in this journal clarifies my thoughts and truth", 0.88, "Empowering");
        engine.AttachBelief("JournalBook", "My words on these pages shape my reality", 0.82, "Manifestation");

        Console.WriteLine("  Attaching beliefs to MountainStone:");
        engine.AttachBelief("MountainStone", "This stone grounds me in strength and stability", 0.85, "Grounding");
        engine.AttachBelief("MountainStone", "Climbing mountains taught me resilience", 0.80, "Achievement");

        Console.WriteLine("\n  ✓ Attached multiple beliefs to objects");
        engine.DisplayObjectBeliefs("AncientLocket");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 3: Supporting Beliefs with Justifications]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Adding justifications to strengthen beliefs:");
        engine.AddJustificationToBeliefs("AncientLocket", "My grandmother wore it every day for 50 years");
        engine.AddJustificationToBeliefs("AncientLocket", "Family members report feeling protected when near it");
        engine.AddJustificationToBeliefs("AncientLocket", "Historical significance of the engravings");

        engine.AddJustificationToBeliefs("JournalBook", "Writing has helped me solve many problems");
        engine.AddJustificationToBeliefs("JournalBook", "Goals written down have manifested repeatedly");

        Console.WriteLine("  ✓ Strengthened belief convictions with supporting evidence");
        engine.DisplayObjectBeliefs("JournalBook");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 4: Attaching Meaningful Memories]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Associating significant memories with objects:");
        engine.AttachMemory("AncientLocket", "Receiving it on my 18th birthday from grandmother");
        engine.AttachMemory("AncientLocket", "Wearing it during my wedding ceremony");
        engine.AttachMemory("AncientLocket", "Finding it after grandmother passed away");

        engine.AttachMemory("JournalBook", "Starting it during a transformative year of my life");
        engine.AttachMemory("JournalBook", "Writing through my greatest challenges and breakthroughs");

        engine.AttachMemory("MountainStone", "Collecting it at the summit after difficult climb");
        engine.AttachMemory("MountainStone", "Touching it when facing difficult decisions");

        engine.AttachMemory("FamilyQuilt", "Working on it with my mother before she passed");
        engine.AttachMemory("FamilyQuilt", "Using it to comfort my child during illness");

        Console.WriteLine("\n  ✓ Memories amplify emotional resonance and meaning");
        engine.DisplayObjectMemories("AncientLocket");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 5: Multiple Descriptions and Perspectives]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Adding multiple ways to describe the same object:");
        engine.AddDescription("AncientLocket", "A vessel of ancestral love and protection");
        engine.AddDescription("AncientLocket", "A bridge between past and present generations");
        engine.AddDescription("AncientLocket", "A portable shrine to family connection");

        engine.AddDescription("JournalBook", "A mirror reflecting my evolving consciousness");
        engine.AddDescription("JournalBook", "A laboratory for experimenting with reality through words");
        engine.AddDescription("JournalBook", "A record of my authentic self-discovery");

        Console.WriteLine("\n  ✓ Multiple meanings coexist for the same object");
        engine.DisplayObjectDescriptions("AncientLocket");
        engine.DisplayObjectDescriptions("JournalBook");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 6: Reinforcement and Meaning Evolution]");
        Console.ResetColor();
        Thread.Sleep(500);

        Console.WriteLine("  Reinforcing beliefs through repeated engagement:");
        for (int i = 0; i < 5; i++)
        {
            engine.ReinforceObjectBelief("AncientLocket", 0);
            engine.ReinforceObjectBelief("JournalBook", 0);
            engine.ReinforceObjectBelief("MountainStone", 0);
        }

        engine.DisplayObjectProfile("AncientLocket");
        engine.DisplayObjectProfile("JournalBook");
        Thread.Sleep(800);

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n[SECTION 7: Object Meaning and Significance Analysis]");
        Console.ResetColor();
        Thread.Sleep(500);

        var rankings = engine.GetMeaningRankings();
        Console.WriteLine("  Object Meaning Rankings:");
        int rank = 1;
        foreach (var kvp in rankings)
        {
            string level = engine.GetMeaningLevel(kvp.Value);
            int beliefs = engine.GetBeliefCount(kvp.Key);
            Console.WriteLine($"  #{rank}: {kvp.Key}");
            Console.WriteLine($"       Meaning: {kvp.Value * 100:F1}%");
            Console.WriteLine($"       Beliefs: {beliefs} | Level: {level}");
            rank++;
        }

        Console.WriteLine("\n  Object Meaning Composition Model:");
        Console.WriteLine("    Base Meaning (20%): Physical properties and initial significance");
        Console.WriteLine("    Attached Beliefs (40%): Consciously assigned meaning and conviction");
        Console.WriteLine("    Associated Memories (20%): Emotional experiences linked to object");
        Console.WriteLine("    Emotional Resonance (20%): Cumulative feeling and connection");
        Console.WriteLine("\n  Key Principles:");
        Console.WriteLine("    ✓ Objects are blank canvases for meaning assignment");
        Console.WriteLine("    ✓ Beliefs give objects power and significance");
        Console.WriteLine("    ✓ Memories deepen emotional connection");
        Console.WriteLine("    ✓ Multiple descriptions reveal different aspects");
        Console.WriteLine("    ✓ Reinforcement strengthens belief and meaning");
        Console.WriteLine("    ✓ Objects become vessels of intention and identity");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✓ Object meaning attachment system complete");
        Console.ResetColor();
    }
}
