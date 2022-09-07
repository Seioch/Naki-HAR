using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Naki_HAR.Comps
{
    public class compNakiWeapons : ThingComp
    {
        public int maximumShots = 100;
        public int currentShots = 0;
        public bool destroyOnEmpty = true;
    }
}
