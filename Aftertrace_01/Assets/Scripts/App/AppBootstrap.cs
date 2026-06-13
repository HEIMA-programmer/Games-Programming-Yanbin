using UnityEngine;

namespace EchoShift
{
    /// <summary>
    /// Spawns the persistent App prefab (AudioManager + SceneFader) before the first
    /// scene loads, so the game works when launched from ANY scene (including pressing
    /// Play directly on a level in the editor).
    /// </summary>
    public static class AppBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Init()
        {
            // Keep ticking when the window loses focus — recording/replay timing and
            // editor-driven remote playtests both rely on an uninterrupted game loop.
            Application.runInBackground = true;

            if (AudioManager.Instance != null) return;
            var prefab = Resources.Load<GameObject>("App");
            if (prefab != null)
                Object.Instantiate(prefab);
            else
                Debug.LogWarning("[EchoShift] Resources/App.prefab not found. Restore Assets/Resources/App.prefab from version control.");
        }
    }
}
