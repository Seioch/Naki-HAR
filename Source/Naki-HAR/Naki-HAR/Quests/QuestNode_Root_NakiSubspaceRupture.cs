using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using RimWorld.QuestGen;


namespace Naki_HAR
{
    public class QuestNode_Root_NakiSubspaceRupture : QuestNode_Root_WandererJoin
    {
        public override Pawn GeneratePawn()
        {
            return ThingUtility.FindPawn(Naki_Defof.NakiSubspaceRupture.root.Generate());
        }

        public override void SendLetter(Quest quest, Pawn pawn)
        {
            // TODO Tagged Strings for Naki Subspace Rift
            TaggedString label = "Subspace Rupture"; // "LetterLabelRefugeePodCrash".Translate();
            TaggedString taggedString = "A subspace rupture event has occured nearby, dropping an injured Naki in the vicinity."; // "RefugeePodCrash".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            taggedString += "\n\n";
            if (pawn.Faction == null)
            {
                taggedString += "RefugeePodCrash_Factionless".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            }
            else if (pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                taggedString += "RefugeePodCrash_Hostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            }
            else
            {
                taggedString += "RefugeePodCrash_NonHostile".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
            }
            QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref taggedString);
            PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref taggedString, ref label, pawn);
            Find.LetterStack.ReceiveLetter(label, taggedString, LetterDefOf.NeutralEvent, new TargetInfo(pawn), null, null, null, null);
        }
    }
}
