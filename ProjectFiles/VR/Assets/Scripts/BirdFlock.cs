using UnityEngine;

/// <summary>
/// Kus surusu - waypoint loop + kanat cirpma animasyonu.
/// ExecuteAlways = editor preview, Play'siz uçar.
/// Child GameObjects "WingL" ve "WingR" varsa kanat cirpma yapar.
/// </summary>
[ExecuteAlways]
public class BirdFlock : MonoBehaviour
{
    public Vector3[] waypoints;
    public float speed = 5f;
    public float turnSpeed = 2.5f;
    public float waypointReachDistance = 1.2f;
    public float flapSpeed = 10f;
    public float flapAngle = 35f;

    private int currentIdx = 0;
    private Transform wingL, wingR;
    private float timeOffset;

    void OnEnable()
    {
        wingL = transform.Find("WingL");
        wingR = transform.Find("WingR");
        // Her kus icin random kanat fazi
        timeOffset = Random.value * 10f;
    }

    void Start()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0];
            currentIdx = 1 % waypoints.Length;
            if (waypoints.Length > 1)
            {
                Vector3 nextDir = waypoints[currentIdx] - waypoints[0];
                if (nextDir.sqrMagnitude > 0.001f)
                    transform.rotation = Quaternion.LookRotation(nextDir);
            }
        }
    }

    private static float GetPreviewTime()
    {
#if UNITY_EDITOR
        return Application.isPlaying ? Time.time : (float)UnityEditor.EditorApplication.timeSinceStartup;
#else
        return Time.time;
#endif
    }

    void Update()
    {
        // Kanat cirpma (her zaman)
        if (wingL != null && wingR != null)
        {
            float t = GetPreviewTime() + timeOffset;
            float flap = Mathf.Sin(t * flapSpeed) * flapAngle;
            wingL.localRotation = Quaternion.Euler(0f, 0f,  flap);
            wingR.localRotation = Quaternion.Euler(0f, 0f, -flap);
        }

        if (waypoints == null || waypoints.Length < 2) return;

        float dt = Application.isPlaying ? Time.deltaTime : 0.02f;
        Vector3 target = waypoints[currentIdx];
        Vector3 toTarget = target - transform.position;
        float dist = toTarget.magnitude;

        if (dist < waypointReachDistance)
        {
            currentIdx = (currentIdx + 1) % waypoints.Length;
        }
        else
        {
            Vector3 dir = toTarget.normalized;
            transform.position += dir * speed * dt;
            Quaternion lookRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, turnSpeed * dt);
        }
    }
}
