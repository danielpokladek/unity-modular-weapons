using UnityEngine;

public class Manager : MonoBehaviour
{
    [SerializeField]
    Canvas _uiCanvas;

    [SerializeField]
    UIController _uiController;

    [SerializeField]
    CameraController _cameraController;

    [Header("Shared Prefabs")]
    [SerializeField]
    AttachmentPointUI _attachmentPointUIPrefab;

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
