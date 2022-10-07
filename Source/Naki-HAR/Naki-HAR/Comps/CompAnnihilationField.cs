using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace Naki_HAR
{
    public class CompAnnihilationField : ThingComp
    {
        public int currentTicks = 0;

        [Unsaved]
        public List<Pawn> tmpPawns = new List<Pawn>();

        public float curRotation = 0f;

        public static float rotSpeed = 5f;

        public static float lerpSpeed = 0.01f;

        // Where the field is (it won't move anyways)
        private IntVec3 fieldPos;

        // The field's current rotation;
        private Rot4 fieldRot;

        // Where the field graphic is
        private Vector3 drawPos;

        // Cached ref to the parent's map
        private Map parentMap;

        public CompProperties_AnnihilationField Props
        {
            get
            {
                return (CompProperties_AnnihilationField)this.props;
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            fieldPos = this.parent.Position;
            drawPos = this.parent.DrawPos;
            fieldRot = this.parent.Rotation;
            parentMap = this.parent.Map;

            // Black hole center Fleck graphic
            FleckCreationData dataStatic = FleckMaker.GetDataStatic(drawPos, parentMap, Naki_Defof.Naki_BlackHole, 1f);
            dataStatic.rotationRate = 1f;
            parentMap.flecks.CreateFleck(dataStatic);
        }

        public override void CompTick()
        {
            // Increment the amount of ticks the field has existed for
            currentTicks++;
            bool selfDestroy = this.currentTicks >= this.Props.maxticks;
            if (selfDestroy)
            {
                Log.Message("[Naki HAR] Destroying annihilation field");
                // this.sustainer.End();
                this.parent.Destroy(0);
                return;
            }
            // Check to see if any pawns should be drawn in every 250 ticks
            // Null Reference error here
            if (this.tmpPawns.Any() && currentTicks % GenTicks.TickRareInterval == 0)
            {
                this.tmpPawns.Clear();
                int rangeForGrabbingPawns = Mathf.RoundToInt(this.Props.radius * 2);
                IntVec2 rangeIntVec2 = new IntVec2(rangeForGrabbingPawns, rangeForGrabbingPawns);

                foreach (IntVec3 intVec3 in GenAdj.OccupiedRect(fieldPos, fieldRot, rangeIntVec2))
                {
                    if (!intVec3.InBounds(parentMap))
                        continue;
                    List<Thing> cellThings = parentMap.thingGrid.ThingsListAt(intVec3);
                    if (cellThings == null)
                        continue;

                    for (int i = 0; i < cellThings.Count; i++)
                    {
                        Thing t = cellThings[i];

                        if (t is Pawn p)
                        {
                            this.tmpPawns.Add(p);
                        }
                    }
                }
            }

            // Move victim pawns closer to the middle and damage them if they are in there
            foreach (Pawn pawn in this.tmpPawns)
            {
                if (!pawn.DestroyedOrNull() && pawn.Spawned)
                {
                    if (pawn.Position != fieldPos)
                    {
                        Vector3 vector3 = Vector3.Lerp(pawn.Position.ToVector3(), fieldPos.ToVector3(), lerpSpeed);
                        pawn.Position = vector3.ToIntVec3();
                        pawn.Notify_Teleported();
                        pawn.pather.nextCell = fieldPos;
                    }

                    if (this.Props.damagePerTick > 0)
                    {
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, this.Props.damagePerTick / GenTicks.TicksPerRealSecond, float.MaxValue, instigator: this.parent);
                        pawn.TakeDamage(dinfo);
                    }
                }
            }
        }
    }
}
