using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Per-level coordinator: holds level config, tracks fragments + time, drives the
    /// recording vignette/HUD, owns pause + white flash + victory + (Level 2) respawn.
    /// One instance per gameplay scene; referenced by UI components wired by the builder.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool Paused { get; private set; }

        [Header("Level config (set by the build script)")]
        public string levelName = "Sector 01";
        public string nextSceneName = "MainMenu";
        [TextArea] public string narrativeLine = "I remember...";
        [TextArea] public string finalMessage = "";
        public int totalFragments = 1;

        [Header("UI references")]
        public HUDController hud;
        public PauseMenu pauseMenu;
        public VictoryScreen victoryScreen;
        public RecordingVignette vignette;
        public Image flashImage;
        public Image hitFlashImage;

        public float flashTime = 0.45f;

        [Header("Detection (stealth)")]
        public float detectionFillTime = 1.6f;    // seconds of continuous sight before caught
        public float detectionDecayMult = 1.8f;   // how fast the meter empties when unseen

        public bool IsCloneActive => activeClones > 0;
        public bool IsVictory => completed;
        public int Collected => collected;

        int activeClones;
        int collected;
        bool completed;
        bool respawning;
        Vector3 checkpoint;
        bool hasCheckpoint;
        PlayerController player;
        CameraShake cameraShake;
        float detection;
        bool seenThisFrame;

        void Awake()
        {
            Instance = this;
            Paused = false;
            Time.timeScale = 1f;
            completed = false;
            if (flashImage != null) SetAlpha(flashImage, 0f);
            if (hitFlashImage != null) SetAlpha(hitFlashImage, 0f);
        }

        void Start()
        {
            player = FindObjectOfType<PlayerController>();
            cameraShake = FindObjectOfType<CameraShake>();
            if (hud != null) { hud.SetCollected(0, totalFragments); hud.SetRecording(false); }
            // generic level track is only the fallback — a SceneMusic in the scene wins
            if (AudioManager.Instance != null && FindObjectOfType<SceneMusic>() == null)
                AudioManager.Instance.PlayLevelMusic();
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            Paused = false;
        }

        // ---- recording / clone ----
        public void SetRecording(bool on)
        {
            if (vignette != null) vignette.SetRecording(on);
            if (hud != null) hud.SetRecording(on);
        }
        public void RegisterClone() => activeClones++;
        public void UnregisterClone() => activeClones = Mathf.Max(0, activeClones - 1);

        // ---- detection (drones report on the physics step the player sits in their cone) ----
        public void ReportSeen() => seenThisFrame = true;

        void FixedUpdate()
        {
            if (Paused || completed || respawning)
            {
                if (respawning) detection = 0f;
                seenThisFrame = false;
                return;
            }
            float per = Mathf.Max(0.1f, detectionFillTime);
            detection = Mathf.Clamp01(detection + (seenThisFrame ? 1f : -detectionDecayMult) * Time.fixedDeltaTime / per);
            seenThisFrame = false;
            if (hitFlashImage != null) SetAlpha(hitFlashImage, detection * 0.55f);
            if (detection >= 1f) { detection = 0f; RespawnPlayer(); }
        }

        // ---- fragments ----
        public void CollectFragment()
        {
            collected++;
            if (hud != null) hud.SetCollected(collected, totalFragments);
            GameProgress.SetFragments(SceneManager.GetActiveScene().name, collected);
        }

        // ---- pause ----
        public void SetPaused(bool p)
        {
            Paused = p;
            Time.timeScale = p ? 0f : 1f;
        }

        // ---- white flash on pickup ----
        public void Flash()
        {
            if (flashImage != null) StartCoroutine(FlashRoutine(flashImage, 0.92f, flashTime));
        }

        IEnumerator FlashRoutine(Image img, float peak, float dur)
        {
            SetAlpha(img, peak);
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                SetAlpha(img, Mathf.Lerp(peak, 0f, t / dur));
                yield return null;
            }
            SetAlpha(img, 0f);
        }

        // ---- victory ----
        public void CompleteLevel()
        {
            if (completed) return;
            completed = true;
            if (player != null) player.ControlEnabled = false;
            float timeTaken = Time.timeSinceLevelLoad;
            string scene = SceneManager.GetActiveScene().name;
            GameProgress.SetTime(scene, timeTaken);
            bool isFinal = string.IsNullOrEmpty(nextSceneName) || nextSceneName == "MainMenu";
            if (isFinal) GameProgress.MarkCompleted();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayVictoryMusic();
            if (victoryScreen != null)
                victoryScreen.Show(levelName, timeTaken, collected, totalFragments, narrativeLine,
                    nextSceneName, finalMessage, GameProgress.TotalFragments(), GameProgress.MaxFragments,
                    GameProgress.TotalTime(), isFinal);
        }

        // ---- checkpoints / respawn (Level 2) ----
        public void SetCheckpoint(Vector3 pos)
        {
            checkpoint = pos;
            hasCheckpoint = true;
        }

        public void RespawnPlayer()
        {
            if (respawning || completed || !hasCheckpoint) return;
            StartCoroutine(RespawnRoutine());
        }

        IEnumerator RespawnRoutine()
        {
            respawning = true;
            if (player == null) player = FindObjectOfType<PlayerController>();
            if (player != null) player.ControlEnabled = false;

            // Death feel: red flash + camera shake + a brief hit-stop. CameraShake runs on
            // unscaled time, so it keeps animating while time is frozen. Kept short so the
            // (sometimes frequent) detection respawns never feel laboured.
            if (hitFlashImage != null) SetAlpha(hitFlashImage, 0.6f);
            if (cameraShake != null) cameraShake.Shake(0.55f);
            Time.timeScale = 0.12f;
            yield return WaitUnscaled(0.09f);
            if (!Paused) Time.timeScale = 1f;
            yield return WaitUnscaled(0.2f);

            if (player != null) player.Respawn(checkpoint);
            yield return WaitUnscaled(0.15f);

            if (hitFlashImage != null)
            {
                float t = 0f, dur = 0.4f;
                while (t < dur)
                {
                    t += Time.unscaledDeltaTime;
                    SetAlpha(hitFlashImage, Mathf.Lerp(0.6f, 0f, t / dur));
                    yield return null;
                }
                SetAlpha(hitFlashImage, 0f);
            }

            if (player != null) player.ControlEnabled = true;
            respawning = false;
        }

        static IEnumerator WaitUnscaled(float seconds)
        {
            float t = 0f;
            while (t < seconds) { t += Time.unscaledDeltaTime; yield return null; }
        }

        static void SetAlpha(Image img, float a)
        {
            Color c = img.color; c.a = a; img.color = c;
        }
    }
}
