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

        /*
         * Note: This is the required attunement per level from Building_Pylon.xml
            <requiredAttunementPerPsylinkLevel>
                <li>100</li>
                <li>150</li>
                <li>200</li>
                <li>250</li>
                <li>300</li>
                <li>350</li>
            </requiredAttunementPerPsylinkLevel>
         * */

        // Random object to generate Dark Matter
        Random rnd = new Random();

        // A List of pawns that can link to this building
        private List<Pawn> pawnsThatCanPsylink = new List<Pawn>();

        // Cached pawn for seeing when to fire off a letter telling player that a psylink upgrade is available
        private Pawn pawnWithLowestPsylink;

        // How far away Pawns can be to meditate
        public const float MaxDistance = 3.9f;

        // How many meditation ticks must be done before spawning a Dark Matter
        private readonly int DarkMatterTicksRequired = 45000;

        // Maximum about of stored attunement;
        private readonly int MAX_ATTUNEMENT = 1000;

        // A linearlly increasing multipler that increases the amount of attunement needed before the next Psylink upgrade can be given
        // private float attunementMultiplerPerGrantedLink = 0.0f;

        // Current attunement created by meditating Naki
        public float currentAttunement = 0.0f;

        // How many ticks of meditation has been done in 1 RW day. This is important so that meditation has a soft upper cap on what can be achieved in 1 day
        private int meditationTicksToday;

        // If this pylon has spawned dark matter today
        private bool hasSpawnedDM = false;

        // Range of dark matter to spawn
        private readonly int minDMSpawn = 3;
        private readonly int maxDMSpawn = 6; 

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
        //private IEnumerable<Pawn> GetPawnsThatCanPsylink(int level = -1)
        //{
        //    // Log.Message($"[Naki HAR] GetPawnsThatCanPsylink level: {level.ToString()}");
        //    return from p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
        //           // where this.Props.requiredFocus.CanPawnUse(p) && p.IsNaki() && currentAttunement >= this.Props.requiredAttunementPerPsylinkLevel[level - 1] && (level == -1 || p.GetPsylinkLevel() == level)
        //           where this.Props.requiredFocus.CanPawnUse(p) && p.IsNaki() && (level == -1 || p.GetPsylinkLevel() == level)
        //           select p;
        //}
        private IEnumerable<Pawn> GetPawnsThatCanPsylink()
        {
            // Problem: I dunno how to get the attunement required for the next level of psycast by indexing by current psylink level.
            // MAX_ATTUNEMENT is 1000
            // p.GetPsylinkLevel() < 6 means that the pawn is still at level 5
            //where p.IsNaki() && p.GetPsylinkLevel() < 6 && this.Props.requiredFocus.CanPawnUse(p) && (currentAttunement >= this.Props.requiredAttunementPerPsylinkLevel[p.GetPsylinkLevel()])
            return from p in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists
                   where p.IsNaki() && p.GetPsylinkLevel() != p.GetMaxPsylinkLevel() && this.Props.requiredFocus.CanPawnUse(p) && (canUpgradeAtPylon(p))
                   select p;
        }

        // Literally only reason this func exists is to prevent a deindexing of this.Props.requiredAttunementPerPsylinkLevel from going over 5
        // This can happen if you're running mods like VPE
        private bool canUpgradeAtPylon(Pawn p)
        {
            int currentLinkLevel = p.GetPsylinkLevel();
            int maximumPsylinkLevel = this.Props.requiredAttunementPerPsylinkLevel.Count();
            // If we are trying to get a psylink upgrade that goes over level 6 (aka index 5)
            if (currentLinkLevel + 1 > maximumPsylinkLevel)
            {
                Log.Warning("[Naki HAR] Attempt to create Naki psylink over level 6");
                return false;
            } else
            {
                return currentAttunement >= this.Props.requiredAttunementPerPsylinkLevel[currentLinkLevel];
            }
        }

        private void OnAttunementFill()
        {
            bool canUpgradePsylink = false;
            foreach (Pawn item in this.GetPawnsThatCanPsylink())
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
                Find.LetterStack.ReceiveLetter(this.Props.enoughAttunementLetterLabel, this.Props.enoughAttunementLetterText.Formatted(this.currentAttunement, text.TrimEndNewlines()), LetterDefOf.NeutralEvent, new LookTargets(this.GetPawnsThatCanPsylink()), null, null, null, null);
            }
            this.pawnsThatCanPsylink.Clear();
            this.pawnsThatCanPsylink.AddRange(this.GetPawnsThatCanPsylink());
            this.pawnWithLowestPsylink = findLowestPsylinkLevel();
        }

        // Private helper function that iterates though pawnsThatCanPsylink and returns the lowest level in there
        private Pawn findLowestPsylinkLevel()
        {
            Pawn weakestPawn = null;
            int lowestLink = -1;
            foreach (Pawn p in pawnsThatCanPsylink)
            {
                if (p.GetPsylinkLevel() > lowestLink)
                {
                    lowestLink = p.GetPsylinkLevel();
                    weakestPawn = p;
                }
            }
            return weakestPawn;
        }

        // Override to reset the number of meditations ticks done today to 0
        public override void CompTick()
        {
            base.CompTick();
            // Log.Message("GenLocalDate.DayTick(this.parent.Map)");
            if (GenLocalDate.DayTick(this.parent.Map) < 2000 && hasSpawnedDM)
            {
                Log.Message("Resetting meditation ticks for DM spawning");
                this.meditationTicksToday = 0;
                hasSpawnedDM = false;
            }
        }

        // Private helper method to see how much attunement is needed for that pawn's current psylink level
        private float GetRequiredAttunement(Pawn p)
        {
            int currentLevel = p.GetPsylinkLevel();
            if (currentLevel == -1)
            {
                // This Naki has no Psylink! This should not happen but let's log it
                Log.Error("[Naki HAR] GetRequiredAttunement: This Naki has no psylink level");
                return -1;
            } else if (currentLevel > 5)
            {
                // Pawn is at max level
                return -1;
            }
            // Log.Message($"Pawn Psylink Level: {currentLevel}");
            return this.Props.requiredAttunementPerPsylinkLevel[currentLevel];
        }


        public AcceptanceReport CanPsylink(Pawn pawn, LocalTargetInfo? knownSpot = null, bool checkSpot = true)
        {
            if (pawn.Dead || pawn.Faction != Faction.OfPlayer)
            {
                // Log.Message($"[Naki HAR] Acceptance report not accepted. Reason: pawn is dead or not player faction");
                return false;
            }

            if (pawn.GetPsylinkLevel() >= pawn.GetMaxPsylinkLevel())
            {
                // Log.Message($"[Naki HAR] Acceptance report not accepted. Reason: pawn at max psylink");
                return new AcceptanceReport("BeginLinkingRitualMaxPsylinkLevel".Translate());
            }

            float requiredAttunement = this.GetRequiredAttunement(pawn);
            // Error checks and attunement checks
            if (requiredAttunement == -1)
            {
                return false;
            } else if (requiredAttunement > this.currentAttunement)
            {
                // Log.Message($"[Naki HAR] Acceptance report not accepted. Reason: required attunement insufficient. Required: {requiredAttunement} Has: {this.currentAttunement}");
                // return new AcceptanceReport($"Cannot Psylink: Insufficient attunement. Current attunement: {this.currentAttunement}. Pawn {pawn.Name} requires {requiredAttunement} to upgrade.");
                return new AcceptanceReport("BeginLinkingRitualInsufficientAttunement".Translate(this.currentAttunement.ToString(), pawn.Name.ToString(), requiredAttunement.ToString()));
            }

            if (!this.Props.requiredFocus.CanPawnUse(pawn))
            {
                // Log.Message($"[Naki HAR] Acceptance report not accepted. Reason: pawn cannot use this Focus");
                return new AcceptanceReport("BeginLinkingRitualNeedFocus".Translate(this.Props.requiredFocus.label));
            }
            
            if (!pawn.Map.reservationManager.CanReserve(pawn, this.parent, 1, -1, null, false))
            {
                // Log.Message($"[Naki HAR] Acceptance report not accepted. Reason: pawn cannot reserve pylon on this map");
                Pawn pawn2 = pawn.Map.reservationManager.FirstRespectedReserver(this.parent, pawn);
                return new AcceptanceReport((pawn2 == null) ? "Reserved".Translate() : "ReservedBy".Translate(pawn.LabelShort, pawn2));
            }
            // This pylon has enough attunement to begin a psylink ritual
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
            AcceptanceReport acceptanceReport = this.CanPsylink(pawn, null, true); // See if the pawn that is selected and that I am rightclicking on the Pylon can psylink
            if (!acceptanceReport.Accepted && !string.IsNullOrWhiteSpace(acceptanceReport.Reason))
            {
                text = text + ": " + acceptanceReport.Reason;
            }
            yield return new FloatMenuOption(text, delegate ()
            {
                Precept_Ritual precept_Ritual = null;
                for (int i = 0; i < pawn.Ideo.PreceptsListForReading.Count; i++)
                {
                    // Log.Message(pawn.Ideo.PreceptsListForReading[i].def.defName); Was trying to find out what precepts pawns have
                    if (pawn.Ideo.PreceptsListForReading[i].def == Naki_Defof.NakiPylonLinking)
                    {
                        precept_Ritual = (Precept_Ritual)pawn.Ideo.PreceptsListForReading[i];
                        break;
                    }
                }
                Log.Message($"[Naki HAR] Precept ritual created: {precept_Ritual.def.defName}");
                // Log.Message($"[Naki HAR] this.parent: {this.parent.def.defName}");
                if (precept_Ritual != null)
                {
                    Find.WindowStack.Add(precept_Ritual.GetRitualBeginWindow(this.parent, null, null, null, null, pawn));
                } else
                {
                    Log.Error("[Naki HAR] Precept ritual is null!");
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
        public void FinishLinkingRitual(Pawn pawn)
        {
            Log.Message($"[Naki HAR] Linking Ritual finished, upgrading psylink for {pawn.Name}");
            if (!ModLister.CheckRoyalty("Psylinkable"))
            {
                return;
            }
            FleckMaker.Static(this.parent.Position, pawn.Map, FleckDefOf.PsycastAreaEffect, 10f);
            SoundDefOf.PsycastPsychicPulse.PlayOneShot(new TargetInfo(this.parent));

            // Set current attunement to remove the amount of attunement needed for that level
            int currentPawnPsylinkLevel = pawn.GetPsylinkLevel();
            this.currentAttunement = this.currentAttunement - this.Props.requiredAttunementPerPsylinkLevel[currentPawnPsylinkLevel + 1]; // Should never go out of index range cause by the time we get here the pawn cannot psylink past level 6

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
            // Just add the progress for now, no bonuses or detractors. Technically right now multiple pawns means a bonus to progress made because multiple delegates for adding progress are made. 
            // But don't add any more if the current attunement goes beyond MAX_ATTUNEMENT
            if (this.currentAttunement < MAX_ATTUNEMENT)
            {
                this.currentAttunement += progress; 
            }
            this.meditationTicksToday++;
            this.TryAttuneOrSpawn();
        }

        // Custom code to handle attunement fill procedure
        private void TryAttuneOrSpawn()
        {
            // If the amount of meditation today has exceeded the minimum needed to spawn 3 dark matter
            if (meditationTicksToday > DarkMatterTicksRequired && !hasSpawnedDM)
            {
                TrySpawnDarkMatter();
            }
            // If the current attunement has exceeded the minimum needed to try to give a psylink at level 2
            if (currentAttunement >= Props.requiredAttunementPerPsylinkLevel.Min())
            {
                OnAttunementFill();
            }
        }

        // Private helper function 
        private void TrySpawnDarkMatter()
        {
            Thing thing = ThingMaker.MakeThing(Naki_Defof.DarkMatter, null);
            thing.stackCount = rnd.Next(minDMSpawn, maxDMSpawn);
            GenPlace.TryPlaceThing(thing, this.parent.InteractionCell, this.parent.Map, ThingPlaceMode.Near, null, (IntVec3 p) => p != this.parent.Position && p != this.parent.InteractionCell, default(Rot4));
            this.hasSpawnedDM = true;
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
                defaultLabel = "DEV: Add 50 attunement",
                action = delegate ()
                {
                    this.AddProgress(50f, true);
                }
            };
            yield break;
        }

        public override string CompInspectStringExtra()
        {
            double percentToDM = (((double)this.meditationTicksToday / (double)DarkMatterTicksRequired))*100;
            if (!hasSpawnedDM && percentToDM < 1.0f)
            {
                return "TotalMeditationToday".Translate((this.meditationTicksToday / 2500).ToString() + "LetterHour".Translate(), this.ProgressMultiplier.ToStringPercent()) + "\n"
                + "Current Attunement: " + this.currentAttunement.ToString("#.#") + "\n"
                + "Progress to dark matter creation: 0%";
            } else if ((!hasSpawnedDM && percentToDM > 1.0f)) {
                return "TotalMeditationToday".Translate((this.meditationTicksToday / 2500).ToString() + "LetterHour".Translate(), this.ProgressMultiplier.ToStringPercent()) + "\n"
                + "Current Attunement: " + this.currentAttunement.ToString("#.#") + "\n"
                + "Progress to dark matter creation: " + percentToDM.ToString("##.#") + "%";
            }
            else
            {
                return "TotalMeditationToday".Translate((this.meditationTicksToday / 2500).ToString() + "LetterHour".Translate(), this.ProgressMultiplier.ToStringPercent()) + "\n"
                + "Current Attunement: " + this.currentAttunement.ToString("#.#") + "\n"
                + "Progress to dark matter creation: Complete";
            }
            
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
            Scribe_Values.Look<bool>(ref this.hasSpawnedDM, "hasSpawnedDM", false, false);
            Scribe_Values.Look<int>(ref this.meditationTicksToday, "meditationTicksToday", 0, false);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                this.pawnsThatCanPsylink.RemoveAll((Pawn x) => x == null);
            }
        }
    }
}
