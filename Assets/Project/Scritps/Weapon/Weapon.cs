#nullable enable

using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData = null!;

    private WeaponAttachment? _weaponBody = null;

    private bool _isExploded = false;
    private bool _isMouseOverUI = false;

    private HashSet<AttachmentPoint> _currentAttachmentPoints = new();
    private HashSet<WeaponAttachment> _currentAttachments = new();

    public bool IsExploded => _isExploded;
    public WeaponData Stats => _weaponData;
    public HashSet<AttachmentPoint> CurrentAttachmentPoints => _currentAttachmentPoints;
    public HashSet<WeaponAttachment> CurrentAttachments => _currentAttachments;

    private void Start()
    {
        float inputStartTime = 0;

        Controls.InputActions.Camera.Pan.performed += _ =>
        {
            inputStartTime = Time.time;
        };

        Controls.InputActions.Camera.Pan.canceled += _ =>
        {
            var currentTime = Time.time;
            var timeDiff = currentTime - inputStartTime;

            if (timeDiff < 0.1f && !_isMouseOverUI)
            {
                Events.OnAttachmentPointUnfocus.Invoke();
            }
        };

        Controls.InputActions.Camera.ResetCamera.performed += _ =>
        {
            if (_weaponBody == null)
                return;

            _weaponBody.RemoveSubAttachments();
            _weaponBody.SetRandomAttachments();
        };

        Events.OnExplodeWeapon.AddListener((_) => _isExploded = true);
        Events.OnCompactWeapon.AddListener((_) => _isExploded = false);

        Events.OnAttachmentChanged.AddListener(HandleAttachmentChanged);
        HandleAttachmentChanged();
    }

    private void OnDestroy()
    {
        Events.OnAttachmentChanged.RemoveListener(HandleAttachmentChanged);

        Events.OnExplodeWeapon.RemoveAllListeners();
        Events.OnCompactWeapon.RemoveAllListeners();
    }

    public void LoadPreset(WeaponPreset preset)
    {
        ChangeBody(preset.Body);

        if (_weaponBody == null)
        {
            Debug.LogError("Could not load preset after changing weapon body, the body is null!");
            return;
        }

        _weaponBody.LoadAttachments(preset.AttachmentIDList);
    }

#if UNITY_EDITOR
    [ContextMenu("Print Current Attachment IDs")]
    public void PrintCurrentAttachmentIDs()
    {
        Debug.Log(string.Join(", ", _currentAttachments.Select(a => a.ID)));
    }
#endif

    public void ChangeBody(WeaponAttachment body)
    {
        _weaponBody?.RemoveAttachment();

        var newBody = Instantiate(body, transform);
        newBody.transform.localPosition = Vector3.zero;
        newBody.transform.rotation = Quaternion.identity;

        _weaponBody = newBody;

        Events.OnAttachmentChanged.Invoke();
        Events.OnBodyChanged.Invoke();
    }

    private void HandleAttachmentChanged()
    {
        if (_weaponBody == null)
            return;

        if (_isExploded)
            Events.OnCompactWeapon.Invoke(true);

        _currentAttachmentPoints = _weaponBody.FetchAttachmentSlots();
        _currentAttachments = _weaponBody.FetchEquippedAttachments();

        ClearStats();
        AddStatsModifiers(_weaponBody.Modifiers);

        foreach (var attachment in _currentAttachments)
            AddStatsModifiers(attachment.Modifiers);

        Events.OnUpdateUI.Invoke();
    }

    private void AddStatsModifiers(WeaponData stats)
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
