using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhoneNotificationUI : MonoBehaviour
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI text;
    private float duration = 1f;
    private float t;
    private bool started;
    
    void Start()
    {
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0.2f);
        StartCoroutine(OnOpen());
    }

    public void Initialize(string message, float duration, Sprite image)
    {
        text.text = message;
        // this.image.sprite = image;
        this.duration = duration;
    }

    // Update is called once per frame
    void Update()
    {
        if (!started) return;
        t += Time.deltaTime;
        if (t >= duration) Destroy(gameObject);
    }

    private IEnumerator OnOpen()
    {
        var timer = 0f;
        var duration = .5f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            rectTransform.anchoredPosition =
                new Vector2(rectTransform.anchoredPosition.x, Mathf.Lerp(0.2f, 0, timer / duration));
            yield return null;
        }
        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 0);
        started = true;
    }
}
