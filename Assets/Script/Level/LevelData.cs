using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public const float STEP_Y = 0.3f;
    [SerializeField]
    public ChunkData[] Chunks;

    public LevelData()
    {
        Chunks = new ChunkData[1];
        Chunks[0] = new ChunkData();
    }
}

[System.Serializable]
public class ChunkData
{
    public Vector3 Position;
    public int SizeX { get => _sizeX; }
    public int SizeY { get => _sizeY; }

    [SerializeField]
    private TileData[] _tiles = new TileData[16];
    [SerializeField]
    private int _sizeX = 4;
    [SerializeField]
    private int _sizeY = 4;

    public void Resize(int newX, int newY)
    {
        TileData[] newTiles = new TileData[newX * newY];
        int oldX = _sizeX;
        int oldY = _sizeY;

        for (int x = 0; x < newX; x++)
        {
            for (int y = 0; y < newY; y++)
            {
                int xx = x < oldX ? x : oldX - 1;
                int yy = y < oldY ? y : oldY - 1;
                newTiles[y + x * newY] = new TileData(GetTile(xx, yy));
            }
        }
        _tiles = newTiles;
        _sizeX = newX;
        _sizeY = newY;
    }

    public TileData GetTile(int x, int y)
    {
        return _tiles[y + x * _sizeY];
    }

    public TileData GetTile(Vector2Int pos)
    {
        return GetTile(pos.x, pos.y);
    }

    public void SetTile(TileData data, int x, int y)
    {
        _tiles[y + x * _sizeY] = data;
    }
}

[System.Serializable]
public class TileData
{
    public int[] vertexY = new int[4]; // 0 = min X, min Y ; 1 = max X, min Y ; 2 = max X, max Y ; 3 = min X, max Y

    public TileData() : this(0, 0, 0, 0) { }
    public TileData(TileData other) : this(other.vertexY[0], other.vertexY[1], other.vertexY[2], other.vertexY[3]) { }

    public TileData(int x1y1, int x2y1, int x2y2, int x1y2)
    {
        vertexY = new int[4];
        vertexY[0] = x1y1;
        vertexY[1] = x2y1;
        vertexY[2] = x2y2;
        vertexY[3] = x1y2;
    }
}
