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

            WriteWav("click", Click());
            WriteWav("chime", Chime());
            WriteWav("doorslide", DoorSlide());
            WriteWav("land", Land());

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
                float env = Mathf.Exp(-t * 65f);
                float tone = Mathf.Sin(2f * Mathf.PI * 1500f * t) * env * 0.6f;
                float tick = (Random.value * 2f - 1f) * Mathf.Exp(-t * 130f) * 0.15f;
                d[i] = tone + tick;
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
            float dur = 0.15f;
            int n = Mathf.CeilToInt(dur * SampleRate);
            var d = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / SampleRate;
                float thud = Mathf.Sin(2f * Mathf.PI * 95f * t) * Mathf.Exp(-t * 32f) * 0.7f;
                float dust = (Random.value * 2f - 1f) * Mathf.Exp(-t * 60f) * 0.22f;
                d[i] = thud + dust;
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
