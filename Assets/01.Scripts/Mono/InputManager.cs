using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInput _playerInput;

    public Vector2 MoveInput { get; private set; }
    public bool BuildModeChangeInput { get; private set; }

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

        _playerInput.Gameplay.BuildModeChange.performed += OnBuildModeChange;
    }

    private void OnDisable()
    {
        _playerInput.Gameplay.Move.performed -= OnMove;
        _playerInput.Gameplay.Move.canceled -= OnMove;

        _playerInput.Gameplay.BuildModeChange.performed -= OnBuildModeChange;

        _playerInput.Disable();
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    private void OnBuildModeChange(InputAction.CallbackContext ctx)
    {
        BuildModeChangeInput = true;
    }

    public bool TryConsumeBuildModeChangeInput()
    {
        if (BuildModeChangeInput)
        {
            BuildModeChangeInput = false;
            return true;
        }
        else
        {
            return false;
        }
    }
}
