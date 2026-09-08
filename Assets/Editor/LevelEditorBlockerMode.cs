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

        private BlockerData _currentBlockerData;
        private Blocker _blockerToRemove;
        private Vector3 _pointerPosition;

        private Material _selectedBlockerMat;

        public override void CreateGUI(VisualElement root)
        {
            _addButton = root.Q<Button>("AddBlockerButton");
            _removeButton = root.Q<Button>("RemoveBlockerButton");

            _addButton.clicked += OnAddButtonClicked;
            _removeButton.clicked += OnRemoveButtonClicked;

            _selectedBlockerMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Editor/BlockerSelectedMaterial.mat");
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
            switch (mode)
            {
                case Mode.Adding:
                    if (_pointerPosition != Vector3.zero)
                    {
                        Handles.DrawWireCube(_pointerPosition + Vector3.up * 0.3f, new Vector3(0.2f, 0.6f, 0.2f));
                    }
                    break;
                case Mode.Removing:
                    if (_blockerToRemove)
                    {
                        _selectedBlockerMat.SetPass(0);
                        Graphics.DrawMeshNow(_blockerToRemove.GetComponent<MeshFilter>().sharedMesh, Matrix4x4.identity);
                    }
                    break;
            }
        }

        private void HandleMouseMove(SceneView view)
        {
            Ray ray;
            RaycastHit hit;
            switch (mode)
            {
                case Mode.Adding:
                    ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Level")))
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
                case Mode.Removing:
                    ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    if (Physics.Raycast(ray, out hit, 100, LayerMask.GetMask("Blocker")))
                    {
                        _blockerToRemove = hit.collider.transform.parent.parent.GetComponent<Blocker>();
                        view.Repaint();
                    }
                    else
                    {
                        if (_blockerToRemove != null)
                        {
                            _blockerToRemove = null;
                            view.Repaint();
                        }
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
                        _currentBlockerData = new BlockerData();
                        _currentBlockerData.StartPos = _pointerPosition.XZ();
                        _currentBlockerData.EndPos = _pointerPosition.XZ();
                        _levelData.Chunks[0].AddBlocker(_currentBlockerData);
                        RaiseLevelDataChanged();
                        Event.current.Use();
                        break;
                    case Mode.Removing:
                        _levelData.Chunks[0].RemoveBlocker(_blockerToRemove.DataIndex);
                        GameObject.DestroyImmediate(_blockerToRemove.gameObject);
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
                            _currentBlockerData.EndPos = _pointerPosition.XZ();
                            RaiseLevelDataChanged();
                        }
                        view.Repaint();
                        break;
                }
            }
        }

        private void HandleMouseUp(SceneView view)
        {
            _currentBlockerData = null;
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
