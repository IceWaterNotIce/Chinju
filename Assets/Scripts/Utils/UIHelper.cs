using UnityEngine;
using UnityEngine.UIElements;

public static class UIHelper
{
    /// <summary>
    /// 初始化 UI 元素
    /// </summary>
    /// <typeparam name="T">UI 元素類型</typeparam>
    /// <param name="root">根元素</param>
    /// <param name="name">元素名稱</param>
    /// <returns>找到的 UI 元素</returns>
    public static T InitializeElement<T>(VisualElement root, string name) where T : VisualElement
    {
        var element = root.Q<T>(name);
        if (element == null)
        {
            var className = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name;
            Debug.LogError($"[{className}] 無法找到名為 '{name}' 的 UI 元素！");
        }
        return element;
    }


    // 核心方法：將 VisualElement 綁定到世界座標
    /// <summary>
    /// 將 UI 元素綁定到世界座標
    /// </summary>
    /// <param name="uiElement">要綁定的 UI 元素</param>
    /// <param name="worldPosition">世界座標</param>
    /// <param name="camera">相機（預設為主相機）</param>
    /// <param name="clampToScreen">是否限制在螢幕範圍內</param>
    /// <param name="yOffset">Y 軸偏移量</param>
    /// <remarks>
    /// 這個方法將 UI 元素綁定到指定的世界座標，並選擇性地限制在螢幕範圍內。
    /// 如果 UI 元素在相機後方，則隱藏它。
    /// </remarks>
    /// <returns>是否成功綁定</returns>
    /// <exception cref="System.ArgumentNullException">當 uiElement 或 camera 為 null 時拋出</exception>
    public static void BindToWorldPosition(
        VisualElement uiElement,
        Vector3 worldPosition,
        Camera camera = null,


        bool clampToScreen = true,
        float yOffset = 0f
    )
    {
        if (camera == null) camera = Camera.main;
        if (camera == null || uiElement == null) return;

        // 世界座標 → 螢幕座標
        Vector3 screenPosition = camera.WorldToScreenPoint(worldPosition + Vector3.up * yOffset);

        // 如果目標在相機後方，隱藏 UI
        if (screenPosition.z < 0)
        {
            uiElement.style.visibility = Visibility.Hidden;
            return;
        }

        // 修正 UI Toolkit 的 Y 軸（螢幕座標 Y 軸反向）
        screenPosition.y = Screen.height - screenPosition.y;

        // 可選：將 UI 限制在螢幕範圍內
        if (clampToScreen)
        {
            screenPosition.x = Mathf.Clamp(screenPosition.x, 0, Screen.width);
            screenPosition.y = Mathf.Clamp(screenPosition.y, 0, Screen.height);
        }

        // 更新 UI 位置
        uiElement.style.position = Position.Absolute;
        uiElement.transform.position = new Vector2(screenPosition.x, screenPosition.y);
        uiElement.style.visibility = Visibility.Visible;
    }

    // 進階：根據距離動態縮放 UI 大小
    public static void BindWithDynamicScaling(
        VisualElement uiElement,
        Vector3 worldPosition,
        Camera camera = null,
        float minScale = 0.5f,
        float maxScale = 1.5f,
        float maxDistance = 20f
    ) {
        if (camera == null) camera = Camera.main;
        if (camera == null || uiElement == null) return;

        // 計算距離比例縮放
        float distance = Vector3.Distance(camera.transform.position, worldPosition);
        float scaleRatio = Mathf.Clamp(1 - (distance / maxDistance), minScale, maxScale);

        // 綁定位置
        BindToWorldPosition(uiElement, worldPosition, camera);

        // 動態調整縮放
        uiElement.style.scale = new Scale(new Vector2(scaleRatio, scaleRatio));
    }
}
