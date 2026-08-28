using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditor : EditorWindow
    {
        public const float HANDLES_Z_BIAS = 0.02f;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private VisualElement _rootContainer;
        private Button _enabledButton;

        private bool _editionEnabled = false;
        private LevelMeshCreator _levelRenderer;
        private MeshCollider _levelCollider;
        private LevelData _levelData;

        private Vector3 _pointerPosition;
        private Selection _selection;

        private const float MULTISELECT_MIN_DISTANCE = 0.1f;
        private bool _isMultiSelect;
        private Vector3 _multiSelectStartPos;

        private Toggle[,] _behaviourButtons = new Toggle[3, 3];

        [MenuItem("Tools/LevelEditor")]
        public static void ShowLevelEditor()
        {
            LevelEditor wnd = GetWindow<LevelEditor>();
            wnd.titleContent = new GUIContent("LevelEditor");
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualElement editorUXML = m_VisualTreeAsset.Instantiate();

            _rootContainer = editorUXML.Q<VisualElement>("RootContainer");
            _rootContainer.visible = false;

            _enabledButton = editorUXML.Q<Button>("EnableButton");
            _enabledButton.clicked += OnEnabledClicked;

            editorUXML.Q<Button>("ChunkExtendXButton").clicked += OnExtendXClicked;
            editorUXML.Q<Button>("ChunkExtendZButton").clicked += OnExtendZClicked;
            editorUXML.Q<Button>("ChunkReduceXButton").clicked += OnReduceXClicked;
            editorUXML.Q<Button>("ChunkReduceZButton").clicked += OnReduceZClicked;

            _behaviourButtons[0, 0] = editorUXML.Q<Toggle>("TopLeft");
            _behaviourButtons[1, 0] = editorUXML.Q<Toggle>("Top");
            _behaviourButtons[2, 0] = editorUXML.Q<Toggle>("TopRight");
            _behaviourButtons[0, 1] = editorUXML.Q<Toggle>("Left");
            _behaviourButtons[2, 1] = editorUXML.Q<Toggle>("Right");
            _behaviourButtons[0, 2] = editorUXML.Q<Toggle>("BottomLeft");
            _behaviourButtons[1, 2] = editorUXML.Q<Toggle>("Bottom");
            _behaviourButtons[2, 2] = editorUXML.Q<Toggle>("BottomRight");

            root.Add(editorUXML);
        }

        private void OnEnabledClicked()
        {
            if (!_editionEnabled)
            {
                _rootContainer.visible = true;
                _levelRenderer = FindAnyObjectByType<LevelMeshCreator>();
                if (_levelRenderer == null)
                {
                    EditorUtility.DisplayDialog("Level Error", "Level renderer not found", "OK");
                }
                else
                {
                    _enabledButton.SetCheckedPseudoState(true);
                    _enabledButton.text = "Disable";
                    _editionEnabled = true;
                    _levelData = _levelRenderer.GetLevelData();
                    _levelRenderer.OnDataUpdated();
                    _selection = new Selection(_levelData, this);
                    _selection.SelectionEdited += OnSelectionEdited;

                    // TODO: create custom component for manage the collision
                   _levelCollider = _levelRenderer.gameObject.GetComponent<MeshCollider>();
                    if (_levelCollider == null)
                        _levelCollider = _levelRenderer.gameObject.AddComponent<MeshCollider>();
                    _levelCollider.sharedMesh = _levelRenderer.GetFloorMesh();
                }
            }
            else
            {
                if (_levelCollider != null) DestroyImmediate(_levelCollider);
                _enabledButton.SetCheckedPseudoState(false);
                _enabledButton.text = "Enable";
                _editionEnabled = false;
                _selection.SelectionEdited -= OnSelectionEdited;
                _rootContainer.visible = false;
                AssetDatabase.SaveAssets();
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (!_editionEnabled) return;

            _selection.OnSceneGUI(view);

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

        private void OnReduceZClicked() { ChangeChunkSize(0, -1); }

        private void OnReduceXClicked() { ChangeChunkSize(-1, 0); }

        private void OnExtendZClicked() { ChangeChunkSize(0, 1); }

        private void OnExtendXClicked() { ChangeChunkSize(1, 0); }

        private void ChangeChunkSize(int x, int y)
        {
            _levelData.Chunks[0].Resize(_levelData.Chunks[0].SizeX + x, _levelData.Chunks[0].SizeY + y);
            EditorUtility.SetDirty(_levelData);
            AssetDatabase.SaveAssets();
            _levelRenderer.OnDataUpdated();
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
            _selection.Draw();
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
                    _selection.UpdateMultiSelect(_multiSelectStartPos, _pointerPosition);
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
                    _selection.UpdateMultiSelect(_multiSelectStartPos, _pointerPosition);
                }
                else
                {
                    _selection.SetSelection(_pointerPosition);
                }
                Event.current.Use();
            }
        }
        private void OnSelectionEdited(Vector2Int minPos, Vector2Int maxPos, int delta_y)
        {
            UpdateVertices(minPos, maxPos, delta_y);
            _levelRenderer.OnDataUpdated();
            _levelCollider.sharedMesh = null;
            _levelCollider.sharedMesh = _levelRenderer.GetFloorMesh();
            EditorUtility.SetDirty(_levelData);
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
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
    }


}
