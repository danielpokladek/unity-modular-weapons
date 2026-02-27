#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttachmentPoint : MonoBehaviour
{
    [SerializeField]
    string _name = "";

    [SerializeField]
    List<WeaponAttachment> _availableAttachments = new();

    [SerializeField]
    List<AttachmentPoint> _incompatibleAttachmentPoints = new();

    [SerializeField]
    List<WeaponAttachment> _incompatibleAttachments = new();

    private Manager? _manager;

    private AttachmentPointController? _pointController;
    private HashSet<AttachmentPoint>? _incompatiblePointList;
    private HashSet<WeaponAttachment>? _incompatibleAttachmentList;

    public string Name => _name;
    public List<WeaponAttachment> AvailableAttachments => _availableAttachments;
    public WeaponAttachment? CurrentAttachment { get; private set; } = null;

    private void Start()
    {
        _manager = Manager.Instance;

        if (_manager == null)
        {
            Debug.LogError(
                "Unable to initialize attachment point, manager instance not found!",
                gameObject
            );
            return;
        }

        _pointController = _manager.UIController.AttachmentPointController;

        _incompatiblePointList = _incompatibleAttachmentPoints.ToHashSet();
        _incompatibleAttachmentList = _incompatibleAttachments.ToHashSet();

        Events.OnUpdateUI.AddListener(Refresh);
        Refresh();
    }

    private void OnDestroy()
    {
        Events.OnUpdateUI.RemoveListener(Refresh);

        RemoveAttachment();
        _pointController?.DetachAttachmentFromUI(this);
    }

    public bool SetAttachment(int id)
    {
        if (_incompatibleAttachmentList == null || _incompatiblePointList == null)
            return false;

        var attachment = _availableAttachments.Find((a) => a.ID == id);

        if (attachment == null)
        {
            Debug.LogWarning($"Tried to attach something that isn't in available list: {id}");
            return false;
        }

        var currentWeapon = Manager.Instance.CurrentWeapon;

        var attachmentIncompatible = currentWeapon.CurrentAttachments.Any(a =>
            _incompatibleAttachmentList.Contains(a)
        );

        var attachmentPointIncompatible = currentWeapon.CurrentAttachmentPoints.Any(p =>
            _incompatiblePointList.Any(p2 => p == p2 && p2.CurrentAttachment != null)
        );

        if (attachmentIncompatible || attachmentPointIncompatible)
            return false;

        RemoveAttachment(false);

        CurrentAttachment = Instantiate(attachment, transform);
        CurrentAttachment.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _pointController?.LinkAttachmentToUI(this);

        Events.OnAttachmentChanged.Invoke();

        return true;
    }

    public void RemoveAttachment(bool notify = true)
    {
        if (CurrentAttachment == null)
            return;

        CurrentAttachment.RemoveAttachment();
        Destroy(CurrentAttachment.gameObject);
        CurrentAttachment = null;

        if (notify)
        {
            Events.OnAttachmentChanged.Invoke();
        }
    }

    private void Refresh()
    {
        if (_pointController == null || _manager == null || _incompatiblePointList == null)
            return;

        var weapon = _manager.CurrentWeapon;

        var isIncompatibleWithPoint = weapon.CurrentAttachmentPoints.Any(p =>
            p.CurrentAttachment != null && _incompatiblePointList.Contains(p)
        );

        var isIncompatibleWithAttachment = weapon.CurrentAttachments.Any(a =>
            _incompatibleAttachmentList.Any(b => b.ID == a.ID)
        );

        var isLinked = _pointController.IsPointLinked(this);

        if (isIncompatibleWithPoint || isIncompatibleWithAttachment)
        {
            if (isLinked)
                _pointController.DetachAttachmentFromUI(this);

            return;
        }

        if (!isLinked)
            _pointController.LinkAttachmentToUI(this);
    }
}
