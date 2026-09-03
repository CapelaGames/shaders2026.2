using UnityEngine;

[ExecuteAlways]
public class GerstnerWaves : MonoBehaviour
{
    // (direction.x, direction.y, steepness, wavelength)
    public Vector4 waveA = new Vector4(1, 0, 0.5f, 10);
    public Vector4 waveB = new Vector4(0, 1, 0.25f, 20);
    public Vector4 waveC = new Vector4(1, 1, 0.15f, 10);

    Renderer rend;
    MaterialPropertyBlock block;

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

        rend.GetPropertyBlock(block);
        block.SetVector("_WaveA", waveA);
        block.SetVector("_WaveB", waveB);
        block.SetVector("_WaveC", waveC);
        rend.SetPropertyBlock(block);
    }

    Vector3 GerstnerWave(Vector4 wave, Vector3 position, ref Vector3 tangent, ref Vector3 binormal, float time)
    {
        Vector2 direction = new Vector2(wave.x, wave.y).normalized;
        float steepness = wave.z;
        float wavelength = wave.w;

        float k = 2f * Mathf.PI / wavelength;
        float speed = Mathf.Sqrt(9.8f / k);
        float f = k * (Vector2.Dot(direction, new Vector2(position.x, position.z)) - speed * time);
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

    public Vector3 GetSurfacePoint(Vector3 originalPosition, float time, out Vector3 normal)
    {
        Vector3 tangent = new Vector3(1, 0, 0);
        Vector3 binormal = new Vector3(0, 0, 1);

        Vector3 p = originalPosition;
        p += GerstnerWave(waveA, originalPosition, ref tangent, ref binormal, time);
        p += GerstnerWave(waveB, originalPosition, ref tangent, ref binormal, time);
        p += GerstnerWave(waveC, originalPosition, ref tangent, ref binormal, time);

        normal = Vector3.Cross(binormal, tangent).normalized;
        return p;
    }
}