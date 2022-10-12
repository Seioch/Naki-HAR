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
    // Very similar to CompMoteEmitter, but the mote itself is a wave that goes in, rather than out. 
    public class CompProperties_GravityWellMoteEmitter : CompProperties
    {
        public CompProperties_GravityWellMoteEmitter()
        {
            this.compClass = typeof(CompGravityWellMoteEmitter);
        }

        public Vector3 EmissionOffset
        {
            get
            {
                return new Vector3(offsetX, offsetY, offsetZ);
            }
        }

        // Token: 0x06006DF8 RID: 28152 RVA: 0x00253C8B File Offset: 0x00251E8B
        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            if (this.mote == null)
            {
                yield return "CompGravityWellMoteEmitter must have a mote assigned.";
            }
            yield break;
        }

        // Token: 0x04003CE2 RID: 15586
        public ThingDef mote;

        // Token: 0x04003CE3 RID: 15587
        public float offsetX = 0;
        public float offsetY = 0;
        public float offsetZ = 0;

        // Token: 0x04003CE6 RID: 15590
        public SoundDef soundOnEmission;

        // Token: 0x04003CE7 RID: 15591
        public int emissionInterval = -1;

        // Token: 0x04003CE8 RID: 15592
        public int ticksSinceLastEmittedMaxOffset;

        // Token: 0x04003CE9 RID: 15593
        public bool maintain;

        // Token: 0x04003CEA RID: 15594
        public string saveKeysPrefix;
    }
}
