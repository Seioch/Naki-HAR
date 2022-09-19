using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse;
using RimWorld;
using System.Linq;

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
            harm = new Harmony("Naki");
            harm.Patch(AccessTools.Method(typeof(VerbTracker), "GetVerbsCommands"), postfix: new HarmonyMethod(typeof(Gizmo_NakiWeaponAmmoCounter), nameof(VerbTrackerAmmo_Postfix)));
            // harm.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void VerbTrackerAmmo_Postfix(ref VerbTracker __instance, ref IEnumerable<Gizmo_NakiWeaponAmmoCounter> __result)
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
                        __result = __result.Append(compNakiWeapons.GetANakiWeaponAmmoCounter());
                    }
                }
            }
        }
    }
}
