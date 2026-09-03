Shader "Custom/Waves"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _MyTime;

        // Must match GerstnerWaves.MaxWaves in GerstnerWaves.cs.
        #define MAX_WAVES 8
        float4 _Waves[MAX_WAVES];
        int _WaveCount;

        float3 GerstnerWave(float4 wave, float3 position, inout float3 tangent, inout float3 binormal)
        {
            float2 direction = normalize(wave.xy);
            float steepness = wave.z; // /3;
            float wavelength = wave.w;

            float k = 2 * UNITY_PI / wavelength;
            float speed = sqrt( 9.8 / k);
            //float2 direction = normalize(_Direction);
            float f = k * (dot(direction,position.xz) - speed * _MyTime);
            float amplitude = steepness / k;
            //position.x += cos(f) * amplitude * direction.x;
            //position.y = sin(f) * amplitude;
            //position.z += cos(f) * amplitude * direction.y;

            tangent += float3(
                 - direction.x * direction.x  * steepness * sin(f),  //x
                   direction.x * steepness                * cos(f),  //y
                 - direction.x *  direction.y * steepness * sin(f)); //z

            binormal += float3(
                 - direction.x *  direction.y * steepness * sin(f),  //x
                   direction.y * steepness                * cos(f),  //y
                 - direction.y * direction.y  * steepness * sin(f)); //z

            return float3(
                direction.x * amplitude * cos(f),
                amplitude * sin(f),
                direction.y * amplitude * cos(f));
        }

        void vert(inout appdata_full vertexData)
        {
            float3 originalPosition = vertexData.vertex.xyz;
            float3 tangent = float3(1,0,0);
            float3 binormal = float3(0,0,1);

            float3 p = originalPosition;
            for (int i = 0; i < _WaveCount; i++)
                p += GerstnerWave(_Waves[i], originalPosition, tangent, binormal);

            float3 normal = normalize(cross(binormal, tangent));

            vertexData.vertex.xyz = p;
            vertexData.normal = normal;
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
