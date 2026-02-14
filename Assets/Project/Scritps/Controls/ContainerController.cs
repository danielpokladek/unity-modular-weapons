using UnityEngine;

public class ContainerController : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
    private float _sensitivity;

    private InputSystem_Actions.WeaponActions _weaponActions;

    private bool _isRotating;

    private void Start()
    {
        _weaponActions = Controls.InputActions.Weapon;
        _isRotating = false;

        _weaponActions.RotateButton.performed += ctx =>
        {
            _isRotating = true;
        };

        _weaponActions.RotateButton.canceled += ctx =>
        {
            _isRotating = false;
        };

        _weaponActions.Enable();
    }

    private void Update()
    {
        if (!_isRotating)
            return;

        var moveDelta = _weaponActions.RotateAxis.ReadValue<Vector2>();

        var rotation = transform.localRotation.eulerAngles;
        rotation.y -= moveDelta.x * _sensitivity * Time.deltaTime;

        transform.localRotation = Quaternion.Euler(rotation);
    }
}
