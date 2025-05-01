using UnityEngine;
using UnityEngine.UIElements;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc != null)
        {
            var chinjuRoot = uiDoc.rootVisualElement;
            //btn1 
            var btn1 = chinjuRoot.Q<Button>("btn1");
            if (btn1 != null)
            {
                btn1.clicked += () => Debug.Log("btn1 clicked!");
                btn1.clicked += () => Debug.Log("btn1 clicked again!");
                btn1.clicked += () => chinjuRoot.style.display = DisplayStyle.None;
                btn1.clicked += () => Debug.Log("btn1 clicked and hidden!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
