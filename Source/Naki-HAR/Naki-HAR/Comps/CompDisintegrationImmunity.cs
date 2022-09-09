using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    public class CompDisintegrationImmunity : ThingComp
    {
        //This is just an empty Comp. The new Hediff_AcidBuildup checks if the creature has it, and doesn't apply damage if so


        public CompProperties_DisintegrationImmunity Props
        {
            get
            {
                return (CompProperties_DisintegrationImmunity)this.props;
            }
        }
    }
}
