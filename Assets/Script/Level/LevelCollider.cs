using UnityEngine;

public class LevelCollider : MonoBehaviour
{
    [SerializeField]
    private LevelData _levelData;

    private void Start()
    {
        CreateColliders();        
    }

    private void CreateColliders()
    {
        Transform collidersRoot = new GameObject("CollidersRoot").transform;
        collidersRoot.transform.parent = transform;

        int sizeX = _levelData.Chunks[0].SizeX;
        int sizeY = _levelData.Chunks[0].SizeY;
        for (int x = 0; x < sizeX; x += 5)
        {
            for (int y = 0; y < sizeY; y += 5)
            {
                GameObject col = new GameObject($"Collider_{x}_{y}");
                col.transform.parent = collidersRoot;
                Mesh mesh = new Mesh();
                LevelMeshUtils.AddFloorMesh(mesh, _levelData, x, y, x + 5, y + 5);
                col.AddComponent<MeshCollider>().sharedMesh = mesh;
            }
        }
    }
}
