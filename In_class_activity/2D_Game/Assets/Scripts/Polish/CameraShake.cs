using System.Collections;
using UnityEngine;

/// <summary>
/// Adds a screen-shake offset to the camera when triggered.
/// Attach to the Main Camera (the same GameObject as CameraController).
/// CameraController reads the offset each frame via GetCurrentOffset().
/// Call CameraShake.instance.Shake(duration, magnitude) from anywhere (e.g., Health.TakeDamage).
/// </summary>
public class CameraShake : MonoBehaviour
{
    /// <summary>Singleton instance for easy access from other scripts.</summary>
    public static CameraShake instance;

    [Header("Default Shake Settings")]
    [Tooltip("Default duration of a shake (in seconds) when no value is passed to Shake().")]
    public float defaultDuration = 0.15f;

    [Tooltip("Default magnitude of a shake (world units) when no value is passed to Shake().")]
    public float defaultMagnitude = 0.2f;

    // Internal shake state
    private float shakeEndTime = 0f;
    private float currentMagnitude = 0f;

    private void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Triggers a screen shake. Pass -1 to use the default values.
    /// </summary>
    public void Shake(float duration = -1f, float magnitude = -1f)
    {
        if (duration < 0f) duration = defaultDuration;
        if (magnitude < 0f) magnitude = defaultMagnitude;

        // Stack new shake on top of existing one (extend time + take stronger magnitude)
        shakeEndTime = Mathf.Max(shakeEndTime, Time.time + duration);
        currentMagnitude = Mathf.Max(currentMagnitude, magnitude);
    }

    /// <summary>
    /// Called by CameraController each frame to know how much to offset the camera.
    /// Returns Vector3.zero when no shake is active.
    /// </summary>
    public Vector3 GetCurrentOffset()
    {
        if (Time.time >= shakeEndTime)
        {
            currentMagnitude = 0f;
            return Vector3.zero;
        }
        return new Vector3(
            Random.Range(-currentMagnitude, currentMagnitude),
            Random.Range(-currentMagnitude, currentMagnitude),
            0f
        );
    }
}
