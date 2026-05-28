using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift
{
    /// <summary>
    /// Modern HUD: single fragment icon + "X / Y" counter top-left; "R" record indicator
    /// top-right that lights red while recording.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Fragments")]
        public Image iconImage;
        public TMP_Text counterText;
        public Color collectedColor = new Color(1f, 1f, 1f, 1f);    // 1-Bit: full white = collected
        public Color emptyColor = new Color(1f, 1f, 1f, 0.4f);      // 1-Bit: dim white = empty

        [Header("Record")]
        public Graphic recordIndicator;
        public Color recordOnColor = new Color(1f, 1f, 1f, 1f);  // 1-Bit: white = recording
        public Color recordOffColor = new Color(1f, 1f, 1f, 0.22f);

        // Legacy fields kept so older prefabs/scenes don't break — unused by SetCollected.
        public Image[] fragmentIcons;
        public Sprite filledIcon;
        public Sprite outlineIcon;

        void Awake()
        {
            SetRecording(false);
        }

        public void SetCollected(int collected, int total)
        {
            if (counterText != null) counterText.text = $"{collected} / {total}";
            if (iconImage != null) iconImage.color = collected > 0 ? collectedColor : emptyColor;
        }

        public void SetRecording(bool on)
        {
            if (recordIndicator != null) recordIndicator.color = on ? recordOnColor : recordOffColor;
        }
    }
}
