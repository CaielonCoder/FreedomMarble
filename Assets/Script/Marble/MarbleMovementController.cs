using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarbleMovementController : MonoBehaviour
{
    public delegate void FallOutsideHandler();
    public event FallOutsideHandler FallOutside;

    public delegate void RespawnHandler();
    public event RespawnHandler Respawn;

    [SerializeField]
    private InputActionAsset _marbleInputActionAsset;

    [SerializeField]
    private float _forceMultiplier;
    [SerializeField]
    private float _torqueMultiplier;
    [SerializeField]
    private float _maxSpeed;
    [SerializeField]
    private float _changeAngleMultiplier;

    [SerializeField]
    private Transform _playerCamera;

    [SerializeField]
    private float _validPositionsGridSize = 0.666f;

    private InputAction _moveAction;
    private Rigidbody _rigidbody;
    private float _initialLinearDamping;

    private Vector3 _lastValidPosition;

    private enum State
    {
        Normal,
        FallOutside
    }
    private State state = State.Normal;

    private LayerMask levelLayerMask;

    private void Start()
    { 
        _lastValidPosition = transform.position;
        _moveAction = _marbleInputActionAsset.FindAction("Move");
        _rigidbody = GetComponent<Rigidbody>();
        _initialLinearDamping = _rigidbody.linearDamping;
        levelLayerMask = LayerMask.GetMask("Level");
        Provider.Instance.Resolve<GameStateManager>().LevelStateChanged += OnLevelStateChanged;
    }

    private void OnDestroy()
    {
        Provider.Instance.Resolve<GameStateManager>().LevelStateChanged -= OnLevelStateChanged;
    }

    void Update()
    {
        CalculateLastValidPosition();
    }

    private void FixedUpdate()
    {
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = _playerCamera.forward * moveValue.y + _playerCamera.right * moveValue.x;
        moveDirection.y = 0;
        moveDirection.Normalize();
        Vector3 clampedLinearVel = Vector3.ClampMagnitude(_rigidbody.linearVelocity, _maxSpeed);
        Vector3 changeDelta = moveDirection * _maxSpeed - clampedLinearVel;
        float angle = Vector3.Angle(changeDelta, _rigidbody.linearVelocity);

        Vector3 forceDelta = moveDirection * (1 + (angle / 360f) * _changeAngleMultiplier);
        _rigidbody.AddForce(forceDelta * _forceMultiplier);
        _rigidbody.AddTorque(Quaternion.Euler(0, 90f, 0) * forceDelta * _torqueMultiplier);
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (state != State.FallOutside && other.CompareTag("Fall"))
        {
            FallOutside();
            state = State.FallOutside;
            StartCoroutine(RespawnCoroutine());
        }
    }

    protected void CalculateLastValidPosition()
    {
        bool checkPosition = false;
        Vector3 rayOrigin = _lastValidPosition;
        if (Mathf.Abs(_lastValidPosition.x - transform.position.x) > _validPositionsGridSize)
        {
            rayOrigin += Vector3.right * _validPositionsGridSize * Mathf.Sign(transform.position.x - _lastValidPosition.x);
            checkPosition = true;
        }
        if (Mathf.Abs(_lastValidPosition.z - transform.position.z) > _validPositionsGridSize)
        {
            rayOrigin += Vector3.forward * _validPositionsGridSize * Mathf.Sign(transform.position.z - _lastValidPosition.z);
            checkPosition = true;
        }
        rayOrigin.y = transform.position.y;

        if (checkPosition && Physics.Raycast(rayOrigin, Vector3.down, 1f, levelLayerMask))
        {
            _lastValidPosition = rayOrigin;
        }
    }

    protected IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1);
        transform.position = _lastValidPosition;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        state = State.Normal;
        Respawn();
    }

    private void OnLevelStateChanged(GameStateManager.LevelState state)
    {
        if (state == GameStateManager.LevelState.Playing)
        {
            _marbleInputActionAsset.FindActionMap("Movement").Enable();
            _rigidbody.linearDamping = _initialLinearDamping;
        }
        else
        {
            _marbleInputActionAsset.FindActionMap("Movement").Disable();
            _rigidbody.linearDamping = 2;
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        Vector2 moveValue = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = _playerCamera.forward * moveValue.y + _playerCamera.right * moveValue.x;
        Vector3 clampedLinearVel = Vector3.ClampMagnitude(_rigidbody.linearVelocity, _maxSpeed);
        Vector3 changeDelta = moveDirection * _maxSpeed - clampedLinearVel;
        float angle = Vector3.Angle(changeDelta, _rigidbody.linearVelocity);

        Vector3 forceDelta = moveDirection * _forceMultiplier * (1 + (angle / 360f) * _changeAngleMultiplier);
        string boxStr = $"clamped lin vel {clampedLinearVel:0.00}\nchange delta {changeDelta:0.00}\n";
        boxStr += $"angle {angle:0.00}\nforce delta {forceDelta:0.00}";
        GUI.Box(new Rect(Screen.width - 200, 0, 200, 200), boxStr);
    }
#endif
}
