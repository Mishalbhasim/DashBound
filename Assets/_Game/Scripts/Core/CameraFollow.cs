using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = new Vector2(0f, 2f);

    [Header("Smoothing")]
    [SerializeField] private float smoothX = 5f;
    [SerializeField] private float smoothY = 3f;

    [Header("Y Deadzone")]
    [SerializeField] private float yDeadzone = 1.5f;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 100f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 20f;

    private float _currentY;

    private void Start()
    {
        if (target == null) return;
        _currentY = target.position.y;
        transform.position = new Vector3(
            target.position.x + offset.x,
            _currentY + offset.y,
            transform.position.z);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // ── X follows instantly with smoothing ──────────────────────
        float targetX = target.position.x + offset.x;
        float newX = Mathf.Lerp(
            transform.position.x,
            targetX,
            smoothX * Time.deltaTime);

        // ── Y only moves outside deadzone ───────────────────────────
        float targetY = target.position.y + offset.y;

        if (Mathf.Abs(targetY - transform.position.y) > yDeadzone)
        {
            _currentY = Mathf.Lerp(
                transform.position.y,
                targetY,
                smoothY * Time.deltaTime);
        }
        else
        {
            _currentY = transform.position.y;
        }

        // ── Apply bounds ────────────────────────────────────────────
        float finalX = useBounds ? Mathf.Clamp(newX, minX, maxX) : newX;
        float finalY = useBounds ? Mathf.Clamp(_currentY, minY, maxY) : _currentY;

        transform.position = new Vector3(finalX, finalY, transform.position.z);
    }

    public void SetTarget(Transform t)
    {
        target = t;
        _currentY = t.position.y;
    }
}