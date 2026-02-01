using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIScript : MonoBehaviour
{
    public RectTransform tutorialIcon; 
    public RectTransform tutorialImage;
    public float animationDuration = 0.3f;
    public float iconBounceSpeed = 15f;
    public float spinAmount = 90f;

    // State tracking
    private bool isImageOpen = false;
    private InputSystem_Actions _input; 

    // Stored original values
    private Vector3 originalIconScale;
    private Vector3 originalImageScale;
    private Vector2 originalImagePos;
    private Vector2 hiddenImagePos;

    // Coroutine references
    private Coroutine imageCoroutine;
    private Coroutine iconCoroutine;

    private void Awake()
    {
        _input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        _input.Player.Enable();
        _input.Player.Tutorial.started += OnTutorialPressed;
        _input.Player.Tutorial.canceled += OnTutorialReleased;
    }

    private void OnDisable()
    {
        _input.Player.Disable();
        _input.Player.Tutorial.started -= OnTutorialPressed;
        _input.Player.Tutorial.canceled -= OnTutorialReleased;
    }

    void Start()
    {
        if (tutorialIcon != null) originalIconScale = tutorialIcon.localScale;
        
        if (tutorialImage != null)
        {
            originalImageScale = tutorialImage.localScale;
            originalImagePos = tutorialImage.anchoredPosition;

            hiddenImagePos = new Vector2(-Screen.width / 2f, -Screen.height / 2f);
            tutorialImage.localScale = Vector3.zero;
            tutorialImage.anchoredPosition = hiddenImagePos;
            
            tutorialImage.localRotation = Quaternion.Euler(0, 0, spinAmount);
            
            tutorialImage.gameObject.SetActive(false);
        }
    }

    private void OnTutorialPressed(InputAction.CallbackContext context)
    {
        isImageOpen = !isImageOpen;
        if (imageCoroutine != null) StopCoroutine(imageCoroutine);
        imageCoroutine = StartCoroutine(AnimateTutorialImage(isImageOpen));

        if (iconCoroutine != null) StopCoroutine(iconCoroutine);
        iconCoroutine = StartCoroutine(AnimateIconPress());
    }

    private void OnTutorialReleased(InputAction.CallbackContext context)
    {
        if (iconCoroutine != null) StopCoroutine(iconCoroutine);
        iconCoroutine = StartCoroutine(AnimateIconRelease());
    }

    private IEnumerator AnimateTutorialImage(bool show)
    {
        if (tutorialImage == null) yield break;

        if (show) tutorialImage.gameObject.SetActive(true);

        float time = 0;
        
        Vector3 startScale = tutorialImage.localScale;
        Vector2 startPos = tutorialImage.anchoredPosition;
        
        Quaternion startRot = tutorialImage.localRotation;

        // TARGETS
        Vector3 targetScale = show ? originalImageScale : Vector3.zero;
        Vector2 targetPos = show ? originalImagePos : hiddenImagePos;

        Quaternion targetRot = show ? Quaternion.identity : Quaternion.Euler(0, 0, spinAmount);

        while (time < animationDuration)
        {
            float t = time / animationDuration;
            t = t * t * (3f - 2f * t); 

            // Apply scale & position
            tutorialImage.localScale = Vector3.Lerp(startScale, targetScale, t);
            tutorialImage.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            
            // Apply rotation
            tutorialImage.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            time += Time.deltaTime;
            yield return null;
        }

        tutorialImage.localScale = targetScale;
        tutorialImage.anchoredPosition = targetPos;
        tutorialImage.localRotation = targetRot;

        if (!show) tutorialImage.gameObject.SetActive(false);
    }

    private IEnumerator AnimateIconPress()
    {
        if (tutorialIcon == null) yield break;

        Vector3 targetScale = originalIconScale * 0.8f; 

        while (Vector3.Distance(tutorialIcon.localScale, targetScale) > 0.01f)
        {
            tutorialIcon.localScale = Vector3.Lerp(tutorialIcon.localScale, targetScale, Time.deltaTime * iconBounceSpeed);
            yield return null;
        }
        tutorialIcon.localScale = targetScale;
    }

    private IEnumerator AnimateIconRelease()
    {
        if (tutorialIcon == null) yield break;

        Vector3 bulgeScale = originalIconScale * 1.1f;
        while (Vector3.Distance(tutorialIcon.localScale, bulgeScale) > 0.01f)
        {
            tutorialIcon.localScale = Vector3.Lerp(tutorialIcon.localScale, bulgeScale, Time.deltaTime * iconBounceSpeed);
            yield return null;
        }

        while (Vector3.Distance(tutorialIcon.localScale, originalIconScale) > 0.01f)
        {
            tutorialIcon.localScale = Vector3.Lerp(tutorialIcon.localScale, originalIconScale, Time.deltaTime * iconBounceSpeed);
            yield return null;
        }
        tutorialIcon.localScale = originalIconScale;
    }
}