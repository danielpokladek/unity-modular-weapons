#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class AttachmentPickerPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Manager _manager;

    [SerializeField]
    ItemUI _itemPrefab;

    [SerializeField]
    TMPro.TMP_Text _panelHeading;

    [SerializeField]
    RectTransform _container;

    private readonly List<ItemUI> _attachmentList = new();
    private readonly Queue<ItemUI> _inactiveItems = new();

    public bool IsVisible => gameObject.activeSelf;

    public void Show(string attachmentName)
    {
        _panelHeading.text = attachmentName;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        Clear();
    }

    public void RefreshButtonList()
    {
        Clear();

        AttachmentPoint? point = _manager.CurrentAttachmentPoint;

        if (point == null)
            return;

        WeaponAttachment? currentAttachment = point.CurrentAttachment;
        int currentAttachmentID = currentAttachment != null ? currentAttachment.ID : -999;

        var removeButton = GetAttachmentButton();
        removeButton.Initialize(null!, "NONE", true);
        removeButton.transform.SetParent(_container);
        removeButton.transform.localScale = Vector3.one;
        _attachmentList.Add(removeButton);

        removeButton.Button.interactable = currentAttachment != null;
        removeButton.Button.onClick.AddListener(() =>
        {
            point.RemoveCurrentAttachment();
        });

        foreach (var attachment in point.AvailableAttachments)
        {
            var attachmentButton = GetAttachmentButton();
            attachmentButton.Initialize(attachment.UISprite, attachment.Name);
            attachmentButton.Button.onClick.AddListener(() =>
            {
                point.RemoveCurrentAttachment();
                point.SetAttachment(attachment.ID);
            });

            if (currentAttachmentID == attachment.ID)
                attachmentButton.Button.interactable = false;

            attachmentButton.transform.SetParent(_container);
            attachmentButton.transform.localScale = Vector3.one;

            _attachmentList.Add(attachmentButton);
        }
    }

    public void Clear()
    {
        while (_attachmentList.Count > 0)
        {
            var item = _attachmentList[0];
            _attachmentList.Remove(item);

            item.Reset();
            _inactiveItems.Enqueue(item);
        }
    }

    private ItemUI GetAttachmentButton()
    {
        if (_inactiveItems.Count == 0)
            return Instantiate(_itemPrefab, Vector2.zero, Quaternion.identity);

        return _inactiveItems.Dequeue();
    }
}
