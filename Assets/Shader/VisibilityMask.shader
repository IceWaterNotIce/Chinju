Shader "Custom/VisibilityMask"
{
    Properties
    {
        _Color ("Mask Color", Color) = (0,0,0,1)
        _CircleCount ("Circle Count", Int) = 0
        [HideInInspector]_Circles ("Circles", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Overlay" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _Color;
            int _CircleCount;
            float4 _Circles[8];

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 worldPos : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex.xy;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                bool visible = false;
                for (int c = 0; c < _CircleCount; c++)
                {
                    float2 center = _Circles[c].xy;
                    float radius = _Circles[c].z;
                    if (distance(i.worldPos, center) < radius)
                    {
                        visible = true;
                        break;
                    }
                }
                if (visible)
                    return fixed4(0,0,0,0); // 完全透明
                else
                    return _Color; // 遮罩顏色（黑色或半透明）
            }
            ENDCG
        }
    }
}