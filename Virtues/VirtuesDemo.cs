using System;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// Demonstration of how all 13 virtues work together in a unified system
    /// </summary>
    public class VirtuesDemo
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                   THE 13 VIRTUES SYSTEM - DEMONSTRATION                ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝\n");

            VirtuesSystem system = new VirtuesSystem();

            // === FORTITUDE ===
            Console.WriteLine("▶ FORTITUDE: 'The Flame Never Dies'");
            system.GetFortitude().StrengthenFoundation();
            system.GetFortitude().ReinforceWithDiligence();
            system.GetFortitude().SitWithPain("Loss", "Understanding that pain forms wisdom");
            system.GetFortitude().ProtectInnerAltar();
            Console.WriteLine($"   {system.GetFortitude().GetState()}\n");

            // === CHASTITY ===
            Console.WriteLine("▶ CHASTITY: Self-Discipline & Restraint");
            system.GetChastity().EstablishLaw("No Deception", 365);
            system.GetChastity().ResistDesire("Shortcuts");
            system.GetChastity().HoldLaw("No Deception");
            system.GetChastity().ChannelDesireIntoConstraint("Creative Expression");
            Console.WriteLine($"   {system.GetChastity().GetState()}\n");

            // === DILIGENCE ===
            Console.WriteLine("▶ DILIGENCE: Time, Effort, Reinforcement & Growth");
            system.GetDiligence().Practice("Morning Meditation");
            system.GetDiligence().Practice("Physical Training");
            system.GetDiligence().ReinforceUnderstanding("Self-Knowledge", true);
            system.GetDiligence().AchieveCompletion("Daily Discipline");
            system.GetDiligence().PassToNextGeneration("The value of consistent practice");
            Console.WriteLine($"   {system.GetDiligence().GetState()}\n");

            // === GRACE ===
            Console.WriteLine("▶ GRACE: Elegance, Timing & Flow");
            system.GetGrace().RememberGrace();
            system.GetGrace().MoveWithFlow();
            system.GetGrace().AlignWithRhythm();
            system.GetGrace().ForgiveWhoHurtYou("Past Self", "Ignorance");
            system.GetGrace().ReclaimSoul();
            Console.WriteLine($"   {system.GetGrace().GetState()}\n");

            // === HONESTY ===
            Console.WriteLine("▶ HONESTY: Integrity & Truth");
            system.GetHonesty().DontLieToSelf("I am capable of growth");
            system.GetHonesty().AcknowledgeWrong("Pride", "Blocked my growth");
            system.GetHonesty().UnderstandAngle("From their perspective");
            system.GetHonesty().DetachFromBelief();
            Console.WriteLine($"   {system.GetHonesty().GetState()}\n");

            // === PATIENCE ===
            Console.WriteLine("▶ PATIENCE: Waiting, Timing & Understanding");
            system.GetPatience().SitInSilence(5);
            system.GetPatience().UnderstandCompletion("Personal Growth", false);
            system.GetPatience().AllowMindToStop();
            system.GetPatience().Breathe();
            Console.WriteLine($"   {system.GetPatience().GetState()}\n");

            // === DEVOTION ===
            Console.WriteLine("▶ DEVOTION: Integration of All Virtues");
            system.GetDevotion().AlignMental();
            system.GetDevotion().AlignPhysical();
            system.GetDevotion().AlignEmotional();
            system.GetDevotion().AlignSpiritual();
            system.GetDevotion().AchieveFourStateAlignment();
            system.GetDevotion().ActivateHealing();
            Console.WriteLine($"   {system.GetDevotion().GetState()}\n");

            // === PRAYER & WORSHIP ===
            Console.WriteLine("▶ PRAYER & WORSHIP: Offering & Intention");
            system.GetPrayer().PrayForOthersWeaker("The Suffering", "Relief and peace");
            system.GetPrayer().SpeakToSilenceFirst();
            system.GetPrayer().ClearMind();
            system.GetPrayer().CalmBrain();
            system.GetPrayer().ExpelSin();
            system.GetPrayer().SitInSilence();
            Console.WriteLine($"   {system.GetPrayer().GetState()}\n");

            // === DIVINITY ===
            Console.WriteLine("▶ DIVINITY: Interconnectedness & Inherited Wisdom");
            system.GetDivinity().ProtectNextGeneration("Compassion", "Through modeling");
            system.GetDivinity().UnderstandInterconnectedness();
            system.GetDivinity().ApplySelfEffort();
            system.GetDivinity().BuildFoundationalTruths();
            system.GetDivinity().AllowMiraclesNaturally();
            Console.WriteLine($"   {system.GetDivinity().GetState()}\n");

            // === CONSTITUTION OF WILL ===
            Console.WriteLine("▶ CONSTITUTION OF WILL: Inherited & Lived");
            system.GetWill().LightTorchForNextGeneration("Courage in adversity");
            system.GetWill().ChooseLoveOverFear();
            system.GetWill().BuildDiscipline();
            system.GetWill().DoWhatNoOneElseWill("Stand for the voiceless");
            system.GetWill().BecomeYourWill();
            Console.WriteLine($"   {system.GetWill().GetState()}\n");

            // === DEVIATION ===
            Console.WriteLine("▶ DEVIATION: Intelligent Evolution");
            system.GetDeviation().Recalibrate("Team dynamics changed");
            system.GetDeviation().RecognizeEvolution();
            system.GetDeviation().TestAlliance("Core Team");
            system.GetDeviation().AvoidBlindFaith();
            Console.WriteLine($"   {system.GetDeviation().GetState()}\n");

            // === SABBATH & REST ===
            Console.WriteLine("▶ SABBATH & REST: Balance & Restoration");
            system.GetSabbath().StartSabbathDay();
            system.GetSabbath().Rest(60);
            system.GetSabbath().RestoreBrainEnergy();
            system.GetSabbath().ReinforceEnergyChemistry();
            system.GetSabbath().EndSabbathDay();
            Console.WriteLine($"   {system.GetSabbath().GetState()}\n");

            // === SACRED MATRIMONY ===
            Console.WriteLine("▶ SACRED MATRIMONY: Lifelong Commitment");
            system.GetMatrimony().MakeVows("Beloved", "Till death do us part");
            system.GetMatrimony().ExcludeDivorce();
            system.GetMatrimony().DeclareLoveWithoutWords();
            system.GetMatrimony().ChooseResponsibility();
            system.GetMatrimony().AcceptInSicknessAndHealth();
            system.GetMatrimony().CommitTillDeath();
            Console.WriteLine($"   {system.GetMatrimony().GetState()}\n");

            // === FINAL HARMONY STATE ===
            Console.WriteLine("╔════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                        TOTAL SYSTEM HARMONY                          ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine($"   Harmony Score: {system.GetTotalHarmony()}/100");
            Console.WriteLine($"\n{system.GetComprehensiveState()}");
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║     'The Flame Never Dies' - Through Virtue, We Become Divine         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════════════╝");
        }
    }
}
