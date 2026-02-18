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

    public WeaponAttachment? CurrentAttachment { get; private set; } = null;

    public HashSet<AttachmentPoint> IncompatibleAttachmentPoints { get; private set; } = new();
    public HashSet<WeaponAttachment> IncompatibleAttachments { get; private set; } = new();

    public string Name => _name;
    public List<WeaponAttachment> AvailableAttachments => _availableAttachments;

    private void Start()
    {
        IncompatibleAttachmentPoints = _incompatibleAttachmentPoints.ToHashSet();
        IncompatibleAttachments = _incompatibleAttachments.ToHashSet();

        Events.OnUpdateUI.AddListener(Refresh);
    }

    private void OnDestroy()
    {
        RemoveCurrentAttachment();
        Events.OnUpdateUI.RemoveListener(Refresh);
        Manager.Instance.UIController.UnregisterAttachmentFromUI(this);
    }

    public void RemoveCurrentAttachment(bool notify = true)
    {
        if (CurrentAttachment == null)
            return;

        // TODO: Pool instead of destroying.
        CurrentAttachment.HandleCleanup();
        Destroy(CurrentAttachment.gameObject);
        CurrentAttachment = null;

        if (notify)
        {
            Events.OnAttachmentChanged.Invoke();
        }
    }

    public void SetAttachment(int id)
    {
        var attachment = _availableAttachments.Find((a) => a.ID == id);

        if (attachment == null)
        {
            Debug.LogWarning($"Tried to attach something that isn't in available list: {id}");
            return;
        }

        RemoveCurrentAttachment(false);

        CurrentAttachment = Instantiate(attachment, transform);

        CurrentAttachment.transform.localPosition = Vector3.zero;
        CurrentAttachment.transform.localRotation = Quaternion.identity;

        Events.OnAttachmentChanged.Invoke();
    }

    private void Refresh()
    {
        Weapon? currentWeapon = Manager.Instance.CurrentWeapon;

        if (currentWeapon == null)
            return;

        var currentAttachmentPoints = currentWeapon.CurrentAttachmentPoints;
        var currentAttachments = currentWeapon.CurrentAttachments;

        var isIncompatibleWithPoint = currentAttachmentPoints.Any(p =>
            IncompatibleAttachmentPoints.Any(p => p)
        );

        var isIncompatibleWithAttachment = currentAttachments.Any(a =>
            IncompatibleAttachments.Any(b => a.ID == b.ID)
        );

        if (isIncompatibleWithPoint || isIncompatibleWithAttachment)
        {
            Manager.Instance.UIController.UnregisterAttachmentFromUI(this);
        }
        else
        {
            Manager.Instance.UIController.RegisterAttachmentToUI(this);
        }
    }
}
