using UnityEngine;
using System.Collections.Generic;

public class Vector3ListProvider : MonoBehaviour {
    public Vector3[] positions; // 在 Inspector 設定
    private MaterialPropertyBlock _propBlock;

    void Update() {
        _propBlock = new MaterialPropertyBlock();
        GetComponent<Renderer>().GetPropertyBlock(_propBlock);
        
        // 將 Vector3[] 轉換為 List<Vector4>
        List<Vector4> vector4List = new List<Vector4>();
        foreach (var position in positions) {
            vector4List.Add(new Vector4(position.x, position.y, position.z, 0)); // 添加第四個分量
        }

        // 傳遞 Vector4 數組
        _propBlock.SetVectorArray("_Vector3List", vector4List);
        _propBlock.SetInt("_Vector3Count", vector4List.Count);
        
        GetComponent<Renderer>().SetPropertyBlock(_propBlock);
    }
}