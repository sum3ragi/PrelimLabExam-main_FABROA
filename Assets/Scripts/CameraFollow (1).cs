using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject followObject;
    public Vector2 followOffset = new Vector2(0f, 0f);
    public float speed = 3f;

    private Vector2 threshold;
    private Rigidbody2D rb;
    private Camera cam;

    private void Start()
    {
        // Cache local camera component instead of using Camera.main
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        if (followObject != null)
        {
            rb = followObject.GetComponent<Rigidbody2D>();
        }

        threshold = CalculateThreshold();
    }

    private void FixedUpdate()
    {
        if (followObject == null) return;

        Vector3 targetPos = followObject.transform.position;
        Vector3 camPos = transform.position;

        // Measure distance on X and Y
        float distX = Mathf.Abs(camPos.x - targetPos.x);
        float distY = Mathf.Abs(camPos.y - targetPos.y);

        // Move target position if distance exceeds deadzone threshold
        if (distX >= threshold.x)
        {
            camPos.x = targetPos.x + followOffset.x;
        }

        if (distY >= threshold.y)
        {
            camPos.y = targetPos.y + followOffset.y;
        }

        // Adjust speed based on player movement
        float currentSpeed = speed;
        if (rb != null && rb.linearVelocity.magnitude > speed)
        {
            currentSpeed = rb.linearVelocity.magnitude;
        }

        // Move camera smoothly while PRESERVING camera's original Z-axis (-10)
        transform.position = Vector3.MoveTowards(transform.position, camPos, currentSpeed * Time.fixedDeltaTime);
    }

    private Vector2 CalculateThreshold()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null) return Vector2.zero;

        Rect pixelRect = cam.pixelRect;
        float aspect = pixelRect.width / pixelRect.height;
        
        // Define a reasonable inner deadzone (20% of screen size)
        Vector2 deadzone = new Vector2(cam.orthographicSize * aspect * 0.2f, cam.orthographicSize * 0.2f);
        return deadzone;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector2 bounds = CalculateThreshold();
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.x * 2f, bounds.y * 2f, 1f));
    }
}