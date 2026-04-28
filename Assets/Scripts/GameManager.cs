using System;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeFacility
{
    [DefaultExecutionOrder(-1000)]
    public class GameManager : MonoBehaviour
    {
        private const string MusicEnabledKey = "EscapeFacility.MusicEnabled";
        private const float MaxRunTimeSeconds = 1800f;
        private const int RequiredPowerCores = 3;

        public static GameManager Instance { get; private set; }

        public AudioManager Audio { get; private set; }
        public SceneContext CurrentSceneContext { get; private set; }
        public bool MusicEnabled { get; private set; } = true;
        public bool IsPaused { get; private set; }
        public bool RunHasStarted { get; private set; }
        public bool PowerCoresCharged { get; private set; }
        public bool RescueMessageBroadcast { get; private set; }

        public int TotalScore { get; private set; }
        public int TotalCollected { get; private set; }
        public int CurrentSceneScore { get; private set; }
        public int CurrentSceneCollectibles { get; private set; }
        public int CurrentSceneRequiredCollectibles { get; private set; }
        public float ElapsedTime { get; private set; }

        public bool LastRunWasVictory { get; private set; }
        public int LastRunScore { get; private set; }
        public int LastRunCollected { get; private set; }
        public float LastRunRemainingTime { get; private set; }

        public bool HasMetCurrentObjective => CurrentSceneCollectibles >= CurrentSceneRequiredCollectibles;
        public string FormattedElapsedTime => FormatTime(ElapsedTime);
        public string FormattedLastRunRemainingTime => FormatTime(LastRunRemainingTime);
        public float RemainingTime => Mathf.Max(0f, MaxRunTimeSeconds - ElapsedTime);
        public string FormattedRemainingTime => FormatTime(RemainingTime);
        public string InteractionPrompt { get; private set; } = string.Empty;
        public string CurrentObjectiveText => GetCurrentObjectiveText();
        public bool CanChargePowerCores => TotalCollected >= RequiredPowerCores && !PowerCoresCharged;
        public bool CanBroadcastRescue => PowerCoresCharged && !RescueMessageBroadcast;

        private string _runStartSceneName = SceneNameRegistry.Level1;
        private readonly HashSet<string> _collectedCollectibleIds = new HashSet<string>();
        private StarterAssetsInputs _starterAssetsInputs;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // Create the global runtime services before any scene logic runs.
            if (Instance != null)
            {
                return;
            }

            GameObject managerObject = new GameObject(nameof(GameManager));
            managerObject.AddComponent<AudioManager>();
            managerObject.AddComponent<GameManager>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Audio = GetComponent<AudioManager>();
            if (Audio == null)
            {
                Audio = gameObject.AddComponent<AudioManager>();
            }

            MusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                SceneManager.sceneLoaded -= HandleSceneLoaded;
            }
        }

        private void Update()
        {
            if (CurrentSceneContext != null && !IsPaused)
            {
                ElapsedTime += Time.deltaTime;
                if (ElapsedTime >= MaxRunTimeSeconds)
                {
                    ElapsedTime = MaxRunTimeSeconds;
                    FinishRun(false);
                }
            }
        }

        public void StartNewRun(string sceneName)
        {
            ResetRunStats();
            RunHasStarted = true;
            _runStartSceneName = sceneName;
            LoadScene(sceneName);
        }

        public void ReplayLastRun()
        {
            StartNewRun(_runStartSceneName);
        }

        public void LoadMainMenu()
        {
            SetPause(false);
            CurrentSceneContext = null;
            LoadScene(SceneNameRegistry.MainMenu);
        }

        public void LoadOptionsMenu()
        {
            SetPause(false);
            CurrentSceneContext = null;
            LoadScene(SceneNameRegistry.OptionsMenu);
        }

        public void RegisterGameplayScene(SceneContext context)
        {
            if (context == null)
            {
                return;
            }

            // Entering a gameplay scene resets the per-scene counters but keeps the full run totals.
            if (!RunHasStarted)
            {
                ResetRunStats();
                RunHasStarted = true;
                _runStartSceneName = SceneManager.GetActiveScene().name;
            }

            CurrentSceneContext = context;
            CurrentSceneScore = 0;
            CurrentSceneCollectibles = 0;
            CurrentSceneRequiredCollectibles = Mathf.Max(0, context.RequiredCollectibles);
            InteractionPrompt = string.Empty;

            SetPause(false);
            ApplyCursorState(true);
            Audio.PlayMusic(context.MusicTheme);
        }

        public bool HasCollectedCollectible(string collectibleId)
        {
            return !string.IsNullOrWhiteSpace(collectibleId) && _collectedCollectibleIds.Contains(collectibleId);
        }

        public void RegisterCollectible(string collectibleId, int scoreValue)
        {
            if (!string.IsNullOrWhiteSpace(collectibleId))
            {
                if (_collectedCollectibleIds.Contains(collectibleId))
                {
                    return;
                }

                _collectedCollectibleIds.Add(collectibleId);
            }

            CurrentSceneCollectibles += 1;
            CurrentSceneScore += scoreValue;
            TotalCollected += 1;
            TotalScore += scoreValue;
        }

        public void RestartCurrentScene()
        {
            // Remove the current scene's contribution before restarting so repeated deaths do not duplicate score.
            TotalCollected = Mathf.Max(0, TotalCollected - CurrentSceneCollectibles);
            TotalScore = Mathf.Max(0, TotalScore - CurrentSceneScore);
            CurrentSceneCollectibles = 0;
            CurrentSceneScore = 0;

            SetPause(false);
            LoadScene(SceneManager.GetActiveScene().name);
        }

        public void SetInteractionPrompt(string prompt)
        {
            InteractionPrompt = prompt ?? string.Empty;
        }

        public void ClearInteractionPrompt()
        {
            InteractionPrompt = string.Empty;
        }

        public bool TryChargePowerCores()
        {
            if (!CanChargePowerCores)
            {
                return false;
            }

            PowerCoresCharged = true;
            Audio.PlayDoorOpen();
            return true;
        }

        public bool TryBroadcastRescueMessage()
        {
            if (!CanBroadcastRescue)
            {
                return false;
            }

            RescueMessageBroadcast = true;
            Audio.PlayDoorOpen();
            FinishRun(true);
            return true;
        }

        public void LoadConfiguredNextScene(string fallbackSceneName = "")
        {
            string nextSceneName = fallbackSceneName;
            if (string.IsNullOrWhiteSpace(nextSceneName) && CurrentSceneContext != null)
            {
                nextSceneName = CurrentSceneContext.NextSceneName;
            }

            if (!string.IsNullOrWhiteSpace(nextSceneName))
            {
                SetPause(false);
                CurrentSceneContext = null;
                LoadScene(nextSceneName);
            }
        }

        public void FinishRun(bool victory)
        {
            LastRunWasVictory = victory;
            LastRunScore = TotalScore;
            LastRunCollected = TotalCollected;
            LastRunRemainingTime = RemainingTime;

            SetPause(false);
            CurrentSceneContext = null;
            RunHasStarted = false;
            InteractionPrompt = string.Empty;

            LoadScene(SceneNameRegistry.EndScreen);
        }

        public void SetPause(bool paused)
        {
            if (CurrentSceneContext == null)
            {
                IsPaused = false;
                Time.timeScale = 1f;
                return;
            }

            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            ApplyCursorState(!paused);
        }

        public void TogglePause()
        {
            SetPause(!IsPaused);
        }

        public void SetMusicEnabled(bool enabled)
        {
            MusicEnabled = enabled;
            PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            Audio.RefreshMusicState();
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Time.timeScale = 1f;
            IsPaused = false;
            _starterAssetsInputs = FindAnyObjectByType<StarterAssetsInputs>();

            switch (scene.name)
            {
                case SceneNameRegistry.MainMenu:
                case SceneNameRegistry.OptionsMenu:
                    CurrentSceneContext = null;
                    InteractionPrompt = string.Empty;
                    ApplyCursorState(false);
                    Audio.PlayMusic(MusicTheme.Menu);
                    break;
                case SceneNameRegistry.EndScreen:
                    CurrentSceneContext = null;
                    InteractionPrompt = string.Empty;
                    ApplyCursorState(false);
                    Audio.PlayMusic(LastRunWasVictory ? MusicTheme.EndVictory : MusicTheme.EndDefeat);
                    break;
                default:
                    InteractionPrompt = string.Empty;
                    ApplyCursorState(true);
                    break;
            }
        }

        private void LoadScene(string sceneName)
        {
            if (!string.IsNullOrWhiteSpace(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private void ApplyCursorState(bool lockCursor)
        {
            // The Starter Assets input component also needs the cursor state so camera look behaves correctly.
            Cursor.visible = !lockCursor;
            Cursor.lockState = lockCursor ? CursorLockMode.Locked : CursorLockMode.None;

            if (_starterAssetsInputs != null)
            {
                _starterAssetsInputs.cursorLocked = lockCursor;
                _starterAssetsInputs.cursorInputForLook = lockCursor;
                _starterAssetsInputs.LookInput(Vector2.zero);
            }
        }

        private void ResetRunStats()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            RunHasStarted = false;
            CurrentSceneContext = null;
            TotalScore = 0;
            TotalCollected = 0;
            CurrentSceneScore = 0;
            CurrentSceneCollectibles = 0;
            CurrentSceneRequiredCollectibles = 0;
            ElapsedTime = 0f;
            PowerCoresCharged = false;
            RescueMessageBroadcast = false;
            InteractionPrompt = string.Empty;
            _collectedCollectibleIds.Clear();
        }

        private static string FormatTime(float seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return time.ToString(@"mm\:ss");
        }

        private string GetCurrentObjectiveText()
        {
            if (CurrentSceneContext == null)
            {
                return string.Empty;
            }

            int progressTarget = CurrentSceneContext.ObjectiveProgressTarget > 0
                ? CurrentSceneContext.ObjectiveProgressTarget
                : RequiredPowerCores;

            switch (CurrentSceneContext.ObjectiveMode)
            {
                case SceneObjectiveMode.CollectAndExit:
                    return TotalCollected >= progressTarget
                        ? CurrentSceneContext.PostProgressObjectiveText
                        : string.Format(
                            CurrentSceneContext.ProgressObjectiveTemplate,
                            Mathf.Min(TotalCollected, progressTarget),
                            progressTarget);

                case SceneObjectiveMode.ChargeAndBroadcast:
                    if (TotalCollected < progressTarget)
                    {
                        return string.Format(
                            CurrentSceneContext.ProgressObjectiveTemplate,
                            Mathf.Min(TotalCollected, progressTarget),
                            progressTarget);
                    }

                    if (!PowerCoresCharged)
                    {
                        return CurrentSceneContext.PostProgressObjectiveText;
                    }

                    if (!RescueMessageBroadcast)
                    {
                        return CurrentSceneContext.PostActivationObjectiveText;
                    }

                    return CurrentSceneContext.CompletedObjectiveText;

                default:
                    return CurrentSceneContext.SceneDisplayName;
            }
        }
    }
}
