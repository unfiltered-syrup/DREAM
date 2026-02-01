using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class RainbowText : MonoBehaviour
{
    private TextMeshProUGUI tmp;
    
    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }
    
    private void Update()
    {
        tmp.color = Helpers.Rainbow();
    }
}
