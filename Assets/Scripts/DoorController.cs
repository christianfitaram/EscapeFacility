using UnityEngine;

namespace EscapeFacility
{
    [RequireComponent(typeof(BoxCollider))]
    public class DoorController : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = "";
        [SerializeField] private bool completesRun;
        [SerializeField] private bool requiresSceneObjective = true;
        [SerializeField] private float openHeight = 3f;
        [SerializeField] private float openSpeed = 2f;

        private bool _isUnlocked;
        private bool _isTransitioning;
        private Vector3 _closedPosition;
        private Vector3 _openPosition;
        private BoxCollider _solidCollider;
        private Transform _triggerZoneTransform;

        private void Awake()
        {
            _closedPosition = transform.position;
            _openPosition = _closedPosition + Vector3.up * openHeight;

            _solidCollider = GetComponent<BoxCollider>();
            if (_solidCollider != null)
            {
                // Keep the original collider solid and create a separate fixed trigger zone in the doorway.
                _solidCollider.isTrigger = false;
                CreateTriggerZone();
            }
        }

        private void Update()
        {
            // Doors unlock automatically once the scene objective is met.
            bool objectiveSatisfied = !requiresSceneObjective
                                     || (GameManager.Instance.CurrentSceneContext != null && GameManager.Instance.HasMetCurrentObjective);

            if (!_isUnlocked && objectiveSatisfied)
            {
                _isUnlocked = true;
                GameManager.Instance.Audio.PlayDoorOpen();
            }

            if (_isUnlocked)
            {
                transform.position = Vector3.MoveTowards(transform.position, _openPosition, openSpeed * Time.deltaTime);
            }

            if (_triggerZoneTransform != null)
            {
                _triggerZoneTransform.SetPositionAndRotation(_closedPosition, transform.rotation);
            }
        }

        public void HandleTriggerEnter(Collider other)
        {
            if (_isTransitioning || !_isUnlocked || !other.CompareTag("Player"))
            {
                return;
            }

            _isTransitioning = true;

            if (completesRun)
            {
                GameManager.Instance.FinishRun(true);
                return;
            }

            GameManager.Instance.LoadConfiguredNextScene(nextSceneName);
        }

        private void CreateTriggerZone()
        {
            GameObject triggerZoneObject = new GameObject("TransitionTrigger");
            triggerZoneObject.transform.SetParent(transform, false);
            triggerZoneObject.transform.localPosition = Vector3.zero;
            triggerZoneObject.transform.localRotation = Quaternion.identity;
            triggerZoneObject.transform.localScale = Vector3.one;
            _triggerZoneTransform = triggerZoneObject.transform;

            BoxCollider triggerCollider = triggerZoneObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.center = _solidCollider.center;
            triggerCollider.size = new Vector3(
                _solidCollider.size.x + 0.6f,
                _solidCollider.size.y,
                Mathf.Max(_solidCollider.size.z + 3f, 4f));

            DoorTransitionTrigger trigger = triggerZoneObject.AddComponent<DoorTransitionTrigger>();
            trigger.Initialize(this);
        }
    }
}
