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
}
