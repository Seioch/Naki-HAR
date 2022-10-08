using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_GravityWell : CompProperties
    {
        public CompProperties_GravityWell()
        {
            this.compClass = typeof(CompGravityWell);
        }

        // The maximum amount of time the Annihilation Field can last 
        public int maxticks = 600;

        public float radius = 6.9f;

        public float damagePerTick = 1.0f;
    }
}
