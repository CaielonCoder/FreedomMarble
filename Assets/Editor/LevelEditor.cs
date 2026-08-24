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
        private LevelRenderer _levelRenderer;
        private MeshCollider _levelCollider;
        private LevelData _levelData;

        private Vector3 _pointerPosition;
        private Selection _selection;

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

            root.Add(editorUXML);
        }

        private void OnEnabledClicked()
        {
            if (!_editionEnabled)
            {
                _levelRenderer = FindAnyObjectByType<LevelRenderer>();
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

                    // TODO: create custom component for manage the collision
                   _levelCollider = _levelRenderer.gameObject.GetComponent<MeshCollider>();
                    if (_levelCollider == null)
                        _levelCollider = _levelRenderer.gameObject.AddComponent<MeshCollider>();
                    _levelCollider.sharedMesh = _levelRenderer.GetMesh();
                }
            }
            else
            {
                if (_levelCollider != null) DestroyImmediate(_levelCollider);
                _enabledButton.SetCheckedPseudoState(false);
                _enabledButton.text = "Enable";
                _editionEnabled = false;
            }
        }

        private void OnSceneGUI(SceneView view)
        {
            if (!_editionEnabled) return;

            if (_selection.OnSceneGUI(view))
            {
                _levelRenderer.OnDataUpdated();
                _levelCollider.sharedMesh = null;
                _levelCollider.sharedMesh = _levelRenderer.GetMesh();
                EditorUtility.SetDirty(_levelData);
            }

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
                Handles.color = new Color(0f, 1f, 0.8f, 0.5f);
                int x = Mathf.FloorToInt(_pointerPosition.x);
                int y = Mathf.FloorToInt(_pointerPosition.z);
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
                //Handles.DrawWireCube(_pointerPosition, Vector3.one * 0.2f);
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

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

}
