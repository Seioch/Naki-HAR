using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;


namespace Naki_HAR
{
    // Very similar, but also checks to see if the user is a Naki. If not, return false.
    public class CompUseEffect_GainNakiAbility : CompUseEffect
    {
        private AbilityDef Ability
        {
            get
            {
                return this.parent.GetComp<CompNeurotrainer>().ability;
            }
        }

        // Token: 0x0600722A RID: 29226 RVA: 0x00267DB4 File Offset: 0x00265FB4
        public override void DoEffect(Pawn user)
        {
            base.DoEffect(user);
            AbilityDef ability = this.Ability;
            user.abilities.GainAbility(ability);
            if (PawnUtility.ShouldSendNotificationAbout(user))
            {
                Messages.Message("AbilityNeurotrainerUsed".Translate(user.Named("USER"), ability.LabelCap), user, MessageTypeDefOf.PositiveEvent, true);
            }
            // Destroy the item
            this.parent.Destroy();
        }

        // Token: 0x0600722B RID: 29227 RVA: 0x00267E1C File Offset: 0x0026601C
        public override bool CanBeUsedBy(Pawn p, out string failReason)
        {
            if (!p.health.hediffSet.HasHediff(HediffDefOf.PsychicAmplifier, false))
            {
                failReason = "PsycastNeurotrainerNoPsylink".TranslateWithBackup("PsycastNeurotrainerNoPsychicAmplifier");
                return false;
            }
            if (p.abilities != null && p.abilities.abilities.Any((Ability a) => a.def == this.Ability))
            {
                failReason = "PsycastNeurotrainerAbilityAlreadyLearned".Translate(p.Named("USER"), this.Ability.LabelCap);
                return false;
            }
            if (!p.IsNaki())
            {
                // TODO Tagged String
                failReason = "Cannot use: this pawn is not a Naki";
                return false;
            }
            return base.CanBeUsedBy(p, out failReason);
        }

        // Token: 0x0600722C RID: 29228 RVA: 0x00267EBC File Offset: 0x002660BC
        public override TaggedString ConfirmMessage(Pawn p)
        {
            Hediff firstHediffOfDef = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.PsychicAmplifier, false);
            if (firstHediffOfDef == null)
            {
                return null;
            }
            if (this.Ability.level > ((Hediff_Level)firstHediffOfDef).level)
            {
                return "PsylinkTooLowForGainAbility".Translate(p.Named("PAWN"), this.Ability.label.Named("ABILITY"));
            }
            return null;
        }
    }
}
