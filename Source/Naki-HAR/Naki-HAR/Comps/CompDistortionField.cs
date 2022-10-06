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
    class CompDistortionField : ThingComp
    {

        // Sound sustainer
        private Sustainer sustainer;

        // What tick did this distortion field start from?
        private int currentTicks = 0;

        // Maximum amount of ticks the Distortion Field can be alive for
        private int maxTicks = 2500;

        // Radius of effect
        private float radius = 9.9f;

        public override void CompTick()
        {
            base.CompTick();
            Map parentMap = this.parent.Map;
            currentTicks++; // Increase the number of ticks that this Distortion Field has lived for
            bool flag = this.sustainer == null;
            if (flag)
            {
                this.sustainer = SoundStarter.TrySpawnSustainer(Naki_Defof.Naki_Distortion_Sustainer, this.parent);
            }
            this.sustainer.Maintain();
            for (int i = 0; i < 3; i++)
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.RandomLocation(), base.parent.Map, Naki_Defof.Naki_Distortion_vibration, 1f);
                dataStatic.rotation = Rand.Range(0f, 360f);
                parentMap.flecks.CreateFleck(dataStatic);
                FleckMaker.ThrowSmoke(this.RandomLocation(), parentMap, 4f);
            }
            bool flag2 = this.currentTicks >= maxTicks;
            if (flag2)
            {
                Log.Message("[Naki HAR] Destroying distortion field");
                this.parent.Destroy(0);
            }
            bool flag3 = Gen.IsHashIntervalTick(this.parent, 30);
            if (flag3)
            {
                foreach (Pawn pawn in GenRadial.RadialDistinctThingsAround(base.parent.Position, parentMap, radius, true).OfType<Pawn>())
                {
                    bool isMechanoid = pawn.RaceProps.IsMechanoid;
                    if (isMechanoid)
                    {
                        pawn.stances.stunner.StunFor(30, this.parent, false, true);
                    }
                    else
                    {
                        bool flag4 = pawn.health.hediffSet.GetFirstHediffOfDef(Naki_Defof.Naki_Distortion, false) == null;
                        if (flag4)
                        {
                            Hediff_Distorted hediff_Distorted = (Hediff_Distorted)HediffMaker.MakeHediff(Naki_Defof.Naki_Distortion, pawn, null);
                            hediff_Distorted.field = this;
                            pawn.health.AddHediff(hediff_Distorted, null, null, null);
                        }
                    }
                }
            }
        }

        //public override void Draw()
        //{
        //}

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
            return this.parent.DrawPos + Vector3Utility.RotatedBy(new Vector3(CompDistortionField.Wrap(Mathf.Abs(Rand.Gaussian(0f, 18.9f)), 18.9f), 0f, 0f), Rand.Range(0f, 360f));
        }
    }
}
