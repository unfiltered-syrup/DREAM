using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Settings")]
    [SerializeReference] public GameObject Head;
    public Transform HeadTransform;

    [Header("Debug View")]
    public float EyeContactDuration = 0f;

    void Awake()
    {
        HeadTransform = Head.transform;
    }

    public void AddEyeContact(float amount)
    {
        EyeContactDuration += amount;
    }
}