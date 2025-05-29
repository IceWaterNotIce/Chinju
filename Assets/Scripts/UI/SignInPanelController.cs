using UnityEngine;
using UnityEngine.UIElements;

public class SignInPanelController : PopupPanelBase
{
    private VisualElement root;
    private TextField usernameField;
    private TextField passwordField;
    private Button signinButton;

    protected override void Awake()
    {
        base.Awake();
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        usernameField = root.Q<TextField>("usernameField");
        passwordField = root.Q<TextField>("passwordField");
        signinButton = root.Q<Button>("signinButton");

        signinButton.clicked += OnSignInClicked;
    }

    private void OnSignInClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        Debug.Log($"Sign In clicked with Username: {username}, Password: {password}");
        // Call AuthManager.SignInWithUsernamePasswordAsync
    }
}
