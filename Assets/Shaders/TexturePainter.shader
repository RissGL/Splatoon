Shader "TNTC/TexturePainter_URP"
{
    Properties
    {
        _PainterColor ("Painter Color", Color) = (0, 0, 0, 0)
        _BrushTex ("Brush Atlas", 2D) = "white" {}
        _BrushIndex ("Brush Index", Float) = 0
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
            
            TEXTURE2D(_BrushTex);
            SAMPLER(sampler_BrushTex);

            float3 _PainterPosition;
            float _Radius;
            float _Hardness;
            float _Strength;
            float4 _PainterColor;
            float _PrepareUV;
            float _BrushIndex;
            
            // 1. 在顶点输入中添加法线获取
            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL; 
            };

            // 2. 将世界法线传递给片元着色器
            struct Varyings
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                float3 normalWS : TEXCOORD2; 
            };

            // 获取笔刷局部 UV 的算法（已重写为支持 3D 任意表面投影）
            float3 GetBrushLocalUV(float3 worldPos, float3 painterPos, float radius, float3 normalWS)
            {
                // 计算真实的 3D 偏移
                float3 offset = worldPos - painterPos;
                
                // 使用真正的 3D 球面距离，不再漏掉 Y 轴
                float dist = length(offset) / max(radius, 0.0001);

                // 基于表面法线，构建局部切线空间 (Tangent Space)
                // 这样笔刷就能根据你涂鸦的墙面朝向自动旋转贴合
                float3 up = abs(normalWS.y) < 0.999 ? float3(0, 1, 0) : float3(1, 0, 0);
                float3 tangent = normalize(cross(up, normalWS));
                float3 bitangent = cross(normalWS, tangent);

                // 将 3D 的球形偏移投影到这堵墙的 2D 平面上
                float2 dir = float2(dot(offset, tangent), dot(offset, bitangent));

                // 压缩到 [-1, 1]
                float2 uvRaw = dir / max(radius, 0.0001);

                // 从 [-1, 1] 映射到 [0, 1]
                float2 uvLocal = uvRaw * 0.5 + 0.5;
                
                return float3(uvLocal, dist);
            }

           float EvaluateBrushMask(float2 uvLocal, float dist, float hardness, float brushIndex)
{

    if (uvLocal.x < 0.0 || uvLocal.x > 1.0 || uvLocal.y < 0.0 || uvLocal.y > 1.0)
    {
        return 0.0;
    }

    const float cellsPerRow = 4.0;
    const float cellsPerCol = 4.0;
    float cellSizeX = 1.0 / cellsPerRow;
    float cellSizeY = 1.0 / cellsPerCol;

    int idx = clamp((int)brushIndex, 0, 15);
    int col = idx % (int)cellsPerRow;
    int row = idx / (int)cellsPerRow;

    float2 cellOffset = float2(col * cellSizeX, row * cellSizeY);
    float2 uvAtlas = cellOffset + uvLocal * float2(cellSizeX, cellSizeY);

    float brushAlpha = SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, uvAtlas).r;

    float outerMask = smoothstep(1.0, clamp(hardness, 0.0, 0.99), dist);

    return brushAlpha * outerMask;
}

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.worldPos = mul(UNITY_MATRIX_M, v.vertex);
                o.uv = v.uv;
                
                // 3. 转换模型法线到世界坐标系
                o.normalWS = TransformObjectToWorldNormal(v.normal);

                float4 uv = float4(0, 0, 1, 1);
                uv.xy = float2(1, _ProjectionParams.x) * (v.uv.xy * float2(2, 2) - float2(1, 1));
                o.vertex = uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                if (_PrepareUV > 0)
                {
                    return half4(0, 0, 1, 1);
                }

                // 4. 将表面法线传入笔刷计算逻辑中
                float3 brushData = GetBrushLocalUV(i.worldPos.xyz, _PainterPosition, _Radius, i.normalWS);

                float brushStrength = EvaluateBrushMask(brushData.xy, brushData.z, _Hardness, _BrushIndex);

                float edge = brushStrength * _Strength;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return lerp(col, _PainterColor, edge);
            }
            ENDHLSL
        }
    }
}