using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Naki_HAR
{
    // Very similar to the original RitualBehaviorWorker_AnimaLinking, however the tagged string and which RitualOutcomeEffectWorker is now set
    // to be the one that I created for Naki. Yes, RW did hardocde this entire thing to look only at the AnimaTree stuff
    class RitualBehaviorWorker_PylonLinking : RitualBehaviorWorker
    {
        // Empty constructor
        public RitualBehaviorWorker_PylonLinking()
        {
        }

        // Empty constructor
        public RitualBehaviorWorker_PylonLinking(RitualBehaviorDef def) : base(def)
        {
        }

        // provides the explanation in the ritual window to what happens with a linking ritual

        public override string GetExplanation(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
        {
            // int count = assignments.SpectatorsForReading.Count;
            // float value = RitualOutcomeEffectWorker_AnimaTreeLinking.RestoredGrassFromQuality.Evaluate(quality);
            // TaggedString taggedString = "AnimaLinkingExplanationBase".Translate(count, value);
            // TODO: Change this later to use the TaggedString system to support other languages
            String pylonLinkExplanation = "The linking ritual doesn't need many spectators to complete, however it is a sight to behold.";
            if (assignments.ExtraRequiredPawnsForReading.Any<Pawn>())
            {
                TaggedString psylinkAffectedByTraitsNegativelyWarning = RoyalTitleUtility.GetPsylinkAffectedByTraitsNegativelyWarning(assignments.ExtraRequiredPawnsForReading.FirstOrDefault<Pawn>());
                if (psylinkAffectedByTraitsNegativelyWarning.RawText != null)
                {
                    pylonLinkExplanation += "\n\n" + psylinkAffectedByTraitsNegativelyWarning.Resolve();
                }
            }
            return pylonLinkExplanation;
        }

        // Token: 0x06005EA2 RID: 24226 RVA: 0x0020ECA8 File Offset: 0x0020CEA8
        public override String ExpectedDuration(Precept_Ritual ritual, RitualRoleAssignments assignments, float quality)
        {
            int count = assignments.SpectatorsForReading.Count;
            return Mathf.RoundToInt((float)ritual.behavior.def.durationTicks.max / RitualStage_AnimaTreeLinking.ProgressPerParticipantCurve.Evaluate((float)(count + 1))).ToStringTicksToPeriod(false, false, true, true, false);
        }
    }
}
