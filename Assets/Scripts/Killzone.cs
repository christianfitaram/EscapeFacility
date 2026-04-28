using UnityEngine;

namespace EscapeFacility
{
    [RequireComponent(typeof(Collider))]
    public class Killzone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            GameManager.Instance.Audio.PlayFailure();
            GameManager.Instance.RestartCurrentScene();
        }
    }
}
