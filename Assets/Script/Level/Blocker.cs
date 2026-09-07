using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    public void SetData(BlockerData data, LevelData levelData)
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh();

        Vector3 startPos = new Vector3(data.StartPos.x, levelData.GetHeightAt(data.StartPos), data.StartPos.y);
        Vector3 endPos = new Vector3(data.EndPos.x, levelData.GetHeightAt(data.EndPos), data.EndPos.y);

        if (Vector3.Distance(startPos, endPos) > 0.1)
        {
            List<Vector3> points = CalculatePoints(startPos, endPos, levelData);
            CreateMesh(meshFilter.sharedMesh, points);
            CreateColliders(points);
        }
    }

    private List<Vector3> CalculatePoints(Vector3 startPos, Vector3 endPos, LevelData levelData)
    {
        List<Vector3> points = new List<Vector3>();

        // Points cossing X axis
        List<float> posXs = new List<float>();
        if (startPos.x < endPos.x)
        {
            int xInt = Mathf.CeilToInt(startPos.x);
            while (xInt < endPos.x)
            {
                posXs.Add(xInt);
                xInt++;
            }
        }
        else
        {
            int xInt = Mathf.FloorToInt(startPos.x);
            while (xInt > endPos.x)
            {
                posXs.Add(xInt);
                xInt--;
            }
        }
        float startX = startPos.x;
        float endX = endPos.x;
        foreach (float posX in posXs)
        {
            Vector3 point = Vector3.Lerp(startPos, endPos, Mathf.InverseLerp(startX, endX, posX));
            point.y = levelData.GetHeightAt(point.x, point.z);
            points.Add(point);
        }

        // Points crossing Y axis
        List<float> posYs = new List<float>();
        if (startPos.z < endPos.z)
        {
            int yInt = Mathf.CeilToInt(startPos.z);
            while (yInt < endPos.z)
            {
                posYs.Add(yInt);
                yInt++;
            }
        }
        else
        {
            int yInt = Mathf.FloorToInt(startPos.z);
            while (yInt > endPos.z)
            {
                posYs.Add(yInt);
                yInt--;
            }
        }
        float startY = startPos.z;
        float endY = endPos.z;
        foreach (float posY in posYs)
        {
            Vector3 point = Vector3.Lerp(startPos, endPos, Mathf.InverseLerp(startY, endY, posY));
            point.y = levelData.GetHeightAt(point.x, point.z);
            points.Add(point);
        }

        points.Sort((Vector3 a, Vector3 b) => { return (a - startPos).sqrMagnitude <= (b - startPos).sqrMagnitude ? -1 : 1; });

        points.Insert(0, startPos);
        points.Add(endPos);

        // Remove points too close
        List<Vector3> pointsPurged = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (!Mathf.Approximately(Vector3.Distance(points[i], points[i + 1]), 0f))
                pointsPurged.Add(points[i]);
        }

        pointsPurged.Add(points[points.Count - 1]);
        return pointsPurged;
    }

    private void CreateMesh(Mesh mesh, List<Vector3> points)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        float size = 0.1f;
        float height = 0.4f;


        Vector3 forward = (points[1] - points[0]).normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);

        vertices.Add(points[0] + right * size);
        vertices.Add(points[0] - right * size);
        vertices.Add(points[0] + right * size + Vector3.up * height);
        vertices.Add(points[0] - right * size + Vector3.up * height);

        // start cap face
        triangles.Add(0); triangles.Add(1); triangles.Add(2);
        triangles.Add(1); triangles.Add(3); triangles.Add(2);

        for (int i = 1; i < points.Count; i++)
        {
            AddMeshPoint(points[i-1], points[i], vertices, triangles, size, height);
        }

        // end cap face
        int vertexIndex = vertices.Count;
        triangles.Add(vertexIndex - 3); triangles.Add(vertexIndex - 4); triangles.Add(vertexIndex - 2);
        triangles.Add(vertexIndex - 3); triangles.Add(vertexIndex - 2); triangles.Add(vertexIndex - 1);

        mesh.triangles = null;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    private void CreateColliders(List<Vector3> points)
    {
        Transform collidersRoot = transform.Find("Colliders");
        if (collidersRoot == null)
        {
            GameObject go = new GameObject("Colliders");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            collidersRoot = go.transform;
        }

        int countDiff = points.Count - 1 - collidersRoot.childCount;
        for (int i = 0; i < countDiff; i++)
        {
            GameObject go = new GameObject("Box");
            Transform colT = go.transform;
            colT.transform.parent = collidersRoot;
            go.AddComponent<BoxCollider>();
        }
        int index = 0;
        for (; index < points.Count - 1; index++)
        {
            Transform colT = collidersRoot.GetChild(index);
            Vector3 diff = points[index + 1] - points[index];
            colT.position = points[index] + Vector3.up * 0.3f;
            colT.rotation = Quaternion.LookRotation(diff);
            BoxCollider col = colT.GetComponent<BoxCollider>();
            col.size = new Vector3(0.2f, 0.2f, diff.magnitude);
            col.center = new Vector3(0f, 0f, diff.magnitude * 0.5f);
        }
        while (index < collidersRoot.childCount)
        {
            DestroyImmediate(collidersRoot.GetChild(index).gameObject);
            index++;
        }
    }

    private void AddMeshPoint(Vector3 prevPoint, Vector3 point, List<Vector3> vertices, List<int> triangles, float size, float height)
    {
        Vector3 forward = (point - prevPoint).normalized;
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        right.Normalize();

        int vertexIndex = vertices.Count;

        vertices.Add(point + right * size);
        vertices.Add(point - right * size);
        vertices.Add(point + right * size + Vector3.up * height);
        vertices.Add(point - right * size + Vector3.up * height);

        triangles.Add(vertexIndex - 2); triangles.Add(vertexIndex - 1); triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex - 1); triangles.Add(vertexIndex + 3); triangles.Add(vertexIndex + 2);

        triangles.Add(vertexIndex - 3); triangles.Add(vertexIndex + 1); triangles.Add(vertexIndex - 1);
        triangles.Add(vertexIndex - 1); triangles.Add(vertexIndex + 1); triangles.Add(vertexIndex + 3);

        triangles.Add(vertexIndex - 2); triangles.Add(vertexIndex + 2); triangles.Add(vertexIndex);
        triangles.Add(vertexIndex - 4); triangles.Add(vertexIndex - 2); triangles.Add(vertexIndex);
    }
}
