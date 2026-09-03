Shader "Unlit/Phbambert"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", color) = (1,1,1,1)
        _Strength("Strength", range(0,1)) = 0.5
        _Gloss("Gloss", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
              // "LightMode"= "ForwardBase"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD1;
                float3 worldPosition : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            //_Gloss("Gloss", float) = 1
            float _Gloss;
            //_Color("Color", color) = (1,1,1,1)
            //_Strength("Strength", range(0,1)) = 0.5
            float3 _Color;
            float _Strength;
             
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            //include these:
            //#include "UnityCG.cginc"
            //#include "Lighting.cginc"

            //BlinnPhong + Lambert
            fixed4 frag (v2f i) : SV_Target
            {
                //diffuse / lambert
                float3 N = normalize(i.normal);
                float3 L = normalize(_WorldSpaceLightPos0);
                float lambert = saturate(dot(N,L));
                float3 diffuseLight = lambert.xxx * _LightColor0.xyz * _Strength;

                //Specular / Blinn Phong
                float3 V = normalize(_WorldSpaceCameraPos - i.worldPosition);
                float3 H = normalize(L + V);
                float3 specularLight = max(0,dot(H,N)).xxx ;
                specularLight = pow(specularLight, _Gloss);
                specularLight *= _LightColor0.xyz;
                specularLight *= lambert;

                //float3 ambient = ShadeSH9(float4(N,1));
              
                return float4(diffuseLight * _Color + specularLight,1);
            }
            ENDCG
        }
    }
}
