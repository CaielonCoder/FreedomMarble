using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditorTileMode : LevelEditorMode
    {
        public const float HANDLES_Z_BIAS = 0.02f;

        private Toggle[,] _behaviourButtons = new Toggle[3, 3];

        private Vector3 _pointerPosition;
        private const float MULTISELECT_MIN_DISTANCE = 0.1f;
        private bool _isMultiSelect;
        private Vector3 _multiSelectStartPos;

        private LevelEditor _editor;
        private Vector2Int _minPos;
        private Vector2Int _maxPos;
        private bool isSomethinSelected = false;

        public override void CreateGUI(VisualElement root) 
        { 
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
                UpdateVertices(_minPos, _maxPos, delta_y);
                RaiseLevelDataChanged();
            }
        }

        private void HandleRepaint()
        {
            if (_pointerPosition != Vector3.zero)
            {
                float squareMargin = 0f;

                ChunkData chunk = _levelData.Chunks[0];
                Handles.color = new Color(0f, 1f, 0.8f, 0.3f);
                int x = Mathf.FloorToInt(_pointerPosition.x);
                int y = Mathf.FloorToInt(_pointerPosition.z);
                if (x >= 0 && x < chunk.SizeX && y >= 0 && y < chunk.SizeY)
                {
                    TileData tile = chunk.GetTile(x, y);
                    Vector3[] verts = new Vector3[4];
                    verts[0] = new Vector3(x + squareMargin, tile.vertexY[0] * LevelData.STEP_Y + HANDLES_Z_BIAS, y + squareMargin);
                    verts[1] = new Vector3(x + 1f - squareMargin, tile.vertexY[1] * LevelData.STEP_Y + HANDLES_Z_BIAS, y + squareMargin);
                    verts[2] = new Vector3(x + 1f - squareMargin, tile.vertexY[2] * LevelData.STEP_Y + HANDLES_Z_BIAS, y + 1 - squareMargin);
                    verts[3] = new Vector3(x + squareMargin, tile.vertexY[3] * LevelData.STEP_Y + HANDLES_Z_BIAS, y + 1 - squareMargin);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                    Handles.DrawSolidRectangleWithOutline(verts, Handles.color, Color.cyan);
                    Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                    Handles.DrawSolidRectangleWithOutline(verts, Handles.color * 0.3f, Color.cyan);
                }
            }
            DrawSelection();
        }

        private void HandleMouseDrag(SceneView view)
        {
            if (Event.current.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                object hit = HandleUtility.RaySnap(ray);
                if (hit != null)
                {
                    _pointerPosition = ((RaycastHit)hit).point;
                    _isMultiSelect = true;
                    UpdateMultiSelect(_multiSelectStartPos, _pointerPosition);
                }
                view.Repaint();
            }
        }

        private void HandleMouseMove(SceneView view)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            object hit = HandleUtility.RaySnap(ray);
            if (hit != null)
            {
                _pointerPosition = ((RaycastHit)hit).point;
                view.Repaint();
            }
            else
            {
                _pointerPosition = Vector3.zero;
            }
        }

        private void HandleMouseDown(SceneView view)
        {
            if (Event.current.button == 0)
            {
                _multiSelectStartPos = _pointerPosition;
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
                    UpdateMultiSelect(_multiSelectStartPos, _pointerPosition);
                }
                else
                {
                    SetSelection(_pointerPosition);
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
            _minPos = new Vector2Int(Mathf.FloorToInt(pointerPosition.x), Mathf.FloorToInt(pointerPosition.z));
            _maxPos = _minPos;
            isSomethinSelected = true;
        }

        public void UnsetSelection()
        {
            isSomethinSelected = false;
        }

        public void DrawSelection()
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
            verts[0] = new Vector3(tileX, tile.vertexY[0] * LevelData.STEP_Y + HANDLES_Z_BIAS, tileY);
            verts[1] = new Vector3(tileX + 1f, tile.vertexY[1] * LevelData.STEP_Y + HANDLES_Z_BIAS, tileY);
            verts[2] = new Vector3(tileX + 1f, tile.vertexY[2] * LevelData.STEP_Y + HANDLES_Z_BIAS, tileY + 1);
            verts[3] = new Vector3(tileX, tile.vertexY[3] * LevelData.STEP_Y + HANDLES_Z_BIAS, tileY + 1);
            Handles.DrawSolidRectangleWithOutline(verts, Handles.color, outlineColor);
        }
    }
}
