#nullable enable

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponPreset", menuName = "Modular Weapon/WeaponPreset", order = 0)]
public class WeaponPreset : ScriptableObject
{
    [SerializeField]
    string _name = "";

    [SerializeField]
    WeaponAttachment _body = null!;

    [SerializeField]
    List<int> _attachmentIDList = new();

    public string Name => _name;
    public WeaponAttachment Body => _body;
    public List<int> AttachmentIDList => _attachmentIDList;
}
