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

    public static Manager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Canvas Canvas => _uiCanvas;
    public UIController UIController => _uiController;
    public CameraController CameraController => _cameraController;

    public AttachmentPointUI AttachmentPointUIPrefab => _attachmentPointUIPrefab;
}
