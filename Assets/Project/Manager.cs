#nullable enable

using System;
using UnityEngine;

[Serializable]
public struct AppSettings
{
    public bool AutoZoomToAttachment;
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

    private AppSettings _settings;

    private AttachmentPoint? _currentAttachmentPoint;

    private void Awake()
    {
        _settings = LoadSettings();
        SaveSettings();

        Instance = this;

        Events.OnAttachmentPointFocus.AddListener((point) => _currentAttachmentPoint = point);
        Events.OnAttachmentPointUnfocus.AddListener(() => _currentAttachmentPoint = null);
    }

    public static Manager Instance { get; private set; }

    public AppSettings Settings => _settings;

    public Canvas Canvas => _uiCanvas;
    public UIController UIController => _uiController;
    public CameraController CameraController => _cameraController;

    public AttachmentPointUI AttachmentPointUIPrefab => _attachmentPointUIPrefab;

    public bool IsAttachmentSelected => _currentAttachmentPoint != null;
    public AttachmentPoint? CurrentAttachmentPoint => _currentAttachmentPoint;

    public Weapon? CurrentWeapon => _currentWeapon;

    private AppSettings LoadSettings()
    {
        var settings = new AppSettings
        {
            AutoZoomToAttachment = PlayerPrefs.GetInt("AutoZoomToAttachment") != 0,
        };

        return settings;
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("AutoZoomToAttachment", _settings.AutoZoomToAttachment ? 1 : 0);
    }
}
