using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    Transform _target;

    [Header("Properties")]
    [SerializeField]
    float _distance = 3f;

    [SerializeField]
    float _sensitivity;

    [SerializeField]
    float _minZoom = 0.4f;

    [SerializeField]
    float _maxZoom = 0.8f;

    private InputSystem_Actions.WeaponActions _actions;

    private bool _isRotating;

    private Vector2 _lookInput;
    private Vector2 _scrollInput;

    private void Start()
    {
        _actions = Controls.InputActions.Weapon;

        _isRotating = false;

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
        if (!_isRotating)
            return;

        if (_isRotating && _lookInput.sqrMagnitude > 0f)
        {
            transform.RotateAround(
                _target.position,
                Vector3.up,
                _lookInput.x * _sensitivity * Time.deltaTime
            );
            transform.RotateAround(
                _target.position,
                transform.right,
                -_lookInput.y * _sensitivity * Time.deltaTime
            );
        }

        transform.LookAt(_target);

        if (_scrollInput.y != 0f)
        {
            _distance -= _scrollInput.y * 2f * Time.deltaTime;
            _distance = Mathf.Clamp(_distance, _minZoom, _maxZoom);

            transform.position = _target.position - transform.forward * _distance;
        }
    }
}
