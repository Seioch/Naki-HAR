using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_CompNakiWeapons : CompProperties
    {
        public int maximumShots = 100;
        public bool destroyOnEmpty = true;

        public CompProperties_CompNakiWeapons()
        {
            this.compClass = typeof(CompNakiWeapons);
        }
    }
}
