using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Naki_HAR
{
    class JobDriver_Attune : JobDriver
    {


        // Where the pawn is facing. Pawn must face Pylon
        protected IntVec3 faceDir;

        // Psyfocus visual mote
        private Mote psyfocusMote;

        // Sound for meditating
        protected Sustainer sustainer;

        // Token: 0x04001EC5 RID: 7877
        protected const TargetIndex SpotInd = TargetIndex.A;

        // Token: 0x04001EC6 RID: 7878
        protected const TargetIndex BedInd = TargetIndex.B;

        // Token: 0x04001EC7 RID: 7879
        protected const TargetIndex FocusInd = TargetIndex.C;

        // How much attunement to give to the Pylon per tick per pawn
        public static float AttunementProgressPerTick = 6.666667E-05f;

        // Token: 0x04001EC9 RID: 7881
        private const int TicksBetweenMotesBase = 100;


        // Taken from JobDriver_Meditate
        public LocalTargetInfo Focus
        {
            get
            {
                return this.job.GetTarget(TargetIndex.C);
            }
        }

        // Taken from JobDriver_Meditate
        private bool FromBed
        {
            get
            {
                return this.job.GetTarget(TargetIndex.B).IsValid;
            }
        }

        // Taken from JobDriver_Meditate
        protected string PsyfocusPerDayReport()
        {
            if (!this.pawn.HasPsylink)
            {
                return "";
            }
            Thing thing = this.Focus.Thing;
            float f = MeditationUtility.PsyfocusGainPerTick(this.pawn, thing) * 60000f;
            return "\n" + "PsyfocusPerDayOfMeditation".Translate(f.ToStringPercent()).CapitalizeFirst();
        }

        // Taken from JobDriver_Meditate
        public override string GetReport()
        {
            if (!ModsConfig.RoyaltyActive)
            {
                return base.GetReport();
            }
            Thing thing = this.Focus.Thing;
            if (thing != null && !thing.Destroyed)
            {
                return "MeditatingAt".Translate() + ": " + thing.LabelShort.CapitalizeFirst() + "." + this.PsyfocusPerDayReport();
            }
            return base.GetReport() + this.PsyfocusPerDayReport();
        }

        // Taken from JobDriver_Meditate
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(TargetIndex.A), this.job, 1, -1, null, errorOnFailed);
        }

        // Custom implementation that only works if the pawn is a Naki and wants to meditate at a pylon
        // Non-naki get yielded nothing cause they shouldn't be able to proceed
        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil meditate = new Toil();
            meditate.socialMode = RandomSocialMode.Off;
            if (!this.pawn.IsNaki())
            {
                // This pawn is not a naki. Ths meditation toil will fail, effectively the Pylon is forbidden
                meditate.FailOnForbidden(TargetIndex.C);
            }
            else
            {
                Log.Message("[Naki HAR] Making Naki Meditation Toil");
                // The pawn is a Naki, create the Toil
                // Allow one to meditate from bed
                if (this.FromBed)
                {
                    this.KeepLyingDown(TargetIndex.B);
                    meditate = Toils_LayDown.LayDown(TargetIndex.B, this.job.GetTarget(TargetIndex.B).Thing is Building_Bed, false, false, true, PawnPosture.LayingOnGroundNormal);
                }
                else
                {
                    // We will let the Pawn go to a cell where they can meditate from the Pylon (e.g. meditation spot)
                    yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.OnCell);
                    meditate.initAction = delegate ()
                    {
                        LocalTargetInfo target = this.job.GetTarget(TargetIndex.C);
                        if (target.IsValid)
                        {
                            this.faceDir = target.Cell - this.pawn.Position;
                            return;
                        }
                        this.faceDir = (this.job.def.faceDir.IsValid ? this.job.def.faceDir : Rot4.Random).FacingCell;
                    };
                    if (this.Focus != null)
                    {
                        meditate.FailOnDespawnedOrNull(TargetIndex.C);
                        meditate.FailOnForbidden(TargetIndex.A);
                        if (this.pawn.HasPsylink && this.Focus.Thing != null)
                        {
                            meditate.FailOn(() => this.Focus.Thing.GetStatValueForPawn(StatDefOf.MeditationFocusStrength, this.pawn, true) < float.Epsilon);
                        }
                    }
                    meditate.handlingFacing = true;
                }
                meditate.defaultCompleteMode = ToilCompleteMode.Delay;
                meditate.defaultDuration = this.job.def.joyDuration;
                meditate.FailOn(() => !MeditationUtility.CanMeditateNow(this.pawn) || !MeditationUtility.SafeEnvironmentalConditions(this.pawn, base.TargetLocA, base.Map));
                meditate.AddPreTickAction(delegate
                {
                    bool flag = this.pawn.GetTimeAssignment() == TimeAssignmentDefOf.Meditate;
                    if (this.job.ignoreJoyTimeAssignment)
                    {
                        Pawn_PsychicEntropyTracker psychicEntropy = this.pawn.psychicEntropy;
                        if (!flag && psychicEntropy.TargetPsyfocus < psychicEntropy.CurrentPsyfocus && (psychicEntropy.TargetPsyfocus < this.job.psyfocusTargetLast || this.job.wasOnMeditationTimeAssignment))
                        {
                            base.EndJobWith(JobCondition.InterruptForced);
                            return;
                        }
                        this.job.psyfocusTargetLast = psychicEntropy.TargetPsyfocus;
                        this.job.wasOnMeditationTimeAssignment = flag;
                    }
                    if (this.faceDir.IsValid && !this.FromBed)
                    {
                        this.pawn.rotationTracker.FaceCell(this.pawn.Position + this.faceDir);
                    }
                    this.MeditationTick();
                    if (ModsConfig.RoyaltyActive && Naki_Defof.NakiFocus.CanPawnUse(this.pawn)) // As long as Royalty is active and this pawn (Naki) can use this Focus
                    {
                        int num = GenRadial.NumCellsInRadius(MeditationUtility.FocusObjectSearchRadius);
                        for (int i = 0; i < num; i++)
                        {
                            IntVec3 c = this.pawn.Position + GenRadial.RadialPattern[i];
                            if (c.InBounds(this.pawn.Map))
                            {
                                // Plant plant = c.GetPlant(this.pawn.Map);
                                Building pylon = c.GetFirstBuilding(this.pawn.Map); // Get a reference to the Pylon
                                if (pylon != null && pylon.def == Naki_Defof.NakiMeditationPylon)
                                {
                                    // CompSpawnSubplant compSpawnSubplant = plant.TryGetComp<CompSpawnSubplant>();
                                    CompNakiPsylinkable compNakiPsylinkable = pylon.TryGetComp<CompNakiPsylinkable>();
                                    if (compNakiPsylinkable != null)
                                    {
                                        // compSpawnSubplant.AddProgress(JobDriver_Meditate.AnimaTreeSubplantProgressPerTick, false);
                                        compNakiPsylinkable.AddProgress(AttunementProgressPerTick, false);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            // Return the Toil
            yield return meditate;
            yield break;
        }

        // Notify that this Toil is starting
        public override void Notify_Starting()
        {
            base.Notify_Starting();
            this.job.psyfocusTargetLast = this.pawn.psychicEntropy.TargetPsyfocus;
        }

        // Things that happen every time a Tick passes while the pawn is meditating. Basically, the pawn gains a bit of Int skill, some Joy, and 
        // psyfocus if they are meditating.
        // TODO: Offset dark matter need here!
        protected void MeditationTick()
        {
            this.pawn.skills.Learn(SkillDefOf.Intellectual, 0.018000001f, false);
            this.pawn.GainComfortFromCellIfPossible(false);
            if (this.pawn.needs.joy != null)
            {
                JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.None, 1f, null);
            }
            if (this.pawn.IsHashIntervalTick(100))
            {
                FleckMaker.ThrowMetaIcon(this.pawn.Position, this.pawn.Map, FleckDefOf.Meditating, 0.42f);
            }
            if (ModsConfig.RoyaltyActive)
            {
                this.pawn.psychicEntropy.Notify_Meditated();
                if (this.pawn.HasPsylink && this.pawn.psychicEntropy.PsychicSensitivity > 1E-45f)
                {
                    float yOffset = (float)(this.pawn.Position.x % 2 + this.pawn.Position.z % 2) / 10f;
                    if (this.psyfocusMote == null || this.psyfocusMote.Destroyed)
                    {
                        this.psyfocusMote = MoteMaker.MakeAttachedOverlay(this.pawn, ThingDefOf.Mote_PsyfocusPulse, Vector3.zero, 1f, -1f);
                        this.psyfocusMote.yOffset = yOffset;
                    }
                    this.psyfocusMote.Maintain();
                    if (this.sustainer == null || this.sustainer.Ended)
                    {
                        this.sustainer = SoundDefOf.MeditationGainPsyfocus.TrySpawnSustainer(SoundInfo.InMap(this.pawn, MaintenanceType.PerTick));
                    }
                    this.sustainer.Maintain();
                    this.pawn.psychicEntropy.GainPsyfocus(this.Focus.Thing);
                    // If the pawn is Naki, offset their Dark Matter need as well
                    if(this.pawn.IsNaki())
                    {
                        Need_NakiRaceDMNeed dMNeed = this.pawn.needs.TryGetNeed<Need_NakiRaceDMNeed>();
                        if (dMNeed != null)
                        {
                            // remove logspam once it is fixed
                            Log.Message("[Naki HAR] incrementing DM Need levels");
                            dMNeed.CurLevel += AttunementProgressPerTick;
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<IntVec3>(ref this.faceDir, "faceDir", default(IntVec3), false);
        }
    }
}
