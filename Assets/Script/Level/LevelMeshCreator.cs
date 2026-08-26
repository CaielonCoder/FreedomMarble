using System.Collections.Generic;
using UnityEngine;

public class LevelMeshCreator : MonoBehaviour
{
    [SerializeField]
    private LevelData _data;

    [SerializeField]
    private MeshFilter _floorMeshFilter;

    [SerializeField]
    private MeshFilter _wallMeshFilter;

    private Mesh _floorMesh;
    private Mesh _wallMesh;

    private void Start()
    {
        CreateFloorMesh();
    }

    private void CreateFloorMesh()
    {
        ChunkData chunk = _data.Chunks[0];
        int vertexCount = chunk.Tiles.Length * 4;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];
        int[] triangles = new int[(vertexCount * 3) / 2];

        Vector2Int chunkSize = new Vector2Int(chunk.Tiles.GetLength(0), chunk.Tiles.GetLength(1));

        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                int vertexIndex = (y + x * chunkSize.y) * 4;
                int triangleIndex = (y + x * chunkSize.y) * 6;

                vertices[vertexIndex].x = x;
                vertices[vertexIndex].y = chunk.Tiles[x, y].vertexY[0] * LevelData.STEP_Y;
                vertices[vertexIndex].z = y;
                normals[vertexIndex] = Vector3.up;
                uv[vertexIndex].x = 0;
                uv[vertexIndex].y = 0;

                vertices[vertexIndex + 1].x = x + 1;
                vertices[vertexIndex + 1].y = chunk.Tiles[x, y].vertexY[1] * LevelData.STEP_Y;
                vertices[vertexIndex + 1].z = y;
                normals[vertexIndex + 1] = Vector3.up;
                uv[vertexIndex + 1].x = 1;
                uv[vertexIndex + 1].y = 0;

                vertices[vertexIndex + 2].x = x + 1;
                vertices[vertexIndex + 2].y = chunk.Tiles[x, y].vertexY[2] * LevelData.STEP_Y;
                vertices[vertexIndex + 2].z = y + 1;
                normals[vertexIndex + 2] = Vector3.up;
                uv[vertexIndex + 2].x = 1;
                uv[vertexIndex + 2].y = 1;

                vertices[vertexIndex + 3].x = x;
                vertices[vertexIndex + 3].y = chunk.Tiles[x, y].vertexY[3] * LevelData.STEP_Y;
                vertices[vertexIndex + 3].z = y + 1;
                normals[vertexIndex + 3] = Vector3.up;
                uv[vertexIndex + 3].x = 0;
                uv[vertexIndex + 3].y = 1;

                triangles[triangleIndex] = vertexIndex;
                triangles[triangleIndex + 1] = vertexIndex + 2;
                triangles[triangleIndex + 2] = vertexIndex + 1;
                triangles[triangleIndex + 3] = vertexIndex;
                triangles[triangleIndex + 4] = vertexIndex + 3;
                triangles[triangleIndex + 5] = vertexIndex + 2;
            }
        }

        if (!_floorMesh) _floorMesh = new Mesh();
        _floorMesh.triangles = null;
        _floorMesh.vertices = vertices;
        _floorMesh.normals = normals;
        _floorMesh.uv = uv;
        _floorMesh.triangles = triangles;

        _floorMeshFilter.mesh = _floorMesh;
    }

    private void CreateWallMesh()
    {
        TileData[,] tiles = _data.Chunks[0].Tiles;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> triangles = new List<int>();

        Vector2Int chunkSize = new Vector2Int(tiles.GetLength(0), tiles.GetLength(1));

        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                CreateFrontWall(x, y, tiles, vertices, uv, triangles);
                //CreateLeftWall(x, y, tiles, vertices, uv, triangles);
                //CreateBackWall(x, y, tiles, vertices, uv, triangles);
                CreateRightWall(x, y, tiles, vertices, uv, triangles);
            }
        }

        if (!_wallMesh) _wallMesh = new Mesh();
        _wallMesh.triangles = null;
        _wallMesh.vertices = vertices.ToArray();
        _wallMesh.uv = uv.ToArray();
        _wallMesh.triangles = triangles.ToArray();

        _wallMesh.RecalculateBounds();
        _wallMesh.RecalculateNormals();

        _wallMeshFilter.mesh = _wallMesh;
    }

    private void CreateLeftWall(int x, int y, TileData[,] tiles, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
    {
        if (x == 0)
        {
            int startIndex = vertices.Count;
            vertices.Add(new Vector3(x, tiles[x, y].vertexY[0] * LevelData.STEP_Y,      y));
            vertices.Add(new Vector3(x, tiles[x, y].vertexY[3] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x, tiles[x, y].vertexY[3] * LevelData.STEP_Y - 10, y + 1));
            vertices.Add(new Vector3(x, tiles[x, y].vertexY[0] * LevelData.STEP_Y - 10, y));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 10));
            uv.Add(new Vector2(0, 10));
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
        }
    }

    private void CreateBackWall(int x, int y, TileData[,] tiles, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
    {
        if (y == 0)
        {
            int startIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, tiles[x, y].vertexY[1] * LevelData.STEP_Y,      y));
            vertices.Add(new Vector3(x,     tiles[x, y].vertexY[0] * LevelData.STEP_Y,      y));
            vertices.Add(new Vector3(x,     tiles[x, y].vertexY[0] * LevelData.STEP_Y - 10, y));
            vertices.Add(new Vector3(x + 1, tiles[x, y].vertexY[1] * LevelData.STEP_Y - 10, y));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 10));
            uv.Add(new Vector2(0, 10));
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 1);
        }
    }

    private void CreateFrontWall(int x, int y, TileData[,] tiles, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
    {
        int delta1;
        int delta2;

        if (y < tiles.GetLength(1) - 1)
        {
            delta1 = tiles[x, y].vertexY[2] - tiles[x, y + 1].vertexY[1];
            delta2 = tiles[x, y].vertexY[3] - tiles[x, y + 1].vertexY[0];
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
            vertices.Add(new Vector3(x + 1, tiles[x, y].vertexY[2] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x,     tiles[x, y].vertexY[3] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x,     (tiles[x, y].vertexY[3] - delta2) * LevelData.STEP_Y, y + 1));
            vertices.Add(new Vector3(x + 1, (tiles[x, y].vertexY[2] - delta1) * LevelData.STEP_Y, y + 1));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 10));
            uv.Add(new Vector2(0, 10));

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

    private void CreateRightWall(int x, int y, TileData[,] tiles, List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
    {
        int delta1;
        int delta2;

        if (x < tiles.GetLength(0) - 1)
        {
            delta1 = tiles[x, y].vertexY[1] - tiles[x + 1, y].vertexY[0];
            delta2 = tiles[x, y].vertexY[2] - tiles[x + 1, y].vertexY[3];
        }
        else
        {
            delta1 = 20;
            delta2 = 20;
        }

        if (delta1 > 0 || delta2 > 0)
        {
            int startIndex = vertices.Count;
            vertices.Add(new Vector3(x + 1, tiles[x, y].vertexY[1] * LevelData.STEP_Y,      y));
            vertices.Add(new Vector3(x + 1, tiles[x, y].vertexY[2] * LevelData.STEP_Y,      y + 1));
            vertices.Add(new Vector3(x + 1, (tiles[x, y].vertexY[2] - delta2) * LevelData.STEP_Y, y + 1));
            vertices.Add(new Vector3(x + 1, (tiles[x, y].vertexY[1] - delta1) * LevelData.STEP_Y, y));
            uv.Add(new Vector2(0, 0));
            uv.Add(new Vector2(1, 0));
            uv.Add(new Vector2(1, 10));
            uv.Add(new Vector2(0, 10));
            triangles.Add(startIndex);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 3);
            triangles.Add(startIndex + 1);
            triangles.Add(startIndex + 2);
            triangles.Add(startIndex + 3);
        }
    }
#if UNITY_EDITOR
    public LevelData GetLevelData()
    {
        return _data;
    }

    public Mesh GetFloorMesh() { return _floorMesh; }

    public void OnDataUpdated()
    {
        CreateFloorMesh();
        CreateWallMesh();
    }
#endif
}
