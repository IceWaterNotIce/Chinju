using UnityEngine;
using UnityEngine.UIElements;

public class DamagePopupUI : MonoBehaviour
{
    private VisualElement _root;
    private Label _damageText;
    private Vector2 _startPosition;
    private float _disappearTimer;

    public void Init(VisualElement root, int damage, Vector2 screenPos, bool isCritical = false) {
        _root = root;
        _damageText = _root.Q<Label>("damageLabel");
        _damageText.text = damage.ToString();
        _startPosition = screenPos;

        // 設定樣式
        if (isCritical) _root.AddToClassList("critical");

        // 初始位置
        _root.style.left = _startPosition.x;
        _root.style.top = _startPosition.y;
        _disappearTimer = 1f;
    }

    private void Update() {
        if (_root == null) return;

        // 向上飄動
        _startPosition.y -= 50f * Time.deltaTime;
        _root.style.top = _startPosition.y;

        // 漸變消失
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer <= 0) {
            _root.style.opacity = 0;
            if (_root.style.opacity.value <= 0.01f) {
                _root.RemoveFromHierarchy();
                Destroy(this);
            }
        }
    }
}