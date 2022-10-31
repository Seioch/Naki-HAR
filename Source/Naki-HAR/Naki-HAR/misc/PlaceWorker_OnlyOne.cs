using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class PlaceWorker_OnlyOne : PlaceWorker
    {
        // Citation: Smash Phil's DropSpot code. Thank you Phil!
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            // The idea is as long as the map is loaded, and attempting to place the building on the map triggers a search such that another NakiMeditationPylon exists, return an AcceptanceReport that says "no way"
            if (map != null && (Current.Game.CurrentMap.listerBuildings.allBuildingsColonist.Find(x => x.def.defName == "NakiMeditationPylon") != null))
            {
                return new AcceptanceReport("OnlyOnePylonBuilding".Translate());
            }
            return true;
        }
    }
}
