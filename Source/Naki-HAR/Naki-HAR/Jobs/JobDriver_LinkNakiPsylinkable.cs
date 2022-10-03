using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace Naki_HAR
{
    // This class is very similar to the original JobDriver_LinkPsylinkable, however in that class it looks specifically for a CompPsylinkable, which cannot work unless you are
    // going to be linking explicitly for the Natural or Royal medication focus. E.g. not using a Neuroformer that changes the level of the psylink. 
    // The reason why I am making a new one is that I still want the ritual to be a thing, and to not have Naki not use a ritual to psylink if the player is using Ideology.
    class JobDriver_LinkNakiPsylinkable : JobDriver
    {
        // How long to do the toil for cause magic numbers bad okay? 
        private readonly int toilDuration = 15000;
        // Unmodified getter
        private Thing PsylinkableThing
        {
            get
            {
                return base.TargetA.Thing;
            }
        }

        // Modified: this now returns the custom CompNakiPsylinkable
        private CompNakiPsylinkable Psylinkable
        {
            get
            {
                return this.PsylinkableThing.TryGetComp<CompNakiPsylinkable>();
            }
        }

        // Which tile to link at
        private LocalTargetInfo LinkSpot
        {
            get
            {
                return this.job.targetB;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.PsylinkableThing, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.LinkSpot, this.job, 1, -1, null, errorOnFailed);
        }

        // Makes the toil for the pawn to look at the pylon, makes the Flecks and the sound effects, and then yields the toil.
        protected override IEnumerable<Toil> MakeNewToils()
        {
            if (!ModLister.CheckRoyalty("Psylinkable"))
            {
                Log.Message("[Naki HAR] JobDriver_LinkNakiPsylinkable: Modlister failed to find Royalty");
                yield break;
            }
            base.AddFailCondition(() => !this.Psylinkable.CanPsylink(this.pawn, new LocalTargetInfo?(this.LinkSpot), true).Accepted);
            yield return Toils_Goto.GotoCell(TargetIndex.B, PathEndMode.OnCell);
            Toil toil = Toils_General.Wait(toilDuration, TargetIndex.None);
            toil.tickAction = delegate ()
            {
                this.pawn.rotationTracker.FaceTarget(this.PsylinkableThing);
                if (Find.TickManager.TicksGame % 720 == 0)
                {
                    Vector3 vector = this.pawn.TrueCenter();
                    vector += (this.PsylinkableThing.TrueCenter() - vector) * Rand.Value;
                    FleckMaker.Static(vector, this.pawn.Map, FleckDefOf.PsycastAreaEffect, 0.5f);
                    this.Psylinkable.Props.linkSound.PlayOneShot(SoundInfo.InMap(new TargetInfo(this.PsylinkableThing), MaintenanceType.None));
                }
            };
            toil.handlingFacing = false;
            toil.socialMode = RandomSocialMode.Off;
            yield return toil;
            yield break;
        }
        // How long it takes to link
        public const int LinkTimeTicks = 15000;

        // Effects firing interval
        public const int EffectsTickInterval = 720;

        // Position of who is getting a psylink
        protected const TargetIndex PsylinkableInd = TargetIndex.A;

        // Position of where the psylink granter (pylon) is
        protected const TargetIndex LinkSpotInd = TargetIndex.B;
    }
}
