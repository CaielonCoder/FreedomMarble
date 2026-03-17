using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float distance;
    [SerializeField]
    private float angle;
    [SerializeField]
    private Vector3 direction;

    [SerializeField]
    private MarbleMovementController marbleController;

    private enum State
    {
        Following,
        Static
    }
    private State state;

    void Start()
    {
        marbleController.FallOutside += OnMarbleFallOutside;
        marbleController.Respawn += OnMarbleRespawn;

        direction.Normalize();
        direction.y = Mathf.Sin(Mathf.Deg2Rad * angle);
        direction.Normalize();
    }

    void Update()
    {
#if UNITY_EDITOR
        direction.Normalize();
        direction.y = Mathf.Sin(Mathf.Deg2Rad * angle);
        direction.Normalize();
#endif
        if (state == State.Following)
        {
            Vector3 moveDirection = direction;
            moveDirection.y = 0;
            moveDirection.Normalize();
            Vector3 position = Vector3.Dot(marbleController.transform.position, moveDirection) * moveDirection;
            position.y = marbleController.transform.position.y;
            transform.position = position + direction * distance;
            transform.forward = -direction;
        }
    }

    protected void OnMarbleFallOutside()
    {
        state = State.Static;
    }

    protected void OnMarbleRespawn()
    {
        state = State.Following;
    }
}
