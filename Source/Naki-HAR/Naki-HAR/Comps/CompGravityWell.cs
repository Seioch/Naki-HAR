using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;
using RimWorld;
using UnityEngine;

namespace Naki_HAR
{
    [StaticConstructorOnStartup]
    public class CompGravityWell : ThingComp
    {
        public int currentTicks = 0;

        [Unsaved]
        public List<Pawn> tmpPawns = new List<Pawn>();

        public float curRotation = 0f;

        public static float rotSpeed = 5f;

        public static float lerpSpeed = 0.01f;

        private bool spawnedBlackHole = false;

        // Sound sustainer
        private Sustainer sustainer;

        // Materials for effects taken from CompProjectileInterceptor
        //private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

        //private static readonly Material ForceFieldConeMat = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);

        //private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        //private const float TextureActualRingSizeFactor = 1.1601562f;

        public CompProperties_GravityWell Props
        {
            get
            {
                return (CompProperties_GravityWell)this.props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDraw()
        {
            base.PostDraw();
        }

        private static void CreateGravityFleck(Map map, Vector3 position, float radius = 3f, int variant = 0)
        {
            float num = UnityEngine.Random.Range(0f, 6.2831855f);
            float num2 = 360f * num / 6.2831855f;
            Vector3 vector = new Vector3(radius * Mathf.Cos(num), 0f, radius * Mathf.Sin(num));
            Vector3 spawnPosition = position + vector;
            FleckManager flecks = map.flecks;
            //FleckCreationData fleckCreationData = new FleckCreationData();
            //FleckDef fleck = ((variant == 0) ? Naki_Defof.Naki_GW_line_A : Naki_Defof.Naki_GW_line_B);
            //fleckCreationData.def = fleck;
            //fleckCreationData.scale = 1f;
            //fleckCreationData.targetSize = 1f; //?
            //fleckCreationData.spawnPosition = spawnPosition;
            //// FleckCreationData fleckCreationData = FleckMaker.GetDataStatic(spawnPosition, map, fleck, UnityEngine.Random.Range(0.8f, 1.4f));
            //fleckCreationData.rotationRate = 0f;
            //fleckCreationData.rotation = 180f - num2;
            //fleckCreationData.velocitySpeed = UnityEngine.Random.Range(2.9f, 3.9f);
            //fleckCreationData.velocityAngle = 270f - num2;
            FleckCreationData fleckCreationData = new FleckCreationData
            {
                def = ((variant == 0) ? Naki_Defof.Naki_GW_line_A : Naki_Defof.Naki_GW_line_B),
                spawnPosition = spawnPosition,
                velocityAngle = 270f - num,
                velocitySpeed = UnityEngine.Random.Range(2.9f, 3.9f),
                rotationRate = 0,
                rotation = 180f - num2,
                scale = 1f,
                ageTicksOverride = -1
            };
            flecks.CreateFleck(fleckCreationData);
        }

        public override void CompTick()
        {
            base.CompTick();
            // Increment the amount of ticks the field has existed for
            currentTicks++;

            bool flag = this.sustainer == null;
            if (flag)
            {
                this.sustainer = SoundStarter.TrySpawnSustainer(Naki_Defof.Naki_Distortion_Sustainer, this.parent);
            }
            this.sustainer.Maintain();

            if (!spawnedBlackHole)
            {
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.DrawPos, this.parent.Map, Naki_Defof.Naki_GravityWellFleck, 6f);
                dataStatic.rotation = Rand.Range(0f, 360f);
                this.parent.Map.flecks.CreateFleck(dataStatic);
                spawnedBlackHole = true; // do it once
            }

            bool selfDestroy = this.currentTicks >= this.Props.maxticks;
            if (selfDestroy)
            {
                Log.Message("[Naki HAR] Destroying Annihilation field");
                // this.sustainer.End();
                this.sustainer.End();
                this.parent.Destroy(0);
                return;
            }
            // Check to see if any pawns should be drawn in every 10 ticks
            bool flag1 = Gen.IsHashIntervalTick(this.parent, 10);
            if (flag1)
            {
                // Create some gravity well flecks
                if (this.currentTicks < this.Props.maxticks)
                {
                    // FleckMaker.Static(this.parent.DrawPos, this.parent.Map, FleckDefOf.PsycastAreaEffect, 1.5f);
                    Log.Message("[Naki HAR] Spawning flecks");
                    for (int i = 0; i < 5; i++)
                    {
                        CreateGravityFleck(this.parent.Map, this.parent.DrawPos, this.Props.radius / 2, UnityEngine.Random.Range(0, 1));
                    }
                }
                // Log.Message("[Naki HAR] Annihilation field checking for victims");
                this.tmpPawns.Clear();
                int rangeForGrabbingPawns = Mathf.RoundToInt(this.Props.radius * 2);
                IntVec2 rangeIntVec2 = new IntVec2(rangeForGrabbingPawns, rangeForGrabbingPawns);

                foreach (IntVec3 intVec3 in GenAdj.OccupiedRect(this.parent.Position, this.parent.Rotation, rangeIntVec2))
                {
                    if (!intVec3.InBounds(this.parent.Map))
                        continue;
                    List<Thing> cellThings = this.parent.Map.thingGrid.ThingsListAt(intVec3);
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
            // Compile and test to see if damaging them a bit every second clears job priority Queue
            bool flag2 = Gen.IsHashIntervalTick(this.parent, 30);
            if (flag2)
            {
                foreach (Pawn pawn in this.tmpPawns)
                {
                    if (!pawn.DestroyedOrNull() && pawn.Spawned)
                    {
                        if (pawn.Position != this.parent.Position)
                        {
                            Vector3 vector3 = Vector3.Lerp(pawn.Position.ToVector3(), this.parent.Position.ToVector3(), lerpSpeed);
                            pawn.Position = vector3.ToIntVec3();
                            pawn.Notify_Teleported();
                            pawn.pather.nextCell = this.parent.Position;
                        }
                    }
                }
            }
            bool flag3 = (Gen.IsHashIntervalTick(this.parent, 60) && this.Props.damagePerTick > 0);
            if (flag3)
            {
                foreach (Pawn pawn in this.tmpPawns)
                {
                    if (!pawn.DestroyedOrNull() && pawn.Spawned)
                    {
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Blunt, this.Props.damagePerTick, float.MaxValue, instigator: this.parent);
                        pawn.TakeDamage(dinfo);
                        pawn.stances.stunner.StunFor(30, this.parent, false, true);
                    }
                }
            }
        }
    }
}
