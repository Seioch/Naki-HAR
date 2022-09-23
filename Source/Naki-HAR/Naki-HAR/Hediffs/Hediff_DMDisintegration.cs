using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    public class Hediff_DMDisintegration : HediffWithComps
    {
        public int tickCounter = 0; // Counter between ticks
        public int applications = 0; // Number of times this thing has been applied
        
        public override void Tick()
        {
            // Naki cannot have DM Disintegration hediff
            if (pawn.IsNaki())
            {
                return;
            }
            base.Tick();
            tickCounter++;
            Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Naki_Defof.DMDisintegration);
            if (tickCounter > hediff.TryGetComp<CompDisintegration>().Props.tickMax)
            {
                if (hediff != null)
                {
                    tickCounter = 0;
                }
                else
                {
                    pawn.TakeDamage(new DamageInfo(Naki_Defof.DMBurn, 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    tickCounter = 0;
                    applications = applications + 1;
                    if (applications == hediff.TryGetComp<CompDisintegration>().Props.maxApplications)
                    {
                        // Remove the hediff
                        pawn.health.RemoveHediff(this);
                    }
                }
            }
        }

    }
}
