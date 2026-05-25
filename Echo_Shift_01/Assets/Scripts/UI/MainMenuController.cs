using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Main menu logic: Start (fade to Level_01), How to Play (toggle overlay), Quit.
    /// Starts the menu BGM. ESC closes the How-to-Play overlay if open.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        public Button startButton;
        public Button howToButton;
        public Button quitButton;
        public Button backButton;
        public GameObject howToPanel;
        public string firstLevelScene = "Level_01";

        void Awake()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (howToButton != null) howToButton.onClick.AddListener(OpenHowTo);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            if (backButton != null) backButton.onClick.AddListener(CloseHowTo);
        }

        void Start()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && howToPanel != null && howToPanel.activeSelf)
                CloseHowTo();
        }

        void StartGame()
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(firstLevelScene);
            else SceneManager.LoadScene(firstLevelScene);
        }

        void OpenHowTo() { if (howToPanel != null) howToPanel.SetActive(true); }
        void CloseHowTo() { if (howToPanel != null) howToPanel.SetActive(false); }

        void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
