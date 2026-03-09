using UnityEngine;
using UnityEngine.UI;

public class ShopPreviewSystem : MonoBehaviour
{
    public Camera modelViewCamera; // Assign your ModelViewCamera here
    public GameObject modelPrefab; // Assign your Circle.010 prefab here
    public GameObject backgroundQuadPrefab; // Assign your background quad prefab here
    public int previewLayer = 5; // Set the layer for the preview objects
    
    [Header("Camera Settings")]
    public float cameraZoomZ = -50f; // Bringing this closer makes the image "Big"

    private GameObject spawnedModel;
    private GameObject backgroundQuad;

    public void PreviewModel()
    {
        // 1. FORCE THE CAMERA TO THE CENTER (Fixes the -642, -1389 issue)
        ResetCameraUI();

        // 2. Clean up old previews if they exist
        if (spawnedModel != null) Destroy(spawnedModel);
        if (backgroundQuad != null) Destroy(backgroundQuad);

        // 3. Instantiate and center the model
        spawnedModel = Instantiate(modelPrefab, Vector3.zero, Quaternion.identity);
        SetLayerRecursively(spawnedModel, previewLayer);

        Renderer modelRenderer = spawnedModel.GetComponentInChildren<Renderer>();
        if (modelRenderer != null)
        {
            // Calculate visual center
            Bounds combinedBounds = modelRenderer.bounds;
            Vector3 modelOffset = combinedBounds.center - spawnedModel.transform.position;

            // Shift model so mesh center is at 0,0,0
            spawnedModel.transform.position = new Vector3(-modelOffset.x, -modelOffset.y, 0);
        }

        // 4. Instantiate and position background
        backgroundQuad = Instantiate(backgroundQuadPrefab, Vector3.zero, Quaternion.identity);
        SetLayerRecursively(backgroundQuad, previewLayer);
        
        // Place background behind the model (Z=20) relative to the model at Z=0
        backgroundQuad.transform.position = new Vector3(0, 0, 20);
        backgroundQuad.transform.localScale = new Vector3(100, 100, 1); // Make it huge

        // 5. Ensure camera settings are correct
        modelViewCamera.farClipPlane = 5000;
    }

    private void ResetCameraUI()
    {
        if (modelViewCamera == null) return;

        // Force the RectTransform to fill the screen (Fixes "Small Image" issue)
        RectTransform rect = modelViewCamera.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Force the X and Y positions to 0 (Fixes the -642, -1389 issue)
            rect.anchoredPosition3D = new Vector3(0, 0, cameraZoomZ);
        }

        // Ensure the camera is looking straight ahead at the world origin
        modelViewCamera.transform.localPosition = new Vector3(0, 0, cameraZoomZ);
        modelViewCamera.transform.localRotation = Quaternion.identity;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}