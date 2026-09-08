using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class FirstPlayableMinuteTests
    {
        [Test]
        public void MissingAndInvalidRecordsFallBackWithoutTouchingOtherPreferences()
        {
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            const string sentinelKey = "realmraiders.tests.firstMinute.sentinel";
            var hadSentinel = PlayerPrefs.HasKey(sentinelKey);
            var previousSentinel = PlayerPrefs.GetString(sentinelKey, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests();
                PlayerPrefs.SetString(sentinelKey, "untouched");
                PlayerPrefs.Save();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.NotStarted));
                foreach (var invalid in new[] { string.Empty, "{not json", "{\"schemaVersion\":2,\"status\":\"Active\"}", "{\"schemaVersion\":1}", "{\"schemaVersion\":1,\"status\":\"Unknown\"}" })
                {
                    PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, invalid);
                    Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.NotStarted));
                    Assert.That(PlayerPrefs.GetString(sentinelKey), Is.EqualTo("untouched"));
                }
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.Zero);
            }
            finally
            {
                Restore(FirstPlayableMinute.KeyForTests, hadGuide, previousGuide);
                Restore(sentinelKey, hadSentinel, previousSentinel);
                PlayerPrefs.Save();
                FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        [Test]
        public void StatusTransitionsAreIdempotentAndWriteOnlyOnSuccessfulChanges()
        {
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests();
                Assert.That(FirstPlayableMinute.TryStart(), Is.True);
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active));
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(1));
                Assert.That(FirstPlayableMinute.TryStart(), Is.False);
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(1));
                Assert.That(FirstPlayableMinute.TryComplete(), Is.True);
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(2));
                Assert.That(FirstPlayableMinute.TryComplete(), Is.False);
                Assert.That(FirstPlayableMinute.Skip(), Is.False);
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(2));

                FirstPlayableMinute.ResetForTests();
                Assert.That(FirstPlayableMinute.Skip(), Is.True);
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(FirstPlayableMinute.Skip(), Is.False);
                Assert.That(FirstPlayableMinute.TryStart(), Is.False);
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(1));
            }
            finally
            {
                Restore(FirstPlayableMinute.KeyForTests, hadGuide, previousGuide);
                PlayerPrefs.Save();
                FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        [Test]
        public void BuildEvaluationRequiresChangedValidPlanAndUsesExistingInvalidReason()
        {
            var entryLayout = DefenseLayout.Default();
            var entry = FirstPlayableMinute.CaptureBuildEntry(entryLayout);
            Assert.That(FirstPlayableMinute.EvaluateBuild(entry, Clone(entryLayout), out var reason), Is.EqualTo(BuildGuideStep.Choose));
            Assert.That(FirstPlayableMinute.BuildCopy(BuildGuideStep.Choose, reason), Is.EqualTo("CHANGE ONE DEFENSE — TAP A SLOT"));

            var invalid = Clone(entryLayout);
            invalid.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
            Assert.That(FirstPlayableMinute.EvaluateBuild(entry, invalid, out reason), Is.EqualTo(BuildGuideStep.Fix));
            Assert.That(reason, Is.EqualTo("Place exactly one Ent."));
            Assert.That(FirstPlayableMinute.BuildCopy(BuildGuideStep.Fix, reason), Is.EqualTo("KEEP CHOOSING — Place exactly one Ent."));

            var changedValid = Clone(entryLayout);
            changedValid.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent);
            changedValid.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
            Assert.That(FirstPlayableMinute.EvaluateBuild(entry, changedValid, out reason), Is.EqualTo(BuildGuideStep.Save));
            Assert.That(FirstPlayableMinute.BuildCopy(BuildGuideStep.Save, reason), Is.EqualTo("PLAN READY — SAVE & DEFEND"));
            Assert.That(FirstPlayableMinute.EvaluateBuild(entry, Clone(entryLayout), out _), Is.EqualTo(BuildGuideStep.Choose), "Cycling back to the entry layout is not success.");
        }

        [Test]
        public void SessionHandoffAcceptsOnlyActiveChangedValidBuildAndIsConsumableOnce()
        {
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests();
                FirstPlayableMinute.TryStart();
                var original = DefenseLayout.Default();
                var entry = FirstPlayableMinute.CaptureBuildEntry(original);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, Clone(original)), Is.False);

                var invalid = Clone(original); invalid.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, invalid), Is.False);

                var changedValid = Clone(original); changedValid.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent); changedValid.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changedValid), Is.True);
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.True);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changedValid), Is.False);
                Assert.That(FirstPlayableMinute.ConsumeChangedBuildHandoff(), Is.True);
                Assert.That(FirstPlayableMinute.ConsumeChangedBuildHandoff(), Is.False);

                FirstPlayableMinute.ResetBuildHandoff();
                FirstPlayableMinute.Skip();
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changedValid), Is.False);
            }
            finally
            {
                Restore(FirstPlayableMinute.KeyForTests, hadGuide, previousGuide);
                PlayerPrefs.Save();
                FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        [Test]
        public void OnlyJourneyButtonIsGuideActivationRoute()
        {
            Assert.That(HubHUD.ActivatesGuideForButton("START SYLVAN JOURNEY"), Is.True);
            foreach (var legacy in new[] { "BUILD SYLVAN", "DEFEND SYLVAN", "RAID SYLVAN", "DEFEND INFERNAL", "CHARACTER SANDBOX" })
                Assert.That(HubHUD.ActivatesGuideForButton(legacy), Is.False, legacy);
        }

        [Test]
        public void DefenseProofAdvancesOnlyInOrderAndIgnoresDuplicates()
        {
            var proof = new FirstPlayableMinuteDefenseProof(true);
            Assert.That(proof.Step, Is.EqualTo(DefenseGuideStep.Select));
            Assert.That(proof.TryPossess(), Is.False);
            Assert.That(proof.TrySelect(), Is.True); Assert.That(proof.TrySelect(), Is.False);
            Assert.That(proof.TryMove(), Is.False);
            Assert.That(proof.TryPossess(), Is.True);
            Assert.That(proof.TryAttack(), Is.False);
            Assert.That(proof.TryMove(), Is.True);
            Assert.That(proof.TryDodge(), Is.False);
            Assert.That(proof.TryAttack(), Is.True);
            Assert.That(proof.TryRelease(), Is.False);
            Assert.That(proof.TryDodge(), Is.True);
            Assert.That(proof.TryKeeperReturn(), Is.False);
            Assert.That(proof.TryRelease(), Is.True);
            Assert.That(proof.TryKeeperReturn(), Is.True);
            Assert.That(proof.Step, Is.EqualTo(DefenseGuideStep.Result));
        }

        [Test]
        public void DefenseProofTerminalIsIdempotentAndEarlyResultRequiresRetry()
        {
            var early = new FirstPlayableMinuteDefenseProof(true);
            early.TrySelect(); early.TryPossess();
            Assert.That(early.ReachTerminal(), Is.EqualTo(DefenseGuideTerminalOutcome.Retry));
            Assert.That(early.Step, Is.EqualTo(DefenseGuideStep.Retry));
            Assert.That(early.ReachTerminal(), Is.EqualTo(DefenseGuideTerminalOutcome.None));
            Assert.That(early.ResetForRetry(), Is.True);
            Assert.That(early.Step, Is.EqualTo(DefenseGuideStep.Select));
            early.TrySelect(); early.TryPossess();
            Assert.That(early.Interrupt(false), Is.True); Assert.That(early.Step, Is.EqualTo(DefenseGuideStep.Inactive));
            Assert.That(early.Interrupt(true), Is.True); Assert.That(early.Step, Is.EqualTo(DefenseGuideStep.Select));

            var complete = new FirstPlayableMinuteDefenseProof(true);
            Assert.That(complete.TrySelect() && complete.TryPossess() && complete.TryMove() && complete.TryAttack() && complete.TryDodge() && complete.TryRelease() && complete.TryKeeperReturn(), Is.True);
            Assert.That(complete.ReachTerminal(), Is.EqualTo(DefenseGuideTerminalOutcome.Completed));
            Assert.That(complete.ReachTerminal(), Is.EqualTo(DefenseGuideTerminalOutcome.None));
            Assert.That(complete.ResetForRetry(), Is.False);
        }

        [Test]
        public void DefenseSessionRequiresChangedBuildAndOnlyAuthorizedRetrySurvivesTeardown()
        {
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart();
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.False, "A fresh Active preference is not a defense-session handoff.");
                var original = DefenseLayout.Default(); var entry = FirstPlayableMinute.CaptureBuildEntry(original); var changed = Clone(original);
                changed.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent); changed.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changed), Is.True);
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out var firstSceneToken), Is.True);
                Assert.That(firstSceneToken, Is.Not.Zero);
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.False, "The same scene cannot consume the handoff twice.");
                Assert.That(FirstPlayableMinute.PrepareDefenseRetry(firstSceneToken), Is.True);
                FirstPlayableMinute.EndDefenseScene(firstSceneToken);
                Assert.That(FirstPlayableMinute.RetryAuthorizedForTests, Is.True);
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out var retrySceneToken), Is.True);
                Assert.That(retrySceneToken, Is.Not.EqualTo(firstSceneToken));
                FirstPlayableMinute.EndDefenseScene(firstSceneToken);
                Assert.That(FirstPlayableMinute.DefenseSceneActiveForTests, Is.True, "An older scene cannot end the active retry scene.");
                FirstPlayableMinute.EndDefenseScene(retrySceneToken);
                Assert.That(FirstPlayableMinute.DefenseSessionEligibleForTests, Is.False, "Unannounced teardown ends the session.");

                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changed), Is.True);
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out var staleSceneToken), Is.True);
                FirstPlayableMinute.ResetBuildHandoff();
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changed), Is.True);
                FirstPlayableMinute.EndDefenseScene(staleSceneToken);
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.True, "An older scene cannot clear a newer BUILD handoff.");
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.True);
                FirstPlayableMinute.ResetProcessSessionForTests();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active));
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.False, "Subsystem reset retains the preference but clears transient eligibility.");

                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changed), Is.True);
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.True);
                Assert.That(FirstPlayableMinute.TryComplete(), Is.True);
                Assert.That(FirstPlayableMinute.DefenseSessionEligibleForTests, Is.False);

                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart();
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(entry, changed), Is.True);
                FirstPlayableMinute.ResetBuildHandoff();
                Assert.That(FirstPlayableMinute.TryBeginSylvanDefense(out _), Is.False, "Hub/BUILD re-entry clears the handoff.");
                Assert.That(FirstPlayableMinute.Skip(), Is.True);
                Assert.That(FirstPlayableMinute.DefenseSessionEligibleForTests, Is.False);
            }
            finally
            {
                Restore(FirstPlayableMinute.KeyForTests, hadGuide, previousGuide);
                PlayerPrefs.Save(); FirstPlayableMinute.ResetBuildHandoff();
            }
        }

        static DefenseLayout Clone(DefenseLayout source)
        {
            var slots = new DefenseSlotLayout[source.Slots.Length];
            for (var index = 0; index < slots.Length; index++) slots[index] = source.Slots[index];
            return new DefenseLayout(slots);
        }

        static void Restore(string key, bool hadValue, string value)
        {
            if (hadValue) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key);
        }
    }
}
