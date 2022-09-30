using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using HarmonyLib;
using Verse;
using RimWorld;
using Verse.AI;


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
            Log.Message("[Naki HAR] Patching TryGiveAbilityOfLevel");
            harm.Patch(AccessTools.Method(typeof(Hediff_Psylink), "TryGiveAbilityOfLevel"), 
                prefix: new HarmonyMethod(typeof(Naki_HarmonyPatch), nameof(TryGiveAbilityOfLevel_Prefix)));
            Log.Message("[Naki HAR] Patching MeditationUtility");
            harm.Patch(AccessTools.Method(typeof(MeditationUtility), "GetMeditationJob"),
                postfix: new HarmonyMethod(typeof(Naki_HarmonyPatch), nameof(GetMeditationJob_Postfix)));
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

        public static void Kill_Postfix(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
        {
            if (__instance.health.hediffSet.GetFirstHediffOfDef(Naki_Defof.Hediff_DMBurn) != null) // If the pawn that just died has Hediff_DMBurn damage
            //if (dinfo.ToString().Contains("DMBurn")) // If the pawn that just died has died from DMBurn
            {
                // Spawn dark matter?
                Thing darkMatter = ThingMaker.MakeThing(Naki_Defof.DarkMatter, null);
                darkMatter.stackCount = 1; // Maybe later use GenMath to create a random amount of DM from 1-3
                GenPlace.TryPlaceThing(darkMatter, __instance.Corpse.Position, __instance.Corpse.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                __instance.Corpse.Destroy(DestroyMode.Vanish); // Destroy the body
            }
        }

        // The idea of this prefix is to intercept the TryGiveAbility upon call before the original code executes.
        // This block will detect if the pawn is a Naki, and if so, executes almost the same Ability granting code
        // except it will only give Abilities that have "naki" in the defname
        public static bool TryGiveAbilityOfLevel_Prefix(Hediff_Psylink __instance, int abilityLevel, bool sendLetter = true)
        {
            // Log.Message($"[Naki HAR] {__instance.ToString()}");
            if (__instance.pawn.IsNaki())
            {
                // Log.Message($"[Naki HAR] Naki psylink giving detected, giving {__instance.pawn.Name} a Naki ability instead.");
                string str = "LetterLabelPsylinkLevelGained".Translate() + ": " + __instance.pawn.LabelShortCap;
                string str2;
                if (!__instance.pawn.abilities.abilities.Any((Ability a) => a.def.level == abilityLevel))
                {
                    // Major change: When the abilityDef is searched out of DefDatabase, only Naki abilities will return with this search
                    AbilityDef abilityDef = (from a in DefDatabase<AbilityDef>.AllDefs
                                             where a.level == abilityLevel && a.defName.ToLower().Contains("naki")
                                             select a).RandomElement<AbilityDef>();
                    Log.Message($"[Naki HAR] EXISTING Naki psylink giving detected, giving {__instance.pawn.Name} Naki ability {abilityDef.defName} instead.");
                    __instance.pawn.abilities.GainAbility(abilityDef);
                    str2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(__instance.pawn, abilityLevel, Gen.YieldSingle<AbilityDef>(abilityDef));
                    return false;
                }
                else
                {
                    // Log.Message($"[Naki HAR] {__instance.pawn.Name} did not have a psylink ability, granting a new one.");
                    // Major change: When the abilityDef is searched out of DefDatabase, only Naki abilities will return with this search
                    AbilityDef abilityDef = (from a in DefDatabase<AbilityDef>.AllDefs
                                             where a.level == abilityLevel && a.defName.ToLower().Contains("naki")
                                             select a).RandomElement<AbilityDef>();
                    Log.Message($"[Naki HAR] NEW Naki psylink giving detected, giving {__instance.pawn.Name} Naki ability {abilityDef.defName} instead.");
                    __instance.pawn.abilities.GainAbility(abilityDef);
                    str2 = Hediff_Psylink.MakeLetterTextNewPsylinkLevel(__instance.pawn, abilityLevel, null);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        // This prefix intercepts the code path to getting a meditation job. If the pawn is a Naki, create an Attunement jobdef instead. 
        public static void GetMeditationJob_Postfix(ref Job __result, Pawn pawn, bool forJoy = false)
        {
            if (pawn.IsNaki())
            {
                // Log.Message("[Naki HAR] Creating a new Attunement job for Naki");
                MeditationSpotAndFocus meditationSpotAndFocus = MeditationUtility.FindMeditationSpot(pawn);
                if (meditationSpotAndFocus.IsValid)
                {
                    Building_Throne t;
                    Job job = __result;
                    if ((t = (meditationSpotAndFocus.focus.Thing as Building_Throne)) != null)
                    {
                        // Do not return a Reigning job. Naki cannot use Empire psycasts anyways.
                        // The code path should never get here but if players somehow try to give a Naki a royal title then this will stop Naki from Reigning
                        Log.Error("[Naki HAR] Naki tried to do a Reigning job when they're not allowed to");
                        job = null;
                    }
                    else
                    {
                        JobDef def = Naki_Defof.Attune;
                        IdeoFoundation_Deity ideoFoundation_Deity;
                        if (forJoy && ModsConfig.IdeologyActive && pawn.Ideo != null && (ideoFoundation_Deity = (pawn.Ideo.foundation as IdeoFoundation_Deity)) != null && ideoFoundation_Deity.DeitiesListForReading.Any<IdeoFoundation_Deity.Deity>())
                        {
                            def = JobDefOf.MeditatePray;
                        }
                        job = JobMaker.MakeJob(def, meditationSpotAndFocus.spot, null, meditationSpotAndFocus.focus);
                    }
                    job.ignoreJoyTimeAssignment = !forJoy;
                    __result = job;
                }
            }
            else
            {
                // Not a Naki don't override the output of GetMeditationJob!

            }
        }
    }
}
