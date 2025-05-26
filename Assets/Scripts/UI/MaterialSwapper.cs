using UnityEngine;

public class MaterialSwapper : MonoBehaviour
{
    private Material[] originalMaterials;
    private Renderer[] renderers;

    public void SetSilhouette(Material silhouetteMat)
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterial;
            renderers[i].sharedMaterial = silhouetteMat;
        }
    }

    public void RestoreMaterials()
    {
        if (renderers == null || originalMaterials == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sharedMaterial = originalMaterials[i];
        }
    }
}
