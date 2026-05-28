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
        public ParticleSystem ambientParticles;
        public GameObject warmGlow;
        public Button selectButton;
        public GameObject levelSelectPanel;
        public Button selectBackButton;

        void Awake()
        {
            if (howToPanel != null) howToPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (startButton != null) startButton.onClick.AddListener(StartGame);
            if (howToButton != null) howToButton.onClick.AddListener(OpenHowTo);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            if (backButton != null) backButton.onClick.AddListener(CloseHowTo);
            if (selectButton != null) selectButton.onClick.AddListener(OpenLevelSelect);
            if (selectBackButton != null) selectBackButton.onClick.AddListener(CloseLevelSelect);
        }

        void Start()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMenuMusic();
            if (GameProgress.IsCompleted())
            {
                if (ambientParticles != null)
                {
                    var main = ambientParticles.main;
                    main.startColor = new Color(1f, 0.7f, 0.35f, 0.55f);
                }
                if (warmGlow != null) warmGlow.SetActive(true);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (howToPanel != null && howToPanel.activeSelf) CloseHowTo();
                else if (levelSelectPanel != null && levelSelectPanel.activeSelf) CloseLevelSelect();
            }
        }

        void StartGame()
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(firstLevelScene);
            else SceneManager.LoadScene(firstLevelScene);
        }

        void OpenHowTo() { if (howToPanel != null) howToPanel.SetActive(true); }
        void CloseHowTo() { if (howToPanel != null) howToPanel.SetActive(false); }
        void OpenLevelSelect() { if (levelSelectPanel != null) levelSelectPanel.SetActive(true); }
        void CloseLevelSelect() { if (levelSelectPanel != null) levelSelectPanel.SetActive(false); }

        void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
