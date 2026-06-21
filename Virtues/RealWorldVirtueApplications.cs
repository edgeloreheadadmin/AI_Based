using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues.Examples
{
    /// <summary>
    /// Real-world scenarios demonstrating how the virtues apply to daily life
    /// Each scenario shows multiple virtues working together in practical situations
    /// </summary>
    public class RealWorldVirtueApplications
    {
        // ===== SCENARIO 1: OVERCOMING PROFESSIONAL SETBACK =====
        public static void ScenarioProfessionalSetback()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        SCENARIO 1: Overcoming Professional Setback         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: Project failed. Team lost confidence.\n");

            Console.WriteLine("► FORTITUDE: Don't run from the pain of failure");
            system.GetFortitude().SitWithPain("Project Failure", "Understanding what went wrong");
            system.GetFortitude().ProtectInnerAltar();
            Console.WriteLine("   Action: Face the failure directly, protect your integrity\n");

            Console.WriteLine("► HONESTY: Acknowledge what went wrong");
            system.GetHonesty().DontLieToSelf("I made mistakes in planning");
            system.GetHonesty().AcknowledgeWrong("Poor Communication", "Led to misalignment");
            Console.WriteLine("   Action: Take responsibility without excuses\n");

            Console.WriteLine("► DILIGENCE: Commit to improvement");
            system.GetDiligence().ReinforceUnderstanding("Project Management", true);
            system.GetDiligence().Practice("Daily Planning");
            Console.WriteLine("   Action: Begin daily practice to improve\n");

            Console.WriteLine("► GRACE: Move forward with elegance");
            system.GetGrace().ForgiveWhoHurtYou("Yourself", "Making mistakes");
            system.GetGrace().ReclaimSoul();
            Console.WriteLine("   Action: Forgive yourself, reclaim confidence\n");

            Console.WriteLine("► PATIENCE: Wait for next opportunity");
            system.GetPatience().AllowMindToStop();
            system.GetPatience().UnderstandCompletion("Learning from failure", true);
            Console.WriteLine("   Action: Be patient with recovery process\n");

            Console.WriteLine($"Result: Harmony Score: {system.GetTotalHarmony()}");
            Console.WriteLine("Lesson: Failure + Honesty + Diligence = Growth\n");
        }

        // ===== SCENARIO 2: MAINTAINING COMMITMENT IN MARRIAGE =====
        public static void ScenarioMarriageCommitment()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      SCENARIO 2: Maintaining Commitment in Marriage       ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: 10 years married. Relationship hitting rough patch.\n");

            Console.WriteLine("► SACRED MATRIMONY: Choose commitment over feeling");
            system.GetMatrimony().MakeVows("Life Partner", "Through challenges");
            system.GetMatrimony().ExcludeDivorce();
            system.GetMatrimony().AcceptInSicknessAndHealth();
            Console.WriteLine("   Action: Recommit to vow despite difficulty\n");

            Console.WriteLine("► CHASTITY: Maintain fidelity through discipline");
            system.GetChastity().EstablishLaw("No Infidelity", 10000);
            system.GetChastity().ResistDesire("Easy escape");
            Console.WriteLine("   Action: Discipline prevents destructive choices\n");

            Console.WriteLine("► HONESTY: Have difficult conversations");
            system.GetHonesty().DontLieToSelf("I'm unhappy");
            system.GetHonesty().DontLieToOthers("I need to tell you how I feel");
            Console.WriteLine("   Action: Face truth together\n");

            Console.WriteLine("► PATIENCE: Work through the conflict slowly");
            system.GetPatience().SitInSilence(15);
            system.GetPatience().UnderstandCompletion("Healing", false);
            Console.WriteLine("   Action: Don't rush resolution\n");

            Console.WriteLine("► DILIGENCE: Practice daily connection");
            system.GetDiligence().Practice("Daily conversation");
            system.GetDiligence().Practice("Physical affection");
            system.GetDiligence().Practice("Shared goals");
            Console.WriteLine("   Action: Rebuild intimacy through consistency\n");

            Console.WriteLine("► GRACE & FORGIVENESS");
            system.GetGrace().ForgiveWhoHurtYou("Partner", "Past disappointments");
            Console.WriteLine("   Action: Let go of resentment\n");

            Console.WriteLine($"Result: Matrimony Score: {system.GetMatrimony().GetMatrimonyScore()}");
            Console.WriteLine("Lesson: Commitment + Honesty + Daily Practice = Lasting Love\n");
        }

        // ===== SCENARIO 3: SPIRITUAL AWAKENING & GROWTH =====
        public static void ScenarioSpiritualAwakening()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          SCENARIO 3: Spiritual Awakening & Growth        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: Beginning spiritual journey, seeking deeper meaning.\n");

            Console.WriteLine("► PRAYER & WORSHIP: Establish spiritual practice");
            system.GetPrayer().PrayForOthersWeaker("All suffering beings", "Peace");
            system.GetPrayer().SpeakToSilenceFirst();
            system.GetPrayer().ClearMind();
            system.GetPrayer().SitInSilence();
            Console.WriteLine("   Action: Daily meditation and prayer for others\n");

            Console.WriteLine("► DEVOTION: Align all four states");
            system.GetDevotion().AlignMental();
            system.GetDevotion().AlignPhysical();
            system.GetDevotion().AlignEmotional();
            system.GetDevotion().AlignSpiritual();
            Console.WriteLine("   Action: Integrate mind, body, emotion, spirit\n");

            Console.WriteLine("► DIVINITY: Understand interconnectedness");
            system.GetDivinity().UnderstandInterconnectedness();
            system.GetDivinity().ProtectNextGeneration("Spiritual wisdom", "Teaching others");
            system.GetDivinity().DevelopInnateWisdom("Unity", true);
            Console.WriteLine("   Action: See interconnectedness in all things\n");

            Console.WriteLine("► PATIENCE: Allow understanding to unfold");
            system.GetPatience().AllowMindToStop();
            system.GetPatience().SitInSilence(30);
            Console.WriteLine("   Action: Be patient with awakening process\n");

            Console.WriteLine("► CONSTITUTION OF WILL: Live the truth");
            system.GetWill().BecomeYourWill();
            system.GetWill().DoWhatNoOneElseWill("Live authentically");
            Console.WriteLine("   Action: Embody spiritual understanding\n");

            Console.WriteLine($"Result: Divinity Score: {system.GetDivinity().GetInnateWisdomScore()}");
            Console.WriteLine($"Result: Will Strength: {system.GetWill().GetWillStrength()}");
            Console.WriteLine("Lesson: Patience + Prayer + Alignment = Spiritual Growth\n");
        }

        // ===== SCENARIO 4: MAJOR LIFE DECISION & DEVIATION =====
        public static void ScenarioMajorDecision()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     SCENARIO 4: Major Life Decision & Intelligent Deviation  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: Considering major career change that breaks expectations.\n");

            Console.WriteLine("► HONESTY: Face your true desires");
            system.GetHonesty().DontLieToSelf("I'm unhappy in this career");
            system.GetHonesty().FaceTheTruth("Change will bring financial risk", true);
            Console.WriteLine("   Action: Acknowledge reality without denial\n");

            Console.WriteLine("► DEVIATION: Break rules intelligently");
            system.GetDeviation().BreakRuleIntelligently(
                "Follow family expectations",
                "Pursue authentic calling",
                "Life satisfaction and meaning"
            );
            system.GetDeviation().CalculateEquivalenceAndWorth("Stability", "Purpose");
            system.GetDeviation().RecognizeEvolution();
            Console.WriteLine("   Action: Break rule with wisdom and full understanding\n");

            Console.WriteLine("► FORTITUDE: Stand firm in your choice");
            system.GetFortitude().StrengthenFoundation();
            system.GetFortitude().ReinforceWithFaith();
            system.GetFortitude().ProtectInnerAltar();
            Console.WriteLine("   Action: Hold firm despite external pressure\n");

            Console.WriteLine("► DILIGENCE: Prepare thoroughly");
            system.GetDiligence().Practice("Learning new skills");
            system.GetDiligence().Practice("Building network");
            system.GetDiligence().ReinforceUnderstanding("New industry", true);
            Console.WriteLine("   Action: Practice discipline in preparation\n");

            Console.WriteLine("► PATIENCE: Allow transition time");
            system.GetPatience().UnderstandCompletion("Transition period", false);
            system.GetPatience().DontRush();
            Console.WriteLine("   Action: Give yourself time to adjust\n");

            Console.WriteLine($"Result: Deviation Intelligence: {system.GetDeviation().GetIntelligenceScore()}");
            Console.WriteLine($"Result: Diligence Wisdom: {system.GetDiligence().GetWisdomScore()}");
            Console.WriteLine("Lesson: Honesty + Intelligent Deviation + Preparation = Authentic Change\n");
        }

        // ===== SCENARIO 5: RECOVERY FROM ADDICTION OR HARMFUL PATTERN =====
        public static void ScenarioRecoveryFromPattern()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║    SCENARIO 5: Recovery from Harmful Pattern or Addiction  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: Overcoming destructive behavior pattern. Day 1.\n");

            Console.WriteLine("► CHASTITY: Establish unbreakable law");
            system.GetChastity().EstablishLaw("No returning to pattern", 90000);
            system.GetChastity().NeverChangeMind();
            Console.WriteLine("   Action: Make binding commitment to change\n");

            Console.WriteLine("► HONESTY: Face the full truth");
            system.GetHonesty().DontLieToSelf("This pattern is harming me");
            system.GetHonesty().AcknowledgeWrong("Self-deception", "Avoided reality");
            system.GetHonesty().RecognizeIllusion("Thinking I could control it");
            Console.WriteLine("   Action: Brutal honesty about impact\n");

            Console.WriteLine("► DILIGENCE: Build new daily practice");
            system.GetDiligence().Practice("Accountability call");
            system.GetDiligence().Practice("Exercise");
            system.GetDiligence().Practice("Meditation");
            Console.WriteLine("   Action: Replace pattern with new practices\n");

            Console.WriteLine("► GRACE: Forgive yourself & others");
            system.GetGrace().ForgiveWhoHurtYou("Yourself", "Years of struggle");
            system.GetGrace().ReclaimSoul();
            Console.WriteLine("   Action: Let go of shame\n");

            Console.WriteLine("► SABBATH: Rest and recovery");
            system.GetSabbath().StartSabbathDay();
            system.GetSabbath().ReinforceEnergyChemistry();
            system.GetSabbath().EndSabbathDay();
            Console.WriteLine("   Action: Prioritize rest for healing\n");

            Console.WriteLine("► PRAYER: Seek help beyond yourself");
            system.GetPrayer().PrayForOthersWeaker("Others in addiction", "Freedom");
            system.GetPrayer().PrayForGuidanceNotCrutch("Stay strong");
            Console.WriteLine("   Action: Connect to something greater\n");

            Console.WriteLine($"Result: Chastity Discipline: {system.GetChastity().GetDisciplineStrength()}");
            Console.WriteLine($"Result: Honesty Integrity: {system.GetHonesty().GetIntegrityScore()}");
            Console.WriteLine("Lesson: Honesty + Chastity + Daily Practice + Grace = Freedom\n");
        }

        // ===== SCENARIO 6: PARENTING & GENERATIONAL WISDOM =====
        public static void ScenarioParenting()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      SCENARIO 6: Parenting & Passing Generational Wisdom   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            Console.WriteLine("Context: Raising children, wanting to pass on virtue.\n");

            Console.WriteLine("► DILIGENCE: Model consistent practice");
            system.GetDiligence().Practice("Morning routine");
            system.GetDiligence().Practice("Reading");
            system.GetDiligence().PassToNextGeneration("Importance of consistency");
            Console.WriteLine("   Action: Live what you want them to learn\n");

            Console.WriteLine("► HONESTY: Teach truth-telling");
            system.GetHonesty().DontLieToSelf("Parenting is hard");
            system.GetHonesty().DontLieToOthers("I don't have all answers");
            system.GetHonesty().UnderstandPersonalities("My child's unique nature",
                new List<string> { "Sensitive", "Creative", "Sensitive" });
            Console.WriteLine("   Action: Be honest and authentic\n");

            Console.WriteLine("► DIVINITY: Protect and guide next generation");
            system.GetDivinity().ProtectNextGeneration("Resilience", "Through modeling strength");
            system.GetDivinity().ProtectNextGeneration("Compassion", "Through showing kindness");
            Console.WriteLine("   Action: Guard their wellbeing and growth\n");

            Console.WriteLine("► FORTITUDE: Model strength through adversity");
            system.GetFortitude().SitWithPain("Difficult moment", "Show them how to handle it");
            system.GetFortitude().StrengthenFoundation();
            Console.WriteLine("   Action: Show them how to face challenges\n");

            Console.WriteLine("► GRACE: Forgive mistakes in parenting");
            system.GetGrace().ForgiveWhoHurtYou("Yourself", "Parenting failures");
            Console.WriteLine("   Action: Model self-forgiveness\n");

            Console.WriteLine("► DEVOTION: Align your life");
            system.GetDevotion().AlignMental();
            system.GetDevotion().AlignPhysical();
            system.GetDevotion().AlignEmotional();
            system.GetDevotion().AlignSpiritual();
            Console.WriteLine("   Action: Be integrated so they see wholeness\n");

            Console.WriteLine($"Result: Generational Knowledge Passed: Diligence tracking");
            Console.WriteLine("Lesson: Modeling Virtue + Consistency + Honesty = Strong Next Generation\n");
        }

        // ===== MAIN ENTRY POINT =====
        public static void Main(string[] args)
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║      REAL-WORLD VIRTUE APPLICATIONS - 6 SCENARIOS        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝");

            ScenarioProfessionalSetback();
            ScenarioMarriageCommitment();
            ScenarioSpiritualAwakening();
            ScenarioMajorDecision();
            ScenarioRecoveryFromPattern();
            ScenarioParenting();

            Console.WriteLine("\n╔════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          KEY INSIGHT: Virtues Multiply in Power          ║");
            Console.WriteLine("║    When working together, they create exponential growth  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════╝\n");

            Console.WriteLine("The 13 virtues system works best when integrated:");
            Console.WriteLine("  • One virtue alone provides limited benefit");
            Console.WriteLine("  • Two virtues working together multiply effectiveness");
            Console.WriteLine("  • All 13 integrated create profound transformation");
            Console.WriteLine("\n\"The Flame Never Dies\" - Through Virtue, We Become Divine.\n");
        }
    }
}
