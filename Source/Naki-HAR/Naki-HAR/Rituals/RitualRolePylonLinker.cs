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
        // Checks to see if the pylon 
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
            if (!MeditationFocusDefOf.Natural.CanPawnUse(p) || p.GetPsylinkLevel() >= p.GetMaxPsylinkLevel())
            {
                if (!skipReason)
                {
                    reason = "RitualTargetAnimaTreeMustBeCapableOfNature".Translate();
                }
                return false;
            }
            if (!p.psychicEntropy.IsPsychicallySensitive)
            {
                if (!skipReason)
                {
                    reason = "RitualTargetAnimaTreeMustBePsychicallySensitive".Translate();
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
