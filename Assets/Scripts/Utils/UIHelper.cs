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
}
