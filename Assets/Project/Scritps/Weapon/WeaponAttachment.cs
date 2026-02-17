#nullable enable

using System.Collections.Generic;
using PrimeTween;
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

    private Vector3 _explodeDirection;
    private Vector3 _originalPosition;

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

        _explodeDirection = GetAxisDirection(transform.parent.position);
        _originalPosition = transform.localPosition;

        if (CanBeRemoved)
        {
            Events.OnExplodeWeapon.AddListener(ExplodeAttachment);
            Events.OnCompactWeapon.AddListener(CompactAttachment);
        }
    }

    private void OnDestroy()
    {
        RemoveUIPoints();
        Events.OnExplodeWeapon.RemoveListener(ExplodeAttachment);
        Events.OnCompactWeapon.RemoveListener(CompactAttachment);
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
            if (point.Transform == null)
                continue;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(point.Transform.position, 0.01f);
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

        // if (absY > absX && absY > absZ)
        //     return new Vector3(0, Mathf.Sign(position.y), 0);

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
