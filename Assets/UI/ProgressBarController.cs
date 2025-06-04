using UnityEngine;
using UnityEngine.UIElements;

public class ProgressBarController : MonoBehaviour
{
    [Header("Progress Bar Settings")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private string progressBarName = "progress-bar";
    [SerializeField] private float progressBarDuration = 10f; // 進度條持續時間

    private VisualElement progressBar;
    private float progressBarTimer = 0f;

    public System.Action OnProgressComplete; // 當進度條完成時觸發的事件

    private void Start()
    {
        InitializeProgressBar();
    }

    private void Update()
    {
        UpdateProgressBar();
    }

    private void InitializeProgressBar()
    {
        if (uiDocument == null)
        {
            Debug.LogError("[ProgressBarController] UIDocument is not assigned!");
            return;
        }

        var root = uiDocument.rootVisualElement;
        progressBar = root.Q<VisualElement>(progressBarName);

        if (progressBar == null)
        {
            Debug.LogError($"[ProgressBarController] Progress bar '{progressBarName}' not found in UI!");
        }
    }

    private void UpdateProgressBar()
    {
        if (progressBar == null) return;

        progressBarTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(progressBarTimer / progressBarDuration);
        progressBar.style.width = new Length(progress * 100, LengthUnit.Percent);

        if (progressBarTimer >= progressBarDuration)
        {
            progressBarTimer = 0f; // 重置進度條計時器
            OnProgressComplete?.Invoke(); // 觸發進度條完成事件
        }
    }

    public void ResetProgressBar()
    {
        progressBarTimer = 0f;
        if (progressBar != null)
        {
            progressBar.style.width = new Length(0, LengthUnit.Percent);
        }
    }
}
