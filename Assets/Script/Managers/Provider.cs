using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Provider : MonoBehaviour
{
    public static Provider Instance { get; private set; }

    private Dictionary<Type, MonoBehaviour> entities = new Dictionary<Type, MonoBehaviour>();

    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeManagers();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this);
        }
    }

    private void InitializeManagers()
    {
        entities[typeof(GameStateManager)] = GetComponent<GameStateManager>();
        entities[typeof(ScoreManager)] = GetComponent<ScoreManager>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "MainMenu")
        {
            entities[typeof(MarbleMovementController)] = FindAnyObjectByType<MarbleMovementController>();
            entities[typeof(InGameUIController)] = FindAnyObjectByType<InGameUIController>();
            entities[typeof(Goal)] = FindAnyObjectByType<Goal>();
        }
    }

    public T Resolve<T>() where T : MonoBehaviour
    {
        if (entities.TryGetValue(typeof(T), out MonoBehaviour entity))
        {
            return entity as T;
        }
        else
        {
            return null;
        }
    }
}
