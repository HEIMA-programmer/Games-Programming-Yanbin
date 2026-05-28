using UnityEngine;
using UnityEngine.EventSystems;

namespace EchoShift
{
    /// <summary>
    /// Plays soft hover / click sounds on a UI button. Color highlight is handled by the
    /// Button's own ColorBlock; this only adds audio. Shares one AudioSource per canvas.
    /// </summary>
    public class ButtonSfx : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        public AudioSource source;
        public AudioClip hoverClip;
        public AudioClip clickClip;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (source != null && hoverClip != null) source.PlayOneShot(hoverClip);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (source != null && clickClip != null) source.PlayOneShot(clickClip);
        }
    }
}
