using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInput _playerInput;

    public Vector2 MoveInput { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
    private void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }
}
