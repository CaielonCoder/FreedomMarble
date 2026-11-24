using System;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;

//[InitializeOnLoad]
public class PlayModeController 
{
    public enum PlayMode
    {
        Normal,
        LevelTest,
    }

    private static PlayMode _mode;

    public static void SetPlayMode(PlayMode mode)
    {
        _mode = mode;
        EditorPrefs.SetInt("PlayMode", (int)_mode);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Init()
    {
        _mode = (PlayMode)EditorPrefs.GetInt("PlayMode", 0);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameObject managers;
            if (_mode == PlayMode.LevelTest) 
                managers = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Editor/LevelTestManagers.prefab"));
            else
                managers = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers.prefab"));

            InitLevel(managers);
        }
    }

    private static async void InitLevel(GameObject managers)
    {
        await Task.Yield();
        managers.GetComponent<GameStateManager>().InitLevel();
    }
}
