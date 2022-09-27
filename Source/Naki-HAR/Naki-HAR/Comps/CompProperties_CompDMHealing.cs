using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Naki_HAR
{
    public class CompProperties_CompDMHealing : CompProperties_AbilityEffect
    {
        public CompProperties_CompDMHealing()
        {
            this.compClass = typeof(CompDMHealing);
        }

        public int maxApplications = 10;
        public float healingEffectiveness = 0.1f;
    }
}
