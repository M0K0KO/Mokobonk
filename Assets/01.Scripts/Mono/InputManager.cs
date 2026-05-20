using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    private PlayerInput _playerInput;

    public Vector2 MoveInput { get; private set; }
    public bool TurretBuildModeInput { get; private set; }
    public bool MortarBuildModeINput { get; private set; }
    public bool WallBuildModeInput { get; private set; }

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

        _playerInput.Gameplay.TurretBuildMode.performed += OnTurretBuildModeChangeInput;
        _playerInput.Gameplay.MortarBuildMode.performed += OnMortarBuildModeChangeInput;
        _playerInput.Gameplay.WallBuildMode.performed += OnWallBuildModeChangeInput;
    }

    private void OnDisable()
    {
        _playerInput.Gameplay.Move.performed -= OnMove;
        _playerInput.Gameplay.Move.canceled -= OnMove;

        _playerInput.Gameplay.TurretBuildMode.performed -= OnTurretBuildModeChangeInput;
        _playerInput.Gameplay.MortarBuildMode.performed -= OnMortarBuildModeChangeInput;
        _playerInput.Gameplay.WallBuildMode.performed -= OnWallBuildModeChangeInput;

        _playerInput.Disable();
    }
    private void OnMove(InputAction.CallbackContext ctx)
    {
        MoveInput = ctx.ReadValue<Vector2>();
    }

    private void OnTurretBuildModeChangeInput(InputAction.CallbackContext ctx)
    {
        TurretBuildModeInput = true;
    }

    private void OnMortarBuildModeChangeInput(InputAction.CallbackContext ctx)
    {
        MortarBuildModeINput = true;
    }

    private void OnWallBuildModeChangeInput(InputAction.CallbackContext ctx)
    {
        WallBuildModeInput = true;
    }

    public bool TryConsumeTurretBuildModeInput()
    {
        if (TurretBuildModeInput)
        {
            TurretBuildModeInput = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryConsumeMortarBuildModeInput()
    {
        if (MortarBuildModeINput)
        {
            MortarBuildModeINput = false;
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool TryConsumeWallBuildModeInput()
    {
        if (WallBuildModeInput)
        {
            WallBuildModeInput = false;
            return true;
        }
        else
        {
            return false;
        }
    }
}
