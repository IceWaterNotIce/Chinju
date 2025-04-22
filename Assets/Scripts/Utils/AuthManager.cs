using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using TMPro;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Awake()
    {
        //UnityServices.Initialize() will initialize all services that are subscribed to Core
        await UnityServices.InitializeAsync();
        Debug.Log($"Unity services initialization: {UnityServices.State}");

        //Shows if a cached session token exist
        Debug.Log($"Cached Session Token Exist: {AuthenticationService.Instance.SessionTokenExists}");

        // Shows Current profile
        Debug.Log($"Current Profile: {AuthenticationService.Instance.Profile}");
    }

    void Start()
    {

        
    }

    void OnEnable()
    {
    }
  
    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        Debug.Log($"Username: {username}");
        Debug.Log($"Password: {password}");
        if (GameObject.Find("AuthenticatedGameObject") != null)
        {
            Debug.Log("Already signed in.");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp With Username and PasswordAsync is successful.");

            GameObject AuthenticatedGameObject = new GameObject("AuthenticatedGameObject");
            DontDestroyOnLoad(AuthenticatedGameObject);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        if (GameObject.Find("AuthenticatedGameObject") != null)
        {
            Debug.Log("Already signed in.");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("ignIn With Username and Password is successful.");

            GameObject AuthenticatedGameObject = new GameObject("AuthenticatedGameObject");
            DontDestroyOnLoad(AuthenticatedGameObject);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async Task AddUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
            Debug.Log("Username and password added.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
    public async Task SignInAnonymously()
    {
        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("User is already signed in");
            return;
        }
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Sign in anonymously failed with error code: {ex.ErrorCode}");
        }
    }

    public void SignOut()
    {
        AuthenticationService.Instance.SignOut();
    }

    public void ClearSessionToken()
    {
        AuthenticationService.Instance.ClearSessionToken();
    }

    public async Task UpdatePassword(string currentPassword, string newPassword)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }
}
