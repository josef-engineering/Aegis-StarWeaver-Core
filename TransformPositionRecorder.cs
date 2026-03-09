using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Attach to the parent that holds the preview instances (for example ModelWorldRoot).
/// - Quietly tracks local transform (pos/rot/scale) of all child Transforms (recursively).
/// - Press F9 in Play mode to save current tracked transforms to JSON (Application.persistentDataPath).
/// - Does NOT spam console for every small movement; only logs a short summary on save.
/// - Saved data uses GameObject name as identifier (if names duplicate, index appended).
/// </summary>
public class TransformPositionRecorder : MonoBehaviour
{
    [Tooltip("Filename (JSON) saved to Application.persistentDataPath")]
    public string positionsFileName = "spawn_positions.json";

    [Tooltip("If true, start tracking automatically in Play mode.")]
    public bool autoTrack = true;

    // internal storage
    private Dictionary<string, TrackedTransform> tracked = new Dictionary<string, TrackedTransform>();

    // small epsilon for change detection (not strict equality)
    private const float posEps = 0.0001f;
    private const float rotEps = 0.0001f;
    private const float scaleEps = 0.0001f;

    void Start()
    {
        if (autoTrack) RefreshTrackedChildren();
    }

    void Update()
    {
        // Manual refresh: press F8 to regenerate tracked list from current children
        if (Input.GetKeyDown(KeyCode.F8))
        {
            RefreshTrackedChildren();
            Debug.Log($"[TPR] Refreshed tracked children. Count={tracked.Count}");
        }

        // Save positions on F9 (no spam)
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SavePositionsToFile();
        }

        // optional: toggle auto-track (not noisy)
        if (Input.GetKeyDown(KeyCode.F7))
        {
            autoTrack = !autoTrack;
            if (autoTrack) RefreshTrackedChildren();
            Debug.Log($"[TPR] autoTrack = {autoTrack}");
        }

        if (!autoTrack) return;

        // Update internal cached values when transforms move (no console logging)
        foreach (var kv in tracked)
        {
            var tt = kv.Value;
            var t = tt.target;
            if (t == null) continue;

            Vector3 p = t.localPosition;
            Quaternion r = t.localRotation;
            Vector3 s = t.localScale;

            if (!ApproximatelyEqual(p, tt.lastLocalPosition, posEps) ||
                !ApproximatelyEqual(r.eulerAngles, tt.lastLocalRotationEuler, rotEps) ||
                !ApproximatelyEqual(s, tt.lastLocalScale, scaleEps))
            {
                tt.lastLocalPosition = p;
                tt.lastLocalRotationEuler = r.eulerAngles;
                tt.lastLocalScale = s;
                tt.dirty = true;
            }
        }
    }

    /// <summary>
    /// Scan children recursively and start tracking them.
    /// </summary>
    public void RefreshTrackedChildren()
    {
        tracked.Clear();
        int counter = 0;
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
        {
            // skip self
            if (child == this.transform) continue;

            // create a unique key using name + index to avoid collisions
            string key = $"{child.gameObject.name}_{counter}";
            tracked[key] = new TrackedTransform
            {
                key = key,
                target = child,
                lastLocalPosition = child.localPosition,
                lastLocalRotationEuler = child.localRotation.eulerAngles,
                lastLocalScale = child.localScale,
                dirty = true
            };
            counter++;
        }
    }

    /// <summary>
    /// Saves the currently tracked transforms to JSON file.
    /// Press F9 in Play mode to call this.
    /// </summary>
    public void SavePositionsToFile()
    {
        List<SpawnRecord> list = new List<SpawnRecord>();
        foreach (var kv in tracked)
        {
            var tt = kv.Value;
            var t = tt.target;
            if (t == null) continue;

            // Build record. Use original GameObject name (without appended index) as prefabName baseline,
            // but we keep the full key so duplicates are distinct.
            string prefabName = t.gameObject.name;
            // If the name contains "(Clone)" remove it (just in case)
            prefabName = prefabName.Replace("(Clone)", "").Trim();

            list.Add(new SpawnRecord
            {
                key = tt.key,
                prefabName = prefabName,
                localPosition = tt.lastLocalPosition,
                localRotationEuler = tt.lastLocalRotationEuler,
                localScale = tt.lastLocalScale
            });
        }

        var wrapper = new SpawnRecordList { records = list.ToArray() };
        string json = JsonUtility.ToJson(wrapper, true);

        string fullPath = Path.Combine(Application.persistentDataPath, positionsFileName);
        try
        {
            File.WriteAllText(fullPath, json);
            Debug.Log($"[TPR] Saved {list.Count} records to: {fullPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[TPR] Failed to save positions: {e.Message}");
        }
    }

    // Helper: small vector equality
    private bool ApproximatelyEqual(Vector3 a, Vector3 b, float eps)
    {
        return Mathf.Abs(a.x - b.x) <= eps && Mathf.Abs(a.y - b.y) <= eps && Mathf.Abs(a.z - b.z) <= eps;
    }

    private bool ApproximatelyEqual(Vector3 a, Vector3 b, double epsDouble)
    {
        float eps = (float)epsDouble;
        return ApproximatelyEqual(a, b, eps);
    }

    // Data classes
    [Serializable]
    private class TrackedTransform
    {
        public string key;
        public Transform target;
        public Vector3 lastLocalPosition;
        public Vector3 lastLocalRotationEuler;
        public Vector3 lastLocalScale;
        public bool dirty;
    }

    [Serializable]
    public class SpawnRecord
    {
        public string key;              // unique key we generated
        public string prefabName;       // name used to locate prefab in Resources
        public Vector3 localPosition;
        public Vector3 localRotationEuler;
        public Vector3 localScale;
    }

    [Serializable]
    public class SpawnRecordList
    {
        public SpawnRecord[] records;
    }

    /// <summary>
    /// Quick utility: returns the full path to the file in persistentDataPath.
    /// </summary>
    public string GetFullSavePath()
    {
        return Path.Combine(Application.persistentDataPath, positionsFileName);
    }
}
