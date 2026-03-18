using UnityEngine;

public class Goal : MonoBehaviour
{
    public delegate void GoalReachedHandler();
    public event GoalReachedHandler GoalReached;

    private void OnTriggerEnter(Collider other)
    {
        GoalReached();
    }
}
