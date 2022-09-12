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
    public class Verb_NumberedShoot : Verb_Shoot
    {

        protected override bool TryCastShot()
        {
            CompNakiWeapons nakiWeaponComp = base.EquipmentSource.GetComp<CompNakiWeapons>();
            // CompNakiWeapons nakiWeaponComp = base.EquipmentSource;
            if (base.TryCastShot())
            {
                if (this.burstShotsLeft <= 1)
                {
                    // Increase the number of shots taken by 1
                    nakiWeaponComp.currentShots += 1;
                    // If we have hit the maximum number of shots for the weapon
                    if (nakiWeaponComp.currentShots == nakiWeaponComp.Props.maximumShots)
                    {
                        this.SelfConsume();
                    }
                }
                return true;
            }
            if (this.burstShotsLeft < this.verbProps.burstShotCount)
            {
                // Increase the number of shots taken by 1
                nakiWeaponComp.currentShots += 1;
                // If we have hit the maximum number of shots for the weapon
                if (nakiWeaponComp.currentShots == nakiWeaponComp.Props.maximumShots)
                {
                    this.SelfConsume();
                }
            }
            return false;
        }

        // Token: 0x06008489 RID: 33929 RVA: 0x002F6C20 File Offset: 0x002F4E20
        public override void Notify_EquipmentLost()
        {
            base.Notify_EquipmentLost();
            if (this.state == VerbState.Bursting && this.burstShotsLeft < this.verbProps.burstShotCount)
            {
                this.SelfConsume();
            }
        }

        // Destroy the weapon when the number of shots taken is maximized
        private void SelfConsume()
        {
            if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
            {
                
                base.EquipmentSource.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
