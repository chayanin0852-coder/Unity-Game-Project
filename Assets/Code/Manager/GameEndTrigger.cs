
using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    public enum EndType
    {
        Die,
        Win
    }

    [Header("End Type")]
    [SerializeField] private EndType endType;

    [Header("References")]
    [SerializeField] private GameEndManager gameEndManager;

    [Header("Player Tag")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        Transform target = other.transform;

        while (target != null)
        {
            if (target.CompareTag(playerTag))
            {
                if (gameEndManager == null)
                {
                    Debug.LogError(
                        "GameEndTrigger: ไม่ได้เชื่อม GameEndManager",
                        this
                    );
                    return;
                }

                hasTriggered = true;

                if (endType == EndType.Die)
                {
                    gameEndManager.PlayerDied();
                }
                else if (endType == EndType.Win)
                {
                    gameEndManager.PlayerWon();
                }

                return;
            }

            target = target.parent;
        }
    }
}
