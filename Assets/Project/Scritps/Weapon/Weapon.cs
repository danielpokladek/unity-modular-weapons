#nullable enable

using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData = null!;

    [SerializeField]
    WeaponAttachment _weaponBody = null!;

    private List<int> _attachedIDList = new();

    private void Start()
    {
        // _weaponBody.SpawnInitialAttachments();
    }

    public WeaponData WeaponData => _weaponData;
}
