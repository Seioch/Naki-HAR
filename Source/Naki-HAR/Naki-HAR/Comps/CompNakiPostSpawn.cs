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
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            Pawn pawn = parent as Pawn;

            if (pawn == null)
            {
                // Only applies against pawns.
                return;
            }

            // All Naki must have DMNeed.
            // if (!pawn.story.traits.allTraits.Any(trait => trait.def == TraitDefOf.Bloodlust))
            if (pawn.needs.TryGetNeed(Naki_Defof.NakiRaceDMNeed) == null)
            {
            }
        }
    }
}
