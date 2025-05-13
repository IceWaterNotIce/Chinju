using UnityEngine;

using UnityEngine.UIElements;
using System.Collections.Generic;

using System.Collections;
using System.Linq;

using System;
using System.IO;

public class PopupManager : Singleton<PopupManager>
{
    private Dictionary<string, VisualElement> PopupPanels = new Dictionary<string, VisualElement>();

    public void RegisterPopup(string popupName, VisualElement popupPanelRoot)
    {
        if (!PopupPanels.ContainsKey(popupName))
        {
            PopupPanels.Add(popupName, popupPanelRoot);
            popupPanelRoot.style.display = DisplayStyle.None; // 隱藏面板
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
            popupPanelRoot.style.display = DisplayStyle.Flex; // 顯示面板
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
            popupPanelRoot.style.display = DisplayStyle.None; // 隱藏面板
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
        return PopupPanels.Values.All(p => p.style.display == DisplayStyle.None);
    }

    public bool IsPopupVisible(string popupName)
    {
        if (PopupPanels.TryGetValue(popupName, out var popupPanelRoot))
        {
            return popupPanelRoot.style.display == DisplayStyle.Flex;
        }
        return false;
    }
}