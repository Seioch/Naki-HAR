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

        // Exposes data
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref currentShots, "currentShots", 0);
        }

        // Returns a new Command_NakiWeaponAmmoCounter, but doable as an IEnumberable
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_NakiWeaponAmmoCounter()
            {
                compNakiWeapons = this
            };
        }

        // Returns a new instanec of a Naki Weapon Command, and attaches this Comp to it so the Command can get this comp's live data
        public Command GetNakiWeaponCommand()
        {
            return new Command_NakiWeaponAmmoCounter(this);
        }
    }
}
