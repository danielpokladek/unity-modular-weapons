#nullable enable

using PrimeTween;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform _pivot = null!;

    [SerializeField]
    Transform _weaponContainer = null!;

    [Header("Properties")]
    [SerializeField]
    Vector3 _pivotOffset;

    [SerializeField]
    float _sensitivity;

    [SerializeField]
    float _minDistance = 0.2f;

    [SerializeField]
    float _maxDistance = 0.8f;

    private InputSystem_Actions.CameraActions _actions;
    private Manager _manager;

    private Transform _target = null!;

    private Vector2 _mouseDelta;
    private Vector2 _scrollInput;

    private float _distance;

    private bool _isRotating;

    private void Start()
    {
        _target = _weaponContainer;

        _actions = Controls.InputActions.Camera;
        _manager = Manager.Instance;

        _isRotating = false;
        _distance = _maxDistance;

        _actions.Rotate.performed += ctx => _isRotating = true;
        _actions.Rotate.canceled += ctx => _isRotating = false;

        _actions.MoveDelta.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
        _actions.MoveDelta.canceled += _ => _mouseDelta = Vector2.zero;

        _actions.ZoomDelta.performed += ctx => _scrollInput = ctx.ReadValue<Vector2>();
        _actions.ZoomDelta.canceled += _ => _scrollInput = Vector2.zero;

        Events.OnAttachmentPointFocus.AddListener(
            (attachmentPoint) =>
            {
                SetTarget(attachmentPoint.transform);
            }
        );

        // Events.OnAttachmentPointUnfocus.AddListener(() => SetTarget(_weaponContainer));

        _actions.Enable();
    }

    private void LateUpdate()
    {
        if (_isRotating && _mouseDelta.sqrMagnitude > 0f)
        {
            transform.RotateAround(
                _pivot.position,
                Vector3.up,
                _mouseDelta.x * _sensitivity * Time.deltaTime
            );
            transform.RotateAround(
                _pivot.position,
                transform.right,
                -_mouseDelta.y * _sensitivity * Time.deltaTime
            );
        }

        if (_scrollInput.y != 0f)
        {
            _distance -= _scrollInput.y * 2f * Time.deltaTime;
            _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
        }

        transform.position = _pivot.position - transform.forward * _distance;
        transform.LookAt(_pivot);
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        Sequence
            .Create()
            .Group(Tween.Position(_pivot, GetPivotPosition(target.position), duration: 0.25f))
            .Group(
                Tween.Custom(
                    _distance,
                    target == _weaponContainer ? _maxDistance : _minDistance,
                    0.25f,
                    (val) =>
                    {
                        _distance = val;
                    }
                )
            );
    }

    private Vector3 GetPivotPosition(Vector3 position)
    {
        return position + _pivotOffset;
    }
}
