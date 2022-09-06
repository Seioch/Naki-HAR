using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Naki_HAR.Comps
{
    public class compNakiWeapons : CompProperties
    {
        public int maximumShots = 100;

        public int currentShots = 0;
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats(StatRequest req)
    {
        foreach (StatDrawEntry statDrawEntry in base.SpecialDisplayStats(req))
        {
            yield return statDrawEntry;
        }
        IEnumerator<StatDrawEntry> enumerator = null;
        if (!req.HasThing)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadMaxCharges_Name".Translate(this.ChargeNounArgument), this.maxCharges.ToString(), "Stat_Thing_ReloadMaxCharges_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
        }
        if (this.ammoDef != null)
        {
            if (this.ammoCountToRefill != 0)
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadRefill_Name".Translate(this.ChargeNounArgument), string.Format("{0} {1}", this.ammoCountToRefill, this.ammoDef.label), "Stat_Thing_ReloadRefill_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
            }
            else
            {
                yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadPerCharge_Name".Translate(this.ChargeNounArgument), string.Format("{0} {1}", this.ammoCountPerCharge, this.ammoDef.label), "Stat_Thing_ReloadPerCharge_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
            }
        }
        if (this.destroyOnEmpty)
        {
            yield return new StatDrawEntry(StatCategoryDefOf.Apparel, "Stat_Thing_ReloadDestroyOnEmpty_Name".Translate(this.ChargeNounArgument), "Yes".Translate(), "Stat_Thing_ReloadDestroyOnEmpty_Desc".Translate(this.ChargeNounArgument), 2749, null, null, false);
        }
        yield break;
        yield break;
    }
}
