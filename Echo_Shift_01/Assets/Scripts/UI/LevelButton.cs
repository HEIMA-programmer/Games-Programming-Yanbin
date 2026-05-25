using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// A level-select button: fades to its target scene when clicked.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        public string sceneName = "Level_00";

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Go);
        }

        void Go()
        {
            if (SceneFader.Instance != null) SceneFader.Instance.FadeToScene(sceneName);
            else SceneManager.LoadScene(sceneName);
        }
    }
}
