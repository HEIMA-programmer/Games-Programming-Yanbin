using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// ESC-toggled pause. Freezes time, shows a panel with Resume / Restart / Main Menu,
    /// and blocks gameplay input (PlayerController + EchoRecorder early-out on Paused).
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        public GameObject panelRoot;
        public Button resumeButton;
        public Button restartButton;
        public Button menuButton;
        public AudioSource sfxSource;
        public AudioClip openClip;

        bool paused;

        void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
            if (restartButton != null) restartButton.onClick.AddListener(Restart);
            if (menuButton != null) menuButton.onClick.AddListener(ToMenu);
        }

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (SceneFader.Instance != null && SceneFader.Instance.IsFading) return;
            Toggle();
        }

        public void Toggle()
        {
            if (paused) Resume(); else Pause();
        }

        public void Pause()
        {
            paused = true;
            if (panelRoot != null) panelRoot.SetActive(true);
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(true);
            if (sfxSource != null && openClip != null) sfxSource.PlayOneShot(openClip);
        }

        public void Resume()
        {
            paused = false;
            if (panelRoot != null) panelRoot.SetActive(false);
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
        }

        public void Restart()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
            Time.timeScale = 1f;
            string scene = SceneManager.GetActiveScene().name;
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(scene);
            else SceneManager.LoadScene(scene);
        }

        public void ToMenu()
        {
            if (GameManager.Instance != null) GameManager.Instance.SetPaused(false);
            Time.timeScale = 1f;
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene("MainMenu");
            else SceneManager.LoadScene("MainMenu");
        }
    }
}
