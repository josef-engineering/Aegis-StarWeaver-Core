using UnityEngine;

public sealed class AegisSkinLoader : MonoBehaviour
{
    [Header("Socket Setup")]
    public Transform skinSocket; // Drag your empty "Skin_Socket" child here

    void Start()
    {
        LoadEquippedSkin();
    }

    public void LoadEquippedSkin()
    {
        // 1. Clear any existing skin in the socket to avoid overlaps
        foreach (Transform child in skinSocket) {
            Destroy(child.gameObject);
        }

        // 2. Get the name of the skin the player chose (default to Aegis_Star)
        string skinToLoad = PlayerPrefs.GetString("SelectedSkin", "Skin_Aegis_Star");

        // 3. Load the prefab from a folder named "Resources/Skins"
        GameObject skinPrefab = Resources.Load<GameObject>("Skins/" + skinToLoad);

        if (skinPrefab != null) {
            // 4. "Plug it in" to the socket
            Instantiate(skinPrefab, skinSocket);
        } else {
            Debug.LogError($"Skin {skinToLoad} not found in Resources/Skins! Check your naming.");
        }
    }
}