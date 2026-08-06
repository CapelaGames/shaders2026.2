Shader "Unlit/HealthBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Health ("_Health", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            // put that above in props
            // _Health ("_Health", Range(0,1)) = 1
            float _Health;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                float sinShake = sin(_Time.y * 50) * 0.02;
                float shake = sinShake * (_Health < 0.2);
                o.vertex.y += shake;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lowColour = float3(0.8,0,0);
                float3 midColour = float3(0.8,0.8,0);
                float3 highColour = float3(0,0.8,0);
                float3 barColour = _Health < 0.5
                    ? lerp(lowColour, midColour, _Health * 2)
                    : lerp(midColour, highColour, (_Health - 0.5) * 2);

                float healthBarMask = i.uv.x < _Health;

                float3 bgColour = float3(0.1,0.1,0.1);

                float3 outColour = lerp(bgColour,barColour,healthBarMask);
                return float4(outColour.xyz , 1 );
            }
            ENDCG
        }
    }
}
