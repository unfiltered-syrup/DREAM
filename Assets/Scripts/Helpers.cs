using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Helpers : MonoBehaviour
{
    public enum EaseType
    {
        None,
        In,
        Out,
        InOut,
        OutBack,
    }
    
    public static IEnumerator TransformPosLerpRoutine(Transform t, Vector3 target, float duration, EaseType easeType = EaseType.None)
    {
        var timer = 0f;
        var startingPos = t.position;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = GetEase(progress, easeType);
            t.position = Vector3.Lerp(startingPos, target, eased);
            yield return null;
        }
        t.position = target;
    }
    
    public static IEnumerator TransformLocalPosLerpRoutine(Transform t, Vector3 target, float duration, EaseType easeType = EaseType.None)
    {
        var timer = 0f;
        var startingPos = t.localPosition;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = GetEase(progress, easeType);
            t.localPosition = Vector3.Lerp(startingPos, target, eased);
            yield return null;
        }
        t.localPosition = target;
    }
    
    public static IEnumerator TransformLocalScaleLerpRoutine(Transform t, Vector3 target, float duration, EaseType easeType = EaseType.None)
    {
        float timer = 0f;
        Vector3 startingScale = t.localScale;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = GetEase(progress, easeType);
            t.localScale = Vector3.LerpUnclamped(startingScale, target, eased);
            yield return null;
        }
        t.localScale = target;
    }
    
    public static IEnumerator RectScaleVec3LerpRoutine(RectTransform t, Vector3 target, float duration, EaseType easeType = EaseType.None, bool isUnscaledTime = false)
    {
        var timer = 0f;
        var startingScale = t.localScale;
        while (timer < duration)
        {
            timer += isUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(timer / duration);
            float eased = GetEase(progress, easeType);
            t.localScale = Vector3.Lerp(startingScale, target, eased);
            yield return null;
        }
        t.localScale = target;
    }
    
    public static IEnumerator LerpMaterialValue(Material mat, int property, float duration, bool isOn = true)
    {
        var timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            mat.SetFloat(property, Mathf.Lerp(isOn ? 0f : 1f, isOn ? 1f : 0f, timer / duration));
            yield return null;
        }
        mat.SetFloat(property, isOn ? 1f : 0f);
    }
    
    public static IEnumerator LerpImageFillAmount(Image img, float targetFill, float duration)
    {
        var timer = 0f;
        var startingFill = img.fillAmount;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            img.fillAmount = Mathf.Lerp(startingFill, targetFill, timer / duration);
            yield return null;
        }
        img.fillAmount = targetFill;
    }
    
    public static IEnumerator LerpImageColor(Image img, Color targetColor, float duration)
    {
        var timer = 0f;
        var startingColor = img.color;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            img.color = Color.Lerp(startingColor, targetColor, timer / duration);
            yield return null;
        }
        img.color = targetColor;
    }
    
    public static IEnumerator LerpCanvasGroupAlpha(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        var timer = 0f;
        var startingAlpha = canvasGroup.alpha;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, timer / duration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
    
    private static float GetEase(float t, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.In:
                return EaseIn(t);
            case EaseType.Out:
                return EaseOut(t);
            case EaseType.InOut:
                return EaseInOut(t);
            case EaseType.OutBack:
                return EaseOutBack(t);
            default:
                return t;
        }
    }
    
    public static float EaseIn(float t, int times = 2)
    {
        return Square(t, times);
    }
    
    public static float Flip(float x)
    {
        return 1 - x;
    }

    public static float EaseOut(float t, int times = 2)
    {
        return Flip(Square(Flip(t), times));
    }
    
    public static float EaseOutBack(float t, float overshoot = 1.70158f)
    {
        // Clamp t to [0,1] just in case
        t = Mathf.Clamp01(t);
        t = t - 1f;
        return (t * t * ((overshoot + 1f) * t + overshoot) + 1f);
    }
    
    public static float EaseInOut(float t, int times = 2)
    {
        return t < 0.5f ? EaseIn(t * 2f, times) * 0.5f : EaseOut((t - 0.5f) * 2f, times) * 0.5f + 0.5f;
    }
    
    public static float Square(float t, float times)
    {
        float value = t;
        for (int i = 1; i < times; i++)
        {
            value *= t;
        }
        return value;
    }
    
    public static Color Rainbow(float speed = 1f, float saturation = 1f, float value = 1f)
    {
        float hue = Time.time * speed % 1f;
        return Color.HSVToRGB(hue, saturation, value);
    }
    
    
    public static IEnumerator NumberCounter(TextMeshProUGUI text, int targetValue, float duration = 0.5f, string format = "N0")
    {
        int startValue = 0;
        if (int.TryParse(text.text, out int parsed)) startValue = parsed;

        var timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            var progress = timer / duration;
            progress = EaseInOut(progress);
            var value = Mathf.RoundToInt(Mathf.Lerp(startValue, targetValue, progress));
            text.text = value.ToString(format);
            yield return null;
        }

        text.text = targetValue.ToString(format);
    }

    public static string KeyCodeToChar(InputActionReference actionReference, int bindingIndex = 0)
    {
        var action = actionReference.action;
        if (action == null || action.bindings.Count <= bindingIndex)
            return string.Empty;
        return action.GetBindingDisplayString(bindingIndex);
    }
}

public static class ResolutionUtility
{
    public static List<Resolution> GetResolutionsByAspect(float targetAspect, float tolerance = 0.02f)
    {
        return Screen.resolutions
            .Where(r =>
            {
                float aspect = (float)r.width / r.height;
                return Mathf.Abs(aspect - targetAspect) < tolerance;
            })
            .OrderByDescending(r => r.width)
            .ToList();
    }

    public static List<Resolution> GetFourThreeResolutions() => GetResolutionsByAspect(4f / 3f);
    public static List<Resolution> GetSixteenNineResolutions() => GetResolutionsByAspect(16f / 9f);
    public static List<Resolution> GetSixteenTenResolutions() => GetResolutionsByAspect(16f / 10f);
}
