using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowPopup(string popupName)
    {
        // 實作顯示彈出視窗的邏輯
    }

    public void HidePopup(string popupName)
    {
        // 實作隱藏彈出視窗的邏輯
    }

}