using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 15f;

    [SerializeField] private InputManager _inputManager;

    private Vector3 _panForward;
    private Vector3 _panRight;

    private void Awake()
    {
        _panForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _panRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
    }

    private void Start()
    {
        _inputManager = InputManager.Instance;
    }

    private void Update()
    {
        Vector3 move = (_panForward * _inputManager.MoveInput.y + _panRight * _inputManager.MoveInput.x);
        if (move.sqrMagnitude > 1f) move.Normalize();

        transform.position += move * (panSpeed * Time.deltaTime);
    }
}
