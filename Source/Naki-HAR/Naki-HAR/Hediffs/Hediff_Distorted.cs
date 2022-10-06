using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace Naki_HAR
{
    class Hediff_Distorted : Hediff
    {
        // This is the Hediff that is applied to a pawn caught in a distortion field
        // The stages here return two PawnCapacityModifiers, set by how close the pawn is to the center of the distortion field. 
        // The closer the pawn is to the center, the stronger the penalty to consciousness and breathing is made
        // Note that here 18.9 is the static unchanging radius of the field. We can change this later if you want larger/smaller fields

        private readonly float fieldRadius = 18.9f;

        public override HediffStage CurStage
        {
            get
            {
                HediffStage result;
                if (this.field != null)
                {
                    // capMods is Capacity Modifiers. What line 29 does is create a new HediffStage, store it in result, and set its internal list capMods to a new list of PawnCapacityModifiers
                    // This lets the result change the pawn's capacities as needed
                    (result = new HediffStage()).capMods = new List<PawnCapacityModifier>
                    {
                        // Note: Lerp is a math tool to get a number between two indexes. The way setMax is done is that it returns a float between 0.4 and 0.8, growing closer to 0.8 the closer the distance
                        // the pawn is to the center of the distortion field. 
                        new PawnCapacityModifier
                        {
                            capacity = PawnCapacityDefOf.Consciousness,
                            setMax = Mathf.Lerp(0.4f, 0.8f, IntVec3Utility.DistanceTo(this.pawn.Position, this.field.parent.Position) / fieldRadius)  
                        },
                        new PawnCapacityModifier
                        {
                            capacity = PawnCapacityDefOf.Breathing,
                            setMax = Mathf.Lerp(0.4f, 0.8f, IntVec3Utility.DistanceTo(this.pawn.Position, this.field.parent.Position) / fieldRadius)
                        }
                    };
                }
                else
                {
                    result = base.CurStage;
                }
                return result;
            }
        }

        // A override to see if the hediff should be removed. 
        public override bool ShouldRemove
        {
            get
            {
                return this.field.parent.Destroyed || IntVec3Utility.DistanceTo(this.pawn.Position, this.field.parent.Position) >= fieldRadius;
            }
        }

        // Token: 0x060002AC RID: 684 RVA: 0x00010711 File Offset: 0x0000E911
        public override void ExposeData()
        {
            base.ExposeData();
            // Could not expose the CompDistortionField here. Commenting it out to see what happens. 
            // Scribe_References.Look<CompDistortionField>(ref this.field, "distortionfield", false);
        }

        // Token: 0x04000146 RID: 326
        public CompDistortionField field;
    }
}
