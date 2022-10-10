using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Naki_HAR
{
    // Very similar to DownRefugeeQuestUtility, but sets a specific PawnKind and Faction
    // Pawnkind: Naki_Citizen
    public static class DownedNakiQuestUtility
    {
        public static Pawn GenerateInjuredNaki(int tile)
        {
            Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(Naki_Defof.Naki_Citizen, DownedNakiQuestUtility.GetNakiFaction(), PawnGenerationContext.NonPlayer, tile, 
                false, false, false, false, true, false, 20f, true, true, true, true, false, false, false, false, 0f, 0f, null, 1f, null, null, null, null, new float?(0.2f), null, null, null, null, null, null, null, null, false, false, false));
            HealthUtility.DamageUntilDowned(pawn, false);
            HealthUtility.DamageLegsUntilIncapableOfMoving(pawn, false);
            return pawn;
        }

        public static Faction GetNakiFaction()
        {
            foreach (Faction f in Find.FactionManager.AllFactionsListForReading)
            {
                if (f.def.defName.Equals("Naki_HiddenFaction"))
                {
                    return f;
                }
            }
            return null;
        }
    }
}
