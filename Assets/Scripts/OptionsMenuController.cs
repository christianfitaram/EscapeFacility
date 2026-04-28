using UnityEngine;
using UnityEngine.UI;

namespace EscapeFacility
{
    public class OptionsMenuController : MonoBehaviour
    {
        [SerializeField] private Text musicStatusText;

        private void Start()
        {
            RefreshLabel();
        }

        public void ToggleMusic()
        {
            GameManager.Instance.SetMusicEnabled(!GameManager.Instance.MusicEnabled);
            RefreshLabel();
        }

        public void LoadLevel1()
        {
            GameManager.Instance.StartNewRun(SceneNameRegistry.Level1);
        }

        public void LoadLevel2()
        {
            GameManager.Instance.StartNewRun(SceneNameRegistry.Level2);
        }

        public void BackToMainMenu()
        {
            GameManager.Instance.LoadMainMenu();
        }

        public void ExitGame()
        {
            ApplicationActions.QuitGame();
        }

        private void RefreshLabel()
        {
            if (musicStatusText != null)
            {
                musicStatusText.text = GameManager.Instance.MusicEnabled ? "Audio Feed: ON" : "Audio Feed: OFF";
            }
        }
    }
}
