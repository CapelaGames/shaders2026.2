// IncLighting.cginc  <- to rename correctly, show file name extenstions

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

struct appdata
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
    float3 normal : NORMAL;
};

struct v2f
{
    float2 uv : TEXCOORD0;
    float4 pos : SV_POSITION;
    float3 normal : TEXCOORD1;
    float3 worldPosition : TEXCOORD2;
    LIGHTING_COORDS(3,4)
};

sampler2D _MainTex;
float4 _MainTex_ST;

            //_Gloss("Gloss", float) = 1
float _Gloss;
            //_Color("Color", color) = (1,1,1,1)
            //_Strength("Strength", range(0,1)) = 0.5
float3 _Color;
             
v2f vert(appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
    o.normal = UnityObjectToWorldNormal(v.normal);
    o.worldPosition = mul(unity_ObjectToWorld, v.vertex);
    TRANSFER_VERTEX_TO_FRAGMENT(o);
    return o;
}

fixed4 frag(v2f i) : SV_Target
{
    //diffuse / lambert
    float3 N = normalize(i.normal);
    float3 L = normalize(UnityWorldSpaceLightDir(i.worldPosition));
    float attenuation = LIGHT_ATTENUATION(i);
    float lambert = saturate(dot(N, L));
    float3 diffuseLight = lambert.xxx * attenuation * _LightColor0.xyz;

                //Specular / Blinn Phong
    float3 V = normalize(_WorldSpaceCameraPos - i.worldPosition);
    float3 H = normalize(L + V);
    float3 specularLight = max(0, dot(H, N)).xxx;   
    specularLight = pow(specularLight, exp2(_Gloss * 11)) *_Gloss * attenuation;
    specularLight *= _LightColor0.xyz;
    specularLight *= lambert; // cuts off the specular if its behind the object

    float3 ambient = ShadeSH9(float4(N,1));

    float4 tex = tex2D(_MainTex, i.uv);
    return float4((diffuseLight + ambient) * _Color * tex.rgb + specularLight, 1);
}