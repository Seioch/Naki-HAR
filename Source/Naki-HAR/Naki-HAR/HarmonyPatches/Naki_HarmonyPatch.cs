﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;

namespace Naki_HAR
{
    [StaticConstructorOnStartup]
    public static class Naki_HarmonyPatch
    {
        private static Harmony harm;
        private static readonly List<MethodInfo> patchedMethods = new List<MethodInfo>();

        public new static Type GetType() => typeof(Naki_HarmonyPatch);

        static Naki_HarmonyPatch()
        {
            Log.Message("[Naki HAR] Trying to patch Naki");
            harm = new Harmony("Seioch.Naki.HAR");
            Log.Message("[Naki HAR] Patching VerbTracker");
            harm.Patch(AccessTools.Method(typeof(VerbTracker), "GetVerbsCommands"), 
                postfix: new HarmonyMethod(typeof(Naki_HarmonyPatch), nameof(VerbTrackerAmmo_Postfix)));
            Log.Message("[Naki HAR] Patching Kill");
            harm.Patch(AccessTools.Method(typeof(Pawn), "Kill"),
                postfix: new HarmonyMethod(typeof(Naki_HarmonyPatch), nameof(Kill_Postfix)));
        }

        public static void VerbTrackerAmmo_Postfix(ref VerbTracker __instance, ref IEnumerable<Command> __result)
        {
            CompEquippable equippable = __instance.directOwner as CompEquippable;
            if (equippable != null)
            {
                ThingWithComps parent = equippable.parent;
                CompNakiWeapons compNakiWeapons;
                if (parent != null && (compNakiWeapons = parent.GetComp<CompNakiWeapons>()) != null) // If we are able to get a reference to the CompNakiWeapons
                {
                    Pawn owner = equippable.PrimaryVerb.CasterPawn as Pawn;
                    if (owner != null && owner.Faction == Faction.OfPlayer)
                    {
                        __result = __result.Append(compNakiWeapons.GetNakiWeaponCommand());
                    }
                }
            }
        }

        public static void Kill_Postfix(ref Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.health.hediffSet.GetFirstHediffOfDef(Naki_Defof.DMDisintegration) != null) // If the pawn that just died has DMDisintegration as a hediff
            //if (dinfo.ToString().Contains("DMBurn")) // If the pawn that just died has died from DMBurn
            {
                __instance.Corpse.Destroy(DestroyMode.Vanish); // Destroy the body
                // Maybe spawn dark matter?
            }
        }

        // The idea of this prefix is to intercept the TryGiveAbility upon call before the original code executes.
        // This block will detect if the pawn is a Naki, and if so, executes almost the same Ability granting code
        // except it will only give Abilities that have "naki" inin the defname
        public static void TryGiveAbilityOfLevel_Prefix(ref Hediff_Psylink __instance, int abilityLevel, bool sendLetter = true)
        {
            if (__instance.pawn.IsNaki())
            {
                string str = "LetterLabelPsylinkLevelGained".Translate() + ": " + __instance.pawn.LabelShortCap;
                string str2;
                if (!__instance.pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
                {
                    // Major change: When the abilityDef is searched out of DefDatabase, only Naki abilities will return with this search
                    AbilityDef abilityDef = (from a in DefDatabase<AbilityDef>.AllDefs
                                             where a.level == abilityLevel && a.defName.ToLower().Contains("naki")
                                             select a).RandomElement<AbilityDef>();
                    __instance.pawn.abilities.GainAbility(abilityDef);
                    str2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(__instance.pawn, abilityLevel, Gen.YieldSingle<AbilityDef>(abilityDef));
                }
                else
                {
                    str2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(__instance.pawn, abilityLevel, null);
                }
                if (sendLetter && PawnUtility.ShouldSendNotificationAbout(__instance.pawn))
                {
                    Find.LetterStack.ReceiveLetter(str, str2, LetterDefOf.PositiveEvent, __instance.pawn, null, null, null, null);
                }
            }
        }
    }
}
