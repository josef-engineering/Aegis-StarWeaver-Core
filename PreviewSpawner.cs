using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple preview spawner: spawns a prefab under a previewStage transform,
/// ensures canvases are world-space and assigned to the preview camera,
/// and optionally clears previously spawned previews.
/// previewStage is a public Transform because other debug/editor tools
/// call .childCount / .GetChild on it.
/// </summary>
public class PreviewSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Optional camera used for preview canvases. If null, Camera.main will be used.")]
    public Camera previewCamera;

    [Tooltip("Parent transform under which previews will be instantiated.")]
    public Transform previewStage;

    [Header("Options")]
    [Tooltip("Name prefix applied to spawned preview instances.")]
    public string previewPrefix = "Preview_";

    private void Start()
    {
        if (previewStage == null)
        {
            Debug.LogError("[PreviewSpawner] previewStage is not assigned! Preview spawning will fail.");
        }
        else
        {
            Debug.Log($"[PreviewSpawner] Ready - previewStage: {previewStage.name}, childCount: {previewStage.childCount}");
        }

        if (previewCamera == null)
        {
            previewCamera = Camera.main;
            Debug.Log($"[PreviewSpawner] Using main camera: {previewCamera?.name ?? "NULL"}");
        }
    }

    public void ClearPreview()
    {
        if (previewStage == null) return;

        List<GameObject> toDestroy = new List<GameObject>();
        for (int i = 0; i < previewStage.childCount; i++)
        {
            var child = previewStage.GetChild(i).gameObject;
            if (child.name.StartsWith(previewPrefix))
                toDestroy.Add(child);
        }

        foreach (var go in toDestroy)
        {
#if UNITY_EDITOR
            DestroyImmediate(go);
#else
            Destroy(go);
#endif
        }
    }

    public GameObject SpawnPreview(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[PreviewSpawner] SpawnPreview called with null prefab.");
            return null;
        }

        ClearPreview();

        GameObject instance;
        if (previewStage != null)
        {
            instance = Instantiate(prefab, previewStage);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }
        else
        {
            Debug.LogWarning("[PreviewSpawner] previewStage not assigned — instantiating at world origin.");
            instance = Instantiate(prefab);
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
        }

        instance.name = previewPrefix + prefab.name;

        // Set layer to "Preview" if exists
        int previewLayer = LayerMask.NameToLayer("Preview");
        if (previewLayer >= 0)
            SetLayerRecursively(instance, previewLayer);

        // Configure Canvas components
        Camera cam = previewCamera != null ? previewCamera : Camera.main;
        var canvases = instance.GetComponentsInChildren<Canvas>(true);
        foreach (var c in canvases)
        {
            if (c.renderMode != RenderMode.WorldSpace)
                c.renderMode = RenderMode.WorldSpace;

            c.transform.localPosition = Vector3.zero;
            c.transform.localRotation = Quaternion.identity;
            c.transform.localScale = Vector3.one;

            if (cam != null)
                c.worldCamera = cam;
        }

        return instance;
    }

    private void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

#if UNITY_EDITOR
    // Editor helper: auto-assign references for quick testing
    private void Reset()
    {
        if (previewStage == null)
        {
            var found = GameObject.Find("PreviewStage");
            if (found != null) previewStage = found.transform;
        }

        if (previewCamera == null)
        {
            var camGO = GameObject.Find("ThemePreviewCamera");
            if (camGO != null)
            {
                var cam = camGO.GetComponent<Camera>();
                if (cam != null) previewCamera = cam;
            }
        }
    }
#endif
}
