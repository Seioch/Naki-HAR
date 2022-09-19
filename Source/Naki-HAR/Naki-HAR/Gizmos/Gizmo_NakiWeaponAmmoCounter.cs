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
    public class Gizmo_NakiWeaponAmmoCounter : Gizmo
    {
        //public static HarmonyMethod Use => new HarmonyMethod(typeof(Gizmo_NakiWeaponAmmoCounter), nameof(VerbTrackerAmmo_Postfix);
        public CompNakiWeapons compNakiWeapons;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));
        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        public Gizmo_NakiWeaponAmmoCounter()
        {
            this.order = -100f;
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
                float fillPercent = this.compNakiWeapons.currentShots / this.compNakiWeapons.Props.maximumShots;
                Widgets.FillableBar(rect3, fillPercent, Gizmo_NakiWeaponAmmoCounter.FullBarTex, Gizmo_NakiWeaponAmmoCounter.EmptyBarTex, false);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect3, this.compNakiWeapons.currentShots.ToString("F0") + " / " + this.compNakiWeapons.Props.maximumShots.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
            }, true, false, 1f, null);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
