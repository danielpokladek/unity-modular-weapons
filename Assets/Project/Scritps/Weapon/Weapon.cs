#nullable enable

using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData = null!;

    [SerializeField]
    WeaponAttachment _weaponBody = null!;

    private HashSet<AttachmentPoint> _currentAttachmentPoints = new();
    private HashSet<WeaponAttachment> _currentAttachments = new();

    private void Start()
    {
        RefreshAttachmentList();
        Events.OnAttachmentChanged.AddListener(RefreshAttachmentList);
    }

    private void OnDestroy()
    {
        Events.OnAttachmentChanged.RemoveListener(RefreshAttachmentList);
    }

    public WeaponData WeaponData => _weaponData;
    public HashSet<AttachmentPoint> CurrentAttachmentPoints => _currentAttachmentPoints;
    public HashSet<WeaponAttachment> CurrentAttachments => _currentAttachments;

    private void RefreshAttachmentList()
    {
        _currentAttachmentPoints = _weaponBody.FetchAttachmentSlots();
        _currentAttachments = _weaponBody.FetchEquippedAttachments();

        Events.OnUpdateUI.Invoke();
    }
}
