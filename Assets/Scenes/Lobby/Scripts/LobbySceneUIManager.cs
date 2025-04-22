using UnityEngine;
using Unity.Services.Authentication;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UIElements;
public class LobbySceneUIManager : MonoBehaviour
{
    private RuntimePlatform runtimePlatform;


    VisualElement root;

    Button BtnGoToGraphView;
    Button BtnGoToGameRoom;
    Button BtnGoToWordView;

    VisualElement AuthPanel;
    Button SignInWithAnonymouslyBtn;
    VisualElement SignInPanel;
    TextField SignInUsernameInput;
    TextField SignInPasswordInput;
    Button SignInBtn;

    VisualElement SignUpPanel;
    TextField SignUpUsernameInput;
    TextField SignUpPasswordInput;
    Button SignUpBtn;
    VisualElement UserProfilePanel;

    Label UIDLabel;
    Label UsernameLabel;
    Label LastPasswordChangeLabel;
    Button SignOutBtn;
    Button AuthBtn;

    Button BtnGoToSignIn;
    Button BtnGoToSignUp;


    void Start()
    {
        // show the canvas only on the right platform or in the editor
        runtimePlatform = Application.platform;

    }

    void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        root = uiDoc.rootVisualElement;

        BtnGoToGraphView = root.Q<Button>("BtnGoToGraphView");
        BtnGoToGraphView.clicked += GoToGraphView;
        BtnGoToGameRoom = root.Q<Button>("BtnGoToGameRoom");
        BtnGoToGameRoom.clicked += GoToGameRoom;
        BtnGoToWordView = root.Q<Button>("BtnGoToWordView");
        BtnGoToWordView.clicked += GoToWordView;

        AuthBtn = root.Q<Button>("BtnAuth");
        AuthBtn.clicked += BtnAuthOnClick;

        AuthPanel = root.Q<VisualElement>("AuthPanel");
        Button BtnCloseAuthPanel = AuthPanel.Query<Button>(className: "button_close");
        BtnCloseAuthPanel.clicked += () => SwitchActiveState(AuthPanel);
        SignInPanel = root.Q<VisualElement>("SignInPanel");
        Button BtnCloseSignInPanel = SignInPanel.Query<Button>(className: "button_close");
        BtnCloseSignInPanel.clicked += () => SwitchActiveState(SignInPanel);
        SignUpPanel = root.Q<VisualElement>("SignUpPanel");
        Button BtnCloseSignUpPanel = SignUpPanel.Query<Button>(className: "button_close");
        BtnCloseSignUpPanel.clicked += () => SwitchActiveState(SignUpPanel);
        UserProfilePanel = root.Q<VisualElement>("UserProfilePanel");
        Button BtnCloseUserProfilePanel = UserProfilePanel.Query<Button>(className: "button_close");
        BtnCloseUserProfilePanel.clicked += () => SwitchActiveState(UserProfilePanel);

        
        SignInWithAnonymouslyBtn = root.Q<Button>("BtnSignInWithAnonymously");
        SignInWithAnonymouslyBtn.clicked += OnSignInAnonymouslyBtnClick;
        BtnGoToSignIn = root.Q<Button>("BtnGoToSignIn");
        BtnGoToSignIn.clicked += () => {
            SwitchActiveState(SignInPanel);
            SwitchActiveState(AuthPanel);
        };
        BtnGoToSignUp = root.Q<Button>("BtnGoToSignUp");
        BtnGoToSignUp.clicked += () => {
            SwitchActiveState(SignUpPanel);
            SwitchActiveState(AuthPanel);
        };


        SignInUsernameInput = root.Q<TextField>("SignInUsernameInput");
        SignInPasswordInput = root.Q<TextField>("SignInPasswordInput");
        SignInBtn = root.Q<Button>("BtnSignIn");
        SignInBtn.clicked += OnSignInBtnClick;

        SignUpUsernameInput = root.Q<TextField>("SignUpUsernameInput");
        SignUpPasswordInput = root.Q<TextField>("SignUpPasswordInput");
        SignUpBtn = root.Q<Button>("BtnSignUp");
        SignUpBtn.clicked += OnSignUpBtnClick;

        SignOutBtn = root.Q<Button>("BtnSignOut");
        SignOutBtn.clicked += BtnSignOutOnClick;

        UIDLabel = root.Q<Label>("UIDLabel");
        UsernameLabel = root.Q<Label>("UsernameLabel");
        LastPasswordChangeLabel = root.Q<Label>("LastPasswordChangeLabel");



    }
    

    

    public void SwitchActiveState(VisualElement obj)
    {
        // Close the panel
        obj.style.display = obj.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
    }


    public void BtnAuthOnClick()
    {
        UserProfilePanel.style.display = AuthenticationService.Instance.IsSignedIn == true ? DisplayStyle.Flex : DisplayStyle.None;
        AuthPanel.style.display = AuthenticationService.Instance.IsSignedIn == false ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void BtnSignOutOnClick()
    {
        // sign out the user
        AuthenticationService.Instance.SignOut();
        //set attributes disabled
        UserProfilePanel.style.display = DisplayStyle.None;
        AuthPanel.style.display = DisplayStyle.Flex;
    }

    public void GoToGraphView()
    {
        // Load the GraphView scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GraphView");
    }
    public void GoToWordView()
    {
        // Load the WordView scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("WordView");
    }

    public void GoToGameRoom()
    {
        // Load the GameRoom scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameRoom");
    }
    public async void OnSignInBtnClick()
    {
        AuthManager authManager = FindFirstObjectByType<AuthManager>();
        await authManager.SignInWithUsernamePasswordAsync(SignInUsernameInput.text, SignInPasswordInput.text);
        UpdateUI();
        // if sign in is successful
        if (AuthenticationService.Instance.IsSignedIn)
        {
            //set attributes enabled
            SignInPanel.style.display = DisplayStyle.None;
            UserProfilePanel.style.display = DisplayStyle.Flex;
        }
    }

    public async void OnSignUpBtnClick()
    {
        AuthManager authManager = FindFirstObjectByType<AuthManager>();
        await authManager.SignUpWithUsernamePasswordAsync(SignUpUsernameInput.text, SignUpPasswordInput.text);
        UpdateUI();
        // if sign up is successful
        if (AuthenticationService.Instance.IsSignedIn)
        {
            //set attributes enabled
            SignUpPanel.style.display = DisplayStyle.None;
            UserProfilePanel.style.display = DisplayStyle.Flex;
        }
    }

    public async void OnSignInAnonymouslyBtnClick()
    {
        AuthManager authManager = FindFirstObjectByType<AuthManager>();
        await authManager.SignInAnonymously();
        UpdateUI();
        // if sign in anonymously is successful
        if (AuthenticationService.Instance.IsSignedIn)
        {
            //set attributes enabled
            AuthPanel.style.display = DisplayStyle.None;
            UserProfilePanel.style.display = DisplayStyle.Flex;
        }

    }
    async void UpdateUI()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            var playerInfo = await AuthenticationService.Instance.GetPlayerInfoAsync();
            UIDLabel.text = playerInfo.Id;
            UsernameLabel.text = playerInfo.Username;
            LastPasswordChangeLabel.text = playerInfo.LastPasswordUpdate.ToString();
        }
        else
        {
            UIDLabel.text = "N/A";
            UsernameLabel.text = "N/A";
            LastPasswordChangeLabel.text = "N/A";
        }
    }
}