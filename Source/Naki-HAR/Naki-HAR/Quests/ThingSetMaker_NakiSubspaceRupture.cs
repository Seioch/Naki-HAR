using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    // Similar to ThingSetMaker_RefugeePod but specifically spawns in a Naki pawnkind
    public class ThingSetMaker_NakiSubspaceRupture : ThingSetMaker
    {
        protected override void Generate(ThingSetMakerParams parms, List<Thing> outThings)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Naki_Defof.Naki_Citizen, DownedNakiQuestUtility.GetNakiFaction(), PawnGenerationContext.NonPlayer,
                -1, false, false, false, true, false, 1f, false, true, false, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, false, false, false, false, null, null, null, null, null, 0f, DevelopmentalStage.Adult, null, null, null, false));
            outThings.Add(pawn);
            HealthUtility.DamageUntilDowned(pawn, true);
        }

        protected override IEnumerable<ThingDef> AllGeneratableThingsDebugSub(ThingSetMakerParams parms)
        {
            yield return Naki_Defof.Naki_Citizen.race;
            yield break;
        }
    }
}
