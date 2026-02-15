using UnityEngine.Events;

public static class Events
{
    public static UnityEvent<WeaponAttachmentPoint> OnAttachmentPointFocus = new();
    public static UnityEvent OnAttachmentPointUnfocus = new();
}
