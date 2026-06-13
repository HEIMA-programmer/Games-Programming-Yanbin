using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Per-level coordinator: holds level config, tracks fragments, drives the
    /// recording vignette/HUD, owns pause + stealth detection + checkpoint respawn.
    /// One instance per gameplay scene; referenced by the scene's UI components.
    /// (Levels end at the AutoDoorExit into the next cutscene act — there is no
    /// in-level victory flow; the removed VictoryScreen path lives in git history.)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public static bool Paused { get; private set; }

        [Header("Level config")]
        public int totalFragments = 1;

        [Header("UI references")]
        public HUDController hud;
        public PauseMenu pauseMenu;
        public RecordingVignette vignette;
        public Image hitFlashImage;

        [Header("Detection (stealth)")]
        public float detectionFillTime = 1.6f;    // seconds of continuous sight before caught
        public float detectionDecayMult = 1.8f;   // how fast the meter empties when unseen

        public bool IsCloneActive => activeClones > 0;
        public int Collected => collected;

        int activeClones;
        int collected;
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
            if (Paused || respawning)
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

        // ---- checkpoints / respawn ----
        public void SetCheckpoint(Vector3 pos)
        {
            checkpoint = pos;
            hasCheckpoint = true;
        }

        public void RespawnPlayer()
        {
            if (respawning || !hasCheckpoint) return;
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
