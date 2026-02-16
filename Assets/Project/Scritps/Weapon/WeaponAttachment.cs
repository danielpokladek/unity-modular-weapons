#nullable enable

using System.Collections.Generic;
using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [Header("Attachment Properties")]
    [SerializeField]
    bool _canBeRemoved = true;

    [SerializeField]
    List<WeaponAttachmentPoint> _attachmentPoints = new();

    [Header("Auto Assigned")]
    [SerializeField]
    Sprite _uiSprite = null!;

    [SerializeField]
    int _id = -1;

    public List<WeaponAttachmentPoint> AttachmentPoints => _attachmentPoints;

    public Sprite UISprite => _uiSprite;

    public bool CanBeRemoved => _canBeRemoved;

    public int ID => _id;

    private void Start()
    {
        if (_id == -1)
        {
            Debug.LogError($"No ID has been assigned for {gameObject.name}!");
            return;
        }

        foreach (var point in _attachmentPoints)
        {
            var worldPos = point.Transform.position;
            var screenPos = Camera.main.WorldToScreenPoint(worldPos);

            var instance = Instantiate(
                Manager.Instance.AttachmentPointUIPrefab,
                screenPos,
                Quaternion.identity,
                Manager.Instance.Canvas.transform
            );
            instance.Initialize(point);

            Manager.Instance.UIController.RegisterAttachmentToUI(point, instance.transform);
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

            point.CurrentAttachment = instance;
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
