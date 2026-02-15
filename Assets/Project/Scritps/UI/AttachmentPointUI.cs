using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AttachmentPointUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    Image _image;

    private void Start()
    {
        _image.CrossFadeAlpha(0.4f, 0, true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _image.CrossFadeAlpha(1f, 0.15f, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _image.CrossFadeAlpha(0.5f, 0.15f, true);
    }
}
