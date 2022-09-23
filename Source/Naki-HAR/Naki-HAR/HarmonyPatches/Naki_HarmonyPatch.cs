using System;
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

        // Checks to see if the pawn is about to die from Dark Matter Disintegration, destroys the body instead, and drops 1 DM
        public static void CheckForStateChange_Prefix(ref Pawn_HealthTracker __instance, ref Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
        {
            /*if (!__instance.Dead)
            {
                Log.Message("[Naki HAR]" + dinfo.ToString());
                ref isGoingToDie = AccessTools.Method(typeof(Pawn_HealthTracker), ""));
                if (__instance && dinfo.ToString().Contains("DMBurn"))
                {
                    if (!___pawn.Destroyed) // If the pawn hasn't been destroyed yet
                    {
                        ___pawn.Kill(dinfo, hediff);
                        ___pawn.Destroy(); // Remove the body
                    }
                    return;
                }
            }*/
            return;
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
    }
}
