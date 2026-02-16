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

    private void HandleAttachmentSelected(WeaponAttachmentPoint point)
    {
        ClearExistingItems();

        bool attachmentIsCurrent = point == Manager.Instance.CurrentAttachmentPoint;

        foreach (var attachment in point.AvailableAttachments)
        {
            var instance = Instantiate(
                _itemPrefab,
                Vector2.zero,
                Quaternion.identity,
                _itemsContainer
            );

            if (attachment.UISprite != null)
            {
                instance.ItemImage.sprite = attachment.UISprite;
            }

            if (!attachmentIsCurrent)
                continue;

            print("Is Current Point");

            bool itemIsCurrent = point.CurrentAttachment.ID == attachment.ID;

            if (!itemIsCurrent)
                continue;

            print("Is Current Attachment");

            instance.Button.interactable = false;
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
