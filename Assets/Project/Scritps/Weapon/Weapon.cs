#nullable enable

using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    [SerializeField, ReadOnly]
    WeaponData _weaponData = null!;

    [SerializeField]
    WeaponAttachment _weaponBody = null!;

    private HashSet<AttachmentPoint> _currentAttachmentPoints = new();
    private HashSet<WeaponAttachment> _currentAttachments = new();

    private bool _isExploded = false;
    private bool _isMouseOverUI = false;

    private void Start()
    {
        HandleAttachmentChanged();
        Events.OnAttachmentChanged.AddListener(HandleAttachmentChanged);

        Events.OnExplodeWeapon.AddListener((_) => _isExploded = true);
        Events.OnCompactWeapon.AddListener((_) => _isExploded = false);

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
            _weaponBody.RemoveAttachments();
            _weaponBody.SetRandomAttachments();
        };
    }

    private void OnDestroy()
    {
        Events.OnAttachmentChanged.RemoveListener(HandleAttachmentChanged);

        Events.OnExplodeWeapon.RemoveAllListeners();
        Events.OnCompactWeapon.RemoveAllListeners();
    }

    private void Update()
    {
        // A little hacky, but stops the warning in console about calling the function
        //  from within event callback.
        // TODO: Figure out a better way to check this in "new" InputSystem.
        _isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
    }

    public WeaponData Stats => _weaponData;
    public HashSet<AttachmentPoint> CurrentAttachmentPoints => _currentAttachmentPoints;
    public HashSet<WeaponAttachment> CurrentAttachments => _currentAttachments;

    public bool IsExploded => _isExploded;

    public void LoadPreset(WeaponPreset preset)
    {
        ChangeBody(preset.Body);

        _weaponBody.LoadAttachments(preset.AttachmentIDList);
    }

    [ContextMenu("Print Current Attachment IDs")]
    public void PrintCurrentAttachmentIDs()
    {
        Debug.Log(string.Join(", ", _currentAttachments.Select(a => a.ID)));
    }

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
        {
            Events.OnCompactWeapon.Invoke(true);
        }

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
