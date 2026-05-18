using System.Collections;
using UnityEngine;

/// <summary>
/// Briefly flashes a SpriteRenderer's color when triggered.
/// Attach to any GameObject with a SpriteRenderer (e.g., Player or Enemy).
/// Call Flash() to trigger - typically from Health.TakeDamage.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class HitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("The SpriteRenderer to flash. Auto-fills from this GameObject if not set.")]
    public SpriteRenderer spriteRenderer;

    [Tooltip("The color to flash to (e.g., red for damage).")]
    public Color flashColor = Color.red;

    [Tooltip("How long each flash lasts (in seconds).")]
    public float flashDuration = 0.08f;

    [Tooltip("Number of times to flash on each Flash() call.")]
    public int flashCount = 3;

    private Color originalColor;
    private Coroutine activeFlash;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Triggers the flash effect. Safe to call repeatedly - will restart the flash.
    /// </summary>
    public void Flash()
    {
        if (spriteRenderer == null) return;
        if (activeFlash != null)
        {
            StopCoroutine(activeFlash);
            spriteRenderer.color = originalColor;
        }
        activeFlash = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(flashDuration);
        }
        spriteRenderer.color = originalColor;
        activeFlash = null;
    }
}
