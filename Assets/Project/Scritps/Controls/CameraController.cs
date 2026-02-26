#nullable enable

using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

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
    float _minDistance = 0.2f;

    [SerializeField]
    float _maxDistance = 0.8f;

    private InputSystem_Actions.CameraActions _actions;

    private Manager _manager;
    private AppSettings _settings;

    private Vector2 _mouseDelta;
    private Vector2 _scrollInput;

    private float _distance;

    private bool _isRotating;
    private bool _isPanning;

    private void Start()
    {
        _actions = Controls.InputActions.Camera;

        _manager = Manager.Instance;
        _settings = _manager.Settings;

        _distance = _maxDistance;

        _isRotating = false;
        _isPanning = false;

        _actions.Pan.performed += _ => _isPanning = true;
        _actions.Pan.canceled += _ => _isPanning = false;

        _actions.Rotate.performed += _ => _isRotating = true;
        _actions.Rotate.canceled += _ => _isRotating = false;

        _actions.MoveDelta.performed += ctx => _mouseDelta = ctx.ReadValue<Vector2>();
        _actions.MoveDelta.canceled += _ => _mouseDelta = Vector2.zero;

        _actions.ZoomDelta.performed += ctx => _scrollInput = ctx.ReadValue<Vector2>();
        _actions.ZoomDelta.canceled += _ => _scrollInput = Vector2.zero;

        // _actions.ResetCamera.performed += _ => MoveTo(_weaponContainer);

        Events.OnAttachmentPointFocus.AddListener(
            (attachmentPoint) =>
            {
                if (!_settings.AutoCameraPan)
                    return;

                MoveTo(attachmentPoint.transform);
            }
        );

        _actions.Enable();
    }

    private void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HandleInputs();
        }

        transform.position = _pivot.position - transform.forward * _distance;
        transform.LookAt(_pivot);
    }

    private void HandleInputs()
    {
        bool hasMovedMouse = _mouseDelta.sqrMagnitude > 0.1f;

        if (_isPanning && hasMovedMouse)
        {
            var cameraTranslationVector =
                transform.right * -_mouseDelta.x + transform.up * -_mouseDelta.y;

            _pivot.position += _settings.PanSensitivity * Time.deltaTime * cameraTranslationVector;
        }

        if (_isRotating && hasMovedMouse)
        {
            transform.RotateAround(
                _pivot.position,
                Vector3.up,
                _mouseDelta.x * _settings.RotationSensitivity * Time.deltaTime
            );
            transform.RotateAround(
                _pivot.position,
                transform.right,
                -_mouseDelta.y * _settings.RotationSensitivity * Time.deltaTime
            );
        }

        if (_scrollInput.y != 0f)
        {
            _distance -= _scrollInput.y * _settings.ZoomSensitivity * Time.deltaTime;
            _distance = Mathf.Clamp(_distance, _minDistance, _maxDistance);
        }
    }

    private void MoveTo(Transform target)
    {
        var duration = 0.25f;

        var sequence = Sequence
            .Create()
            .Group(Tween.Position(_pivot, target.position + _pivotOffset, duration));

        if (target == _weaponContainer)
        {
            sequence.Group(
                Tween.Custom(_distance, _maxDistance, duration, (val) => _distance = val)
            );
        }
    }
}
