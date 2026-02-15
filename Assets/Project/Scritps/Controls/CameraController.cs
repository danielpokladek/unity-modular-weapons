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
    Vector3 _cameraOffset;

    [SerializeField]
    float _sensitivity;

    [SerializeField]
    float _minDistance = 0.2f;

    [SerializeField]
    float _maxDistance = 0.8f;

    private InputSystem_Actions.WeaponActions _actions;

    private Transform _target = null!;

    private Vector2 _lookInput;
    private Vector2 _scrollInput;

    private float _distance;

    private bool _isRotating;

    private void Start()
    {
        _target = _weaponContainer;

        _actions = Controls.InputActions.Weapon;

        _isRotating = false;
        _distance = _maxDistance;

        _actions.RotateButton.performed += ctx => _isRotating = true;
        _actions.RotateButton.canceled += ctx => _isRotating = false;

        _actions.RotateAxis.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _actions.RotateAxis.canceled += _ => _lookInput = Vector2.zero;

        _actions.ZoomAxis.performed += ctx => _scrollInput = ctx.ReadValue<Vector2>();
        _actions.ZoomAxis.canceled += _ => _scrollInput = Vector2.zero;

        _actions.Enable();
    }

    private void LateUpdate()
    {
        if (_isRotating && _lookInput.sqrMagnitude > 0f)
        {
            transform.RotateAround(
                _pivot.position,
                Vector3.up,
                _lookInput.x * _sensitivity * Time.deltaTime
            );
            transform.RotateAround(
                _pivot.position,
                transform.right,
                -_lookInput.y * _sensitivity * Time.deltaTime
            );
        }

        transform.position = _pivot.position - transform.forward * _distance;
        transform.LookAt(_pivot);

        if (_scrollInput.y != 0f)
        {
            _distance -= _scrollInput.y * 2f * Time.deltaTime;
            _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);

            transform.position = _target.position - transform.forward * _distance;
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;

        Tween.Position(_pivot, _target.position, duration: 0.25f);
    }
}
