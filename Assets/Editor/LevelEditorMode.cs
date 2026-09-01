using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LevelEditor
{
    public abstract class LevelEditorMode
    {
        protected LevelData _levelData;

        public event Action LevelDataChanged;
        public abstract void CreateGUI(VisualElement root);
        public virtual void Enter(LevelData levelData) { _levelData = levelData; }
        public abstract void Exit();
        public abstract void OnSceneGUI(SceneView view);

        protected void RaiseLevelDataChanged()
        {
            LevelDataChanged?.Invoke();
        }

    }
}
