using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Gradually increases game difficulty by speeding up enemy spawners.
/// Every N enemies defeated, all spawners' spawnDelay is multiplied by spawnDelayMultiplier
/// (e.g. 0.85 means each level reduces delay by 15%, capped at minSpawnDelay).
/// Optionally shows a "LEVEL X!" notification.
///
/// Attach to any GameObject in the scene (e.g., create an empty GameObject called "DifficultyManager").
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager instance;

    [Header("Difficulty Settings")]
    [Tooltip("Difficulty level increases every N enemies defeated.")]
    public int enemiesPerLevel = 3;

    [Tooltip("Multiplier applied to spawn delay each level (0.85 = 15% faster spawning).")]
    [Range(0.1f, 1f)] public float spawnDelayMultiplier = 0.85f;

    [Tooltip("Minimum spawn delay - the spawner cannot go below this value.")]
    public float minSpawnDelay = 0.5f;

    [Tooltip("Max difficulty level cap (prevents game from becoming impossible).")]
    public int maxDifficultyLevel = 8;

    [Header("Optional UI Notification")]
    [Tooltip("If set, displays 'LEVEL X!' here when difficulty increases. Leave empty to skip.")]
    public TextMeshProUGUI notificationText;

    [Tooltip("How long the level-up notification stays on screen.")]
    public float notificationDuration = 1.5f;

    [Header("Camera Shake On Level Up (Optional)")]
    [Tooltip("Trigger a small camera shake when leveling up for extra feedback.")]
    public bool shakeOnLevelUp = true;

    /// <summary>Current difficulty level (starts at 1).</summary>
    public int CurrentLevel { get; private set; } = 1;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    private void Update()
    {
        if (GameManager.instance == null) return;
        if (GameManager.instance.gameIsOver) return;

        int defeated = GameManager.instance.EnemiesDefeated;
        int targetLevel = Mathf.Min(1 + (defeated / enemiesPerLevel), maxDifficultyLevel);

        if (targetLevel > CurrentLevel)
        {
            LevelUp(targetLevel);
        }
    }

    private void LevelUp(int newLevel)
    {
        int levelsGained = newLevel - CurrentLevel;
        CurrentLevel = newLevel;

        // Speed up all spawners: each gained level applies the multiplier once
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        foreach (EnemySpawner spawner in spawners)
        {
            for (int i = 0; i < levelsGained; i++)
            {
                spawner.spawnDelay = Mathf.Max(spawner.spawnDelay * spawnDelayMultiplier, minSpawnDelay);
            }
        }

        // Feedback
        ShowNotification("LEVEL " + CurrentLevel + "!");
        if (shakeOnLevelUp && CameraShake.instance != null)
        {
            CameraShake.instance.Shake(0.3f, 0.4f);
        }

        Debug.Log($"[DifficultyManager] Level up to {CurrentLevel}. Spawn delays:");
        foreach (var s in spawners) Debug.Log($"  - {s.name}: {s.spawnDelay:F2}s");
    }

    private void ShowNotification(string text)
    {
        if (notificationText == null) return;
        StopAllCoroutines();
        StartCoroutine(NotificationRoutine(text));
    }

    private IEnumerator NotificationRoutine(string text)
    {
        notificationText.text = text;
        yield return new WaitForSeconds(notificationDuration);
        notificationText.text = "";
    }
}
