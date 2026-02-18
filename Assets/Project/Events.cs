using UnityEngine.Events;

public static class Events
{
    public static UnityEvent<AttachmentPoint> OnAttachmentPointFocus = new();
    public static UnityEvent OnAttachmentPointUnfocus = new();

    public static UnityEvent OnExplodeWeapon = new();
    public static UnityEvent OnCompactWeapon = new();

    public static UnityEvent OnAttachmentChanged = new();

    public static UnityEvent OnUpdateUI = new();
}
