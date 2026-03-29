using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Battery Settings")]
    public int batteriesCollected = 0;
    public int batteriesNeeded = 7;

    [Header("UI References")]
    public Slider batterySlider;
    public TMP_Text batteryText;

    void Start()
    {
        // Setup slider
        if (batterySlider != null)
        {
            batterySlider.maxValue = batteriesNeeded;
            batterySlider.value = 0;
        }

        // Setup text
        if (batteryText != null)
        {
            batteryText.text = "0 / " + batteriesNeeded;
        }
    }

    public void CollectBattery()
    {
        batteriesCollected++;

        // Update slider
        if (batterySlider != null)
        {
            batterySlider.value = batteriesCollected;
        }

        // Update text
        if (batteryText != null)
        {
            batteryText.text = batteriesCollected + " / " + batteriesNeeded;
        }

        // Win condition
        if (batteriesCollected >= batteriesNeeded)
        {
            SceneManager.LoadScene("WinScene");
        }
    }
}