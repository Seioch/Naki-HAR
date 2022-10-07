using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_AnnihilationField : CompProperties
    {
        public CompProperties_AnnihilationField()
        {
            this.compClass = typeof(CompAnnihilationField);
        }

        // The maximum amount of time the Annihilation Field can last 
        public int maxticks = 900;

        public float radius = 11.9f;

        public float damagePerTick = 1.0f;
    }
}
