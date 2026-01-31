using UnityEngine;

public class CatBehaviour : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float offset;
    
    private void Update()
    {
        transform.LookAt(player.position, Vector3.up);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y + offset, 0);
    }
}
