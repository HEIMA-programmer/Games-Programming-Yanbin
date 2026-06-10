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

        // "Record the world you touched": every crate is tracked tick-for-tick with the
        // player frames; only the ones that actually moved get rewound and replayed.
        struct CrateRec
        {
            public PushableCrate crate;
            public Vector2 start;
            public List<Vector2> track;
        }
        readonly List<CrateRec> crateRecs = new List<CrateRec>();

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
            for (int i = 0; i < crateRecs.Count; i++)
            {
                CrateRec r = crateRecs[i];
                r.track.Add(r.crate != null ? r.crate.Position : r.start);
            }
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

            // Snapshot the pushable world. The old clone was dissolved above, which
            // released its crates, so everything we see here is free to track.
            crateRecs.Clear();
            foreach (PushableCrate c in PushableCrate.All)
            {
                if (c == null || c.IsReplaying) continue;
                crateRecs.Add(new CrateRec { crate = c, start = c.Position, track = new List<Vector2>(maxFrames) });
            }

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
                if (currentClone != null)
                {
                    currentClone.Play(new List<RecordedFrame>(frames));

                    // hand the disturbed crates to the clone — they rewind and replay with it
                    var moved = new List<EchoClone.CrateTrack>();
                    foreach (CrateRec r in crateRecs)
                    {
                        if (r.crate == null) continue;
                        bool disturbed = false;
                        for (int i = 0; i < r.track.Count && !disturbed; i++)
                            disturbed = (r.track[i] - r.start).sqrMagnitude > 0.0001f;
                        if (disturbed) moved.Add(new EchoClone.CrateTrack { crate = r.crate, frames = r.track });
                    }
                    if (moved.Count > 0)
                    {
                        currentClone.AttachCrates(moved);
                        NudgePlayerOutOfCrates(moved);
                    }
                }
                if (audioSource != null && materializeClip != null) audioSource.PlayOneShot(materializeClip);
            }
            crateRecs.Clear();
        }

        // A rewound crate can reappear inside the player — squeeze the player out sideways
        // instead of letting the physics depenetration throw them.
        void NudgePlayerOutOfCrates(List<EchoClone.CrateTrack> moved)
        {
            Rigidbody2D body = GetComponent<Rigidbody2D>();
            Collider2D myCol = GetComponent<Collider2D>();
            if (body == null || myCol == null) return;

            foreach (EchoClone.CrateTrack t in moved)
            {
                if (t.crate == null) continue;
                Collider2D crateCol = t.crate.GetComponent<Collider2D>();
                if (crateCol == null || !myCol.bounds.Intersects(crateCol.bounds)) continue;

                float side = Mathf.Sign(body.position.x - crateCol.bounds.center.x);
                if (side == 0f) side = 1f;
                float targetX = crateCol.bounds.center.x +
                    side * (crateCol.bounds.extents.x + myCol.bounds.extents.x + 0.05f);
                body.position = new Vector2(targetX, body.position.y);
            }
        }
    }
}
