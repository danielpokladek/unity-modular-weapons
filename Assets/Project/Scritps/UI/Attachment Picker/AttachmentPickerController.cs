#nullable enable

using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.UIElements;

public class AttachmentPickerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    UIDocument _uiDocument = null!;

    private readonly List<Action> _cleanupActions = new();

    private VisualElement? _root;
    private Label? _heading;
    private VisualElement? _container;

    private Tween? _currentTween;
    private AttachmentPoint? _currentPoint;

    private bool _isVisible = true;

    public void Initialize()
    {
        _root = _uiDocument.rootVisualElement;
        _heading = _root.Q<Label>("heading");
        _container = _root.Q<VisualElement>("container");
    }

    public void Show(string attachmentName, AttachmentPoint point, bool isInstant = false)
    {
        _currentPoint = point;

        if (_heading == null || _container == null || _root == null)
            return;

        if (_isVisible)
        {
            Refresh();
            return;
        }

        _currentTween?.Stop();

        Clear();

        _isVisible = true;
        _heading.text = attachmentName;

        Refresh();

        var duration = isInstant ? 0 : 0.25f;

        _currentTween = Tween.Custom(
            300,
            0,
            duration,
            (val) => _root.style.translate = new Translate(val, 0)
        );
    }

    public void Hide(bool isInstant = false)
    {
        if (_root == null)
            return;

        if (!_isVisible)
            return;

        var duration = isInstant ? 0 : 0.25f;

        _currentTween = Tween
            .Custom(0, 300, duration, (val) => _root.style.translate = new Translate(val, 0))
            .OnComplete(() =>
            {
                _isVisible = false;
                _currentPoint = null;

                Clear();
            });
    }

    public void Refresh()
    {
        if (_container == null || _currentPoint == null)
            return;

        Clear();

        WeaponAttachment? currentAttachment = _currentPoint.CurrentAttachment;
        List<WeaponAttachment> availableAttachments = _currentPoint.AvailableAttachments;

        var noneButton = GetButton("NONE", null);
        noneButton.SetEnabled(currentAttachment != null);

        var noneCallback =
            (EventCallback<ClickEvent>)((_) => _currentPoint.RemoveCurrentAttachment());
        _cleanupActions.Add(() => noneButton.UnregisterCallback(noneCallback));
        noneButton.RegisterCallback(noneCallback);

        _container.Add(noneButton);

        foreach (var attachment in availableAttachments)
        {
            var attachmentButton = GetButton(attachment.Name, attachment.UISprite);
            attachmentButton.SetEnabled(currentAttachment != attachment);

            var attachmentCallback =
                (EventCallback<ClickEvent>)((_) => _currentPoint.SetAttachment(attachment.ID));
            _cleanupActions.Add(() => attachmentButton.UnregisterCallback(noneCallback));
            attachmentButton.RegisterCallback(attachmentCallback);

            _container.Add(attachmentButton);
        }
    }

    public void Clear()
    {
        if (_container == null)
            return;

        foreach (var cleanup in _cleanupActions)
            cleanup();

        _cleanupActions.Clear();
        _container.Clear();
    }

    private Button GetButton(string text, Sprite? icon)
    {
        var button = new Button();
        button.AddToClassList("item-button");
        button.text = text;

        if (icon != null)
            button.style.backgroundImage = new StyleBackground(icon);
        else
            button.style.backgroundColor = Color.clear;

        return button;
    }
}
