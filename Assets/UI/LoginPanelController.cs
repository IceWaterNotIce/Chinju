using UnityEngine;
using UnityEngine.UIElements;
using Unity.Services.Authentication;

public class LoginPanelController : PopupPanelBase
{
    private VisualElement root;
    private TextField usernameField;
    private TextField passwordField;
    private Button loginButton;
    private Button signupButton;

    protected override void Awake() {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null || uiDocument.rootVisualElement == null) {
            Debug.LogError("[LoginPanelController] rootVisualElement 為 null，無法初始化 LoginPanel！");
            return;
        }

        root = uiDocument.rootVisualElement;

        usernameField = root.Q<TextField>("usernameField");
        passwordField = root.Q<TextField>("passwordField");
        loginButton = root.Q<Button>("loginButton");
        signupButton = root.Q<Button>("signupButton");

        if (loginButton == null) {
            Debug.LogError("[LoginPanelController] LoginButton 未找到！");
            return;
        }

        loginButton.clicked += OnLoginClicked;
        signupButton.clicked += OnSignupClicked;
    }

    private void OnLoginClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        Debug.Log($"Login clicked with Username: {username}, Password: {password}");
        AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("Login successful.");
                    // Handle successful login (e.g., close panel, load next scene)
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"Login failed: {task.Exception}");
                    // Handle login failure (e.g., show error message)
                }
            });
    }

    private void OnSignupClicked()
    {
        string username = usernameField.value;
        string password = passwordField.value;

        Debug.Log($"Sign Up clicked with Username: {username}, Password: {password}");
        AuthenticationService.Instance.AddUsernamePasswordAsync(username, password)
            .ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    Debug.Log("Sign Up successful.");
                    // Handle successful sign-up (e.g., close panel, load next scene)
                }
                else if (task.IsFaulted)
                {
                    Debug.LogError($"Sign Up failed: {task.Exception}");
                    // Handle sign-up failure (e.g., show error message)
                }
            });
    }
}
