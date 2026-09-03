using UnityEngine;

public static class Vector3ExtensionMethods
{
    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
}
