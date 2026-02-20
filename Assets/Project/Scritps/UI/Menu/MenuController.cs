using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    CanvasGroup _canvasGroup;

    [SerializeField]
    Toggle _autoZoomToggle;

    private Manager _manager;

    private Tween? _currentTween;

    public void Initialize()
    {
        _manager = Manager.Instance;

        _autoZoomToggle.isOn = _manager.Settings.AutoZoomToAttachment;
        _autoZoomToggle.onValueChanged.AddListener(
            (val) =>
            {
                _manager.Settings.AutoZoomToAttachment = val;
                _manager.SaveSettings();
            }
        );
    }

    public void ToggleVisibility(bool isInstant = false)
    {
        _currentTween?.Stop();

        var isVisible = _canvasGroup.alpha > 0;
        var from = _canvasGroup.alpha;
        var to = isVisible ? 0 : 1;
        var duration = isInstant ? 0 : 0.25f;

        if (!isVisible)
        {
            gameObject.SetActive(true);
        }

        _currentTween = Tween
            .Custom(from, to, duration, (val) => _canvasGroup.alpha = val)
            .OnComplete(() =>
            {
                if (isVisible)
                {
                    gameObject.SetActive(false);
                }
            });
    }
}
