using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData;

    [SerializeField]
    WeaponAttachment _weaponBody;

    private void Start()
    {
        _weaponBody.SpawnInitialAttachments();
    }
}
