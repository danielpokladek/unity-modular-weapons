using PrimeTween;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField]
    CanvasGroup _canvasGroup;

    private Tween? _currentTween;

    public void ToggleMenu(bool isInstant = false)
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
