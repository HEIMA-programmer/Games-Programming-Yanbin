using TMPro;
using UnityEngine;

/// <summary>
/// HUD element that displays the player's current health (HP: 3 / 3).
/// Auto-finds the Player's Health component if not assigned in Inspector.
/// Polls every frame so it stays in sync with Health changes.
/// </summary>
public class HealthDisplay : UIelement
{
    [Tooltip("The TextMeshPro text UI used for display")]
    public TextMeshProUGUI displayText = null;

    [Tooltip("Health component to read from. If left empty, auto-finds the Player at Start.")]
    public Health healthSource = null;

    [Tooltip("Format string. Use {0} for current health, {1} for max health.")]
    public string format = "HP: {0} / {1}";

    private void Start()
    {
        TryAutoFindHealthSource();
    }

    private void TryAutoFindHealthSource()
    {
        if (healthSource != null) return;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            healthSource = player.GetComponent<Health>();
        }
    }

    private void Update()
    {
        // Health doesn't push UI updates, so we poll every frame
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();

        if (displayText == null) return;

        // Player may have been destroyed (final death) — reacquire if possible
        if (healthSource == null)
        {
            TryAutoFindHealthSource();
            if (healthSource == null)
            {
                displayText.text = string.Format(format, 0, 0);
                return;
            }
        }

        displayText.text = string.Format(format, healthSource.currentHealth, healthSource.maximumHealth);
    }
}
