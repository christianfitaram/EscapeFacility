using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeFacility
{
    [RequireComponent(typeof(Collider))]
    public class Collectible : MonoBehaviour
    {
        [SerializeField] private int scoreValue = 100;
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float bobHeight = 0.25f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private string collectibleId = "";

        private bool _collected;
        private Vector3 _startPosition;

        private void Awake()
        {
            Collider triggerCollider = GetComponent<Collider>();
            triggerCollider.isTrigger = true;

            if (string.IsNullOrWhiteSpace(collectibleId))
            {
                collectibleId = BuildFallbackCollectibleId();
            }

            _startPosition = transform.position;

            if (GameManager.Instance != null && GameManager.Instance.HasCollectedCollectible(collectibleId))
            {
                _collected = true;
                Destroy(gameObject);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying || !string.IsNullOrWhiteSpace(collectibleId))
            {
                return;
            }

            collectibleId = Guid.NewGuid().ToString("N");
        }
#endif

        private void Update()
        {
            if (_collected)
            {
                return;
            }

            // A small bob + spin makes the pickup readable without needing a separate animation clip.
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            transform.position = _startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected || !other.CompareTag("Player"))
            {
                return;
            }

            _collected = true;
            GameManager.Instance.RegisterCollectible(collectibleId, scoreValue);
            GameManager.Instance.Audio.PlayPickup();
            Destroy(gameObject);
        }

        private string BuildFallbackCollectibleId()
        {
            return $"{SceneManager.GetActiveScene().name}:{BuildHierarchyPath(transform)}";
        }

        private static string BuildHierarchyPath(Transform current)
        {
            string path = current.name;
            Transform parent = current.parent;
            while (parent != null)
            {
                path = $"{parent.name}/{path}";
                parent = parent.parent;
            }

            return path;
        }
    }
}
