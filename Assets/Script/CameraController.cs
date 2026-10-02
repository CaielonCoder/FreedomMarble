using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _distance;
    [SerializeField]
    private float _angle;
    [SerializeField]
    private Vector3 _direction;

    [SerializeField]
    private MarbleMovementController _marbleController;

    private enum State
    {
        Following,
        Static
    }
    private State state;

    void Start()
    {
        _marbleController.FallOutside += OnMarbleFallOutside;
        _marbleController.Respawn += OnMarbleRespawn;

        _direction.Normalize();
        _direction.y = Mathf.Sin(Mathf.Deg2Rad * _angle);
        _direction.Normalize();
    }

    void Update()
    {
        if (state == State.Following)
        {
            UpdateCameraPosition();
        }
    }

    public void UpdateCameraPosition()
    {
#if UNITY_EDITOR
        _direction.Normalize();
        _direction.y = Mathf.Sin(Mathf.Deg2Rad * _angle);
        _direction.Normalize();
#endif
        transform.position = _marbleController.transform.position + _direction * _distance;
        transform.forward = -_direction;
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
