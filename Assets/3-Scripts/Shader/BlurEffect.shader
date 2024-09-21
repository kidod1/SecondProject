Shader "Custom/BlurEffect"
{
    Properties
    {
        _Radius("Radius", Range(1, 10)) = 2
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Radius;
            float2 _MainTex_TexelSize; // Add this line to define texel size

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 col = half4(0,0,0,0);

                // Use _MainTex_TexelSize to calculate the texel size
                float2 texSize = _MainTex_TexelSize;

                for (float x = -_Radius; x <= _Radius; x++)
                {
                    for (float y = -_Radius; y <= _Radius; y++)
                    {
                        float2 offset = float2(x * texSize.x, y * texSize.y);
                        col += tex2D(_MainTex, i.uv + offset);
                    }
                }

                return col / ((_Radius * 2.0 + 1.0) * (_Radius * 2.0 + 1.0));
            }
            ENDCG
        }
    }

    // Add a separate pass to set texel size
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float2 _MainTex_TexelSize; // Define texel size as a property
            sampler2D _MainTex;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : COLOR
            {
                half4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
