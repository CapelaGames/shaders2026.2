using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
    public GerstnerWaves water;

    [Header("Floatation")]
    [Tooltip("Sample points around the hull (e.g. bow-left, bow-right, stern-left, stern-right). Each one pushes up independently, which is what produces pitch/roll tilt.")]
    public Transform[] floatPoints;
    [Tooltip("Depth (in world units) at which a point counts as fully submerged. Keeps force from spiking when a point plunges deep below the surface (e.g. on first impact).")]
    public float floatPointRadius = 0.5f;
    [Tooltip("1 = the boat floats with each fully-submerged point exactly at the surface. Raise slightly (1.1-1.3) to have it ride a bit higher.")]
    public float buoyancyMultiplier = 1.2f;
    [Tooltip("Damping on each point's vertical velocity, relative to critical damping. 1 = settles without oscillating; lower = bobbier, higher = risks instability.")]
    public float dampingRatio = 1f;

    [Header("Friction")]
    [Tooltip("Rigidbody linear/angular damping while fully clear of the water.")]
    public float airLinearDrag = 0.05f;
    public float airAngularDrag = 0.05f;
    [Tooltip("Rigidbody linear/angular damping while fully submerged. Much higher than air drag since water is far thicker.")]
    public float waterLinearDrag = 2f;
    public float waterAngularDrag = 3f;

    [Header("Controls (WASD)")]
    public float enginePower = 6000f;
    public float turnPower = 4000f;

    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        ApplyBuoyancy();
        ApplyControls();
    }

    void ApplyBuoyancy()
    {
        if (water == null || floatPoints == null || floatPoints.Length == 0) return;

        float weightPerPoint = rb.mass * -Physics.gravity.y / floatPoints.Length;
        float springConstant = weightPerPoint * buoyancyMultiplier / floatPointRadius;
        float pointMass = rb.mass / floatPoints.Length;
        float criticalDamping = 2f * Mathf.Sqrt(springConstant * pointMass);
        float damping = dampingRatio * criticalDamping;

        float submersionTotal = 0f;

        foreach (Transform point in floatPoints)
        {
            Vector3 surface = water.GetSurfacePoint(point.position, out _);
            float submersion = surface.y - point.position.y;
            if (submersion <= 0f) continue;

            float fraction = Mathf.Clamp01(submersion / floatPointRadius);
            submersionTotal += fraction;
            float buoyancy = fraction * weightPerPoint * buoyancyMultiplier;

            // Only damp the vertical component so it doesn't fight the boat's forward motion.
            float verticalVelocity = rb.GetPointVelocity(point.position).y;
            float dampingForce = -verticalVelocity * damping;

            rb.AddForceAtPosition(Vector3.up * (buoyancy + dampingForce), point.position, ForceMode.Force);
        }

        float wetness = submersionTotal / floatPoints.Length;
        rb.linearDamping = Mathf.Lerp(airLinearDrag, waterLinearDrag, wetness);
        rb.angularDamping = Mathf.Lerp(airAngularDrag, waterAngularDrag, wetness);
    }

    void ApplyControls()
    {
        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        float throttle = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
        float steer = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);

        rb.AddForce(transform.forward * (throttle * enginePower), ForceMode.Force);
        rb.AddTorque(Vector3.up * (steer * turnPower), ForceMode.Force);
    }
}
