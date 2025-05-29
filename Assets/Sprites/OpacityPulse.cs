using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpacityPulse : MonoBehaviour
{
    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    public float fadeInDuration = 0.2f;  // Fast fade in
    public float fadeOutDuration = 1.0f; // Slow fade out

    private void Awake()
    {
        // Get all SpriteRenderers in this GameObject and its children
        spriteRenderers.AddRange(GetComponentsInChildren<SpriteRenderer>());
    }

    private void Start()
    {
        StartCoroutine(PulseLoop());
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return StartCoroutine(FadeAlpha(0.25f, 1f, fadeInDuration));
            yield return StartCoroutine(FadeAlpha(1f, 0.25f, fadeOutDuration));
        }
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;

        // Assume all sprites have the same starting color
        Color[] originalColors = new Color[spriteRenderers.Count];
        for (int i = 0; i < spriteRenderers.Count; i++)
            originalColors[i] = spriteRenderers[i].color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(from, to, elapsed / duration);
            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                Color c = originalColors[i];
                spriteRenderers[i].color = new Color(c.r, c.g, c.b, alpha);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final alpha
        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            Color c = originalColors[i];
            spriteRenderers[i].color = new Color(c.r, c.g, c.b, to);
        }
    }
}
