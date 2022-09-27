using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace Naki_HAR
{
    class CompNakiPsylinkable : ThingComp
    {
        private List<Pawn> pawnsThatCanPsylink = new List<Pawn>();

        public const float MaxDistance = 3.9f;

        private const float attunementMultiplerPerGrantedLink = 0.0f;

        public float currentAttunement = 0.0f;

        public CompProperties_NakiPsylinkable Props
        {
            get
            {
                return (CompProperties_NakiPsylinkable)this.props;
            }
        }

        // The only pawns who can get a Naki Psylink are Naki. This checks all pawns on the Map, in Caravans, and in Pods if they can use Naki Meditations, if this Pylon's attunement is greater than what they need at that level
        // and that the pawn's current psylink level is at the level we are checking it at.
        // Note that level is being passed in as a value from 1 to 6, but the array requiredAttunementPerPsylinkLevel in Props is zero indexed, so you have to decrement level by 1
        private IEnumerable<Pawn> GetPawnsThatCanPsylink(int level = -1)
        {
            return from p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
                   where this.Props.requiredFocus.CanPawnUse(p) && p.IsNaki() && currentAttunement >= this.Props.requiredAttunementPerPsylinkLevel[level - 1] && (level == -1 || p.GetPsylinkLevel() == level)
                   select p;
        }

        private void OnAttunementFill()
        {
            bool canUpgradePsylink = false;
            foreach (Pawn item in this.GetPawnsThatCanPsylink(-1))
            {
                if (!this.pawnsThatCanPsylink.Contains(item))
                {
                    canUpgradePsylink = true;
                    break;
                }
            }
            // Once the attunement meter is filled
            if (canUpgradePsylink)
            {
                string text = "";
                IEnumerable<string> enumerable = from p in this.GetPawnsThatCanPsylink()
                                                    select p.LabelShort;
                // Find the pawns that can upgrade and get their levels
                // IEnumerable < Pawn > psylinkablePawns = this.GetPawnsThatCanPsylink();

                // Iterate through all 6 possible Naki Psylink levels to generate text per pawn who can psylink
                for (int i = 0; i < this.Props.requiredAttunementPerPsylinkLevel.Count; i++)
                {
                    if (enumerable.Count<string>() > 0)
                    {
                        text = string.Concat(new object[]
                        {
                            text,
                            "- " + "Level".Translate().CapitalizeFirst() + " ",
                            i + 1,
                            ": ",
                            this.Props.requiredAttunementPerPsylinkLevel[i],
                            " ",
                            this.Props.attunementFlavorText,
                            " (",
                            enumerable.ToCommaList(false, false),
                            ")\n"
                        });
                    }
                }
                Find.LetterStack.ReceiveLetter(this.Props.enoughAttunementLetterLabel, this.Props.enoughAttunementLetterText.Formatted(this.currentAttunement, text.TrimEndNewlines()), LetterDefOf.NeutralEvent, new LookTargets(this.GetPawnsThatCanPsylink(-1)), null, null, null, null);
            }
            this.pawnsThatCanPsylink.Clear();
            this.pawnsThatCanPsylink.AddRange(this.GetPawnsThatCanPsylink(-1));
        }

        // Private helper method to see how much attunement is needed for that pawn's current psylink level
        private float GetRequiredAttunement(Pawn p)
        {
            int currentLevel = p.GetPsylinkLevel();
            if (currentLevel == -1)
            {
                // This Naki has no Psylink! This should not happen but let's log it
                Log.Warning("[Naki HAR] GetRequiredAttunement: This Naki has no psylink level");
                return -1;
            }
            return this.Props.requiredAttunementPerPsylinkLevel[currentLevel - 1];
        }

        public AcceptanceReport CanPsylink(Pawn pawn, LocalTargetInfo? knownSpot = null, bool checkSpot = true)
        {
            if (pawn.Dead || pawn.Faction != Faction.OfPlayer)
            {
                return false;
            }

            float requiredAttunement = this.GetRequiredAttunement(pawn);
            if (requiredAttunement == -1)
            {
                return false;
            }

            if (!this.Props.requiredFocus.CanPawnUse(pawn))
            {
                return new AcceptanceReport("BeginLinkingRitualNeedFocus".Translate(this.Props.requiredFocus.label));
            }
            if (pawn.GetPsylinkLevel() >= pawn.GetMaxPsylinkLevel())
            {
                return new AcceptanceReport("BeginLinkingRitualMaxPsylinkLevel".Translate());
            }
            if (!pawn.Map.reservationManager.CanReserve(pawn, this.parent, 1, -1, null, false))
            {
                Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(this.parent, pawn);
                return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
            }
            // This pylon has enough attunement to begin a psylink ritual
            if (this.currentAttunement < requiredAttunement)
            {
                // Acceptance Report reports the required attunement for that Pawn, a bit of flavor text for it, and the current attunement on the Pylon itself
                return new AcceptanceReport("BeginNakiLinkingRitual".Translate(requiredAttunement.ToString(), this.Props.attunementFlavorText, this.currentAttunement.ToString()));
            }
            if (checkSpot)
            {
                LocalTargetInfo localTargetInfo;
                if (knownSpot != null)
                {
                    if (!this.CanUseSpot(pawn, knownSpot.Value))
                    {
                        return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
                    }
                }
                else if (!this.TryFindLinkSpot(pawn, out localTargetInfo))
                {
                    return new AcceptanceReport("BeginLinkingRitualNeedLinkSpot".Translate());
                }
            }
            return AcceptanceReport.WasAccepted;
        }

        // Taken from CompPsylinkable
        // Finds a spot near the Pylon for a pawn to move towards, stand there, and begin the linking ritual
        public bool TryFindLinkSpot(Pawn pawn, out LocalTargetInfo spot)
        {
            spot = MeditationUtility.FindMeditationSpot(pawn).spot;
            if (this.CanUseSpot(pawn, spot))
            {
                return true;
            }
            int num = GenRadial.NumCellsInRadius(2.9f);
            int num2 = GenRadial.NumCellsInRadius(3.9f);
            for (int i = num; i < num2; i++)
            {
                IntVec3 c = this.parent.Position + GenRadial.RadialPattern[i];
                if (this.CanUseSpot(pawn, c))
                {
                    spot = c;
                    return true;
                }
            }
            spot = IntVec3.Zero;
            return false;
        }

        // Taken from CompPsylinkable
        private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
        {
            IntVec3 cell = spot.Cell;
            return cell.DistanceTo(this.parent.Position) <= 3.9f && cell.Standable(this.parent.Map) && GenSight.LineOfSight(cell, this.parent.Position, this.parent.Map, false, null, 0, 0) && pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
        }
    }
}
