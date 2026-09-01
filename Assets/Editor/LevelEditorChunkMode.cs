using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public class LevelEditorChunkMode : LevelEditorMode
    {
        public override void CreateGUI(VisualElement root)
        {
            root.Q<Button>("ChunkExtendXButton").clicked += OnExtendXClicked;
            root.Q<Button>("ChunkExtendZButton").clicked += OnExtendZClicked;
            root.Q<Button>("ChunkReduceXButton").clicked += OnReduceXClicked;
            root.Q<Button>("ChunkReduceZButton").clicked += OnReduceZClicked;
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
            RaiseLevelDataChanged();
        }
    }
}
