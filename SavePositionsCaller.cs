// TransformPositionSaver.cs
// Single drop-in script. Place in Assets/Scripts.
// Press F8 to refresh tracked children. Press K to save once.
// External callers can call RefreshTrackedChildren() and SavePositionsToFile().

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DisallowMultipleComponent]
public class TransformPositionSaver : MonoBehaviour
{
    [Header("Parent to track (optional)")]
    [Tooltip("If left null the script will look for a GameObject named 'ModelWorldRoot' at runtime.")]
    public GameObject parentToTrack;

    [Tooltip("Name used to auto-find the runtime parent if 'parentToTrack' is null.")]
    public string fallbackParentName = "ModelWorldRoot";

    [Tooltip("If true saves localPosition; otherwise saves world position.")]
    public bool saveLocalPosition = true;

    [Tooltip("If true, script will auto-scan children when it finds the parent.")]
    public bool autoScanWhenFound = true;

    [Tooltip("How often (seconds) to attempt auto-find when parent is missing.")]
    public float autoFindInterval = 1f;

    private readonly List<Transform> tracked = new List<Transform>();
    private int lastTrackedCount = -1;
    private float nextAutoFindAt = 0f;
    private string lastSavedPath = "";

    [Serializable]
    public class TransformRecord
    {
        public string name;
        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;
    }

    [Serializable]
    public class TransformRecordCollection
    {
        public string createdAt;
        public string parentName;
        public List<TransformRecord> records = new List<TransformRecord>();
    }

    void Start()
    {
        if (parentToTrack != null && autoScanWhenFound)
        {
            RefreshTracked();
        }
        else
        {
            nextAutoFindAt = Time.time;
        }
    }

    void Update()
    {
        // Manual controls
        if (Input.GetKeyDown(KeyCode.F8))
        {
            RefreshTracked();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            SavePositionsToFile();
        }

        // Auto-find parent if not set
        if (parentToTrack == null && Time.time >= nextAutoFindAt)
        {
            nextAutoFindAt = Time.time + Mathf.Max(0.1f, autoFindInterval);
            GameObject found = GameObject.Find(fallbackParentName);
            if (found != null)
            {
                parentToTrack = found;
                Debug.LogFormat("[TransformPositionSaver] Auto-found parent '{0}'.", fallbackParentName);
                if (autoScanWhenFound) RefreshTracked();
            }
        }

        // keep console quieter: only log when count changes
        if (tracked.Count != lastTrackedCount)
        {
            lastTrackedCount = tracked.Count;
            if (tracked.Count > 0)
                Debug.LogFormat("[TransformPositionSaver] Tracking {0} children on '{1}'. Press K to save. Press F8 to refresh.", tracked.Count, parentToTrack != null ? parentToTrack.name : "(null)");
            else
                Debug.Log("[TransformPositionSaver] No tracked children. Press F8 after the runtime root is created or assign parentToTrack.");
        }
    }

    // Public compatibility methods
    public void RefreshTrackedChildren()
    {
        RefreshTracked();
    }

    public void SavePositionsToFile()
    {
        SaveToFile();
    }

    // Refresh the tracked list (direct children of parentToTrack)
    public void RefreshTracked()
    {
        tracked.Clear();

        if (parentToTrack == null)
        {
            Debug.LogWarningFormat("[TransformPositionSaver] parentToTrack is null. Will attempt to find '{0}' automatically. Press F8 after the runtime root is created.", fallbackParentName);
            return;
        }

        int childCount = parentToTrack.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            tracked.Add(parentToTrack.transform.GetChild(i));
        }
        // lastTrackedCount updated next Update to reduce spam
    }

    // Save currently tracked transforms to JSON file
    public void SaveToFile()
    {
        if (tracked.Count == 0)
        {
            Debug.LogWarning("[TransformPositionSaver] No tracked children found. Call RefreshTrackedChildren() (F8) or ensure parentToTrack exists.");
            return;
        }

        var collection = new TransformRecordCollection();
        collection.createdAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        collection.parentName = parentToTrack != null ? parentToTrack.name : "(null)";

        foreach (var t in tracked)
        {
            var rec = new TransformRecord();
            rec.name = t.name;
            rec.position = saveLocalPosition ? t.localPosition : t.position;
            rec.eulerAngles = t.eulerAngles;
            rec.scale = t.localScale;
            collection.records.Add(rec);
        }

        string json = JsonUtility.ToJson(collection, true);

        string filename = string.Format("saved_positions_{0}.json", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        string folder = Application.persistentDataPath;
        string dir = Path.Combine(folder, "transform_saves");

        try
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            string fullPath = Path.Combine(dir, filename);
            File.WriteAllText(fullPath, json);
            lastSavedPath = fullPath;
            Debug.LogFormat("[TransformPositionSaver] Saved {0} records to: {1}", collection.records.Count, fullPath);
        }
        catch (Exception ex)
        {
            Debug.LogErrorFormat("[TransformPositionSaver] Failed to write file: {0}\n{1}", ex.Message, ex.StackTrace);
        }
    }

    // helpers for other scripts
    public void SetParentToTrack(GameObject parent)
    {
        parentToTrack = parent;
        RefreshTracked();
    }

    public string GetLastSavedPath()
    {
        return lastSavedPath;
    }
}
