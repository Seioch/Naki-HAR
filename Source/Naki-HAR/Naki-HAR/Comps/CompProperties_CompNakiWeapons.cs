using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompProperties_CompNakiWeapons : CompProperties
    {
        public int maximumShots = 100;
        public bool destroyOnEmpty = true;
        public string ammoGizmoLabel = "Naki Weapons ammo";


        public CompProperties_CompNakiWeapons()
        {
            this.compClass = typeof(CompNakiWeapons);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
        {
            foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
            {
                yield return statDrawEntry;
            }
            IEnumerator<StatDrawEntry> enumerator = null;
            if (((ThingDef)req.Def).GetType().Equals(this.compClass)) // Only draw for CompNakiWeapons
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Weapon, "Maximum shots", ((int)this.maximumShots).ToString(), "Shots Before Weapon Destroy", 3171, null, null, false);
            }
            yield break;
        }
    }
}
