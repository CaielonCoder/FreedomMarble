using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditorBlockerMode : LevelEditorMode
    {
        private enum Mode
        {
            None,
            Adding,
            Removing,
        }
        private Mode mode = Mode.None;

        private Button _addButton;
        private Button _removeButton;

        private BlockerData _currentBlocker;
        private Vector3 _pointerPosition;
        private Vector3 _startDragPos;

        public override void CreateGUI(VisualElement root)
        {
            _addButton = root.Q<Button>("AddBlockerButton");
            _removeButton = root.Q<Button>("RemoveBlockerButton");

            _addButton.clicked += OnAddButtonClicked;
            _removeButton.clicked += OnRemoveButtonClicked;
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

        private void HandleRepaint()
        {
            if (_pointerPosition != Vector3.zero)
            {
                if (mode == Mode.Adding)
                {
                    Handles.DrawWireCube(_pointerPosition + Vector3.up * 0.3f, new Vector3(0.2f, 0.6f, 0.2f));
                    /*/
                                    float squareMargin = 0f;
                                    ChunkData chunk = _levelData.Chunks[0];
                                    Handles.color = new Color(0f, 1f, 0.8f, 0.3f);
                                    int x = Mathf.FloorToInt(_pointerPosition.x);
                                    int y = Mathf.FloorToInt(_pointerPosition.z);
                                    if (x >= 0 && x < chunk.SizeX && y >= 0 && y < chunk.SizeY)
                                    {
                                        TileData tile = chunk.GetTile(x, y);
                                        Vector3[] verts = new Vector3[4];
                                        verts[0] = new Vector3(x + squareMargin, tile.vertexY[0] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
                                        verts[1] = new Vector3(x + 1f - squareMargin, tile.vertexY[1] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + squareMargin);
                                        verts[2] = new Vector3(x + 1f - squareMargin, tile.vertexY[2] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);
                                        verts[3] = new Vector3(x + squareMargin, tile.vertexY[3] * LevelData.STEP_Y + LevelEditor.HANDLES_Z_BIAS, y + 1 - squareMargin);
                                        Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                                        Handles.DrawSolidRectangleWithOutline(verts, Handles.color, Color.cyan);
                                        Handles.zTest = UnityEngine.Rendering.CompareFunction.Greater;
                                        Handles.DrawSolidRectangleWithOutline(verts, Handles.color * 0.3f, Color.cyan);
                                    }
                    /*/
                }
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
                _startDragPos = _pointerPosition;
                _currentBlocker = new BlockerData();
                _currentBlocker.StartPos = _pointerPosition.XZ();
                _currentBlocker.EndPos = _pointerPosition.XZ();
                _levelData.Chunks[0].AddBlocker(_currentBlocker);
                RaiseLevelDataChanged();
                Event.current.Use();
            }
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
                    _currentBlocker.EndPos = _pointerPosition.XZ();
                    RaiseLevelDataChanged();
                }
                view.Repaint();
            }
        }

        private void HandleMouseUp(SceneView view)
        {
            _currentBlocker = null;
        }

        private void OnAddButtonClicked()
        {
            if (mode == Mode.Adding)
            {
                _addButton.SetActivePseudoState(false);
                _removeButton.SetEnabled(true);
                mode = Mode.None;
            }
            else
            {
                _addButton.SetActivePseudoState(true);
                _removeButton.SetEnabled(false);
                mode = Mode.Adding;
            }
        }

        private void OnRemoveButtonClicked()
        {
            if (mode == Mode.Removing)
            {
                _removeButton.SetActivePseudoState(false);
                _addButton.SetEnabled(true);
                mode = Mode.None;
            }
            else
            {
                _removeButton.SetActivePseudoState(true);
                _addButton.SetEnabled(false);
                mode = Mode.Removing;
            }
        }
    }
}
