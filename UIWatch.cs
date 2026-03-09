// UIWatch.cs — attach to the TryAgain button GameObject
using UnityEngine;
using UnityEngine.UI;

public class UIWatch : MonoBehaviour
{
    private Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        Debug.Log($"[UIWatch] ({name}) Awake. hasButton: {btn != null}");
    }

    void OnEnable()
    {
        Debug.Log($"[UIWatch] ({name}) OnEnable - activeInHierarchy: {gameObject.activeInHierarchy}");
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnBtnClicked); // avoid double-register
            btn.onClick.AddListener(OnBtnClicked);
        }
    }

    void OnDisable()
    {
        Debug.Log($"[UIWatch] ({name}) OnDisable");
        if (btn != null) btn.onClick.RemoveListener(OnBtnClicked);
    }

    void OnDestroy()
    {
        Debug.Log($"[UIWatch] ({name}) OnDestroy");
    }

    void OnBtnClicked()
    {
        Debug.Log($"[UIWatch] ({name}) Clicked");
    }
}
