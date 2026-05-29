Shader "Hidden/MetaballComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaStep ("Alpha Step", Range(0, 1)) = 0.3
        _AlphaStep2 ("Alpha Step2", Range(0, 1)) = 0.7
        _Clip ("Clip", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        ZWrite Off
        ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MetaballDepthRT);
            SAMPLER(sampler_MetaballDepthRT);
            float _AlphaStep;
            float _AlphaStep2;
            float _Clip;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings o;
                o.positionCS = input.positionOS;
                o.uv = input.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half alpha = smoothstep(_AlphaStep, _AlphaStep2, color.a);
                clip(alpha - _Clip);

                float particleDepth = SAMPLE_TEXTURE2D(_MetaballDepthRT, sampler_MetaballDepthRT, i.uv).r;
                if (particleDepth > 0.0001)
                {
                    float sceneRaw = SampleSceneDepth(i.uv);
                    float scene01 = Linear01Depth(sceneRaw, _ZBufferParams);
                    if (scene01 < particleDepth - 0.0001)
                        discard;
                }

                return half4(color.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
