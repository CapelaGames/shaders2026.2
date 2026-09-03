using UnityEngine;

public class Buoy : MonoBehaviour
{
    public GerstnerWaves water;
    public float offset;

    void Update()
    {
        Vector3 normal;
        Vector3 restPos = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 surface = water.GetSurfacePoint(restPos, Time.time + offset, out normal);

        transform.position = new Vector3(transform.position.x, surface.y, transform.position.z);
       // transform.rotation = Quaternion.FromToRotation(Vector3.up, normal) * transform.rotation;
    }
}