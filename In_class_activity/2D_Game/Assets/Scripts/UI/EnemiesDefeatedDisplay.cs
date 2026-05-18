using TMPro;
using UnityEngine;

/// <summary>
/// HUD element that displays the enemy-defeat progress ("Enemies: 3 / 10").
/// Reads from GameManager.instance and polls every frame.
/// </summary>
public class EnemiesDefeatedDisplay : UIelement
{
    [Tooltip("The TextMeshPro text UI used for display")]
    public TextMeshProUGUI displayText = null;

    [Tooltip("Format string. {0} = current defeated, {1} = required to win.")]
    public string format = "Enemies: {0} / {1}";

    private void Update()
    {
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();

        if (displayText == null) return;

        if (GameManager.instance == null)
        {
            displayText.text = string.Format(format, 0, 0);
            return;
        }

        displayText.text = string.Format(
            format,
            GameManager.instance.EnemiesDefeated,
            GameManager.instance.enemiesToDefeat
        );
    }
}
