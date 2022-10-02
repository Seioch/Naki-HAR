using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;

namespace Naki_HAR
{
    class JobGiver_PylonLinking : ThinkNode_JobGiver
    {
        // Very similar to how JobGiver_AnimaLinking works. It will find the Naki's CompNakiPsylinkable (not the normal one!) and then make a job to do it. 
        // Logs if the pawn cannot get the CompNakiPsylinkable
        protected override Job TryGiveJob(Pawn pawn)
        {
            PawnDuty duty = pawn.mindState.duty;
            if (duty == null)
            {
                return null;
            }
            if (!pawn.CanReserveAndReach(duty.focus, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false))
            {
                return null;
            }
            Thing thing = duty.focusSecond.Thing;
            CompNakiPsylinkable compNakiPsylinkable = (thing != null) ? thing.TryGetComp<CompNakiPsylinkable>() : null;
            if (compNakiPsylinkable == null || !compNakiPsylinkable.CanPsylink(pawn, null, true).Accepted)
            {
                Log.Error($"[Naki HAR] Thing {thing.def.defName} was unable to get comp compNakiPsylinkable when trying to get a job for psylink upgrade ritual.");
                return null;
            }
            // Creates a new job of def LinkNakiPsylinkable. This can be found in Jobs_Naki.xml
            // Note that the original JobDriver_LinkPsylinkable will always look for a CompPsylinkable, which is not going to work for our situation. 
            // So I needed to make a new JobDriver for this which is similar to JobDriver_LinkPsylinkable but finds a different CompPsylinkable
            return JobMaker.MakeJob(Naki_Defof.LinkNakiPsylinkable, duty.focusSecond, duty.focus);
        }
    }
}
