using UnityEngine;

using UnityEngine.UIElements;
using System.Collections.Generic;

using System.Collections;
using System.Linq;



public class PopupManager : Singleton<PopupManager>
{
    private Dictionary<string, GameObject> PopupPanels = new Dictionary<string, GameObject>();

    public void RegisterPopup(string popupName, GameObject popupPanelRoot)
    {
        if (!PopupPanels.ContainsKey(popupName))
        {
            PopupPanels.Add(popupName, popupPanelRoot);
            popupPanelRoot.SetActive(false); // 隱藏面板
        }
        else
        {
            Debug.LogWarning($"Popup {popupName} 已經存在，無法重複添加。");
        }
    }

    public void ShowPopup(string popupName)
    {
        if (PopupPanels.TryGetValue(popupName, out var popupPanelRoot))
        {
            popupPanelRoot.SetActive(true); // 顯示面板
        }
        else
        {
            Debug.LogWarning($"Popup {popupName} 不存在，無法顯示。");
        }
    }

    public void HidePopup(string popupName)
    {
        if (PopupPanels.TryGetValue(popupName, out var popupPanelRoot))
        {
            popupPanelRoot.SetActive(false); // 隱藏面板
        }
        else
        {
            Debug.LogWarning($"Popup {popupName} 不存在，無法隱藏。");
        }
    }

    public void HideAllPopups()
    {
        foreach (var popupName in PopupPanels.Keys.ToList())
        {
            HidePopup(popupName);
        }
    }

    public void RemovePopup(string popupName)
    {
        if (PopupPanels.ContainsKey(popupName))
        {
            PopupPanels.Remove(popupName);
        }
        else
        {
            Debug.LogWarning($"Popup {popupName} 不存在，無法移除。");
        }
    }

    public bool IsAllPopupsHidden()
    {
        return PopupPanels.Values.All(p => !p.activeSelf);
    }

    public bool IsPopupVisible(string popupName)
    {
        if (PopupPanels.TryGetValue(popupName, out var popupPanelRoot))
        {
            return popupPanelRoot.activeSelf;
        }
        return false;
    }
}