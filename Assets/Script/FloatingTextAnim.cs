using UnityEngine;
using TMPro;

public class FloatingTextAnim : MonoBehaviour
{
    public float MoveSpeed = 0.1f;
    public float Lifetime = 0.8f;

    void Start()
    {
        // Auto-destroy after X seconds
        Destroy(gameObject, Lifetime);
        
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }

    void Update()
    {
        transform.position += Vector3.up * MoveSpeed * Time.deltaTime;
    }
}