using UnityEngine;

public class SplinePath : MonoBehaviour
{
    [Header("Waypoints — set in scene")]
    public Transform[] waypoints;

    [Header("Settings")]
    public float speed = 2f;
    public bool loop = true;
    public bool pingPong = false;
    public float waitAtWaypoint = 0.5f;

    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
        if (loop && waypoints[0] != null && waypoints[waypoints.Length - 1] != null)
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);

        Gizmos.color = Color.yellow;
        foreach (var wp in waypoints)
            if (wp != null) Gizmos.DrawSphere(wp.position, 0.15f);
    }
}