using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompDisintegration : HediffComp
    {      
        public CompProperties_DMDisintegration Props
        {
            get
            {
                return (CompProperties_DMDisintegration)this.props;
            }
        }

        public int tickCounter = 0; // Counter between ticks
        public int applications = 0; // Number of times this thing has been applied
        public float severityAdjustment = 0;

        public override void CompPostTick(ref float SeverityAdjustment)
        {
            // Naki cannot have DM Disintegration hediff
            if (Pawn.IsNaki())
            {
                this.parent.pawn.health.RemoveHediff(this.parent);
                return;
            }
            tickCounter++;
            bool activation = tickCounter > this.Props.tickMax;
            if (activation)
            {
                // Log.Message("Adding DM burn");
                // Make the ash spawn from disintegration
                Map map = this.parent.pawn.Map;
                foreach (IntVec3 c in this.parent.pawn.OccupiedRect())
                {
                    FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Ash, 1, FilthSourceFlags.None);
                }
                this.parent.pawn.TakeDamage(new DamageInfo(Naki_Defof.DMBurn, 1f, 1f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                tickCounter = 0;
                applications = applications + 1;
                if (applications == this.Props.maxApplications) // Maximum number of applications inflicted has been reached
                {
                    // Remove the hediff
                    this.parent.pawn.health.RemoveHediff(this.parent);
                }
            }
        }

    }
}
