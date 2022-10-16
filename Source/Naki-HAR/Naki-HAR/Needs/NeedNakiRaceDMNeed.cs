using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Naki_HAR
{
    // NOTE this class may be deprecated cause I'm using Need_Chemical currently for DM Need
    public class Need_NakiRaceDMNeed : Need
    {
        public Need_NakiRaceDMNeed(Pawn pawn) : base(pawn)
        {
            threshPercents = Thresholds;
        }

        public override bool ShowOnNeedList => !Disabled;

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
            ThreshProblematic
        };

        public override float CurLevel
        {
            get => base.CurLevel;
            set => base.CurLevel = Math.Min(value, 1.0f);
        }

        private static float DecayPerDay { get; set; }
        public static int TickMultTimer => 10;

        public static float ThreshEmpty => 0.00f;
        public static float ThreshPeril => 0.20f;
        public static float ThreshProblematic => 0.60f;
        public bool PawnAffected => !pawn.IsNaki();

        public bool Peril
        {
            get
            {
                return this.CurLevel <= ThreshProblematic;
            }
        }

        public bool Problematic
        {
            get
            {
                return this.CurLevel <= ThreshPeril;
            }
        }

        private float GetFallPerTick()
        {
            return DecayPerDay / 60000.0f;
        }

        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f, bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null, bool drawLabel = true)
        {
            if (this.threshPercents == null)
            {
                this.threshPercents = new List<float>();
            }
            this.threshPercents.Clear();
            this.threshPercents.Add(ThreshPeril);
            this.threshPercents.Add(ThreshProblematic);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip);
        }

        // Executes drop in DM need every 10 ticks
        public override void NeedInterval()
        {
            if (Disabled || pawn.Map == null)
            {
                return;
            }

            if (!def.freezeWhileSleeping || pawn.Awake())
            {
                // 150 ticks per NeedInterval call. 
                CurLevel -= 150 * TickMultTimer * GetFallPerTick();
            }

            // Check to see if we should inflict the pawn with withdrawal symptoms
            if (CurLevel < ThreshProblematic)
            {
                
            }
        }
    }
}