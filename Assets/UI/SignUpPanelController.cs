using UnityEngine;
using UnityEngine.UIElements;

public class SignUpPanelController : PopupPanelBase
{
    private VisualElement root;
    private TextField usernameField;
    private TextField passwordField;
    private TextField emailField;
    private Button signupButton;

    protected override void Awake()
    {
        base.Awake();
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        usernameField = root.Q<TextField>("usernameField");
        passwordField = root.Q<TextField>("passwordField");
        emailField = root.Q<TextField>("emailField");
        signupButton = root.Q<Button>("signupButton");

        signupButton.clicked += OnSignUpClicked;
    }

    private void OnSignUpClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;
        string email = emailField.value;

        Debug.Log($"Sign Up clicked with Username: {username}, Password: {password}, Email: {email}");
        // Call AuthManager.SignUpWithUsernamePasswordEmailAsync
    }
}
