using UnityEngine;
using UnityEngine.UI;

public class mono : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // resources load platform.txt
        string platform = Resources.Load<TextAsset>("platform").text;
        Debug.Log(platform);
        Text text = GetComponent<Text>();
        text.text = platform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
