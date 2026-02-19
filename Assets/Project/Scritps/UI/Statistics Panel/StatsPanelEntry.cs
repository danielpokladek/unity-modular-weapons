#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsPanelEntry : MonoBehaviour
{
    [SerializeField]
    public Image? _currentSlider = null!;

    [SerializeField]
    public Image? _potentialSlider = null!;

    [SerializeField]
    public TMP_Text _valueText = null!;

    private void Start()
    {
        if (_currentSlider)
            _currentSlider.fillAmount = 0;

        if (_potentialSlider)
            _potentialSlider.fillAmount = 0;

        _valueText.text = "0";
    }

    public void SetCurrentSliderProgress(float progress)
    {
        if (!_currentSlider)
            return;

        _currentSlider.fillAmount = progress;
    }

    public void SetPotentialSliderProgress(float progress)
    {
        if (!_potentialSlider)
            return;

        _potentialSlider.fillAmount = progress;
    }

    public void SetValueText(float newValue)
    {
        _valueText.text = newValue.ToString();
    }
}
