using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace Naki_HAR
{
    class IngestionOutcomeDoer_GiveHediffCheckRace : IngestionOutcomeDoer
    {
        protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested)
        {
            if (!pawn.IsNaki()) // Only poison pawns if they are not Naki
            {
                Hediff hediff = HediffMaker.MakeHediff(this.hediffDef, pawn, null);
                float num;
                if (this.severity > 0f)
                {
                    num = this.severity;
                }
                else
                {
                    num = this.hediffDef.initialSeverity;
                }
                if (this.divideByBodySize)
                {
                    num /= pawn.BodySize;
                }
                // AddictionUtility.ModifyChemicalEffectForToleranceAndBodySize(pawn, this.toleranceChemical, ref num);
                hediff.Severity = num;
                pawn.health.AddHediff(hediff, null, null, null);
            }
        }

        // Token: 0x060040B1 RID: 16561 RVA: 0x00161D32 File Offset: 0x0015FF32
        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(ThingDef parentDef)
        {
            if (parentDef.IsDrug && this.chance >= 1f)
            {
                foreach (StatDrawEntry statDrawEntry in this.hediffDef.SpecialDisplayStats(StatRequest.ForEmpty()))
                {
                    yield return statDrawEntry;
                }
                IEnumerator<StatDrawEntry> enumerator = null;
            }
            yield break;
        }

        // Token: 0x04002343 RID: 9027
        public HediffDef hediffDef;

        // Token: 0x04002344 RID: 9028
        public float severity = -1f;

        // Token: 0x04002345 RID: 9029
        public ChemicalDef toleranceChemical;

        // Token: 0x04002346 RID: 9030
        private bool divideByBodySize;
    }
}