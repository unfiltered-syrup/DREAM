using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    [Header("Settings")]
    public Transform HeadTransform;

    [Header("Debug View")]
    public float EyeContactDuration = 0f;

    private void Start()
    {
        GameObject headObject = GameObject.FindWithTag("Head");

        if (headObject != null)
        {
            HeadTransform = headObject.transform;
        }
        else
        {
            Debug.LogError($"[PlayerLogic] My guy has no head");
        }
    }

    public void AddEyeContact(float amount)
    {
        EyeContactDuration += amount;
    }
}