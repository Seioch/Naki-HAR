using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace Naki_HAR
{
    // A literal one-off class that checks if the pawn can use Naki pylons
    // Why? Who knows. 
    class RitualSpectatorFilter_NakiFocus : RitualSpectatorFilter
    {
        public override bool Allowed(Pawn p)
        {
            // return MeditationFocusDefOf.Natural.CanPawnUse(p);
            return Naki_Defof.NakiFocus.CanPawnUse(p);
        }
    }
}
