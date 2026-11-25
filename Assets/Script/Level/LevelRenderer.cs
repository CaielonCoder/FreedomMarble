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
        CreateWallMesh();
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
        if (!_wallMesh) _wallMesh = new Mesh();
        _wallMesh.triangles = new int[0];
        LevelMeshUtils.AddWallMesh(_wallMesh, _data, 0, 0, 10000000, 10000000, LevelMeshUtils.WALL_FRONT_FLAG | LevelMeshUtils.WALL_RIGHT_FLAG);

        _wallMesh.RecalculateBounds();
        //_wallMesh.RecalculateNormals();

        _wallMeshFilter.mesh = _wallMesh;
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
