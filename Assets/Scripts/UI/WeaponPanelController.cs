using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class WeaponPanelController : MonoBehaviour
{
    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        var weaponList = root.Q<ScrollView>("weapon-list");
        if (weaponList == null)
        {
            Debug.LogError("[WeaponPanelController] 找不到 'weapon-list' 元素。");
            return;
        }

        weaponList.Clear(); // 清空清單

        // 從玩家資料中獲取武器清單
        var playerData = GameDataController.Instance?.CurrentGameData?.playerData;
        if (playerData != null)
        {
            foreach (var weapon in playerData.Weapons)
            {
                var item = new Label($"{weapon.Name} (傷害: {weapon.Damage}, 範圍: {weapon.Range}, 攻擊速度: {weapon.AttackSpeed}, 冷卻時間: {weapon.CooldownTime})");
                item.AddToClassList("weapon-item");
                weaponList.Add(item);
            }
        }
        else
        {
            Debug.LogError("[WeaponPanelController] 無法取得玩家資料，無法顯示武器清單。");
        }
    }
}
