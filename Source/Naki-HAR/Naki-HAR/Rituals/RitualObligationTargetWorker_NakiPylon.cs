using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    // This class is very similar to RitualObligationTargetWorker_AnimaTree, but removes the Natural hardcoding and instead looks 
    // for CompNakiPsylinkable to execute targeting for the linking ritual
    class RitualObligationTargetWorker_NakiPylon : RitualObligationTargetFilter
    {
        // Empty constructors
        public RitualObligationTargetWorker_NakiPylon() { }
        public RitualObligationTargetWorker_NakiPylon(RitualObligationTargetFilterDef def) : base(def) { }

        public override IEnumerable<TargetInfo> GetTargets(RitualObligation obligation, Map map)
        {
            yield break;
        }

        // Completely new RitualTargetUseReport return. Checks if the target is a Naki, gets the Naki's CompNakiPsylinkable comp
        // and then sees if that target can start the ritual, which it should
        // TODO: Use Tagged Strings to allow translations!
        protected override RitualTargetUseReport CanUseTargetInternal(TargetInfo target, RitualObligation obligation)
        {
            CompNakiPsylinkable compNakiPsylinkable = target.Thing.TryGetComp<CompNakiPsylinkable>();
            if (compNakiPsylinkable == null)
            {
                // Log.Warning($"[Naki HAR] compNakiPsylinkable is null in RitualObligationTargetWorker_NakiPylon. target.Thing is {target.Thing.ToString()}");
                return false;
            }
            bool flag = false;
            bool flag2 = false;
            foreach (Pawn pawn in target.Map.mapPawns.FreeColonistsSpawned)
            {
                if (compNakiPsylinkable.CanPsylink(pawn, null, false))
                {
                    flag2 = true;
                }
                if (compNakiPsylinkable.Props.requiredFocus.CanPawnUse(pawn))
                {
                    flag = true;
                }
            }
            // Check the amount of attunement is enough (at level 2 because Naki already spawn with Level 1)
            if (compNakiPsylinkable.currentAttunement < compNakiPsylinkable.Props.requiredAttunementPerPsylinkLevel[1])
            {
                // return "RitualTargetNotEnoughAttunement".Translate(compNakiPsylinkable.Props.requiredAttunementPerPsylinkLevel[1]);
                return $"Not enough attunement to upgrade psylink. Minimum needed attunement: {compNakiPsylinkable.Props.requiredAttunementPerPsylinkLevel[1]}";
            }
            // Check if pawn can use focus
            if (!flag)
            {
                // return "RitualTargetNoPawnsWithNakiFocus".Translate();
                return "No Naki colonists can link with the pylon right now.";
            }
            // Check if the component can link
            if (!flag2)
            {
                return "No colonists with the Naki meditation type.";
                //return "RitualTargetNakiNoPawnsToLink".Translate();
            }
            // Log.Message("RitualTargetUseReport successfully created Naki linking ritual");
            return true;
        }

        // Token: 0x06005F38 RID: 24376 RVA: 0x002105D0 File Offset: 0x0020E7D0
        public override IEnumerable<string> GetTargetInfos(RitualObligation obligation)
        {
            //yield return "RitualTargetAnimaTreeInfo".Translate();
            yield return "A Naki Pylon.";
            yield break;
        }
    }
}
