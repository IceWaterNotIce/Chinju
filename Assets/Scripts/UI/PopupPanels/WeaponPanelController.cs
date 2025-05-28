using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponPanelController : MonoBehaviour
{
    public VisualElement root;
    public ScrollView weaponList;
    [SerializeField] private VisualTreeAsset weaponTemplate;

    private void Awake()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;
        weaponList = root.Q<ScrollView>("weapon-list");

        PopupManager.Instance.RegisterPopup("WeaponPanel", gameObject);
    }

    void OnEnable()
    {
        StartCoroutine(WaitForGameDataControllerInitialization());
    }

    private IEnumerator WaitForGameDataControllerInitialization()
    {
        while (GameDataController.Instance == null || GameDataController.Instance.CurrentGameData == null)
        {
            yield return null;
        }

        var playerData = GameDataController.Instance.CurrentGameData.playerData;
        InitializeWeaponList(playerData);
    }

    private void InitializeWeaponList(GameData.PlayerData playerData)
    {
        weaponList.Clear();

        if (playerData == null || playerData.Weapons == null || playerData.Weapons.Count == 0)
        {
            var noWeaponLabel = new Label("目前沒有武器可顯示。");
            noWeaponLabel.AddToClassList("no-weapon-item");
            weaponList.Add(noWeaponLabel);
            return;
        }

        foreach (var weapon in playerData.Weapons)
        {
            if (weaponTemplate == null)
            {
                Debug.LogError("[WeaponPanelController] 找不到 WeaponTemplate.uxml");
                continue;
            }
            VisualElement weaponElement = weaponTemplate.Instantiate();

            var nameLabel = weaponElement.Q<Label>("lblWeaponName");
            var damageLabel = weaponElement.Q<Label>("lblDamage");
            var rangeLabel = weaponElement.Q<Label>("lblRange");
            var speedLabel = weaponElement.Q<Label>("lblSpeed");
            var cooldownLabel = weaponElement.Q<Label>("lblCooldown");

            if (nameLabel != null) nameLabel.text = weapon.Name;
            if (damageLabel != null) damageLabel.text = $"傷害: {weapon.Damage}";
            if (rangeLabel != null) rangeLabel.text = $"範圍: {weapon.MinAttackDistance}-{weapon.MaxAttackDistance}";
            if (speedLabel != null) speedLabel.text = $"攻速: {weapon.AttackSpeed}";
            if (cooldownLabel != null) cooldownLabel.text = $"冷卻: {weapon.CooldownTime}";

            weaponElement.AddToClassList("weapon-item");
            weaponList.Add(weaponElement);
        }
    }
}
