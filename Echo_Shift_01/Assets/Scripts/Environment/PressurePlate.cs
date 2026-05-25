using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoShift
{
    /// <summary>
    /// Trigger plate that is "pressed" while one or more PlateActivators overlap it.
    /// Depresses ~3px, swaps its light + sprite tint amber→green, and clicks on press.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PressurePlate : MonoBehaviour
    {
        public Transform plateVisual;
        public SpriteRenderer plateSprite;
        public Light2D plateLight;
        public AudioSource audioSource;
        public AudioClip clickClip;

        public Color inactiveColor = new Color(1f, 0.667f, 0f, 1f);   // amber
        public Color activeColor = new Color(0f, 1f, 0.533f, 1f);     // green
        public float depressDistance = 0.09f;                         // ~3px @ PPU 32

        public bool IsPressed { get; private set; }
        public event Action<bool> PressedChanged;

        readonly HashSet<PlateActivator> occupants = new HashSet<PlateActivator>();
        Vector3 upPos;
        Vector3 downPos;

        void Awake()
        {
            if (plateVisual == null) plateVisual = transform;
            if (plateSprite == null) plateSprite = plateVisual.GetComponent<SpriteRenderer>();
            upPos = plateVisual.localPosition;
            downPos = upPos + Vector3.down * depressDistance;
            ApplyVisualState();
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            PlateActivator a = other.GetComponentInParent<PlateActivator>();
            if (a == null) return;
            bool was = IsPressed;
            occupants.Add(a);
            Refresh(was);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            PlateActivator a = other.GetComponentInParent<PlateActivator>();
            if (a == null) return;
            bool was = IsPressed;
            occupants.Remove(a);
            Refresh(was);
        }

        void Refresh(bool was)
        {
            occupants.RemoveWhere(x => x == null);
            IsPressed = occupants.Count > 0;
            if (IsPressed == was) return;

            ApplyVisualState();
            if (IsPressed && audioSource != null && clickClip != null)
                audioSource.PlayOneShot(clickClip);
            PressedChanged?.Invoke(IsPressed);
        }

        void Update()
        {
            Vector3 target = IsPressed ? downPos : upPos;
            plateVisual.localPosition = Vector3.Lerp(
                plateVisual.localPosition, target, 1f - Mathf.Exp(-18f * Time.deltaTime));
        }

        void ApplyVisualState()
        {
            Color c = IsPressed ? activeColor : inactiveColor;
            if (plateLight != null) plateLight.color = c;
            if (plateSprite != null) plateSprite.color = c;
        }
    }
}
