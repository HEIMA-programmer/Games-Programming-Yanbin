using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Cross-scene progress, persisted with PlayerPrefs.
    /// </summary>
    public static class GameProgress
    {
        static readonly string[] Levels = { "Level_00", "Level_01", "Level_02" };

        const string CompletedKey = "echo_completed";
        static string FragKey(string scene) => "echo_frag_" + scene;

        public static void SetFragments(string scene, int collected)
        {
            if (collected > PlayerPrefs.GetInt(FragKey(scene), 0))
                PlayerPrefs.SetInt(FragKey(scene), collected);
            PlayerPrefs.Save();
        }

        public static void MarkCompleted()
        {
            PlayerPrefs.SetInt(CompletedKey, 1);
            PlayerPrefs.Save();
        }

        public static bool IsCompleted() => PlayerPrefs.GetInt(CompletedKey, 0) == 1;

        // Dev utility: wipe the local save (not called by gameplay code).
        public static void ResetAll()
        {
            foreach (string s in Levels)
                PlayerPrefs.DeleteKey(FragKey(s));
            PlayerPrefs.DeleteKey(CompletedKey);
            PlayerPrefs.Save();
        }
    }
}
