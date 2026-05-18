using TMPro;
using UnityEngine;

/// <summary>
/// HUD element that displays the player's current lives (Lives: 3).
/// Auto-finds the Player's Health component if not assigned.
/// </summary>
public class LivesDisplay : UIelement
{
    [Tooltip("The TextMeshPro text UI used for display")]
    public TextMeshProUGUI displayText = null;

    [Tooltip("Health component to read from. If left empty, auto-finds the Player at Start.")]
    public Health healthSource = null;

    [Tooltip("Format string. Use {0} for current lives.")]
    public string format = "Lives: {0}";

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
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();

        if (displayText == null) return;

        if (healthSource == null)
        {
            TryAutoFindHealthSource();
            if (healthSource == null)
            {
                displayText.text = string.Format(format, 0);
                return;
            }
        }

        displayText.text = string.Format(format, healthSource.currentLives);
    }
}
