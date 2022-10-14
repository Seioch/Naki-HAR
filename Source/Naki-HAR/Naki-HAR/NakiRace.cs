using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static Verse.Widgets;

namespace Naki_HAR
{

    public class NakiRace : Verse.Mod
    {
        public delegate RecipeDef CreateRecipeDefDelegate(ThingDef def, int adjustedCount);
        public static CreateRecipeDefDelegate createRecipe;

        public new static Type GetType() => typeof(NakiRace);

        public NakiRace(ModContentPack content) : base(content)
        {
        }
    }
}
