using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    public class Selection
    {
        public event Action<Vector2Int, int> SelectionEdited; // Vector2Int tile x and y, float new y position

        private LevelData _levelData;
        private int x;
        private int y;
        private bool isSomethinSelected = false;

        public Selection(LevelData data)
        {
            _levelData = data;
        }

        public void SetSelection(Vector3 pointerPosition)
        {
            x = Mathf.FloorToInt(pointerPosition.x);
            y = Mathf.FloorToInt(pointerPosition.z);
            isSomethinSelected = true;
        }

        public void UnsetSelection()
        {
            isSomethinSelected = false;
        }

        public void OnSceneGUI(SceneView view)
        {
            if (!isSomethinSelected) return;

            ChunkData chunk = _levelData.Chunks[0];
            TileData tile = chunk.Tiles[x, y];
            float centerY = (tile.vertexY[0] + tile.vertexY[1] + tile.vertexY[2] + tile.vertexY[3]) * LevelData.STEP_Y;
            Vector3 center = new Vector3(x + 0.5f, centerY / 4.0f, y + 0.5f);

            Handles.color = Handles.yAxisColor;

            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.Slider(center, Vector3.up);
            if (EditorGUI.EndChangeCheck())
            {
                int delta_y = Mathf.RoundToInt((newCenter.y - center.y) / LevelData.STEP_Y);
                SelectionEdited?.Invoke(new Vector2Int(x, y), delta_y);
            }
        }

        public void Draw()
        {
            if (!isSomethinSelected) return;

            float squareMargin = 0.3f;

            ChunkData chunk = _levelData.Chunks[0];
            TileData tile = chunk.Tiles[x, y];
            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(x + squareMargin, tile.vertexY[0] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
            verts[1] = new Vector3(x + 1f - squareMargin, tile.vertexY[1] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
            verts[2] = new Vector3(x + 1f - squareMargin, tile.vertexY[2] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);
            verts[3] = new Vector3(x + squareMargin, tile.vertexY[3] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline(verts, new Color(0f, 1f, 0.2f, 0.7f), Color.green);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline(verts, new Color(0f, 1f, 0.2f, 0.7f) * 0.3f, Color.green);
        }
    }
}
