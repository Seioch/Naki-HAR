using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Naki_HAR
{
    // Token: 0x020011BE RID: 4542
    public class CompProperties_NakiPsylinkable : CompProperties
    {
        // Token: 0x06006E83 RID: 28291 RVA: 0x00256C55 File Offset: 0x00254E55
        public CompProperties_NakiPsylinkable()
        {
            this.compClass = typeof(CompNakiPsylinkable);
        }

        // Token: 0x04003D3F RID: 15679
        public int requiredAttunementForPsylink;

        // Token: 0x04003D40 RID: 15680
        public MeditationFocusDef requiredFocus;

        // Token: 0x04003D41 RID: 15681
        public SoundDef linkSound;

        // Token: 0x04003D42 RID: 15682
        public string enoughAttunementLetterLabel;

        // Token: 0x04003D43 RID: 15683
        public string enoughAttunementLetterText;
    }
}
