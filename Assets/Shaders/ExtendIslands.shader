Shader "TNTC/ExtendIslands_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UVIslands ("Texture UVIslands", 2D) = "white" {}
        _OffsetUV ("UVOffset", float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;

            TEXTURE2D(_UVIslands);
            SAMPLER(sampler_UVIslands);

            float _OffsetUV;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 offsets[8] =
                {
                    float2(-_OffsetUV, 0), float2(_OffsetUV, 0),
                    float2(0, _OffsetUV), float2(0, -_OffsetUV),
                    float2(-_OffsetUV, _OffsetUV), float2(_OffsetUV, _OffsetUV),
                    float2(_OffsetUV, -_OffsetUV), float2(-_OffsetUV, -_OffsetUV)
                };

                float2 uv = i.uv;
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half4 island = SAMPLE_TEXTURE2D(_UVIslands, sampler_UVIslands, uv);

                // 原逻辑使用蓝通道 < 1 作为扩展条件
                if (island.z < 1.0)
                {
                    half4 extendedColor = color;
                    for (int idx = 0; idx < 8; idx++)
                    {
                        float2 currentUV = uv + offsets[idx] * _MainTex_TexelSize.xy;
                        half4 offsetColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, currentUV);
                        extendedColor = max(offsetColor, extendedColor);
                    }
                    color = extendedColor;
                }
                return color;
            }
            ENDHLSL
        }
    }
}