﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;

namespace Naki_HAR
{
    // This class is orphaned now, but I will save it here for postierity
    // Why is it orphaned? That's because the Hediffs for disintegration now rely on CompDMDisintegration to do damage over time. 
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
                    pawn.TakeDamage(new DamageInfo(Naki_Defof.DMBurn, 8f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    // sizzle
                    Naki_Defof.Naki_DM_Sizzle.PlayOneShot(SoundInfo.InMap((TargetInfo)(Thing)(pawn)));
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
