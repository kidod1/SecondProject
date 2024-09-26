Shader "Custom/SpriteLitHDR"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {} // �ؽ�ó
        _Color ("Tint", Color) = (1,1,1,1) [HDR] // HDR �Ӽ� �߰�
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5 // ���� �ƿ��� (�ʿ��� ���)
    }

    SubShader
    {
        Tags{"Queue"="Transparent" "RenderType"="Transparent" "LightMode"="UniversalForward"}

        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}

            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // ���� ���� ����
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
            float4 _Color; // HDR ���� �Ӽ�
            half _Cutoff;  // ���� �ƿ���

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // �ؽ�ó�� ������ ����
                half4 texColor = tex2D(_MainTex, IN.uv) * _Color;

                // ���� �ƿ��� ����
                if (texColor.a < _Cutoff)
                    discard;

                return texColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Sprite-Lit-Default"
}
