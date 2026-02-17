#nullable enable

using System;
using System.Collections.Generic;
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
public class WeaponAttachmentPoint
{
    public string Name = "";
    public Transform Transform = null!;
    public List<WeaponAttachment> AvailableAttachments = new();

    public WeaponAttachment? CurrentAttachment = null;

    public void RemoveCurrentAttachment(bool notify = true)
    {
        if (CurrentAttachment == null)
            return;

        CurrentAttachment.RemoveUIPoints();
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
