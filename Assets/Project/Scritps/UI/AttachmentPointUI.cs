#nullable enable

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttachmentPointUI
    : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
{
    [SerializeField]
    Image _image = null!;

    [SerializeField]
    float _selectedAlpha;

    [SerializeField]
    float _normalAlpha;

    [SerializeField]
    float _otherSelectedAlpha;

    private WeaponAttachmentPoint _attachmentPoint = null!;

    private bool _isSelected = false;

    private void Start()
    {
        _image.CrossFadeAlpha(0.4f, 0, true);

        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentPointFocus);
        Events.OnAttachmentPointUnfocus.AddListener(FadeOut);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        FadeIn();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected)
            return;

        FadeOut();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected)
            return;

        _isSelected = true;
        Events.OnAttachmentPointFocus.Invoke(_attachmentPoint);
    }

    public void Initialize(WeaponAttachmentPoint attachmentPoint)
    {
        _attachmentPoint = attachmentPoint;
    }

    private void HandleAttachmentPointFocus(WeaponAttachmentPoint attachmentPoint)
    {
        if (attachmentPoint == _attachmentPoint)
            return;

        _isSelected = false;
        FadeOut();
    }

    private void FadeOut()
    {
        _image.CrossFadeAlpha(
            Manager.Instance.IsAttachmentSelected ? _otherSelectedAlpha : _normalAlpha,
            0.15f,
            true
        );
    }

    private void FadeIn()
    {
        _image.CrossFadeAlpha(_selectedAlpha, 0.15f, true);
    }
}
