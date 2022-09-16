using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    [DefOf]
    public static class Naki_Defof
    {
        public static HediffDef DMDisintegration;
        public static DamageDef DMBurn;
        public static NeedDef NakiRaceDMNeed;

        static Naki_Defof()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Naki_Defof));
        }
    }
}
