using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_AbilityDistortionField : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityDistortionField()
        {
            this.compClass = typeof(CompAbilityEffect_DistortionField);
        }

        // The Distortion Field itself
        // public ThingDef spawnableThing;

        // How big the distortion field is
        public float radius = 9.9f;

        // How many ticks should it last for
        public int duration = 2500;
    }
}
