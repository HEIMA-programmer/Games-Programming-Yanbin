using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace EchoShift.EditorTools
{
    /// <summary>
    /// Synthesizes tiny procedural sound effects and writes them as 16-bit PCM WAV files
    /// (click, chime, door slide, landing). No runtime audio manager — clips are attached
    /// to AudioSources on prefabs.
    /// </summary>
    public static class EchoAudio
    {
        const int SampleRate = 44100;

        public static void GenerateAll()
        {
            EchoBuildUtils.EnsureFolder(EchoBuildUtils.AudioDir);
            Random.InitState(82914);   // deterministic noise → identical WAVs each Build All (no git churn)

            WriteWav("click", Click());
            WriteWav("chime", Chime());
            WriteWav("doorslide", DoorSlide());
            WriteWav("land", Land());
            WriteWav("footstep", Footstep());
            WriteWav("jump", Jump());
            WriteWav("recordstart", RecordStart());
            WriteWav("materialize", Materialize());
            WriteWav("clonedissolve", CloneDissolve());
            WriteWav("platerelease", PlateRelease());
            WriteWav("doorclose", DoorClose());
            WriteWav("uihover", UiHover());
            WriteWav("uiclick", UiClick());
            WriteWav("pauseopen", PauseOpen());
            WriteWav("bgm_menu", MenuDrone());
            WriteWav("bgm_level", LevelAmbient());
            WriteWav("bgm_victory", VictoryChord());

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static float[] Click()
        {
            float dur = 0.07f;
            int n = Mathf.CeilToInt(dur * SampleRate);
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Exp(-t * 60f);
                float tone = Mathf.Sin(2f * Mathf.PI * 1500f * t) * env * 0.5f;
                float ring = Mathf.Sin(2f * Mathf.PI * 2250f * t) * Mathf.Exp(-t * 95f) * 0.2f;  // bright overtone
                float tick = (Random.value * 2f - 1f) * Mathf.Exp(-t * 130f) * 0.13f;
                d[i] = tone + ring + tick;
            }
            return d;
        }

        static float[] Chime()
        {
            float dur = 1.3f;
            int n = Mathf.CeilToInt(dur * SampleRate);
            var d = new float[n];
            float[] f = { 784f, 1175f, 1568f };
            float[] a = { 0.5f, 0.32f, 0.2f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float s = 0f;
                for (int k = 0; k < f.Length; k++)
                    s += Mathf.Sin(2f * Mathf.PI * f[k] * t) * a[k] * Mathf.Exp(-t * (2.6f + k * 0.6f));
                d[i] = s * 0.6f;
            }
            return d;
        }

        static float[] DoorSlide()
        {
            float dur = 0.34f;
            int n = Mathf.CeilToInt(dur * SampleRate);
            var d = new float[n];
            float phase = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float k = t / dur;
                float freq = 130f + k * 80f;
                phase += 2f * Mathf.PI * freq / SampleRate;
                float env = Mathf.Sin(Mathf.PI * k);
                float tone = Mathf.Sin(phase) * env * 0.4f;
                float air = (Random.value * 2f - 1f) * env * 0.18f;
                d[i] = tone + air;
            }
            return d;
        }

        static float[] Land()
        {
            float dur = 0.2f;
            int n = Mathf.CeilToInt(dur * SampleRate);
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float sub   = Mathf.Sin(2f * Mathf.PI * 55f * t) * Mathf.Exp(-t * 24f) * 0.38f;   // weight
                float thud  = Mathf.Sin(2f * Mathf.PI * 95f * t) * Mathf.Exp(-t * 32f) * 0.52f;   // body
                float knock = Mathf.Sin(2f * Mathf.PI * 190f * t) * Mathf.Exp(-t * 55f) * 0.18f;  // attack
                float dust  = (Random.value * 2f - 1f) * Mathf.Exp(-t * 55f) * 0.2f;              // grit
                d[i] = (sub + thud + knock + dust) * 0.8f;
            }
            return d;
        }

        static float[] Footstep()
        {
            float dur = 0.06f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float env = Mathf.Exp(-t * 80f);
                d[i] = ((Random.value * 2f - 1f) * 0.3f + Mathf.Sin(2f * Mathf.PI * 170f * t) * 0.2f) * env;
            }
            return d;
        }

        static float[] Jump()
        {
            float dur = 0.18f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n]; float ph = 0f, ph2 = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float k = t / dur;
                float f = Mathf.Lerp(220f, 560f, k); ph += 2f * Mathf.PI * f / SampleRate;
                ph2 += 2f * Mathf.PI * (f * 1.5f) / SampleRate;   // a fifth above for a brighter lift
                float env = Mathf.Sin(Mathf.PI * k);
                d[i] = (Mathf.Sin(ph) * 0.4f + Mathf.Sin(ph2) * 0.15f + (Random.value * 2f - 1f) * 0.08f) * env;
            }
            return d;
        }

        static float[] RecordStart()
        {
            float dur = 0.14f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float env = Mathf.Clamp01(Mathf.Min(t * 40f, Mathf.Exp(-(t - 0.02f) * 22f)));
                float s = Mathf.Sin(2f * Mathf.PI * 1046f * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * 1568f * t) * 0.25f;
                d[i] = s * env;
            }
            return d;
        }

        static float[] Materialize()
        {
            float dur = 0.55f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            float[] f = { 1318f, 1760f, 2349f, 3136f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float env = Mathf.Exp(-t * 4.5f);
                float vib = 1f + 0.01f * Mathf.Sin(2f * Mathf.PI * 7f * t);
                float s = 0f; for (int k = 0; k < f.Length; k++) s += Mathf.Sin(2f * Mathf.PI * f[k] * vib * t) * (0.3f / (k + 1));
                d[i] = s * env;
            }
            return d;
        }

        static float[] CloneDissolve()
        {
            float dur = 0.55f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float k = t / dur;
                float f = Mathf.Lerp(220f, 110f, k); ph += 2f * Mathf.PI * f / SampleRate;
                float env = Mathf.Exp(-t * 3.5f);
                d[i] = (Mathf.Sin(ph) * 0.4f + Mathf.Sin(ph * 1.5f) * 0.15f) * env;
            }
            return d;
        }

        static float[] PlateRelease()
        {
            float dur = 0.09f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float k = t / dur;
                float f = Mathf.Lerp(600f, 1000f, k); ph += 2f * Mathf.PI * f / SampleRate;
                float env = Mathf.Exp(-t * 45f);
                d[i] = (Mathf.Sin(ph) * 0.5f + (Random.value * 2f - 1f) * 0.1f) * env;
            }
            return d;
        }

        static float[] DoorClose()
        {
            float dur = 0.34f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n]; float ph = 0f;
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float k = t / dur;
                float f = Mathf.Lerp(210f, 120f, k); ph += 2f * Mathf.PI * f / SampleRate;
                float env = Mathf.Sin(Mathf.PI * k);
                d[i] = (Mathf.Sin(ph) * 0.4f + (Random.value * 2f - 1f) * 0.16f) * env;
            }
            return d;
        }

        static float[] UiHover()
        {
            float dur = 0.04f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float env = Mathf.Exp(-t * 120f);
                d[i] = Mathf.Sin(2f * Mathf.PI * 1400f * t) * env * 0.25f;
            }
            return d;
        }

        static float[] UiClick()
        {
            float dur = 0.1f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float env = Mathf.Exp(-t * 30f);
                float s = Mathf.Sin(2f * Mathf.PI * 660f * t) * 0.4f + Mathf.Sin(2f * Mathf.PI * 990f * t) * 0.25f;
                d[i] = s * env;
            }
            return d;
        }

        static float[] PauseOpen()
        {
            float dur = 0.45f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate; float k = t / dur;
                float env = Mathf.Sin(Mathf.PI * k);
                float s = Mathf.Sin(2f * Mathf.PI * 120f * t) * 0.4f + Mathf.Sin(2f * Mathf.PI * 180f * t) * 0.2f;
                d[i] = s * env;
            }
            return d;
        }

        // ---- BGM: seamless loops (integer Hz over an integer-second length) ----
        static float[] MenuDrone()
        {
            float dur = 8f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            int[] f = { 55, 110, 131, 165, 220, 262 };       // + upper shimmer partials
            float[] amp = { 0.26f, 0.2f, 0.14f, 0.14f, 0.07f, 0.05f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float lfo = 0.85f + 0.15f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float shimmer = 0.5f + 0.5f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);   // slow pad swell
                float s = 0f;
                for (int k = 0; k < f.Length; k++)
                {
                    float a = amp[k];
                    if (f[k] >= 200) a *= shimmer;
                    s += Mathf.Sin(2f * Mathf.PI * f[k] * t) * a;
                }
                d[i] = s * lfo * 0.5f;
            }
            return d;
        }

        static float[] LevelAmbient()
        {
            float dur = 8f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            int[] f = { 27, 41, 62, 82, 123 };          // + sub-bass (27) and a high shimmer (123)
            float[] amp = { 0.22f, 0.26f, 0.18f, 0.12f, 0.07f };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float lfo = 0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * 0.125f * t);     // slow amplitude swell
                float shimmer = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);  // pad layer breathes in/out
                float s = 0f;
                for (int k = 0; k < f.Length; k++)
                {
                    float a = amp[k];
                    if (f[k] >= 100) a *= shimmer;
                    s += Mathf.Sin(2f * Mathf.PI * f[k] * t) * a;
                }
                d[i] = s * lfo * 0.45f;
            }
            return d;
        }

        static float[] VictoryChord()
        {
            float dur = 4f; int n = Mathf.CeilToInt(dur * SampleRate); var d = new float[n];
            int[] f = { 131, 165, 196, 262 };
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float env = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);
                float s = 0f; for (int k = 0; k < f.Length; k++) s += Mathf.Sin(2f * Mathf.PI * f[k] * t) * 0.22f;
                d[i] = s * env * 0.6f;
            }
            return d;
        }

        static void WriteWav(string name, float[] data)
        {
            string path = $"{EchoBuildUtils.AudioDir}/{name}.wav";
            using (var fs = new FileStream(path, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                int dataLen = data.Length * 2;
                bw.Write(Encoding.ASCII.GetBytes("RIFF"));
                bw.Write(36 + dataLen);
                bw.Write(Encoding.ASCII.GetBytes("WAVE"));
                bw.Write(Encoding.ASCII.GetBytes("fmt "));
                bw.Write(16);
                bw.Write((short)1);            // PCM
                bw.Write((short)1);            // mono
                bw.Write(SampleRate);
                bw.Write(SampleRate * 2);      // byte rate
                bw.Write((short)2);            // block align
                bw.Write((short)16);           // bits
                bw.Write(Encoding.ASCII.GetBytes("data"));
                bw.Write(dataLen);
                foreach (float f in data)
                    bw.Write((short)(Mathf.Clamp(f, -1f, 1f) * 32767f));
            }
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
