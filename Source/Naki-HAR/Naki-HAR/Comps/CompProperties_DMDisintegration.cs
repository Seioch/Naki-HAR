using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_DMDisintegration : HediffCompProperties
    {
        public int maxApplications = 6; // How many times should disintegration fire upon the victim
        public int tickMax = 64; // How often to fire off the Damage
        public float damagePerTick = 1.0f; // DEFAULT Damage per tick per body part, change this in XML

        public CompProperties_DMDisintegration()
        {
            this.compClass = typeof(CompDisintegration);
        }
    }
}
