#nullable enable

using UnityEngine;

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

    public static Manager Instance { get; private set; }

    private WeaponAttachmentPoint? _currentAttachmentPoint;

    private void Awake()
    {
        Instance = this;

        Events.OnAttachmentPointFocus.AddListener((point) => _currentAttachmentPoint = point);
        Events.OnAttachmentPointUnfocus.AddListener(() => _currentAttachmentPoint = null);
    }

    public Canvas Canvas => _uiCanvas;
    public UIController UIController => _uiController;
    public CameraController CameraController => _cameraController;

    public AttachmentPointUI AttachmentPointUIPrefab => _attachmentPointUIPrefab;

    public bool IsAttachmentSelected => _currentAttachmentPoint != null;
    public WeaponAttachmentPoint? CurrentAttachmentPoint => _currentAttachmentPoint;

    public Weapon? CurrentWeapon => _currentWeapon;
}
