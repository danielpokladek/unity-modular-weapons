#nullable enable

using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponAttachment : MonoBehaviour
{
    [Header("Attachment Properties")]
    [SerializeField]
    bool _canBeRemoved = true;

    [SerializeField]
    List<AttachmentPoint> _attachmentPoints = new();

    [Header("Auto Assigned")]
    [SerializeField]
    Sprite _uiSprite = null!;

    [SerializeField]
    int _id = -1;

    private Vector3 _explodeDirection;
    private Vector3 _originalPosition;

    public List<AttachmentPoint> AttachmentPoints => _attachmentPoints;

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

        _explodeDirection = GetAxisDirection(transform.parent.position);
        _originalPosition = transform.localPosition;

        if (CanBeRemoved)
        {
            Events.OnExplodeWeapon.AddListener(ExplodeAttachment);
            Events.OnCompactWeapon.AddListener(CompactAttachment);
        }
    }

    public void HandleCleanup()
    {
        foreach (var point in _attachmentPoints)
        {
            Manager.Instance.UIController.UnregisterAttachmentFromUI(point);
            point.CurrentAttachment?.HandleCleanup();
        }

        RemoveUIPoints();

        Events.OnExplodeWeapon.RemoveListener(ExplodeAttachment);
        Events.OnCompactWeapon.RemoveListener(CompactAttachment);
    }

    private void OnDestroy()
    {
        Events.OnExplodeWeapon.RemoveListener(ExplodeAttachment);
        Events.OnCompactWeapon.RemoveListener(CompactAttachment);
    }

    public HashSet<WeaponAttachment> FetchEquippedAttachments()
    {
        HashSet<WeaponAttachment> attachments = new();

        foreach (var point in _attachmentPoints)
        {
            if (point.CurrentAttachment == null)
                continue;

            var currentAttachment = point.CurrentAttachment;

            attachments.Add(point.CurrentAttachment);
            attachments.AddRange(currentAttachment.FetchEquippedAttachments());
        }

        return attachments;
    }

    public HashSet<AttachmentPoint> FetchAttachmentSlots()
    {
        HashSet<AttachmentPoint> attachmentPoints = new();

        foreach (var point in _attachmentPoints)
        {
            attachmentPoints.Add(point);

            if (point.CurrentAttachment != null)
            {
                var a = point.CurrentAttachment.FetchAttachmentSlots();
                attachmentPoints.AddRange(a);
            }
        }

        return attachmentPoints;
    }

    public void RemoveUIPoints()
    {
        foreach (var point in _attachmentPoints)
        {
            Manager.Instance.UIController.UnregisterAttachmentFromUI(point);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var point in _attachmentPoints)
        {
            // TODO: Remove after updating from old script to new.
            if (point == null)
                continue;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.transform.position, 0.01f);
        }
    }

    private Vector3 GetAxisDirection(Vector3 position)
    {
        position = position.normalized;

        float absX = Mathf.Abs(position.x);
        float absY = Mathf.Abs(position.y);
        float absZ = Mathf.Abs(position.z);

        if (absX > absY && absX > absZ)
            return new Vector3(Mathf.Sign(position.x), 0, 0);

        return new Vector3(0, 0, Mathf.Sign(position.z));
    }

    private void ExplodeAttachment()
    {
        Tween.LocalPosition(transform, _explodeDirection * 0.3f, 0.25f);
    }

    private void CompactAttachment()
    {
        Tween.LocalPosition(transform, _originalPosition, 0.25f);
    }
}
