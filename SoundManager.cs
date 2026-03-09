using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("SFX clips")]
    public List<AudioClip> nodeClips = new List<AudioClip>();
    public AudioClip whooshClip;
    public AudioClip successClip;
    public AudioClip failClip;

    [Header("Volumes (0..1)")]
    [Range(0f, 1f)] public float nodeVolume = 0.9f;
    [Range(0f, 1f)] public float whooshVolume = 0.65f;
    [Range(0f, 1f)] public float successVolume = 1.0f;
    [Range(0f, 1f)] public float failVolume = 0.9f;

    [Header("Pool settings")]
    public int poolSize = 8;
    public bool persistentAcrossScenes = true;

    [Header("Playback")]
    public float defaultPitchVariance = 0.02f;

    private List<AudioSource> pool = new List<AudioSource>();
    private int poolIndex = 0;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (persistentAcrossScenes) DontDestroyOnLoad(gameObject);

        pool = new List<AudioSource>(poolSize);
        for (int i = 0; i < Mathf.Max(1, poolSize); i++)
        {
            var go = new GameObject("SFXSrc_" + i);
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.loop = false;
            pool.Add(src);
        }
    }

    AudioSource GetNextSource()
    {
        if (pool == null || pool.Count == 0) return null;
        var s = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Count;
        return s;
    }

    // --- NEW METHOD FOR PATTERN MEMORY GAME ---
    public void PlayNodeHit(float pitch = 1.0f)
    {
        if (nodeClips == null || nodeClips.Count == 0) return;
        
        // Pick the first clip or a random one from your nodeClips list
        AudioClip clip = nodeClips[0]; 
        if (clip == null) return;

        var s = GetNextSource();
        if (s == null) return;

        s.pitch = pitch; // Set the pitch passed by the game script
        s.PlayOneShot(clip, nodeVolume);
    }

    public void PlayClipOneShot(AudioClip clip, float volume = 1f, float pitchVariance = -1f)
    {
        if (clip == null) return;
        var s = GetNextSource();
        if (s == null) return;

        float pv = (pitchVariance < 0f) ? defaultPitchVariance : pitchVariance;
        s.pitch = (pv > 0f) ? Mathf.Clamp(1f + Random.Range(-pv, pv), 0.5f, 2f) : 1f;
        s.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void PlayNodeSound(int shapeIndex)
    {
        if (nodeClips == null || nodeClips.Count == 0) return;
        int safeIndex = Mathf.Clamp(shapeIndex, 0, nodeClips.Count - 1);
        AudioClip clip = nodeClips[safeIndex];
        if (clip == null) return;
        PlayClipOneShot(clip, nodeVolume, defaultPitchVariance * 0.6f);
    }

    public void PlayWhoosh()
    {
        if (whooshClip == null) return;
        PlayClipOneShot(whooshClip, whooshVolume, defaultPitchVariance * 0.5f);
    }

    public void PlaySuccess()
    {
        if (successClip == null) return;
        PlayClipOneShot(successClip, successVolume, defaultPitchVariance * 0.5f);
    }

    public void PlayFail()
    {
        if (failClip == null) return;
        PlayClipOneShot(failClip, failVolume, defaultPitchVariance * 0.5f);
    }

    public void StopAllSFX()
    {
        if (pool == null) return;
        foreach (var s in pool) if (s != null) s.Stop();
    }
}