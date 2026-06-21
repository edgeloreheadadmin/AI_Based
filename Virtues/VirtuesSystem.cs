using System;
using System.Collections.Generic;

namespace VirtueSystem.Virtues
{
    /// <summary>
    /// The Complete Virtues System - Integrating all 13 virtues into a unified framework
    ///
    /// 1. Fortitude - "The Flame Never Dies" - Foundation & Resolve
    /// 2. Chastity - Self-discipline & Restraint
    /// 3. Diligence - Time, Effort, Reinforcement & Growth
    /// 4. Grace - Elegance, Timing, Flow & Forgiveness
    /// 5. Honesty - Integrity, Truth, & Non-attachment to Belief
    /// 6. Patience - Waiting, Timing, & Understanding
    /// 7. Devotion - Integration of all Virtues
    /// 8. Prayer & Worship - Offering, Intention & Divinity
    /// 9. Divinity - Interconnectedness & Inherited Wisdom
    /// 10. Constitution of Will - Inherited Strength that becomes Action
    /// 11. Deviation - Intelligent Rule-Breaking & Evolution
    /// 12. Sabbath & Rest - Balance, Restoration & Recovery
    /// 13. Sacred Matrimony - Lifelong Commitment & Growth
    /// </summary>
    public class VirtuesSystem
    {
        private Fortitude _fortitude;
        private Chastity _chastity;
        private Diligence _diligence;
        private Grace _grace;
        private Honesty _honesty;
        private Patience _patience;
        private Devotion _devotion;
        private PrayerAndWorship _prayer;
        private Divinity _divinity;
        private ConstitutionOfWill _will;
        private Deviation _deviation;
        private SabbathAndRest _sabbath;
        private SacredMatrimony _matrimony;
        private int _totalHarmony = 0;

        public VirtuesSystem()
        {
            Initialize();
        }

        private void Initialize()
        {
            _fortitude = new Fortitude();
            _chastity = new Chastity();
            _diligence = new Diligence();
            _grace = new Grace();
            _honesty = new Honesty();
            _patience = new Patience();
            _devotion = new Devotion(_fortitude, _diligence, _patience);
            _prayer = new PrayerAndWorship();
            _divinity = new Divinity();
            _will = new ConstitutionOfWill();
            _deviation = new Deviation();
            _sabbath = new SabbathAndRest();
            _matrimony = new SacredMatrimony();
        }

        public Fortitude GetFortitude() => _fortitude;
        public Chastity GetChastity() => _chastity;
        public Diligence GetDiligence() => _diligence;
        public Grace GetGrace() => _grace;
        public Honesty GetHonesty() => _honesty;
        public Patience GetPatience() => _patience;
        public Devotion GetDevotion() => _devotion;
        public PrayerAndWorship GetPrayer() => _prayer;
        public Divinity GetDivinity() => _divinity;
        public ConstitutionOfWill GetWill() => _will;
        public Deviation GetDeviation() => _deviation;
        public SabbathAndRest GetSabbath() => _sabbath;
        public SacredMatrimony GetMatrimony() => _matrimony;

        public void CalculateHarmony()
        {
            _totalHarmony = 0;

            // Each virtue contributes to overall harmony
            _totalHarmony += _fortitude.GetState().GetHashCode() % 100;
            _totalHarmony += _chastity.GetDisciplineStrength();
            _totalHarmony += _diligence.GetWisdomScore();
            _totalHarmony += _grace.GetEleganceScore();
            _totalHarmony += _honesty.GetIntegrityScore();
            _totalHarmony += _patience.GetMindStillness();
            _totalHarmony += _devotion.GetDevotionalScore();
            _totalHarmony += (_prayer.CanWorshipNow() ? 50 : 0);
            _totalHarmony += _divinity.GetInterconnectednessScore();
            _totalHarmony += _will.GetWillStrength();
            _totalHarmony += _deviation.GetAdaptationScore();
            _totalHarmony += _sabbath.GetTotalRestScore();
            _totalHarmony += _matrimony.GetMatrimonyScore();
        }

        public int GetTotalHarmony()
        {
            CalculateHarmony();
            return _totalHarmony / 13; // Average across all virtues
        }

        public void PrintAllStates()
        {
            Console.WriteLine("=== VIRTUES SYSTEM STATE ===\n");
            Console.WriteLine($"1. Fortitude:\n   {_fortitude.GetState()}\n");
            Console.WriteLine($"2. Chastity:\n   {_chastity.GetState()}\n");
            Console.WriteLine($"3. Diligence:\n   {_diligence.GetState()}\n");
            Console.WriteLine($"4. Grace:\n   {_grace.GetState()}\n");
            Console.WriteLine($"5. Honesty:\n   {_honesty.GetState()}\n");
            Console.WriteLine($"6. Patience:\n   {_patience.GetState()}\n");
            Console.WriteLine($"7. Devotion:\n   {_devotion.GetState()}\n");
            Console.WriteLine($"8. Prayer & Worship:\n   {_prayer.GetState()}\n");
            Console.WriteLine($"9. Divinity:\n   {_divinity.GetState()}\n");
            Console.WriteLine($"10. Constitution of Will:\n   {_will.GetState()}\n");
            Console.WriteLine($"11. Deviation:\n   {_deviation.GetState()}\n");
            Console.WriteLine($"12. Sabbath & Rest:\n   {_sabbath.GetState()}\n");
            Console.WriteLine($"13. Sacred Matrimony:\n   {_matrimony.GetState()}\n");
            Console.WriteLine($"=== TOTAL HARMONY SCORE: {GetTotalHarmony()} ===");
        }

        public string GetComprehensiveState()
        {
            return $"Virtues System Harmony: {GetTotalHarmony()}\n" +
                   $"Fortitude Active: {_fortitude.GetState()}\n" +
                   $"Chastity Discipline: {_chastity.GetDisciplineStrength()}\n" +
                   $"Diligence Wisdom: {_diligence.GetWisdomScore()}\n" +
                   $"Grace Elegance: {_grace.GetEleganceScore()}\n" +
                   $"Honesty Integrity: {_honesty.GetIntegrityScore()}\n" +
                   $"Patience Stillness: {_patience.GetMindStillness()}\n" +
                   $"Devotion Score: {_devotion.GetDevotionalScore()}\n" +
                   $"Prayer Readiness: {_prayer.CanWorshipNow()}\n" +
                   $"Divinity Wisdom: {_divinity.GetInnateWisdomScore()}\n" +
                   $"Will Strength: {_will.GetWillStrength()}\n" +
                   $"Deviation Intelligence: {_deviation.GetIntelligenceScore()}\n" +
                   $"Rest Recovery: {_sabbath.GetTotalRestScore()}\n" +
                   $"Matrimony Bond: {_matrimony.GetMatrimonyScore()}";
        }
    }
}
