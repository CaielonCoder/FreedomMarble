using System;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public event Action GoalReached;

    private void OnTriggerEnter(Collider other)
    {
        GoalReached?.Invoke();
    }
}
