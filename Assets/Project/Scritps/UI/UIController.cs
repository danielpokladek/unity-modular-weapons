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
        Events.OnAttachmentChanged.AddListener(RefreshButtonList);

        Controls.InputActions.UI.ToggleUI.performed += _ => ToggleUI();

        HidePanel(true);
    }

    private void Update()
    {
        foreach (var point in _attachmentDictionary)
        {
            if (point.Value == null)
                return;

            var screenPos = Camera.main.WorldToScreenPoint(point.Key.Transform.position);
            point.Value.transform.position = screenPos;

            if (_isPanelShown && point.Key == Manager.Instance.CurrentAttachmentPoint)
            {
                var pointHeight = (point.Value.transform as RectTransform)?.sizeDelta.y ?? 30;
                var panelPos = screenPos;
                panelPos.y -= pointHeight * 0.85f;

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

    public void RegisterAttachmentToUI(WeaponAttachmentPoint point)
    {
        if (_attachmentDictionary.ContainsKey(point))
            return;

        var worldPos = point.Transform.position;
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);

        var newPoint = Instantiate(
            Manager.Instance.AttachmentPointUIPrefab,
            screenPos,
            Quaternion.identity,
            Manager.Instance.Canvas.transform
        );
        newPoint.Initialize(point);

        _attachmentDictionary.Add(point, newPoint.transform);
        newPoint.transform.SetParent(_pointsContainer);
    }

    public void UnregisterAttachmentFromUI(WeaponAttachmentPoint point)
    {
        if (!_attachmentDictionary.ContainsKey(point))
            return;

        // TODO: Pool those.
        Transform? uiPoint = _attachmentDictionary[point];
        uiPoint?.SetParent(null);

        _attachmentDictionary.Remove(point);
    }

    public void UnselectAttachment()
    {
        Events.OnAttachmentPointUnfocus.Invoke();
    }

    private void HandleAttachmentSelected(WeaponAttachmentPoint point)
    {
        ClearExistingItems();
        RefreshButtonList();

        ShowPanel(point);
    }

    private async Task HandleAttachmentUnselected()
    {
        HidePanel();

        ClearExistingItems();
    }

    private void RefreshButtonList()
    {
        ClearExistingItems();

        WeaponAttachmentPoint? point = Manager.Instance.CurrentAttachmentPoint;

        if (point == null)
            return;

        var removeButton = GetAttachmentButton();
        removeButton.ItemImage.sprite = _removeAttachmentIcon;
        removeButton.Button.onClick.AddListener(() =>
        {
            point.RemoveCurrentAttachment();
        });
        removeButton.Button.interactable = point.CurrentAttachment != null;
        removeButton.transform.SetParent(_itemsContainer);
        removeButton.transform.localScale = Vector3.one;

        _currentItems.Add(removeButton);

        foreach (var attachment in point.AvailableAttachments)
        {
            var attachmentButton = GetAttachmentButton();
            attachmentButton.ItemImage.sprite = attachment.UISprite;

            attachmentButton.Button.onClick.AddListener(() =>
            {
                point.RemoveCurrentAttachment();
                point.SetAttachment(attachment.ID);
            });

            if (point.CurrentAttachment?.ID == attachment.ID)
            {
                attachmentButton.Button.interactable = false;
            }

            attachmentButton.transform.SetParent(_itemsContainer);
            attachmentButton.transform.localScale = Vector3.one;

            _currentItems.Add(attachmentButton);
        }
    }

    public void ShowPanel(WeaponAttachmentPoint point, bool isInstant = false)
    {
        if (_isPanelShown == true)
            return;

        _isPanelShown = true;
        _itemsPanel.gameObject.SetActive(true);
    }

    private void HidePanel(bool isInstant = false)
    {
        _isPanelShown = false;
        _itemsPanel.gameObject.SetActive(false);
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

    private ItemUI GetAttachmentButton()
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
