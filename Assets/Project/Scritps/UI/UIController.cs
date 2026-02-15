#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    ItemUI _itemPrefab;

    [SerializeField]
    RectTransform _itemsContainer;

    [SerializeField]
    RectTransform _pointsContainer;

    private Dictionary<WeaponAttachmentPoint, Transform> _attachmentDictionary = new();

    private void Start()
    {
        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentSelected);
        Events.OnAttachmentPointUnfocus.AddListener(HandleAttachmentUnselected);
    }

    private void Update()
    {
        foreach (var point in _attachmentDictionary)
        {
            var screenPos = Camera.main.WorldToScreenPoint(point.Key.Transform.position);
            point.Value.transform.position = screenPos;
        }
    }

    public void RegisterAttachmentToUI(WeaponAttachmentPoint point, Transform uiPoint)
    {
        _attachmentDictionary.Add(point, uiPoint);
        uiPoint.SetParent(_pointsContainer);
    }

    public void UnselectAttachment()
    {
        Events.OnAttachmentPointUnfocus.Invoke();
    }

    private void HandleAttachmentSelected(WeaponAttachmentPoint attachment)
    {
        ClearExistingItems();

        foreach (var possibleAttachment in attachment.AvailableAttachments)
        {
            var instance = Instantiate(
                _itemPrefab,
                Vector2.zero,
                Quaternion.identity,
                _itemsContainer
            );
            // instance.ItemImage.sprite = possibleAttachment.UISprite;
        }
    }

    private void HandleAttachmentUnselected()
    {
        ClearExistingItems();
    }

    private void ClearExistingItems()
    {
        // TODO: Pool these items.
        foreach (RectTransform child in _itemsContainer)
        {
            Destroy(child.gameObject);
        }
    }
}
