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
public class WeaponStats
{
    public float Weight;
    public float Ergonomics;
    public float Accuracy;
    public float SightingRange;
    public float VerticalRecoil;
    public float HorizontalRecoil;
    public float FireRate;
    public float EffectiveDistance;
}

[Serializable]
public class WeaponAttachmentPoint
{
    public string Name = "";
    public Transform Transform = null!;
    public List<WeaponAttachment> AvailableAttachments = new();
    public List<WeaponAttachment> IncompatibleAttachments = new();

    public WeaponAttachment? CurrentAttachment = null;

    public HashSet<int> IncompatibleAttachmentIDs { get; private set; } = new();

    public void Initialize()
    {
        IncompatibleAttachmentIDs = IncompatibleAttachments.Select(a => a.ID).ToHashSet();
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
