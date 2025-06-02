using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class OpacityPulseAll : MonoBehaviour
{
    private List<Graphic> pulsableGraphics = new List<Graphic>();

    public float fadeInDuration = 0.2f; public float fadeOutDuration = 1.0f;
    private void Awake()
    {
        pulsableGraphics.AddRange(GetComponentsInChildren<Image>());

        pulsableGraphics.AddRange(GetComponentsInChildren<TMP_Text>());


        if (pulsableGraphics.Count == 0)
        {
            Debug.LogWarning("OpacityPulseAll: No Image or TMP_Text components found on this GameObject or its children.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        StartCoroutine(PulseLoop());
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            yield return StartCoroutine(FadeAlpha(0.0f, 0.75f, fadeInDuration));

            yield return StartCoroutine(FadeAlpha(0.75f, 0.0f, fadeOutDuration));
        }
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;

        Color[] originalColors = new Color[pulsableGraphics.Count];
        for (int i = 0; i < pulsableGraphics.Count; i++)
        {
            originalColors[i] = pulsableGraphics[i].color;
        }

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(from, to, elapsed / duration);

            for (int i = 0; i < pulsableGraphics.Count; i++)
            {
                Color c = originalColors[i]; pulsableGraphics[i].color = new Color(c.r, c.g, c.b, alpha);
            }

            elapsed += Time.deltaTime; yield return null;
        }

        for (int i = 0; i < pulsableGraphics.Count; i++)
        {
            Color c = originalColors[i];
            pulsableGraphics[i].color = new Color(c.r, c.g, c.b, to);
        }
    }
}