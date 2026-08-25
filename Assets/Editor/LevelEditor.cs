using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditor : EditorWindow
    {
        public const float HANDLES_Z_BIAS = 0.01f;

        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private Button _enabledButton;

        private bool _editionEnabled = false;
        private LevelMeshCreator _levelRenderer;
        private MeshCollider _levelCollider;
        private LevelData _levelData;

        private Vector3 _pointerPosition;
        private Selection _selection;

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

            _enabledButton = editorUXML.Q<Button>("EnableButton");
            _enabledButton.clicked += OnEnabledClicked;

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
                    _selection = new Selection(_levelData);
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
            }
        }

        private void HandleRepaint()
        {
            if (_pointerPosition != Vector3.zero)
            {
                float squareMargin = 0.2f;

                ChunkData chunk = _levelData.Chunks[0];
                TileData[,] tiles = chunk.Tiles;
                Handles.color = new Color(0f, 1f, 0.8f, 0.5f);
                int x = Mathf.FloorToInt(_pointerPosition.x);
                int y = Mathf.FloorToInt(_pointerPosition.z);
                if (x >= 0 && x < tiles.GetLength(0) && y >= 0 && y < tiles.GetLength(1))
                {
                    TileData tile = chunk.Tiles[x, y];
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
                _selection.SetSelection(_pointerPosition);
                Event.current.Use();
            }
        }
        private void OnSelectionEdited(Vector2Int tilePos, int delta_y)
        {
            UpdateVertices(tilePos.x, tilePos.y, delta_y);
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

        private void UpdateVertices(int tileX, int tileY, int delta_y)
        {
            TileData[,] tiles = _levelData.Chunks[0].Tiles;
            int maxX = tiles.GetLength(0) - 1;
            int maxY = tiles.GetLength(1) - 1;

            if (_behaviourButtons[0, 0].value && tileX > 0 && tileY > 0) tiles[tileX - 1, tileY - 1].vertexY[2] += delta_y;

            if (_behaviourButtons[1, 0].value && tileY > 0)
            {
                tiles[tileX, tileY - 1].vertexY[3] += delta_y;
                tiles[tileX, tileY - 1].vertexY[2] += delta_y;
            }

            if (_behaviourButtons[2, 0].value && tileX < maxX && tileY > 0) tiles[tileX + 1, tileY - 1].vertexY[3] += delta_y;

            if (_behaviourButtons[0, 1].value && tileX > 0)
            {
                tiles[tileX - 1, tileY].vertexY[2] += delta_y;
                tiles[tileX - 1, tileY].vertexY[1] += delta_y;
            }

            if (_behaviourButtons[2, 1].value && tileX < maxX)
            {
                tiles[tileX + 1, tileY].vertexY[0] += delta_y;
                tiles[tileX + 1, tileY].vertexY[3] += delta_y;
            }

            if (_behaviourButtons[0, 2].value && tileX > 0 && tileY < maxY) tiles[tileX - 1, tileY + 1].vertexY[1] += delta_y;

            if (_behaviourButtons[1, 2].value && tileY < maxY)
            {
                tiles[tileX, tileY + 1].vertexY[0] += delta_y;
                tiles[tileX, tileY + 1].vertexY[1] += delta_y;
            }

            if (_behaviourButtons[2, 2].value && tileX < maxX && tileY < maxY) tiles[tileX + 1, tileY + 1].vertexY[0] += delta_y;

            TileData tile = _levelData.Chunks[0].Tiles[tileX, tileY];
            tile.vertexY[0] += delta_y;
            tile.vertexY[1] += delta_y;
            tile.vertexY[2] += delta_y;
            tile.vertexY[3] += delta_y;
        }
    }

}
