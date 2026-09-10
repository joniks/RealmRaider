using System;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace RealmRaiders.Editor
{
    [InitializeOnLoad]
    public static class RealmRaidersTestGate
    {
        const string LogPrefix = "[Realm Raiders QA]";
        const string ActiveKey = "RealmRaiders.TestGate.Active";
        const string ModeKey = "RealmRaiders.TestGate.Mode";
        const string StartedKey = "RealmRaiders.TestGate.Started";
        const string JobIdKey = "RealmRaiders.TestGate.JobId";
        const double StaleRecoveryDelaySeconds = 3d;
        const double StartAcknowledgementDelaySeconds = 3d;

        static TestRunnerApi testRunnerApi;
        static GateCallbacks callbacks;
        static double staleRecoveryDeadline;
        static bool staleRecoveryPending;
        static double startAcknowledgementDeadline;
        static bool startAcknowledgementPending;

        static RealmRaidersTestGate()
        {
            EnsureCallbacksRegistered();
            ScheduleStaleStateRecoveryIfNeeded();
        }

        [MenuItem("Realm Raiders/QA/Run All EditMode Tests", false, 100)]
        public static void RunAllEditModeTests()
        {
            StartRun(TestMode.EditMode);
        }

        [MenuItem("Realm Raiders/QA/Run All PlayMode Tests", false, 101)]
        public static void RunAllPlayModeTests()
        {
            StartRun(TestMode.PlayMode);
        }

        [MenuItem("Realm Raiders/QA/Clear Stale Test Run Ownership", false, 102)]
        public static void ClearStaleTestRunOwnership()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                Debug.Log($"{LogPrefix} No test-run ownership is active.");
                return;
            }

            if (SessionState.GetBool(StartedKey, false))
            {
                Debug.LogWarning($"{LogPrefix} Cannot clear ownership because the owned test run has started.");
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"{LogPrefix} Cannot clear ownership while Unity is compiling, updating assets, or changing Play Mode.");
                return;
            }

            string mode = TryGetOwnedMode(out TestMode ownedMode) ? ModeName(ownedMode) : "unknown-mode";
            ClearOwnedRun();
            Debug.LogWarning($"{LogPrefix} Cleared unacknowledged {mode} test-run ownership manually. QA may invoke menu once.");
        }

        static void StartRun(TestMode mode)
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning($"{LogPrefix} Cannot start {ModeName(mode)} tests while Unity is compiling.");
                return;
            }

            if (EditorApplication.isUpdating)
            {
                Debug.LogWarning($"{LogPrefix} Cannot start {ModeName(mode)} tests while Unity is updating assets.");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning($"{LogPrefix} Cannot start {ModeName(mode)} tests while Unity is entering or inside Play Mode.");
                return;
            }

            if (SessionState.GetBool(ActiveKey, false))
            {
                string activeMode = TryGetOwnedMode(out TestMode ownedMode) ? ModeName(ownedMode) : "unknown-mode";
                Debug.LogWarning($"{LogPrefix} Cannot start {ModeName(mode)} tests because this tool already owns an active {activeMode} run.");
                return;
            }

            EnsureCallbacksRegistered();
            SetOwnedRun(mode);

            try
            {
                var settings = new ExecutionSettings(new Filter { testMode = mode });
                Debug.Log($"{LogPrefix} Starting full unfiltered {ModeName(mode)} test run.");
                string jobId = testRunnerApi.Execute(settings);

                // Execute normally schedules asynchronously. Keep this assignment guarded in case
                // a future Test Framework version completes synchronously and clears ownership first.
                if (SessionState.GetBool(ActiveKey, false))
                {
                    SessionState.SetString(JobIdKey, jobId ?? string.Empty);
                    ScheduleStartAcknowledgementIfNeeded();
                }
            }
            catch (Exception exception)
            {
                ClearOwnedRun();
                Debug.LogError($"{LogPrefix} Failed to start {ModeName(mode)} tests: {exception.Message}");
            }
        }

        static void EnsureCallbacksRegistered()
        {
            if (testRunnerApi != null && callbacks != null) return;

            testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.hideFlags = HideFlags.HideAndDontSave;
            callbacks = new GateCallbacks();
            testRunnerApi.RegisterCallbacks(callbacks);
        }

        static void SetOwnedRun(TestMode mode)
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetInt(ModeKey, (int)mode);
            SessionState.SetBool(StartedKey, false);
            SessionState.EraseString(JobIdKey);
            CancelStaleStateRecovery();
            CancelStartAcknowledgement();
        }

        static void ClearOwnedRun()
        {
            SessionState.SetBool(ActiveKey, false);
            SessionState.EraseInt(ModeKey);
            SessionState.EraseBool(StartedKey);
            SessionState.EraseString(JobIdKey);
            CancelStaleStateRecovery();
            CancelStartAcknowledgement();
        }

        static bool TryGetOwnedMode(out TestMode mode)
        {
            int storedMode = SessionState.GetInt(ModeKey, 0);
            if (storedMode == (int)TestMode.EditMode || storedMode == (int)TestMode.PlayMode)
            {
                mode = (TestMode)storedMode;
                return true;
            }

            mode = default;
            return false;
        }

        static bool MatchesOwnedMode(ITestAdaptor test)
        {
            return test != null && TryGetOwnedMode(out TestMode mode) && (test.TestMode & mode) == mode;
        }

        static string ModeName(TestMode mode)
        {
            return mode == TestMode.EditMode ? "EditMode" : mode == TestMode.PlayMode ? "PlayMode" : mode.ToString();
        }

        static void ScheduleStaleStateRecoveryIfNeeded()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;

            if (!TryGetOwnedMode(out TestMode mode))
            {
                Debug.LogWarning($"{LogPrefix} Cleared invalid persisted test-run ownership after script reload.");
                ClearOwnedRun();
                return;
            }

            staleRecoveryDeadline = EditorApplication.timeSinceStartup + StaleRecoveryDelaySeconds;
            staleRecoveryPending = true;
            EditorApplication.update -= RecoverOrClearStaleState;
            EditorApplication.update += RecoverOrClearStaleState;

            string phase = SessionState.GetBool(StartedKey, false) ? "running" : "scheduled";
            Debug.Log($"{LogPrefix} Recovered {phase} {ModeName(mode)} test-run ownership after script reload.");
        }

        static void RecoverOrClearStaleState()
        {
            if (!SessionState.GetBool(ActiveKey, false))
            {
                CancelStaleStateRecovery();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                staleRecoveryDeadline = EditorApplication.timeSinceStartup + StaleRecoveryDelaySeconds;
                return;
            }

            if (EditorApplication.timeSinceStartup < staleRecoveryDeadline) return;

            string mode = TryGetOwnedMode(out TestMode ownedMode) ? ModeName(ownedMode) : "unknown-mode";
            Debug.LogWarning($"{LogPrefix} Cleared stale {mode} test-run ownership after Unity returned idle without a completion callback.");
            ClearOwnedRun();
        }

        static void CancelStaleStateRecovery()
        {
            if (!staleRecoveryPending) return;
            EditorApplication.update -= RecoverOrClearStaleState;
            staleRecoveryPending = false;
        }

        static void ScheduleStartAcknowledgementIfNeeded()
        {
            if (!SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(StartedKey, false)) return;

            startAcknowledgementDeadline = EditorApplication.timeSinceStartup + StartAcknowledgementDelaySeconds;
            startAcknowledgementPending = true;
            EditorApplication.update -= RecoverMissingRunStart;
            EditorApplication.update += RecoverMissingRunStart;
        }

        static void RecoverMissingRunStart()
        {
            if (!SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(StartedKey, false))
            {
                CancelStartAcknowledgement();
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                startAcknowledgementDeadline = EditorApplication.timeSinceStartup + StartAcknowledgementDelaySeconds;
                return;
            }

            if (EditorApplication.timeSinceStartup < startAcknowledgementDeadline) return;

            string mode = TryGetOwnedMode(out TestMode ownedMode) ? ModeName(ownedMode) : "unknown-mode";
            ClearOwnedRun();
            Debug.LogWarning($"{LogPrefix} {mode} run did not report RunStarted after Execute; ownership cleared. QA may invoke menu once.");
        }

        static void CancelStartAcknowledgement()
        {
            if (!startAcknowledgementPending) return;
            EditorApplication.update -= RecoverMissingRunStart;
            startAcknowledgementPending = false;
        }

        sealed class GateCallbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                if (!SessionState.GetBool(ActiveKey, false) || !MatchesOwnedMode(testsToRun)) return;

                SessionState.SetBool(StartedKey, true);
                CancelStartAcknowledgement();
                Debug.Log($"{LogPrefix} {ModeName(testsToRun.TestMode)} run started with {testsToRun.TestCaseCount} test case(s).");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                if (!SessionState.GetBool(ActiveKey, false) || result == null || !MatchesOwnedMode(result.Test)) return;

                string mode = ModeName(result.Test.TestMode);
                string jobId = SessionState.GetString(JobIdKey, string.Empty);
                int total = result.Test.TestCaseCount;
                ClearOwnedRun();

                Debug.Log(
                    $"{LogPrefix} {mode} run finished: status={result.ResultState}, total={total}, " +
                    $"passed={result.PassCount}, failed={result.FailCount}, skipped={result.SkipCount}, " +
                    $"inconclusive={result.InconclusiveCount}, duration={result.Duration:0.###}s, job={jobId}.");
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
            }
        }
    }
}
