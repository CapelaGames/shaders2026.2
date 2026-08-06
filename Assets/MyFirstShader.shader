Shader "Unlit/MyFirstShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Multiply ("Multiply", Float) = 1


        _FloatAmount ("Float Amount", Float) = 1
        _FloatFrequency ("Float Frequency", Float) = 1

        _WiggleSpeed ("Wiggle Speed", Float) = 2
        _WiggleFrequency ("Wiggle Frequency", Float) = 1
        _WiggleAmount ("Wiggle Amount", Float) = 0.2
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
            float _Multiply; // add this in properties _Multiply ("Multiply", Float) = 1

            /*
            Add these in properties above
            _FloatAmount ("Float Amount", Float) = 1
            _FloatFrequency ("Float Frequency", Float) = 1

            _WiggleSpeed ("Wiggle Speed", Float) = 2
            _WiggleFrequency ("Wiggle Frequency", Float) = 1
            _WiggleAmount ("Wiggle Amount", Float) = 0.2
            */
            float _FloatAmount, _FloatFrequency, _WiggleSpeed, _WiggleFrequency, _WiggleAmount;

            v2f vert (appdata v)
            {
                v2f o;

                float wiggle = sin(_Time.y * _WiggleSpeed + v.vertex.y * _WiggleFrequency);
                v.vertex.x += wiggle * _WiggleAmount;

                //Object space
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                //world space
                worldPos.y += sin(_Time.y * _FloatFrequency ) * _FloatAmount;
                o.vertex = UnityWorldToClipPos(worldPos) ;
                //clip space
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //float cutoff = (i.uv.x * _Multiply) < 1;
                //return float4( cutoff , 0 ,0,1);

                float sinWave = sin((i.uv.x + _Time.y * 0.1) * _Multiply);
                return float4(sinWave < 0, 0,0,1);
            }
            ENDCG
        }
    }
}
