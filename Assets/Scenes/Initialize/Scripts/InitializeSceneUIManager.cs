using UnityEngine;
using UnityEngine.UIElements;

public class InitializeSceneUIManager : MonoBehaviour
{
    VisualElement root;

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        Label versionLabel = root.Q<Label>("lblVersion");
        versionLabel.text = "Version: " + Application.version.ToString();
        // Debug.Log("Version: " + Application.version);
    }

    void Update()
    {

    }
}
