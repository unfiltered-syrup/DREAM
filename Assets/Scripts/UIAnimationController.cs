using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Canvas))]
public class UIAnimationController : MonoBehaviour
{
    private Animator animator;
    private Canvas canvas;
    [SerializeField] private AnimationClip defaultClip;
    
    [SerializeField] private bool playOnEnable;
    [SerializeField] private AnimationClip motionIn;
    [SerializeField] private AnimationClip motionOut;
    [Tooltip("This event is called before the in animation and after the out animation.")]
    [SerializeField] private UnityEvent<bool> onCanvasToggleWithMotionEvent;
    [Tooltip("This event is called after the in animation and before the out animation.")]
    [SerializeField] private UnityEvent<bool> onCanvasToggleBetweenMotionEvent;
    // BEFORE in animation and AFTER out animation
    public delegate void CanvasToggleMotionDelegate(bool isIn);
    public CanvasToggleMotionDelegate OnCanvasToggleWithMotion;
    // Between is AFTER in animation and BEFORE out animation
    public delegate void CanvasToggleMotionBetweenDelegate(bool isIn);
    public CanvasToggleMotionBetweenDelegate OnCanvasToggleBetweenMotion;
    
    public bool IsVisible => canvas.enabled;
    
    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        canvas = GetComponent<Canvas>();
        if (playOnEnable)
        {
            PlayAnimation(true);
        }
    }
    
    private void Start()
    {
        if (defaultClip != null)
        {
            animator.Play(defaultClip.name);
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
    
    private void PlayAnimation(bool isIn)
    {
        if (motionIn == null && motionOut == null)
        {
            canvas.enabled = isIn;
            return;
        }
        StopAllCoroutines();
        if (motionOut == null && !isIn)
        {
            canvas.enabled = false;
            OnCanvasToggleWithMotion?.Invoke(false);
            onCanvasToggleWithMotionEvent?.Invoke(false);
            onCanvasToggleBetweenMotionEvent?.Invoke(false);
            OnCanvasToggleBetweenMotion?.Invoke(false);
            return;
        }
        animator.Play(isIn ? motionIn.name : motionOut.name);
        StartCoroutine(AfterAnimation(isIn, isIn ? motionIn : motionOut));
    }
    
    private IEnumerator AfterAnimation(bool isIn, AnimationClip clip)
    {
        yield return new WaitForSeconds(.05f);
        if (isIn)
        {
            onCanvasToggleWithMotionEvent?.Invoke(true); // DO NOT REMOVE, idk why but it's needed
            OnCanvasToggleWithMotion?.Invoke(true);
        }
        else
        {
            onCanvasToggleBetweenMotionEvent?.Invoke(false);
            OnCanvasToggleBetweenMotion?.Invoke(false);
        }

        if (clip.length > 0)
        {
            yield return new WaitForSeconds(clip.length);
        }
       
        if (isIn)
        {
            animator.enabled = false;
            animator.Rebind();
            onCanvasToggleBetweenMotionEvent?.Invoke(true);
            OnCanvasToggleBetweenMotion?.Invoke(true);
        }
        else
        {
            OnCanvasToggleWithMotion?.Invoke(false);
            onCanvasToggleWithMotionEvent?.Invoke(false);
            canvas.enabled = false;
            // _animator.Rebind();
        }
    }

    public void ToggleCanvasWithMotion(bool enable)
    {
        if (!enable && !canvas.enabled) return;
        canvas.enabled = true;
        animator.enabled = true;
        PlayAnimation(enable);
    }

    public void ToggleCanvasWithoutMotion(bool isVisible)
    {
        canvas.enabled = isVisible;
        animator.enabled = false;
    }
}
