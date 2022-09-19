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

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Gizmo_NakiWeaponAmmoCounter
            {
                compNakiWeapons = this
            };
        }

        public Gizmo_NakiWeaponAmmoCounter GetANakiWeaponAmmoCounter()
        {
            return new Gizmo_NakiWeaponAmmoCounter();
        }
    }
}
