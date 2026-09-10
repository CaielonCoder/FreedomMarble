using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class MeshUtils
{
    public static void AddCylinder(Mesh mesh, Vector3 startPoint, Vector3 endPoint, float radius, Vector3 forward, int faces = 16)
    {
        Vector3 refUp = forward.z < 0 ? Vector3.up : Vector3.down;
        Vector3 refRight = Vector3.right;
        Vector3.OrthoNormalize(ref forward, ref refUp, ref refRight);

        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        List<Vector3> normals = new List<Vector3>(mesh.normals);
        Vector3 normal;
        float angle = 0;
        float angleStep = 2 * Mathf.PI / faces;

        int startIndex = mesh.vertices.Length;
        List<int> triangles = new List<int>(mesh.triangles);

        for (int i = 0; i < faces; i++)
        {
            normal = refUp * Mathf.Sin(angle) + refRight * Mathf.Cos(angle);
            vertices.Add(normal * radius + startPoint);
            vertices.Add(normal * radius + endPoint);
            normals.Add(normal);
            normals.Add(normal);
            angle += angleStep;

            triangles.Add(startIndex + i * 2);
            triangles.Add(startIndex + i * 2 + 1);
            triangles.Add(startIndex + i * 2 + 3);
            triangles.Add(startIndex + i * 2);
            triangles.Add(startIndex + i * 2 + 3);
            triangles.Add(startIndex + i * 2 + 2);
        }

        triangles[triangles.Count - 4] = startIndex + 1;
        triangles[triangles.Count - 2] = startIndex + 1;
        triangles[triangles.Count - 1] = startIndex;

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    public static void AddCylinderUnion(Mesh mesh, 
        Vector3 startPoint, Vector3 startForward, Vector3 endPoint, Vector3 endForward, 
        float radius, int subdivisions = 4, int faces = 16)
    {
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        List<Vector3> normals = new List<Vector3>(mesh.normals);
        List<int> triangles = new List<int>(mesh.triangles);
        startForward.Normalize();
        endForward.Normalize();

        Vector3 center = CalculateCircleCenter(startPoint, startForward, endPoint, endForward);
        Vector3 sleprStart = startPoint - center;
        Vector3 sleprEnd = endPoint - center;

        for (int subIndex = 0; subIndex < subdivisions; subIndex++)
        {
            float tStart = 1f * subIndex / subdivisions;
            Vector3 subPStart = Vector3.Slerp(sleprStart, sleprEnd, tStart) + center;
            Vector3 subFStart = Vector3.Slerp(startForward, endForward, tStart);

            float tEnd = (1f + subIndex) / subdivisions;
            Vector3 subPEnd = Vector3.Slerp(sleprStart, sleprEnd, tEnd) + center;
            Vector3 subFEnd = Vector3.Slerp(startForward, endForward, tEnd);

            Vector3 subUpStart = subFStart - subFEnd + Vector3.up * 0.1f;
            Vector3 subUpEnd = subUpStart;

            Vector3.OrthoNormalize(ref subFStart, ref subUpStart);
            Vector3.OrthoNormalize(ref subFEnd, ref subUpEnd);

            Vector3 subRightStart = Vector3.Cross(subFStart, subUpStart);
            Vector3 subRightEnd = Vector3.Cross(subFEnd, subUpEnd);

            Vector3 normalStart;
            Vector3 normalEnd;
            float angle = 0;
            float angleStep = 2 * Mathf.PI / faces;

            int startIndex = vertices.Count;

            for (int i = 0; i < faces; i++)
            {
                normalStart = subUpStart * Mathf.Sin(angle) + subRightStart * Mathf.Cos(angle);
                vertices.Add(normalStart * radius + subPStart);
                normals.Add(normalStart);

                normalEnd = subUpEnd * Mathf.Sin(angle) + subRightEnd * Mathf.Cos(angle);
                vertices.Add(normalEnd * radius + subPEnd);
                normals.Add(normalEnd);

                angle += angleStep;

                triangles.Add(startIndex + i * 2);
                triangles.Add(startIndex + i * 2 + 1);
                triangles.Add(startIndex + i * 2 + 3);
                triangles.Add(startIndex + i * 2);
                triangles.Add(startIndex + i * 2 + 3);
                triangles.Add(startIndex + i * 2 + 2);
            }

            triangles[triangles.Count - 4] = startIndex + 1;
            triangles[triangles.Count - 2] = startIndex + 1;
            triangles[triangles.Count - 1] = startIndex;
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    public static Vector3 CalculateCircleCenter(Vector3 p1, Vector3 t1, Vector3 p2, Vector3 t2)
    {
        Vector3 dis = p2 - p1;
        Vector3 normal = Vector3.Cross(dis, t1).normalized;
        Vector3 n1 = Vector3.Cross(normal, t1).normalized;
        Vector3 n2 = Vector3.Cross(normal, t2).normalized;
        float t = Vector3.Dot(Vector3.Cross(dis, n2), normal) / Vector3.Dot(Vector3.Cross(n1, n2), normal);
        return p1 + t * n1;
    }
}
