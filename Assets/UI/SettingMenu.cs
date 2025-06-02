using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // 新增：使用新輸入系統

public class SettingMenu : MonoBehaviour
{
    private VisualElement root;
    private Slider textSizeSlider; // 新增：Slider 欄位
    private Slider BGMSlider; // 新增
    private Slider SFXSlider; // 新增
    private Button openMenuButtonPanelBtn; // 新增
    private Button openServerListPanelBtn; // 新增

    void Awake()
    {
        PopupManager.Instance.RegisterPopup("SettingPanel", gameObject);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    void OnEnable()
    {
        InitializeUI();
    }

    void OnDestroy()
    {
        // 新增：解除 Slider callback
        if (textSizeSlider != null)
            textSizeSlider.UnregisterValueChangedCallback(OnTextSizeSliderChanged);
        if (BGMSlider != null)
            BGMSlider.UnregisterValueChangedCallback(OnBGMSliderChanged);
        if (SFXSlider != null)
            SFXSlider.UnregisterValueChangedCallback(OnSFXSliderChanged);
    }

    private void InitializeUI()
    {
        try
        {
            var menuDocument = GetComponent<UIDocument>();
            if (menuDocument == null)
            {
                Debug.LogError("[SettingPanel] 無法找到 UIDocument 組件");
                return;
            }

            root = menuDocument.rootVisualElement; // 確保 root 被正確初始化
            if (root == null)
            {
                Debug.LogError("[SettingPanel] 無法初始化 root 元素");
                return;
            }

            // 不再掛載 MenuButtonPanel

            // 新增：取得開啟按鈕並註冊事件
            openMenuButtonPanelBtn = root.Q<Button>("openMenuButtonPanelBtn");
            if (openMenuButtonPanelBtn != null)
            {
                openMenuButtonPanelBtn.clicked += ShowMenuButtonPanel;
            }

            openServerListPanelBtn = root.Q<Button>("openServerListPanelBtn"); // 新增
            if (openServerListPanelBtn != null)
            {
                openServerListPanelBtn.clicked += ShowServerListPanel; // 新增
            }

            textSizeSlider = UIHelper.InitializeElement<Slider>(root, "textSizeSlider"); // 新增：Slider 初始化
            BGMSlider = UIHelper.InitializeElement<Slider>(root, "BGMSlider"); // 新增
            SFXSlider = UIHelper.InitializeElement<Slider>(root, "SFXSlider"); // 新增

            // 不再註冊按鈕 callback

            if (textSizeSlider != null)
            {
                textSizeSlider.RegisterValueChangedCallback(OnTextSizeSliderChanged);
                // 預設初始化一次
                UpdateMenuTextSize((int)textSizeSlider.value);
            }
            if (BGMSlider != null)
            {
                BGMSlider.RegisterValueChangedCallback(OnBGMSliderChanged);
                // 預設初始化一次
                SetBGMVolume(BGMSlider.value);
            }
            if (SFXSlider != null)
            {
                SFXSlider.RegisterValueChangedCallback(OnSFXSliderChanged);
                // 預設初始化一次
                SetSFXVolume(SFXSlider.value);
            }

            PopupManager.Instance.RegisterPopup("SettingMenu", gameObject);
            Debug.Log("[SettingPanel] UI初始化成功");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SettingPanel] UI初始化失敗: {e.Message}\n{e.StackTrace}");
            return; // 新增：初始化失敗直接 return
        }
    }

    private void ToggleMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingPanel] root 未初始化");
            return;
        }

        if (root.style.display == DisplayStyle.Flex)
        {
            HideGameMenu();
        }
        else
        {
            ShowGameMenu();
        }
    }

    public void ShowGameMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingPanel] 無法顯示選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.ShowPopup("SettingMenu");
        Time.timeScale = 0f; // 暫停遊戲
        Debug.Log("[SettingPanel] 顯示遊戲選單並暫停遊戲");
    }

    public void HideGameMenu()
    {
        if (root == null)
        {
            Debug.LogError("[SettingPanel] 無法隱藏選單：選單引用丟失");
            return;
        }

        PopupManager.Instance.HidePopup("SettingMenu");
        Time.timeScale = 1f; // 恢復遊戲
        Debug.Log("[SettingPanel] 隱藏遊戲選單並恢復遊戲");
    }

    // 新增：Slider callback
    private void OnTextSizeSliderChanged(ChangeEvent<float> evt)
    {
        UpdateMenuTextSize((int)evt.newValue);
    }

    // 新增：調整所有 menu 內 Label 與 Button 的字體大小
    private void UpdateMenuTextSize(int fontSize)
    {
        if (root == null) return;
        Debug.Log($"[SettingPanel] Slider value changed: {fontSize}");
        var labels = root.Query<Label>().ToList();
        foreach (var label in labels)
        {
            label.style.fontSize = fontSize;
        }
        var buttons = root.Query<Button>().ToList();
        foreach (var button in buttons)
        {
            button.style.fontSize = fontSize;
        }
    }

    // 新增：BGM 音量 callback
    private void OnBGMSliderChanged(ChangeEvent<float> evt)
    {
        SetBGMVolume(evt.newValue);
    }

    // 新增：SFX 音量 callback
    private void OnSFXSliderChanged(ChangeEvent<float> evt)
    {
        SetSFXVolume(evt.newValue);
    }

    // 新增：實際調整 BGM 音量
    private void SetBGMVolume(float volume)
    {
        Debug.Log($"[SettingPanel] BGM 音量調整: {volume}");
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetBGMVolume(volume);
        // 若無 AudioManager，請自行實作
    }

    // 新增：實際調整 SFX 音量
    private void SetSFXVolume(float volume)
    {
        Debug.Log($"[SettingPanel] SFX 音量調整: {volume}");
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(volume);
        // 若無 AudioManager，請自行實作
    }

    // 新增：顯示 MenuButtonPanel
    private void ShowMenuButtonPanel()
    {
        PopupManager.Instance.ShowPopup("MenuButtonPanel");
        PopupManager.Instance.HidePopup("SettingMenu");
    }

    // 新增：顯示 ServerListPanel
    private void ShowServerListPanel()
    {
        PopupManager.Instance.ShowPopup("ServerListPanel");
        PopupManager.Instance.HidePopup("SettingMenu");
    }
}
