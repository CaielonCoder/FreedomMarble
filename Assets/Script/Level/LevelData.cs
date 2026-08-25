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
                Chunks[0].Tiles[x, y] = new TileData();
                Chunks[0].Tiles[x, y].vertexY = new int[4];
            }
        }
    }
}

[System.Serializable]
public struct ChunkData
{
    public Vector3 Position;
    public TileData[,] Tiles;
}

[System.Serializable]
public struct TileData
{
    public int[] vertexY; // 0 = min X, min Y ; 1 = max X, min Y ; 2 = max X, max Y ; 3 = min X, max Y
}
