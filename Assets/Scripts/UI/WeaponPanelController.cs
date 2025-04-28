using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class WeaponPanelController : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name;
        public int attack;
    }

    public List<Weapon> weapons = new List<Weapon>
    {
        new Weapon { name = "長劍", attack = 15 },
        new Weapon { name = "弓箭", attack = 10 },
        new Weapon { name = "斧頭", attack = 20 }
    };

    public VisualTreeAsset weaponPanelUXML;
    public StyleSheet weaponPanelUSS;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var panel = weaponPanelUXML.CloneTree();
        panel.styleSheets.Add(weaponPanelUSS);

        var weaponList = panel.Q<ScrollView>("weapon-list");
        foreach (var weapon in weapons)
        {
            var item = new Label($"{weapon.name} (攻擊力: {weapon.attack})");
            item.AddToClassList("weapon-item");
            weaponList.Add(item);
        }

        root.Add(panel);
    }
}
