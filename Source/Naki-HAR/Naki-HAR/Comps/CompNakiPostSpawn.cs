using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompNakiPostSpawn : ThingComp
    {
        // This code does nothing now, but if I want to give Naki something after they spawn in this is how we can do it. 
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Pawn pawn = parent as Pawn;

            if (pawn == null)
            {
                // Only applies against pawns.
                return;
            }

            // If the pawn is a Naki, start them with the DM Addiction
            if (pawn.IsNaki())
            {
                Log.Message($"[Naki HAR] Adding DarkMatterAddiction to {pawn.Name}");
                HediffDef dmAddiction = HediffDef.Named("DarkMatterAddiction");
                dmAddiction.initialSeverity = 1.0f;
                pawn.health.AddHediff(dmAddiction);

                // Also add their first level of Psylink
                Log.Message($"[Naki HAR] Adding Naki Psylink to {pawn.Name}");
                // Tried this but it called give ability twice
                // pawn.ChangePsylinkLevel(1, true); 

                //HediffDef psylink = HediffDef.Named("PsychicAmplifier");
                Hediff_Psylink psylink = (Hediff_Psylink)HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn, null);
                dmAddiction.initialSeverity = 1.0f;
                psylink.suppressPostAddLetter = true;
                pawn.health.AddHediff(psylink, pawn.health.hediffSet.GetBrain(), null, null);
            }
        }
    }
}
