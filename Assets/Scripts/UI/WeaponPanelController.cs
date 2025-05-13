using System.Collections;
using System.ComponentModel.Design.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponPanelController : MonoBehaviour
{
    public VisualElement root;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        PopupManager.Instance.RegisterPopup("WeaponPanel", gameObject);
    }
    void OnEnable()
    {
        Debug.Log("[WeaponPanelController] Start 方法執行。");

        StartCoroutine(WaitForGameDataControllerInitialization());
    }

    private IEnumerator WaitForGameDataControllerInitialization()
    {
        while (GameDataController.Instance == null || GameDataController.Instance.CurrentGameData == null)
        {
            Debug.LogWarning("[WeaponPanelController] 等待 GameDataController 初始化...");
            yield return null; // 等待下一幀
        }

        var root = GetComponent<UIDocument>().rootVisualElement;

        var weaponList = UIHelper.InitializeElement<ScrollView>(root, "weapon-list");
        if (weaponList == null) yield break;

        weaponList.Clear(); // 清空清單

        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        if (playerData != null && playerData.Weapons != null)
        {
            Debug.Log($"[WeaponPanelController] 玩家武器數量: {playerData.Weapons.Count}");
            if (playerData.Weapons.Count > 0)
            {
                foreach (var weapon in playerData.Weapons)
                {
                    Debug.Log($"[WeaponPanelController] 武器: {weapon.Name}, 傷害: {weapon.Damage}");
                    var item = new Label($"{weapon.Name} (傷害: {weapon.Damage}, 最大範圍: {weapon.MaxAttackDistance}, 最小範圍: {weapon.MinAttackDistance}, 攻擊速度: {weapon.AttackSpeed}, 冷卻時間: {weapon.CooldownTime})");
                    item.AddToClassList("weapon-item");
                    weaponList.Add(item);
                }
            }
            else
            {
                Debug.LogWarning("[WeaponPanelController] 玩家沒有武器。");
                var noWeaponLabel = new Label("目前沒有武器可顯示。");
                noWeaponLabel.AddToClassList("no-weapon-item");
                weaponList.Add(noWeaponLabel);
            }
        }
        else
        {
            Debug.LogError("[WeaponPanelController] 玩家資料或武器清單為空。");
        }
    }
}
