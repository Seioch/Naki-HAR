using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Naki_HAR
{
    public class CompNakiWeapons : ThingComp
    {
        public CompProperties_CompNakiWeapons Props => (CompProperties_CompNakiWeapons)props;
        public int currentShots = 0;

        //public CompProperties_CompNakiWeapons Props
        //{
        //    get
        //    {
        //        return (CompProperties_CompNakiWeapons)this.props;
        //    }
        //}
    }
}
