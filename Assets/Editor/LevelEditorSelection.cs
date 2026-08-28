using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace LevelEditor
{
    public class Selection
    {
        public event Action<Vector2Int, Vector2Int, int> SelectionEdited; // Vector2Int tile x and y, float new y position

        private LevelData _levelData;
        private LevelEditor _editor;
        private Vector2Int _minPos;
        private Vector2Int _maxPos;
        private bool isSomethinSelected = false;

        public Selection(LevelData data, LevelEditor editor)
        {
            _levelData = data;
            _editor = editor;
        }

        public void SetSelection(Vector3 pointerPosition)
        {
            _minPos = new Vector2Int(Mathf.FloorToInt(pointerPosition.x), Mathf.FloorToInt(pointerPosition.z));
            _maxPos = _minPos;
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
            TileData tile = chunk.GetTile(Mathf.FloorToInt((_minPos.x + _maxPos.x + 1) / 2f), Mathf.FloorToInt((_minPos.y + _maxPos.y + 1) / 2f));

            float centerY = tile.vertexY[0];
            float q = 1;
            bool oddX = (_minPos.x + _maxPos.x) % 2 == 0;
            bool oddY = (_minPos.y + _maxPos.y) % 2 == 0;
            if (oddX)
            {
                centerY += tile.vertexY[1];
                q++;
            }
            if (oddY)
            {
                centerY += tile.vertexY[3];
                q++;
            }
            if (oddX && oddY)
            {
                centerY += tile.vertexY[2];
                q++;
            }

            Vector3 center = new Vector3((_minPos.x + _maxPos.x+1) / 2f, centerY * LevelData.STEP_Y / q, (_minPos.y + _maxPos.y+1) / 2f);

            Handles.color = Handles.yAxisColor;

            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.Slider(center, Vector3.up);
            if (EditorGUI.EndChangeCheck())
            {
                int delta_y = Mathf.RoundToInt((newCenter.y - center.y) / LevelData.STEP_Y);
                SelectionEdited?.Invoke(_minPos, _maxPos, delta_y);
            }
        }

        public void Draw()
        {
            if (!isSomethinSelected) return;

            for (int x = _minPos.x; x <= _maxPos.x; x++)
            {
                for (int y = _minPos.y; y <= _maxPos.y; y++)
                {
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                    Handles.color = new Color(0f, 1f, 0.2f, 0.5f);
                    DrawSquareOnTile(x, y, Color.green);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                    Handles.color = new Color(0f, 0.3f, 0.1f, 0.15f);
                    DrawSquareOnTile(x, y, Color.green);
                }
            }

            // Draw behaviour data
            ChunkData chunk = _levelData.Chunks[0];
            Handles.color = new Color(1f, 0f, 0.2f, 0.5f);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            float dotSize = 0.07f;

            Vector3 vert0 = new Vector3(_minPos.x, chunk.GetTile(_minPos).vertexY[0] * LevelData.STEP_Y,              _minPos.y);
            Vector3 vert1 = new Vector3(_maxPos.x + 1, chunk.GetTile(_maxPos.x, _minPos.y).vertexY[1] * LevelData.STEP_Y, _minPos.y);
            Vector3 vert2 = new Vector3(_maxPos.x + 1, chunk.GetTile(_maxPos).vertexY[2] * LevelData.STEP_Y             , _maxPos.y + 1);
            Vector3 vert3 = new Vector3(_minPos.x, chunk.GetTile(_minPos.x, _maxPos.y).vertexY[3] * LevelData.STEP_Y, _maxPos.y + 1);

            if (!_editor.GetTileBehaviour(0, 0))
                Handles.DotHandleCap(0, vert0, Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(1, 0))
                Handles.DotHandleCap(0, Vector3.Lerp(vert0, vert1, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(2, 0))
                Handles.DotHandleCap(0, vert1, Quaternion.identity, dotSize, EventType.Repaint);

            if (!_editor.GetTileBehaviour(2, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(vert1, vert2, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(2, 2))
                Handles.DotHandleCap(0, vert2, Quaternion.identity, dotSize, EventType.Repaint);

            if (!_editor.GetTileBehaviour(1, 2))
                Handles.DotHandleCap(0, Vector3.Lerp(vert2, vert3, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(0, 2))
                Handles.DotHandleCap(0, vert3, Quaternion.identity, dotSize, EventType.Repaint);
            if (!_editor.GetTileBehaviour(0, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(vert3, vert0, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
        }

        public void UpdateMultiSelect(Vector3 startPointerPos, Vector3 endPointerPosition)
        {
            _minPos.x = Mathf.FloorToInt(Mathf.Min(startPointerPos.x, endPointerPosition.x));
            _minPos.y = Mathf.FloorToInt(Mathf.Min(startPointerPos.z, endPointerPosition.z));
            _maxPos.x = Mathf.FloorToInt(Mathf.Max(startPointerPos.x, endPointerPosition.x));
            _maxPos.y = Mathf.FloorToInt(Mathf.Max(startPointerPos.z, endPointerPosition.z));
            isSomethinSelected = true;
        }

        private void DrawSquareOnTile(int tileX, int tileY, Color outlineColor)
        {
            TileData tile = _levelData.Chunks[0].GetTile(tileX, tileY);
            Vector3[] verts = new Vector3[4];
            verts[0] = new Vector3(tileX, tile.vertexY[0] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, tileY);
            verts[1] = new Vector3(tileX + 1f, tile.vertexY[1] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, tileY);
            verts[2] = new Vector3(tileX + 1f, tile.vertexY[2] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, tileY + 1);
            verts[3] = new Vector3(tileX, tile.vertexY[3] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, tileY + 1);
            Handles.DrawSolidRectangleWithOutline(verts, Handles.color, outlineColor);
        }
    }
}
