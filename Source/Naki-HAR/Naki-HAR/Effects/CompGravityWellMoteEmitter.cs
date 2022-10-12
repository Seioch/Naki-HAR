using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace Naki_HAR
{
    [StaticConstructorOnStartup]
    public class CompGravityWellMoteEmitter : ThingComp
    {
        public int ticksSinceLastEmitted;

        protected Mote mote;

        // Properties
        private CompProperties_GravityWellMoteEmitter Props
        {
            get
            {
                return (CompProperties_GravityWellMoteEmitter)this.props;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            if (this.Props.ticksSinceLastEmittedMaxOffset > 0)
            {
                this.ticksSinceLastEmitted = Rand.Range(0, this.Props.ticksSinceLastEmittedMaxOffset);
            }
        }

        public override void CompTick()
        {
            if (!this.parent.Spawned)
            {
                return;
            }

            if (this.Props.emissionInterval != -1 && !this.Props.maintain)
            {
                if (this.ticksSinceLastEmitted >= this.Props.emissionInterval)
                {
                    // Log.Message("Emitting mote on pylon");
                    this.Emit();
                    this.ticksSinceLastEmitted = 0;
                }
                else
                {
                    this.ticksSinceLastEmitted++;
                }
            }
            else if (this.mote == null || this.mote.Destroyed)
            {
                this.Emit();
            }
        }

        // Token: 0x06006DFC RID: 28156 RVA: 0x00253E20 File Offset: 0x00252020
        protected virtual void Emit()
        {
            if (!this.parent.Spawned)
            {
                Log.Error("Thing tried spawning mote without being spawned!");
                return;
            }

            Vector3 vector = this.Props.EmissionOffset;
            // Refresh the BH fleck
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.DrawPos + vector, this.parent.Map, Naki_Defof.Naki_BlackHoleFleck, 2f);
            dataStatic.targetSize = 0.8f;
            this.parent.Map.flecks.CreateFleck(dataStatic);

            if (typeof(MoteAttached).IsAssignableFrom(this.Props.mote.thingClass))
            {
                this.mote = MoteMaker.MakeAttachedOverlay(this.parent, this.Props.mote, vector, 1f, -1f);
            }
            else
            {
                Vector3 vector2 = this.parent.DrawPos + vector;
                // Log.Message($"[Naki HAR]: vector2: {vector2.ToString()}");
                if (vector2.InBounds(this.parent.Map))
                {
                    this.mote = MoteMaker.MakeStaticMote(vector2, this.parent.Map, this.Props.mote, 1f);
                }
            }
            // If a sound is attached
            if (!this.Props.soundOnEmission.NullOrUndefined())
            {
                this.Props.soundOnEmission.PlayOneShot(SoundInfo.InMap(this.parent, MaintenanceType.None));
            }
        }

        // Token: 0x06006DFD RID: 28157 RVA: 0x00253F54 File Offset: 0x00252154
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.ticksSinceLastEmitted, ((this.Props.saveKeysPrefix != null) ? (this.Props.saveKeysPrefix + "_") : "") + "ticksSinceLastEmitted", 0, false);
        }
    }
}
