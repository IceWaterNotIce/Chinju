Shader "Unlit/MultiTransparentCircles"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BackgroundColor ("Background Color", Color) = (0.533, 0.533, 0.533, 1)
        _CircleCount ("Circle Count", Int) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent" 
        }
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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            fixed4 _BackgroundColor;
            int _CircleCount;
            float4 _Circles[100];

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float alpha = 1.0;
                for (int j = 0; j < _CircleCount; j++)
                {
                    float2 center = _Circles[j].xy;
                    float radius = _Circles[j].z;
                    float dist = distance(i.uv, center);
                    if (dist <= radius)
                    {
                        alpha = 0.0;
                        break;
                    }
                }
                fixed4 finalColor = _BackgroundColor;
                finalColor.a = alpha;
                return finalColor;
            }
            ENDCG
        }
    }
}