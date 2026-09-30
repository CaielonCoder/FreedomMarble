using System.Collections.Generic;
using UnityEngine;

public class LevelRenderer : MonoBehaviour
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
        if (!_floorMesh) _floorMesh = new Mesh();
        _floorMesh.triangles = null;
        LevelMeshUtils.AddFloorMesh(_floorMesh, _data);
        _floorMeshFilter.mesh = _floorMesh;
    }

    private void CreateWallMesh()
    {
        ChunkData chunk = _data.Chunks[0];
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int x = 0; x < chunk.SizeX; x++)
        {
            for (int y = 0; y < chunk.SizeY; y++)
            {
                CreateFrontWall(x, y, chunk, vertices, triangles);
                CreateRightWall(x, y, chunk, vertices, triangles);
            }
        }

        if (!_wallMesh) _wallMesh = new Mesh();
        _wallMesh.triangles = null;
        _wallMesh.vertices = vertices.ToArray();
        _wallMesh.triangles = triangles.ToArray();

        _wallMesh.RecalculateBounds();
        _wallMesh.RecalculateNormals();

        _wallMeshFilter.mesh = _wallMesh;
    }

    private void CreateFrontWall(int x, int y, ChunkData chunk, List<Vector3> vertices, List<int> triangles)
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

    private void CreateRightWall(int x, int y, ChunkData chunk, List<Vector3> vertices, List<int> triangles)
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
#if UNITY_EDITOR
    public LevelData GetLevelData()
    {
        return _data;
    }

    public Mesh GetFloorMesh() { return _floorMesh; }

    public void OnDataUpdated()
    {
        if (!_floorMesh)
        {
            _floorMesh = new Mesh();
        }
        else
        {
            _floorMesh.triangles = new int[0];
            _floorMesh.vertices = new Vector3[0];
            _floorMesh.normals = new Vector3[0];
        }
        CreateFloorMesh();
        CreateWallMesh();
    }
#endif
}
