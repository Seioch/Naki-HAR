using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    // For temporary Things that are spawned in game, check after a while if it should destroy itself. 
    public class CompSpawnedBuilding : ThingComp
    {
        public int finalTick = -1;
        public int damagePerTick = 0;
        public int lastDamageTick = 0;


        public override void CompTick()
        {
            base.CompTick();
            this.TickCheck();
        }

        public override void CompTickRare()
        {
            base.CompTickRare();
            this.TickCheck();
        }

        public override void CompTickLong()
        {
            base.CompTickLong();
            this.TickCheck();
        }

        private void TickCheck()
        {
            int ticksGame = Find.TickManager.TicksGame;
            bool destroy = false;


            if (this.damagePerTick > 0 && this.lastDamageTick < ticksGame)
            {
                this.parent.HitPoints -= this.damagePerTick * (ticksGame - this.lastDamageTick);
                this.lastDamageTick = ticksGame;

                if (this.parent.HitPoints <= 0)
                    destroy = true;
            }

            // for logging final tick of damage
            if (this.finalTick > 0)
            {
                if (this.finalTick < ticksGame)
                {
                    destroy = true;
                }
            }
            
            if (destroy)
            {
                this.parent.Destroy();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_Values.Look(ref this.finalTick, nameof(this.finalTick));
            Scribe_Values.Look(ref this.damagePerTick, nameof(this.damagePerTick));
            Scribe_Values.Look(ref this.lastDamageTick, nameof(this.lastDamageTick));
        }
    }
}
