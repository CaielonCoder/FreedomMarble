using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine.UIElements;
using System;
using TMPro;

[Overlay(typeof(SceneView), "Editor Overlay", defaultDisplay = true)]
public class EditorOverlay : Overlay
{
    private const string UxmlPath = "Assets/Editor/Overlay/EditorOverlay.uxml";

    private DropdownField _modeDropdown;

    public override VisualElement CreatePanelContent()
    {
        VisualTreeAsset editorUXML = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

        VisualElement root = new VisualElement();
        editorUXML.CloneTree(root);

        _modeDropdown = root.Q<DropdownField>("ModeDropdown");
        _modeDropdown.index = EditorPrefs.GetInt("PlayMode", 0);
        _modeDropdown.RegisterValueChangedCallback(OnModeChange);

        return root;
    }

    private void OnModeChange(ChangeEvent<string> evt)
    {
        PlayModeController.SetPlayMode((PlayModeController.PlayMode)_modeDropdown.index);
    }
}
