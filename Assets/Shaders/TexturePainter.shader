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

             float3 GetBrushLocalUV(float3 worldPos, float3 painterPos, float radius)
            {
                // 世界空间偏移（默认XZ平面，垂直轴为Y，可根据需要修改）
                float3 offset = worldPos - painterPos;
                float2 dir = float2(offset.x, offset.z);   // 改为 offset.xy 即对墙壁使用

                // 除以半径，将范围压缩到 [-1, 1]
                float2 uvRaw = dir / max(radius, 0.0001);
                float dist = length(uvRaw);

                // 从 [-1,1] 映射到 [0,1]
                float2 uvLocal = uvRaw * 0.5 + 0.5;
                return float3(uvLocal, dist);
            }

            // 函数2：根据局部UV和硬度、图集索引采样笔刷纹理，返回遮罩强度
            float EvaluateBrushMask(float2 uvLocal, float dist, float hardness, float brushIndex)
            {
                // 外层软化（超出半径1.0的部分逐渐淡出，保留硬度控制）
                float outerMask = 1.0 - smoothstep(1.0 - hardness * 0.5, 1.0 + 0.01, dist);
                if (outerMask <= 0.0) return 0.0;

                // 图集布局：4x4，共16个格子
                const float cellsPerRow = 4.0;
                const float cellsPerCol = 4.0;
                float cellSizeX = 1.0 / cellsPerRow;
                float cellSizeY = 1.0 / cellsPerCol;

                // 将浮点索引钳制到 0～15
                int idx = clamp((int)brushIndex, 0, 15);
                int col = idx % (int)cellsPerRow;
                int row = idx / (int)cellsPerRow;

                // 计算图集采样UV
                float2 cellOffset = float2(col * cellSizeX, row * cellSizeY);
                float2 uvAtlas = cellOffset + uvLocal * float2(cellSizeX, cellSizeY);

                // 采样笔刷纹理（使用Alpha作为遮罩）
                float brushAlpha = SAMPLE_TEXTURE2D(_BrushTex, sampler_BrushTex, uvAtlas).a;

                // 综合外层软化和纹理alpha
                return brushAlpha * outerMask;
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
                    return half4(0, 0, 1, 1);
                }

                // 1. 计算笔刷局部UV和距离
                float3 brushData = GetBrushLocalUV(i.worldPos.xyz, _PainterPosition, _Radius);

                // 2. 获取笔刷遮罩强度
                float brushStrength = EvaluateBrushMask(brushData.xy, brushData.z, _Hardness, _BrushIndex);

                // 3. 结合绘制强度
                float edge = brushStrength * _Strength;

                // 4. 混合新旧颜色
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                return lerp(col, _PainterColor, edge);
            }
            ENDHLSL
        }
    }
}