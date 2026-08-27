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
        private LevelEditor _editor;
        private int x;
        private int y;
        private bool isSomethinSelected = false;

        public Selection(LevelData data, LevelEditor editor)
        {
            _levelData = data;
            _editor = editor;
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

            if (x >= chunk.SizeX) x = chunk.SizeX - 1;
            if (y >= chunk.SizeY) y = chunk.SizeY - 1;

            TileData tile = chunk.GetTile(x, y);
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
            TileData tile = chunk.GetTile(x, y);
            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(x, tile.vertexY[0] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y);
            verts[1] = new Vector3(x + 1f, tile.vertexY[1] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y);
            verts[2] = new Vector3(x + 1f, tile.vertexY[2] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1);
            verts[3] = new Vector3(x, tile.vertexY[3] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.DrawSolidRectangleWithOutline(verts, new Color(0f, 1f, 0.2f, 0.5f), Color.green);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
            Handles.DrawSolidRectangleWithOutline(verts, new Color(0f, 1f, 0.2f, 0.5f) * 0.3f, Color.green);

            Handles.color = new Color(1f, 0f, 0.2f, 0.5f);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            float dotSize = 0.07f;

            if (!_editor.GetTileBehaviour(0, 0))
                Handles.DotHandleCap(0, verts[0], Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(1, 0))
                Handles.DotHandleCap(0, Vector3.Lerp(verts[0], verts[1], 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(2, 0))
                Handles.DotHandleCap(0, verts[1], Quaternion.identity, dotSize, EventType.Repaint);

            if (!_editor.GetTileBehaviour(2, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(verts[1], verts[2], 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(2, 2))
                Handles.DotHandleCap(0, verts[2], Quaternion.identity, dotSize, EventType.Repaint);

            if (!_editor.GetTileBehaviour(1, 2))
                Handles.DotHandleCap(0, Vector3.Lerp(verts[2], verts[3], 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(0, 2))
                Handles.DotHandleCap(0, verts[3], Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(0, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(verts[3], verts[0], 0.5f), Quaternion.identity, 0.1f, EventType.Repaint);
        }

        private void DrawRectangleInPosition(Vector3 position, float size)
        {
            Handles.DrawSolidRectangleWithOutline(new Vector3[] {
                position + new Vector3(-size, 0, -size),
                position + new Vector3(size, 0, -size),
                position + new Vector3(size, 0, size),
                position + new Vector3(-size, 0, size),
            }, Handles.color, Color.green);
        }
    }
}
