using UnityEngine;

public class LevelRenderer : MonoBehaviour
{
    [SerializeField]
    private LevelData _data;

    private Mesh _mesh;
    private MeshFilter _meshFilter;

    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        CreateMesh();
    }

    private void CreateMesh()
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

        if (!_mesh) _mesh = new Mesh();
        _mesh.vertices = vertices;
        _mesh.normals = normals;
        _mesh.uv = uv;
        _mesh.triangles = triangles;


        _meshFilter.mesh = _mesh;
    }

#if UNITY_EDITOR
    public LevelData GetLevelData()
    {
        return _data;
    }

    public Mesh GetMesh() { return _mesh; }

    public void OnDataUpdated()
    {
        _meshFilter = GetComponent<MeshFilter>();
        CreateMesh();
    }
#endif
}
