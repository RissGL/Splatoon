Shader "LSQ/Effect/Paint/UnwrapWorld"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv1 : TEXCOORD1;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 worldPos : TEXCOORD;
    };

    v2f vert (appdata v)
    {
        v2f o;
        //unwrap camera params: 
        //nearClipPlane = 0.0f£»
        //farClipPlane = 1.0f; 
        //orthographicSize = 1.0f; 
        //aspect = 1.0f;
        //to match uvWorldPos xy(-1~1) z(0~1)
        float4 uvWorldPos = float4(v.uv1 * 2 - 1, 0.5, 1);
		o.vertex = mul(UNITY_MATRIX_VP, uvWorldPos);
		o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        return o;
    }

    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        // unwrap world pos
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.worldPos, 1.0);
            }
            ENDCG
        }
    }
}
