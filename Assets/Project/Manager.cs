#nullable enable

using System;
using UnityEngine;

[Serializable]
public class AppSettings
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

    private AttachmentPoint? _currentAttachmentPoint;

    private void Awake()
    {
        Settings = LoadSettings();

        Instance = this;

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

    public AppSettings LoadSettings()
    {
        var settings = new AppSettings
        {
            AutoZoomToAttachment = PlayerPrefs.GetInt("AutoZoomToAttachment", 1) != 0,
        };

        return settings;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("AutoZoomToAttachment", Settings.AutoZoomToAttachment ? 1 : 0);
    }
}
