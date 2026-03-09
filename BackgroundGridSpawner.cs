using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// BackgroundGridSpawner
/// - Loads all prefabs from Resources/<resourcesFolder> (or fill list in Inspector)
/// - Spawns them under a world-space root (modelWorldRoot)
/// - Default behaviour: arrange in pairs (every two prefabs share same X/Y) in a grid
/// - Optional: useManualPositions = true to spawn using the exact manualPositions/manualRotationsEuler/manualScales lists
/// - Provides DumpSpawnPositions() to print & copy a C# spawn snippet for the current spawned instances
/// </summary>
[DisallowMultipleComponent]
public class BackgroundGridSpawner : MonoBehaviour
{
    [Header("Resources")]
    [Tooltip("Folder inside Resources/ where your .prefab assets live (no slashes).")]
    public string resourcesFolder = "ModelPrefabs";

    [Header("Prefab list (optional)")]
    [Tooltip("If you want to populate manually in Inspector, assign prefabs here. Otherwise the script can auto-load from Resources.")]
    public List<GameObject> prefabList = new List<GameObject>();

    [Header("Parent/Root (world-space)")]
    [Tooltip("If null, a runtime ModelWorldRoot will be created at scene root.")]
    public Transform modelWorldRoot;

    [Header("Pair Grid Layout (used if useManualPositions == false)")]
    [Tooltip("How many pair columns (number of pair slots per row).")]
    public int pairColumns = 6;
    [Tooltip("If >0, limits pair rows (otherwise computed automatically).")]
    public int pairRows = 0;
    [Tooltip("Spacing between pair slots in world units (x = horizontal, y = vertical).")]
    public Vector2 cellSpacing = new Vector2(3f, 3f);
    [Tooltip("Origin (top-left) world position for the first pair slot.")]
    public Vector3 origin = new Vector3(50f, 50f, 0f);
    [Tooltip("Whether the two items in a pair should stack at same X/Y.")]
    public bool stackPairs = true;
    [Tooltip("If not stacking, offset the second item in the pair vertically by this amount.")]
    public float pairSecondYOffset = -0.25f;

    [Header("Z Offset (avoid z-fight when stacked)")]
    public bool applyInstanceZOffset = true;
    public float instanceZStep = 0.02f;

    [Header("Scaling / Fit")]
    [Tooltip("Target maximum dimension for each preview (world units).")]
    public float targetMaxDimension = 1.5f;
    [Tooltip("Clamp the uniform scale to avoid insanely large numbers.")]
    public float maxUniformScale = 50f;
    public float minUniformScale = 0.0005f;

    [Header("Manual positions (if useManualPositions = true)")]
    [Tooltip("If enabled the spawner will use these lists to position rotate and scale spawned prefabs instead of grid layout.")]
    public bool useManualPositions = false;
    public List<Vector3> manualPositions = new List<Vector3>();
    [Tooltip("Euler rotations in degrees (x,y,z) for each prefab when using manual positions.")]
    public List<Vector3> manualRotationsEuler = new List<Vector3>();
    public List<Vector3> manualScales = new List<Vector3>();

    [Header("Behavior & Debug")]
    public bool spawnOnStart = true;
    public bool autoPopulateFromResources = true;
    public bool verboseLogging = true;

    // runtime
    private List<GameObject> modelPrefabs = new List<GameObject>();
    private List<GameObject> spawnedInstances = new List<GameObject>();
    private Material fallbackMaterial;

    void Start()
    {
        // Ensure root exists
        if (modelWorldRoot == null)
        {
            GameObject go = new GameObject("ModelWorldRoot");
            go.transform.SetParent(null);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            modelWorldRoot = go.transform;
            if (verboseLogging) Debug.Log("[BackgroundGridSpawner] Created ModelWorldRoot at scene root.");
        }

        // Populate the internal prefab list either from inspector list or Resources
        if (autoPopulateFromResources && (prefabList == null || prefabList.Count == 0))
        {
            LoadPrefabsFromResources();
        }
        else
        {
            // copy inspector list into working list
            modelPrefabs = new List<GameObject>(prefabList);
            if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Using prefabList from Inspector. Count={modelPrefabs.Count}");
        }

        if (spawnOnStart) SpawnAll();
    }

    /// <summary>
    /// Load all prefabs from Resources/<resourcesFolder>
    /// </summary>
    public void LoadPrefabsFromResources()
    {
        modelPrefabs.Clear();
        var arr = Resources.LoadAll<GameObject>(resourcesFolder);
        if (arr != null) modelPrefabs.AddRange(arr);
        if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Loaded {modelPrefabs.Count} prefab(s) from Resources/{resourcesFolder}");
    }

    /// <summary>
    /// Spawn all loaded prefabs
    /// </summary>
    public void SpawnAll()
    {
        ClearSpawned();

        if (modelPrefabs == null || modelPrefabs.Count == 0)
        {
            Debug.LogWarning("[BackgroundGridSpawner] No prefabs loaded. Call LoadPrefabsFromResources() or populate list in Inspector.");
            return;
        }

        if (useManualPositions)
        {
            SpawnUsingManualPositions();
        }
        else
        {
            SpawnUsingGridPairs();
        }

        if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Finished spawning {spawnedInstances.Count} instances.");
    }

    private void SpawnUsingManualPositions()
    {
        int total = modelPrefabs.Count;

        if (manualPositions == null || manualPositions.Count < total)
        {
            Debug.LogWarning($"[BackgroundGridSpawner] manualPositions list length ({manualPositions?.Count ?? 0}) is less than prefab count ({total}). Falling back to grid spawn for remaining items.");
        }

        for (int i = 0; i < total; i++)
        {
            GameObject prefab = modelPrefabs[i];
            if (prefab == null)
            {
                Debug.LogWarning($"[BackgroundGridSpawner] Prefab at index {i} is null - skipping.");
                continue;
            }

            GameObject instance = Instantiate(prefab, modelWorldRoot);
            instance.name = prefab.name + "_BGPreview";

            SanitizeInstance(instance);

            // choose transform from manual lists if available
            Vector3 pos = (manualPositions != null && i < manualPositions.Count) ? manualPositions[i] : Vector3.zero;
            Vector3 euler = (manualRotationsEuler != null && i < manualRotationsEuler.Count) ? manualRotationsEuler[i] : Vector3.zero;
            Vector3 scale = (manualScales != null && i < manualScales.Count) ? manualScales[i] : Vector3.one;

            instance.transform.localPosition = pos;
            instance.transform.localRotation = Quaternion.Euler(euler);
            instance.transform.localScale = scale;

            EnsureFallbackMaterial(instance);
            TryFitModelToTargetSize(instance, targetMaxDimension); // optional - will scale unless manualScale provided; user may want to disable this if manualScales used

            spawnedInstances.Add(instance);
            if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Spawned '{instance.name}' at {pos} (manual index {i}).");
        }
    }

    private void SpawnUsingGridPairs()
    {
        int totalPrefabs = modelPrefabs.Count;
        int totalPairs = Mathf.CeilToInt(totalPrefabs / 2f);
        int cols = Mathf.Max(1, pairColumns);
        int rows = pairRows > 0 ? pairRows : Mathf.CeilToInt((float)totalPairs / cols);

        if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Spawning {totalPrefabs} prefabs as {totalPairs} pairs in grid {cols}x{rows}.");

        int prefabIndex = 0;
        int instanceCounter = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                if (prefabIndex >= totalPrefabs) break;

                float x = origin.x + c * cellSpacing.x;
                float y = origin.y - r * cellSpacing.y;
                float zBase = origin.z;

                for (int pairSlot = 0; pairSlot < 2 && prefabIndex < totalPrefabs; pairSlot++)
                {
                    GameObject prefab = modelPrefabs[prefabIndex];
                    if (prefab == null)
                    {
                        Debug.LogWarning($"[BackgroundGridSpawner] Prefab at index {prefabIndex} is null - skipping.");
                        prefabIndex++;
                        continue;
                    }

                    GameObject instance = Instantiate(prefab, modelWorldRoot);
                    instance.name = prefab.name + "_BGPreview";

                    SanitizeInstance(instance);

                    Vector3 localPos;
                    if (stackPairs)
                    {
                        localPos = new Vector3(x, y, zBase);
                    }
                    else
                    {
                        float yOffset = (pairSlot == 0) ? 0f : pairSecondYOffset;
                        localPos = new Vector3(x, y + yOffset, zBase);
                    }

                    if (applyInstanceZOffset)
                        localPos.z += instanceCounter * instanceZStep;

                    instance.transform.localPosition = localPos;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;

                    EnsureFallbackMaterial(instance);
                    TryFitModelToTargetSize(instance, targetMaxDimension);

                    spawnedInstances.Add(instance);
                    if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Spawned '{instance.name}' at {localPos} (index {prefabIndex}).");

                    prefabIndex++;
                    instanceCounter++;
                }

                if (prefabIndex >= totalPrefabs) break;
            }
            if (prefabIndex >= totalPrefabs) break;
        }
    }

    /// <summary>
    /// Destroy all spawned preview instances.
    /// </summary>
    public void ClearSpawned()
    {
        for (int i = spawnedInstances.Count - 1; i >= 0; i--)
        {
            if (spawnedInstances[i] != null) Destroy(spawnedInstances[i]);
        }
        spawnedInstances.Clear();
    }

    // ---------------- helpers ----------------

    private void SanitizeInstance(GameObject go)
    {
        if (go == null) return;

        var monos = go.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (m == null) continue;
            string name = m.GetType().Name.ToLower();
            if (name.Contains("rotate") || name.Contains("spin") || name.Contains("autorotate"))
            {
                if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Removing '{m.GetType().Name}' from '{go.name}'");
                DestroyImmediate(m);
            }
        }

        var anim = go.GetComponentInChildren<Animator>(true);
        if (anim != null) anim.enabled = false;
        var legacy = go.GetComponentInChildren<Animation>(true);
        if (legacy != null) legacy.enabled = false;

        var rbs = go.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePosition;
        }
    }

    private void EnsureFallbackMaterial(GameObject root)
    {
        if (root == null) return;

        if (fallbackMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? Shader.Find("Unlit/Color");
            fallbackMaterial = new Material(shader ?? Shader.Find("Standard"));
            fallbackMaterial.name = "BGSpawner_FallbackMat";
            if (fallbackMaterial.HasProperty("_BaseColor")) fallbackMaterial.SetColor("_BaseColor", Color.white);
            else if (fallbackMaterial.HasProperty("_Color")) fallbackMaterial.SetColor("_Color", Color.white);
        }

        var rends = root.GetComponentsInChildren<Renderer>(true);
        foreach (var r in rends)
        {
            try
            {
                if (r.sharedMaterial == null)
                {
                    r.sharedMaterial = fallbackMaterial;
                    if (verboseLogging) Debug.LogWarning($"[BackgroundGridSpawner] Assigned fallback material to renderer '{r.gameObject.name}' (null material).");
                }
                else
                {
                    string shaderName = r.sharedMaterial.shader != null ? r.sharedMaterial.shader.name : "<null>";
                    if (string.IsNullOrEmpty(shaderName) || shaderName.Contains("Hidden/InternalErrorShader"))
                    {
                        r.sharedMaterial = fallbackMaterial;
                        if (verboseLogging) Debug.LogWarning($"[BackgroundGridSpawner] Replaced invalid shader on '{r.gameObject.name}' with fallback.");
                    }
                }
            }
            catch
            {
                r.sharedMaterial = fallbackMaterial;
                if (verboseLogging) Debug.LogWarning($"[BackgroundGridSpawner] Material check failed on '{r.gameObject.name}'. Using fallback.");
            }
        }
    }

    private void TryFitModelToTargetSize(GameObject instance, float targetSize)
    {
        if (instance == null || targetSize <= 0f) return;

        var rends = instance.GetComponentsInChildren<Renderer>(true);
        if (rends == null || rends.Length == 0)
        {
            if (verboseLogging) Debug.LogWarning($"[BackgroundGridSpawner] '{instance.name}' has no renderers; skipping fit.");
            return;
        }

        Bounds combined = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) combined.Encapsulate(rends[i].bounds);

        float maxDim = Mathf.Max(combined.size.x, Mathf.Max(combined.size.y, combined.size.z));
        if (maxDim <= 1e-6f)
        {
            // try mesh bounds fallback
            float fallbackMax = 0f;
            foreach (var r in rends)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null && mf.sharedMesh != null)
                {
                    var mb = mf.sharedMesh.bounds;
                    fallbackMax = Mathf.Max(fallbackMax, Mathf.Max(mb.size.x, Mathf.Max(mb.size.y, mb.size.z)));
                }
            }
            if (fallbackMax <= 1e-6f)
            {
                if (verboseLogging) Debug.LogWarning($"[BackgroundGridSpawner] Could not compute bounds for '{instance.name}'.");
                return;
            }
            maxDim = fallbackMax;
        }

        float uniformScale = targetSize / maxDim;
        uniformScale = Mathf.Clamp(uniformScale, minUniformScale, maxUniformScale);
        instance.transform.localScale = Vector3.one * uniformScale;

        if (verboseLogging) Debug.Log($"[BackgroundGridSpawner] Fitted '{instance.name}' (maxDim={maxDim:F4}) with scale {uniformScale:F4}.");
    }

    /// <summary>
    /// Dumps positions/rotations/scales of spawnedInstances to Console,
    /// and copies a C# snippet to the clipboard matching the spawn order.
    /// Call this after SpawnAll() while in Play mode.
    /// </summary>
    public void DumpSpawnPositions()
    {
        if (spawnedInstances == null || spawnedInstances.Count == 0)
        {
            Debug.LogWarning("[BackgroundGridSpawner] No spawned instances to dump.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("// Spawn snippet generated by BackgroundGridSpawner.DumpSpawnPositions()");
        sb.AppendLine($"// Count = {spawnedInstances.Count}");
        sb.AppendLine("/* --- begin spawn snippet --- */");
        for (int i = 0; i < spawnedInstances.Count; i++)
        {
            var inst = spawnedInstances[i];
            if (inst == null) continue;
            Vector3 p = inst.transform.localPosition;
            Vector3 e = inst.transform.localEulerAngles;
            Vector3 s = inst.transform.localScale;
            string name = inst.name.Replace("\"", "\\\"");
            sb.AppendLine($"// {i}: {name}");
            sb.AppendLine($"positions[{i}] = new Vector3({p.x:F6}f, {p.y:F6}f, {p.z:F6}f);");
            sb.AppendLine($"rotations[{i}] = Quaternion.Euler({e.x:F4}f, {e.y:F4}f, {e.z:F4}f);");
            sb.AppendLine($"scales[{i}] = new Vector3({s.x:F6}f, {s.y:F6}f, {s.z:F6}f);");
            sb.AppendLine();
        }
        sb.AppendLine("/* --- end spawn snippet --- */");

        Debug.Log(sb.ToString());
        GUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log("[BackgroundGridSpawner] Spawn snippet copied to clipboard.");
    }
}
