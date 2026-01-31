using UnityEngine;
using TMPro;

public class EyeContactUI : MonoBehaviour
{
    private TMP_Text _textComponent;
    private PlayerLogic _playerLogic;

    void Start()
    {
        _textComponent = GetComponent<TMP_Text>();

        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            _playerLogic = playerObj.GetComponent<PlayerLogic>();
        }

        if (_playerLogic == null)
        {
            Debug.LogError("[EyeContactUI] Player is missing");
        }
    }

    void Update()
    {
        if (_playerLogic != null && _textComponent != null)
        {
            float timeValue = _playerLogic.EyeContactDuration;
            _textComponent.text = $"Cringe\nLV:{Mathf.RoundToInt(timeValue).ToString()}";
        }
    }
}