using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    public const float STEP_Y = 0.3f;
    public ChunkData[] Chunks;

    public LevelData()
    {
        Chunks = new ChunkData[1];
        Chunks[0] = new ChunkData();
        Chunks[0].Tiles = new TileData[6, 6];
        for (int x = 0; x < Chunks[0].Tiles.GetLength(0); x++)
        {
            for (int y = 0; y < Chunks[0].Tiles.GetLength(1); y++)
            {
                Chunks[0].Tiles[x, y] = new TileData(0, 0, 0, 0);
            }
        }
    }
}

[System.Serializable]
public struct ChunkData
{
    public Vector3 Position;
    public TileData[,] Tiles;

    public void Resize(int newX, int newY)
    {
        TileData[,] newTiles = new TileData[newX, newY];
        int oldX = Tiles.GetLength(0);
        int oldY = Tiles.GetLength(1);

        for (int x = 0; x < newX; x++)
        {
            for (int y = 0; y < newY; y++)
            {
                int xx = x < oldX ? x : oldX - 1;
                int yy = y < oldY ? y : oldY - 1;
                newTiles[x, y] = new TileData(Tiles[xx, yy]);
            }
        }
        Tiles = newTiles;
    }
}

[System.Serializable]
public struct TileData
{
    public int[] vertexY; // 0 = min X, min Y ; 1 = max X, min Y ; 2 = max X, max Y ; 3 = min X, max Y

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
