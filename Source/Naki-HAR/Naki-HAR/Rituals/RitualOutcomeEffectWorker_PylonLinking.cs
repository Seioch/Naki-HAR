using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    // Custom RitualOutcomeEffectWorker based on RitualOutcomeEffectWorker_AnimaLinking
    class RitualOutcomeEffectWorker_PylonLinking : RitualOutcomeEffectWorker_FromQuality
    {
        public override bool SupportsAttachableOutcomeEffect
        {
            get
            {
                return false;
            }
        }

        // Same as RitualOutcomeEffectWorker_AnimaLinking
        public override bool GivesDevelopmentPoints
        {
            get
            {
                return false;
            }
        }

        // Empty Constructors
        public RitualOutcomeEffectWorker_PylonLinking()
        {
        }

        // Token: 0x0600604D RID: 24653 RVA: 0x0021355D File Offset: 0x0021175D
        public RitualOutcomeEffectWorker_PylonLinking(RitualOutcomeEffectDef def) : base(def)
        {
        }

        // New apply, again we don't want to use CompPsylinkable cause it only deals with Anima stuff. 
        public override void Apply(float progress, Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual)
        {
            Log.Message("[Naki HAR] Applying pylon linking outcome");
            float quality = base.GetQuality(jobRitual, progress);
            Pawn pawn = jobRitual.PawnWithRole("organizer");
            Thing thing = jobRitual.selectedTarget.Thing;
            CompNakiPsylinkable compNakiPsylinkable = (thing != null) ? thing.TryGetComp<CompNakiPsylinkable>() : null;
            // int num = (int)RitualOutcomeEffectWorker_AnimaTreeLinking.RestoredGrassFromQuality.Evaluate(quality);
            // object obj2 = obj;
            if (compNakiPsylinkable != null)
            {
                compNakiPsylinkable.FinishLinkingRitual(pawn);
            }
            string text = "LetterTextLinkingRitualCompleted".Translate(pawn.Named("PAWN"), jobRitual.selectedTarget.Thing.Named("LINKABLE"));

            text = text + "\n\n" + this.OutcomeQualityBreakdownDesc(quality, progress, jobRitual);
            Find.LetterStack.ReceiveLetter("LetterLabelLinkingRitualCompleted".Translate(), text, LetterDefOf.RitualOutcomePositive, new LookTargets(new TargetInfo[]
            {
                pawn,
                jobRitual.selectedTarget.Thing
            }), null, null, null, null);
        }
    }
}
