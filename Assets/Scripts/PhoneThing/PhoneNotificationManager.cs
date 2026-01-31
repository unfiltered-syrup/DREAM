using UnityEngine;

public class PhoneNotificationManager : MonoBehaviour
{
    [SerializeField] private PhoneNotificationUI notificationUI;
    [SerializeField] private Transform phoneParent;
    [SerializeField] private string[] notificationTexts;
    private float spawnTimer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= Random.Range(1.5f, 3f))
        {
            var notif = Instantiate(notificationUI, phoneParent);
            var chosenText = notificationTexts[Random.Range(0, notificationTexts.Length)];
            notif.Initialize(chosenText, 1f + chosenText.Length * 0.2f, null);
            spawnTimer = 0;
        }
    }
}
