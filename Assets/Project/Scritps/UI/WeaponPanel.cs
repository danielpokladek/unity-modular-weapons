using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct WeaponButton
{
    public ItemUI ItemUI;
    public WeaponAttachment Body;
}

public class WeaponPanel : MonoBehaviour
{
    [SerializeField]
    WeaponButton[] _weaponBodies;

    [SerializeField]
    CanvasGroup _canvasGroup;

    private Manager _manager;
    private Tween? _currentTween;

    private void OnDestroy()
    {
        foreach (var body in _weaponBodies)
            body.ItemUI.Button.onClick.RemoveAllListeners();
    }

    public void Initialize()
    {
        _manager = Manager.Instance;

        foreach (var body in _weaponBodies)
        {
            var bodyPrefab = body.Body;
            var itemUI = body.ItemUI;

            itemUI.Initialize(bodyPrefab.UISprite, bodyPrefab.Name);
            itemUI.Button.onClick.AddListener(() =>
            {
                _manager.CurrentWeapon.ChangeBody(body.Body);
            });
        }
    }

    public void ToggleVisibility()
    {
        _currentTween?.Stop();

        var isVisible = _canvasGroup.alpha > 0;
        var from = _canvasGroup.alpha;
        var to = isVisible ? 0 : 1;

        if (!isVisible)
            gameObject.SetActive(true);

        _currentTween = Tween
            .Custom(from, to, 0.25f, (val) => _canvasGroup.alpha = val)
            .OnComplete(() =>
            {
                if (isVisible)
                    gameObject.SetActive(false);
            });
    }
}
