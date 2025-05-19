using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private float _disappearTimer;
    private Color _textColor;

    private void Awake() {
        _text = GetComponent<TextMeshProUGUI>();
    }

    public void Setup(float damageAmount) {
        _text.text = damageAmount.ToString();
        _textColor = _text.color;
        _disappearTimer = 1f; // 消失時間（秒）
    }

    private void Update() {
        // 向上飄動效果
        transform.position += new Vector3(0, 1f, 0) * Time.deltaTime;

        // 漸變消失
        _disappearTimer -= Time.deltaTime;
        if (_disappearTimer <= 0) {
            _textColor.a -= 3f * Time.deltaTime;
            _text.color = _textColor;
            if (_textColor.a <= 0) Destroy(gameObject);
        }
    }
}