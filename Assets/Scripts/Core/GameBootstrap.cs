using UnityEngine;

namespace LuckyQuest.Core
{
    /// <summary>
    /// Entry point for scene initialisation.
    /// Attach to the GameRoot GameObject alongside GameManager.
    /// Handles any startup sequencing that must happen before other MonoBehaviours Start.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake()
        {
            if (gameManager == null)
                gameManager = GetComponent<GameManager>();

            if (gameManager == null)
                Debug.LogError("[GameBootstrap] GameManager reference is missing. Assign it in the Inspector.");
        }
    }
}
