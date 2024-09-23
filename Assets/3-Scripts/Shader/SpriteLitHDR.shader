Shader "Custom/SpriteLitHDR"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {} // 텍스처
        _Color ("Tint", Color) = (1,1,1,1) [HDR] // HDR 속성 추가
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // 알파 컷오프 (필요한 경우)
    }

    SubShader
    {
        Tags{"Queue"="Transparent" "RenderType"="Transparent" "LightMode"="UniversalForward"}

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌딩 설정
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionHCS : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color; // HDR 색상 속성
            half _Cutoff;  // 알파 컷오프

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // 텍스처와 색상을 곱함
                half4 texColor = tex2D(_MainTex, IN.uv) * _Color;

                // 알파 컷오프 적용
                if (texColor.a < _Cutoff)
                    discard;

                return texColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Sprite-Lit-Default"
}
