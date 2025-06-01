using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For Image components
using TMPro;        // IMPORTANT: For TextMeshPro components

public class OpacityPulseAll : MonoBehaviour
{
    // A list to hold both Image and TMP_Text components
    // We'll process them together since they both have a .color property
    private List<Graphic> pulsableGraphics = new List<Graphic>();

    public float fadeInDuration = 0.2f;  // Fast fade in
    public float fadeOutDuration = 1.0f; // Slow fade out

    private void Awake()
    {
        // Get all Image components in this GameObject and its children
        pulsableGraphics.AddRange(GetComponentsInChildren<Image>());

        // Get all TMP_Text components (which covers both TextMeshProUGUI and TextMeshPro)
        pulsableGraphics.AddRange(GetComponentsInChildren<TMP_Text>());

        // Optional: If you only want to affect components on the current GameObject, uncomment below
        // Image selfImage = GetComponent<Image>();
        // if (selfImage != null)
        // {
        //     pulsableGraphics.Add(selfImage);
        // }
        // TMP_Text selfTMP = GetComponent<TMP_Text>();
        // if (selfTMP != null)
        // {
        //     pulsableGraphics.Add(selfTMP);
        // }

        if (pulsableGraphics.Count == 0)
        {
            Debug.LogWarning("OpacityPulseAll: No Image or TMP_Text components found on this GameObject or its children.", this);
            enabled = false; // Disable the script if nothing to pulse
        }
    }

    private void Start()
    {
        // Start the pulsing loop
        StartCoroutine(PulseLoop());
    }

    private IEnumerator PulseLoop()
    {
        while (true)
        {
            // Fade from 0.25 alpha to 1 alpha
            yield return StartCoroutine(FadeAlpha(0.0f, 0.75f, fadeInDuration));

            // Fade from 1 alpha to 0.25 alpha
            yield return StartCoroutine(FadeAlpha(0.75f, 0.0f, fadeOutDuration));
        }
    }

    private IEnumerator FadeAlpha(float from, float to, float duration)
    {
        float elapsed = 0f;

        // Store original colors to preserve RGB values for all found graphics
        Color[] originalColors = new Color[pulsableGraphics.Count];
        for (int i = 0; i < pulsableGraphics.Count; i++)
        {
            originalColors[i] = pulsableGraphics[i].color;
        }

        while (elapsed < duration)
        {
            // Calculate the current alpha value using Lerp
            float alpha = Mathf.Lerp(from, to, elapsed / duration);

            // Apply the new alpha to each graphic component
            for (int i = 0; i < pulsableGraphics.Count; i++)
            {
                Color c = originalColors[i]; // Get the original RGB
                pulsableGraphics[i].color = new Color(c.r, c.g, c.b, alpha); // Apply new alpha
            }

            elapsed += Time.deltaTime; // Increment elapsed time
            yield return null; // Wait for the next frame
        }

        // Ensure the final alpha is set precisely to 'to' value
        for (int i = 0; i < pulsableGraphics.Count; i++)
        {
            Color c = originalColors[i];
            pulsableGraphics[i].color = new Color(c.r, c.g, c.b, to);
        }
    }
}