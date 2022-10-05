using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    // Maybe orphan?
    class CompProperties_SpawnableBuilding : CompProperties
    {
        public CompProperties_SpawnableBuilding()
        {
            this.compClass = typeof(CompSpawnedBuilding);
        }
    }
}
