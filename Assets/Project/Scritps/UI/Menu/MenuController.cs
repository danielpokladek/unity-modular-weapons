#nullable enable

using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
struct BodyButtonData
{
    public string ButtonKey;
    public WeaponAttachment BodyPrefab;

    [HideInInspector]
    public Button Button;
}

public class MenuController : MonoBehaviour
{
    [SerializeField]
    UIDocument _uiDocument = null!;

    [SerializeField]
    List<BodyButtonData> _bodyButtons = new();

    private readonly List<Action> _cleanupActions = new();

    private Manager? _manager;

    private VisualElement? _menuPanel;

    private Tween? _currentTween;

    private void OnDestroy()
    {
        foreach (var action in _cleanupActions)
            action();

        _cleanupActions.Clear();
    }

    public void Initialize()
    {
        _manager = Manager.Instance;
        var uiRoot = _uiDocument.rootVisualElement;

        var settings = _manager.Settings;

        if (uiRoot == null)
        {
            Debug.LogError("Could not find the root element of UI!", gameObject);
            return;
        }

        _menuPanel = uiRoot.Q<VisualElement>("menu-panel");

        var exitButton = uiRoot.Q<Button>("exit-button");

#if UNITY_STANDALONE
        EventCallback<ClickEvent>? exitEvent = null;

        if (Application.isEditor)
        {
            exitEvent = (_) => EditorApplication.isPlaying = false;
        }
        else
        {
            exitEvent = (_) => Application.Quit();
        }

        exitButton.RegisterCallback(exitEvent);
        _cleanupActions.Add(() => exitButton.UnregisterCallback(exitEvent));
#else
        // No need for exit button in non-standalone builds (e.g. web builds).
        exitButton.style.display = DisplayStyle.None;
#endif

        var menuButton = uiRoot.Q<Button>("menu-button");
        var presetDropdown = uiRoot.Q<DropdownField>("preset-dropdown");

        var autoCameraPan = uiRoot.Q<Toggle>("auto-camera-pan");
        autoCameraPan.value = _manager.Settings.AutoCameraPan;

        var panSensitivity = uiRoot.Q<Slider>("pan-sensitivity-slider");
        panSensitivity.lowValue = 0.1f;
        panSensitivity.highValue = 10f;
        panSensitivity.value = settings.PanSensitivity;

        var rotationSensitivity = uiRoot.Q<Slider>("rotation-sensitivity-slider");
        rotationSensitivity.lowValue = 10;
        rotationSensitivity.highValue = 100f;
        rotationSensitivity.value = settings.RotationSensitivity;

        var zoomSensitivity = uiRoot.Q<Slider>("zoom-sensitivity-slider");
        zoomSensitivity.lowValue = 1f;
        zoomSensitivity.highValue = 10f;
        zoomSensitivity.value = settings.ZoomSensitivity;

        var menuButtonCallback = (EventCallback<ClickEvent>)ToggleVisibility;

        menuButton.RegisterCallback<ClickEvent>(ToggleVisibility);
        _cleanupActions.Add(() => menuButton.UnregisterCallback(menuButtonCallback));

        foreach (var preset in _manager.WeaponPresets)
        {
            presetDropdown.choices.Add(preset.Name);
        }

        var dropdownCallback =
            (EventCallback<ChangeEvent<string>>)((e) => _manager.LoadPreset(presetDropdown.index));

        presetDropdown.RegisterValueChangedCallback(dropdownCallback);
        _cleanupActions.Add(() => presetDropdown.UnregisterValueChangedCallback(dropdownCallback));

        var autoCameraPanCallback =
            (EventCallback<ChangeEvent<bool>>)((e) => _manager.Settings.AutoCameraPan = e.newValue);

        autoCameraPan.RegisterValueChangedCallback(autoCameraPanCallback);
        _cleanupActions.Add(() =>
            autoCameraPan.UnregisterValueChangedCallback(autoCameraPanCallback)
        );

        var panSensitivityCallback =
            (EventCallback<ChangeEvent<float>>)((e) => settings.PanSensitivity = e.newValue);

        panSensitivity.RegisterValueChangedCallback(panSensitivityCallback);
        _cleanupActions.Add(() =>
            panSensitivity.UnregisterValueChangedCallback(panSensitivityCallback)
        );

        var rotationSensitivityCallback =
            (EventCallback<ChangeEvent<float>>)((e) => settings.RotationSensitivity = e.newValue);

        rotationSensitivity.RegisterValueChangedCallback(rotationSensitivityCallback);
        _cleanupActions.Add(() =>
            rotationSensitivity.UnregisterValueChangedCallback(rotationSensitivityCallback)
        );

        var zoomSensitivityCallback =
            (EventCallback<ChangeEvent<float>>)((e) => settings.ZoomSensitivity = e.newValue);

        zoomSensitivity.RegisterValueChangedCallback(zoomSensitivityCallback);
        _cleanupActions.Add(() =>
            zoomSensitivity.UnregisterValueChangedCallback(zoomSensitivityCallback)
        );

        for (int i = 0; i < _bodyButtons.Count; i++)
        {
            var data = _bodyButtons[i];
            var button = uiRoot.Q<Button>(data.ButtonKey);

            if (button == null)
            {
                Debug.LogWarning(
                    $"Could not find body button for {data.ButtonKey} - is element named and do keys match?"
                );
                continue;
            }

            var buttonCallback =
                (EventCallback<ClickEvent>)(
                    (_) =>
                    {
                        _manager.CurrentWeapon.ChangeBody(data.BodyPrefab);
                    }
                );

            data.Button = button;
            data.Button.RegisterCallback(buttonCallback);
            _cleanupActions.Add(() => data.Button.UnregisterCallback(buttonCallback));
        }
    }

    public void ToggleVisibility(ClickEvent _)
    {
        if (_menuPanel == null)
        {
            Debug.LogError("Menu button is missing from menu controller!");
            return;
        }

        if (_menuPanel.style.opacity.value > 0)
            Hide();
        else
            Show();
    }

    public void Show(bool isInstant = false)
    {
        if (_menuPanel == null || _menuPanel.style.opacity == 1)
            return;

        _currentTween?.Stop();

        var duration = isInstant ? 0 : 0.25f;

        _menuPanel.style.display = DisplayStyle.Flex;

        _currentTween = Tween.Custom(
            _menuPanel.style.opacity.value,
            1f,
            duration,
            (val) => _menuPanel.style.opacity = val
        );
    }

    public void Hide(bool isInstant = false)
    {
        if (_menuPanel == null || _menuPanel.style.opacity == 0)
            return;

        _currentTween?.Stop();

        var duration = isInstant ? 0 : 0.25f;

        _currentTween = Tween
            .Custom(
                _menuPanel.style.opacity.value,
                0f,
                duration,
                (val) => _menuPanel.style.opacity = val
            )
            .OnComplete(() =>
            {
                _menuPanel.style.display = DisplayStyle.None;
            });
    }
}
