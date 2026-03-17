using UnityEngine;

public class RefProvider : MonoBehaviour
{
    static RefProvider instance;

    protected void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
