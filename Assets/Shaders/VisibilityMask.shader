Shader "Custom/VisibilityMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CircleCount ("Circle Count", Int) = 0
        _Circles ("Circles", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            int _CircleCount;
            float4 _Circles[50]; // 支援最多 50 個圓

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; // 使用頂點的 XY 作為 UV
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                for (int j = 0; j < _CircleCount; j++)
                {
                    float2 center = _Circles[j].xy;
                    float radius = _Circles[j].z;
                    if (distance(uv, center) < radius)
                    {
                        return fixed4(1, 1, 1, 0); // 可見區域為透明
                    }
                }
                return fixed4(0, 0, 0, 1); // 遮罩區域為黑色
            }
            ENDCG
        }
    }
}
