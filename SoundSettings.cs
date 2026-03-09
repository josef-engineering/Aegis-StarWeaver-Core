using UnityEngine;
using UnityEngine.UI;

public class SoundSettings : MonoBehaviour
{
    [Header("UI Toggles")]
    public Toggle soundToggle;      // Drag Sound Checkbox here
    public Toggle vibrationToggle;  // Drag Vibration Checkbox here

    void Start()
    {
        // --- 1. SETUP SOUND ---
        bool isMuted = PlayerPrefs.GetInt("Muted", 0) == 1; // Default: Not Muted (0)
        if (soundToggle != null)
        {
            soundToggle.isOn = !isMuted;
            soundToggle.onValueChanged.AddListener(OnSoundChanged);
        }
        AudioListener.volume = isMuted ? 0 : 1;

        // --- 2. SETUP VIBRATION ---
        bool isVibOn = PlayerPrefs.GetInt("VibrationEnabled", 1) == 1; // Default: ON (1)
        if (vibrationToggle != null)
        {
            vibrationToggle.isOn = isVibOn;
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
        }
    }

    public void OnSoundChanged(bool isOn)
    {
        AudioListener.volume = isOn ? 1 : 0;
        PlayerPrefs.SetInt("Muted", isOn ? 0 : 1);
        PlayerPrefs.Save();
    }

    public void OnVibrationChanged(bool isOn)
    {
        PlayerPrefs.SetInt("VibrationEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
        
        // Optional: Tiny buzz to confirm it works
        if (isOn) Handheld.Vibrate();
    }
}