#nullable enable

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AttachmentPointController : MonoBehaviour
{
    [SerializeField]
    UIDocument _uiDocument = null!;

    private readonly Dictionary<AttachmentPoint, VisualElement> _shownPoints = new();

    private VisualElement? _uiRoot;

    public bool IsPointLinked(AttachmentPoint point) => _shownPoints.ContainsKey(point);

    private void Update()
    {
        if (_uiRoot == null)
            return;

        foreach (var (point, uiPoint) in _shownPoints)
        {
            if (uiPoint == null)
                return;

            var pointUIPos = RuntimePanelUtils.CameraTransformWorldToPanel(
                _uiRoot.panel,
                point.transform.position,
                Camera.main
            );

            uiPoint.style.translate = pointUIPos;
        }
    }

    private void OnDestroy()
    {
        foreach (var (point, _) in _shownPoints)
            DetachAttachmentFromUI(point);
    }

    public void Initialize()
    {
        _uiRoot = _uiDocument.rootVisualElement;

        if (_uiRoot == null)
        {
            Debug.LogError(
                "Unable to setup attachment point controller, the root is null!",
                gameObject
            );
            return;
        }
    }

    public void LinkAttachmentToUI(AttachmentPoint point)
    {
        if (_uiRoot == null || _shownPoints.ContainsKey(point))
            return;

        var uiPoint = GetNewUIPoint();
        uiPoint.RegisterCallback<ClickEvent>((_) => Events.OnAttachmentPointFocus.Invoke(point));

        _shownPoints.Add(point, uiPoint);
        _uiRoot.Add(uiPoint);
    }

    public void DetachAttachmentFromUI(AttachmentPoint point)
    {
        if (point == null || _uiRoot == null)
            return;

        if (!_shownPoints.ContainsKey(point))
            return;

        _shownPoints.Remove(point);

        _shownPoints.TryGetValue(point, out VisualElement uiPoint);

        if (uiPoint == null)
        {
            Debug.LogWarning(
                $"Failed to remove UI point for {point.Name} - UI element could still be in tree."
            );

            return;
        }

        uiPoint.ClearBindings();
        _uiRoot.Remove(uiPoint);
    }

    private VisualElement GetNewUIPoint()
    {
        var point = new VisualElement();
        point.AddToClassList("attachment-point");

        return point;
    }
}
