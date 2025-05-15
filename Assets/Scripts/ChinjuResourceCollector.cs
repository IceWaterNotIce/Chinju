using UnityEngine;

public class ChinjuResourceCollector : MonoBehaviour
{
    public int GoldPerMinute = 3;
    public int OilPerMinute = 10;
    public int CubePerMinute = 0; // 可根據需求調整

    private float timer = 0f;

    void Start()
    {
        Debug.Log("[ChinjuResourceCollector] 開始初始化...");
        StartCoroutine(InitializeGameDataController());
    }

    private System.Collections.IEnumerator InitializeGameDataController()
    {
        int retries = 5;
        while (GameDataController.Instance == null && retries > 0)
        {
            Debug.LogWarning("[ChinjuResourceCollector] 等待 GameDataController 初始化...");
            yield return new WaitForSeconds(1f);
            retries--;
        }

        if (GameDataController.Instance == null)
        {
            Debug.LogError("[ChinjuResourceCollector] GameDataController 未初始化，請確保場景中有 GameDataController！");
        }
        else if (GameDataController.Instance.CurrentGameData == null)
        {
            Debug.LogError("[ChinjuResourceCollector] CurrentGameData 未初始化，請確保 GameDataController 正常運行！");
        }
        else
        {
            Debug.Log("[ChinjuResourceCollector] 初始化成功！");
        }
    }

    void Update()
    {
        if (GameDataController.Instance == null || GameDataController.Instance.CurrentGameData == null)
        {
            Debug.LogWarning("[ChinjuResourceCollector] 無法執行資源收集，GameDataController 或 CurrentGameData 為 null！");
            return;
        }

        timer += Time.deltaTime;

        if (timer >= 60f) // 每分鐘執行一次
        {
            CollectResources();
            timer = 0f;
        }
    }

    private void CollectResources()
    {
        var gameData = GameDataController.Instance.CurrentGameData;
        if (gameData.playerData != null)
        {
            // 合併資源更新邏輯
            gameData.playerData.Gold += GoldPerMinute;
            gameData.playerData.Oils += OilPerMinute;
            gameData.playerData.Cube += CubePerMinute;

            // 觸發資源變更事件
            gameData.playerData.OnResourceChanged?.Invoke();

            Debug.Log($"[ChinjuResourceCollector] 收集資源成功：金幣 +{GoldPerMinute}，石油 +{OilPerMinute}，方塊 +{CubePerMinute}");
        }
        else
        {
            Debug.LogWarning("[ChinjuResourceCollector] 無法收集資源，PlayerData 為 null！");
        }
    }
}
