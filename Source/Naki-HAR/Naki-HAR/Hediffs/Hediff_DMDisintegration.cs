using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    public class Hediff_DMDisintegration : HediffWithComps
    {
        public int tickMax = 64; // How often to fire off the Damage
        public int tickCounter = 0; // Counter between ticks
        
        public override void Tick()
        {
            // Naki cannot have DM Disintegration hediff
            if (pawn.IsNaki())
            {
                return;
            }
            base.Tick();
            tickCounter++;
            if (tickCounter > tickMax)
            {
                CompDisintegrationImmunity comp = pawn.TryGetComp<CompDisintegrationImmunity>();
                if (comp != null)
                {
                    tickCounter = 0;
                }
                else
                {
                    pawn.TakeDamage(new DamageInfo(DefDatabase<DamageDef>.GetNamed("Dark_Matter_Burn"), 1f, 0f, -1f, null, null, null, DamageInfo.SourceCategory.ThingOrUnknown, null));
                    tickCounter = 0;

                }

            }
            
        }

    }
}
