using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    private Material[] originalMaterials;
    private Renderer[] renderers;

    [Tooltip("Assign the material you want to swap to here.")]
    public Material swapMaterial; // This is the new material field

    public void SetSilhouette() // Removed the parameter, now uses swapMaterial field
    {
        if (swapMaterial == null)
        {
            Debug.LogWarning("Swap Material is not assigned on " + gameObject.name + ". Cannot set silhouette.", this);
            return;
        }

        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterial;
            renderers[i].sharedMaterial = swapMaterial; // Use the public field
        }
    }

    public void RestoreMaterials()
    {
        if (renderers == null || originalMaterials == null)
        {
            Debug.LogWarning("No materials to restore or renderers not initialized on " + gameObject.name + ".", this);
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            // Check if the renderer still exists before trying to access it
            if (renderers[i] != null)
            {
                renderers[i].sharedMaterial = originalMaterials[i];
            }
            else
            {
                Debug.LogWarning("Renderer at index " + i + " is null, skipping restore for this one.", this);
            }
        }
    }
}