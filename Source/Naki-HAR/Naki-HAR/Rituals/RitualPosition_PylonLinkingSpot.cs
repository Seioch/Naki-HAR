using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    public class RitualPosition_PylonLinkingSpot : RitualPosition
    {
        // Once again, defining new CompPsylinkable behavior means I need to change what comp this thing is looking for
        public override PawnStagePosition GetCell(IntVec3 spot, Pawn p, LordJob_Ritual ritual)
        {
            IntVec3 intVec;
            if (SpectatorCellFinder.TryFindCircleSpectatorCellFor(p, CellRect.CenteredOn(spot, 0), 2f, 3f, p.Map, out intVec, null, null))
            {
                return new PawnStagePosition(intVec, null, Rot4.FromAngleFlat((spot - intVec).AngleFlat), this.highlight);
            }
            Thing thing = ritual.selectedTarget.Thing;
            CompNakiPsylinkable compPsylinkable = (thing != null) ? thing.TryGetComp<CompNakiPsylinkable>() : null;
            LocalTargetInfo localTargetInfo;
            if (compPsylinkable != null && compPsylinkable.TryFindLinkSpot(p, out localTargetInfo))
            {
                Rot4 orientation = Rot4.FromAngleFlat((spot - localTargetInfo.Cell).AngleFlat);
                return new PawnStagePosition(localTargetInfo.Cell, null, orientation, this.highlight);
            }
            return new PawnStagePosition(IntVec3.Invalid, null, Rot4.Invalid, this.highlight);
        }
    }
}