using UnityEngine;

//https://www.youtube.com/watch?v=B0T7UxjsLxU
[ExecuteAlways]
public class GerstnerWaves : MonoBehaviour
{
    // Must match MAX_WAVES in Waves.shader.
    public const int MaxWaves = 8;

    public Vector4[] waves = new Vector4[]
    {
        new Vector4(0.80f,  0.60f, 0.32f, 51f),
        new Vector4(-0.42f, 0.91f, 0.22f, 23f),
        new Vector4(0.95f, -0.31f, 0.16f, 13f),
        new Vector4(-0.65f,-0.76f, 0.14f, 7.5f),
        new Vector4(0.15f,  0.99f, 0.10f, 3.6f),
    };

    Renderer rend;
    MaterialPropertyBlock block;
    Vector4[] waveBuffer = new Vector4[MaxWaves];

    void OnEnable()
    {
        rend = GetComponent<Renderer>();
        block = new MaterialPropertyBlock();
        Apply();
    }

    void OnValidate() => Apply();

    void Update() => Apply();

    void Apply()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        if (rend == null) return;
        if (block == null) block = new MaterialPropertyBlock();

        int count = Mathf.Min(waves.Length, MaxWaves);
        for (int i = 0; i < MaxWaves; i++)
            waveBuffer[i] = i < count ? waves[i] : Vector4.zero;

        rend.GetPropertyBlock(block);
        block.SetVectorArray("_Waves", waveBuffer);
        block.SetInt("_WaveCount", count);
        block.SetFloat("_MyTime", Time.time);
        rend.SetPropertyBlock(block);
    }

    Vector3 GerstnerWave(Vector4 wave, Vector3 position, ref Vector3 tangent, ref Vector3 binormal)
    {
        Vector2 direction = new Vector2(wave.x, wave.y).normalized;
        float steepness = wave.z;
        float wavelength = wave.w;

        float k = 2f * Mathf.PI / wavelength;
        float speed = Mathf.Sqrt(9.8f / k);
        float f = k * (Vector2.Dot(direction, new Vector2(position.x, position.z)) - speed * Time.time);
        float amplitude = steepness / k;

        tangent += new Vector3(
            -direction.x * direction.x * steepness * Mathf.Sin(f),
             direction.x * steepness * Mathf.Cos(f),
            -direction.x * direction.y * steepness * Mathf.Sin(f));

        binormal += new Vector3(
            -direction.x * direction.y * steepness * Mathf.Sin(f),
             direction.y * steepness * Mathf.Cos(f),
            -direction.y * direction.y * steepness * Mathf.Sin(f));

        return new Vector3(
            direction.x * amplitude * Mathf.Cos(f),
            amplitude * Mathf.Sin(f),
            direction.y * amplitude * Mathf.Cos(f));
    }

    public Vector3 GetSurfacePoint(Vector3 worldPosition, out Vector3 normal)
    {
        Vector3 originalPosition = transform.InverseTransformPoint(worldPosition);
        originalPosition.y = 0f;

        Vector3 tangent = new Vector3(1, 0, 0);
        Vector3 binormal = new Vector3(0, 0, 1);

        Vector3 p = originalPosition;
        for (int i = 0; i < waves.Length && i < MaxWaves; i++)
            p += GerstnerWave(waves[i], originalPosition, ref tangent, ref binormal);

        Vector3 localNormal = Vector3.Cross(binormal, tangent).normalized;

        normal = transform.worldToLocalMatrix.transpose.MultiplyVector(localNormal).normalized;
        return transform.TransformPoint(p);
    }
}
