Shader "Custom/MaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CircleCount ("Circle Count", Int) = 0
        _Circles ("Circles", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
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
            };

            int _CircleCount;
            float4 _Circles[8];

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.pos.xy / i.pos.w;
                for (int j = 0; j < _CircleCount; j++)
                {
                    float2 center = _Circles[j].xy;
                    float radius = _Circles[j].z;
                    if (distance(uv, center) < radius)
                    {
                        return fixed4(0, 0, 0, 0); // 遮罩區域
                    }
                }
                return fixed4(1, 1, 1, 1); // 可見區域
            }
            ENDCG
        }
    }
}
