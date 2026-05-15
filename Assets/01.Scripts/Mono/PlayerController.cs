using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    PlayerInput _playerInput;
    Camera _cam;

    [SerializeField] private float _moveSpeed = 2f;
    
    private Vector3 _relativeMoveVector = Vector3.zero;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void OnEnable()
    {
        if (_playerInput == null) _playerInput = new PlayerInput();

        _playerInput.Enable();

        _playerInput.Gameplay.Move.performed += OnMove;
        _playerInput.Gameplay.Move.canceled += OnMove;
    }

    private void OnDisable()
    {
        _playerInput.Gameplay.Move.performed -= OnMove;
        _playerInput.Gameplay.Move.canceled -= OnMove;

        _playerInput.Disable();
    }

    private void Update()
    {
        transform.position += _relativeMoveVector * _moveSpeed * Time.deltaTime;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _relativeMoveVector = CalculateRelativeMoveVector(ctx.ReadValue<Vector2>());
    }

    private Vector3 CalculateRelativeMoveVector(Vector2 rawInputVector)
    {
        rawInputVector.Normalize();

        Vector3 relativeVector = _cam.transform.right * rawInputVector.x + _cam.transform.forward * rawInputVector.y;
        relativeVector.y = 0;
        relativeVector.Normalize();

        return relativeVector;
    }
}
