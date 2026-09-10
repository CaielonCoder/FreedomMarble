using System;
using System.Collections.Generic;
using TreeEditor;
using Unity.VisualScripting;
using UnityEngine;

public class Blocker : MonoBehaviour
{
    public int DataIndex { get; private set; }

    public void SetData(LevelData levelData, int dataIndex)
    {
        DataIndex = dataIndex;
        BlockerData data = levelData.Chunks[0].GetBlockerData(DataIndex);
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
        float radius = 0.11f;
        float height = 0.3f;
        int cornerFaces = 4;

        Vector3 yOffset = Vector3.up * height;

        Vector3 tempStart = points[0] + Vector3.up * (height - radius);
        Vector3 tempEnd = points[points.Count-1] + Vector3.up * (height - radius);

        Vector3 unionForward = points[1] - points[0];
        unionForward.y = 0;

        MeshUtils.AddCylinder(mesh, points[0], tempStart, radius, Vector3.up);

        Vector3 endPoint = points[points.Count - 1];
        points[0] += (points[1] - points[0]).normalized * radius;
        points[points.Count-1] += (points[points.Count-2] - points[points.Count-1]).normalized * radius;

        MeshUtils.AddCylinderUnion(mesh, tempStart, Vector3.up, points[0] + yOffset, unionForward, radius);

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 forward = points[i+1] - points[i];
            forward.y = 0;
            MeshUtils.AddCylinder(mesh, points[i] + yOffset, points[i+1] + yOffset, radius, forward);
        }

        unionForward = tempEnd - points[points.Count-1];
        unionForward.y = 0;
        MeshUtils.AddCylinderUnion(mesh, points[points.Count-1] + yOffset, unionForward, tempEnd, Vector3.down, radius);
        MeshUtils.AddCylinder(mesh, endPoint, endPoint + Vector3.up * (height - radius), radius, Vector3.up);

        mesh.RecalculateBounds();
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
            go.layer = gameObject.layer;
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
