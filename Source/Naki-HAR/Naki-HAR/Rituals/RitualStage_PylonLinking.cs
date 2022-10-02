using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Naki_HAR
{
    class RitualStage_PylonLinking : RitualStage
    {
        // This class is basically the same as RitualStage_AnimaTreeLinking, but I want this to be separate in case
        // other mods mess with RitualStage_AnimaTreeLinking. Stuff that affects Empire psycasts should not affect
        // the Naki psycast system.
        public override TargetInfo GetSecondFocus(LordJob_Ritual ritual)
        {
            return ritual.selectedTarget;
        }

        // Token: 0x06005E5E RID: 24158 RVA: 0x0020DA74 File Offset: 0x0020BC74
        public override float ProgressPerTick(LordJob_Ritual ritual)
        {
            int num = 1;
            foreach (Pawn p in ritual.assignments.SpectatorsForReading)
            {
                if (ritual.IsParticipating(p))
                {
                    num++;
                }
            }
            return RitualStage_AnimaTreeLinking.ProgressPerParticipantCurve.Evaluate((float)num);
        }

        // Token: 0x040036DE RID: 14046
        public static readonly SimpleCurve ProgressPerParticipantCurve = new SimpleCurve
        {
            {
                new CurvePoint(1f, 1f),
                true
            },
            {
                new CurvePoint(2f, 1.2f),
                true
            },
            {
                new CurvePoint(4f, 1.5f),
                true
            },
            {
                new CurvePoint(6f, 2f),
                true
            },
            {
                new CurvePoint(8f, 3f),
                true
            }
        };
    }
}
