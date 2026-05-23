Shader "LSQ/Effect/Paint/Object_URP"
{
    Properties
    {
        [Header(Standard)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpTex ("Normal", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Splat)]
        _ClipValue("Clip Value", Range(0, 1)) = 0.5
        _ClipSoft("Clip Soft", Range(0, 1)) = 0.01
        _SplatBumpTex ("Bump Map", 2D) = "bump" {}
        _SplatBumpScale ("Bump Scale", Range(0, 1)) = 1.0
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.3
        _EdgeBumpOffset ("Edge Bump Offset", Range(1, 10)) = 1.0
        _SplatGlossiness ("Splat Smoothness", Range(0, 1)) = 0.5
        _SplatMetallic ("Splat Metallic", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpTex);       SAMPLER(sampler_BumpTex);
            TEXTURE2D(_SplatTex);      SAMPLER(sampler_SplatTex);
            TEXTURE2D(_SplatBumpTex);  SAMPLER(sampler_SplatBumpTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SplatTex_TexelSize;

                half _Glossiness;
                half _Metallic;
                half4 _Color;

                float _ClipValue;
                float _ClipSoft;
                float _SplatBumpScale;
                float _EdgeWidth;
                float _EdgeBumpOffset;
                half _SplatGlossiness;
                half _SplatMetallic;
            CBUFFER_END

            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_SplatBumpTex : TEXCOORD0;
                float2 uv2_SplatTex : TEXCOORD1;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_SplatBumpTex : TEXCOORD1;
                float2 uv2_SplatTex : TEXCOORD2;
                float3 worldPos : TEXCOORD3;
                float3 worldNormal : TEXCOORD4;
                float4 worldTangent : TEXCOORD5;
                float3 worldBinormal : TEXCOORD6;
            };

            float3x3 cotangent_frame(float3 N, float3 p, float2 uv)
            {
                float3 dp1 = ddx(p);
                float3 dp2 = ddy(p);
                float2 duv1 = ddx(uv);
                float2 duv2 = ddy(uv);

                float3 dp2perp = cross(dp2, N);
                float3 dp1perp = cross(N, dp1);
                float3 T = dp2perp * duv1.x + dp1perp * duv2.x;
                float3 B = dp2perp * duv1.y + dp1perp * duv2.y;

                float invmax = rsqrt(max(dot(T,T), dot(B,B)));
                float3 TinvMax = normalize(T * invmax);
                float3 BinvMax =  normalize(B * invmax);
                return float3x3(float3(TinvMax.x, BinvMax.x, N.x),
                                float3(TinvMax.y, BinvMax.y, N.y),
                                float3(TinvMax.z, BinvMax.z, N.z));
            }

            float3 perturb_normal(float3 localNormal, float3 N, float3 V, float2 uv)
            {
                float3x3 TBN = cotangent_frame(N, V, uv);
                return normalize(mul(TBN, localNormal));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.vertex.xyz);
                OUT.vertex = vertexInput.positionCS;
                OUT.worldPos = vertexInput.positionWS;

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normal, IN.tangent);
                OUT.worldNormal = normalInput.normalWS;
                OUT.worldTangent = float4(normalInput.tangentWS, IN.tangent.w);
                OUT.worldBinormal = normalInput.bitangentWS;

                OUT.uv_MainTex = TRANSFORM_TEX(IN.uv_MainTex, _MainTex);
                OUT.uv_SplatBumpTex = IN.uv_SplatBumpTex;
                OUT.uv2_SplatTex = IN.uv2_SplatTex;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv_MainTex);
                half4 c = MainTex * _Color;
                half4 splatSDF = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, IN.uv2_SplatTex);

                half splatDDX = length(ddx(IN.uv2_SplatTex * _SplatTex_TexelSize.zw));
                half splatDDY = length(ddy(IN.uv2_SplatTex * _SplatTex_TexelSize.zw));
                half clipDist = sqrt(splatDDX * splatDDX + splatDDY * splatDDY);
                half clipDistHard = max(clipDist * _ClipSoft, _ClipSoft);
                half splatMask = smoothstep(_ClipValue - _ClipSoft, _ClipValue + _ClipSoft, splatSDF.a);
                half splatMaskInside = smoothstep(_ClipValue - _EdgeWidth, _ClipValue + _EdgeWidth, splatSDF.a);

                half4 splatSDFx = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, IN.uv2_SplatTex + float2(_SplatTex_TexelSize.x, 0));
                half4 splatSDFy = SAMPLE_TEXTURE2D(_SplatTex, sampler_SplatTex, IN.uv2_SplatTex + float2(0, _SplatTex_TexelSize.y));
                half2 splatNormalOffset = half2(splatSDFx.a - splatSDF.a, splatSDFy.a - splatSDF.a);
                splatNormalOffset = normalize(half3(splatNormalOffset, 1e-5)).xy;
                splatNormalOffset = splatNormalOffset * (1 - splatMaskInside) * _EdgeBumpOffset;
                half2 splatBumpTex = SAMPLE_TEXTURE2D(_SplatBumpTex, sampler_SplatBumpTex, IN.uv_SplatBumpTex).xy;
                splatNormalOffset += (splatBumpTex.xy - 0.5) * _SplatBumpScale;
                half splatNormalOffsetZ = sqrt(1 - saturate(dot(splatNormalOffset, splatNormalOffset)));
                half3 splatNormalTS = normalize(half3(splatNormalOffset, splatNormalOffsetZ));

                half3 worldNormal = normalize(IN.worldNormal);
                half3 worldView = normalize(_WorldSpaceCameraPos - IN.worldPos);
                half3 splatNormalWS = perturb_normal(splatNormalTS, worldNormal, worldView, IN.uv2_SplatTex);

                half3 worldTangent = normalize(IN.worldTangent.xyz);
                half3 worldBinormal = normalize(IN.worldBinormal);
                half2 normalOffsetTS = half2(dot(worldTangent, splatNormalWS), dot(worldBinormal, splatNormalWS));

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpTex, sampler_BumpTex, IN.uv_MainTex));
                normalTS.xy += normalOffsetTS * splatMask;
                normalTS = normalize(normalTS);

                half3 albedo = lerp(c.rgb, splatSDF.rgb, splatMask);
                half metallic = lerp(_Metallic, _SplatMetallic, splatMask);
                half smoothness = lerp(_Glossiness, _SplatGlossiness, splatMask);
                half alpha = c.a;

                // ★ 完全初始化 SurfaceData
                SurfaceData surfaceData = (SurfaceData)0;   // 所有字段清零
                surfaceData.albedo = albedo;
                surfaceData.metallic = metallic;
                surfaceData.smoothness = smoothness;
                surfaceData.specular = 0;
                surfaceData.occlusion = 1;
                surfaceData.alpha = alpha;
                surfaceData.normalTS = normalTS;
                surfaceData.emission = 0;
                // 如果Unity版本需要clearCoat，它们已经是0（因为清零了），所以可以不显式赋值

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.worldPos;
                inputData.normalWS = worldNormal;
                inputData.viewDirectionWS = worldView;
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.worldPos);
                inputData.fogCoord = 0;
                inputData.vertexLighting = half3(0,0,0);
                inputData.bakedGI = SampleSH(worldNormal);

                half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
                return finalColor;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}