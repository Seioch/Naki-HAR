using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace Naki_HAR
{
    [StaticConstructorOnStartup]
    public class CompNakiWeapons : ThingComp
    {
        public CompProperties_CompNakiWeapons Props => (CompProperties_CompNakiWeapons)props;
        public int currentShots = 0;

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentShots, "currentShots", 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_NakiWeaponAmmoCounter()
            {
                compNakiWeapons = this
            };
        }

        public Command GetNakiWeaponCommand()
        {
            return new Command_NakiWeaponAmmoCounter(this);
        }
    }
}
