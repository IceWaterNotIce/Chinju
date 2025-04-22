using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;

public class CloudSaveManager : Singleton<CloudSaveManager>
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogError("User is not signed in");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public async void SavePlayerFile(string fileName, byte[] fileBytes)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("User is not signed in");
            return;
        }
        try
        {
            await CloudSaveService.Instance.Files.Player.SaveAsync(fileName, fileBytes);
            Debug.Log($"Saved file successfully: {fileName}");
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e);
        }
    }

    public async Task<byte[]> LoadPlayerFile(string fileName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("User is not signed in");
            return null;
        }
        try
        {
            var fileBytes = await CloudSaveService.Instance.Files.Player.LoadBytesAsync(fileName);
            Debug.Log($"Loaded file successfully: {fileName}");
            return fileBytes;
        }
        catch (CloudSaveValidationException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveRateLimitedException e)
        {
            Debug.LogError(e);
        }
        catch (CloudSaveException e)
        {
            Debug.LogError(e.ErrorCode);
        }
        return null;

    }

}