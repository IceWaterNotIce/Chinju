using UnityEngine;
using UnityEngine.UIElements;

public class LoginPanelController : PopupPanelBase
{
    private VisualElement root;
    private TextField usernameField;
    private TextField passwordField;
    private Button loginButton;
    private Button signupButton;

    protected override void Awake()
    {
        base.Awake();
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        usernameField = root.Q<TextField>("usernameField");
        passwordField = root.Q<TextField>("passwordField");
        loginButton = root.Q<Button>("loginButton");
        signupButton = root.Q<Button>("signupButton");

        loginButton.clicked += OnLoginClicked;
        signupButton.clicked += OnSignupClicked;
    }

    private void OnLoginClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        Debug.Log($"Login clicked with Username: {username}, Password: {password}");
        // Call AuthManager.SignInWithUsernamePasswordAsync
    }

    private void OnSignupClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        Debug.Log($"Sign Up clicked with Username: {username}, Password: {password}");
        // Call AuthManager.SignUpWithUsernamePasswordAsync
    }
}
