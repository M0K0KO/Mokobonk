using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    PlayerInput _playerInput;
    Camera _cam;

    [SerializeField] private float _moveSpeed = 2f;

    private Vector2 _moveInput;

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
        Move();
    }
    private void Move()
    {
        Vector3 camForward = _cam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = _cam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 moveDir = camRight * _moveInput.x + camForward * _moveInput.y;

        moveDir.Normalize();

        transform.position += moveDir * _moveSpeed * Time.deltaTime;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        _moveInput = ctx.ReadValue<Vector2>();
    }
}
