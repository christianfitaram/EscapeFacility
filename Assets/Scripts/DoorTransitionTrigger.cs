using UnityEngine;

namespace EscapeFacility
{
    [RequireComponent(typeof(BoxCollider))]
    public class DoorTransitionTrigger : MonoBehaviour
    {
        [SerializeField] private DoorController doorController;

        public void Initialize(DoorController controller)
        {
            doorController = controller;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (doorController == null)
            {
                return;
            }

            doorController.HandleTriggerEnter(other);
        }
    }
}
