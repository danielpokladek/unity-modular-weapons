#nullable enable

using PrimeTween;
using UnityEngine;

public class StatsPanelController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    CanvasGroup _canvasGroup = null!;

    [Header("Weapon Stats")]
    [SerializeField]
    StatsPanelEntry _weight;

    [SerializeField]
    StatsPanelEntry _ergonomics;

    [SerializeField]
    StatsPanelEntry _accuracy;

    [SerializeField]
    StatsPanelEntry _sightDistance;

    [SerializeField]
    StatsPanelEntry _verticalRecoil;

    [SerializeField]
    StatsPanelEntry _horizontalRecoil;

    [SerializeField]
    StatsPanelEntry _roundsPerMinute;

    private Tween? _activeTween = null;

    public void ToggleStatsPanel()
    {
        _activeTween?.Stop();

        var isVisible = _canvasGroup.alpha > 0;
        var from = _canvasGroup.alpha;
        var to = isVisible ? 0 : 1;

        _activeTween = Tween.Custom(from, to, 0.25f, (val) => _canvasGroup.alpha = val);
    }

    public void UpdateStats(WeaponStats stats)
    {
        _weight.SetValueText(stats.Weight);
        _ergonomics.SetValueText(stats.Ergonomics);
        _accuracy.SetValueText(stats.Accuracy);
        _sightDistance.SetValueText(stats.SightingRange);
        _verticalRecoil.SetValueText(stats.VerticalRecoil);
        _horizontalRecoil.SetValueText(stats.HorizontalRecoil);
        _roundsPerMinute.SetValueText(stats.FireRate);
    }
}
