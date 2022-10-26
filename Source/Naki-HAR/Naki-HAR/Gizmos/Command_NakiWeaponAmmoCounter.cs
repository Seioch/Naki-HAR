using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using HarmonyLib;
using UnityEngine;

namespace Naki_HAR
{
    [StaticConstructorOnStartup]
    public class Command_NakiWeaponAmmoCounter : Command
    {
        //public static HarmonyMethod Use => new HarmonyMethod(typeof(Gizmo_NakiWeaponAmmoCounter), nameof(VerbTrackerAmmo_Postfix);
        public CompNakiWeapons compNakiWeapons;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        public Command_NakiWeaponAmmoCounter()
        {
            this.Order = -100f;
        }

        public Command_NakiWeaponAmmoCounter(CompNakiWeapons c)
        {
            this.compNakiWeapons = c;
            this.Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, this.GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(1523289473, overRect, WindowLayer.GameUI, delegate
            {
                Rect rect2;
                Rect rect = rect2 = overRect.AtZero().ContractedBy(6f);
                rect2.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(rect2, this.compNakiWeapons.Props.ammoGizmoLabel);
                Rect rect3 = rect;
                rect3.yMin = overRect.height / 2f;
                float fillPercent = 1.0f - ((float)this.compNakiWeapons.currentShots / (float)this.compNakiWeapons.Props.maximumShots); // How much shots used out of the maximumShots
                Widgets.FillableBar(rect3, fillPercent, Command_NakiWeaponAmmoCounter.FullBarTex, Command_NakiWeaponAmmoCounter.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                int numShotsRemaining = this.compNakiWeapons.Props.maximumShots - this.compNakiWeapons.currentShots;
                Widgets.Label(rect3, numShotsRemaining.ToString("F0") + " / " + this.compNakiWeapons.Props.maximumShots.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f, null);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
