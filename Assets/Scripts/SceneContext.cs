using UnityEngine;

namespace EscapeFacility
{
    public class SceneContext : MonoBehaviour
    {
        [SerializeField] private string sceneDisplayName = "Gameplay Scene";
        [SerializeField] private int requiredCollectibles = 3;
        [SerializeField] private string nextSceneName = "";
        [SerializeField] private MusicTheme musicTheme = MusicTheme.Outdoor;
        [SerializeField] private SceneObjectiveMode objectiveMode = SceneObjectiveMode.Generic;
        [SerializeField] private int objectiveProgressTarget;
        [SerializeField] private string progressObjectiveTemplate = "Objective: Collect required items ({0}/{1})";
        [SerializeField] private string postProgressObjectiveText = "";
        [SerializeField] private string postActivationObjectiveText = "";
        [SerializeField] private string completedObjectiveText = "";

        public string SceneDisplayName => sceneDisplayName;
        public int RequiredCollectibles => requiredCollectibles;
        public string NextSceneName => nextSceneName;
        public MusicTheme MusicTheme => musicTheme;
        public SceneObjectiveMode ObjectiveMode => objectiveMode;
        public int ObjectiveProgressTarget => objectiveProgressTarget;
        public string ProgressObjectiveTemplate => progressObjectiveTemplate;
        public string PostProgressObjectiveText => postProgressObjectiveText;
        public string PostActivationObjectiveText => postActivationObjectiveText;
        public string CompletedObjectiveText => completedObjectiveText;

        private void Start()
        {
            // Each gameplay scene registers its own requirements when it becomes active.
            GameManager.Instance.RegisterGameplayScene(this);
        }
    }
}
