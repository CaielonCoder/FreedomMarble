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
        private TabView _rootTabView;
        private Button _enabledButton;

        private bool _editionEnabled = false;
        private LevelMeshCreator _levelRenderer;
        private LevelBlockerCreator _levelBlockers;
        private MeshCollider _levelCollider;
        private LevelData _levelData;

        private int _currentMode = 0;
        private LevelEditorMode[] _modes = new LevelEditorMode[3];

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

            _modes[0] = new LevelEditorChunkMode();
            _modes[1] = new LevelEditorTileMode();
            _modes[2] = new LevelEditorBlockerMode();

            foreach (LevelEditorMode editorMode in _modes)
            {
                editorMode.CreateGUI(editorUXML);
            }

            _rootTabView = editorUXML.Q<TabView>("TabView");
            _rootTabView.visible = false;

            _enabledButton = editorUXML.Q<Button>("EnableButton");
            _enabledButton.clicked += OnEnabledClicked;

            root.Add(editorUXML);
        }

        private void OnEnabledClicked()
        {
            if (!_editionEnabled)
            {
                _rootTabView.visible = true;
                _levelRenderer = FindAnyObjectByType<LevelMeshCreator>();
                _levelBlockers = FindAnyObjectByType<LevelBlockerCreator>();
                if (_levelRenderer == null || _levelBlockers == null)
                {
                    EditorUtility.DisplayDialog("Level Error", _levelRenderer ? "LevelBlockerCreator not found" : "LevelMeshCreator not found", "OK");
                }
                else
                {
                    _enabledButton.SetCheckedPseudoState(true);
                    _enabledButton.text = "Disable";
                    _editionEnabled = true;
                    _levelData = _levelRenderer.GetLevelData();
                    _levelRenderer.OnDataUpdated();
                    _levelBlockers.OnDataUpdated();

                    _rootTabView.activeTabChanged += OnActiveTabChanged;
                    _currentMode = _rootTabView.selectedTabIndex;
                    _modes[_currentMode].LevelDataChanged += OnLevelDataChanged;
                    _modes[_currentMode].Enter(_levelData);

                    UpdateColliderMesh();
                }
            }
            else
            {
                if (_levelCollider != null) DestroyImmediate(_levelCollider);

                _enabledButton.SetCheckedPseudoState(false);
                _enabledButton.text = "Enable";
                _editionEnabled = false;
                _rootTabView.activeTabChanged -= OnActiveTabChanged;
                _modes[_currentMode].Exit();
                _modes[_currentMode].LevelDataChanged -= OnLevelDataChanged;
                _rootTabView.visible = false;
                AssetDatabase.SaveAssets();
            }
        }

        private void OnActiveTabChanged(Tab tab1, Tab tab2)
        {
            _modes[_currentMode].Exit();
            _modes[_currentMode].LevelDataChanged -= OnLevelDataChanged;
            _currentMode = _rootTabView.selectedTabIndex;
            _modes[_currentMode].LevelDataChanged += OnLevelDataChanged;
            _modes[_currentMode].Enter(_levelData);
        }

        private void OnLevelDataChanged()
        {
            _levelRenderer.OnDataUpdated();
            _levelBlockers.OnDataUpdated();
            UpdateColliderMesh();
            EditorUtility.SetDirty(_levelData);
        }

        private void OnSceneGUI(SceneView view)
        {
            if (!_editionEnabled) return;
            _modes[_currentMode].OnSceneGUI(view);
        }

        private void UpdateColliderMesh()
        {
            // TODO: create custom component for manage the collision
           _levelCollider = _levelRenderer.gameObject.GetComponent<MeshCollider>();
            if (_levelCollider == null)
                _levelCollider = _levelRenderer.gameObject.AddComponent<MeshCollider>();
            _levelCollider.sharedMesh = _levelRenderer.GetFloorMesh();
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
