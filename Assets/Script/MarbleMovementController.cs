using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class MarbelMovementController : MonoBehaviour
{
    public delegate void FallOutsideHandler();
    public event FallOutsideHandler FallOutside;

    public delegate void RespawnHandler();
    public event RespawnHandler Respawn;

    [SerializeField]
    private InputActionAsset marbleInputActionAsset;

    [SerializeField]
    private float moveForce;

    [SerializeField]
    private Transform playerCamera;

    [SerializeField]
    private float validPositionsGridSize = 0.666f;

    private InputAction moveAction;
    private new Rigidbody rigidbody;

    private Vector3 lastValidPosition;

    private enum State
    {
        Normal,
        FallOutside
    }
    private State state = State.Normal;

    private LayerMask levelLayerMask;

    void Start()
    { 
        marbleInputActionAsset.FindActionMap("Movement").Enable();
        lastValidPosition = transform.position;
        moveAction = marbleInputActionAsset.FindAction("Move");
        rigidbody = GetComponent<Rigidbody>();
        levelLayerMask = LayerMask.GetMask("Level");
    }

    void Update()
    {
        CalculateLastValidPosition();
    }

    private void FixedUpdate()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        Vector3 forceDirection = playerCamera.forward * moveValue.y + playerCamera.right * moveValue.x;
        rigidbody.AddForce(forceDirection * moveForce);
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
        Vector3 rayOrigin = lastValidPosition;
        if (Mathf.Abs(lastValidPosition.x - transform.position.x) > validPositionsGridSize)
        {
            rayOrigin += Vector3.right * validPositionsGridSize * Mathf.Sign(transform.position.x - lastValidPosition.x);
            checkPosition = true;
        }
        if (Mathf.Abs(lastValidPosition.z - transform.position.z) > validPositionsGridSize)
        {
            rayOrigin += Vector3.forward * validPositionsGridSize * Mathf.Sign(transform.position.z - lastValidPosition.z);
            checkPosition = true;
        }
        rayOrigin.y = transform.position.y;

        if (checkPosition && Physics.Raycast(rayOrigin, Vector3.down, 1f, levelLayerMask))
        {
            lastValidPosition = rayOrigin;
        }
    }

    protected IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1);
        transform.position = lastValidPosition;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        state = State.Normal;
        Respawn();
    }
}
