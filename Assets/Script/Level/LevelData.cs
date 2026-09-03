using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    public float GetHeightAt(Vector2 position)
    {
        return GetHeightAt(position.x, position.y);
    }

    public float GetHeightAt(float x, float y)
    {
        ChunkData chunk = Chunks[0];
        int xx = Mathf.FloorToInt(x);
        int yy = Mathf.FloorToInt(y);
        TileData tile = chunk.GetTile(xx, yy);
        return tile.GetHeightAt(x - xx, y - yy);
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

    [SerializeField]
    private List<BlockerData> _blockers = new List<BlockerData>();

    public void Resize(int newX, int newY)
    {
        TileData[] newTiles = new TileData[newX * newY];
        int oldSizeX = _sizeX;
        int oldSizeY = _sizeY;

        for (int x = 0; x < newX; x++)
        {
            for (int y = 0; y < newY; y++)
            {
                int xx = x < oldSizeX ? x : oldSizeX - 1;
                int yy = y < oldSizeY ? y : oldSizeY - 1;
                newTiles[y + x * newY] = new TileData(GetTile(xx, yy));
                TileData tile = newTiles[y + x * newY];
                if (x >= oldSizeX)
                {
                    tile.vertexY[0] = tile.vertexY[1];
                    tile.vertexY[3] = tile.vertexY[2];
                }
                if (y >= oldSizeY)
                {
                    tile.vertexY[0] = tile.vertexY[3];
                    tile.vertexY[1] = tile.vertexY[2];
                }
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

    public int AddBlocker(BlockerData blocker)
    {
        _blockers.Add(blocker);
        return _blockers.Count-1;
    }

    public BlockerData GetBlockerData(int index)
    {
        return _blockers[index]; 
    }

    public int GetBlockerCount()
    {
        return _blockers.Count; 
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

    internal float GetHeightAt(float x, float y)
    {
        float lerpX1 = Mathf.Lerp(vertexY[0] * LevelData.STEP_Y, vertexY[1] * LevelData.STEP_Y, x);
        float lerpX2 = Mathf.Lerp(vertexY[3] * LevelData.STEP_Y, vertexY[2] * LevelData.STEP_Y, x);
        return Mathf.Lerp(lerpX1, lerpX2, y);
    }
}

[System.Serializable]
public class BlockerData
{
    public Vector2 StartPos;
    public Vector2 EndPos;
}
