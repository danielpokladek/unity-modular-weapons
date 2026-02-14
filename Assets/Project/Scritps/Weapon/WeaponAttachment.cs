using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [SerializeField]
    bool _canBeRemoved = true;

    [SerializeField]
    List<WeaponAttachment> _availableAttachments = new();

    [SerializeField]
    List<WeaponAttachmentPoint> _attachmentPoints = new();

    public bool CanBeRemoved => _canBeRemoved;
    public List<WeaponAttachmentPoint> AttachmentPoints => _attachmentPoints;
}
