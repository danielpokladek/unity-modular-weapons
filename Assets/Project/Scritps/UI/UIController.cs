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

    [SerializeField]
    Sprite _removeAttachmentIcon;

    [SerializeField]
    CanvasGroup _pointsCanvasGroup;

    private Dictionary<WeaponAttachmentPoint, Transform> _attachmentDictionary = new();

    private List<ItemUI> _currentItems = new();
    private List<ItemUI> _inactiveItems = new();

    private bool _isPanelShown;

    private Tween? _uiTween;

    private bool _exploded = false;

    private void Start()
    {
        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentSelected);
        Events.OnAttachmentPointUnfocus.AddListener(() => _ = HandleAttachmentUnselected());

        Controls.InputActions.UI.ToggleUI.performed += _ => ToggleUI();

        HidePanel(true);
    }

    private void Update()
    {
        foreach (var point in _attachmentDictionary)
        {
            var screenPos = Camera.main.WorldToScreenPoint(point.Key.Transform.position);
            point.Value.transform.position = screenPos;

            if (_isPanelShown && point.Key == Manager.Instance.CurrentAttachmentPoint)
            {
                var pointHeight = (point.Value.transform as RectTransform)?.sizeDelta.y ?? 30;
                var panelPos = screenPos;
                panelPos.y -= pointHeight;

                _itemsPanel.transform.position = panelPos;
            }
        }
    }

    public void HandleExplodeButtonPressed()
    {
        _exploded = !_exploded;

        if (_exploded)
        {
            Events.OnExplodeWeapon.Invoke();
        }
        else
        {
            Events.OnCompactWeapon.Invoke();
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

        ShowPanel(point);
    }

    private async Task HandleAttachmentUnselected()
    {
        HidePanel();

        ClearExistingItems();
    }

    private void RefreshCurrentItems()
    {
        ClearExistingItems();

        WeaponAttachmentPoint? point = Manager.Instance.CurrentAttachmentPoint;

        if (point == null)
            return;

        var removeButton = GetItemUI();
        removeButton.ItemImage.sprite = _removeAttachmentIcon;
        removeButton.Button.onClick.AddListener(() =>
        {
            point.RemoveCurrentAttachment();
            RefreshCurrentItems();
        });
        removeButton.Button.interactable = point.CurrentAttachment != null;
        removeButton.transform.SetParent(_itemsContainer);
        removeButton.transform.localScale = Vector3.one;

        _currentItems.Add(removeButton);

        foreach (var attachment in point.AvailableAttachments)
        {
            var newItem = GetItemUI();
            _currentItems.Add(newItem);

            if (attachment.UISprite != null)
            {
                newItem.ItemImage.sprite = attachment.UISprite;
            }

            newItem.Button.onClick.AddListener(() =>
            {
                point.RemoveCurrentAttachment();

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

            if (point.CurrentAttachment?.ID == attachment.ID)
            {
                newItem.Button.interactable = false;
            }

            newItem.transform.SetParent(_itemsContainer);
            newItem.transform.localScale = Vector3.one;
        }
    }

    public void ShowPanel(WeaponAttachmentPoint point, bool isInstant = false)
    {
        if (_isPanelShown == true)
            return;

        _isPanelShown = true;
        _itemsPanel.gameObject.SetActive(true);

        // var uiPoint = _attachmentDictionary[point];

        // _itemsPanel.transform.localPosition = uiPoint.transform.localPosition;

        // Tween.UIAnchoredPosition(_itemsPanel, Vector2.zero, duration: isInstant ? 0 : 0.15f);
    }

    private void HidePanel(bool isInstant = false)
    {
        _isPanelShown = false;
        _itemsPanel.gameObject.SetActive(false);

        // return Tween.UIAnchoredPosition(
        //     _itemsPanel,
        //     new Vector2(370, 0),
        //     duration: isInstant ? 0 : 0.15f
        // );
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
