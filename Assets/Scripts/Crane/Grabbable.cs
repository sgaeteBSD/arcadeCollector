// GrabbableSqueeze.cs
using UnityEngine;
using System.Collections;

public class GrabbableSqueeze : MonoBehaviour
{
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;

    [Tooltip("Factor by which to multiply the original scale when squeezed (e.g., 0.9 for 90%).")]
    public float squeezeScaleFactor = 0.9f;
    [Tooltip("Factor for Y axis, if different (e.g., for a plush toy that squashes more vertically). Use 1 to keep proportional to XZ.")]
    public float squeezeYScaleFactorMultiplier = 1f;


    [Tooltip("Duration of the squeeze animation in seconds.")]
    public float squeezeDuration = 0.15f;
    [Tooltip("Duration of the unsqueeze animation in seconds.")]
    public float unsqueezeDuration = 0.2f;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void ApplySqueeze()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        Vector3 targetScale = new Vector3(
            originalScale.x * squeezeScaleFactor,
            originalScale.y * squeezeScaleFactor * squeezeYScaleFactorMultiplier,
            originalScale.z * squeezeScaleFactor // Assuming 2D, Z might be uniform or originalScale.z
        );
        scaleCoroutine = StartCoroutine(AnimateScale(targetScale, squeezeDuration));
    }

    public void ReleaseSqueeze()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(AnimateScale(originalScale, unsqueezeDuration));
    }

    IEnumerator AnimateScale(Vector3 targetScale, float duration)
    {
        Vector3 initialScale = transform.localScale;
        float timer = 0f;

        while (timer < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        scaleCoroutine = null;
    }

    // Call this if the object is destroyed or reset while squeezed
    public void ResetScaleImmediate()
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        transform.localScale = originalScale;
        scaleCoroutine = null;
    }

    // Ensure reset on disable/destroy if it might be grabbed
    void OnDisable()
    {
        // If you want it to visually reset if it's disabled while grabbed
        // ResetScaleImmediate();
    }
}