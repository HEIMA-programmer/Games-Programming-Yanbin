using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Cross-scene progress, persisted with PlayerPrefs.
    /// </summary>
    public static class GameProgress
    {
        public const int MaxFragments = 12;
        static readonly string[] Levels = { "Level_00", "Level_01", "Level_02", "Level_03" };

        const string CompletedKey = "echo_completed";
        static string FragKey(string scene) => "echo_frag_" + scene;
        static string TimeKey(string scene) => "echo_time_" + scene;

        public static void SetFragments(string scene, int collected)
        {
            if (collected > PlayerPrefs.GetInt(FragKey(scene), 0))
                PlayerPrefs.SetInt(FragKey(scene), collected);
            PlayerPrefs.Save();
        }

        public static int GetFragments(string scene) => PlayerPrefs.GetInt(FragKey(scene), 0);

        public static int TotalFragments()
        {
            int t = 0;
            foreach (string s in Levels) t += PlayerPrefs.GetInt(FragKey(s), 0);
            return t;
        }

        public static void SetTime(string scene, float seconds)
        {
            PlayerPrefs.SetFloat(TimeKey(scene), seconds);
            PlayerPrefs.Save();
        }

        public static float TotalTime()
        {
            float t = 0f;
            foreach (string s in Levels) t += PlayerPrefs.GetFloat(TimeKey(s), 0f);
            return t;
        }

        public static void MarkCompleted()
        {
            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.Save();
        }

        public static bool IsCompleted() => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        public static void ResetAll()
        {
            foreach (string s in Levels)
            {
                PlayerPrefs.DeleteKey(FragKey(s));
                PlayerPrefs.DeleteKey(TimeKey(s));
            }
            PlayerPrefs.DeleteKey(CompletedKey);
            PlayerPrefs.Save();
        }
    }
}
