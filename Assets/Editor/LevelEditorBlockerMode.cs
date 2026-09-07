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
                switch (mode)
                {
                    case Mode.Adding:
                        Handles.DrawWireCube(_pointerPosition + Vector3.up * 0.3f, new Vector3(0.2f, 0.6f, 0.2f));
                        break;
                }
            }
        }

        private void HandleMouseMove(SceneView view)
        {
            switch (mode)
            {
                case Mode.Adding:
                    Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Level")))
                    {
                        _pointerPosition = hit.point;
                        CalculateSnap();

                        view.Repaint();
                    }
                    else
                    {
                        _pointerPosition = Vector3.zero;
                    }
                    break;
            }
        }

        private void CalculateSnap()
        {
            float x = Mathf.Round(_pointerPosition.x);
            float y = Mathf.Round(_pointerPosition.z);

            float SNAP_DISTANCE = 0.2f;

            if (Utils.DistanceSqrt(x, y, _pointerPosition.x, _pointerPosition.z) < SNAP_DISTANCE * SNAP_DISTANCE)
            {
                if (x - _pointerPosition.x > 0) x -= 0.1f;
                else x += 0.1f;

                if (y - _pointerPosition.z > 0) y -= 0.1f;
                else y += 0.1f;

                _pointerPosition.x = x;
                _pointerPosition.z = y;
            }
        }

        private void HandleMouseDown(SceneView view)
        {
            if (Event.current.button == 0)
            {
                switch (mode)
                {
                    case Mode.Adding:
                        CalculateSnap();
                        _startDragPos = _pointerPosition;
                        _currentBlocker = new BlockerData();
                        _currentBlocker.StartPos = _pointerPosition.XZ();
                        _currentBlocker.EndPos = _pointerPosition.XZ();
                        _levelData.Chunks[0].AddBlocker(_currentBlocker);
                        RaiseLevelDataChanged();
                        Event.current.Use();
                        break;
                }
            }
        }

        private void HandleMouseDrag(SceneView view)
        {
            if (Event.current.button == 0)
            {
                switch (mode)
                {
                    case Mode.Adding:
                        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        if (Physics.Raycast(ray, out RaycastHit hit, 100, LayerMask.GetMask("Level")))
                        {
                            _pointerPosition = hit.point;
                            CalculateSnap();
                            _currentBlocker.EndPos = _pointerPosition.XZ();
                            RaiseLevelDataChanged();
                        }
                        view.Repaint();
                        break;
                }
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
