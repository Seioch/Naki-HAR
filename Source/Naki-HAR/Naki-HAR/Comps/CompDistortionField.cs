using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.Sound;
using UnityEngine;

namespace Naki_HAR
{
    // Based on Vortex from VPE, this is an object that slows down and lowers the consciousness of pawns trapped in it
    class CompDistortionField : ThingWithComps
    {
        // How big the distortion field is
        public const float RADIUS = 18.9f;

        // How many ticks should it last for
        public const int DURATION = 2500;

        // Sound sustainer
        private Sustainer sustainer;

        // 
        private int startTick;

        public override void Tick()
        {
            base.Tick();
            bool flag = this.sustainer == null;
            if (flag)
            {
                this.sustainer = SoundStarter.TrySpawnSustainer(Naki_Defof.Naki_Distortion_Sustainer, this);
            }
            this.sustainer.Maintain();
            for (int i = 0; i < 3; i++)
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.RandomLocation(), base.Map, VPE_DefOf.VPE_VortexSpark, 1f);
                dataStatic.rotation = Rand.Range(0f, 360f);
                base.Map.flecks.CreateFleck(dataStatic);
                FleckMaker.ThrowSmoke(this.RandomLocation(), base.Map, 4f);
            }
            bool flag2 = Find.TickManager.TicksGame - this.startTick > 2500;
            if (flag2)
            {
                this.Destroy(0);
            }
            bool flag3 = Gen.IsHashIntervalTick(this, 30);
            if (flag3)
            {
                foreach (Pawn pawn in GenRadial.RadialDistinctThingsAround(base.Position, base.Map, 18.9f, true).OfType<Pawn>())
                {
                    bool isMechanoid = pawn.RaceProps.IsMechanoid;
                    if (isMechanoid)
                    {
                        pawn.stances.stunner.StunFor(30, this, false, true);
                    }
                    else
                    {
                        bool flag4 = pawn.health.hediffSet.GetFirstHediffOfDef(VPE_DefOf.VPE_Vortex, false) == null;
                        if (flag4)
                        {
                            Hediff_Vortexed hediff_Vortexed = (Hediff_Vortexed)HediffMaker.MakeHediff(VPE_DefOf.VPE_Vortex, pawn, null);
                            hediff_Vortexed.Vortex = this;
                            pawn.health.AddHediff(hediff_Vortexed, null, null, null);
                        }
                    }
                }
            }
        }

        public override void Draw()
        {
        }

        // Local helper to make sure that an inputted float x is set to 0 out of float max
        public static float Wrap(float x, float max)
        {
            while (x > max)
            {
                x -= max;
            }
            return x;
        }

        // From what I can tell, it rotates the draw position by some amount between 0 and 360 degrees. 
        // TODO: Document here exactly how this works
        private Vector3 RandomLocation()
        {
            return this.DrawPos + Vector3Utility.RotatedBy(new Vector3(CompDistortionField.Wrap(Mathf.Abs(Rand.Gaussian(0f, 18.9f)), 18.9f), 0f, 0f), Rand.Range(0f, 360f));
        }
    }
}
