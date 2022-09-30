using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace Naki_HAR
{
    class CompNakiPsylinkable : ThingComp
    {
        // A List of pawns that can link to this building
        private List<Pawn> pawnsThatCanPsylink = new List<Pawn>();

        // How far away Pawns can be to meditate
        public const float MaxDistance = 3.9f;

        // How many meditation ticks must be done before spawning a Dark Matter
        public const int DarkMatterTicksRequired = 30000;

        // A linearlly increasing multipler that increases the amount of attunement needed before the next Psylink upgrade can be given
        private float attunementMultiplerPerGrantedLink = 0.0f;

        // Current attunement created by meditating Naki
        public float currentAttunement = 0.0f;

        // How many ticks of meditation has been done in 1 RW day. This is important so that meditation has a soft upper cap on what can be achieved in 1 day
        private int meditationTicksToday;

        // Private list of meditation penalties if you are spending X number of ticks mediating at this Pylon
        private static readonly List<Pair<int, float>> TicksToProgressMultipliers = new List<Pair<int, float>>
        {
            new Pair<int, float>(30000, 1f),
            new Pair<int, float>(60000, 0.5f),
            new Pair<int, float>(120000, 0.25f),
            new Pair<int, float>(240000, 0.15f)
        };

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
            // Log.Message($"[Naki HAR] GetPawnsThatCanPsylink level: {level.ToString()}");
            return from p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
                   // where this.Props.requiredFocus.CanPawnUse(p) && p.IsNaki() && currentAttunement >= this.Props.requiredAttunementPerPsylinkLevel[level - 1] && (level == -1 || p.GetPsylinkLevel() == level)
                   where this.Props.requiredFocus.CanPawnUse(p) && p.IsNaki() && (level == -1 || p.GetPsylinkLevel() == level)
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
                Log.Message($"[Naki HAR] Found {enumerable.Count()} pawns that can psylink here.");

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

        // Override to reset the number of meditations ticks done today to 0
        public override void CompTickLong()
        {
            if (GenLocalDate.DayTick(this.parent.Map) < 2000)
            {
                this.meditationTicksToday = 0;
            }
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
        // Finds a spot that the pawn AI will navigate the pawn to
        private bool CanUseSpot(Pawn pawn, LocalTargetInfo spot)
        {
            IntVec3 cell = spot.Cell;
            return cell.DistanceTo(this.parent.Position) <= 3.9f && cell.Standable(this.parent.Map) && GenSight.LineOfSight(cell, this.parent.Position, this.parent.Map, false, null, 0, 0) && pawn.CanReach(spot, PathEndMode.OnCell, Danger.Deadly, false, false, TraverseMode.ByPawn);
        }

        // Creates a new floating menu for Pawns to 
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (pawn.Dead || pawn.Drafted)
            {
                yield break;
            }
            string text = "BeginLinkingRitualFloatMenu".Translate();
            AcceptanceReport acceptanceReport = this.CanPsylink(pawn, null, true);
            if (!acceptanceReport.Accepted && !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
            {
                text = text + ": " + acceptanceReport.Reason;
            }
            yield return new FloatMenuOption(text, delegate ()
            {
                Precept_Ritual precept_Ritual = null;
                for (int i = 0; i < pawn.Ideo.PreceptsListForReading.Count; i++)
                {
                    if (pawn.Ideo.PreceptsListForReading[i].def == PreceptDefOf.AnimaTreeLinking)
                    {
                        precept_Ritual = (Precept_Ritual)pawn.Ideo.PreceptsListForReading[i];
                        break;
                    }
                }
                if (precept_Ritual != null)
                {
                    Find.WindowStack.Add(precept_Ritual.GetRitualBeginWindow(this.parent, null, null, null, null, pawn));
                }
            }, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0)
            {
                Disabled = !acceptanceReport.Accepted
            };
            yield break;
        }

        // Taken from CompPsylinkable
        // Difference is that we are not going to despawn plants anymore, we are actually going to set the current
        // attunement to 0. 
        public void FinishLinkingRitual(Pawn pawn, int plantsToKeep)
        {
            if (!ModLister.CheckRoyalty("Psylinkable"))
            {
                return;
            }
            FleckMaker.Static(this.parent.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 10f);
            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(this.parent));
            CompSpawnSubplant compSpawnSubplant = this.parent.TryGetComp<CompSpawnSubplant>();

            // Set current attunement to 0
            this.currentAttunement = 0;

            pawn.ChangePsylinkLevel(1, true);
            Find.History.Notify_PsylinkAvailable();
        }

        // Taken from CompSpawnSubplant
        // Adds 100 attunement for debugging purposes if debugCall is true
        // Otherwise adds a small amount of attunement PER TICK. See JobDriver_Attune for how much is added per tick! It is not 1f!
        public void AddProgress(float progress, bool multiplier)
        {
            if (!multiplier)
            {
                progress *= this.ProgressMultiplier;
            }
            //this.currentAttunement += progress * (1f + this.parent.GetStatValue(StatDefOf.MeditationPlantGrowthOffset, true));
            this.currentAttunement += progress; // Just add the progress for now, no bonuses or detractors. Technically right now multiple pawns means a bonus to progress made because multiple delegates for adding progress are made. 
            this.meditationTicksToday++;
            this.TryAttunementComplete();
        }

        // Custom code to handle attunement fill procedure
        private void TryAttunementComplete()
        {
            if (meditationTicksToday > DarkMatterTicksRequired)
            {
                // Spawn a Dark matter under the pylon
                OnAttunementFill();
            }
        }

        // Taken from CompSpawnSubplant
        // in devmode this immediately sets the progress up to 100%. In our situation it adds 100 attunement
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!Prefs.DevMode)
            {
                yield break;
            }
            yield return new Command_Action
            {
                defaultLabel = "DEV: Add 100 attunement",
                action = delegate ()
                {
                    this.AddProgress(1f, true);
                }
            };
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            return "TotalMeditationToday".Translate((this.meditationTicksToday / 2500).ToString() + "LetterHour".Translate(), 
                this.ProgressMultiplier.ToStringPercent()) + "\n" + "Current Attunement: " + this.currentAttunement.ToString();
        }

        // Taken from CompSpawnSubplant
        // Checks the amount of ticks meditated today, and returns the attunement gain penalty (a multipler)
        private float ProgressMultiplier
        {
            get
            {
                foreach (Pair<int, float> pair in TicksToProgressMultipliers)
                {
                    if (this.meditationTicksToday < pair.First)
                    {
                        return pair.Second;
                    }
                }
                return TicksToProgressMultipliers.Last<Pair<int, float>>().Second;
            }
        }

        // Taken from CompPsylinkable
        // Exposes the list of pawns that can psylink (Naki psylinks only)
        // Option to remove all pawns from the psylink list that are null as a cleaning measure
        public override void PostExposeData()
        {
            Scribe_Collections.Look<Pawn>(ref this.pawnsThatCanPsylink, "pawnsThatCanPsylink", LookMode.Reference, Array.Empty<object>());
            Scribe_Values.Look<float>(ref this.currentAttunement, "currentAttunement", 0f, false);
            Scribe_Values.Look<int>(ref this.meditationTicksToday, "meditationTicksToday", 0, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.pawnsThatCanPsylink.RemoveAll((Pawn x) => x == null);
            }
        }
    }
}
