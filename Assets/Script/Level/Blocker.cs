using System.Collections.Generic;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    public void SetData(BlockerData data, LevelData levelData)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();

        Vector3 startPos = new Vector3(data.StartPos.x, 0, data.StartPos.y);
        Vector3 endPos = new Vector3(data.EndPos.x, 0, data.EndPos.y);
        CreateMesh(meshFilter.sharedMesh, startPos, endPos);
    }

    private void CreateMesh(Mesh mesh, Vector3 startPos, Vector3 endPos)
    {
        int vertexCount = 8;
        Vector3[] vertices = new Vector3[vertexCount];
        List<int> triangles = new List<int>();

        float size = 0.2f;
        float height = 0.6f;

        Vector3 forward = (endPos - startPos).normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        vertices[0] = startPos + right * size;
        vertices[1] = startPos - right * size;
        vertices[2] = startPos + right * size + Vector3.up * height;
        vertices[3] = startPos - right * size + Vector3.up * height;

        vertices[4] = endPos + right * size;
        vertices[5] = endPos - right * size;
        vertices[6] = endPos + right * size + Vector3.up * height;
        vertices[7] = endPos - right * size + Vector3.up * height;

        triangles.Add(0); triangles.Add(1); triangles.Add(2);
        triangles.Add(1); triangles.Add(3); triangles.Add(2);

        triangles.Add(2); triangles.Add(3); triangles.Add(6);
        triangles.Add(3); triangles.Add(7); triangles.Add(6);

        triangles.Add(1); triangles.Add(5); triangles.Add(3);
        triangles.Add(3); triangles.Add(5); triangles.Add(7);

        triangles.Add(2); triangles.Add(6); triangles.Add(4);
        triangles.Add(0); triangles.Add(2); triangles.Add(4);

        triangles.Add(5); triangles.Add(4); triangles.Add(6);
        triangles.Add(5); triangles.Add(6); triangles.Add(7);

        mesh.triangles = null;
        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();
    }
}
