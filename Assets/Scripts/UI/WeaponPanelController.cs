using UnityEngine;
using UnityEngine.UIElements;

public class WeaponPanelController : MonoBehaviour
{
    void OnEnable()
    {
        Debug.Log("[WeaponPanelController] Start 方法執行。");

        var root = GetComponent<UIDocument>().rootVisualElement;

        var weaponList = root.Q<ScrollView>("weapon-list");
        if (weaponList == null)
        {
            Debug.LogError("[WeaponPanelController] 找不到 'weapon-list' 元素。");
            return;
        }

        weaponList.Clear(); // 清空清單

        // 檢查 GameDataController 和 CurrentGameData 是否正確初始化
        if (GameDataController.Instance == null)
        {
            Debug.LogError("[WeaponPanelController] GameDataController.Instance 為空。");
            return;
        }
        else
        {
            Debug.Log("[WeaponPanelController] GameDataController.Instance 已初始化。");
        }

        if (GameDataController.Instance.CurrentGameData == null)
        {
            Debug.LogError("[WeaponPanelController] CurrentGameData 為空。");
            return;
        }
        else
        {
            Debug.Log("[WeaponPanelController] CurrentGameData 已初始化。");
        }

        // 從玩家資料中獲取武器清單
        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        if (playerData != null && playerData.Weapons != null)
        {
            Debug.Log($"[WeaponPanelController] 玩家武器數量: {playerData.Weapons.Count}");
            if (playerData.Weapons.Count > 0)
            {
                foreach (var weapon in playerData.Weapons)
                {
                    Debug.Log($"[WeaponPanelController] 武器: {weapon.Name}, 傷害: {weapon.Damage}");
                    var item = new Label($"{weapon.Name} (傷害: {weapon.Damage}, 範圍: {weapon.Range}, 攻擊速度: {weapon.AttackSpeed}, 冷卻時間: {weapon.CooldownTime})");
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
