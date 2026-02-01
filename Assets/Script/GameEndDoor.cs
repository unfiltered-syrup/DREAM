using UnityEngine;

public class GameEndDoor : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameEndUI.Instance != null)
            {
                GameEndUI.Instance.TriggerWin();
            }
        }
    }
}