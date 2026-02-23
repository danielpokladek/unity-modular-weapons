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
    float _sensitivity;

    [SerializeField]
    float _minDistance = 0.2f;

    [SerializeField]
    float _maxDistance = 0.8f;

    private InputSystem_Actions.CameraActions _actions;
    private Manager _manager;

    private Vector2 _mouseDelta;
    private Vector2 _scrollInput;

    private float _distance;

    private bool _isRotating;
    private bool _isPanning;

    private void Start()
    {
        _actions = Controls.InputActions.Camera;
        _manager = Manager.Instance;

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

        _actions.ResetCamera.performed += _ => MoveTo(_weaponContainer);

        Events.OnAttachmentPointFocus.AddListener(
            (attachmentPoint) =>
            {
                if (!_manager.Settings.AutoZoomToAttachment)
                    return;

                MoveTo(attachmentPoint.transform);
            }
        );

        _actions.Enable();
    }

    private void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        bool hasMovedMouse = _mouseDelta.sqrMagnitude > 0.1f;

        if (_isPanning && hasMovedMouse)
        {
            Vector3 pan =
                0.1f
                * Time.deltaTime
                * (transform.right * -_mouseDelta.x + transform.up * -_mouseDelta.y);

            _pivot.position += pan;
        }

        if (_isRotating && hasMovedMouse)
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

    public void MoveTo(Transform target)
    {
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
