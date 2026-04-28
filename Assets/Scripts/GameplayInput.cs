using UnityEngine;

namespace EscapeFacility
{
    public static class GameplayInput
    {
        public static KeyCode PauseKey => KeyCode.Escape;
        public static KeyCode InteractKey => KeyCode.E;
        public static string InteractKeyLabel => InteractKey.ToString().ToUpperInvariant();

        public static bool PausePressed()
        {
            return Input.GetKeyDown(PauseKey);
        }

        public static bool InteractPressed()
        {
            return Input.GetKeyDown(InteractKey);
        }
    }
}
