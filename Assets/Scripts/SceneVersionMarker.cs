using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeFacility
{
    public class SceneVersionMarker : MonoBehaviour
    {
        [SerializeField] private string sceneKey = "";
        [SerializeField] private string generationStamp = "";

        private void Start()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"Loaded scene marker: {SceneManager.GetActiveScene().name} | key={sceneKey} | generated={generationStamp}");
#endif
        }
    }
}
