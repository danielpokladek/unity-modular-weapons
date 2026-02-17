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

    [SerializeField]
    HashSet<int> _currentAttachmentIDList = new();

    private void Start()
    {
        // _weaponBody.SpawnInitialAttachments();

        RefreshAttachmentList();
        Events.OnAttachmentChanged.AddListener(RefreshAttachmentList);
    }

    private void OnDestroy()
    {
        Events.OnAttachmentChanged.RemoveListener(RefreshAttachmentList);
    }

    public WeaponData WeaponData => _weaponData;
    public HashSet<int> CurrentAttachmentIDList => _currentAttachmentIDList;

    private void RefreshAttachmentList()
    {
        _currentAttachmentIDList = _weaponBody.GetCurrentAttachmentIDList();
    }
}
