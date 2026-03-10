using UnityEngine;

/// <summary>
/// Smooth top-down orthographic camera that follows the car.
/// Attach to the Main Camera. Set Camera component to Orthographic.
/// Portrait aspect is enforced via Camera.rect in Start().
/// </summary>
public class TopDownCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Drag car root here

    [Header("Follow Settings")]
    [Tooltip("Height above the car (Y offset in world space)")]
    public float height = 20f;

    [Tooltip("How fast camera catches up to car")]
    public float smoothSpeed = 5f;

    [Tooltip("Optional slight forward look-ahead based on car velocity")]
    public float lookAheadDistance = 2f;

    private Rigidbody _targetRb;
    private Vector3   _velocity = Vector3.zero;

    void Start()
    {
        if (target != null)
            _targetRb = target.GetComponent<Rigidbody>();

        // Lock to portrait aspect (9:16 typical)
        Camera cam = GetComponent<Camera>();
        cam.orthographic = true;

        // Rotate camera to look straight down
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 lookAhead = Vector3.zero;
        if (_targetRb != null)
        {
            // Flatten velocity to XZ plane for look-ahead
            Vector3 vel = _targetRb.linearVelocity;
            vel.y = 0f;
            lookAhead = vel.normalized * lookAheadDistance;
        }

        Vector3 desiredPos = new Vector3(
            target.position.x + lookAhead.x,
            target.position.y + height,
            target.position.z + lookAhead.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _velocity, 1f / smoothSpeed);
    }
}
