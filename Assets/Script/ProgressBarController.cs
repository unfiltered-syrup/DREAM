using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    private Slider _slider;
    private PlayerLogic _playerLogic;

    [Header("Settings")]
    public float maxValue = 100f;

    void Start()
    {
        _slider = GetComponent<Slider>();

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            _playerLogic = playerObj.GetComponent<PlayerLogic>();
        }
        
        if (_slider != null)
        {
            _slider.minValue = 0;
            _slider.maxValue = maxValue;
        }
    }

    void Update()
    {
        if (_playerLogic != null && _slider != null)
        {

            float currentProgress = _playerLogic.EyeContactDuration;
            _slider.value = Mathf.Clamp(currentProgress, 0f, maxValue);
        }
    }
}