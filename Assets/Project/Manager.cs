using UnityEngine;

public class Manager : MonoBehaviour
{
    [SerializeField]
    Canvas _attachmentsCanvas;

    [SerializeField]
    UIController _attachmentPointsUI;

    public static Manager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public Canvas AttachmentCanvas => _attachmentsCanvas;
    public UIController AttachmentPointsUI => _attachmentPointsUI;
}
