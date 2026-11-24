using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    public string sceneName = "Level2";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ชนแล้ว: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("กำลังวาร์ป...");
            SceneManager.LoadScene(sceneName);
        }
    }
}