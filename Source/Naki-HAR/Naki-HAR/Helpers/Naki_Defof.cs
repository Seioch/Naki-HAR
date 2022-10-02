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
        public static HediffDef Hediff_DMBurn;
        public static DamageDef DMBurn;
        public static NeedDef NakiRaceDMNeed;
        public static ThingDef DarkMatter;
        public static SoundDef Naki_DM_Sizzle;

        [MayRequireRoyalty]
        public static PreceptDef NakiPylonLinking;

        [MayRequireRoyalty]
        public static MeditationFocusDef NakiFocus;

        [MayRequireRoyalty]
        public static ThingDef NakiMeditationPylon;

        [MayRequireRoyalty]
        public static JobDef Attune;

        [MayRequireRoyalty]
        public static JobDef LinkNakiPsylinkable;

        static Naki_Defof()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(Naki_Defof));
        }
    }
}
