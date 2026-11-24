using UnityEngine;
using UnityEngine.SceneManagement;

public class Door : MonoBehaviour
{
    public string nextSceneName = "Level2";

    public Transform door; // ?? ลาก Door_A_Door มาใส่

    private bool isOpen = false;
    private bool isOpening = false;

    public float openSpeed = 120f;
    public float openAngle = -90f;

    private float rotated = 0f;

    void Update()
    {
        if (isOpening && rotated < Mathf.Abs(openAngle))
        {
            float step = openSpeed * Time.deltaTime;
            float rotateStep = Mathf.Min(step, Mathf.Abs(openAngle) - rotated);

            // ?? หมุนเฉพาะ door
            door.Rotate(0, Mathf.Sign(openAngle) * rotateStep, 0);

            rotated += rotateStep;
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        isOpening = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isOpen && other.CompareTag("Player"))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}