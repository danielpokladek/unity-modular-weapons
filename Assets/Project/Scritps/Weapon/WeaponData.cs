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
    public string Name;
    public Transform AttachmentPosition;
    public List<WeaponAttachment> AvailableAttachments = new();
    public WeaponAttachment CurrentAttachment;
}
