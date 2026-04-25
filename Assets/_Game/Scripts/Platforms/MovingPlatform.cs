using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private SplinePath path;

    private Rigidbody2D rb;
    private int targetIndex = 1;
    private int dir = 1;
    private float waitTimer = 0;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (path != null && path.waypoints.Length > 0)
            transform.position = path.waypoints[0].position;
    }

    private void FixedUpdate()
    {
        if (path == null || path.waypoints.Length < 2) return;

        if (waitTimer > 0)
        {
            waitTimer -= Time.fixedDeltaTime;
            return;
        }

        Vector2 target = path.waypoints[targetIndex].position;
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, path.speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, target) < 0.05f)
        {
            waitTimer = path.waitAtWaypoint;

            if (path.pingPong)
            {
                if (targetIndex == path.waypoints.Length - 1 || targetIndex == 0)
                    dir = -dir;
                targetIndex += dir;
            }
            else
            {
                targetIndex = path.loop ? (targetIndex + 1) % path.waypoints.Length : Mathf.Min(targetIndex + 1, path.waypoints.Length - 1);
            }
        }
    }

    // ─── Carry Player ───
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint2D contact in col.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                col.transform.SetParent(transform);
                break;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
            col.transform.SetParent(null);
    }
}