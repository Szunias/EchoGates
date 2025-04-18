using UnityEngine;
using System.Linq;

public class DayNightCycle : MonoBehaviour
{
    [Header("Cycle Settings")]
    [Tooltip("Duration of a full day in seconds.")]
    public float dayDuration = 120f;

    [Header("Sun Settings")]
    [Tooltip("Directional light representing the sun.")]
    public Light sun;
    [Tooltip("Euler Y rotation of the sun (e.g. 170 for a tilted axis).")]
    public float sunYAxisRotation = 170f;

    // Internal timer
    private float timeElapsed = 0f;

    private void Awake()
    {
        // If you forgot to assign the sun in Inspector, try to grab the one in RenderSettings
        if (sun == null && RenderSettings.sun != null)
            sun = RenderSettings.sun;
    }

    private void Update()
    {
        // Advance time
        timeElapsed += Time.deltaTime;

        // Compute progression through the day (0–1)
        float dayProgress = timeElapsed / dayDuration;

        // Rotate sun from -90° (sunrise) to +270° (next sunrise)
        float sunAngle = Mathf.Lerp(-90f, 270f, dayProgress);
        sun.transform.rotation = Quaternion.Euler(sunAngle, sunYAxisRotation, 0f);

        // Check for end of day
        if (timeElapsed >= dayDuration)
        {
            // Wrap the timer
            timeElapsed -= dayDuration;
            // Trigger resets
            ResetWorldStates();
        }
    }

    /// <summary>
    /// Finds all GameObjects tagged "Resettable" and sends them a ResetState message.
    /// Any component on those objects can implement a public void ResetState() method
    /// to restore its default values at the start of the next day.
    /// </summary>
    private void ResetWorldStates()
    {
        GameObject[] resettableObjects = GameObject.FindGameObjectsWithTag("Resettable");
        foreach (var obj in resettableObjects)
        {
            obj.SendMessage(
                "ResetState",
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    // (Optional) For debugging: visualize the cycle progress in the Inspector
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        float radius = 1f;
        Vector3 dir = Quaternion.Euler(Mathf.Lerp(-90f, 270f, (timeElapsed / dayDuration)), sunYAxisRotation, 0f) * Vector3.forward;
        Gizmos.DrawLine(transform.position, transform.position + dir * radius);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
