using UnityEngine;

namespace EscapeFacility
{
    [RequireComponent(typeof(Collider))]
    public class MissionTerminal : MonoBehaviour
    {
        public enum TerminalAction
        {
            ChargePowerCores,
            BroadcastRescue
        }

        [SerializeField] private TerminalAction terminalAction;
        [SerializeField] private MeshRenderer indicatorRenderer;
        [SerializeField] private Color idleColor = new Color(0.15f, 0.18f, 0.22f, 1f);
        [SerializeField] private Color readyColor = new Color(0.16f, 0.62f, 0.82f, 1f);
        [SerializeField] private Color activeColor = new Color(0.22f, 0.92f, 0.68f, 1f);

        private bool _playerInside;
        private Material _runtimeMaterial;

        private void Awake()
        {
            Collider terminalCollider = GetComponent<Collider>();
            terminalCollider.isTrigger = true;

            if (indicatorRenderer == null)
            {
                indicatorRenderer = GetComponent<MeshRenderer>();
            }

            if (indicatorRenderer != null)
            {
                _runtimeMaterial = new Material(indicatorRenderer.sharedMaterial);
                _runtimeMaterial.name = $"{name}_Runtime";
                indicatorRenderer.sharedMaterial = _runtimeMaterial;
            }
        }

        private void Update()
        {
            UpdateVisualState();

            if (!_playerInside || GameManager.Instance == null || GameManager.Instance.IsPaused)
            {
                return;
            }

            GameManager.Instance.SetInteractionPrompt(GetPromptText());

            if (GameplayInput.InteractPressed())
            {
                TryActivate();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            _playerInside = false;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ClearInteractionPrompt();
            }
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(_runtimeMaterial);
                return;
            }

            DestroyImmediate(_runtimeMaterial);
        }

        private void TryActivate()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            bool activated = terminalAction == TerminalAction.ChargePowerCores
                ? GameManager.Instance.TryChargePowerCores()
                : GameManager.Instance.TryBroadcastRescueMessage();

            if (activated)
            {
                GameManager.Instance.ClearInteractionPrompt();
            }
        }

        private string GetPromptText()
        {
            if (GameManager.Instance == null)
            {
                return string.Empty;
            }

            if (terminalAction == TerminalAction.ChargePowerCores)
            {
                if (GameManager.Instance.PowerCoresCharged)
                {
                    return "Power cores charged";
                }

                return GameManager.Instance.CanChargePowerCores
                    ? $"Press {GameplayInput.InteractKeyLabel} to charge the power cores"
                    : "Collect all 3 power cores first";
            }

            if (GameManager.Instance.RescueMessageBroadcast)
            {
                return "Rescue message sent";
            }

            return GameManager.Instance.CanBroadcastRescue
                ? $"Press {GameplayInput.InteractKeyLabel} to broadcast the rescue message"
                : "Charge the power cores first";
        }

        private void UpdateVisualState()
        {
            if (_runtimeMaterial == null || GameManager.Instance == null)
            {
                return;
            }

            Color targetColor;
            if (terminalAction == TerminalAction.ChargePowerCores)
            {
                targetColor = GameManager.Instance.PowerCoresCharged
                    ? activeColor
                    : (GameManager.Instance.CanChargePowerCores ? readyColor : idleColor);
            }
            else
            {
                targetColor = GameManager.Instance.RescueMessageBroadcast
                    ? activeColor
                    : (GameManager.Instance.CanBroadcastRescue ? readyColor : idleColor);
            }

            _runtimeMaterial.SetColor("_BaseColor", targetColor);
            _runtimeMaterial.SetColor("_Color", targetColor);
            _runtimeMaterial.SetColor("_EmissionColor", targetColor * 0.8f);
            _runtimeMaterial.EnableKeyword("_EMISSION");
        }
    }
}
