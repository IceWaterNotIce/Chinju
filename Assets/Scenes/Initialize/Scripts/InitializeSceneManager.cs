using UnityEngine;
using UnityEngine.SceneManagement;
using MessageWindow;
using UnityEngine.UIElements;

namespace InternetEmpire
{
    public class InitializeSceneManager : MonoBehaviour
    {
        public bool isNeedNetwork = false;

        public bool isNeedVersionCheck = false;

        public bool isNeedAssetBundleUpdate = false;

        [HideInInspector]
        public bool isAssetBundleReady = false;

        [HideInInspector]
        public bool isVersionChecked = false;

        void Start()
        {

            CheckNetwork();
            Check();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Check()
        {
            if (isNeedAssetBundleUpdate != isAssetBundleReady)
            {
                return;
            }
            if (isNeedVersionCheck != isVersionChecked)
            {
                return;
            }
            // Load the lobby scene
            SceneManager.LoadScene("Lobby");


        }

        public void QuitGame()
        {
            Application.Quit();
        }

        void CheckNetwork()
        {
            if (isNeedNetwork)
            {
                // Check network
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    // Show network error panel
                    MessageManager.Instance.CreateYesNoMessage("No network connection. Please check your network settings first.", QuitGame, null);
                }
            }
        }

        public void SetVersionChecked(bool isVersionChecked)
        {
            this.isVersionChecked = isVersionChecked;
            if (!isVersionChecked && isNeedVersionCheck)
            {
                UpdateChecker updateChecker = FindFirstObjectByType<UpdateChecker>();
                Debug.Log("有新版本可用: " + updateChecker.versionInfo.latestVersion);
                // 可以提示用戶下載新版本
                MessageManager.Instance.CreateYesNoMessage("There is a new version available. This game needs to run on the latest version. Do you want to update now?", UpdateGame, QuitGame);
            }
            else
            {
                Check();
            }
        }

        public void SetAssetBundleReady(bool isAssetBundleReady)
        {
            this.isAssetBundleReady = isAssetBundleReady;
            if (!isAssetBundleReady && isNeedAssetBundleUpdate)
            {
                Debug.Log("AssetBundle not ready.");
                MessageManager.Instance.CreateCloseMessage("AssetBundle not ready.", QuitGame);
            }
            else
            {
                Check();
            }
        }

        public void UpdateGame()
        {
            UpdateChecker updateChecker = FindFirstObjectByType<UpdateChecker>();
            Application.OpenURL(updateChecker.versionInfo.downloadURL);
            Application.Quit();
        }

    }
}