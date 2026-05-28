using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Minimal HUD: a row of memory-fragment icons (filled vs outline) top-left, and a
    /// recording "R" indicator top-right that lights red while recording.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public Image[] fragmentIcons;
        public Sprite filledIcon;
        public Sprite outlineIcon;

        public Graphic recordIndicator;
        public Color recordOnColor = new Color(1f, 0.25f, 0.2f, 1f);
        public Color recordOffColor = new Color(1f, 1f, 1f, 0.22f);

        void Awake()
        {
            SetRecording(false);
        }

        public void SetCollected(int collected, int total)
        {
            if (fragmentIcons == null) return;
            for (int i = 0; i < fragmentIcons.Length; i++)
            {
                if (fragmentIcons[i] == null) continue;
                bool used = i < total;
                fragmentIcons[i].enabled = used;
                fragmentIcons[i].sprite = (i < collected) ? filledIcon : outlineIcon;
            }
        }

        public void SetRecording(bool on)
        {
            if (recordIndicator != null) recordIndicator.color = on ? recordOnColor : recordOffColor;
        }
    }
}
