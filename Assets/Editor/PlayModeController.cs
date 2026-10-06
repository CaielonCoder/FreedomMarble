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
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            GameObject managers = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Managers.prefab"));
            InitLevel(managers);
        }
    }

    private static async void InitLevel(GameObject managers)
    {
        await Task.Yield();
        managers.GetComponent<GameStateManager>().InitLevel();
    }
}
