using UnityEngine;
using UnityEngine.UI;

namespace EscapeFacility
{
    public class EndScreenController : MonoBehaviour
    {
        [SerializeField] private Text titleText;
        [SerializeField] private Text subtitleText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text timeText;
        [SerializeField] private Text collectedText;

        private void Start()
        {
            if (titleText != null)
            {
                titleText.text = GameManager.Instance.LastRunWasVictory ? "Victory" : "Defeat";
            }

            if (subtitleText != null)
            {
                subtitleText.text = GameManager.Instance.LastRunWasVictory
                    ? "Rescue signal transmitted. Extraction is inbound."
                    : "Mission failed. The yard claimed the run before extraction.";
            }

            if (scoreText != null)
            {
                scoreText.text = $"Score: {GameManager.Instance.LastRunScore}";
            }

            if (timeText != null)
            {
                timeText.text = $"Time Left: {GameManager.Instance.FormattedLastRunRemainingTime}";
            }

            if (collectedText != null)
            {
                collectedText.text = $"Collected: {GameManager.Instance.LastRunCollected}";
            }
        }

        public void Replay()
        {
            GameManager.Instance.ReplayLastRun();
        }

        public void ExitGame()
        {
            ApplicationActions.QuitGame();
        }
    }
}
