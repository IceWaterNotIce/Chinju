using UnityEngine;
using TMPro;

public class Test : MonoBehaviour
{
    public TextMeshProUGUI text;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text.text = Application.platform.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
