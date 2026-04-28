using UnityEngine;
using UnityEngine.UI;

namespace EscapeFacility
{
    public class GameplayHUD : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timerText;
        [SerializeField] private Text objectiveText;
        [SerializeField] private Text interactionPromptText;
        [SerializeField] private GameObject pauseRoot;

        private void Start()
        {
            if (pauseRoot != null)
            {
                pauseRoot.SetActive(false);
            }
        }

        private void Update()
        {
            // Pause is driven from the HUD because this object exists in every gameplay scene.
            if (GameplayInput.PausePressed())
            {
                GameManager.Instance.TogglePause();
            }

            RefreshHUD();
        }

        public void ResumeGame()
        {
            GameManager.Instance.SetPause(false);
        }

        public void ReturnToMainMenu()
        {
            GameManager.Instance.LoadMainMenu();
        }

        public void ExitGame()
        {
            ApplicationActions.QuitGame();
        }

        private void RefreshHUD()
        {
            // Read directly from the manager so the HUD always reflects the current scene and run state.
            if (scoreText != null)
            {
                scoreText.text = $"Score: {GameManager.Instance.TotalScore}";
            }

            if (timerText != null)
            {
                timerText.text = $"Time Left: {GameManager.Instance.FormattedRemainingTime}";
            }

            if (objectiveText != null)
            {
                objectiveText.text = GameManager.Instance.CurrentObjectiveText;
            }

            if (interactionPromptText != null)
            {
                interactionPromptText.text = GameManager.Instance.InteractionPrompt;
                interactionPromptText.gameObject.SetActive(!string.IsNullOrWhiteSpace(GameManager.Instance.InteractionPrompt));
            }

            if (pauseRoot != null)
            {
                pauseRoot.SetActive(GameManager.Instance.IsPaused);
            }
        }
    }
}
