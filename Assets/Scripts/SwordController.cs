using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SwordController : MonoBehaviour
{
    public enum SwordDirection
    {
        Top,
        TopRight,
        Right,
        Left,
        TopLeft
    }
    [SerializeField] private List<Image> directions;
    [SerializeField] private RectTransform angleRect;
    [SerializeField] private GameObject swordRingObject;
    [SerializeField] private CinemachinePanTilt cam;
    [SerializeField] private float virtualCursorRadius;
    private Vector2 _virtualMousePos;
    private float _lastAngle;

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        swordRingObject.SetActive(false);
        cam.enabled = true;
        if (!Mouse.current.rightButton.isPressed) return;

        Cursor.lockState = CursorLockMode.Confined;
        swordRingObject.SetActive(true);
        cam.enabled = false;
        
        var delta = Mouse.current.delta.ReadValue();
        _virtualMousePos += delta;
        if (_virtualMousePos.magnitude > virtualCursorRadius)
        {
            _virtualMousePos = _virtualMousePos.normalized * virtualCursorRadius;
        }

        var direction = _virtualMousePos.normalized;
        var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle - 90f + 360f) % 360f;
        angleRect.localRotation = Quaternion.Euler(0, 0, angle);

        var swordDirection = GetDirection(angle);
        for (int i = 0; i < directions.Count; i++)
        {
            if (i == (int)swordDirection)
            {
                directions[i].color = Color.green;
            }
            else
            {
                directions[i].color = Color.white;
            }
        }
    }


    private SwordDirection GetDirection(float angle)
    {
        switch (angle)
        {
            case >= 315f:
            case < 45f:
                return SwordDirection.Top;
            case >= 45f and < 90f:
                return SwordDirection.TopLeft;
            case >= 90f and < 180f:
                return SwordDirection.Left;
            case >= 180f and < 270f:
                return SwordDirection.Right;
            case >= 270f and < 315f:
                return SwordDirection.TopRight;
            default:
                return SwordDirection.Top;
        }
    }
}
