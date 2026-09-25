using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditorVertexMode : LevelEditorMode
    {
        private struct PointerData
        {
            public Vector3 position;
            public int vertexX;
            public int vertexY;
            public TileData tile;
            public int vertexIndex;
        }
        private PointerData _pointerData;
        private PointerData _selectionData;

        private Toggle _topLeftToggle;
        private Toggle _bottomLeftToggle;
        private Toggle _topRightToggle;
        private Toggle _bottomRightToggle;
        private bool isSomethinSelected;

        public override void CreateGUI(VisualElement root)
        {
            _topLeftToggle = root.Q<Toggle>("EdgeTopLeft");
            _bottomLeftToggle = root.Q<Toggle>("EdgeBottomLeft");
            _topRightToggle = root.Q<Toggle>("EdgeTopRight");
            _bottomRightToggle = root.Q<Toggle>("EdgeBottomRight");
        }

        public override void Exit()
        {
            isSomethinSelected = false;
        }

        public override void OnSceneGUI(SceneView view)
        {
            SelectionOnSceneGUI(view);
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    HandleRepaint();
                    break;
                case EventType.MouseMove:
                    HandleMouseMove(view);
                    break;
                case EventType.MouseDown:
                    HandleMouseDown(view);
                    break;
            }
        }

        private void HandleRepaint()
        {
            Vector3 position = new Vector3(_pointerData.vertexX, _pointerData.tile.vertexY[_pointerData.vertexIndex] * LevelData.STEP_Y, _pointerData.vertexY);
            Handles.color = Color.cyan;
            Handles.CubeHandleCap(10, position, Quaternion.identity, 0.1f, EventType.Repaint);
            DrawSelection();
        }

        public void SelectionOnSceneGUI(SceneView view)
        {
            if (!isSomethinSelected) return;

            ChunkData chunk = _levelData.Chunks[0];
            int chunkMaxX = chunk.SizeX - 1;
            int chunkMaxY = chunk.SizeY - 1;
            int x = _selectionData.vertexX;
            int y = _selectionData.vertexY;
            int tileX = _selectionData.vertexX;
            int tileY = _selectionData.vertexY;

            float medianY = 0;
            int q = 0;
            if (_topLeftToggle.value && x > 0 && y > 0)
            {
                medianY += chunk.GetTile(tileX - 1, tileY - 1).vertexY[2];
                q++;
            }
            if (_bottomLeftToggle.value && x > 0 && y <= chunkMaxY)
            {
                medianY += chunk.GetTile(tileX - 1, tileY).vertexY[1];
                q++;
            }
            if (_topRightToggle.value && x <= chunkMaxX && y > 0)
            {
                medianY += chunk.GetTile(tileX, tileY - 1).vertexY[3];
                q++;
            }
            if (_bottomRightToggle.value && x <= chunkMaxX && y <= chunkMaxY)
            {
                medianY += chunk.GetTile(tileX, tileY).vertexY[0];
                q++;
            }

            Vector3 position = new Vector3(_selectionData.vertexX, (medianY / q) * LevelData.STEP_Y, _selectionData.vertexY);

            Handles.color = Handles.yAxisColor;

            EditorGUI.BeginChangeCheck();
            Vector3 newPosition = Handles.Slider(position, Vector3.up);
            if (EditorGUI.EndChangeCheck())
            {
                int delta_y = Mathf.RoundToInt((newPosition.y - position.y) / LevelData.STEP_Y);
                UpdateVertex(delta_y);
                RaiseLevelDataChanged();
            }
        }

        private void HandleMouseMove(SceneView view)
        {
            ChunkData chunk = _levelData.Chunks[0];
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Level")))
            {
                _pointerData.position = hit.point;
                Handles.color = new Color(0f, 1f, 0.8f, 0.3f);
                int x = Mathf.FloorToInt(_pointerData.position.x);
                int y = Mathf.FloorToInt(_pointerData.position.z);
                _pointerData.tile = _levelData.Chunks[0].GetTile(x, y);
                float deltaX = _pointerData.position.x - x - 0.5f;
                float deltaY = _pointerData.position.z - y - 0.5f;
                if (deltaX > 0 && deltaY > 0)
                {
                    _pointerData.vertexX = x + 1;
                    _pointerData.vertexY = y + 1;
                    _pointerData.vertexIndex = 2;
                }
                else if (deltaX > 0 && deltaY < 0)
                {
                    _pointerData.vertexX = x + 1;
                    _pointerData.vertexY = y;
                    _pointerData.vertexIndex = 1;
                }
                else if (deltaX < 0 && deltaY > 0)
                {
                    _pointerData.vertexX = x;
                    _pointerData.vertexY = y + 1;
                    _pointerData.vertexIndex = 3;
                }
                else if (deltaX < 0 && deltaY < 0)
                {
                    _pointerData.vertexX = x;
                    _pointerData.vertexY = y;
                    _pointerData.vertexIndex = 0;
                }

                view.Repaint();
            }
        }

        private void HandleMouseDown(SceneView view)
        {
            if (Event.current.button == 0)
            {
                _selectionData = _pointerData;
                isSomethinSelected = true;
                Event.current.Use();
            }
        }

        public void DrawSelection()
        {
            if (!isSomethinSelected) return;

            Vector3 position = new Vector3(_selectionData.vertexX, 
                _selectionData.tile.vertexY[_selectionData.vertexIndex] * LevelData.STEP_Y, _selectionData.vertexY);
            Handles.color = Color.green;
            Handles.CubeHandleCap(20, position, Quaternion.identity, 0.08f, EventType.Repaint);

            // Draw behaviour data
            ChunkData chunk = _levelData.Chunks[0];
            Handles.color = new Color(1f, 0f, 0.2f, 0.5f);
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            float dotSize = 0.07f;
            float offset = 0.2f;
            int chunkMaxX = chunk.SizeX - 1;
            int chunkMaxY = chunk.SizeY - 1;
            int x = _selectionData.vertexX;
            int y = _selectionData.vertexY;
            int tileX = _selectionData.vertexX;
            int tileY = _selectionData.vertexY;

            if (!_topLeftToggle.value && x > 0 && y > 0)
                Handles.DotHandleCap(0, new Vector3(x - offset, chunk.GetTile(tileX-1, tileY-1).vertexY[2] * LevelData.STEP_Y, y - offset)
                    , Quaternion.identity, dotSize, EventType.Repaint);
            if (!_bottomLeftToggle.value && x > 0 && y <= chunkMaxY)
                Handles.DotHandleCap(0, new Vector3(x - offset, chunk.GetTile(tileX-1, tileY).vertexY[1] * LevelData.STEP_Y, y + offset)
                    , Quaternion.identity, dotSize, EventType.Repaint);
            if (!_topRightToggle.value && x <= chunkMaxX && y > 0)
                Handles.DotHandleCap(0, new Vector3(x + offset, chunk.GetTile(tileX, tileY-1).vertexY[3] * LevelData.STEP_Y, y - offset)
                    , Quaternion.identity, dotSize, EventType.Repaint);
            if (!_bottomRightToggle.value && x <= chunkMaxX && y <= chunkMaxY)
                Handles.DotHandleCap(0, new Vector3(x + offset, chunk.GetTile(tileX, tileY).vertexY[0] * LevelData.STEP_Y, y + offset)
                    , Quaternion.identity, dotSize, EventType.Repaint);
        }

        private void UpdateVertex(int delta_y)
        {
            ChunkData chunk = _levelData.Chunks[0];
            int chunkMaxX = chunk.SizeX - 1;
            int chunkMaxY = chunk.SizeY - 1;
            int x = _selectionData.vertexX;
            int y = _selectionData.vertexY;
            int tileX = _selectionData.vertexX;
            int tileY = _selectionData.vertexY;

            if (_topLeftToggle.value && x > 0 && y > 0)
                chunk.GetTile(tileX - 1, tileY - 1).vertexY[2] += delta_y;
            if (_bottomLeftToggle.value && x > 0 && y <= chunkMaxY)
                chunk.GetTile(tileX - 1, tileY).vertexY[1] += delta_y;
            if (_topRightToggle.value && x <= chunkMaxX && y > 0)
                chunk.GetTile(tileX, tileY - 1).vertexY[3] += delta_y;
            if (_bottomRightToggle.value && x <= chunkMaxX && y <= chunkMaxY)
                chunk.GetTile(tileX, tileY).vertexY[0] += delta_y;
        }

    }
}
