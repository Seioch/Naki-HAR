using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompDMHealing : CompAbilityEffect
    {
        public new CompProperties_CompDMHealing Props => (CompProperties_CompDMHealing)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.sound = SoundDefOf.PsycastPsychicEffect;//SoundDefOf.PsycastPsychicPulse;
            base.Apply(target, dest);

            if (target.Pawn == null) { return; }
            // Up to a publicly set number of applications, heal that injury up to some %
            for (int i = 0; i < 10 *  Props.maxApplications; i++)
            {
                Hediff_Injury hediff_Injury = FindInjury(target.Pawn);
                if (hediff_Injury != null)
                {
                    hediff_Injury.Heal(Props.healingEffectiveness);
                    HediffWithComps hediffWithComps = hediff_Injury as HediffWithComps;
                }
            }
            //SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
        }

        private Hediff_Injury FindInjury(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                // 1.4 overhauls how GetHediffs is done. You can run the predicate filters on the GetHediffs method itself, and it pushes the result to a referenced List rather than Collection
                List<Hediff_Injury> hediffs = new List<Hediff_Injury>();
                Predicate<Hediff_Injury> predicate = (Hediff_Injury injury) => (injury != null && injury.Visible && injury.def.everCurableByItem && !injury.IsPermanent() && injury.CanHealNaturally());
                pawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref hediffs, predicate);
                // IEnumerable<Hediff_Injury> injuryList = hediffs.Where(predicate);
                if (hediffs.Count() == 0) return null;
                return hediffs.ElementAt(Rand.Range(0, hediffs.Count()));
            }
            return null;
        }
    }
}
