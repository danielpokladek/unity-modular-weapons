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

    private void Start()
    {
        foreach (var point in _attachmentPoints)
        {
            var worldPos = point.Transform.position;
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);

            var instance = Instantiate(
                Prefabs.Instance.Get(PrefabId.ATTACHMENT_POINT),
                screenPos,
                Quaternion.identity,
                Manager.Instance.AttachmentCanvas.transform
            );

            Manager.Instance.AttachmentPointsUI.RegisterAttachmentToUI(point, instance.transform);
        }
    }

    public void SpawnInitialAttachments()
    {
        foreach (var point in _attachmentPoints)
        {
            if (point.Transform == null)
                return;

            if (point.AvailableAttachments.Count == 0)
                return;

            var parent = point.Transform;
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
            if (point.Transform == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.Transform.position, 0.01f);
        }
    }
}
