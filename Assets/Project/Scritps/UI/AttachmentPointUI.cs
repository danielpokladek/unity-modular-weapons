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

    private WeaponAttachmentPoint _attachmentPoint = null!;

    private void Start()
    {
        _image.CrossFadeAlpha(0.4f, 0, true);
    }

    public void Initialize(WeaponAttachmentPoint attachmentPoint)
    {
        _attachmentPoint = attachmentPoint;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.CrossFadeAlpha(1f, 0.15f, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.CrossFadeAlpha(0.5f, 0.15f, true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Manager.Instance.CameraController.SetTarget(_attachmentPoint.Transform);
    }
}
