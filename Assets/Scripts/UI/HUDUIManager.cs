using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HUDUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreDisplay;
    [SerializeField] private TextMeshProUGUI comboDisplay;
    [SerializeField] private TextMeshProUGUI multiplierDisplay;
    [SerializeField] private TextMeshProUGUI timeDisplay;
    [SerializeField] private Slider comboSlider;

    public void SetScoreDisplay(float display)
    {
        scoreDisplay.text = $"Score: {display:F2}";
    }

    public void SetComboDisplay(int display)
    {
        comboDisplay.text = $"Combo! x{display}";
    }
    public void SetMultiplierDisplay(float display)
    {
        multiplierDisplay.text = $"Score x{display}";
    }

    public void SetTimeDisplay(float display)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(display);
        string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 10);
        timeDisplay.text = formattedTime;

    }

    public void DisableComboDisplay()
    {
        comboDisplay.text = "";
    }

    public void DisableMultiplierDisplay()
    {
        multiplierDisplay.text = "";
    }

    public void initComboSlider(float maxTime)
    {
        comboSlider.gameObject.SetActive(true);
        comboSlider.maxValue = maxTime;
    }

    public void SetComboSlider(float time)
    {
        comboSlider.value = time;
    }

    public void DisableComboSlider()
    {
        comboSlider.gameObject.SetActive(false);
    }
}
