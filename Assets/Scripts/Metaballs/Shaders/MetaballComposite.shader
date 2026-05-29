Shader "Hidden/MetaballComposite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AlphaStep ("Alpha Step", Range(0,1)) = 0.3
        _AlphaStep2 ("Alpha Step2", Range(0,1)) = 0.7
        _Clip ("Clip", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Overlay" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            Name "MetaballComposite"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MetaballDepthRT);
            SAMPLER(sampler_MetaballDepthRT);
            float _AlphaStep;
            float _AlphaStep2;
            float _Clip;

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.positionOS);
                o.uv = i.uv;
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half a = smoothstep(_AlphaStep, _AlphaStep2, c.a);
                clip(a - _Clip);

                // depth occlusion
                float pd = SAMPLE_TEXTURE2D(_MetaballDepthRT, sampler_MetaballDepthRT, i.uv).r;
                if (pd > 0.0001)
                {
                    float sd = SampleSceneDepth(i.uv);
                    float sl = Linear01Depth(sd, _ZBufferParams);
                    if (sl < pd - 0.0001)
                        discard;
                }

                return half4(c.rgb, a);
            }
            ENDHLSL
        }
    }
}
