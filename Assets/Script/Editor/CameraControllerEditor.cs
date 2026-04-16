using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraController))]
public class CameraControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CameraController controller = (CameraController)target;
        if (GUILayout.Button("Update Camera Position"))
        {
            controller.UpdateCameraPosition();
            EditorUtility.SetDirty(target);
        }
    }
}
