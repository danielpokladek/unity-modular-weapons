#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum FireMode
{
    SINGLE_SHOT,
    SEMI_AUTOMATIC,
    AUTOMATIC,
}

[Serializable]
public class WeaponData
{
    public float RateOfFire;
    public float MagazineSize;
    public float Accuracy;
    public float Stability;
    public float Range;
    public float ReloadTime;
}

[Serializable]
public class WeaponAttachmentPoint : ISerializationCallbackReceiver
{
    public string Name = "";
    public Transform Transform = null!;
    public List<WeaponAttachment> AvailableAttachments = new();
    public List<WeaponAttachment> IncompatibleAttachments = new();

    public WeaponAttachment? CurrentAttachment = null;

    private HashSet<int> _incompatibleAttachmentIDs;

    public WeaponAttachmentPoint()
    {
        _incompatibleAttachmentIDs = new();
    }

    public HashSet<int> IncompatibleAttachmentIDs => _incompatibleAttachmentIDs;

    public void OnAfterDeserialize()
    {
        // Update the incompatible attachment IDs after deserialization to ensure they are in sync with the IncompatibleAttachments list.
        _incompatibleAttachmentIDs = IncompatibleAttachments.Select(a => a.ID).ToHashSet();
    }

    public void OnBeforeSerialize()
    {
        // Required by ISerializationCallbackReceiver.
    }

    public void AddIncompatibleAttachment(WeaponAttachment attachment)
    {
        if (!IncompatibleAttachments.Contains(attachment))
        {
            IncompatibleAttachments.Add(attachment);
            _incompatibleAttachmentIDs.Add(attachment.ID);
        }
    }

    public void RemoveCurrentAttachment(bool notify = true)
    {
        if (CurrentAttachment == null)
            return;

        CurrentAttachment.HandleCleanup();
        UnityEngine.Object.Destroy(CurrentAttachment.gameObject);
        CurrentAttachment = null;

        if (notify)
        {
            Events.OnAttachmentChanged.Invoke();
        }
    }

    public void SetAttachment(int id)
    {
        var attachment = AvailableAttachments.Find((a) => a.ID == id);

        if (attachment == null)
        {
            Debug.LogWarning($"Tried to attach something that isn't in available list: {id}");
            return;
        }

        RemoveCurrentAttachment(false);

        CurrentAttachment = UnityEngine.Object.Instantiate(attachment, Transform);

        CurrentAttachment.transform.localPosition = Vector3.zero;
        CurrentAttachment.transform.localRotation = Quaternion.identity;

        Events.OnAttachmentChanged.Invoke();
    }
}
