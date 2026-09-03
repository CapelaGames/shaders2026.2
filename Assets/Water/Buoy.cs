using UnityEngine;

public class Buoy : MonoBehaviour
{
    public GerstnerWaves water;
    Vector3 anchor;

    void Start()
    {
        anchor = new Vector3(transform.position.x, 0, transform.position.z);
    }

    void Update()
    {
        const float damping = 0.2f;
        Vector3 normal = Vector3.up;
        Vector3 samplePos = anchor;
        Vector3 surface = anchor;
        for (int i = 0; i < 6; i++)
        {
            surface = water.GetSurfacePoint(samplePos, out normal);
            Vector3 error = surface - samplePos;
            error.y = 0;
            samplePos -= error * damping;
        }

        transform.position = new Vector3(anchor.x, surface.y, anchor.z);
        transform.rotation = Quaternion.FromToRotation(Vector3.up, normal);
    }
}