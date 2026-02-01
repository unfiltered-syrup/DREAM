using UnityEngine;

public class ChairScript : MonoBehaviour
{
    public bool Occupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Occupied = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Occupied = false;
        }
    }
}