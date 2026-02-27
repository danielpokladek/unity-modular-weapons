#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    private AttachmentPointController? _pointController;

    private void Start()
    {
        var manager = Manager.Instance;

        if (manager == null)
        {
            Debug.LogError(
                "Unable to initialize attachment point, manager instance not found!",
                gameObject
            );
            return;
        }

        _pointController = manager.UIController.AttachmentPointController;

        IncompatibleAttachmentPoints = _incompatibleAttachmentPoints.ToHashSet();
        IncompatibleAttachments = _incompatibleAttachments.ToHashSet();

        Events.OnUpdateUI.AddListener(Refresh);
        Refresh();
    }

    private void OnDestroy()
    {
        RemoveCurrentAttachment();
        Events.OnUpdateUI.RemoveListener(Refresh);
        _pointController?.DetachAttachmentFromUI(this);
        // Manager.Instance.UIController.DetachAttachmentFromUI(this);
    }

    public WeaponAttachment? CurrentAttachment { get; private set; } = null;

    public HashSet<AttachmentPoint> IncompatibleAttachmentPoints { get; private set; } = new();
    public HashSet<WeaponAttachment> IncompatibleAttachments { get; private set; } = new();

    public string Name => _name;
    public List<WeaponAttachment> AvailableAttachments => _availableAttachments;

    public void RemoveCurrentAttachment(bool notify = true)
    {
        if (CurrentAttachment == null)
            return;

        // TODO: Pool instead of destroying.
        CurrentAttachment.RemoveAttachment();
        Destroy(CurrentAttachment.gameObject);
        CurrentAttachment = null;

        if (notify)
        {
            Events.OnAttachmentChanged.Invoke();
        }
    }

    public void Remove()
    {
        RemoveCurrentAttachment(false);
        _pointController?.DetachAttachmentFromUI(this);
        // Manager.Instance.UIController.DetachAttachmentFromUI(this);
    }

    public bool SetAttachment(int id)
    {
        var attachment = _availableAttachments.Find((a) => a.ID == id);

        if (attachment == null)
        {
            Debug.LogWarning($"Tried to attach something that isn't in available list: {id}");
            return false;
        }

        var currentWeapon = Manager.Instance.CurrentWeapon;

        var attachmentIncompatible = currentWeapon.CurrentAttachments.Any(a =>
            _incompatibleAttachments.Contains(a)
        );

        var attachmentPointIncompatible = currentWeapon.CurrentAttachmentPoints.Any(p =>
            _incompatibleAttachmentPoints.Any(p2 => p == p2 && p2.CurrentAttachment != null)
        );

        if (attachmentIncompatible || attachmentPointIncompatible)
            return false;

        RemoveCurrentAttachment(false);

        CurrentAttachment = Instantiate(attachment, transform);
        CurrentAttachment.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _pointController?.LinkAttachmentToUI(this);

        Events.OnAttachmentChanged.Invoke();

        return true;
    }

    private void Refresh()
    {
        Weapon? currentWeapon = Manager.Instance.CurrentWeapon;

        if (currentWeapon == null)
            return;

        var currentAttachmentPoints = currentWeapon.CurrentAttachmentPoints;
        var currentAttachments = currentWeapon.CurrentAttachments;

        var isIncompatibleWithPoint = currentAttachmentPoints.Where(p =>
            IncompatibleAttachmentPoints.Contains(p) && p.CurrentAttachment != null
        );

        var isIncompatibleWithAttachment = currentAttachments.Where(a =>
            IncompatibleAttachments.Any(b => a.ID == b.ID)
        );

        if (isIncompatibleWithPoint.Count() > 0 || isIncompatibleWithAttachment.Count() > 0)
        {
            _pointController?.DetachAttachmentFromUI(this);
            // Manager.Instance.UIController.DetachAttachmentFromUI(this);
        }
        else
        {
            _pointController?.LinkAttachmentToUI(this);
            // Manager.Instance.UIController.LinkAttachmentToUI(this);
        }
    }
}
