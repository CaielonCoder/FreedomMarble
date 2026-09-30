using System.Collections.Generic;
using UnityEngine;

public class LevelMeshUtils
{
    public static void AddFloorMesh(Mesh mesh, LevelData data, int startX = 0, int startY = 0, int endX = int.MaxValue, int endY = int.MaxValue)
    {
        ChunkData chunk = data.Chunks[0];
        List<Vector3> vertices = new List<Vector3>(mesh.vertices);
        List<Vector3> normals = new List<Vector3>(mesh.normals);
        List<int> triangles = new List<int>(mesh.triangles);

        if (endX > chunk.SizeX) endX = chunk.SizeX;
        if (endY > chunk.SizeY) endY = chunk.SizeY;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (!chunk.GetTile(x, y).active) continue;

                int vertexIndex = vertices.Count;

                vertices.Add(new Vector3(x, chunk.GetTile(x, y).vertexY[0] * LevelData.STEP_Y, y));
                normals.Add(Vector3.up);

                vertices.Add(new Vector3(x + 1, chunk.GetTile(x, y).vertexY[1] * LevelData.STEP_Y, y));
                normals.Add(Vector3.up);

                vertices.Add(new Vector3(x + 1, chunk.GetTile(x, y).vertexY[2] * LevelData.STEP_Y, y + 1));
                normals.Add(Vector3.up);

                vertices.Add(new Vector3(x, chunk.GetTile(x, y).vertexY[3] * LevelData.STEP_Y, y + 1));
                normals.Add(Vector3.up);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 2);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
    }

    public const byte WALL_FRONT_FLAG = 0x01;
    public const byte WALL_RIGHT_FLAG = 0x02;
    public const byte WALL_BACK_FLAG  = 0x04;
    public const byte WALL_LEFT_FLAG  = 0x08;

    public static void AddWallMesh(Mesh mesh, LevelData data, int startX = 0, int startY = 0, int endX = int.MaxValue, int endY = int.MaxValue, 
        byte flags = WALL_FRONT_FLAG | WALL_RIGHT_FLAG)
    {
        ChunkData chunk = data.Chunks[0];
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        if (endX > chunk.SizeX) endX = chunk.SizeX;
        if (endY > chunk.SizeY) endY = chunk.SizeY;

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if ((flags & WALL_FRONT_FLAG) != 0) CreateFrontWall(x, y, chunk, vertices, triangles);
                if ((flags & WALL_RIGHT_FLAG) != 0) CreateRightWall(x, y, chunk, vertices, triangles);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    private static void CreateFrontWall(int x, int y, ChunkData chunk, List<Vector3> vertices, List<int> triangles)
    {
        if (!chunk.GetTile(x, y).active) return;

        int delta1;
        int delta2;

        if (y < chunk.SizeY - 1 && chunk.GetTile(x, y+1).active)
        {
            delta1 = chunk.GetTile(x, y).vertexY[2] - chunk.GetTile(x, y + 1).vertexY[1];
            delta2 = chunk.GetTile(x, y).vertexY[3] - chunk.GetTile(x, y + 1).vertexY[0];
        }
        else
        {
            delta1 = 20;
            delta2 = 20;
        }

        if (delta1 > 0 || delta2 > 0)
        {
            if (delta1 < 0) delta1 = 0;
            if (delta2 < 0) delta2 = 0;

            int startIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, chunk.GetTile(x, y).vertexY[2] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x,     chunk.GetTile(x, y).vertexY[3] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x,     (chunk.GetTile(x, y).vertexY[3] - delta2) * LevelData.STEP_Y, y + 1));
            vertices.Add(new Vector3(x + 1, (chunk.GetTile(x, y).vertexY[2] - delta1) * LevelData.STEP_Y, y + 1));

            if (delta1 > 0)
            {
                triangles.Add(startIndex);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 3);
            }
            if (delta2 > 0)
            {
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 3);
            }
        }
    }

    private static void CreateRightWall(int x, int y, ChunkData chunk, List<Vector3> vertices, List<int> triangles)
    {
        if (!chunk.GetTile(x, y).active) return;

        int delta1;
        int delta2;

        if (x < chunk.SizeX - 1 && chunk.GetTile(x+1, y).active)
        {
            delta1 = chunk.GetTile(x, y).vertexY[1] - chunk.GetTile(x + 1, y).vertexY[0];
            delta2 = chunk.GetTile(x, y).vertexY[2] - chunk.GetTile(x + 1, y).vertexY[3];
        }
        else
        {
            delta1 = 20;
            delta2 = 20;
        }

        if (delta1 > 0 || delta2 > 0)
        {
            if (delta1 < 0) delta1 = 0;
            if (delta2 < 0) delta2 = 0;

            int startIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, chunk.GetTile(x, y).vertexY[1] * LevelData.STEP_Y,      y));
            vertices.Add(new Vector3(x + 1, chunk.GetTile(x, y).vertexY[2] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x + 1, (chunk.GetTile(x, y).vertexY[2] - delta2) * LevelData.STEP_Y, y + 1));
            vertices.Add(new Vector3(x + 1, (chunk.GetTile(x, y).vertexY[1] - delta1) * LevelData.STEP_Y, y));
            if (delta1 > 0)
            {
                triangles.Add(startIndex);
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 3);
            }
            if (delta2 > 0)
            {
                triangles.Add(startIndex + 1);
                triangles.Add(startIndex + 2);
                triangles.Add(startIndex + 3);
            }
        }
    }
}
