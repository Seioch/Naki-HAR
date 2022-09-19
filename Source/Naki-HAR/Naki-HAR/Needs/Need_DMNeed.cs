using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    public class Need_NakiRaceDMNeed : Need
    {
        public Need_NakiRaceDMNeed(Pawn pawn) : base(pawn)
        {
            threshPercents = Thresholds;
        }
        public static bool Enabled { get; set; }
        // Only disabled on those who are not Naki
        private bool Disabled
        {
            get
            {
                return (!pawn.IsNaki());
            }
        }
        public static float DaysToEmpty
        {
            get => _daysToEmpty;
            set
            {
                _daysToEmpty = value;
                DecayPerDay = 1.0f / DaysToEmpty;
            }
        }
        private static float _daysToEmpty;


        public static readonly List<float> Thresholds = new List<float>()
        {
            ThreshEmpty,
            ThreshPeril,
            ThreshAgitated,
            ThreshNeedy,
            ThreshSatisfied,
        };

        public override float CurLevel
        {
            get => base.CurLevel;
            set => base.CurLevel = Math.Min(value, 1.0f);
        }

        private static float DecayPerDay { get; set; }
        public static int TickMultTimer => 10;

        public static float ThreshEmpty => 0.00f;
        public static float ThreshPeril => 0.10f;
        public static float ThreshAgitated => 0.30f;
        public static float ThreshNeedy => 0.50f;
        public static float ThreshSatisfied => 0.70f;
        public bool PawnAffected => !pawn.IsNaki();

        private float GetFallPerTick()
        {
            return DecayPerDay / 60000.0f;
        }
        private int curTick = 0;

        // Executes drop in DM need every 10 ticks
        public override void NeedInterval()
        {
            if (!Enabled || pawn.Map == null)
            {
                return;
            }

            curTick++;

            if (curTick >= 10)
            {
                curTick = 0;

                if (!PawnAffected) // Only affect Naki TODO see if this is too strong for a starting decay.
                {
                    CurLevel = ThreshNeedy;
                    return;
                }

                if (!def.freezeWhileSleeping || pawn.Awake())
                {
                    // 150 ticks per NeedInterval call. 
                    CurLevel -= 150 * TickMultTimer * GetFallPerTick();
                }
            }
        }
    }
}