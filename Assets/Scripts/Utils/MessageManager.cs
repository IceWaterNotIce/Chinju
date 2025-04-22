using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using MessageWindow;
using UnityEngine.SceneManagement;

namespace InternetEmpire
{
    public class MessageManager : Singleton<MessageManager>
    {
        [SerializeField] private GameObject Canvas;
        [SerializeField] private GameObject panelprefab;
        [SerializeField] private GameObject tmpTextPrefab;
        [SerializeField] private GameObject buttonPrefab;
        [SerializeField] private GameObject buttonClosePrefab;

        UIDocument uiDoc;
        VisualElement root;

        override protected void Awake()
        {
            base.Awake();
            uiDoc = Instance.AddComponent<UIDocument>();
            // set uidoc panel settings , load from resources
            uiDoc.panelSettings = Resources.Load<PanelSettings>("UI/PanelSettings");
            // add scource asset
            uiDoc.visualTreeAsset = Resources.Load<VisualTreeAsset>("UI/MessageWindow");
            // set root to uidoc
            root = uiDoc.rootVisualElement;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        void Start()
        {


        }
 private void OnDestroy()
        {
            // Unsubscribe from the event to prevent memory leaks
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
                private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearUI(); // Clear UI when a new scene is loaded
        }

        private void ClearUI()
        {
            root.Clear(); // Clear all child elements from the root VisualElement
        }
        public void CreateYesNoMessage(string message, System.Action onYes, System.Action onNo)
        {
            MsgConfirmationDialog messageWindow = new MsgConfirmationDialog(message, onYes, onNo);
            root.Add(messageWindow);
        }

        public void ToastMessage(string message, float duration = 2f)
        {
            GameObject panel = Instantiate(panelprefab, Canvas.transform);
            GameObject tmpText = Instantiate(tmpTextPrefab, panel.transform);
            tmpText.GetComponent<TMP_Text>().text = message;
            StartCoroutine(DestroyAfter(panel, duration));
        }

        private IEnumerator DestroyAfter(GameObject panel, float duration)
        {
            yield return new WaitForSeconds(duration);
            Destroy(panel);
        }

        public void CreateCloseMessage(string message, System.Action onClose)
        {
            GameObject panel = Instantiate(panelprefab, Canvas.transform);
            GameObject tmpText = Instantiate(tmpTextPrefab, panel.transform);
            tmpText.GetComponent<TMP_Text>().text = message;
            GameObject buttonClose = Instantiate(buttonClosePrefab, panel.transform);
            buttonClose.GetComponentInChildren<TMP_Text>().text = "Close";
            buttonClose.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                onClose?.Invoke();
                Destroy(panel);
            });
        }
    }
}
