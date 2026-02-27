#nullable enable

using PrimeTween;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    MenuController _menuPanel = null!;

    [SerializeField]
    AttachmentPickerController _attachmentPicker = null!;

    [SerializeField]
    AttachmentPointController _attachmentPointController = null!;

    [SerializeField]
    CanvasGroup _pointsCanvasGroup = null!;

    private Tween? _uiTween;
    public AttachmentPointController AttachmentPointController => _attachmentPointController;

    public void Initialize()
    {
        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentSelected);
        Events.OnAttachmentPointUnfocus.AddListener(HandleAttachmentUnselected);
        Events.OnAttachmentChanged.AddListener(() => _attachmentPicker.Refresh());
        Events.OnBodyChanged.AddListener(() =>
        {
            _menuPanel.Hide();
            _attachmentPicker.Hide();
        });

        Controls.InputActions.UI.ToggleUI.performed += _ => ToggleUI();

        _menuPanel.Initialize();
        _attachmentPicker.Initialize();
        _attachmentPointController.Initialize();
        _attachmentPicker.Hide(true);
    }

    public void HandleExplodeButtonPressed()
    {
        if (!Manager.Instance.CurrentWeapon)
            return;

        if (!Manager.Instance.CurrentWeapon.IsExploded)
        {
            Events.OnExplodeWeapon.Invoke(false);
        }
        else
        {
            Events.OnCompactWeapon.Invoke(false);
        }
    }

    private void HandleAttachmentSelected(AttachmentPoint point)
    {
        var heading = string.IsNullOrEmpty(point.Name) ? " " : point.Name;
        _attachmentPicker.Show(heading, point);
    }

    private void HandleAttachmentUnselected()
    {
        _attachmentPicker.Hide();
    }

    private void ToggleUI()
    {
        _uiTween?.Complete();

        var uiVisible = _pointsCanvasGroup.alpha == 1;
        var desiredAlpha = uiVisible ? 0 : 1;

        _uiTween = Tween.Custom(
            _pointsCanvasGroup.alpha,
            desiredAlpha,
            0.2f,
            (val) =>
            {
                _pointsCanvasGroup.alpha = val;
            }
        );
    }
}
