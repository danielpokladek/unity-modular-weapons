using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [SerializeField]
    bool _canBeRemoved = true;

    [SerializeField]
    List<WeaponAttachmentPoint> _attachmentPoints = new();

    public bool CanBeRemoved => _canBeRemoved;
    public List<WeaponAttachmentPoint> AttachmentPoints => _attachmentPoints;

    public void SpawnInitialAttachments()
    {
        foreach (var point in _attachmentPoints)
        {
            if (point.AttachmentPosition == null)
                return;

            if (point.AvailableAttachments.Count == 0)
                return;

            var parent = point.AttachmentPosition;
            var position = parent.position;
            var element = point.AvailableAttachments[0];

            var instance = Instantiate(element, position, Quaternion.identity, parent);
            instance.SpawnInitialAttachments();

            point.CurrentAttachment = instance.gameObject;
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var point in _attachmentPoints)
        {
            if (point.AttachmentPosition == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.AttachmentPosition.position, 0.01f);
        }
    }
}
