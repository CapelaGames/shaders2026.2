using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterMesh : MonoBehaviour
{
    [Header("Distance-warped plane")]
    [Tooltip("Vertices per side. Total vertex count is resolution^2.")]
    public int resolution = 128;
    [Tooltip("Distance from center to the outer edge of the plane.")]
    public float halfExtent = 800f;
    [Tooltip("Shapes the falloff: X = normalized distance from center (0..1), Y = normalized vertex offset (0..1). A straight diagonal line is evenly spaced (no LOD). Pull the early part of the curve up to spread out the near-camera verts; keep the tail flat to hold density longer at mid-range.")]
    public AnimationCurve densityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("How far below the surface the outer edge drops, so the boundary isn't visible from a grazing angle.")]
    public float skirtDepth = 50f;
    [Tooltip("Camera must move this many world units before the mesh recenters, so tiny movements don't re-upload the mesh every frame.")]
    public float recenterThreshold = 0.5f;
    [Tooltip("Extra vertical padding on the mesh bounds, since wave displacement happens in the vertex shader and isn't reflected in the raw mesh geometry.")]
    public float boundsPadding = 10f;
    [Tooltip("Camera to follow. Defaults to Camera.main in play mode, or the scene view camera in the editor.")]
    public Camera targetCamera;

    Mesh mesh;
    Vector3[] baseVertices;
    Vector3[] vertices;
    float[] offsets;
    Vector3 lastCenter;
    bool built;

    void OnEnable()
    {
        built = false;
        Build();
    }

    void OnValidate()
    {
        resolution = Mathf.Max(2, resolution);
        halfExtent = Mathf.Max(0.01f, halfExtent);
        built = false;
    }

    void Update()
    {
        if (!built) Build();

        Camera cam = GetCamera();
        if (cam == null) return;

        Vector3 center = transform.InverseTransformPoint(cam.transform.position);
        center.y = 0f;

        if ((center - lastCenter).sqrMagnitude > recenterThreshold * recenterThreshold)
        {
            Recenter(center);
        }
    }

    Camera GetCamera()
    {
        if (targetCamera != null) return targetCamera;
#if UNITY_EDITOR
        if (!Application.isPlaying && SceneView.lastActiveSceneView != null)
            return SceneView.lastActiveSceneView.camera;
#endif
        return Camera.main;
    }

    void Build()
    {
        offsets = new float[resolution];
        for (int i = 0; i < resolution; i++)
        {
            float t = resolution == 1 ? 0f : (2f * i / (resolution - 1) - 1f); // -1..1
            float frac = Mathf.Max(0f, densityCurve.Evaluate(Mathf.Abs(t)));
            offsets[i] = Mathf.Sign(t) * halfExtent * frac;
        }

        int vertCount = resolution * resolution;
        baseVertices = new Vector3[vertCount];
        vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                bool edge = i == 0 || i == resolution - 1 || j == 0 || j == resolution - 1;
                float y = edge ? -skirtDepth : 0f;
                baseVertices[i * resolution + j] = new Vector3(offsets[j], y, offsets[i]);
                uvs[i * resolution + j] = new Vector2((float)j / (resolution - 1), (float)i / (resolution - 1));
            }
        }

        int quadsPerSide = resolution - 1;
        int[] triangles = new int[quadsPerSide * quadsPerSide * 6];
        int ti = 0;
        for (int i = 0; i < quadsPerSide; i++)
        {
            int rowStart = i * resolution;
            int nextRowStart = rowStart + resolution;
            for (int j = 0; j < quadsPerSide; j++)
            {
                int v0 = rowStart + j;
                int v1 = nextRowStart + j;
                int v2 = rowStart + j + 1;
                int v3 = nextRowStart + j + 1;

                triangles[ti++] = v0;
                triangles[ti++] = v1;
                triangles[ti++] = v2;

                triangles[ti++] = v2;
                triangles[ti++] = v1;
                triangles[ti++] = v3;
            }
        }

        System.Array.Copy(baseVertices, vertices, vertCount);

        mesh = new Mesh { name = "Water (distance-warped plane)", hideFlags = HideFlags.DontSave };
        mesh.indexFormat = vertCount > 60000
            ? UnityEngine.Rendering.IndexFormat.UInt32
            : UnityEngine.Rendering.IndexFormat.UInt16;
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        lastCenter = new Vector3(float.MaxValue, 0f, float.MaxValue);
        built = true;
    }

    void Recenter(Vector3 center)
    {
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            v.x += center.x;
            v.z += center.z;
            vertices[i] = v;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        Bounds b = mesh.bounds;
        b.Expand(new Vector3(0f, boundsPadding * 2f, 0f));
        mesh.bounds = b;

        lastCenter = center;
    }
}
