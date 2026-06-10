using System.Collections.Generic;
using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Hold R to record the player's motion (max 5s). Release to spawn an Echo clone
    /// at the position recording began; the clone replays the captured frames.
    /// Only one clone exists at a time — starting a new recording dissolves the old one.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class EchoRecorder : MonoBehaviour
    {
        public GameObject echoClonePrefab;
        public GameObject recordIndicator;        // red dot shown above head while recording
        public ParticleSystem recordParticles;    // emitted while recording

        public float maxRecordSeconds = 5f;
        public KeyCode recordKey = KeyCode.R;

        [Header("Audio")]
        public AudioSource audioSource;
        public AudioClip recordStartClip;
        public AudioClip materializeClip;

        public bool IsRecording { get; private set; }
        public float RecordTimeNormalized => maxRecordSeconds <= 0f ? 0f : Mathf.Clamp01(elapsed / maxRecordSeconds);

        PlayerController pc;
        readonly List<RecordedFrame> frames = new List<RecordedFrame>();
        float elapsed;
        int maxFrames;
        EchoClone currentClone;

        void Awake()
        {
            pc = GetComponent<PlayerController>();
            if (recordIndicator != null) recordIndicator.SetActive(false);
        }

        void Update()
        {
            if (GameManager.Paused) return;
            // Recording is gated by player control: while control is disabled — the intro,
            // a blocking narrative beat, or the victory sequence — you can't start (and any
            // in-progress recording stops), so a story beat is never interrupted by an echo.
            if (pc != null && !pc.ControlEnabled)
            {
                if (IsRecording) StopRecording();
                return;
            }
            if (Input.GetKeyDown(recordKey)) StartRecording();
            else if (IsRecording && (Input.GetKeyUp(recordKey) || elapsed >= maxRecordSeconds)) StopRecording();
        }

        void FixedUpdate()
        {
            if (!IsRecording) return;

            frames.Add(new RecordedFrame(transform.position, pc.FacingRight, pc.MoveInput, pc.IsGrounded));
            elapsed += Time.fixedDeltaTime;

            if (frames.Count >= maxFrames) StopRecording();
        }

        void StartRecording()
        {
            if (IsRecording) return;

            if (currentClone != null)
            {
                currentClone.BeginDissolve();
                currentClone = null;
            }

            IsRecording = true;
            elapsed = 0f;
            maxFrames = Mathf.Max(2, Mathf.CeilToInt(maxRecordSeconds / Time.fixedDeltaTime));
            frames.Clear();

            if (recordIndicator != null) recordIndicator.SetActive(true);
            if (recordParticles != null) recordParticles.Play();
            if (audioSource != null && recordStartClip != null) audioSource.PlayOneShot(recordStartClip);
            if (GameManager.Instance != null) GameManager.Instance.SetRecording(true);
        }

        void StopRecording()
        {
            if (!IsRecording) return;
            IsRecording = false;

            if (recordIndicator != null) recordIndicator.SetActive(false);
            if (recordParticles != null) recordParticles.Stop();
            if (GameManager.Instance != null) GameManager.Instance.SetRecording(false);

            if (frames.Count > 1 && echoClonePrefab != null)
            {
                GameObject go = Instantiate(echoClonePrefab, frames[0].position, Quaternion.identity);
                currentClone = go.GetComponent<EchoClone>();
                if (currentClone != null) currentClone.Play(new List<RecordedFrame>(frames));
                if (audioSource != null && materializeClip != null) audioSource.PlayOneShot(materializeClip);
            }
        }
    }
}
