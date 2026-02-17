using UnityEngine.Events;

public static class Events
{
    public static UnityEvent<WeaponAttachmentPoint> OnAttachmentPointFocus = new();
    public static UnityEvent OnAttachmentPointUnfocus = new();

    public static UnityEvent OnExplodeWeapon = new();
    public static UnityEvent OnCompactWeapon = new();
}
