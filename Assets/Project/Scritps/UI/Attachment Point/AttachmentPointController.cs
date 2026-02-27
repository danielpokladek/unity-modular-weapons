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
            if (point == null || uiPoint == null)
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
        if (!_shownPoints.ContainsKey(point))
            return;

        var uiPoint = _shownPoints[point];

        uiPoint.ClearBindings();
        _uiRoot!.Remove(uiPoint);

        _shownPoints.Remove(point);
    }

    private VisualElement GetNewUIPoint()
    {
        var point = new VisualElement();
        point.AddToClassList("attachment-point");

        return point;
    }
}
