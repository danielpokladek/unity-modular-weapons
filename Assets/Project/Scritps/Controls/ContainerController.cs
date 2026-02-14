using UnityEngine;

public class ContainerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Transform _sceneCamera;

    [Header("Properties")]
    [SerializeField]
    float _sensitivity;

    [SerializeField]
    float _minZoom = 0.4f;

    [SerializeField]
    float _maxZoom = 0.8f;

    private InputSystem_Actions.WeaponActions _weaponActions;

    private bool _isRotating;

    private float _currentYaw;
    private float _currentRoll;

    private void Start()
    {
        _weaponActions = Controls.InputActions.Weapon;

        _isRotating = false;
        _currentYaw = 0f;
        _currentRoll = 0f;

        _weaponActions.RotateButton.performed += ctx =>
        {
            _isRotating = true;
        };

        _weaponActions.RotateButton.canceled += ctx =>
        {
            _isRotating = false;
        };

        _weaponActions.ZoomAxis.performed += ctx =>
        {
            var delta = ctx.ReadValue<Vector2>();
            var pos = _sceneCamera.position;

            pos.x -= delta.y * 0.01f;
            pos.x = Mathf.Clamp(pos.x, _minZoom, _maxZoom);

            _sceneCamera.position = pos;
        };

        _weaponActions.Enable();
    }

    private void Update()
    {
        if (!_isRotating)
            return;

        var moveDelta = _weaponActions.RotateAxis.ReadValue<Vector2>();

        _currentYaw -= moveDelta.x * _sensitivity * Time.deltaTime;
        _currentRoll += moveDelta.y * _sensitivity * Time.deltaTime;

        _currentRoll = Mathf.Clamp(_currentRoll, -45, 45);

        transform.localRotation = Quaternion.Euler(0f, _currentYaw, _currentRoll);
    }
}
