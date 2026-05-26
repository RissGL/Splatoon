Shader "TNTC/TexturePainter_URP"
{
    Properties
    {
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float3 _PainterPosition;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            float _PrepareUV;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
            };

            float mask(float3 position, float3 center, float radius, float hardness)
            {
                float m = distance(center, position);
                return 1.0 - smoothstep(radius * hardness, radius, m);
            }

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = v.uv;

                // 使用 UV 坐标构建覆盖整个渲染目标的全屏四边形
                float4 uv = float4(0, 0, 0, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                if (_PrepareUV > 0)
                {
                    return half4(0, 0, 0, 1);
                }

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float f = mask(i.worldPos, _PainterPosition, _Radius, _Hardness);
                float edge = f * _Strength;
                return lerp(col, _PainterColor, edge);
            }
            ENDHLSL
        }
    }
}