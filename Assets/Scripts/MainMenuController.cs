using UnityEngine;

namespace EscapeFacility
{
    public class MainMenuController : MonoBehaviour
    {
        public void StartGame()
        {
            GameManager.Instance.StartNewRun(SceneNameRegistry.Level1);
        }

        public void OpenOptions()
        {
            GameManager.Instance.LoadOptionsMenu();
        }

        public void ExitGame()
        {
            ApplicationActions.QuitGame();
        }
    }
}
