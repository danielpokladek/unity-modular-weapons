#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    RectTransform _pointsContainer;

    [Header("References")]
    [SerializeField]
    MenuController _menuPanel;

    [SerializeField]
    StatsPanelController _statsPanel;

    [SerializeField]
    AttachmentPickerController _attachmentPicker;

    [SerializeField]
    CanvasGroup _pointsCanvasGroup;

    private Dictionary<AttachmentPoint, AttachmentPointUI> _attachmentDictionary = new();

    private Queue<AttachmentPointUI> _attachmentPointPool = new();

    private Tween? _uiTween;

    private void Update()
    {
        foreach (var point in _attachmentDictionary)
        {
            if (point.Value == null)
                return;

            var screenPos = Camera.main.WorldToScreenPoint(point.Key.transform.position);
            point.Value.transform.position = screenPos;
        }
    }

    public StatsPanelController StatsPanel => _statsPanel;

    public void Initialize()
    {
        Events.OnAttachmentPointFocus.AddListener(HandleAttachmentSelected);
        Events.OnAttachmentPointUnfocus.AddListener(() => _ = HandleAttachmentUnselected());
        Events.OnAttachmentChanged.AddListener(() => _attachmentPicker.Refresh());
        Events.OnBodyChanged.AddListener(() =>
        {
            _menuPanel.Hide();
            _attachmentPicker.Hide();
        });

        Events.OnUpdateUI.AddListener(() =>
        {
            if (Manager.Instance.CurrentWeapon == null)
                return;

            _statsPanel.UpdateStats(Manager.Instance.CurrentWeapon.Stats);
        });

        Controls.InputActions.UI.ToggleUI.performed += _ => ToggleUI();

        _menuPanel.Initialize();
        _attachmentPicker.Initialize();

        _statsPanel.ToggleStatsPanel();
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

    public void LinkAttachmentToUI(AttachmentPoint point)
    {
        if (_attachmentDictionary.ContainsKey(point))
            return;

        var worldPos = point.transform.position;
        var screenPos = Camera.main.WorldToScreenPoint(worldPos);

        var newPoint =
            _attachmentPointPool.Count > 0
                ? _attachmentPointPool.Dequeue()
                : Instantiate(Manager.Instance.AttachmentPointUIPrefab);

        newPoint.transform.SetPositionAndRotation(screenPos, Quaternion.identity);
        newPoint.transform.SetParent(Manager.Instance.Canvas.transform);
        newPoint.Initialize(point);

        _attachmentDictionary.Add(point, newPoint);
        newPoint.transform.SetParent(_pointsContainer);
    }

    public void DetachAttachmentFromUI(AttachmentPoint point)
    {
        if (point == null)
            return;

        if (!_attachmentDictionary.ContainsKey(point))
            return;

        if (_attachmentDictionary.TryGetValue(point, out AttachmentPointUI pointUI))
        {
            pointUI.transform.SetParent(null);
            _attachmentPointPool.Enqueue(pointUI);
        }

        _attachmentDictionary.Remove(point);
    }

    private void HandleAttachmentSelected(AttachmentPoint point)
    {
        var heading = string.IsNullOrEmpty(point.Name) ? " " : point.Name;
        _attachmentPicker.Show(heading, point);
    }

    private async Task HandleAttachmentUnselected()
    {
        _attachmentPicker.Hide();
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
