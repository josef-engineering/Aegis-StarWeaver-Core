using UnityEngine;

public class MaterialMemory : MonoBehaviour
{
    private Material originalMaterial;
    private Renderer myRenderer;

    void Awake()
    {
        // 1. Get the renderer (where your baked orange/purple texture lives)
        myRenderer = GetComponent<Renderer>();
        
        // 2. Remember the UNIQUE material assigned by the loadout shop
        if (myRenderer != null) {
            originalMaterial = myRenderer.material;
        }
    }

    // 3. This function allows your pattern code to safely swap back
    public void RestoreOriginalMaterial()
    {
        if (myRenderer != null && originalMaterial != null) {
            myRenderer.material = originalMaterial;
        }
    }
}