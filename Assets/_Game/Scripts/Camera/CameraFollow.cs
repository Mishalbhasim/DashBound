using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [SerializeField] private float smoothTimeX = 0.2f;
    [SerializeField] private float smoothTimeY = 0.35f;
    [SerializeField] private Vector2 baseOffset = new Vector2(0f, 1.5f);

    [Header("Look Ahead")]
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSmoothTime = 0.15f;
    [SerializeField] private float movementThreshold = 0.1f;

    [Header("Vertical Follow")]
    [SerializeField] private bool followY = true;
    [SerializeField] private float verticalDeadZone = 1f;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float minX, maxX, minY, maxY;

    // Shake 
    private bool isShaking = false;
    private Vector3 shakeOffset = Vector3.zero;
    

    private Camera cam;
    private Rigidbody2D targetRb;
    private float currentLookAhead;
    private float targetLookAhead;
    private float lookAheadVelocity;
    private float velocityX;
    private float velocityY;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (target != null)
            targetRb = target.GetComponent<Rigidbody2D>();
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (targetRb == null)
            targetRb = target.GetComponent<Rigidbody2D>();

        HandleLookAhead();

        float desiredX = target.position.x + baseOffset.x + currentLookAhead;
        float desiredY = transform.position.y;

        if (followY)
        {
            float targetY = target.position.y + baseOffset.y;
            if (Mathf.Abs(targetY - transform.position.y) > verticalDeadZone)
                desiredY = targetY;
        }

        float smoothedX = Mathf.SmoothDamp(
            transform.position.x, desiredX, ref velocityX, smoothTimeX);

        float smoothedY = Mathf.SmoothDamp(
            transform.position.y, desiredY, ref velocityY, smoothTimeY);

        Vector3 newPos = new Vector3(smoothedX, smoothedY, transform.position.z);

        if (useBounds)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;
            newPos.x = Mathf.Clamp(newPos.x, minX + halfWidth, maxX - halfWidth);
            newPos.y = Mathf.Clamp(newPos.y, minY + halfHeight, maxY - halfHeight);
        }

        
        transform.position = newPos + shakeOffset;
        
    }

    private void HandleLookAhead()
    {
        if (targetRb == null)
        {
            targetLookAhead = 0f;
        }
        else
        {
            float moveX = targetRb.velocity.x;
            targetLookAhead = Mathf.Abs(moveX) > movementThreshold
                ? Mathf.Sign(moveX) * lookAheadDistance
                : 0f;
        }

        currentLookAhead = Mathf.SmoothDamp(
            currentLookAhead,
            targetLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );
    }

    //called by BossBase on every hit
    public void ShakeCamera(float intensity, float duration)
    {
        if (!isShaking)
            StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private IEnumerator ShakeRoutine(float intensity, float duration)
    {
        isShaking = true;
        float t = 0f;

        while (t < duration)
        {
            shakeOffset = (Vector3)Random.insideUnitCircle * intensity;
            t += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        isShaking = false;
    }
    

    private void OnDrawGizmosSelected()
    {
        if (!useBounds) return;
        Gizmos.color = Color.yellow;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0f);
        Gizmos.DrawWireCube(center, size);
    }
}