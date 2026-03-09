#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor utility to move any top-level "Skins" GameObject children
/// into the proper ShopContent/Skins container.
/// </summary>
public static class ShopFixerEditor
{
    [MenuItem("Tools/Shop/Move top-level Skins into ShopContent/Skins")]
    public static void MoveTopLevelSkinsIntoShopContent()
    {
        // Look for top-level "Skins" object
        var topSkins = GameObject.Find("Skins");
        if (topSkins == null)
        {
            Debug.Log("No top-level 'Skins' GameObject found in scene.");
            return;
        }

        // Try to find ShopContent
        GameObject shopContent = GameObject.Find("ShopContent");
        if (shopContent == null)
        {
            // fallback: try Menu_Shop_Panel/ShopContent
            var menuShop = GameObject.Find("Menu_Shop_Panel");
            if (menuShop != null)
            {
                shopContent = menuShop.transform.Find("ShopContent")?.gameObject;
            }
        }

        if (shopContent == null)
        {
            Debug.LogWarning("Could not find ShopContent in scene. Please open the Shop panel and try again.");
            return;
        }

        // Look for ShopContent/Skins container
        Transform dest = shopContent.transform.Find("Skins");
        if (dest == null)
        {
            Debug.LogWarning("ShopContent exists but does not have a 'Skins' child. Please create one or assign manually.");
            return;
        }

        // Move all children from top-level Skins into ShopContent/Skins
        int moved = 0;
        for (int i = topSkins.transform.childCount - 1; i >= 0; i--)
        {
            var child = topSkins.transform.GetChild(i);
            Undo.SetTransformParent(child, dest, "Move skin into ShopContent/Skins");
            moved++;
        }

        Debug.Log($"Moved {moved} children from top-level 'Skins' into '{dest.name}' under ShopContent.");
    }
}
#endif
