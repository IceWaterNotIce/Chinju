using UnityEngine;

public abstract class PopupPanelBase : MonoBehaviour
{
    [Tooltip("Popup 名稱，需與 PopupManager 註冊名稱一致")]
    public string popupName;

    protected virtual void Awake()
    {
        if (!string.IsNullOrEmpty(popupName))
        {
            PopupManager.Instance.RegisterPopup(popupName, gameObject);
        }
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
