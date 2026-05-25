using EchoShift;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Builds and saves every prefab (player, echo, plate, door, moving platform,
    /// collectible, end arch) with components, colliders, lights and particles wired.
    /// </summary>
    public static class EchoPrefabs
    {
        static Material lit;
        static Material unlit;
        static Material particle;
        static int groundLayer;

        public static void GenerateAll()
        {
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.PrefabDir);
            lit = EchoBuildUtils.LoadMaterial(EchoMaterials.LitName);
            unlit = EchoBuildUtils.LoadMaterial(EchoMaterials.UnlitName);
            particle = EchoBuildUtils.LoadMaterial(EchoMaterials.ParticleName);
            groundLayer = EchoBuildUtils.EnsurePhysicsLayer(EchoBuildUtils.GroundLayer);

            GameObject echo = BuildEcho();
            BuildPlayer(echo);
            BuildPressurePlate();
            BuildDoor();
            BuildMovingPlatform();
            BuildCollectible();
            BuildEndArch();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // ----------------------------------------------------------- echo
        static GameObject BuildEcho()
        {
            var root = new GameObject("EchoClone");
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;

            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = EchoBuildUtils.LoadSprite("echo");
            sr.sharedMaterial = unlit;
            sr.sortingLayerName = "Player";
            sr.sortingOrder = -1;
            Color ec = EchoBuildUtils.ColEcho; ec.a = 0.6f;
            sr.color = ec;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            root.AddComponent<PlateActivator>();
            var clone = root.AddComponent<EchoClone>();

            var tr = root.AddComponent<TrailRenderer>();
            tr.sharedMaterial = particle;
            tr.time = 0.4f;
            tr.startWidth = 0.28f;
            tr.endWidth = 0f;
            tr.minVertexDistance = 0.04f;
            tr.numCapVertices = 4;
            tr.sortingLayerName = "Player";
            tr.sortingOrder = -2;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(EchoBuildUtils.ColEcho, 0f), new GradientColorKey(EchoBuildUtils.ColEcho, 1f) },
                new[] { new GradientAlphaKey(0.5f, 0f), new GradientAlphaKey(0f, 1f) });
            tr.colorGradient = grad;

            var ripple = EchoBuildUtils.SpriteGO("Ripple", EchoBuildUtils.LoadSprite("ring"), "Player", 1, root.transform, unlit,
                new Color(0.7f, 0.87f, 1f, 0.9f));
            ripple.SetActive(false);

            var diss = MakePS("DissolveParticles", root.transform, new Color(EchoBuildUtils.ColEcho.r, EchoBuildUtils.ColEcho.g, EchoBuildUtils.ColEcho.b, 1f),
                0.7f, 1.2f, 0.12f, 0f, false, "Player", 2);
            var dissMain = diss.main; dissMain.gravityModifier = -0.5f;
            var dissEm = diss.emission; dissEm.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)18) });

            var lightGO = new GameObject("EchoLight");
            lightGO.transform.SetParent(root.transform, false);
            EchoBuildUtils.AddPointLight(lightGO, EchoBuildUtils.ColEcho, 0.5f, 3f);

            clone.ripple = ripple.transform;
            clone.dissolveParticles = diss;

            return SavePrefab(root, "Echo");
        }

        // ----------------------------------------------------------- player
        static void BuildPlayer(GameObject echoAsset)
        {
            var root = new GameObject("Player") { tag = "Player" };
            root.layer = 0;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.gravityScale = 4f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = root.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.62f, 0.96f);
            col.offset = new Vector2(0f, -0.02f);

            var visual = EchoBuildUtils.SpriteGO("Visual", EchoBuildUtils.LoadSprite("player"), "Player", 0, root.transform, unlit);

            var pc = root.AddComponent<PlayerController>();
            pc.groundMask = 1 << groundLayer;

            var rec = root.AddComponent<EchoRecorder>();
            root.AddComponent<PlateActivator>();

            var anim = root.AddComponent<PlayerAnimator>();
            anim.visual = visual.transform;
            anim.controller = pc;

            var lightGO = new GameObject("PlayerLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            EchoBuildUtils.AddPointLight(lightGO, EchoBuildUtils.ColPlayer, 0.95f, 5f, 0.4f);

            var dot = EchoBuildUtils.SpriteGO("RecordIndicator", EchoBuildUtils.LoadSprite("glow"), "Foreground", 5, root.transform, unlit,
                new Color(1f, 0.2f, 0.15f, 1f));
            dot.transform.localPosition = new Vector3(0f, 1.0f, 0f);
            dot.transform.localScale = Vector3.one * 0.5f;
            var pg = dot.AddComponent<PulseGlow>();
            pg.target = dot.GetComponent<SpriteRenderer>();

            var recPS = MakePS("RecordParticles", root.transform, new Color(1f, 0.45f, 0.18f, 1f),
                0.6f, 0.5f, 0.1f, 14f, false, "Foreground", 4);
            recPS.transform.localPosition = new Vector3(0f, 0.9f, 0f);

            var dust = MakePS("FootDust", root.transform, new Color(0.6f, 0.7f, 0.85f, 0.8f),
                0.5f, 0.6f, 0.12f, 0f, true, "Player", -1);
            dust.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            anim.footDust = dust;
            rec.recordIndicator = dot;
            rec.recordParticles = recPS;
            rec.echoClonePrefab = echoAsset;
            dot.SetActive(false);

            SavePrefab(root, "Player");
        }

        // ----------------------------------------------------------- plate
        static void BuildPressurePlate()
        {
            var root = new GameObject("PressurePlate");
            root.layer = 0;

            var trig = root.AddComponent<BoxCollider2D>();
            trig.isTrigger = true;
            trig.size = new Vector2(1.4f, 0.7f);
            trig.offset = new Vector2(0f, 0.3f);

            var audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            var visual = EchoBuildUtils.SpriteGO("Visual", EchoBuildUtils.LoadSprite("plate"), "Environment", 1, root.transform, unlit);

            var plate = root.AddComponent<PressurePlate>();
            plate.plateVisual = visual.transform;
            plate.plateSprite = visual.GetComponent<SpriteRenderer>();

            var pl = new GameObject("PlateLight");
            pl.transform.SetParent(root.transform, false);
            pl.transform.localPosition = new Vector3(0f, 0.15f, 0f);
            plate.plateLight = EchoBuildUtils.AddPointLight(pl, EchoBuildUtils.ColPlateAmber, 0.9f, 2.2f);

            plate.audioSource = audio;
            plate.clickClip = EchoBuildUtils.LoadAudio("click");

            SavePrefab(root, "PressurePlate");
        }

        // ----------------------------------------------------------- door
        static void BuildDoor()
        {
            var root = new GameObject("Door");
            var audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            var body = new GameObject("Body");
            body.transform.SetParent(root.transform, false);
            body.layer = groundLayer;
            var bsr = body.AddComponent<SpriteRenderer>();
            bsr.sprite = EchoBuildUtils.LoadSprite("door");
            bsr.sharedMaterial = lit;
            bsr.sortingLayerName = "Environment";
            bsr.sortingOrder = 2;
            var bcol = body.AddComponent<BoxCollider2D>();
            bcol.size = new Vector2(0.5f, 2.0f);

            var door = root.AddComponent<Door>();
            door.doorBody = body.transform;
            door.audioSource = audio;
            door.slideClip = EchoBuildUtils.LoadAudio("doorslide");
            door.openDistance = 2.6f;

            SavePrefab(root, "Door");
        }

        // ----------------------------------------------------------- moving platform
        static void BuildMovingPlatform()
        {
            var root = new GameObject("MovingPlatform");
            root.layer = groundLayer;

            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = root.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.0f, 0.6f);

            var visual = EchoBuildUtils.SpriteGO("Visual", EchoBuildUtils.LoadSprite("platform"), "Environment", 1, root.transform, lit);
            visual.transform.localScale = new Vector3(2f, 0.6f, 1f);

            var mp = root.AddComponent<MovingPlatform>();
            mp.riseHeight = 4.5f;
            mp.moveSpeed = 3.2f;

            var lightGO = new GameObject("PlatLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            EchoBuildUtils.AddPointLight(lightGO, new Color(0.5f, 0.75f, 1f, 1f), 0.5f, 2.6f);

            SavePrefab(root, "MovingPlatform");
        }

        // ----------------------------------------------------------- collectible
        static void BuildCollectible()
        {
            var root = new GameObject("Collectible");
            root.layer = 0;

            var col = root.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            var audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            audio.spatialBlend = 0f;

            var glow = EchoBuildUtils.SpriteGO("Glow", EchoBuildUtils.LoadSprite("glow"), "Environment", 2, root.transform, unlit,
                new Color(EchoBuildUtils.ColFragment.r, EchoBuildUtils.ColFragment.g, EchoBuildUtils.ColFragment.b, 0.6f));
            glow.transform.localScale = Vector3.one * 1.4f;
            var gpg = glow.AddComponent<PulseGlow>();
            gpg.target = glow.GetComponent<SpriteRenderer>();
            gpg.minScale = 0.85f;
            gpg.maxScale = 1.2f;

            var visual = EchoBuildUtils.SpriteGO("Visual", EchoBuildUtils.LoadSprite("fragment"), "Environment", 3, root.transform, unlit);
            var vpg = visual.AddComponent<PulseGlow>();
            vpg.target = visual.GetComponent<SpriteRenderer>();
            vpg.pulseScale = false;
            vpg.minAlpha = 0.8f;
            vpg.maxAlpha = 1f;

            var lightGO = new GameObject("FragLight");
            lightGO.transform.SetParent(root.transform, false);
            EchoBuildUtils.AddPointLight(lightGO, EchoBuildUtils.ColFragment, 1f, 3f);

            root.AddComponent<FloatingBob>();
            var coll = root.AddComponent<Collectible>();
            coll.audioSource = audio;
            coll.chimeClip = EchoBuildUtils.LoadAudio("chime");

            SavePrefab(root, "Collectible");
        }

        // ----------------------------------------------------------- end arch
        static void BuildEndArch()
        {
            var root = new GameObject("EndArch");
            EchoBuildUtils.SpriteGO("Visual", EchoBuildUtils.LoadSprite("endarch"), "Environment", 0, root.transform, unlit);

            var lightGO = new GameObject("ArchLight");
            lightGO.transform.SetParent(root.transform, false);
            lightGO.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            EchoBuildUtils.AddPointLight(lightGO, EchoBuildUtils.ColEnd, 0.85f, 4.2f);

            SavePrefab(root, "EndArch");
        }

        // ----------------------------------------------------------- helpers
        static ParticleSystem MakePS(string name, Transform parent, Color color,
            float lifetime, float speed, float size, float rate, bool playOnAwake, string sortingLayer, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ps = go.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = true;
            main.duration = 2f;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = 200;
            main.playOnAwake = playOnAwake;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0f;

            var em = ps.emission;
            em.rateOverTime = rate;

            var sh = ps.shape;
            sh.enabled = true;
            sh.shapeType = ParticleSystemShapeType.Circle;
            sh.radius = 0.12f;

            var colOL = ps.colorOverLifetime;
            colOL.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colOL.color = grad;

            var rend = go.GetComponent<ParticleSystemRenderer>();
            rend.sharedMaterial = particle;
            rend.sortingLayerName = sortingLayer;
            rend.sortingOrder = order;

            if (!playOnAwake) ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            return ps;
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            string path = $"{EchoBuildUtils.PrefabDir}/{name}.prefab";
            EchoBuildUtils.DeleteIfExists(path);
            GameObject asset = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return asset;
        }
    }
}
