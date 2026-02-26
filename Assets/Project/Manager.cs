#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class AppSettings
{
    private bool _autoCameraPan = false;
    private float _panSensitivity = 0.1f;
    private float _rotationSensitivity = 0.1f;
    private float _scrollSensitivity = 1f;

    public UnityEvent OnSettingChanged = new();

    public bool AutoCameraPan
    {
        get => _autoCameraPan;
        set
        {
            _autoCameraPan = value;
            OnSettingChanged.Invoke();
        }
    }

    public float PanSensitivity
    {
        get => _panSensitivity;
        set
        {
            _panSensitivity = value;
            OnSettingChanged.Invoke();
        }
    }

    public float RotationSensitivity
    {
        get => _rotationSensitivity;
        set
        {
            _rotationSensitivity = value;
            OnSettingChanged.Invoke();
        }
    }

    public float ZoomSensitivity
    {
        get => _scrollSensitivity;
        set
        {
            _scrollSensitivity = value;
            OnSettingChanged.Invoke();
        }
    }
}

public class Manager : MonoBehaviour
{
    [SerializeField]
    Canvas _uiCanvas = null!;

    [SerializeField]
    UIController _uiController = null!;

    [SerializeField]
    CameraController _cameraController = null!;

    [Header("Shared Prefabs")]
    [SerializeField]
    AttachmentPointUI _attachmentPointUIPrefab = null!;

    [SerializeField]
    Weapon _currentWeapon = null!;

    [SerializeField]
    List<WeaponPreset> _weaponPresets = new();

    private AttachmentPoint? _currentAttachmentPoint;

    private void Awake()
    {
        Instance = this;

        Settings = LoadSettings();
        Settings.OnSettingChanged.AddListener(SaveSettings);

        Events.OnAttachmentPointFocus.AddListener((point) => _currentAttachmentPoint = point);
        Events.OnAttachmentPointUnfocus.AddListener(() => _currentAttachmentPoint = null);

        _uiController.Initialize();
    }

    public static Manager Instance { get; private set; }

    public AppSettings Settings { get; private set; }

    public Canvas Canvas => _uiCanvas;
    public UIController UIController => _uiController;
    public CameraController CameraController => _cameraController;

    public AttachmentPointUI AttachmentPointUIPrefab => _attachmentPointUIPrefab;

    public bool IsAttachmentSelected => _currentAttachmentPoint != null;
    public AttachmentPoint? CurrentAttachmentPoint => _currentAttachmentPoint;

    public Weapon CurrentWeapon => _currentWeapon;
    public List<WeaponPreset> WeaponPresets => _weaponPresets;

    [ContextMenu("Load Random Preset")]
    public void LoadRandomPreset()
    {
        var index = UnityEngine.Random.Range(0, _weaponPresets.Count);
        LoadPreset(index);
    }

    public void LoadPreset(int index)
    {
        if (index < 0 || index > _weaponPresets.Count - 1)
        {
            Debug.Log(
                $"Tried to load weapon preset from index list, but index {index} is invalid!"
            );
            return;
        }
        _currentWeapon.LoadPreset(_weaponPresets[index]);
    }

    public AppSettings LoadSettings()
    {
        return new AppSettings
        {
            AutoCameraPan = PlayerPrefs.GetInt("AutoCameraPan", 1) != 0,
            PanSensitivity = PlayerPrefs.GetFloat("PanSensitivity", 0.1f),
            RotationSensitivity = PlayerPrefs.GetFloat("RotationSensitivity", 0.1f),
        };
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("AutoCameraPan", Settings.AutoCameraPan ? 1 : 0);
        PlayerPrefs.SetFloat("PanSensitivity", Settings.PanSensitivity);
        PlayerPrefs.SetFloat("RotationSensitivity", Settings.RotationSensitivity);

        print(Settings.RotationSensitivity);
    }
}
