using System.Collections.Generic;
using UnityEngine;

public class PopupMenuManager : MonoBehaviour
{
    private Dictionary<string, GameObject> popupMenus = new Dictionary<string, GameObject>();

    // Register a popup menu with a unique key
    public void RegisterPopup(string key, GameObject popup)
    {
        if (!popupMenus.ContainsKey(key))
        {
            popupMenus[key] = popup;
            popup.SetActive(false); // Ensure it's initially hidden
        }
    }

    // Open a specific popup menu
    public void OpenPopup(string key)
    {
        if (popupMenus.ContainsKey(key))
        {
            CloseAllPopups(); // Close other popups
            popupMenus[key].SetActive(true);
        }
    }

    // Close a specific popup menu
    public void ClosePopup(string key)
    {
        if (popupMenus.ContainsKey(key))
        {
            popupMenus[key].SetActive(false);
        }
    }

    // Toggle a specific popup menu
    public void TogglePopup(string key)
    {
        if (popupMenus.ContainsKey(key))
        {
            bool isActive = popupMenus[key].activeSelf;
            CloseAllPopups();
            popupMenus[key].SetActive(!isActive);
        }
    }

    // Close all popup menus
    public void CloseAllPopups()
    {
        foreach (var popup in popupMenus.Values)
        {
            popup.SetActive(false);
        }
    }
}
