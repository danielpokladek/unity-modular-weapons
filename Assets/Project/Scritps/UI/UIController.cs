#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    ItemUI _itemPrefab;

    [SerializeField]
    RectTransform _itemsPanel;

    [SerializeField]
    RectTransform _itemsContainer;

    [SerializeField]
    RectTransform _pointsContainer;

    private Dictionary<WeaponAttachmentPoint, Transform> _attachmentDictionary = new();

    private List<ItemUI> _currentItems = new();
    private List<ItemUI> _inactiveItems = new();

    private bool _isPanelShown;

    private void Start()
    {
        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentSelected);
        Events.OnAttachmentPointUnfocus.AddListener(() => _ = HandleAttachmentUnselected());

        HidePanel(true);
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

    public void UnregisterAttachmentFromUI(WeaponAttachmentPoint point)
    {
        // TODO: Pool those.
        var uiPoint = _attachmentDictionary[point];
        uiPoint.SetParent(null);

        _attachmentDictionary.Remove(point);
    }

    public void UnselectAttachment()
    {
        Events.OnAttachmentPointUnfocus.Invoke();
    }

    private void HandleAttachmentSelected(WeaponAttachmentPoint point)
    {
        ClearExistingItems();
        RefreshCurrentItems();

        ShowPanel();
    }

    private async Task HandleAttachmentUnselected()
    {
        await HidePanel();

        ClearExistingItems();
    }

    private void RefreshCurrentItems()
    {
        ClearExistingItems();

        WeaponAttachmentPoint? point = Manager.Instance.CurrentAttachmentPoint;

        if (point == null)
            return;

        foreach (var attachment in point.AvailableAttachments)
        {
            var newItem = GetItemUI();
            _currentItems.Add(newItem);

            if (attachment.UISprite != null)
            {
                newItem.ItemImage.sprite = attachment.UISprite;
            }

            bool itemIsCurrent = point.CurrentAttachment?.ID == attachment.ID;

            if (itemIsCurrent)
            {
                newItem.Button.interactable = false;
            }
            else
            {
                newItem.Button.onClick.AddListener(() =>
                {
                    if (point.CurrentAttachment != null)
                    {
                        point.CurrentAttachment.RemoveUIPoints();
                        Destroy(point.CurrentAttachment.gameObject);
                    }

                    var newAttachment = Instantiate(
                        attachment,
                        Vector3.zero,
                        Quaternion.identity,
                        point.Transform
                    );
                    newAttachment.transform.localPosition = Vector3.zero;

                    point.CurrentAttachment = newAttachment;
                    RefreshCurrentItems();
                });
            }

            newItem.transform.SetParent(_itemsContainer);
        }
    }

    public void ShowPanel(bool isInstant = false)
    {
        if (_isPanelShown == true)
            return;

        _isPanelShown = true;

        Tween.UIAnchoredPosition(_itemsPanel, Vector2.zero, duration: isInstant ? 0 : 0.15f);
    }

    private Tween HidePanel(bool isInstant = false)
    {
        _isPanelShown = false;

        return Tween.UIAnchoredPosition(
            _itemsPanel,
            new Vector2(370, 0),
            duration: isInstant ? 0 : 0.15f
        );
    }

    private void ClearExistingItems()
    {
        while (_currentItems.Count > 0)
        {
            var item = _currentItems[0];
            _currentItems.Remove(item);

            item.Reset();
            _inactiveItems.Add(item);
        }
    }

    private ItemUI GetItemUI()
    {
        if (_inactiveItems.Count == 0)
            return Instantiate(_itemPrefab, Vector2.zero, Quaternion.identity);

        var item = _inactiveItems[0];
        _inactiveItems.Remove(item);

        return item;
    }
}
