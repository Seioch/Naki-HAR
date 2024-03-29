﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Naki_HAR
{
    public class CompAbilityEffect_DistortionField : CompAbilityEffect
    {
        // Orphaned. TODO Clean up and delete this later
        // Get the properties, as well as a reference to a CompDistortionField
        public new CompProperties_AbilityDistortionField Props
        {
            get
            {
                return (CompProperties_AbilityDistortionField)this.props;
            }
        }

        // Similar to how CompAbilityEffect_Waterskip works, but uses the GenSpawn utility to put a building there
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            // Log.Message("[Naki HAR] Apply");
            Map map = this.parent.pawn.Map;

            // Log.Message("[Naki HAR] Distortion Field Spawning");
            GenSpawn.Spawn(this.Props.thingDef, target.Cell, map, WipeMode.Vanish);
            //building.SetFactionDirect(this.parent.pawn.Faction);

            //CompSpawnedBuilding comp = building.TryGetComp<CompSpawnedBuilding>();
            //if (comp != null)
            //{
            //    comp.lastDamageTick = Find.TickManager.TicksGame;
            //    comp.damagePerTick = 0; // no damage for distortion field

            //    int duration = this.Props.duration;
            //    if (duration > 0)
            //        comp.finalTick = comp.lastDamageTick + duration;
            //} else
            //{
            //    Log.Warning("[Naki HAR] Could not spawn Distortion Field Effector");
            //}
        }

        // Token: 0x060051B9 RID: 20921 RVA: 0x001B7D04 File Offset: 0x001B5F04
        private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
        {
            if (target.Cell.Filled(this.parent.pawn.Map))
            {
                yield break;
            }
            foreach (IntVec3 intVec in GenRadial.RadialCellsAround(target.Cell, this.parent.def.EffectRadius, true))
            {
                if (intVec.InBounds(map) && GenSight.LineOfSightToEdges(target.Cell, intVec, map, true, null))
                {
                    yield return intVec;
                }
            }
            IEnumerator<IntVec3> enumerator = null;
            yield break;
        }

        // Token: 0x060051BA RID: 20922 RVA: 0x001B7D24 File Offset: 0x001B5F24
        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            GenDraw.DrawFieldEdges(this.AffectedCells(target, this.parent.pawn.Map).ToList<IntVec3>(), this.Valid(target, false) ? Color.white : Color.red, null);
        }

        // checks if the target is valid
        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (target.Cell.Filled(this.parent.pawn.Map) || (target.Cell.GetFirstBuilding(this.parent.pawn.Map) != null))
            {
                if (throwMessages)
                {
                    Messages.Message("AbilityOccupiedCells".Translate(this.parent.def.LabelCap), target.ToTargetInfo(this.parent.pawn.Map), MessageTypeDefOf.RejectInput, false);
                }
                return false;
            }
            return true;
        }
    }
}
