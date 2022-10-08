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
        private static readonly Material ForceFieldMat = MaterialPool.MatFrom("Other/ForceField", ShaderDatabase.MoteGlow);

        private static readonly Material ForceFieldConeMat = MaterialPool.MatFrom("Other/ForceFieldCone", ShaderDatabase.MoteGlow);

        private static readonly MaterialPropertyBlock MatPropertyBlock = new MaterialPropertyBlock();

        private const float TextureActualRingSizeFactor = 1.1601562f;

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

            Log.Message("[Naki HAR] Annihilation Field Activating");

            // fieldPos = this.parent.Position;
            // drawPos = this.parent.DrawPos;
            // fieldRot = this.parent.Rotation;
            // parentMap = this.parent.Map;

            // Black hole center Fleck graphic
            //FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.Position.ToVector3(), this.parent.Map, Naki_Defof.Naki_BlackHole, 1f);
            //dataStatic.rotationRate = 1f;
            //this.parent.Map.flecks.CreateFleck(dataStatic);
        }

        public override void PostDraw()
        {
            base.PostDraw();

            Vector3 pos = this.parent.Position.ToVector3Shifted();
            pos.y = AltitudeLayer.MoteOverhead.AltitudeFor();

            CompGravityWell.MatPropertyBlock.SetColor(ShaderPropertyIDs.Color, Color.black);
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(pos, Quaternion.identity, new Vector3(this.Props.radius * 2f * 1.1601562f, 1f, this.Props.radius * 2f * 1.1601562f));
            Graphics.DrawMesh(MeshPool.plane10, matrix, CompGravityWell.ForceFieldMat, 0, null, 0, CompGravityWell.MatPropertyBlock);
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
                FleckCreationData dataStatic = FleckMaker.GetDataStatic(this.parent.DrawPos, this.parent.Map, Naki_Defof.Naki_BlackHole, 6f);
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
            // Check to see if any pawns should be drawn in every 30 ticks
            if (currentTicks % 30 == 0)
            {
                Log.Message("[Naki HAR] Annihilation field checking for victims");
                // this.tmpPawns.Clear();
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
