using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    class RitualRolePylonLinker : RitualRole
    {
        // Checks to see if the pawn can link to the pylon
        // TODO: Tagged strings to be added here!
        public override bool AppliesToPawn(Pawn p, out string reason, LordJob_Ritual ritual = null, RitualRoleAssignments assignments = null, Precept_Ritual precept = null, bool skipReason = false)
        {
            reason = null;
            if (ritual != null)
            {
                if (p == ritual.Organizer)
                {
                    return true;
                }
            }
            else if (assignments != null && assignments.Required(p))
            {
                return true;
            }
            // if (!MeditationFocusDefOf.Natural.CanPawnUse(p) || p.GetPsylinkLevel() >= p.GetMaxPsylinkLevel())
            if (!Naki_Defof.NakiFocus.CanPawnUse(p) || p.GetPsylinkLevel() >= p.GetMaxPsylinkLevel())
            {
                if (!skipReason)
                {
                    reason = "Must be a colonist with the Naki meditation type who is below maximum psylink level.";
                }
                return false;
            }
            if (!p.psychicEntropy.IsPsychicallySensitive)
            {
                if (!skipReason)
                {
                    reason = "This pawn is not psychically sensitive";
                }
                return false;
            }
            return true;
        }

        // Token: 0x06006100 RID: 24832 RVA: 0x00218287 File Offset: 0x00216487
        public override bool AppliesToRole(Precept_Role role, out string reason, Precept_Ritual ritual = null, Pawn p = null, bool skipReason = false)
        {
            reason = null;
            return false;
        }
    }
}
