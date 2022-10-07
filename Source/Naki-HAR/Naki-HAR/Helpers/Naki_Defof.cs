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
        public static HediffDef Naki_Distortion;
        public static DamageDef DMBurn;
        public static NeedDef NakiRaceDMNeed;
        public static ThingDef DarkMatter;
        public static ThingDef Naki_DistortionFieldBuilding;
        public static ThingDef Naki_AnnihilationField;
        public static SoundDef Naki_DM_Sizzle;
        public static SoundDef Naki_Distortion_Sustainer;

        public static FleckDef Naki_Distortion_vibration;
        public static FleckDef Naki_BlackHole;

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
