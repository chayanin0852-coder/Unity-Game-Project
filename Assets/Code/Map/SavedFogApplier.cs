
using System.Collections;
using UnityEngine;

public class SavedFogApplier : MonoBehaviour
{
    [SerializeField] private PlayerMove playerMove;

    private IEnumerator Start()
    {
        // Wait until PlayerMove captures the original scene fog.
        yield return null;

        if (playerMove == null)
        {
            playerMove = FindFirstObjectByType<PlayerMove>();
        }

        if (playerMove == null)
        {
            Debug.LogError("‰¡Ëæ∫ PlayerMove „π Level 1!");
            yield break;
        }

        float savedFog = PlayerPrefs.GetFloat(
            "FogDensity",
            0.6667f
        );

        playerMove.SetFogAmount(savedFog);

        Debug.Log("Applied saved Fog: " +
                  Mathf.RoundToInt(savedFog * 100f) + "%");
    }
}
