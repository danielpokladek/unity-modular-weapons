#nullable enable

using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponStats _weaponData = null!;

    [SerializeField]
    WeaponAttachment _weaponBody = null!;

    private HashSet<AttachmentPoint> _currentAttachmentPoints = new();
    private HashSet<WeaponAttachment> _currentAttachments = new();

    private void Start()
    {
        RefreshAttachmentList();
        Events.OnAttachmentChanged.AddListener(RefreshAttachmentList);
    }

    private void OnDestroy()
    {
        Events.OnAttachmentChanged.RemoveListener(RefreshAttachmentList);
    }

    public WeaponStats Stats => _weaponData;
    public HashSet<AttachmentPoint> CurrentAttachmentPoints => _currentAttachmentPoints;
    public HashSet<WeaponAttachment> CurrentAttachments => _currentAttachments;

    private void RefreshAttachmentList()
    {
        _currentAttachmentPoints = _weaponBody.FetchAttachmentSlots();
        _currentAttachments = _weaponBody.FetchEquippedAttachments();

        ClearStats();
        AddStatsModifiers(_weaponBody.StatsModifiers);

        foreach (var attachment in _currentAttachments)
        {
            AddStatsModifiers(attachment.StatsModifiers);
        }

        Events.OnUpdateUI.Invoke();
    }

    private void AddStatsModifiers(WeaponStats stats)
    {
        _weaponData.Weight += stats.Weight;
        _weaponData.Ergonomics += stats.Ergonomics;
        _weaponData.Accuracy += stats.Accuracy;
        _weaponData.SightingRange += stats.SightingRange;
        _weaponData.VerticalRecoil += stats.VerticalRecoil;
        _weaponData.HorizontalRecoil += stats.HorizontalRecoil;
        _weaponData.FireRate += stats.FireRate;
    }

    private void ClearStats()
    {
        _weaponData.Weight = 0;
        _weaponData.Ergonomics = 0;
        _weaponData.Accuracy = 0;
        _weaponData.SightingRange = 0;
        _weaponData.VerticalRecoil = 0;
        _weaponData.HorizontalRecoil = 0;
        _weaponData.FireRate = 0;
    }
}
