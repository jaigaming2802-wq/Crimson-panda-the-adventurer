using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;      // Player
    public float smoothSpeed = 5f;
    public Vector3 offset;

    public float minX;            // Level left limit
    public float maxX;            // Level right limit
    public float minY;            // Level bottom limit
    public float maxY;            // Level top limit

    void Start()
    {
        if (target == null) return;

        float startX = Mathf.Clamp(
            target.position.x + offset.x,
            minX,
            maxX
        );

        float startY = Mathf.Clamp(
            target.position.y + offset.y,
            minY,
            maxY
        );

        transform.position = new Vector3(
            startX,
            startY,
            transform.position.z
        );
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetX = target.position.x + offset.x;
        float targetY = target.position.y + offset.y;

        float smoothX = Mathf.Lerp(
            transform.position.x,
            targetX,
            smoothSpeed * Time.deltaTime
        );

        float smoothY = Mathf.Lerp(
            transform.position.y,
            targetY,
            smoothSpeed * Time.deltaTime
        );

        // 🔒 Clamp always (stops blue screen)
        smoothX = Mathf.Clamp(smoothX, minX, maxX);
        smoothY = Mathf.Clamp(smoothY, minY, maxY);

        transform.position = new Vector3(
            smoothX,
            smoothY,
            transform.position.z
        );
    }
}