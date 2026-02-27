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
    string _name = "";

    [SerializeField]
    bool _canBeRemoved = true;

    [SerializeField]
    List<AttachmentPoint> _attachmentPoints = new();

    [SerializeField]
    WeaponData _statsModifiers = null!;

    [Header("Auto Assigned")]
    [SerializeField]
    Sprite _uiSprite = null!;

    [SerializeField]
    int _id = -1;

    private Vector3 _explodeDirection;
    private Vector3 _originalPosition;

    public string Name => _name;

    public List<AttachmentPoint> AttachmentPoints => _attachmentPoints;

    public WeaponData StatsModifiers => _statsModifiers;

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

        // TODO: In actual project this would be removed and used by stats from inspector.
        GenerateRandomWeaponStats();

        _explodeDirection = GetAxisDirection(transform.position);
        _originalPosition = transform.localPosition;

        if (CanBeRemoved)
        {
            Events.OnExplodeWeapon.AddListener(ExplodeAttachment);
            Events.OnCompactWeapon.AddListener(CompactAttachment);
        }
    }

    public void HandleCleanup() { }

    private void OnDestroy()
    {
        Events.OnExplodeWeapon.RemoveListener(ExplodeAttachment);
        Events.OnCompactWeapon.RemoveListener(CompactAttachment);
    }

    public void LoadAttachments(List<int> preset)
    {
        foreach (var point in _attachmentPoints)
        {
            int i = 0;

            foreach (var attachment in point.AvailableAttachments)
            {
                if (preset.Contains(attachment.ID))
                {
                    point.SetAttachment(attachment.ID);
                }

                i++;
            }

            if (point.CurrentAttachment != null)
            {
                point.CurrentAttachment.LoadAttachments(preset);
            }
        }
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

    public void RemoveAttachment()
    {
        RemoveAttachments();

        Events.OnExplodeWeapon.RemoveListener(ExplodeAttachment);
        Events.OnCompactWeapon.RemoveListener(CompactAttachment);

        Destroy(gameObject);
    }

    public void RemoveAttachments()
    {
        foreach (var point in _attachmentPoints)
        {
            point.RemoveCurrentAttachment();
        }
    }

    public void SetRandomAttachments()
    {
        foreach (var point in _attachmentPoints)
        {
            int attempt = 0;
            int maxAttempts = _attachmentPoints.Count;

            while (point.CurrentAttachment == null && attempt < maxAttempts)
            {
                var randomAttachmentIndex = Random.Range(0, point.AvailableAttachments.Count);
                var randomAttachment = point.AvailableAttachments[randomAttachmentIndex];

                point.SetAttachment(randomAttachment.ID);
                point.CurrentAttachment?.SetRandomAttachments();

                attempt++;
            }
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
        var dir = position - new Vector3(0, 0.06f, 0.05f);

        if (dir == Vector3.zero)
            return Vector3.zero;

        dir.Normalize();

        float horiz = Mathf.Sqrt(dir.x * dir.x + dir.z * dir.z);
        float azDeg = Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg;
        float elDeg = Mathf.Atan2(dir.y, horiz) * Mathf.Rad2Deg;

        float roundedAz = Mathf.Round(azDeg / 45f) * 45f;
        float roundedEl = Mathf.Round(elDeg / 45f) * 45f;

        float azRad = roundedAz * Mathf.Deg2Rad;
        float elRad = roundedEl * Mathf.Deg2Rad;
        float cosEl = Mathf.Cos(elRad);

        var result = new Vector3(
            cosEl * Mathf.Cos(azRad),
            Mathf.Sin(elRad),
            cosEl * Mathf.Sin(azRad)
        );

        return result.normalized;
    }

    private void ExplodeAttachment(bool isInstant)
    {
        Tween.LocalPosition(transform, _explodeDirection * 0.1f, isInstant ? 0 : 0.25f);
    }

    private void CompactAttachment(bool isInstant)
    {
        Tween.LocalPosition(transform, _originalPosition, isInstant ? 0 : 0.25f);
    }

    private void GenerateRandomWeaponStats()
    {
        _statsModifiers.Weight = Random.Range(0.2f, 0.4f);
        _statsModifiers.Accuracy = Random.Range(0, 350);
        _statsModifiers.SightingRange = Random.Range(0, 80);
        _statsModifiers.VerticalRecoil = Random.Range(0, 150);
        _statsModifiers.HorizontalRecoil = Random.Range(0, 350);
        _statsModifiers.FireRate = Random.Range(0, 5);
    }
}
