using Microsoft.Win32;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditorTileMode : LevelEditorMode
    {
        private struct PointerData
        {
            public Vector3 position;
            public bool isHoverTile;
            public int tileX;
            public int tileY;
            public TileData tileData;
        }

        private PointerData _pointerData;

        private Button _removeButton;
        private Toggle[,] _behaviourButtons = new Toggle[3, 3];


        private bool _isMultiSelect;
        private Vector3 _multiSelectStartPos;
        private Vector2Int _minSelectionPos;
        private Vector2Int _maxSelectionPos;
        private bool isSomethinSelected = false;

        public override void CreateGUI(VisualElement root) 
        {
            _removeButton = root.Q<Button>("RemoveTilesButton");
            _removeButton.clicked += OnRemoveButtonClicked;

            _behaviourButtons[0, 0] = root.Q<Toggle>("TopLeft");
            _behaviourButtons[1, 0] = root.Q<Toggle>("Top");
            _behaviourButtons[2, 0] = root.Q<Toggle>("TopRight");
            _behaviourButtons[0, 1] = root.Q<Toggle>("Left");
            _behaviourButtons[2, 1] = root.Q<Toggle>("Right");
            _behaviourButtons[0, 2] = root.Q<Toggle>("BottomLeft");
            _behaviourButtons[1, 2] = root.Q<Toggle>("Bottom");
            _behaviourButtons[2, 2] = root.Q<Toggle>("BottomRight");
        }

        public override void Enter(LevelData levelData)
        {
            base.Enter(levelData);
        }

        public override void Exit()
        {
        }

        private void OnRemoveButtonClicked()
        {
            for (int x = _minSelectionPos.x; x <= _maxSelectionPos.x; x++)
            {
                for (int y = _minSelectionPos.y; y <= _maxSelectionPos.y; y++)
                {
                    _levelData.Chunks[0].GetTile(x, y).active = false;
                }
            }
            RaiseLevelDataChanged();
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
                case EventType.MouseDrag:
                    HandleMouseDrag(view);
                    break;
                case EventType.MouseUp:
                    HandleMouseUp(view);
                    break;
            }
        }

        public void SelectionOnSceneGUI(SceneView view)
        {
            if (!isSomethinSelected) return;

            ChunkData chunk = _levelData.Chunks[0];
            TileData tile = chunk.GetTile(Mathf.FloorToInt((_minSelectionPos.x + _maxSelectionPos.x + 1) / 2f), Mathf.FloorToInt((_minSelectionPos.y + _maxSelectionPos.y + 1) / 2f));

            float centerY = tile.vertexY[0];
            float q = 1;
            bool oddX = (_minSelectionPos.x + _maxSelectionPos.x) % 2 == 0;
            bool oddY = (_minSelectionPos.y + _maxSelectionPos.y) % 2 == 0;
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

            Vector3 center = new Vector3((_minSelectionPos.x + _maxSelectionPos.x+1) / 2f, 
                centerY * LevelData.STEP_Y / q, 
                (_minSelectionPos.y + _maxSelectionPos.y+1) / 2f);

            Handles.color = Handles.yAxisColor;

            EditorGUI.BeginChangeCheck();
            Vector3 newCenter = Handles.Slider(center, Vector3.up);
            if (EditorGUI.EndChangeCheck())
            {
                int delta_y = Mathf.RoundToInt((newCenter.y - center.y) / LevelData.STEP_Y);
                UpdateVertices(_minSelectionPos, _maxSelectionPos, delta_y);
                RaiseLevelDataChanged();
            }
        }

        private void HandleRepaint()
        {
            if (_pointerData.isHoverTile)
            {

                int x = _pointerData.tileX;
                int y = _pointerData.tileY;
                TileData tile = _pointerData.tileData;
                float squareMargin = tile.active ? 0 : 0.1f;
                Vector3[] verts = new Vector3[4];
                verts[0] = new Vector3(x + squareMargin, tile.vertexY[0] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
                verts[1] = new Vector3(x + 1f - squareMargin, tile.vertexY[1] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
                verts[2] = new Vector3(x + 1f - squareMargin, tile.vertexY[2] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);
                verts[3] = new Vector3(x + squareMargin, tile.vertexY[3] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);

                Handles.color = new Color(0f, 0.8f, 0.3f, tile.active ? 0.4f : 0.85f);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Handles.DrawSolidRectangleWithOutline(verts, Handles.color, Color.cyan);
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                Handles.DrawSolidRectangleWithOutline(verts, Handles.color * 0.3f, Color.cyan);
            }
            DrawSelection();
        }

        private void HandleMouseDrag(SceneView view)
        {
            if (Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Level")))
                {
                    _pointerData.position = hit.point;
                    _isMultiSelect = true;
                    UpdateMultiSelect(_multiSelectStartPos, _pointerData.position);
                    _pointerData.isHoverTile = false;
                }
                view.Repaint();
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
                if (x >= 0 && x < chunk.SizeX && y >= 0 && y < chunk.SizeY)
                {
                    _pointerData.isHoverTile = true;
                    _pointerData.tileX = x;
                    _pointerData.tileY = y;
                    _pointerData.tileData = chunk.GetTile(x, y);
                }
                else
                {
                    _pointerData.isHoverTile = false;
                }
            }
            else
            {
                float minDist = 999999;
                TileData closestTile = null;
                int minX = 0;
                int minY = 0;
                for (int x = 0; x < chunk.SizeX; x++)
                {
                    for (int y = 0; y < chunk.SizeY; y++)
                    {
                        if (chunk.GetTile(x, y).active) continue;

                        // TODO: work this matematically to better performance
                        Vector3 tileCenter = new Vector3(x + 0.5f, chunk.GetTile(x, y).vertexY[0], y + 0.5f);
                        float t = Vector3.Dot(tileCenter - ray.origin, ray.direction.normalized);
                        float dist = Vector3.SqrMagnitude((ray.origin + ray.direction.normalized * t) - tileCenter);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            minX = x;
                            minY = y;
                            closestTile = chunk.GetTile(x, y);
                        }
                    }
                }
                if (closestTile != null)
                {
                    _pointerData.isHoverTile = true;
                    _pointerData.tileX = minX;
                    _pointerData.tileY = minY;
                    _pointerData.tileData = closestTile;
                }
                else
                {
                    _pointerData.isHoverTile = false;
                }
            }
            view.Repaint();
        }

        private void HandleMouseDown(SceneView view)
        {
            if (Event.current.button == 0)
            {
                _multiSelectStartPos = _pointerData.position;
                _isMultiSelect = false;
                Event.current.Use();
            }
        }

        private void HandleMouseUp(SceneView view)
        {
            if (Event.current.button == 0)
            {
                if (_isMultiSelect)
                {
                    UpdateMultiSelect(_multiSelectStartPos, _pointerData.position);
                }
                else
                {
                    if (_pointerData.isHoverTile && !_pointerData.tileData.active)
                    {
                        _pointerData.tileData.active = true;
                        RaiseLevelDataChanged();
                        view.Repaint();
                    }
                    else
                    {
                        SetSelection(_pointerData.position); // TODO: old, change for new method
                    }
                }
                Event.current.Use();
            }
        }

        private void UpdateVertices(Vector2Int minPos, Vector2Int maxPos, int delta_y)
        {
            ChunkData chunk = _levelData.Chunks[0];
            int chunkMaxX = chunk.SizeX - 1;
            int chunkMaxY = chunk.SizeY - 1;

            // Update the edges of selection
            if (GetTileBehaviour(0, 0) && minPos.x > 0 && minPos.y > 0) chunk.GetTile(minPos.x-1, minPos.y-1).vertexY[2] += delta_y;
            if (GetTileBehaviour(2, 0) && maxPos.x < chunkMaxX && minPos.y > 0) chunk.GetTile(maxPos.x+1, minPos.y-1).vertexY[3] += delta_y;
            if (GetTileBehaviour(0, 2) && minPos.x > 0 && maxPos.y < chunkMaxY) chunk.GetTile(minPos.x-1, maxPos.y+1).vertexY[1] += delta_y;
            if (GetTileBehaviour(2, 2) && maxPos.x < chunkMaxX && maxPos.y < chunkMaxY) chunk.GetTile(maxPos.x+1, maxPos.y+1).vertexY[0] += delta_y;

            for (int tileX = minPos.x; tileX <= maxPos.x; tileX++)
            {
                for (int tileY = minPos.y; tileY <= maxPos.y; tileY++)
                {
                    if (GetTileBehaviour(1, 0) && tileY > 0 && tileY == minPos.y)
                    {
                        chunk.GetTile(tileX, tileY - 1).vertexY[3] += delta_y;
                        chunk.GetTile(tileX, tileY - 1).vertexY[2] += delta_y;
                    }

                    if (GetTileBehaviour(0, 1) && tileX > 0 && tileX == minPos.x)
                    {
                        chunk.GetTile(tileX - 1, tileY).vertexY[2] += delta_y;
                        chunk.GetTile(tileX - 1, tileY).vertexY[1] += delta_y;
                    }

                    if (GetTileBehaviour(2, 1) && tileX < chunkMaxX && tileX == maxPos.x)
                    {
                        chunk.GetTile(tileX + 1, tileY).vertexY[0] += delta_y;
                        chunk.GetTile(tileX + 1, tileY).vertexY[3] += delta_y;
                    }

                    if (GetTileBehaviour(1, 2) && tileY < chunkMaxY && tileY == maxPos.y)
                    {
                        chunk.GetTile(tileX, tileY + 1).vertexY[0] += delta_y;
                        chunk.GetTile(tileX, tileY + 1).vertexY[1] += delta_y;
                    }

                    TileData tile = chunk.GetTile(tileX, tileY);
                    tile.vertexY[0] += delta_y;
                    tile.vertexY[1] += delta_y;
                    tile.vertexY[2] += delta_y;
                    tile.vertexY[3] += delta_y;
                }
            }
        }

        public bool GetTileBehaviour(int x, int y)
        {
            return _behaviourButtons[x, y].value;
        }

        public void SetSelection(Vector3 pointerPosition)
        {
            _minSelectionPos = new Vector2Int(Mathf.FloorToInt(pointerPosition.x), Mathf.FloorToInt(pointerPosition.z));
            _maxSelectionPos = _minSelectionPos;
            isSomethinSelected = true;
        }

        public void UnsetSelection()
        {
            isSomethinSelected = false;
        }

        public void DrawSelection()
        {
            if (!isSomethinSelected) return;

            for (int x = _minSelectionPos.x; x <= _maxSelectionPos.x; x++)
            {
                for (int y = _minSelectionPos.y; y <= _maxSelectionPos.y; y++)
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

            Vector3 vert0 = new Vector3(_minSelectionPos.x, chunk.GetTile(_minSelectionPos).vertexY[0] * LevelData.STEP_Y,              _minSelectionPos.y);
            Vector3 vert1 = new Vector3(_maxSelectionPos.x + 1, chunk.GetTile(_maxSelectionPos.x, _minSelectionPos.y).vertexY[1] * LevelData.STEP_Y, _minSelectionPos.y);
            Vector3 vert2 = new Vector3(_maxSelectionPos.x + 1, chunk.GetTile(_maxSelectionPos).vertexY[2] * LevelData.STEP_Y             , _maxSelectionPos.y + 1);
            Vector3 vert3 = new Vector3(_minSelectionPos.x, chunk.GetTile(_minSelectionPos.x, _maxSelectionPos.y).vertexY[3] * LevelData.STEP_Y, _maxSelectionPos.y + 1);

            if (!GetTileBehaviour(0, 0))
                Handles.DotHandleCap(0, vert0, Quaternion.identity, dotSize, EventType.Repaint);
            if (!GetTileBehaviour(1, 0))
                Handles.DotHandleCap(0, Vector3.Lerp(vert0, vert1, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!GetTileBehaviour(2, 0))
                Handles.DotHandleCap(0, vert1, Quaternion.identity, dotSize, EventType.Repaint);

            if (!GetTileBehaviour(2, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(vert1, vert2, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!GetTileBehaviour(2, 2))
                Handles.DotHandleCap(0, vert2, Quaternion.identity, dotSize, EventType.Repaint);

            if (!GetTileBehaviour(1, 2))
                Handles.DotHandleCap(0, Vector3.Lerp(vert2, vert3, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
            if (!GetTileBehaviour(0, 2))
                Handles.DotHandleCap(0, vert3, Quaternion.identity, dotSize, EventType.Repaint);
            if (!GetTileBehaviour(0, 1))
                Handles.DotHandleCap(0, Vector3.Lerp(vert3, vert0, 0.5f), Quaternion.identity, dotSize, EventType.Repaint);
        }

        public void UpdateMultiSelect(Vector3 startPointerPos, Vector3 endPointerPosition)
        {
            _minSelectionPos.x = Mathf.FloorToInt(Mathf.Min(startPointerPos.x, endPointerPosition.x));
            _minSelectionPos.y = Mathf.FloorToInt(Mathf.Min(startPointerPos.z, endPointerPosition.z));
            _maxSelectionPos.x = Mathf.FloorToInt(Mathf.Max(startPointerPos.x, endPointerPosition.x));
            _maxSelectionPos.y = Mathf.FloorToInt(Mathf.Max(startPointerPos.z, endPointerPosition.z));
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
