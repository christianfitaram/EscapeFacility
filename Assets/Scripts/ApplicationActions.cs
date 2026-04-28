using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeFacility
{
    public static class ApplicationActions
    {
        public static void QuitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
